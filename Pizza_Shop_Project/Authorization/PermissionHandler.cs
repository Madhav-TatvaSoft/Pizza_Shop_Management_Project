
using BLL.Interface;
using Microsoft.AspNetCore.Authorization;

namespace Pizza_Shop_Project.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IRolePermission _roleService;
    private readonly IJWTService _jWTService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionHandler(IRolePermission roleService, IJWTService jWTService, IHttpContextAccessor httpContextAccessor)
    {
        this._roleService = roleService;
        this._jWTService = jWTService;
        this._httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            context.Fail(); // No HTTP context available
            return Task.CompletedTask;
        }

        var cookieSavedToken = httpContext.Request.Cookies["AuthToken"];

        // Check if the token is missing (Unauthenticated user)
        if (string.IsNullOrEmpty(cookieSavedToken))
        {
            httpContext.Response.Cookies.Delete("email");
            httpContext.Response.Cookies.Delete("username");
            httpContext.Response.Cookies.Delete("profileImage");

            httpContext.Response.Redirect("/UserLogin/VerifyUserLogin");
            return Task.CompletedTask;
        }

        var roleName = _jWTService.GetClaimValue(cookieSavedToken, "role");

        // Check if the role is missing (Invalid token)
        if (string.IsNullOrEmpty(roleName))
        {
            httpContext.Response.Redirect("/Error/Unauthorized");
            return Task.CompletedTask;
        }

        var permissionsData = _roleService.GetPermissionByRole(roleName);

        // Ensure permissionsData is valid before accessing index positions
        // if (permissionsData == null)
        // {
        //     httpContext.Response.Redirect("/Error/Forbidden");
        //     return Task.CompletedTask;
        // }

        switch (requirement.Permission)
        {
            case "Users.View":
                if (permissionsData[0].Canview)
                    context.Succeed(requirement);
                break;
            case "Users.AddEdit":
                if (permissionsData[0].Canaddedit)
                    context.Succeed(requirement);
                break;
            case "Users.Delete":
                if (permissionsData[0].Candelete)
                    context.Succeed(requirement);
                break;
            case "Role.View":
                if (permissionsData[1].Canview)
                    context.Succeed(requirement);
                break;
            case "Role.AddEdit":
                if (permissionsData[1].Canaddedit)
                    context.Succeed(requirement);
                break;
            case "Role.Delete":
                if (permissionsData[1].Candelete)
                    context.Succeed(requirement);
                break;
            case "Menu.View":
                if (permissionsData[2].Canview)
                    context.Succeed(requirement);
                break;
            case "Menu.AddEdit":
                if (permissionsData[2].Canaddedit)
                    context.Succeed(requirement);
                break;
            case "Menu.Delete":
                if (permissionsData[2].Candelete)
                    context.Succeed(requirement);
                break;
            case "TableSection.View":
                if (permissionsData[3].Canview)
                    context.Succeed(requirement);
                break;
            case "TableSection.AddEdit":
                if (permissionsData[3].Canaddedit)
                    context.Succeed(requirement);
                break;
            case "TableSection.Delete":
                if (permissionsData[3].Candelete)
                    context.Succeed(requirement);
                break;
            case "TaxFees.View":
                if (permissionsData[4].Canview)
                    context.Succeed(requirement);
                break;
            case "TaxFees.AddEdit":
                if (permissionsData[4].Canaddedit)
                    context.Succeed(requirement);
                break;
            case "TaxFees.Delete":
                if (permissionsData[4].Candelete)
                    context.Succeed(requirement);
                break;
            case "Orders.View":
                if (permissionsData[5].Canview)
                    context.Succeed(requirement);
                break;
            case "Orders.AddEdit":
                if (permissionsData[5].Canaddedit)
                    context.Succeed(requirement);
                break;
            case "Orders.Delete":
                if (permissionsData[5].Candelete)
                    context.Succeed(requirement);
                break;
            case "Customers.View":
                if (permissionsData[6].Canview)
                    context.Succeed(requirement);
                break;
            case "Customers.AddEdit":
                if (permissionsData[6].Canaddedit)
                    context.Succeed(requirement);
                break;
            case "Customers.Delete":
                if (permissionsData[6].Candelete)
                    context.Succeed(requirement);
                break;
            case "AccountManager":
                if (roleName == "Account Manager")
                    context.Succeed(requirement);
                break;
            default:
                context.Fail();
                break;
        }
        return Task.CompletedTask;
    }

}
