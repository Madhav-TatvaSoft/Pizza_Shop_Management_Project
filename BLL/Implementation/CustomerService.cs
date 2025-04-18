using System.Drawing;
using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BLL.Implementation;

public class CustomerService : ICustomerService
{
    private readonly PizzaShopDbContext _context;

    #region Customer Service Constructor
    public CustomerService(PizzaShopDbContext context)
    {
        _context = context;
    }
    #endregion

    #region Get Data
    public IQueryable<CustomerViewModel> GetAllCustomers()
    {
        return _context.Customers
            .Include(u => u.Orders)
            .Where(u => !u.Isdelete).OrderBy(u => u.CustomerId)
            .Select(u => new CustomerViewModel
            {
                CustomerId = u.CustomerId,
                CustomerName = u.CustomerName,
                Email = u.Email,
                PhoneNo = u.PhoneNo,
                CreatedAt = System.DateOnly.FromDateTime((DateTime)u.CreatedAt),
                totalOrder = u.Orders.Count()
            })
            .AsQueryable();
    }
    #endregion

    #region Pagination - Get Order List
    public PaginationViewModel<CustomerViewModel> GetCustomerList(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5, string fromDate = "", string toDate = "", string selectRange = "")
    {
        var query = GetAllCustomers();

        // IQueryable<CustomerViewModel>? query2 = from c in _context.Customers.Where(i => !i.Isdelete)
        //                                         join o in _context.Orders.Where(i => !i.Isdelete)
        //                                         on c.CustomerId equals o.CustomerId into customerOrders
        //                                         orderby c.CustomerId
        //                                         select new CustomerViewModel
        //                                         {
        //                                             CustomerId = c.CustomerId,
        //                                             CustomerName = c.CustomerName,
        //                                             Email = c.Email,
        //                                             PhoneNo = c.PhoneNo,
        //                                             CreatedAt = System.DateOnly.FromDateTime((DateTime)c.CreatedAt),
        //                                             totalOrder = customerOrders.Count()
        //                                         };

        // Apply search 
        if (!string.IsNullOrEmpty(search))
        {
            string lowerSearchTerm = search.ToLower();
            query = query.Where(u =>
                u.CustomerName.ToLower().Contains(lowerSearchTerm)
            );
        }

        // Apply date range filter
        if (!string.IsNullOrEmpty(selectRange))
        {
            switch (selectRange)
            {
                case "Last 7 days":
                    query = query.Where(x => x.CreatedAt >= DateOnly.FromDateTime(DateTime.Now.AddDays(-7)));
                    break;
                case "Last 30 days":
                    query = query.Where(x => x.CreatedAt >= DateOnly.FromDateTime(DateTime.Now.AddDays(-30)));
                    break;
                case "Current Month":
                    query = query.Where(x => x.CreatedAt.Month == DateTime.Now.Month);
                    break;
                case "Custom Date":
                    // Apply date filter
                    if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                    {
                        query = query.Where(u => u.CreatedAt >= DateOnly.Parse(fromDate) && u.CreatedAt <= DateOnly.Parse(toDate));
                    }
                    else if (!string.IsNullOrEmpty(toDate))
                    {
                        query = query.Where(u => u.CreatedAt <= DateOnly.Parse(toDate));
                    }
                    else if (!string.IsNullOrEmpty(fromDate))
                    {
                        query = query.Where(u => u.CreatedAt >= DateOnly.Parse(fromDate));
                    }
                    break;
            }
        }

        // Get total records count (before pagination)
        int totalCount = query.Count();

        //sorting
        switch (sortColumn)
        {
            case "Name":
                query = sortDirection == "asc" ? query.OrderBy(u => u.CustomerName) : query.OrderByDescending(u => u.CustomerName);
                break;

            case "Date":
                query = sortDirection == "asc" ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt);
                break;
            case "TotalOrder":
                query = sortDirection == "asc" ? query.OrderBy(u => u.totalOrder) : query.OrderByDescending(u => u.totalOrder);
                break;
        }

