using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory();
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

var queue = "publish_confirms";

channel.QueueDeclare(queue, exclusive: false, autoDelete: false, durable: false);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Message received: {message}");
};

channel.BasicConsume(queue, autoAck: true, consumer);

Console.WriteLine("[x] Press [enter] to exit.");
Console.ReadLine();