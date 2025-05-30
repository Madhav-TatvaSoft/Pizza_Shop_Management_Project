using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace BLL.Implementation;

public class OrderAppKOTService : IOrderAppKOTService
{
    private readonly PizzaShopDbContext _context;
    private readonly IConfiguration _configuration;

    public OrderAppKOTService(PizzaShopDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    #region Get KOT Items

    // public async Task<PaginationViewModel<OrderAppKOTViewModel>> GetKOTItems(long catid, string filter, int page = 1, int pageSize = 5)
    // {
    // IQueryable<Order> query = _context.Orders
    //     .Include(x => x.Orderdetails)
    //         .ThenInclude(x => x.Item)
    //     .Include(x => x.Orderdetails)
    //         .ThenInclude(x => x.Modifierorders)
    //     .Include(x => x.AssignTables)
    //         .ThenInclude(x => x.Table)
    //     .Include(x => x.Section)
    //     .Where(x => !x.Isdelete && x.Status != "Completed" && x.Status != "Cancelled").OrderByDescending(x => x.OrderId);

    //     if (catid == 0)
    //     {
    //         query = query.Where(x => !x.Isdelete && x.Orderdetails.Any(od => !od.Item.Isdelete && ((filter == "Ready") ? (od.ReadyQuantity > 0) : (od.Quantity - od.ReadyQuantity > 0))));

    //         List<OrderAppKOTViewModel>? data = query
    //         .Select(x => new OrderAppKOTViewModel
    //         {
    //             OrderId = x.OrderId,
    //             OrderDate = x.OrderDate,
    //             ExtraInstruction = x.ExtraInstruction,
    //             SectionId = x.SectionId,
    //             SectionName = x.Section.SectionName,
    //             tableList = x.AssignTables.Select(y => new Table
    //             {
    //                 TableId = y.TableId,
    //                 TableName = y.Table.TableName
    //             }).ToList(),
    //             itemOrderVM = x.Orderdetails.Where(od => !od.Item.Isdelete && (filter == "Ready") ? (od.ReadyQuantity > 0) : (od.Quantity - od.ReadyQuantity > 0))
    //             .Select(y => new ItemOrderViewModel
    //             {
    //                 ItemId = y.ItemId,
    //                 OrderdetailId = y.OrderdetailId,
    //                 ItemName = y.Item.ItemName,
    //                 ExtraInstruction = y.ExtraInstruction,
    //                 Quantity = (filter == "Ready") ? y.ReadyQuantity : (y.Quantity - y.ReadyQuantity),
    //                 modifierOrderVM = y.Modifierorders.Select(z => new ModifierorderViewModel
    //                 {
    //                     ModifierId = z.ModifierId,
    //                     ModifierName = z.Modifier.ModifierName,
    //                     // Quantity = (int)z.ModifierQuantity
    //                 }).ToList()
    //             }).ToList()
    //         }).ToList();

    //         int totalCount = data.Count();

    //         List<OrderAppKOTViewModel>? items = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();

    //         return new PaginationViewModel<OrderAppKOTViewModel>(items, totalCount, page, pageSize);
    //     }
    //     else
    //     {
    //         query = query.Where(x => !x.Isdelete && x.Orderdetails.Any(od => (od.Item.CategoryId == catid) && !od.Item.Isdelete && ((filter == "Ready") ? (od.ReadyQuantity > 0) : (od.Quantity - od.ReadyQuantity > 0))));

    //         List<OrderAppKOTViewModel>? data = query
    //         .Select(x => new OrderAppKOTViewModel
    //         {
    //             OrderId = x.OrderId,
    //             OrderDate = x.OrderDate,
    //             ExtraInstruction = x.ExtraInstruction,
    //             SectionId = x.SectionId,
    //             SectionName = x.Section.SectionName,
    //             tableList = x.AssignTables.Select(y => new Table
    //             {
    //                 TableId = y.TableId,
    //                 TableName = y.Table.TableName
    //             }).ToList(),
    //             itemOrderVM = x.Orderdetails.Where(x => (x.Item.CategoryId == catid) && !x.Item.Isdelete && ((filter == "Ready") ? (x.ReadyQuantity > 0) : (x.Quantity - x.ReadyQuantity > 0)))
    //             .Select(y => new ItemOrderViewModel
    //             {
    //                 ItemId = y.ItemId,
    //                 OrderdetailId = y.OrderdetailId,
    //                 ItemName = y.Item.ItemName,
    //                 ExtraInstruction = y.ExtraInstruction,
    //                 Quantity = (filter == "Ready") ? y.ReadyQuantity : (y.Quantity - y.ReadyQuantity),
    //                 modifierOrderVM = y.Modifierorders.Select(z => new ModifierorderViewModel
    //                 {
    //                     ModifierId = z.ModifierId,
    //                     ModifierName = z.Modifier.ModifierName,
    //                     // Quantity = (int)z.ModifierQuantity
    //                 }).ToList()
    //             }).ToList()
    //         }).ToList();

    //         int totalCount = data.Count();

    //         List<OrderAppKOTViewModel>? items = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();

    //         return new PaginationViewModel<OrderAppKOTViewModel>(items, totalCount, page, pageSize);
    //     }
    // }


    // Using a Function in DB to get KOT items
    
    public async Task<PaginationViewModel<OrderAppKOTViewModel>> GetKOTItems(long catid, string filter, int page = 1, int pageSize = 5)
    {
        using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection")))
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand("SELECT get_kot_items(@p_catid, @p_filter)", connection))
            {
                command.Parameters.AddWithValue("p_catid", catid);
                command.Parameters.AddWithValue("p_filter", filter ?? "");

                try
                {
                    var jsonResult = await command.ExecuteScalarAsync();
                    if (jsonResult == null || jsonResult == DBNull.Value)
                    {
                        return new PaginationViewModel<OrderAppKOTViewModel>(new List<OrderAppKOTViewModel>(), 0, page, pageSize);
                    }

                    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    var data = JsonSerializer.Deserialize<List<OrderAppKOTViewModel>>(jsonResult.ToString(), options)
                        ?? new List<OrderAppKOTViewModel>();

                    int totalCount = data.Count;
                    var items = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    return new PaginationViewModel<OrderAppKOTViewModel>(items, totalCount, page, pageSize);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return new PaginationViewModel<OrderAppKOTViewModel>(new List<OrderAppKOTViewModel>(), 0, page, pageSize);
                }
            }
        }
    }

    public async Task<OrderAppKOTViewModel> GetKOTItemsFromModal(long catid, string filter, long orderid)
    {
        PaginationViewModel<OrderAppKOTViewModel> KotModalData = await GetKOTItems(catid, filter, 1, 5);
        OrderAppKOTViewModel? KotData = KotModalData.Items.SingleOrDefault(x => x.OrderId == orderid);
        if (KotData == null)
        {
            KotData = new OrderAppKOTViewModel();
        }
        return KotData;
    }

    #endregion

    #region Update KOT Status

    // public async Task<bool> UpdateKOTStatus(string filter, int[] orderDetailId, int[] quantity)
    // {
    //     using (var transaction = await _context.Database.BeginTransactionAsync())
    //     {
    //         try
    //         {
    //             if (orderDetailId.Length == 0 || orderDetailId == null || quantity == null || quantity.Length == 0 || orderDetailId.Length != quantity.Length)
    //             {
    //                 return false;
    //             }

    //             for (int i = 0; i < orderDetailId.Length; i++)
    //             {
    //                 Orderdetail? orderDetail = await _context.Orderdetails.FirstOrDefaultAsync(x => x.OrderdetailId == orderDetailId[i] && !x.Isdelete && x.Status != "Completed" && x.Status != "Cancelled");

    //                 if (orderDetail == null)
    //                 {
    //                     return false;
    //                 }
    //                 if (filter == "In_Progress")
    //                 {
    //                     orderDetail.ReadyQuantity += quantity[i];
    //                 }
    //                 else
    //                 {
    //                     orderDetail.ReadyQuantity -= quantity[i];
    //                 }
    //                 _context.Orderdetails.Update(orderDetail);
    //             }
    //             await _context.SaveChangesAsync();

    //             await transaction.CommitAsync();

    //             return true;
    //         }
    //         catch
    //         {
    //             await transaction.RollbackAsync();
    //             throw;
    //         }
    //     }
    // }

    // Updating the KOT Item status using a stored procedure
    
    public async Task<bool> UpdateKOTStatus(string filter, int[] orderDetailId, int[] quantity)
    {

        using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection")))
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand("CALL update_kot_status(@p_filter, @p_order_detail_ids, @p_quantities)", connection))
            {
                command.Parameters.AddWithValue("p_filter", filter ?? "");
                command.Parameters.AddWithValue("p_order_detail_ids", orderDetailId.Select(id => (long)id).ToArray());
                command.Parameters.AddWithValue("p_quantities", quantity);

                try
                {
                    await command.ExecuteNonQueryAsync();
                    return true;
                }
                catch (PostgresException ex)
                {
                    Console.WriteLine($"Error: {ex.MessageText}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }
    }

    #endregion

}