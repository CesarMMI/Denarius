using Denarius.Application.Transactions.Commands.Create;
using Denarius.Application.Transactions.Commands.Delete;
using Denarius.Application.Transactions.Commands.GetAll;
using Denarius.Application.Transactions.Commands.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class TransactionsController(
    ICreateTransactionCommand createTransactionCommand,
    IDeleteTransactionCommand deleteTransactionCommand,
    IGetAllTransactionsCommand getAllTransactionsCommand,
    IUpdateTransactionCommand updateTransactionCommand
) : Controller
{
    [HttpPost]
    [Authorize]
    public Task<IActionResult> Create([FromBody] CreateTransactionQuery body)
    {
        return HandleCommand(createTransactionCommand, body, account => Created(HttpContext.Request.Path + "/" + account.Id, account));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleCommand(deleteTransactionCommand, new() { Id = id }, _ => NoContent());
    }

    [HttpGet]
    [Authorize]
    public Task<IActionResult> GetAll()
    {
        return HandleCommand(getAllTransactionsCommand, new());
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateTransactionQuery body)
    {
        body.Id = id;
        return HandleCommand(updateTransactionCommand, body);
    }
}
