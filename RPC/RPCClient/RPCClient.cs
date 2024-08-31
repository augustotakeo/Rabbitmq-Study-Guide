using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class RPCClient : IDisposable
{
    private readonly string _queueName = "rpc_queue";
    private readonly string _replyToQueueName;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();

    public RPCClient()
    {
        var factory = new ConnectionFactory();
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            if (!_callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                return;

            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            tcs.TrySetResult(message);
        };

        _replyToQueueName = _channel.QueueDeclare().QueueName;

        _channel.BasicConsume(_replyToQueueName, autoAck: true, consumer);
    }

    public Task<string> CallAsync(string message, CancellationToken cancellationToken = default)
    {
        var props = _channel.CreateBasicProperties();
        props.ReplyTo = _replyToQueueName;
        props.CorrelationId = Guid.NewGuid().ToString();

        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(props.CorrelationId, tcs);

        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: string.Empty, _queueName, props, body);

        cancellationToken.Register(() => _callbackMapper.TryRemove(props.CorrelationId, out _));

        return tcs.Task;
    }

    public void Dispose()
    {
        _connection.Close();
    }
}