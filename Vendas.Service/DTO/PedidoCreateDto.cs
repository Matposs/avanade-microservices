using System.ComponentModel.DataAnnotations;

namespace Vendas.Service.DTO
{
    public class PedidoItemDto
    {
        [Required] public Guid ProdutoId { get; set; }
        [Range(1, int.MaxValue)] public int Quantidade { get; set; }
    }

    public class PedidoCreateDto
    {
        [Required]
        public List<PedidoItemDto> Items { get; set; } = new();
    }
}
