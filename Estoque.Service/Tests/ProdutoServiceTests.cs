using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Estoque.Service.Interface;
using Estoque.Service.Modelo;
using Estoque.Service.Servico;
using Estoque.Service.DTO;
using Microsoft.Extensions.Logging;

namespace Estoque.Tests
{
    public class ProdutoServiceTests
    {
        private ProdutoService CriarService(Mock<IRepositorioProduto> mockRepo, Mock<ILogger<ProdutoService>>? mockLogger = null)
        {
            return new ProdutoService(mockRepo.Object, (mockLogger ?? new Mock<ILogger<ProdutoService>>()).Object);
        }

        [Fact]
        public async Task AdicionarAsync_DeveCriarProduto_QuandoDadosValidos()
        {
            var produto = new Produto { Nome = "Notebook", Preco = 3500, Quantidade = 10 };
            var mockRepo = new Mock<IRepositorioProduto>();
            mockRepo.Setup(r => r.AdicionarAsync(It.IsAny<Produto>())).Returns(Task.CompletedTask);

            var service = CriarService(mockRepo);

            var resultado = await service.AdicionarAsync(produto);

            Assert.NotEqual(Guid.Empty, resultado.Id);
            Assert.Equal("Notebook", resultado.Nome);
            mockRepo.Verify(r => r.AdicionarAsync(It.IsAny<Produto>()), Times.Once);
        }

        [Fact]
        public async Task AdicionarAsync_DeveLancarExcecao_QuandoPrecoInvalido_ERegistrarLog()
        {
            var produto = new Produto { Nome = "Mouse", Preco = 0, Quantidade = 5 };
            var mockRepo = new Mock<IRepositorioProduto>();
            var mockLogger = new Mock<ILogger<ProdutoService>>();
            var service = CriarService(mockRepo, mockLogger);

            await Assert.ThrowsAsync<ArgumentException>(() => service.AdicionarAsync(produto));

            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("preço inválido")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RemoverAsync_DeveRetornarTrue_QuandoProdutoExiste()
        {
            var id = Guid.NewGuid();
            var produto = new Produto { Id = id, Nome = "Teclado", Preco = 200, Quantidade = 5 };

            var mockRepo = new Mock<IRepositorioProduto>();
            mockRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(produto);
            mockRepo.Setup(r => r.RemoverAsync(id)).Returns(Task.CompletedTask);

            var service = CriarService(mockRepo);

            var resultado = await service.RemoverAsync(id);

            Assert.True(resultado);
            mockRepo.Verify(r => r.RemoverAsync(id), Times.Once);
        }

        [Fact]
        public async Task RemoverAsync_DeveRetornarFalse_QuandoProdutoNaoExiste()
        {
            var id = Guid.NewGuid();
            var mockRepo = new Mock<IRepositorioProduto>();
            mockRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Produto?)null);

            var service = CriarService(mockRepo);

            var resultado = await service.RemoverAsync(id);

            Assert.False(resultado);
        }

        [Fact]
        public async Task PatchAsync_DeveAtualizarNomeEPreco()
        {
            var id = Guid.NewGuid();
            var produto = new Produto { Id = id, Nome = "Monitor", Preco = 800, Quantidade = 5 };
            var patch = new ProdutoPatchDto { Nome = "Monitor Gamer", Preco = 1200 };

            var mockRepo = new Mock<IRepositorioProduto>();
            mockRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(produto);
            mockRepo.Setup(r => r.AtualizarAsync(It.IsAny<Produto>())).Returns(Task.CompletedTask);

            var service = CriarService(mockRepo);

            var atualizado = await service.PatchAsync(id, patch);

            Assert.NotNull(atualizado);
            Assert.Equal("Monitor Gamer", atualizado!.Nome);
            Assert.Equal(1200, atualizado.Preco);
            mockRepo.Verify(r => r.AtualizarAsync(It.IsAny<Produto>()), Times.Once);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarProduto_QuandoExiste()
        {
            var id = Guid.NewGuid();
            var produto = new Produto { Id = id, Nome = "Cadeira Gamer", Preco = 900, Quantidade = 2 };

            var mockRepo = new Mock<IRepositorioProduto>();
            mockRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(produto);

            var service = CriarService(mockRepo);

            var resultado = await service.ObterPorIdAsync(id);

            Assert.NotNull(resultado);
            Assert.Equal("Cadeira Gamer", resultado!.Nome);
        }

        [Fact]
        public async Task ObterTodosAsync_DeveRetornarListaDeProdutos()
        {
            var produtos = new List<Produto>
            {
                new Produto { Id = Guid.NewGuid(), Nome = "Notebook", Preco = 3500 },
                new Produto { Id = Guid.NewGuid(), Nome = "Mouse", Preco = 100 }
            };

            var mockRepo = new Mock<IRepositorioProduto>();
            mockRepo.Setup(r => r.ObterTodosAsync()).ReturnsAsync(produtos);

            var service = CriarService(mockRepo);

            var resultado = await service.ObterTodosAsync();

            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, p => p.Nome == "Notebook");
        }
    }
}
