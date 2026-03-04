using Denarius.Application.Exceptions;
using Denarius.Domain.Exceptions;

namespace Denarius.API.Middlewares;

public class ExceptionMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            await HandleDomainException(context, ex);
        }
        catch (AppException ex)
        {
            await HandleAppException(context, ex);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex, StatusCodes.Status500InternalServerError);
        }
    }

    private static Task HandleAppException(HttpContext context, Exception ex)
    {
        return ex switch
        {
            NotFoundException => HandleException(context, ex, StatusCodes.Status404NotFound),
            _ => HandleException(context, ex, StatusCodes.Status422UnprocessableEntity),
        };
    }

    private static Task HandleDomainException(HttpContext context, DomainException ex)
    {
        return HandleException(context, ex, StatusCodes.Status400BadRequest);
    }

    private static Task HandleException(HttpContext context, Exception ex, int statusCode)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            type = ex.GetType().Name,
            title = ex.Message,
            status = statusCode,
            instance = context.Request.Path.Value,
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}