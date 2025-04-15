using System.Drawing;
using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BLL.Implementation;

public class OrderService : IOrderService
{
    private readonly PizzaShopDbContext _context;

    #region OrderService Constructor
    public OrderService(PizzaShopDbContext context)
    {
        _context = context;
    }
    #endregion

    #region Pagination - Get Order List
    public PaginationViewModel<OrdersViewModel> GetOrderList(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5, string orderStatus = "", string fromDate = "", string toDate = "", string selectRange = "")
    {
        IQueryable<OrdersViewModel>? query = _context.Orders
            .Include(u => u.Customer)
            .Include(u => u.Rating)
            .Include(u => u.Paymentmethod)
            .Where(u => u.Isdelete == false).OrderBy(u => u.OrderId)
            .Select(u => new OrdersViewModel
            {
                OrderId = u.OrderId,
                OrderDate = System.DateOnly.FromDateTime(u.OrderDate.Date),
                CustomerId = u.Customer.CustomerId,
                CustomerName = u.Customer.CustomerName,
                Status = u.Status,
                TotalAmount = u.TotalAmount,
                PaymentmethodId = u.Paymentmethod.PaymentMethodId,
                PaymentMethodName = u.Paymentmethod.PaymentType,
                RatingId = u.Rating.RatingId,
                rating = (int)Math.Ceiling(((double)u.Rating.Ambience + (double)u.Rating.Food + (double)u.Rating.Service) / 3),
            })
            .AsQueryable();

        // Apply search 
        if (!string.IsNullOrEmpty(search))
        {
            string lowerSearchTerm = search.ToLower();
            query = query.Where(u =>
                u.CustomerName.ToLower().Contains(lowerSearchTerm) ||
                u.OrderId.ToString().Contains(lowerSearchTerm)
            );
        }

        // Apply filter
        if (!string.IsNullOrEmpty(orderStatus) && orderStatus != "All Status")
        {
            query = query.Where(u => u.Status == orderStatus);
        }

        // Apply date range filter
        if (!string.IsNullOrEmpty(selectRange))
        {
            switch (selectRange)
            {
                case "Last 7 days":
                    query = query.Where(x => x.OrderDate >= DateOnly.FromDateTime(DateTime.Now.AddDays(-7)));
                    break;
                case "Last 30 days":
                    query = query.Where(x => x.OrderDate >= DateOnly.FromDateTime(DateTime.Now.AddDays(-30)));
                    break;
                case "Current Month":
                    query = query.Where(x => x.OrderDate.Month == DateTime.Now.Month);
                    break;

            }
        }

        // Apply date filter
        if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
        {
            query = query.Where(u => u.OrderDate >= DateOnly.Parse(fromDate) && u.OrderDate <= DateOnly.Parse(toDate));
        }
        else if (!string.IsNullOrEmpty(toDate))
        {
            query = query.Where(u => u.OrderDate <= DateOnly.Parse(toDate));
        }
        else if (!string.IsNullOrEmpty(fromDate))
        {
            query = query.Where(u => u.OrderDate >= DateOnly.Parse(fromDate));
        }

        // Get total records count (before pagination)
        int totalCount = query.Count();

        //sorting
        switch (sortColumn)
        {
            case "Order":
                query = sortDirection == "asc" ? query.OrderBy(u => u.OrderId) : query.OrderByDescending(u => u.OrderId);
                break;

            case "Date":
                query = sortDirection == "asc" ? query.OrderBy(u => u.OrderDate) : query.OrderByDescending(u => u.OrderDate);
                break;
            case "Customer":
                query = sortDirection == "asc" ? query.OrderBy(u => u.CustomerName) : query.OrderByDescending(u => u.CustomerName);
                break;
            case "Amount":
                query = sortDirection == "asc" ? query.OrderBy(u => u.TotalAmount) : query.OrderByDescending(u => u.TotalAmount);
                break;
        }

        // Apply pagination
        List<OrdersViewModel>? items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginationViewModel<OrdersViewModel>(items, totalCount, pageNumber, pageSize);
    }
    #endregion

