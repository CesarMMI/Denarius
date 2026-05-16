using System.Text.Json;
using Denarius.Application.Exceptions;
using Denarius.Application.Exceptions.Accounts;
using Denarius.Application.Exceptions.Transactions;
using Denarius.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (status, title, detail) = ex switch
        {
            NotFoundException => (404, "Not Found", ex.Message),
            InactiveAccountException => (422, "Unprocessable Entity", ex.Message),
            InvalidDateRangeException => (400, "Bad Request", ex.Message),
            DomainException => (400, "Bad Request", ex.Message),
            AppException => (400, "Bad Request", ex.Message),
            _ => (500, "Internal Server Error", "An unexpected error occurred.")
        };

        var problem = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807#section-3.1",
            Title = title,
            Status = status,
            Detail = detail
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