        // Apply pagination
        List<CustomerViewModel>? items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginationViewModel<CustomerViewModel>(items, totalCount, pageNumber, pageSize);
    }
    #endregion

    #region Export Order Data To Excel
    public Task<byte[]> ExportData(string search = "", string fromDate = "", string toDate = "", string selectRange = "")
    {
        var query = GetAllCustomers();

        // Apply search 
        if (!string.IsNullOrEmpty(search))
        {
            string lowerSearchTerm = search.ToLower();
            query = query.Where(u =>
                u.CustomerName.ToLower().Contains(lowerSearchTerm)
            );
        }

        // Apply date range filter
        if (!string.IsNullOrEmpty(selectRange))
        {
            switch (selectRange)
            {
                case "Last 7 days":
                    query = query.Where(x => x.CreatedAt >= DateOnly.FromDateTime(DateTime.Now.AddDays(-7)));
                    break;
                case "Last 30 days":
                    query = query.Where(x => x.CreatedAt >= DateOnly.FromDateTime(DateTime.Now.AddDays(-30)));
                    break;
                case "Current Month":
                    query = query.Where(x => x.CreatedAt.Month == DateTime.Now.Month);
                    break;
                case "Custom Date":
                    // Apply date filter
                    if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                    {
                        query = query.Where(u => u.CreatedAt >= DateOnly.Parse(fromDate) && u.CreatedAt <= DateOnly.Parse(toDate));
                    }
                    else if (!string.IsNullOrEmpty(toDate))
                    {
                        query = query.Where(u => u.CreatedAt <= DateOnly.Parse(toDate));
                    }
                    else if (!string.IsNullOrEmpty(fromDate))
                    {
                        query = query.Where(u => u.CreatedAt >= DateOnly.Parse(fromDate));
                    }
                    break;
            }
        }

        List<CustomerViewModel>? customers = query.ToList();

        // Create Excel package
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage())
        {
            ExcelWorksheet? worksheet = package.Workbook.Worksheets.Add("Customers");
            int currentRow = 3;
            int currentCol = 2;

            // this is first row....................................
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = "Account: ";

            using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1])
            {
                headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headingCells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0066A7"));
                headingCells.Style.Font.Bold = true;
                headingCells.Style.Font.Color.SetColor(Color.White);
                headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            currentCol += 2;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = "";
            using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3])
            {
                headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headingCells.Style.Fill.BackgroundColor.SetColor(Color.White);
                headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);


                headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            currentCol += 5;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = "Search Text: ";
            using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1])
            {
                headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headingCells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0066A7"));
                headingCells.Style.Font.Bold = true;
                headingCells.Style.Font.Color.SetColor(Color.White);
                headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            currentCol += 2;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = search;
            using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3])
            {
                headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headingCells.Style.Fill.BackgroundColor.SetColor(Color.White);
                headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);


                headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            currentCol += 5;

            worksheet.Cells[currentRow, currentCol, currentRow + 4, currentCol + 1].Merge = true;

            // Insert Logo
            string? imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logos", "pizzashop_logo.png");

            if (File.Exists(imagePath))
            {
                var picture = worksheet.Drawings.AddPicture("Image", new FileInfo(imagePath));
                picture.SetPosition(currentRow - 1, 1, currentCol - 1, 1);
                picture.SetSize(125, 95);
            }
            else
            {
                worksheet.Cells[currentRow, currentCol].Value = "Image not found";
            }

            // this is second row....................................
            currentRow += 3;
            currentCol = 2;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = "Date: ";
            using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1])
            {
                headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headingCells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0066A7"));
                headingCells.Style.Font.Bold = true;
                headingCells.Style.Font.Color.SetColor(Color.White);
                headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            currentCol += 2;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = selectRange;
            using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3])
            {
                headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headingCells.Style.Fill.BackgroundColor.SetColor(Color.White);
                headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);


                headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            currentCol += 5;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = "No. of Records: ";
            using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1])
            {
                headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headingCells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0066A7"));
                headingCells.Style.Font.Bold = true;
                headingCells.Style.Font.Color.SetColor(Color.White);
                headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            currentCol += 2;
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = customers.Count;
            using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3])
            {
                headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headingCells.Style.Fill.BackgroundColor.SetColor(Color.White);
                headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);


                headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            // this is table ....................................
            int headingRow = currentRow + 4;
            int headingCol = 2;

            worksheet.Cells[headingRow, headingCol].Value = "Id";
            headingCol++;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 2].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Name";
            headingCol += 3;  // Move to next unmerged column

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 2].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Email";
            headingCol += 3;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 2].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Date";
            headingCol += 3;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 1].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Mobile Number";
            headingCol += 2;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 1].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Total Order";

            using (var headingCells = worksheet.Cells[headingRow, 2, headingRow, headingCol + 1])
            {
                headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headingCells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0066A7"));
                headingCells.Style.Font.Bold = true;
                headingCells.Style.Font.Color.SetColor(Color.White);

                headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);


                headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }


            // Populate data
            int row = headingRow + 1;

            foreach (var customer in customers)
            {
                int startCol = 2;

                worksheet.Cells[row, startCol].Value = customer.CustomerId;
                startCol += 1;

                worksheet.Cells[row, startCol, row, startCol + 2].Merge = true;
                worksheet.Cells[row, startCol].Value = customer.CustomerName;
                startCol += 3;

                worksheet.Cells[row, startCol, row, startCol + 2].Merge = true;
                worksheet.Cells[row, startCol].Value = customer.Email;
                startCol += 3;

                worksheet.Cells[row, startCol, row, startCol + 2].Merge = true;
                worksheet.Cells[row, startCol].Value = customer.CreatedAt.ToString();
                startCol += 3;

                worksheet.Cells[row, startCol, row, startCol + 1].Merge = true;
                worksheet.Cells[row, startCol].Value = customer.PhoneNo;
                startCol += 2;

                worksheet.Cells[row, startCol, row, startCol + 1].Merge = true;
                worksheet.Cells[row, startCol].Value = customer.totalOrder;

                using (var rowCells = worksheet.Cells[row, 2, row, startCol + 1])
                {
                    // Apply alternating row colors (light gray for better readability)
                    if (row % 2 == 0)
                    {
                        rowCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rowCells.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    // Apply black borders to each row
                    rowCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);


                    rowCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rowCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                row++;
            }
            return Task.FromResult(package.GetAsByteArray());
        }
    }
    #endregion

    #region Customer History
    public CustomerHistoryViewModel GetCustomerHistory(long customerid)
    {
        CustomerHistoryViewModel? customerDetails = _context.Customers.
        Include(x => x.Orders).
        ThenInclude(x => x.Orderdetails).
        ThenInclude(x => x.Order.PaymentStatus).
        Where(x => x.CustomerId == customerid).
        Select(x => new CustomerHistoryViewModel
        {
            // Customer Details

            CustomerId = x.CustomerId,
            CustomerName = x.CustomerName,
            PhoneNo = (long)x.PhoneNo,
            CreatedAt = (DateTime)x.CreatedAt,
            visits = x.Orders.Count(),
            MaxOrder = x.Orders.Max(x => x.TotalAmount),
            AvgBill = Math.Round(x.Orders.Average(x => x.TotalAmount), 2),
            orderList = x.Orders.Select(x => new OrderListViewModel
            {
                OrderDate = DateOnly.FromDateTime(x.OrderDate),
                OrderType = x.OrderType,
                Paymentstatus = x.PaymentStatus.PaymentStatus,
                NoOfItems = x.Orderdetails.Count(),
                TotalAmount = x.TotalAmount
            }).ToList()
        }).FirstOrDefault();

        return customerDetails;
    }
    #endregion

    #region IsCustomerPresent
    public long IsCustomerPresent(string Email)
    {
        Customer customer = _context.Customers.FirstOrDefault(x => x.Email == Email && !x.Isdelete);
        if (customer != null) return customer.CustomerId;
        else return 0;
    }
    #endregion

    #region Get Customer Email
    public List<CustomerViewModel> GetCustomerEmail(string searchTerm)
    {
        List<CustomerViewModel>? Emails = _context.Customers
        .Where(c => c.Email.Contains(searchTerm))
        .Select(c => new CustomerViewModel
        {
            Email = c.Email,
            CustomerName = c.CustomerName ?? "",
            PhoneNo = c.PhoneNo
        })
        .Take(10)
        .ToList();

        return Emails;
    }
    #endregion

    #region AddCustomer
    public async Task<bool> AddEditCustomer(WaitingTokenDetailViewModel waitingTokenVM, long userId)
    {
        var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == waitingTokenVM.Email && !c.Isdelete);

        if (existingCustomer != null)
        {
            // Update existing customer
            existingCustomer.CustomerName = waitingTokenVM.CustomerName;
            existingCustomer.Email = waitingTokenVM.Email;
            existingCustomer.PhoneNo = waitingTokenVM.PhoneNo;
            existingCustomer.ModifiedBy = userId;
            existingCustomer.ModifiedAt = DateTime.Now;
            _context.Update(existingCustomer);
            await _context.SaveChangesAsync();
            return true;
        }

        Customer customer = new();
        customer.CustomerName = waitingTokenVM.CustomerName;
        customer.Email = waitingTokenVM.Email;
        customer.PhoneNo = waitingTokenVM.PhoneNo;
        customer.CreatedBy = userId;
        await _context.AddAsync(customer);
        await _context.SaveChangesAsync();
        return true;
    }
    #endregion


}