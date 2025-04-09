
using Microsoft.EntityFrameworkCore;
using DAL.Models;
using BLL.Implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using BLL.Interface;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authorization;
using Pizza_Shop_Project.Authorization;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var conn = builder.Configuration.GetConnectionString("PizzaShopConnection");
builder.Services.AddDbContext<PizzaShopDbContext>(q => q.UseNpgsql(conn));
builder.Services.AddScoped<IUserLoginService,UserLoginService>();
builder.Services.AddScoped<IUserService,UserService>();
builder.Services.AddScoped<IJWTService,JWTService>();
builder.Services.AddScoped<IRolePermission,RolePermissionService>();
builder.Services.AddScoped<ICategoryService,CategoryService>();
builder.Services.AddScoped<IItemService,ItemService>();
builder.Services.AddScoped<IModifierGroupService,ModifierGroupService>();
builder.Services.AddScoped<IModifierItemService,ModifierItemService>();
builder.Services.AddScoped<ITableSectionService, TableSectionService>();
builder.Services.AddScoped<ITaxFeesService, TaxFeesService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService,CustomerService>();
builder.Services.AddScoped<IOrderAppKOTService,OrderAppKOTService>();
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(x=>{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtConfig:Issuer"],  // The issuer of the token (e.g., your app's URL)
            ValidAudience = builder.Configuration["JwtConfig:Audience"], // The audience for the token (e.g., your API)
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]?? "")), // The key to validate the JWT's signature
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name 
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Check for the token in cookies
                var token = context.Request.Cookies["AuthToken"]; // Change "AuthToken" to your cookie name if it's different
                // if (!string.IsNullOrEmpty(token))
                // {
                //     context.Request.Headers["Authorization"] = "Bearer " + token;
                // }
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // Redirect to login page when unauthorized 
                context.HandleResponse();
                context.Response.Redirect("/Error/Unauthorized");
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                // Redirect to login when access is forbidden (403)
                context.Response.Redirect("/Error/Forbidden");
                return Task.CompletedTask;
            }
        };
    }
);  

builder.Services.AddAuthorization(options =>
{
    var permissions = new[]
    {
        "Users.View", "Users.AddEdit", "Users.Delete",
        "Role.View", "Role.AddEdit", "Role.Delete",
        "Menu.View", "Menu.AddEdit", "Menu.Delete",
        "TableSection.View", "TableSection.AddEdit", "TableSection.Delete",
        "TaxFees.View", "TaxFees.AddEdit", "TaxFees.Delete",
        "Orders.View", "Orders.AddEdit", "Orders.Delete",
        "Customers.View", "Customers.AddEdit", "Customers.Delete",
        "AccountManager", "Chef"  
    };

    foreach (var permission in permissions)
    {
        options.AddPolicy(permission, policy => policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

builder.Services.AddAuthorization();

// app.Use(async (context, next) =>
// {
//     context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
//     context.Response.Headers.Add("Pragma", "no-cache");
//     context.Response.Headers.Add("Expires", "0");

//     await next();
// });


builder.Services.AddSession(
    options => {
        options.IdleTimeout = TimeSpan.FromHours(10);
    }
);

builder.Services.AddSingleton<IHttpContextAccessor,HttpContextAccessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/InternalServerError");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRotativa();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=UserLogin}/{action=VerifyUserLogin}");

app.Run();