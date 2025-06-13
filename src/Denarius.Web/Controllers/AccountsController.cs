using Denarius.Application.Accounts.Commands.Create;
using Denarius.Application.Accounts.Commands.Delete;
using Denarius.Application.Accounts.Commands.GetAll;
using Denarius.Application.Accounts.Commands.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class AccountsController(
    ICreateAccountCommand createAccountCommand,
    IDeleteAccountCommand deleteAccountCommand,
    IGetAllAccountsCommand getAllAccountsCommand,
    IUpdateAccountCommand updateAccountCommand
) : Controller
{
    [HttpPost]
    [Authorize]
    public Task<IActionResult> Create([FromBody] CreateAccountQuery body)
    {
        return HandleCommand(createAccountCommand, body, account => Created(HttpContext.Request.Path + "/" + account.Id, account));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleCommand(deleteAccountCommand, new() { Id = id }, _ => NoContent());
    }

    [HttpGet]
    [Authorize]
    public Task<IActionResult> GetAll()
    {
        return HandleCommand(getAllAccountsCommand, new());
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateAccountQuery body)
    {
        body.Id = id;
        return HandleCommand(updateAccountCommand, body);
    }
}
