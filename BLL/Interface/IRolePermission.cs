using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface IRolePermission
{
    List<Role> GetAllRoles();

    bool EditPermissionMapping(RolesPermissionViewModel rolepermissionmapping);

    List<RolesPermissionViewModel> GetPermissionByRole(string name);

}