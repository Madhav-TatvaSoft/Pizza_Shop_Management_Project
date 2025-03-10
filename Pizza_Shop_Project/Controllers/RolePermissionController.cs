using BLL.Interface;
using DAL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Pizza_Shop_Project.Controllers
{
    public class RolePermissionController : Controller
    {
        private readonly IRolePermission _rolePermission;

        public RolePermissionController(IRolePermission rolePermission)
        {
            _rolePermission = rolePermission;
        }

        //Fetching roles
        public IActionResult RoleDashboard()
        {
            ViewData["sidebar-active"] = "Role";
            var Roles = _rolePermission.GetAllRoles();
            return View(Roles);
        }

        public IActionResult Permission(string name)
        {
            ViewData["sidebar-active"] = "Role";
            List<RolesPermissionViewModel> permissions = _rolePermission.GetPermissionByRole(name);
            return View(permissions);
        }

        [HttpPost]
        public IActionResult Permission(List<RolesPermissionViewModel> rolesPermissionViewModel)
        {
            for (int i = 0; i < rolesPermissionViewModel.Count; i++)
            {
                RolesPermissionViewModel rolesPermissionvm = new RolesPermissionViewModel();
                rolesPermissionvm.RolepermissionmappingId = rolesPermissionViewModel[i].RolepermissionmappingId;
                rolesPermissionvm.Canview = rolesPermissionViewModel[i].Canview;
                rolesPermissionvm.Canaddedit = rolesPermissionViewModel[i].Canaddedit;
                rolesPermissionvm.Candelete = rolesPermissionViewModel[i].Candelete;
                rolesPermissionvm.Permissioncheck = rolesPermissionViewModel[i].Permissioncheck;
                _rolePermission.EditPermissionMapping(rolesPermissionvm);
            }
            TempData["SuccessMessage"] = "Permissions Updated Successfully";
            return RedirectToAction("Permission", "RolePermission", new { name = rolesPermissionViewModel[0].RoleName });// 3rd para ma obj create krvopade bcoz
                                                                                                                         //  redirectToAction ma 3rd para obj accept kre string nai..nd get method ma name pass krva mate ahiyathi name no ob banavvi moklvu
        }
    }
}