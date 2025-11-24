using System.ComponentModel.DataAnnotations;

namespace Estoque.Service.DTO
{
    public class ProdutoCreateDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string Nome { get; set; } = null!;

        [MaxLength(255)]
        public string? Descricao { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Preco { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantidade { get; set; }
    }
}
