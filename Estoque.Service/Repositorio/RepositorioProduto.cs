using Microsoft.EntityFrameworkCore;
using Estoque.Service.Modelo;
using Estoque.Service.Infra;
using Estoque.Service.Interface;

namespace Estoque.Service.Repositorio
{
    public class RepositorioProduto : IRepositorioProduto
    {
        private readonly EstoqueDbContext _contexto;

        public RepositorioProduto(EstoqueDbContext contexto)
        {
            _contexto = contexto;
        }

        public async Task AdicionarAsync(Produto produto)
        {
            produto.Id = Guid.NewGuid();
            produto.CriadoEm = DateTime.UtcNow;
            await _contexto.Produtos.AddAsync(produto);
            await _contexto.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Produto produto)
        {
            produto.AtualizadoEm = DateTime.UtcNow;
            _contexto.Produtos.Update(produto);
            await _contexto.SaveChangesAsync();
        }

        public async Task<Produto?> ObterPorIdAsync(Guid id)
        {
            return await _contexto.Produtos.FindAsync(id);
        }

        public async Task<List<Produto>> ObterTodosAsync()
        {
            return await _contexto.Produtos.AsNoTracking().ToListAsync();
        }

        public async Task RemoverAsync(Guid id)
        {
            var produto = await _contexto.Produtos.FindAsync(id);
            if (produto is null) return;
            _contexto.Produtos.Remove(produto);
            await _contexto.SaveChangesAsync();
        }
    }
}
