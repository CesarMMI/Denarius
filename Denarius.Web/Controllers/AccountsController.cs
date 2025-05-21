using Denarius.Application.Accounts.Commands.Create;
using Denarius.Application.Accounts.Commands.Delete;
using Denarius.Application.Accounts.Commands.GetAll;
using Denarius.Application.Accounts.Commands.GetById;
using Denarius.Application.Accounts.Commands.Update;
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
    IGetAccountByIdCommand getAccountByIdCommand,
    IUpdateAccountCommand updateAccountCommand
) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateAccountQuery body)
    {
        body.SetRequestUserId(HttpContext);
        var account = await createAccountCommand.Execute(body);
        return Created(HttpContext.Request.Path + "/" + account.Id, account);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var body = new DeleteAccountQuery { Id = id };
        body.SetRequestUserId(HttpContext);
        await deleteAccountCommand.Execute(body);
        return NoContent();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var body = new GetAllAccountsQuery();
        body.SetRequestUserId(HttpContext);
        return Ok(await getAllAccountsCommand.Execute(body));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var body = new GetAccountByIdQuery { Id = id };
        body.SetRequestUserId(HttpContext);
        return Ok(await getAccountByIdCommand.Execute(body));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateAccountQuery body)
    {
        body.Id = id;
        body.SetRequestUserId(HttpContext);
        return Ok(await updateAccountCommand.Execute(body));
    }
}
