using Microsoft.AspNetCore.Mvc;
using Vendas.Service.DTO;
using Vendas.Service.Servico;
using Vendas.Service.Interface;
using Microsoft.AspNetCore.Authorization;

namespace Vendas.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PedidoController : ControllerBase
    {
        private readonly PedidoService _service;
        private readonly IRabbitPublisher _publisher;

        public PedidoController(PedidoService service, IRabbitPublisher publisher)
        {
            _service = service;
            _publisher = publisher;
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] PedidoCreateDto dto)
        {
            try
            {
                if (dto.Items == null || !dto.Items.Any())
                    return BadRequest(new { erro = "Pedido sem itens", codigo = "VENDAS_400" });

                var token = HttpContext.Request.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

                var criado = await _service.CriarPedidoAsync(dto, token);

                foreach (var item in dto.Items)
                    _publisher.PublishVendaConfirmada(item.ProdutoId, item.Quantidade);

                return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message, codigo = "VENDAS_401" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Erro interno no servidor", detalhe = ex.Message, codigo = "VENDAS_500" });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var p = await _service.ObterPorIdAsync(id);
            if (p is null)
                return NotFound(new { erro = "Pedido não encontrado", codigo = "VENDAS_404" });

            return Ok(p);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.ObterTodosAsync());
    }
}
