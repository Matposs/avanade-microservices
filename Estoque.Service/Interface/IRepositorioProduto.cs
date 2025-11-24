using Estoque.Service.Modelo;
namespace Estoque.Service.Interface
{
    public interface IRepositorioProduto
    {
        Task<Produto?> ObterPorIdAsync(Guid id);
        Task<List<Produto>> ObterTodosAsync();
        Task AdicionarAsync(Produto produto);
        Task AtualizarAsync(Produto produto);
        Task RemoverAsync(Guid id);
    }
}