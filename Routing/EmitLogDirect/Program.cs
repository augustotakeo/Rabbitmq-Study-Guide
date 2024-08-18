using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory();
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

var exchange = "direct_logs";
channel.ExchangeDeclare(exchange, ExchangeType.Direct);

var severity = args.Length > 0 ? args[0] : "info";
var message = args.Length > 1 ? string.Join(" ", args.Skip(1)) : "Hello World";
var body = Encoding.UTF8.GetBytes(message);

channel.BasicPublish(exchange, routingKey: severity, null, body);

Console.WriteLine($"[x] Sent {severity}:{message}");
Console.ReadLine();