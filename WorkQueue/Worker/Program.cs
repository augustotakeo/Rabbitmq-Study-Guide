using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// await Task.Delay(60000);

var factory = new ConnectionFactory { HostName = "localhost" };
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

var queueName = "hello";

channel.QueueDeclare(queue: queueName,
                     autoDelete: false,
                     exclusive: false,
                     durable: false,
                     arguments: null);

channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += async (model, ea) => 
{
    var body = ea.Body.ToArray();
    var message = System.Text.Encoding.UTF8.GetString(body);
    Console.WriteLine($"[x] Received {message}");

    var dots = message.Split(".").Length - 1;
    await Task.Delay(dots*1000);

    Console.WriteLine("[x] Done");

    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
};

channel.BasicConsume(queue: queueName,
                     autoAck: false,
                     consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();