namespace Vendas.Service.Interface
{
    public interface IRabbitPublisher
    {
        void PublishVendaConfirmada(Guid produtoId, int quantidade);
    }
}
