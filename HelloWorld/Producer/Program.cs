using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost"  };
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

var queueName = "hello";

channel.QueueDeclare(queue: queueName,
                     durable: false,
                     autoDelete: false,
                     exclusive: false,
                     arguments: null);

var message = "Hello World";
var body = System.Text.Encoding.UTF8.GetBytes(message);

channel.BasicPublish(exchange: string.Empty,
                     routingKey: queueName,
                     basicProperties: null,
                     body: body);

Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
