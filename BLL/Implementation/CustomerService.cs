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

    public CustomerService(PizzaShopDbContext context)
    {
        _context = context;
    }

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
                CreatedAt = u.CreatedAt != null ? DateOnly.FromDateTime((DateTime)u.CreatedAt) : default,
                totalOrder = u.Orders.Where(o => o.Status == "Completed").Count()
            })
            .AsQueryable();
    }

    public PaginationViewModel<CustomerViewModel> GetCustomerList(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5, string fromDate = "", string toDate = "", string selectRange = "")
    {
        IQueryable<CustomerViewModel>? query = GetAllCustomers();

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

    public Task<byte[]> ExportData(string search = "", string fromDate = "", string toDate = "", string selectRange = "")
    {
        IQueryable<CustomerViewModel>? query = GetAllCustomers();

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

    public CustomerHistoryViewModel GetCustomerHistory(long customerid)
    {
        if (_context.Orders.Where(o => o.CustomerId == customerid && o.Status != "Cancelled" && !o.Isdelete).Count() == 0)
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
                PhoneNo = x.PhoneNo.HasValue ? x.PhoneNo.Value : 0,
                CreatedAt = (DateTime)x.CreatedAt,
                visits = 0,
                MaxOrder = 0,
                AvgBill = 0.00M,
                orderList = null
            }).FirstOrDefault();

            if (customerDetails == null)
            {
                return null;
            }

            return customerDetails;
        }
        else
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
                        PhoneNo = x.PhoneNo.HasValue ? x.PhoneNo.Value : 0,
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

            if (customerDetails == null)
            {
                return null;
            }

            return customerDetails;
        }

    }

    public long IsCustomerPresent(string Email)
    {
        Customer? customer = _context.Customers.FirstOrDefault(x => x.Email == Email && !x.Isdelete);
        if (customer != null) return customer.CustomerId;
        else return 0;
    }

    public List<CustomerViewModel> GetCustomerEmail(string searchTerm)
    {
        List<CustomerViewModel>? Emails = _context.Customers
        .Where(c => c.Email.Contains(searchTerm.Trim()) && !c.Isdelete)
        .Select(c => new CustomerViewModel
        {
            Email = c.Email,
            CustomerId = c.CustomerId,
            CustomerName = c.CustomerName ?? "",
            PhoneNo = c.PhoneNo
        })
        .Take(10)
        .ToList();

        return Emails;
    }

    public async Task<bool> SaveCustomer(WaitingTokenDetailViewModel waitingTokenVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync()){
            try
            {
                if (waitingTokenVM == null) return false;

                // Check if customer already exists
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
                }
                else
                {
                    // Add new customer
                    Customer customer = new();
                    customer.CustomerName = waitingTokenVM.CustomerName;
                    customer.Email = waitingTokenVM.Email;
                    customer.PhoneNo = waitingTokenVM.PhoneNo;
                    customer.CreatedBy = userId;
                    await _context.AddAsync(customer);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public async Task<long> GetCustomerIdByTableId(long tableId)
    {
        AssignTable? customer = _context.AssignTables.FirstOrDefault(x => x.TableId == tableId && !x.Isdelete);
        return (customer != null) ? customer.CustomerId : 0;
    }

    public async Task<OrderDetailViewModel> UpdateCustomerDetails(OrderDetailViewModel orderDetailVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                Customer? customer = await _context.Customers.SingleOrDefaultAsync(x => x.CustomerId == orderDetailVM.CustomerId && !x.Isdelete);

                if (customer == null)
                {
                    return null;
                }
                customer.CustomerName = orderDetailVM.CustomerName;
                customer.PhoneNo = orderDetailVM.PhoneNo;
                customer.Email = orderDetailVM.Email;
                customer.ModifiedAt = DateTime.Now;
                customer.ModifiedBy = userId;
                _context.Customers.Update(customer);

                var AssignTable = _context.AssignTables.Where(x => x.CustomerId == orderDetailVM.CustomerId && !x.Isdelete).ToList();

                foreach (var table in AssignTable)
                {
                    table.NoOfPerson = orderDetailVM.NoOfPerson;
                    table.ModifiedAt = DateTime.Now;
                    table.ModifiedBy = userId;
                    _context.AssignTables.Update(table);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return orderDetailVM;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

}