using System.Text.Json;
using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BLL.Implementation;

public class OrderAppWaitingListService : IOrderAppWaitingListService
{
    private readonly PizzaShopDbContext _context;
    private readonly IConfiguration _configuration;

    public OrderAppWaitingListService(PizzaShopDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // public List<WaitingTokenDetailViewModel> GetWaitingList(long sectionid)
    // {
    //     if (sectionid == 0)
    //     {
    //         List<WaitingTokenDetailViewModel>? waiting = _context.Waitinglists.Include(x => x.Customer).Where(waiting => !waiting.Isdelete && !waiting.Isassign)
    //                 .Select(waiting => new WaitingTokenDetailViewModel
    //                 {
    //                     WaitingId = waiting.WaitingId,
    //                     CustomerId = waiting.CustomerId,
    //                     CustomerName = waiting.Customer.CustomerName,
    //                     PhoneNo = waiting.Customer.PhoneNo,
    //                     Email = waiting.Customer.Email,
    //                     NoOfPerson = waiting.NoOfPerson,
    //                     CreatedAt = (DateTime)waiting.CreatedAt,
    //                     WaitingTime = TimeOnly.FromTimeSpan((TimeSpan)(DateTime.Now - waiting.CreatedAt)),
    //                     SectionId = waiting.SectionId,
    //                     SectionName = waiting.Section.SectionName
    //                 }).OrderBy(w => w.WaitingId).ToList();
    //         if (waiting == null)
    //         {
    //             return null;
    //         }
    //         return waiting;
    //     }
    //     else
    //     {
    //         var waitingList = _context.Waitinglists.Where(waiting => !waiting.Isdelete && waiting.SectionId == sectionid && !waiting.Isassign)
    //             .Select(waiting => new WaitingTokenDetailViewModel
    //             {
    //                 WaitingId = waiting.WaitingId,
    //                 CustomerId = waiting.CustomerId,
    //                 CustomerName = waiting.Customer.CustomerName,
    //                 PhoneNo = waiting.Customer.PhoneNo,
    //                 Email = waiting.Customer.Email,
    //                 NoOfPerson = waiting.NoOfPerson,
    //                 CreatedAt = (DateTime)waiting.CreatedAt,
    //                     WaitingTime = TimeOnly.FromTimeSpan((TimeSpan)(DateTime.Now - waiting.CreatedAt)),
    //                 SectionId = waiting.SectionId,
    //                 SectionName = waiting.Section.SectionName
    //             }).ToList();

    //         if (waitingList == null)
    //         {
    //             return null;
    //         }
    //         return waitingList;
    //     }
    // }

    public List<WaitingTokenDetailViewModel> GetWaitingList(long sectionid)
    {
        using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection")))
        {
            connection.Open();

            using (var command = new NpgsqlCommand("SELECT get_waiting_list()", connection))
            {
                // command.Parameters.AddWithValue("p_sectionid", sectionid);

                var jsonResult = command.ExecuteScalar();
                if (jsonResult == null)
                {
                    return new List<WaitingTokenDetailViewModel>();
                }

                var data = JsonSerializer.Deserialize<List<WaitingTokenDetailViewModel>>(jsonResult.ToString())
                    ?? new List<WaitingTokenDetailViewModel>();

                if (sectionid != 0)
                {
                    data = data.Where(w => w.SectionId == sectionid).ToList();
                }

                return data;
            }
        }
    }


    // public WaitingTokenDetailViewModel GetWaitingToken(long waitingid)
    // {
    //     Waitinglist? waitingList = _context.Waitinglists.Include(w => w.Customer).Include(wc => wc.Section).FirstOrDefault(wcs => wcs.WaitingId == waitingid && !wcs.Isdelete && !wcs.Isassign);
    //     if (waitingList != null)
    //     {
    //         WaitingTokenDetailViewModel WaitingListVM = new WaitingTokenDetailViewModel
    //         {
    //             WaitingId = waitingList.WaitingId,
    //             CustomerId = waitingList.CustomerId,
    //             CustomerName = waitingList.Customer.CustomerName,
    //             PhoneNo = waitingList.Customer.PhoneNo,
    //             Email = waitingList.Customer.Email,
    //             NoOfPerson = waitingList.NoOfPerson,
    //             SectionName = waitingList.Section.SectionName,
    //             SectionId = waitingList.Section.SectionId,
    //             CreatedAt = (DateTime)waitingList.CreatedAt
    //         };
    //         return WaitingListVM;
    //     }
    //     return null;
    // }

    // public async Task<bool> DeleteWaitingToken(long waitingid, long userId)
    // {
    //     using (var transaction = await _context.Database.BeginTransactionAsync())
    //     {
    //         try
    //         {
    //             var waiting = await _context.Waitinglists.FirstOrDefaultAsync(w => w.WaitingId == waitingid && !w.Isdelete && !w.Isassign);
    //             if (waiting != null)
    //             {
    //                 waiting.Isdelete = true;
    //                 waiting.ModifiedAt = DateTime.Now;
    //                 waiting.ModifiedBy = userId;
    //                 _context.Waitinglists.Update(waiting);
    //                 await _context.SaveChangesAsync();

    //                 await transaction.CommitAsync();
    //                 return true;
    //             }
    //             await transaction.RollbackAsync();

    //             return false;
    //         }
    //         catch
    //         {
    //             await transaction.RollbackAsync();
    //             throw;
    //         }
    //     }
    // }

    public async Task<bool> DeleteWaitingToken(long waitingid, long userId)
    {
        using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection"));

        try
        {
            var result = await connection.ExecuteAsync(
                "CALL delete_waiting_token(@p_waitingid, @p_userid)",
                new { p_waitingid = waitingid, p_userid = userId });

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

    // public List<OrderAppTableVM> GetAvailableTables(long sectionid)
    // {
    //     List<OrderAppTableVM>? tables = _context.Tables
    //        .Where(t => t.SectionId == sectionid && !t.Isdelete && t.Status == "Available")
    //        .Select(t => new OrderAppTableVM
    //        {
    //            TableId = t.TableId,
    //            TableName = t.TableName,
    //            SectionId = t.SectionId,
    //            Capacity = t.Capacity,
    //        }).OrderBy(t => t.TableId).ToList();

    //     if (tables == null)
    //     {
    //         return null;
    //     }
    //     return tables;
    // }

    public List<OrderAppTableVM> GetAvailableTables(long sectionid)
    {
        using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection")))
        {
            connection.Open();

            using (var command = new NpgsqlCommand("SELECT get_available_tables(@inp_sectionid)", connection))
            {
                command.Parameters.AddWithValue("inp_sectionid", sectionid);

                var jsonResult = command.ExecuteScalar();
                if (jsonResult == null)
                {
                    return new List<OrderAppTableVM>();
                }

                var data = JsonSerializer.Deserialize<List<OrderAppTableVM>>(jsonResult.ToString())
                    ?? new List<OrderAppTableVM>();

                return data;
            }
        }
    }

    // public async Task<bool> AssignTableInWaiting(long waitingId, long sectionId, long customerid, int persons, int[] tableIds, long userId)
    // {
    //     using (var transaction = await _context.Database.BeginTransactionAsync())
    //     {
    //         try
    //         {
    //             List<int>? tableIdsList = tableIds.ToList();
    //             Waitinglist? waitinglist = await _context.Waitinglists.Include(x => x.Customer).FirstOrDefaultAsync(x => x.WaitingId == waitingId && !x.Isdelete && !x.Isassign);

    //             if (waitinglist == null)
    //             {
    //                 return false;
    //             }

    //             waitinglist.Isassign = true;
    //             // new Added
    //             waitinglist.Isdelete = true;
    //             waitinglist.AssignedAt = DateTime.Now;
    //             waitinglist.ModifiedAt = DateTime.Now;
    //             waitinglist.ModifiedBy = userId;
    //             _context.Waitinglists.Update(waitinglist);

    //             List<Table>? tables = _context.Tables.Where(t => tableIdsList.Contains((int)t.TableId) && !t.Isdelete && t.Status == "Available").ToList();

    //             if (tables != null)
    //             {

    //                 for (int i = 0; i < tables.Count(); i++)
    //                 {
    //                     AssignTable assignTable = new();
    //                     assignTable.CustomerId = customerid;
    //                     assignTable.TableId = tableIdsList[i];
    //                     assignTable.NoOfPerson = persons;
    //                     assignTable.CreatedAt = DateTime.Now;
    //                     assignTable.CreatedBy = userId;
    //                     await _context.AddAsync(assignTable);

    //                     Table? table = await _context.Tables.FirstOrDefaultAsync(x => x.TableId == tableIdsList[i] && !x.Isdelete);
    //                     if (table != null)
    //                     {
    //                         table.Status = "Assigned";
    //                         table.ModifiedAt = DateTime.Now;
    //                         table.ModifiedBy = userId;
    //                         _context.Tables.Update(table);
    //                         await _context.SaveChangesAsync();
    //                     }
    //                 }
    //             }
    //             else
    //             {
    //                 return false;
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

    public async Task<bool> AssignTableInWaiting(long waitingId, long sectionId, long customerid, int persons, int[] tableIds, long userId)
    {
        using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection"));

        try
        {
            var result = await connection.ExecuteAsync(
                "CALL assign_table_in_waiting(@inp_waitingid, @inp_sectionid, @inp_customerid, @inp_persons, @inp_tableids, @inp_userid)",
                new { inp_waitingid = waitingId, inp_sectionid = sectionId, inp_customerid = customerid, inp_persons = persons, inp_tableids = tableIds, inp_userid = userId });

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

    // public async Task<bool> AlreadyAssigned(long customerid)
    // {
    //     AssignTable? IsTableAssigned = await _context.AssignTables.FirstOrDefaultAsync(at => at.CustomerId == customerid && !at.Isdelete);
    //     if (IsTableAssigned != null)
    //     {
    //         return true;
    //     }
    //     return false;
    // }

    public async Task<bool> AlreadyAssigned(long customerid)
    {
        using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection")))
        {
            connection.Open();

            using (var command = new NpgsqlCommand("SELECT already_assigned(@inp_customerid)", connection))
            {
                command.Parameters.AddWithValue("inp_customerid", customerid);

                try
                {
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToBoolean(result);
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

}