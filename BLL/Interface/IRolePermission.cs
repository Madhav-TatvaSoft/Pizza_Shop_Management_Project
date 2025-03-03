using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface IRolePermission
{
    public List<Role> GetAllRoles();

    public bool EditPermissionMapping(RolesPermissionViewModel rolepermissionmapping);

    public List<RolesPermissionViewModel> GetPermissionByRole(string name);

}
