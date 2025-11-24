namespace Vendas.Service.Modelo
{
    public enum StatusPedido { Pendente, Confirmado, Cancelado }

    public class Pedido
    {
        public Guid Id { get; set; }
        public DateTime CriadoEm { get; set; }
        public StatusPedido Status { get; set; }
        public List<ItemPedido> Items { get; set; } = new();

        // totals helper
        public decimal Total => Items.Sum(i => i.PrecoUnitario * i.Quantidade);
    }
}