    #region Export Order Data To Excel
    public Task<byte[]> ExportData(string search = "", string orderStatus = "", string selectRange = "")
    {
        IQueryable<OrdersViewModel>? query = _context.Orders
            .Include(u => u.Customer)
            .Include(u => u.Rating)
            .Include(u => u.Paymentmethod)
            .Where(u => u.Isdelete == false).OrderBy(u => u.OrderId)
            .Select(u => new OrdersViewModel
            {
                OrderId = u.OrderId,
                OrderDate = System.DateOnly.FromDateTime(u.OrderDate.Date),
                CustomerId = u.Customer.CustomerId,
                CustomerName = u.Customer.CustomerName,
                Status = u.Status,
                TotalAmount = u.TotalAmount,
                PaymentmethodId = u.Paymentmethod.PaymentMethodId,
                PaymentMethodName = u.Paymentmethod.PaymentType,
                RatingId = u.Rating.RatingId,
                rating = (int)Math.Ceiling(((double)u.Rating.Ambience + (double)u.Rating.Food + (double)u.Rating.Service) / 3),
            })
            .AsQueryable();

        // Apply search 
        if (!string.IsNullOrEmpty(search))
        {
            string lowerSearchTerm = search.ToLower();
            query = query.Where(u =>
                u.CustomerName.ToLower().Contains(lowerSearchTerm) ||
                u.OrderId.ToString().Contains(lowerSearchTerm)
            );
        }

        // Apply filter
        if (!string.IsNullOrEmpty(orderStatus) && orderStatus != "All Status")
        {
            query = query.Where(u => u.Status == orderStatus);
        }

        // Apply date range filter
        if (!string.IsNullOrEmpty(selectRange))
        {
            switch (selectRange)
            {
                case "Last 7 days":
                    query = query.Where(x => x.OrderDate >= DateOnly.FromDateTime(DateTime.Now.AddDays(-7)));
                    break;
                case "Last 30 days":
                    query = query.Where(x => x.OrderDate >= DateOnly.FromDateTime(DateTime.Now.AddDays(-30)));
                    break;
                case "Current Month":
                    query = query.Where(x => x.OrderDate.Month == DateTime.Now.Month);
                    break;

            }
        }

        List<OrdersViewModel>? orders = query.ToList();

        // Create Excel package
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage())
        {
            ExcelWorksheet? worksheet = package.Workbook.Worksheets.Add("Orders");
            int currentRow = 3;
            int currentCol = 2;

            // this is first row....................................
            worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1].Merge = true;
            worksheet.Cells[currentRow, currentCol].Value = "Status: ";
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
            worksheet.Cells[currentRow, currentCol].Value = orderStatus;
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
            worksheet.Cells[currentRow, currentCol].Value = orders.Count;
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

