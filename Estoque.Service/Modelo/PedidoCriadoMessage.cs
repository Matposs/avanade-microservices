namespace Estoque.Domain.Modelo
{
    public class PedidoCriadoMessage
    {
        public int PedidoId { get; set; }
        public List<PedidoItemMessage> Itens { get; set; } = new();
    }

    public class PedidoItemMessage
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }
}
