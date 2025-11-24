using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Estoque.Service.Interface;
using Estoque.Service.Modelo;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Estoque.Service.Servico
{
    public class RabbitConsumerBackgroundService : BackgroundService
    {
        private readonly ILogger<RabbitConsumerBackgroundService> _logger;
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;

        private IConnection? _connection;
        private RabbitMQ.Client.IModel? _channel;

        private string _exchangeName;
        private string _queueName;

        public RabbitConsumerBackgroundService(
            ILogger<RabbitConsumerBackgroundService> logger,
            IConfiguration config,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _config = config;
            _scopeFactory = scopeFactory;

            _exchangeName = _config.GetValue<string>("RabbitMQ:Exchange") ?? "vendas";
            _queueName = _config.GetValue<string>("RabbitMQ:Queue") ?? "estoque.vendas";
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var host = _config.GetValue<string>("RabbitMQ:Host") ?? "localhost";
                var factory = new ConnectionFactory() { HostName = host };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Fanout, durable: true);
                _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: "");

                _logger.LogInformation("RabbitConsumer conectado: Host={Host}, Exchange={Exchange}, Queue={Queue}", host, _exchangeName, _queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar RabbitMQ consumer no StartAsync.");
            }

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null)
            {
                _logger.LogWarning("Canal RabbitMQ não inicializado — consumer inativo.");
                return Task.CompletedTask;
            }

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (sender, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var texto = Encoding.UTF8.GetString(body);
                    _logger.LogInformation("Mensagem recebida: {Raw}", texto);

                    var venda = JsonSerializer.Deserialize<MensagemVenda>(texto);
                    if (venda == null)
                    {
                        _logger.LogWarning("Mensagem inválida. Ack e ignora.");
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var repositorio = scope.ServiceProvider.GetRequiredService<IRepositorioProduto>();

                    var produto = await repositorio.ObterPorIdAsync(venda.ProdutoId);
                    if (produto == null)
                    {
                        _logger.LogWarning("Produto {ProdutoId} não encontrado. Ack e ignora.", venda.ProdutoId);
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    produto.Quantidade = Math.Max(0, produto.Quantidade - venda.Quantidade);
                    produto.AtualizadoEm = DateTime.UtcNow;

                    await repositorio.AtualizarAsync(produto);

                    _logger.LogInformation("Estoque atualizado: Produto {ProdutoId} => {Quantidade}", produto.Id, produto.Quantidade);

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem RabbitMQ. NACK sem requeue.");
                    try { _channel.BasicNack(ea.DeliveryTag, false, requeue: false); } catch { }
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            try { _channel?.Close(); } catch { }
            try { _connection?.Close(); } catch { }
            base.Dispose();
        }

        public record MensagemVenda(Guid ProdutoId, int Quantidade);
    }
}
