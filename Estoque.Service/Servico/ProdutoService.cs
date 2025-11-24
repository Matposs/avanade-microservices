using Estoque.Service.Interface;
using Estoque.Service.Modelo;
using Estoque.Service.DTO;
using Microsoft.Extensions.Logging;

namespace Estoque.Service.Servico
{
    public class ProdutoService : IProdutoService
    {
        private readonly IRepositorioProduto _repositorio;
        private readonly ILogger<ProdutoService> _logger;

        public ProdutoService(IRepositorioProduto repositorio, ILogger<ProdutoService> logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        public async Task<List<Produto>> ObterTodosAsync()
        {
            var produtos = await _repositorio.ObterTodosAsync();
            _logger.LogInformation("Consulta de todos os produtos retornou {Qtd} registros.", produtos.Count);
            return produtos;
        }

        public async Task<Produto?> ObterPorIdAsync(Guid id)
        {
            var produto = await _repositorio.ObterPorIdAsync(id);
            _logger.LogInformation("Consulta de produto {ProdutoId}: {Resultado}", id, produto is null ? "não encontrado" : "encontrado");
            return produto;
        }

        public async Task<Produto> AdicionarAsync(Produto produto)
        {
            if (string.IsNullOrWhiteSpace(produto.Nome))
            {
                _logger.LogWarning("Tentativa de adicionar produto com nome inválido.");
                throw new ArgumentException("Nome obrigatório.");
            }

            if (produto.Preco <= 0)
            {
                _logger.LogWarning("Tentativa de adicionar produto com preço inválido.");
                throw new ArgumentException("Preço deve ser maior que zero.");
            }

            if (produto.Quantidade < 0)
            {
                _logger.LogWarning("Tentativa de adicionar produto com quantidade negativa.");
                throw new ArgumentException("Quantidade não pode ser negativa.");
            }

            produto.Id = Guid.NewGuid();
            produto.CriadoEm = DateTime.UtcNow;

            await _repositorio.AdicionarAsync(produto);
            _logger.LogInformation("Produto {ProdutoId} adicionado com sucesso.", produto.Id);

            return produto;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var existente = await _repositorio.ObterPorIdAsync(id);
            if (existente is null)
            {
                _logger.LogWarning("Tentativa de remover produto {ProdutoId} que não existe.", id);
                return false;
            }

            await _repositorio.RemoverAsync(id);
            _logger.LogInformation("Produto {ProdutoId} removido com sucesso.", id);
            return true;
        }

        public async Task<Produto?> PatchAsync(Guid id, ProdutoPatchDto patch)
        {
            var existente = await _repositorio.ObterPorIdAsync(id);
            if (existente is null)
            {
                _logger.LogWarning("Tentativa de atualizar produto {ProdutoId} que não existe.", id);
                return null;
            }

            if (patch.Nome is not null)
            {
                if (string.IsNullOrWhiteSpace(patch.Nome))
                {
                    _logger.LogWarning("Tentativa de atualizar produto {ProdutoId} com nome inválido.", id);
                    throw new ArgumentException("Nome inválido.");
                }
                existente.Nome = patch.Nome;
            }

            if (patch.Descricao is not null)
                existente.Descricao = patch.Descricao;

            if (patch.Preco is not null)
            {
                if (patch.Preco <= 0)
                {
                    _logger.LogWarning("Tentativa de atualizar produto {ProdutoId} com preço inválido.", id);
                    throw new ArgumentException("Preço inválido.");
                }
                existente.Preco = patch.Preco.Value;
            }

            if (patch.Quantidade is not null)
            {
                if (patch.Quantidade < 0)
                {
                    _logger.LogWarning("Tentativa de atualizar produto {ProdutoId} com quantidade negativa.", id);
                    throw new ArgumentException("Quantidade não pode ser negativa.");
                }
                existente.Quantidade = patch.Quantidade.Value;
            }

            existente.AtualizadoEm = DateTime.UtcNow;

            await _repositorio.AtualizarAsync(existente);
            _logger.LogInformation("Produto {ProdutoId} atualizado com sucesso.", id);

            return existente;
        }
    }
}
