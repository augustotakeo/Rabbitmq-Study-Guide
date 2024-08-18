using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory();
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

var logExchange = "logs";
channel.ExchangeDeclare(logExchange, ExchangeType.Fanout);

var message = GetMessage();
var body = Encoding.UTF8.GetBytes(message);
channel.BasicPublish(logExchange, routingKey: string.Empty, null, body: body);

string GetMessage() {
    return args.Length > 0 ? string.Join(" ", args) : "Hello Pub Sub";
}
