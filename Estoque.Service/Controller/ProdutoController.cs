using Microsoft.AspNetCore.Mvc;
using Estoque.Service.DTO;
using Estoque.Service.Interface;
using Estoque.Service.Modelo;
using Microsoft.AspNetCore.Authorization;

namespace Estoque.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProdutoController : ControllerBase
    {
        private readonly IProdutoService _service;

        public ProdutoController(IProdutoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var produtos = await _service.ObterTodosAsync();
            return Ok(produtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var produto = await _service.ObterPorIdAsync(id);
            if (produto == null)
                return NotFound(new { erro = "Produto não encontrado", codigo = "ESTOQUE_404" });

            return Ok(produto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProdutoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { erro = "Dados inválidos", codigo = "ESTOQUE_400" });

            if (dto.Preco <= 0)
                return BadRequest(new { erro = "Preço deve ser maior que zero", codigo = "ESTOQUE_401" });

            if (dto.Quantidade < 0)
                return BadRequest(new { erro = "Quantidade não pode ser negativa", codigo = "ESTOQUE_402" });

            var novoProduto = new Produto
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Preco = dto.Preco,
                Quantidade = dto.Quantidade
            };

            var criado = await _service.AdicionarAsync(novoProduto);
            return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] ProdutoPatchDto patch)
        {
            if (patch == null)
                return BadRequest(new { erro = "Payload inválido", codigo = "ESTOQUE_400" });

            var atualizado = await _service.PatchAsync(id, patch);
            if (atualizado == null)
                return NotFound(new { erro = "Produto não encontrado", codigo = "ESTOQUE_404" });

            return Ok(atualizado);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var removido = await _service.RemoverAsync(id);
            if (!removido)
                return NotFound(new { erro = "Produto não encontrado", codigo = "ESTOQUE_404" });

            return NoContent();
        }
    }
}
