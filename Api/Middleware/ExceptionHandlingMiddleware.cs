using System.Net;
using Application.Exceptions;

namespace Api.Middleware;

// Sits at the very start of the request pipeline (see Program.cs) and wraps every
// request in a try/catch, so controllers don't each need their own try/catch just to
// turn an exception into a sensible HTTP response. Add a new "catch" block here
// whenever a new exception type needs its own status code.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Runs the rest of the pipeline (routing, the controller action, etc).
            // If anything below throws, we catch it here instead of it becoming a
            // raw, unhandled 500 with a stack trace leaking to the caller.
            await _next(context);
        }
        catch (InvalidIpAddressException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Invalid IP address", ex.Message);
        }
        catch (Exception ex)
        {
            // Anything we didn't explicitly plan for - log the real details for us,
            // but don't leak them to the caller.
            _logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);
            await WriteProblemAsync(
                context,
                HttpStatusCode.InternalServerError,
                "Unexpected error",
                "An unexpected error occurred while processing the request.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string title, string detail)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new
        {
            title,
            status = (int)statusCode,
            detail
        };

        return context.Response.WriteAsJsonAsync(problem);
    }
}

// Small extension so Program.cs reads as app.UseExceptionHandling() instead of the
// more verbose app.UseMiddleware<ExceptionHandlingMiddleware>().
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
