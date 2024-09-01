using RabbitMQ.Client;
using System.Text;
using System.Collections.Concurrent;

var pc = new PublisherConfirms();
pc.PublishMessagesIndividually();
pc.PublishMessagesInBatch();
await pc.HandlePublishConfirmsAsynchronously();