            worksheet.Cells[headingRow, headingCol].Value = "Order No";
            headingCol++;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 2].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Order Date";
            headingCol += 3;  // Move to next unmerged column

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 2].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Customer Name";
            headingCol += 3;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 2].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Status";
            headingCol += 3;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 1].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Payment Mode";
            headingCol += 2;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 1].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Average Rating";
            headingCol += 2;

            worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 1].Merge = true;
            worksheet.Cells[headingRow, headingCol].Value = "Total Amount";


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

            foreach (var order in orders)
            {
                int startCol = 2;

                worksheet.Cells[row, startCol].Value = order.OrderId;
                startCol += 1;

                worksheet.Cells[row, startCol, row, startCol + 2].Merge = true;
                worksheet.Cells[row, startCol].Value = order.OrderDate.ToString();
                startCol += 3;

                worksheet.Cells[row, startCol, row, startCol + 2].Merge = true;
                worksheet.Cells[row, startCol].Value = order.CustomerName;
                startCol += 3;

                worksheet.Cells[row, startCol, row, startCol + 2].Merge = true;
                worksheet.Cells[row, startCol].Value = order.Status;
                startCol += 3;

                worksheet.Cells[row, startCol, row, startCol + 1].Merge = true;
                worksheet.Cells[row, startCol].Value = order.PaymentMethodName;
                startCol += 2;

                worksheet.Cells[row, startCol, row, startCol + 1].Merge = true;
                worksheet.Cells[row, startCol].Value = order.rating;
                startCol += 2;

                worksheet.Cells[row, startCol, row, startCol + 1].Merge = true;
                worksheet.Cells[row, startCol].Value = order.TotalAmount;

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

    #region Get Order Details
    public async Task<OrderDetailViewModel> GetOrderDetails(long orderid)
    {
        try
        {
            Invoice? orderdetails = _context.Invoices.Include(x => x.Order).ThenInclude(x => x.Table).ThenInclude(x => x.Section).Include(x => x.Customer).ThenInclude(x => x.Waitinglists).FirstOrDefault(x => x.OrderId == orderid);

            if(orderdetails == null)
            {
                return null;
            }

            OrderDetailViewModel orderDetailVM = new OrderDetailViewModel();

            // Order Details 
            orderDetailVM.InvoiceId = orderdetails.InvoiceId;
            orderDetailVM.InvoiceNo = orderdetails.InvoiceNo;
            orderDetailVM.OrderId = orderdetails.OrderId;
            orderDetailVM.OrderDate = orderdetails.Order.OrderDate;
            orderDetailVM.Status = orderdetails.Order.Status;

            // Customer Details
            orderDetailVM.CustomerId = orderdetails.Customer.CustomerId;
            orderDetailVM.CustomerName = orderdetails.Customer.CustomerName;
            orderDetailVM.PhoneNo = orderdetails.Customer.PhoneNo;
            orderDetailVM.Email = orderdetails.Customer.Email;

            // No of Person
            List<AssignTable> AssignTableList = _context.AssignTables.Include(x => x.Customer).Include(x => x.Order).Where(x => x.CustomerId == orderdetails.Order.CustomerId && x.OrderId == orderid).ToList();
            orderDetailVM.NoOfPerson = AssignTableList.Sum(x => x.NoOfPerson);

            // Table Details
            orderDetailVM.tableList = _context.AssignTables.Include(x => x.Customer).Include(x => x.Order).Include(x => x.Table).Where(x => x.CustomerId == orderdetails.Order.CustomerId && x.OrderId == orderid).Select(x => new Table
            {
                TableId = x.TableId,
                TableName = x.Table.TableName
            }).ToList();
            orderDetailVM.SectionId = orderdetails.Order.SectionId;
            orderDetailVM.SectionName = orderdetails.Order.Section.SectionName;

            // Item Order Details
            orderDetailVM.itemOrderVM = _context.Orderdetails.Include(x => x.Item).Where(x => x.OrderId == orderid).Select(x => new ItemOrderViewModel
            {
                ItemId = x.ItemId,
                ItemName = x.Item.ItemName,
                Quantity = x.Quantity,
                Rate = x.Item.Rate,
                TotalItemAmount = Math.Round(x.Quantity * x.Item.Rate, 2),
                modifierOrderVM = _context.Modifierorders.Include(m => m.Modifier).Include(m => m.Orderdetail).ThenInclude(m => m.Item).Where(m => m.Orderdetail.ItemId == x.ItemId).Select(m => new ModifierorderViewModel
                {
                    ModifierId = m.ModifierId,
                    ModifierName = m.Modifier.ModifierName,
                    Rate = m.Modifier.Rate,
                    Quantity = (int)m.ModifierQuantity,
                    TotalModifierAmount = Math.Round((int)m.ModifierQuantity * (decimal)m.Modifier.Rate, 2),
                }).ToList()
            }).ToList();

            // SubTotal Amount
            orderDetailVM.SubTotalAmountOrder = Math.Round((decimal)orderDetailVM.itemOrderVM.Sum(x => x.TotalItemAmount + x.modifierOrderVM.Sum(x => x.TotalModifierAmount)), 2);

            // Taxes Details
            // List<Tax> taxes = _context.Taxes.Where(x => x.Isdelete == false).ToList();
            // Invoice invoices = _context.Invoices.FirstOrDefault(x => x.Isdelete == false && x.OrderId == orderid);

            // for (int i = 1; i < taxes.Count; i++)
            // {
            //     TaxInvoiceMapping fillTax = new TaxInvoiceMapping();
            //     fillTax.TaxId = taxes[i].TaxId;
            //     fillTax.InvoiceId = invoices.InvoiceId;
            //     _context.Add(fillTax);
            //     await _context.SaveChangesAsync();
            // }

            List<TaxInvoiceMapping>? taxdetails = _context.TaxInvoiceMappings.Include(x => x.Invoice).Include(x => x.Tax).Where(x => x.Invoice.OrderId == orderid).ToList();

            orderDetailVM.taxInvoiceVM = new List<TaxInvoiceViewModel>();

            foreach (var tax in taxdetails)
            {

                if (tax.Tax.TaxType == "Flat Amount")
                {
                    orderDetailVM.taxInvoiceVM.Add(
                        new TaxInvoiceViewModel
                        {
                            TaxId = tax.Tax.TaxId,
                            TaxName = tax.Tax.TaxName,
                            TaxType = tax.Tax.TaxType,
                            TaxValue = tax.Tax.TaxValue,
                            InvoiceId = tax.Invoice.InvoiceId
                        }
                    );
                }
                if (tax.Tax.TaxType == "Percentage")
                {
                    orderDetailVM.taxInvoiceVM.Add(
                        new TaxInvoiceViewModel
                        {
                            TaxId = tax.Tax.TaxId,
                            TaxName = tax.Tax.TaxName,
                            TaxType = tax.Tax.TaxType,
                            TaxValue = Math.Round(tax.Tax.TaxValue / 100 * orderDetailVM.SubTotalAmountOrder, 2),
                            InvoiceId = tax.Invoice.InvoiceId
                        }
                    );
                }
            }

            orderDetailVM.TotalAmountOrder = orderDetailVM.SubTotalAmountOrder + orderDetailVM.taxInvoiceVM.Sum(x => x.TaxValue);

            // orderdetails.Order.TotalAmount = orderDetailVM.SubTotalAmountOrder;

            return orderDetailVM;
        }

        catch (Exception exception)
        {
            return null;
        }
    }


    #endregion

}