using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory();
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

var logExchange = "logs";
channel.ExchangeDeclare(logExchange, ExchangeType.Fanout);

var queue = channel.QueueDeclare().QueueName;

channel.QueueBind(queue, logExchange, routingKey: string.Empty);

Console.WriteLine(" [*] Waiting for logs.");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) => {
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine(message);
};

channel.BasicConsume(queue, autoAck: true, consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();