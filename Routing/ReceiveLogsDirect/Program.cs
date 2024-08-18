using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory();
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

var exchange = "direct_logs";

channel.ExchangeDeclare(exchange, ExchangeType.Direct);

var queue = channel.QueueDeclare().QueueName;

if(args.Length == 0) {
    Console.Error.WriteLine("Usage: {0} [info] [warning] [error]",
                            Environment.GetCommandLineArgs()[0]);
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine(" [*] Waiting for logs.");

foreach(var severity in args) {
    channel.QueueBind(queue, exchange, severity);
}

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) => {
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var severity = ea.RoutingKey;
    Console.WriteLine($"Received {severity}:{message}");
};

channel.BasicConsume(queue, autoAck: true, consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();