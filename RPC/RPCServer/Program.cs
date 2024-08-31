using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory();
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

var queue = "rpc_queue";

channel.QueueDeclare(queue, exclusive: false, autoDelete: false, durable: false, arguments: null);

channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var props = channel.CreateBasicProperties();
    props.CorrelationId = ea.BasicProperties.CorrelationId;

    var replyMessage = string.Empty;

    try
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        Console.WriteLine($"Received {message}");

        var n = int.Parse(message);
        var fib = Fibonacci.Calculate(n);
        replyMessage = fib.ToString();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[.] {ex.Message}");
        replyMessage = string.Empty;
    }
    finally
    {
        var replyBody = Encoding.UTF8.GetBytes(replyMessage);
        channel.BasicPublish(exchange: string.Empty, routingKey: ea.BasicProperties.ReplyTo, props, replyBody);
    }
};

channel.BasicConsume(queue, autoAck: false, consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();