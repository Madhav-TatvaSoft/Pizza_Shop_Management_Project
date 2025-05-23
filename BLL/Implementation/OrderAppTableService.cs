using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BLL.Implementation;

public class OrderAppTableService : IOrderAppTableService
{
    private readonly PizzaShopDbContext _context;
    private readonly ICustomerService _customerService;
    private readonly IConfiguration _configuration;

    public OrderAppTableService(PizzaShopDbContext context, ICustomerService customerService, IConfiguration configuration)
    {
        _configuration = configuration;
        _context = context;
        _customerService = customerService;
    }

    // public List<OrderAppSectionVM> GetAllSectionList()
    // {
    //     List<OrderAppSectionVM> sectionList = _context.Sections
    //         .Where(sec => !sec.Isdelete).OrderBy(sec => sec.SectionId)
    //         .Select(sec => new OrderAppSectionVM
    //         {
    //             SectionId = sec.SectionId,
    //             SectionName = sec.SectionName,
    //             AvailableCount = sec.Tables.Count(table => table.Status == "Available" && !table.Isdelete),
    //             AssignedCount = sec.Tables.Count(table => table.Status == "Assigned" || table.Status == "Occupied" && !table.Isdelete),
    //             RunningCount = sec.Tables.Count(table => table.Status == "Running" && !table.Isdelete),
    //             WaitingCount = sec.Waitinglists.Count(w => w.SectionId == sec.SectionId && !w.Isdelete && !w.Isassign),
    //         }).ToList();

    //     if (sectionList == null)
    //     {
    //         return null;
    //     }

    //     return sectionList;
    // }

    public async Task<List<OrderAppSectionVM>> GetAllSectionList()
    {
        using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection"));
        await connection.OpenAsync();

        using var command = new NpgsqlCommand("SELECT get_section_list_orderapp()", connection);

        var jsonResult = await command.ExecuteScalarAsync();
        if (jsonResult == null)
        {
            return new List<OrderAppSectionVM>();
        }

        var data = JsonSerializer.Deserialize<List<OrderAppSectionVM>>(jsonResult.ToString())
            ?? new List<OrderAppSectionVM>();

        return data;
    }

    // public List<OrderAppTableVM> GetTablesBySection(long SectionId)
    // {
    //     List<OrderAppTableVM>? tableListVM = _context.Tables
    //     .Include(table => table.AssignTables).ThenInclude(At => At.Order)
    //     .Where(table => table.Section.SectionId == SectionId && !table.Isdelete).OrderBy(table => table.TableId)
    //     .Select(table => new OrderAppTableVM
    //     {
    //         TableId = table.TableId,
    //         SectionId = table.SectionId,
    //         TableName = table.TableName,
    //         Capacity = table.Capacity,
    //         Status = table.Status,
    //         TableTime = (DateTime)(table.AssignTables.FirstOrDefault(x => !x.Isdelete) != null ? table.AssignTables.FirstOrDefault(x => !x.Isdelete).CreatedAt : DateTime.Now),
    //         OrderAmount = table.Status == "Running" ? (table.AssignTables.FirstOrDefault(x => !x.Isdelete) != null ? (table.AssignTables.FirstOrDefault(x => !x.Isdelete).Order != null ? table.AssignTables.FirstOrDefault(x => !x.Isdelete).Order.TotalAmount : 0) : 0) : 0
    //     }).ToList();

    //     if (tableListVM == null)
    //     {
    //         return null;
    //     }

    //     return tableListVM;
    // }

    public List<OrderAppTableVM> GetTablesBySection(long SectionId)
    {
        using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection"));

        connection.Open();

        using var command = new NpgsqlCommand("SELECT get_tables_by_section_orderapp(@inp_sectionid)", connection);

        command.Parameters.AddWithValue("inp_sectionid", SectionId);

        var jsonResult = command.ExecuteScalar();

        if (jsonResult == null)
        {
            return new List<OrderAppTableVM>();
        }

        var data = JsonSerializer.Deserialize<List<OrderAppTableVM>>(jsonResult.ToString())
            ?? new List<OrderAppTableVM>();

        return data;
    }

    // public bool CheckTokenExists(WaitingTokenDetailViewModel waitingTokenVM)
    // {
    //     Waitinglist? waitinglist = _context.Waitinglists.FirstOrDefault(w => w.Customer.Email == waitingTokenVM.Email && w.WaitingId != waitingTokenVM.WaitingId && !w.Isdelete && !w.Isassign);
    //     if (waitinglist != null)
    //     {
    //         return true;
    //     }
    //     return false;
    // }

