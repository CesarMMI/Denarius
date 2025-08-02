using Microsoft.AspNetCore.Mvc;

namespace Denarius.Web.Extensions;

internal static class ResultExtensions
{
    public static async Task<IActionResult> Status<T>(this Task<T> resultTask, int statusCode) where T : struct
    {
        return new ObjectResult(await resultTask) { StatusCode = statusCode };
    }

    public static async Task<IActionResult> Status<T>(this Task<IEnumerable<T>> resultTask, int statusCode) where T : struct
    {
        return new ObjectResult(await resultTask) { StatusCode = statusCode };
    }

    public static async Task<IActionResult> Ok<T>(this Task<T> resultTask) where T : struct
    {
        return await resultTask.Status(StatusCodes.Status200OK);
    }

    public static async Task<IActionResult> Ok<T>(this Task<IEnumerable<T>> resultTask) where T : struct
    {
        return await resultTask.Status(StatusCodes.Status200OK);
    }

    public static async Task<IActionResult> Created<T>(this Task<T> resultTask) where T : struct
    {
        return await resultTask.Status(StatusCodes.Status201Created);
    } 

    public static async Task<IActionResult> NoContent<T>(this Task<T> resultTask) where T : struct
    {
        await resultTask;
        return new NoContentResult();
    }
}