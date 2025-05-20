using System.Net;
using System.Text.Json;
using BLL;

namespace Pizza_Shop_Project.MiddleWare;

public class ExceptionMiddleWare
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleWare> _logger;
    public ExceptionMiddleWare(RequestDelegate next, ILogger<ExceptionMiddleWare> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode code = HttpStatusCode.InternalServerError;
        string message = "Something went wrong. Please try after some time.";

        _logger.LogError(exception, "An unhandled exception occurred.");

        bool isAjax = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (isAjax)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.Headers.Add("X-Error", "true");

            var jsonResponse = new
            {
                success = false,
                statusCode = (int)code,
                error = message
            };

            string jsonMessage = JsonSerializer.Serialize(jsonResponse);
            await context.Response.WriteAsync(jsonMessage);
        }
        else
        {
            var redirectUrl = $"/Error/HandleErrorWithToaster?message={Uri.EscapeDataString(message)}";
            _logger.LogInformation("Redirecting to: {RedirectUrl}", redirectUrl);
            context.Response.StatusCode = (int)HttpStatusCode.Redirect; // 302 To Temporary redirect
            context.Response.Headers["Location"] = redirectUrl;
            await context.Response.CompleteAsync();
        }
    }
}