    public bool CheckTokenExists(WaitingTokenDetailViewModel waitingTokenVM)
    {
        using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection")))
        {
            connection.Open();

            using (var command = new NpgsqlCommand("SELECT check_token_exists(@inp_email,@inp_waitingid)", connection))
            {
                command.Parameters.AddWithValue("inp_email", waitingTokenVM.Email);
                command.Parameters.AddWithValue("inp_waitingid", waitingTokenVM.WaitingId);

                try
                {
                    var result = command.ExecuteScalar();
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

    // public async Task<bool> SaveCustomerToWaitingList(WaitingTokenDetailViewModel waitingTokenVM, long userId)
    // {
    //     using (var transaction = await _context.Database.BeginTransactionAsync())
    //     {
    //         try
    //         {
    //             long customerId = _customerService.IsCustomerPresent(waitingTokenVM.Email);

    //             if (waitingTokenVM.WaitingId == 0)
    //             {
    //                 Waitinglist waitinglist = new();
    //                 waitinglist.CustomerId = customerId;
    //                 waitinglist.NoOfPerson = waitingTokenVM.NoOfPerson;
    //                 waitinglist.SectionId = waitingTokenVM.SectionId;
    //                 waitinglist.CreatedAt = DateTime.Now;
    //                 waitinglist.CreatedBy = userId;
    //                 await _context.Waitinglists.AddAsync(waitinglist);

    //             }
    //             else
    //             {
    //                 Waitinglist? waitinglist = await _context.Waitinglists.FirstOrDefaultAsync(w => w.WaitingId == waitingTokenVM.WaitingId && !w.Isdelete && !w.Isassign);
    //                 if (waitinglist != null)
    //                 {
    //                     waitinglist.CustomerId = customerId;
    //                     waitinglist.WaitingId = waitingTokenVM.WaitingId;
    //                     waitinglist.NoOfPerson = waitingTokenVM.NoOfPerson;
    //                     waitinglist.SectionId = waitingTokenVM.SectionId;
    //                     waitinglist.ModifiedAt = DateTime.Now;
    //                     waitinglist.ModifiedBy = userId;
    //                     _context.Update(waitinglist);
    //                 }
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

    public async Task<bool> SaveCustomerToWaitingList(WaitingTokenDetailViewModel waitingTokenVM, long userId)
    {
        using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection"));

        try
        {
            await connection.ExecuteAsync(
                "CALL save_customer_to_waiting_list(@inp_waitingid, @inp_email, @inp_noofperson, @inp_sectionid, @inp_userid)",
                new
                {
                    inp_waitingid = waitingTokenVM.WaitingId,
                    inp_email = waitingTokenVM.Email,
                    inp_noofperson = waitingTokenVM.NoOfPerson,
                    inp_sectionid = waitingTokenVM.SectionId,
                    inp_userid = userId
                });

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

    // public async Task<List<WaitingTokenDetailViewModel>> GetWaitingCustomerList(long sectionid)
    // {
    //     List<WaitingTokenDetailViewModel>? WaitingCustomer = await _context.Waitinglists.Include(w => w.Customer).Include(c => c.Section)
    //     .Where(w => !w.Isdelete && w.Section.SectionId == sectionid && !w.Isassign)
    //     .Select(w => new WaitingTokenDetailViewModel
    //     {
    //         WaitingId = w.WaitingId,
    //         CustomerName = w.Customer.CustomerName,
    //         PhoneNo = w.Customer.PhoneNo,
    //         Email = w.Customer.Email,
    //         NoOfPerson = w.NoOfPerson,
    //         SectionId = w.Section.SectionId,
    //         SectionName = w.Section.SectionName
    //     }).ToListAsync();

    //     if (WaitingCustomer != null)
    //     {
    //         return WaitingCustomer;
    //     }

    //     return null;
    // }

    public async Task<List<WaitingTokenDetailViewModel>> GetWaitingCustomerList(long sectionid)
    {
        using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection"));
        await connection.OpenAsync();

        using var command = new NpgsqlCommand("SELECT get_waiting_customer_list_orderapp(@inp_sectionid)", connection);
        command.Parameters.AddWithValue("inp_sectionid", sectionid);

        var jsonResult = await command.ExecuteScalarAsync();
        if (jsonResult == null)
        {
            return new List<WaitingTokenDetailViewModel>();
        }

        var data = JsonSerializer.Deserialize<List<WaitingTokenDetailViewModel>>(jsonResult.ToString())
            ?? new List<WaitingTokenDetailViewModel>();

        return data;
    }

    #region Get Waiting Customer List By Id

    // public async Task<WaitingTokenDetailViewModel> GetCustomerDetails(long waitingId)
    // {
    //     WaitingTokenDetailViewModel? waitingCustomer = await _context.Waitinglists.Include(w => w.Customer).Include(c => c.Section)
    //     .Where(w => !w.Isdelete && w.WaitingId == waitingId)
    //     .Select(w => new WaitingTokenDetailViewModel
    //     {
    //         WaitingId = w.WaitingId,
    //         CustomerId = w.Customer.CustomerId,
    //         CustomerName = w.Customer.CustomerName,
    //         PhoneNo = w.Customer.PhoneNo,
    //         Email = w.Customer.Email,
    //         NoOfPerson = w.NoOfPerson,
    //         SectionId = w.Section.SectionId,
    //         SectionName = w.Section.SectionName
    //     }).FirstOrDefaultAsync();

    //     if (waitingCustomer == null) return null;

    //     return waitingCustomer;
    // }

    public async Task<WaitingTokenDetailViewModel> GetCustomerDetails(long waitingId)
    {
        using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection"));
        await connection.OpenAsync();

        using var command = new NpgsqlCommand("SELECT get_customer_details_orderapp(@inp_waitingid)", connection);
        command.Parameters.AddWithValue("inp_waitingid", waitingId);

        var jsonResult = await command.ExecuteScalarAsync();
        if (jsonResult == null)
        {
            return new WaitingTokenDetailViewModel();
        }

        var data = JsonSerializer.Deserialize<WaitingTokenDetailViewModel>(jsonResult.ToString())
            ?? new WaitingTokenDetailViewModel();

        return data;
    }

    #endregion

    #region Assign Table

    // public async Task<bool> AssignTable(OrderAppTableMainViewModel TableMainVM, long userId)
    // {
    //     using (var transaction = await _context.Database.BeginTransactionAsync())
    //     {
    //         try
    //         {
    //             JsonArray? tableIds = JsonSerializer.Deserialize<JsonArray>(TableMainVM.TableIds);
    //             List<int>? tableIdList = tableIds.Select(id => int.Parse(id.GetValue<string>())).ToList();
    //             Waitinglist? waitinglist = await _context.Waitinglists.Include(x => x.Customer).FirstOrDefaultAsync(x => x.WaitingId == TableMainVM.waitingTokenDetailViewModel.WaitingId && !x.Isdelete && !x.Isassign);

    //             if (waitinglist != null)
    //             {
    //                 waitinglist.Isassign = true;
    //                 waitinglist.AssignedAt = DateTime.Now;
    //                 waitinglist.ModifiedAt = DateTime.Now;
    //                 waitinglist.ModifiedBy = userId;
    //                 _context.Waitinglists.Update(waitinglist);
    //             }

    //             List<Table>? tables = _context.Tables.Where(t => tableIdList.Contains((int)t.TableId) && !t.Isdelete && t.Status == "Available").ToList();

    //             if (tables != null)
    //             {

    //                 for (int i = 0; i < tableIdList.Count(); i++)
    //                 {
    //                     AssignTable assignTable = new();
    //                     assignTable.CustomerId = _customerService.IsCustomerPresent(TableMainVM.waitingTokenDetailViewModel.Email);
    //                     assignTable.TableId = tableIdList[i];
    //                     assignTable.NoOfPerson = TableMainVM.waitingTokenDetailViewModel.NoOfPerson;
    //                     assignTable.CreatedAt = DateTime.Now;
    //                     assignTable.CreatedBy = userId;
    //                     await _context.AddAsync(assignTable);
    //                     Table? table = await _context.Tables.FirstOrDefaultAsync(x => x.TableId == tableIdList[i] && !x.Isdelete);
    //                     if (table != null)
    //                     {
    //                         table.Status = "Assigned";
    //                         table.ModifiedAt = DateTime.Now;
    //                         table.ModifiedBy = userId;
    //                         _context.Tables.Update(table);
    //                     }
    //                 }
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

    public async Task<bool> AssignTable(OrderAppTableMainViewModel TableMainVM, long userId)
    {

        using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PizzaShopConnection"));

        try
        {
            JsonArray? tableIds = JsonSerializer.Deserialize<JsonArray>(TableMainVM.TableIds);
            List<int>? tableIdList = tableIds.Select(id => int.Parse(id.GetValue<string>())).ToList();

            await connection.ExecuteAsync(
                "CALL assign_table(@inp_waitingid, @inp_email, @inp_noofperson, @inp_tableids, @inp_userid)",
                new
                {
                    inp_waitingid = TableMainVM.waitingTokenDetailViewModel.WaitingId,
                    inp_email = TableMainVM.waitingTokenDetailViewModel.Email,
                    inp_noofperson = TableMainVM.waitingTokenDetailViewModel.NoOfPerson,
                    inp_tableids = tableIdList,
                    inp_userid = userId
                });

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

    

    #endregion

}