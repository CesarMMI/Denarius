using Denarius.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.Web.Middlewares;


public class ExceptionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            await HandleExceptionAsync(context, ex.StatusCode, ex.Title, ex.Message);
        }
        catch (Exception)
        {
            await HandleExceptionAsync(context, StatusCodes.Status500InternalServerError, "Internal Server Error", "Internal Server Error");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, int status, string title, string detail)
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Status = status
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}
