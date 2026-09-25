using Denarius.Application.IO.Transactions;
using Denarius.Application.UseCases.Transactions.Create;
using Denarius.Application.UseCases.Transactions.Delete;
using Denarius.Application.UseCases.Transactions.GetById;
using Denarius.Application.UseCases.Transactions.List;
using Denarius.Application.UseCases.Transactions.Update;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController(ICreateTransactionUseCase createTransactionUseCase, IUpdateTransactionUseCase updateTransactionUseCase, IDeleteTransactionUseCase deleteTransactionUseCase, IListTransactionsUseCase listTransactionsUseCase, IGetTransactionByIdUseCase getTransactionByIdUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionOutput>>> List(
        [FromQuery] string? description,
        [FromQuery] DateTime? dateRef,
        [FromQuery] Guid? categoryId,
        [FromQuery] TransactionType type = TransactionType.All,
        [FromQuery] TransactionOrderField orderBy = TransactionOrderField.Date,
        [FromQuery] bool asc = false)
    {
        var output = await listTransactionsUseCase.Execute(new ListTransactionsInput(description, dateRef, type, categoryId, orderBy, asc));
        return Ok(output);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionOutput>> GetById(Guid id)
    {
        var output = await getTransactionByIdUseCase.Execute(id);
        return Ok(output);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionOutput>> Create([FromBody] CreateTransactionInput input)
    {
        var output = await createTransactionUseCase.Execute(input);
        return Created($"/api/transactions/{output.Id}", output);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TransactionOutput>> Update(Guid id, [FromBody] UpdateTransactionInput input)
    {
        var output = await updateTransactionUseCase.Execute((id, input));
        return Ok(output);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await deleteTransactionUseCase.Execute(id);
        return NoContent();
    }
}
