using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Vendas.Service.DTO;
using Vendas.Service.Interface;
using Vendas.Service.Modelo;
using Vendas.Service.Servico;
using Xunit;

namespace Vendas.Tests
{
    public class PedidoServiceTests
    {
        private PedidoService CriarService(Mock<IRepositorioPedido> mockRepo, Mock<ILogger<PedidoService>>? mockLogger = null)
        {
            var mockHttpFactory = new Mock<IHttpClientFactory>();
            var mockPublisher = new Mock<IRabbitPublisher>();

            var client = new HttpClient(new FakeHttpMessageHandler())
            {
                BaseAddress = new Uri("http://localhost:5085")
            };

            mockHttpFactory.Setup(f => f.CreateClient("estoque"))
                           .Returns(client);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Servicos:Estrutura:EstoqueBaseUrl", "http://localhost:5085" }
                })
                .Build();

            return new PedidoService(
                mockRepo.Object,
                mockHttpFactory.Object,
                mockPublisher.Object,
                config,
                (mockLogger ?? new Mock<ILogger<PedidoService>>()).Object
            );
        }

        [Fact]
        public async Task CriarPedidoAsync_DeveLancarExcecao_QuandoSemItens_ERegistrarLog()
        {
            var dto = new PedidoCreateDto { Items = new List<PedidoItemDto>() };
            var mockRepo = new Mock<IRepositorioPedido>();
            var mockLogger = new Mock<ILogger<PedidoService>>();
            var service = CriarService(mockRepo, mockLogger);

            await Assert.ThrowsAsync<ArgumentException>(() => service.CriarPedidoAsync(dto, "fake-token"));

            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("sem itens")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CriarPedidoAsync_DeveCriarPedido_QuandoItensValidos()
        {
            var dto = new PedidoCreateDto
            {
                Items = new List<PedidoItemDto>
                {
                    new PedidoItemDto { ProdutoId = Guid.NewGuid(), Quantidade = 2 }
                }
            };

            var pedidoCriado = new Pedido
            {
                Id = Guid.NewGuid(),
                Status = StatusPedido.Confirmado,
                Items = new List<ItemPedido>
                {
                    new ItemPedido
                    {
                        Id = Guid.NewGuid(),
                        ProdutoId = dto.Items[0].ProdutoId,
                        Quantidade = 2,
                        PrecoUnitario = 100
                    }
                }
            };

            var mockRepo = new Mock<IRepositorioPedido>();
            mockRepo.Setup(r => r.AdicionarAsync(It.IsAny<Pedido>()))
                    .ReturnsAsync(pedidoCriado);

            var service = CriarService(mockRepo);

            var resultado = await service.CriarPedidoAsync(dto, "fake-token");

            Assert.NotNull(resultado);
            Assert.Equal(StatusPedido.Confirmado, resultado.Status);
            Assert.Single(resultado.Items);
            Assert.Equal(dto.Items[0].ProdutoId, resultado.Items[0].ProdutoId);
            mockRepo.Verify(r => r.AdicionarAsync(It.IsAny<Pedido>()), Times.Once);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarPedido()
        {
            var id = Guid.NewGuid();
            var pedido = new Pedido { Id = id, Status = StatusPedido.Confirmado, Items = new List<ItemPedido>() };

            var mockRepo = new Mock<IRepositorioPedido>();
            mockRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(pedido);

            var service = CriarService(mockRepo);

            var resultado = await service.ObterPorIdAsync(id);

            Assert.NotNull(resultado);
            Assert.Equal(id, resultado!.Id);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoExiste()
        {
            var id = Guid.NewGuid();
            var mockRepo = new Mock<IRepositorioPedido>();
            mockRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Pedido?)null);

            var service = CriarService(mockRepo);

            var resultado = await service.ObterPorIdAsync(id);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterTodosAsync_DeveRetornarListaDePedidos()
        {
            var pedidos = new List<Pedido>
            {
                new Pedido { Id = Guid.NewGuid(), Status = StatusPedido.Confirmado, Items = new List<ItemPedido>() },
                new Pedido { Id = Guid.NewGuid(), Status = StatusPedido.Pendente, Items = new List<ItemPedido>() }
            };

            var mockRepo = new Mock<IRepositorioPedido>();
            mockRepo.Setup(r => r.ObterTodosAsync()).ReturnsAsync(pedidos);

            var service = CriarService(mockRepo);

            var resultado = await service.ObterTodosAsync();

            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, p => p.Status == StatusPedido.Confirmado);
        }
    }
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"Id\":\"00000000-0000-0000-0000-000000000000\",\"Nome\":\"Fake\",\"Descricao\":null,\"Preco\":100,\"Quantidade\":10}")
            };
            return Task.FromResult(response);
        }
    }
}
