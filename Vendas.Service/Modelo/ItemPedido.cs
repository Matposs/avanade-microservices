namespace Vendas.Service.Modelo
{
    public class ItemPedido
    {
        public Guid Id { get; set; }
        public Guid ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; } // snapshot do preço
    }
}
