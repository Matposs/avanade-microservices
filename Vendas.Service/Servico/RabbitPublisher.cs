using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Vendas.Service.Interface;

namespace Vendas.Service.Servico
{
    public class RabbitPublisher : IRabbitPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchange;

        public RabbitPublisher(string host, string exchange = "vendas")
        {
            _exchange = exchange;
            var factory = new ConnectionFactory() { HostName = host };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Fanout, durable: true);
        }

        public void PublishVendaConfirmada(Guid produtoId, int quantidade)
        {
            var message = JsonSerializer.Serialize(new { ProdutoId = produtoId, Quantidade = quantidade });
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: _exchange, routingKey: "", basicProperties: null, body: body);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
