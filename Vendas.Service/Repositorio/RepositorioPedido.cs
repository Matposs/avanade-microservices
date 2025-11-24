using Microsoft.EntityFrameworkCore;
using Vendas.Service.Infra;
using Vendas.Service.Interface;
using Vendas.Service.Modelo;

namespace Vendas.Service.Repositorio
{
    public class RepositorioPedido : IRepositorioPedido
    {
        private readonly VendasDbContext _ctx;
        public RepositorioPedido(VendasDbContext ctx) => _ctx = ctx;

        public async Task<Pedido> AdicionarAsync(Pedido pedido)
        {
            pedido.Id = Guid.NewGuid();
            pedido.CriadoEm = DateTime.UtcNow;
            await _ctx.Pedidos.AddAsync(pedido);
            await _ctx.SaveChangesAsync();
            return pedido;
        }

        public async Task<Pedido?> ObterPorIdAsync(Guid id) =>
            await _ctx.Pedidos.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<List<Pedido>> ObterTodosAsync() =>
            await _ctx.Pedidos.Include(p => p.Items).AsNoTracking().ToListAsync();

        public async Task AtualizarAsync(Pedido pedido)
        {
            _ctx.Pedidos.Update(pedido);
            await _ctx.SaveChangesAsync();
        }
    }
}
