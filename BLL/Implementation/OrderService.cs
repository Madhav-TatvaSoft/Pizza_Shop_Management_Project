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
        var query = _context.Orders
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
        var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginationViewModel<OrdersViewModel>(items, totalCount, pageNumber, pageSize);
    }
    #endregion

    #region Export Order Data To Excel
    public Task<byte[]> ExportData(string search = "", string orderStatus = "", string selectRange = "")
    {
        var query = _context.Orders
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

        var orders = query.ToList();

        // Create Excel package
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Orders");
            var currentRow = 3;
            var currentCol = 2;

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
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logos", "pizzashop_logo.png");

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
                worksheet.Cells[row, startCol].Value = order.OrderDate;
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

}