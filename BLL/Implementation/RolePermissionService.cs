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

    public List<Rolepermissionmapping> GetPermissionsByRole(int id)
    {
        return _context.Rolepermissionmappings.Include(x => x.Role).Where(x => x.RoleId == id).OrderBy(x => x.PermissionId).ToList();
    }

    public bool EditPermissionManage(RolesPermissionViewModel rolepermissionmapping)
    {
        var data = _context.Rolepermissionmappings.FirstOrDefault(x => x.RolepermissionmappingId == rolepermissionmapping.RolepermissionmappingId);
        if (data == null)
        {
            return false;
        }
        data.Canview = rolepermissionmapping.Canview;
        data.Canaddedit = rolepermissionmapping.Canaddedit;
        data.Candelete = rolepermissionmapping.Candelete;
        data.Permissioncheck = rolepermissionmapping.Permissioncheck;
        _context.Update(data);
        _context.SaveChanges();
        return true;
    }

}
