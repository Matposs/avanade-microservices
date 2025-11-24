using Estoque.Service.DTO;
using Estoque.Service.Modelo;

namespace Estoque.Service.Interface
{
    public interface IProdutoService
    {
        Task<List<Produto>> ObterTodosAsync();
        Task<Produto?> ObterPorIdAsync(Guid id);
        Task<Produto> AdicionarAsync(Produto produto);
        Task<bool> RemoverAsync(Guid id);
        Task<Produto?> PatchAsync(Guid id, ProdutoPatchDto patch);
    }
}
