using Denarius.Application.Transactions.Commands.Create;
using Denarius.Application.Transactions.Commands.Delete;
using Denarius.Application.Transactions.Commands.GetAll;
using Denarius.Application.Transactions.Commands.GetById;
using Denarius.Application.Transactions.Commands.Update;
using Denarius.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class TransactionsController(
    ICreateTransactionCommand createTransactionCommand,
    IDeleteTransactionCommand deleteTransactionCommand,
    IGetAllTransactionsCommand getAllTransactionsCommand,
    IGetTransactionByIdCommand getTransactionByIdCommand,
    IUpdateTransactionCommand updateTransactionCommand
) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateTransactionQuery body)
    {
        body.SetRequestUserId(HttpContext);
        var account = await createTransactionCommand.Execute(body);
        return Created(HttpContext.Request.Path + "/" + account.Id, account);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var body = new DeleteTransactionQuery { Id = id };
        body.SetRequestUserId(HttpContext);
        await deleteTransactionCommand.Execute(body);
        return NoContent();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var body = new GetAllTransactionsQuery();
        body.SetRequestUserId(HttpContext);
        return Ok(await getAllTransactionsCommand.Execute(body));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var body = new GetTransactionByIdQuery { Id = id };
        body.SetRequestUserId(HttpContext);
        return Ok(await getTransactionByIdCommand.Execute(body));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateTransactionQuery body)
    {
        body.Id = id;
        body.SetRequestUserId(HttpContext);
        return Ok(await updateTransactionCommand.Execute(body));
    }
}
