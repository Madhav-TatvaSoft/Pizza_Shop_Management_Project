using Microsoft.AspNetCore.Authorization;

namespace Pizza_Shop_Project.Authorization;

public class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    public PermissionAuthorizeAttribute(string permission) : base()
    {
        Policy = $"{permission}";
    }
}






