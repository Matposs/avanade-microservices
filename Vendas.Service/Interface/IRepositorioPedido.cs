using Vendas.Service.Modelo;

namespace Vendas.Service.Interface
{
    public interface IRepositorioPedido
    {
        Task<Pedido> AdicionarAsync(Pedido pedido);
        Task<Pedido?> ObterPorIdAsync(Guid id);
        Task<List<Pedido>> ObterTodosAsync();
        Task AtualizarAsync(Pedido pedido);
    }
}
