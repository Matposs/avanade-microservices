namespace Estoque.Service.DTO
{
    public class ProdutoPatchDto
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal? Preco { get; set; }
        public int? Quantidade { get; set; }
    }
}
