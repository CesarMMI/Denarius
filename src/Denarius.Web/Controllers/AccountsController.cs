using Denarius.Application.Domain.Commands.Accounts;
using Denarius.Application.Domain.Queries.Accounts;
using Denarius.Web.Extensions;
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
) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public Task<IActionResult> GetAll() => getAllAccountsCommand
        .Execute(new GetAllAccountsQuery()
            .WithUserId(HttpContext))
        .Ok();

    [Authorize]
    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateAccountQuery body) => createAccountCommand
        .Execute(body
            .WithUserId(HttpContext))
        .Created();

    [Authorize]
    [HttpPut("{id:int}")]
    public Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateAccountQuery body) => updateAccountCommand
       .Execute(body
           .WithId(id)
           .WithUserId(HttpContext))
       .Ok();

    [Authorize]
    [HttpDelete("{id:int}")]
    public Task<IActionResult> Delete([FromRoute] int id) => deleteAccountCommand
        .Execute(new DeleteAccountQuery()
            .WithId(id)
            .WithUserId(HttpContext))
        .NoContent();    
}
