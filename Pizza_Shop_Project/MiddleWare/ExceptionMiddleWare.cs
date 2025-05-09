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
        HttpStatusCode code;
        string message;

        switch (exception)
        {
            case NotFoundException:
                code = HttpStatusCode.NotFound;
                message = exception.Message;
                break;
            default:
                code = HttpStatusCode.InternalServerError;
                message = "Internal server error. Please try again later.";
                break;
        }

        _logger.LogError(exception, "An unhandled exception occurred.");

        bool isAjax = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        

        if (isAjax)
        {
            // For AJAX - return JSON response
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200; // Always OK (to avoid redirect issues)

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
            // // For Normal Requests - use TempData for Toastr
            // context.Response.Redirect($"/Error/HandleErrorWithToaster?message={Uri.EscapeDataString(message)}");
            if (!context.Response.HasStarted)
            {
                var redirectUrl = $"/Error/HandleErrorWithToaster?message={Uri.EscapeDataString(message)}";
                _logger.LogInformation("Redirecting to: {RedirectUrl}", redirectUrl);
                context.Response.StatusCode = (int)HttpStatusCode.Redirect; // 302
                context.Response.Headers["Location"] = redirectUrl; // Manual redirect
                // return; // Exit to prevent further processing
                await context.Response.CompleteAsync();
            }
            else
            {
                _logger.LogWarning("Response already started, cannot redirect.");
                context.Response.StatusCode = (int)code;
                await context.Response.WriteAsync(message);
            }
        }
    }

}