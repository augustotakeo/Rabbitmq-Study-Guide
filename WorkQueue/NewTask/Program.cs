using System.Text;
using RabbitMQ.Client;

// await Task.Delay(60000);

var factory = new ConnectionFactory { HostName = "localhost"  };
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

var queueName = "hello";

channel.QueueDeclare(queue: queueName,
                     durable: true,
                     autoDelete: false,
                     exclusive: false,
                     arguments: null);

var message = GetMessage(args);
var body = System.Text.Encoding.UTF8.GetBytes(message);

var properties = channel.CreateBasicProperties();
properties.Persistent = true;

channel.BasicPublish(exchange: string.Empty,
                     routingKey: queueName,
                     basicProperties: properties,
                     body: body);

Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

static string GetMessage(string[] args) {
    return args.Length > 0 ? string.Join(" ", args) : "Hello World";
}