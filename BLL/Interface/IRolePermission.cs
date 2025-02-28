using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface IRolePermission
{
    public List<Role> GetAllRoles();

    public List<Rolepermissionmapping> GetPermissionsByRole(int id);

    public bool EditPermissionManage(RolesPermissionViewModel rolepermissionmapping);


}
