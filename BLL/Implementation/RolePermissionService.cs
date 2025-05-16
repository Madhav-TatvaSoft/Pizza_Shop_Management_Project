using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class RolePermissionService : IRolePermission
{
    private readonly PizzaShopDbContext _context;

    public RolePermissionService(PizzaShopDbContext context)
    {
        _context = context;
    }

    public List<Role> GetAllRoles()
    {
        return _context.Roles.ToList();
    }

    #region Update 
    public bool EditPermissionMapping(RolesPermissionViewModel rolepermissionmapping)
    {
        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                Rolepermissionmapping? data = _context.Rolepermissionmappings.FirstOrDefault(x => x.RolepermissionmappingId == rolepermissionmapping.RolepermissionmappingId);
                if (data == null)
                {
                    return false;
                }
                data.Canview = rolepermissionmapping.Canview;
                data.Canaddedit = rolepermissionmapping.Canaddedit;
                data.Candelete = rolepermissionmapping.Candelete;
                // data.Permissioncheck = rolepermissionmapping.Permissioncheck;
                _context.Update(data);
                _context.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

    }
    
    #endregion

    #region Get
    public List<RolesPermissionViewModel> GetPermissionByRole(string name)
    {
        List<Rolepermissionmapping> data = _context.Rolepermissionmappings.Include(x => x.Role).Include(x => x.Permission).Where(x => x.Role.RoleName == name).OrderBy(x => x.PermissionId).ToList();

        List<RolesPermissionViewModel> permissions = new();

        for (int i = 0; i < data.Count; i++)
        {
            RolesPermissionViewModel obj = new();

            obj.RolepermissionmappingId = data[i].RolepermissionmappingId;
            obj.RoleName = data[i].Role.RoleName;
            obj.Name = data[i].Permission.PermissionsName;
            obj.Canview = data[i].Canview;
            obj.Canaddedit = data[i].Canaddedit;
            obj.Candelete = data[i].Candelete;
            // obj.Permissioncheck = data[i].Permissioncheck;
            permissions.Add(obj);
        }
        return permissions;
    }
    
    #endregion

}