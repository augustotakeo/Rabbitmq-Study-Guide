using RabbitMQ.Client;
using System.Text;
using System.Collections.Concurrent;
using System.Diagnostics;

public class PublisherConfirms
{
    private readonly IConnection _connection;
    private const int MESSAGES_COUNT = 10000;
    private const int BATCH_SIZE = 100;
    private const string QUEUE = "publish_confirms";

    public PublisherConfirms()
    {
        var factory = new ConnectionFactory();
        _connection = factory.CreateConnection();
        var channel = _connection.CreateModel();
        channel.QueueDeclare(QUEUE, exclusive: false, autoDelete: false, durable: false);
    }

    public void PublishMessagesIndividually()
    {
        using var channel = CreateConfirmedSelectChannel();

        var startTime = Stopwatch.GetTimestamp();

        for (int i = 0; i < MESSAGES_COUNT; i++)
        {
            var message = i.ToString();
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: string.Empty, QUEUE, null, body);
            channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(30));
        }

        var endTime = Stopwatch.GetTimestamp();

        Console.WriteLine($"Published {MESSAGES_COUNT:N0} messages individually in {Stopwatch.GetElapsedTime(startTime, endTime).TotalMilliseconds:N0} ms");

        channel.Close();
    }

    public void PublishMessagesInBatch()
    {
        using var channel = CreateConfirmedSelectChannel();

        var startTime = Stopwatch.GetTimestamp();

        var numberOfSentMessages = 0;
        for (int i = 0; i < MESSAGES_COUNT; i++)
        {
            var message = i.ToString();
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: string.Empty, QUEUE, null, body);
            numberOfSentMessages++;

            if (numberOfSentMessages >= BATCH_SIZE)
            {
                channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(30));
                numberOfSentMessages = 0;
            }
        }

        if (numberOfSentMessages > 0)
            channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(30));

        var endTime = Stopwatch.GetTimestamp();

        Console.WriteLine($"Published {MESSAGES_COUNT:N0} messages in batches in {Stopwatch.GetElapsedTime(startTime, endTime).TotalMilliseconds:N0} ms");

        channel.Close();
    }

    public async Task HandlePublishConfirmsAsynchronously()
    {
        var channel = CreateConfirmedSelectChannel();

        var outstandingConfirms = new ConcurrentDictionary<ulong, string>();

        channel.BasicAcks += (sender, ea) => CleanOutstandingConfirms(ea.Multiple, ea.DeliveryTag);

        channel.BasicNacks += (sender, ea) =>
        {
            if (outstandingConfirms.TryGetValue(ea.DeliveryTag, out var message))
                Console.WriteLine($"Nacked message: {message}");
            CleanOutstandingConfirms(ea.Multiple, ea.DeliveryTag);
        };

        var startTime = Stopwatch.GetTimestamp();

        for (int i = 0; i < MESSAGES_COUNT; i++)
        {
            var message = i.ToString();
            outstandingConfirms.TryAdd(channel.NextPublishSeqNo, message);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: string.Empty, QUEUE, null, body);
        }

        if (!await WaitUntil(60, () => outstandingConfirms.IsEmpty))
            throw new Exception("All messages could not be confirmed in 60 seconds");

        var endTime = Stopwatch.GetTimestamp();

        Console.WriteLine($"Published {MESSAGES_COUNT:N0} messages and handled confirm asynchronously in {Stopwatch.GetElapsedTime(startTime, endTime).TotalMilliseconds:N0} ms");
        Console.WriteLine("[x] Press [enter] to exit.");
        Console.ReadLine();

        channel.Close();

        void CleanOutstandingConfirms(bool multiple, ulong deliveryTag)
        {
            if (multiple)
            {
                foreach (var entry in outstandingConfirms.Where(x => x.Key <= deliveryTag))
                    outstandingConfirms.TryRemove(entry.Key, out _);
            }
            else
            {
                outstandingConfirms.TryRemove(deliveryTag, out _);
            }
        }

        async Task<bool> WaitUntil(int seconds, Func<bool> condition)
        {
            var waitedMiliseconds = 0;

            while (!condition() && waitedMiliseconds < seconds * 1000)
            {
                await Task.Delay(100);
                waitedMiliseconds += 100;
            }

            return condition();
        }
    }

    public IModel CreateConfirmedSelectChannel()
    {
        var channel = _connection.CreateModel();
        channel.ConfirmSelect();
        return channel;
    }
}