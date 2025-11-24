using System.Net.Http.Headers;
using System.Net.Http.Json;
using Vendas.Service.DTO;
using Vendas.Service.Interface;
using Vendas.Service.Modelo;
using Microsoft.Extensions.Logging;

namespace Vendas.Service.Servico
{
    public class PedidoService
    {
        private readonly IRepositorioPedido _repositorio;
        private readonly HttpClient _http;
        private readonly IRabbitPublisher _publisher;
        private readonly string _estoqueBaseUrl;
        private readonly ILogger<PedidoService> _logger;

        public PedidoService(
            IRepositorioPedido repositorio,
            IHttpClientFactory httpFactory,
            IRabbitPublisher publisher,
            IConfiguration config,
            ILogger<PedidoService> logger)
        {
            _repositorio = repositorio;
            _http = httpFactory.CreateClient("estoque");
            _publisher = publisher;
            _estoqueBaseUrl = config.GetValue<string>("Servicos:Estrutura:EstoqueBaseUrl") ?? "http://localhost:5085";
            _logger = logger;
        }

        public async Task<Pedido> CriarPedidoAsync(PedidoCreateDto dto, string token)
        {
            if (dto.Items == null || !dto.Items.Any())
            {
                _logger.LogWarning("Tentativa de criar pedido sem itens.");
                throw new ArgumentException("Pedido sem itens.");
            }

            var itens = new List<ItemPedido>();

            foreach (var item in dto.Items)
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var resp = await _http.GetAsync($"/api/produto/{item.ProdutoId:D}");
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("Produto {ProdutoId} não encontrado no estoque.", item.ProdutoId);
                    throw new InvalidOperationException($"Produto {item.ProdutoId} não encontrado no estoque.");
                }

                var produto = await resp.Content.ReadFromJsonAsync<EstoqueProdutoDto>();
                if (produto == null)
                {
                    _logger.LogError("Erro ao ler produto {ProdutoId} do estoque.", item.ProdutoId);
                    throw new InvalidOperationException("Erro ao ler produto do estoque.");
                }

                if (produto.Quantidade < item.Quantidade)
                {
                    _logger.LogWarning("Estoque insuficiente para produto {ProdutoId}.", produto.Id);
                    throw new InvalidOperationException($"Estoque insuficiente para o produto {produto.Id}");
                }

                itens.Add(new ItemPedido
                {
                    Id = Guid.NewGuid(),
                    ProdutoId = produto.Id,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = produto.Preco
                });
            }

            var pedido = new Pedido
            {
                Status = StatusPedido.Confirmado,
                Items = itens
            };

            var criado = await _repositorio.AdicionarAsync(pedido);

            _logger.LogInformation("Pedido {PedidoId} criado com {QtdItens} itens.", criado.Id, criado.Items.Count);

            foreach (var item in criado.Items)
            {
                _publisher.PublishVendaConfirmada(item.ProdutoId, item.Quantidade);
                _logger.LogInformation("Venda confirmada publicada para produto {ProdutoId}, quantidade {Qtd}.", item.ProdutoId, item.Quantidade);
            }

            return criado;
        }

        public async Task<Pedido?> ObterPorIdAsync(Guid id)
        {
            var pedido = await _repositorio.ObterPorIdAsync(id);
            _logger.LogInformation("Consulta de pedido {PedidoId}: {Resultado}", id, pedido is null ? "não encontrado" : "encontrado");
            return pedido;
        }

        public async Task<List<Pedido>> ObterTodosAsync()
        {
            var pedidos = await _repositorio.ObterTodosAsync();
            _logger.LogInformation("Consulta de todos os pedidos retornou {Qtd} registros.", pedidos.Count);
            return pedidos;
        }
    }

    public record EstoqueProdutoDto(Guid Id, string Nome, string? Descricao, decimal Preco, int Quantidade);
}
