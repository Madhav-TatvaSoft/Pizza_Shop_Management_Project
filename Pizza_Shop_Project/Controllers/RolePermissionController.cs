using BLL.Implementation;
using DAL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Pizza_Shop_Project.Controllers
{
    public class RolePermissionController : Controller
    {
        private readonly RolePermissionService _rolePermission;

        public RolePermissionController(RolePermissionService rolePermission)
        {
            _rolePermission = rolePermission;
        }

        //Fetching roles
        public IActionResult RoleDashboard()
        {
            var Roles = _rolePermission.GetAllRoles();
            return View(Roles);
        }

        public IActionResult Permission(int id)
        {
            var role = _rolePermission.GetPermissionsByRole(id);
            return View(role);
        }

        [HttpPost]
        public IActionResult Permission(List<RolesPermissionViewModel> rolesPermissionViewModel)
        {
            for (int i = 0; i < rolesPermissionViewModel.Count; i++)
            {
                RolesPermissionViewModel rolesPermissionVM = new RolesPermissionViewModel();
                rolesPermissionVM.RolepermissionmappingId = rolesPermissionViewModel[i].RolepermissionmappingId;
                rolesPermissionVM.Canview = rolesPermissionViewModel[i].Canview;
                rolesPermissionVM.Canaddedit = rolesPermissionViewModel[i].Canaddedit;
                rolesPermissionVM.Candelete = rolesPermissionViewModel[i].Candelete;
                rolesPermissionVM.Permissioncheck = rolesPermissionViewModel[i].Permissioncheck;
                _rolePermission.EditPermissionManage(rolesPermissionVM);
            }
            return RedirectToAction("Permission");
        }
    }
}