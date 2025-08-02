using Denarius.Application.Domain.Commands.Transactions;
using Denarius.Application.Domain.Queries.Transactions;
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
    IUpdateTransactionCommand updateTransactionCommand
) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public Task<IActionResult> GetAll() => getAllTransactionsCommand
        .Execute(new GetAllTransactionsQuery()
            .WithUserId(HttpContext))
        .Ok();

    [Authorize]
    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateTransactionQuery body) => createTransactionCommand
        .Execute(body
            .WithUserId(HttpContext))
        .Created();

    [Authorize]
    [HttpPut("{id:int}")]
    public Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateTransactionQuery body) => updateTransactionCommand
        .Execute(body
            .WithId(id)
            .WithUserId(HttpContext))
        .Ok();

    [Authorize]
    [HttpDelete("{id:int}")]
    public Task<IActionResult> Delete([FromRoute] int id) => deleteTransactionCommand
        .Execute(new DeleteTransactionQuery()
            .WithId(id)
            .WithUserId(HttpContext))
        .NoContent();
}
