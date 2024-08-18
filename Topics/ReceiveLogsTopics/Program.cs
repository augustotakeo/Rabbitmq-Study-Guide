using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory();
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

var exchange = "topic_log";

channel.ExchangeDeclare(exchange, ExchangeType.Topic);

var queue = channel.QueueDeclare().QueueName;

if(args.Length == 0) {
    Console.Error.WriteLine("Usage: {0} [routingKey...]",
                            Environment.GetCommandLineArgs()[0]);
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
    Environment.ExitCode = 1;
    return;
}

foreach(var routingKey in args) {
    Console.WriteLine(routingKey);
    channel.QueueBind(queue, exchange, routingKey);
}

Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) => {
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var routingKey = ea.RoutingKey;
    Console.WriteLine($"[x] Received {routingKey}:{message}");
};

channel.BasicConsume(queue, autoAck: true, consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();