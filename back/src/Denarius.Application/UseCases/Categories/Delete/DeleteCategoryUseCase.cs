using Denarius.Application.Exceptions;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Categories.Delete;

internal class DeleteCategoryUseCase(ICategoryRepository categoryRepository, ITransactionRepository transactionRepository, IUnitOfWork unitOfWork) : IDeleteCategoryUseCase
{
    public async Task Execute(Guid input)
    {
        var category = await categoryRepository.GetByIdAsync(input)
            ?? throw new NotFoundException("Categoria não encontrada.");

        if (await transactionRepository.ExistsByCategoryIdAsync(input))
            throw new DomainException("Não é possível excluir uma categoria que possui transações associadas.");

        await categoryRepository.DeleteAsync(category);
        await unitOfWork.SaveChangesAsync();
    }
}
