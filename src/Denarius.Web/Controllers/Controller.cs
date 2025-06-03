using Denarius.Application.Auth.Services;
using Denarius.Application.Shared.Command;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.Web.Controllers;

public class Controller : ControllerBase
{
    protected Task<IActionResult> HandleCommand<Q, R>(ICommand<Q, R> command, Q query)
        where Q : Query
        where R : class
    {
        return HandleCommand(command, query, Ok);
    }

    protected async Task<IActionResult> HandleCommand<Q, R>(ICommand<Q, R> command, Q query, Func<R, IActionResult> resultHandler)
        where Q : Query
        where R : class
    {
        try
        {
            var userId = HttpContext.User.FindFirst("sub")?.Value;
            query.UserId = userId is null ? 0 : int.Parse(userId);
        }
        catch (Exception)
        {
            query.UserId = 0;
        }

        var result = await command.Execute(query);
        return resultHandler(result);
    }
}
