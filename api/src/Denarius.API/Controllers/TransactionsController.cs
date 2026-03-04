using Denarius.Application.Commands.Transactions;
using Denarius.Application.Interfaces.Transactions;
using Denarius.Application.Results;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.API.Controllers
{
    [ApiController]
    [Route("transactions")]
    public class TransactionsController(
        ICreateTransactionUseCase createTransactionUseCase,
        IDeleteTransactionUseCase deleteTransactionUseCase,
        IGetAllTransactionsUseCase getAllTransactionsUseCase,
        IUpdateTransactionUseCase updateTransactionUseCase) : ControllerBase
    {

        [HttpGet]
        public IEnumerable<TransactionResult> GetAll() =>
            getAllTransactionsUseCase.Execute(new());

        [HttpPost]
        public Task<TransactionResult> Create([FromBody] CreateTransactionCommand command) =>
            createTransactionUseCase.Execute(command);

        [HttpPut("{id:Guid}")]
        public Task<TransactionResult> Update(Guid id, [FromBody] UpdateTransactionCommand command) =>
            updateTransactionUseCase.Execute(command with { Id = id });

        [HttpDelete("{id:Guid}")]
        public Task<TransactionResult> Delete(Guid id) =>
            deleteTransactionUseCase.Execute(new() { Id = id });
    }
}
