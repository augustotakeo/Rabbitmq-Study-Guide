# Rabbitmq Study Guide
I've created this repository to showcase my studies on RabbitMQ. Initially, I'm following the [Get Started](https://www.rabbitmq.com/tutorials) to grasp the basic concepts. So far, I've completed seven projects based on the [Queue Tutorials](https://www.rabbitmq.com/tutorials#queue-tutorials). Below I'll outline the key concepts covered in these projects.

### What is a message broker?
It's a software that enables communication and exchange of information between applications, systems and services. I+t receives messages from a publisher (applications that publish messages) and route them to consumers (applications that process messages)

### What is an exchange?
Exchanges are entities that receive messages and route them to queues. They route messages based on an algorithm defined by its type and rules called bindings. There four types of exchanges:   

- **Direct**: A message is routed to a bound queue when the message's routing key matches the queue's routing key.
- **Fanout**: A message is routed to every bound queue.
- **Topic**: A message is routed to a bound queue when the message's routing key matches the pattern defined by the queue's routing key. The queue's routing key consists of a list of words separated by dots, where asterisks ('*') matches any single word and hash ('#') matches any single word or sequence of words.
- **Header**: A message is routed to a bound queue when the message's headers match the criteria specified by the queue's headers. If header 'x-match' is set to 'any', a single matching header is sufficient, if it is set to 'all', all headers must match. Headers that begin with 'x-' will not evaluated regardless of whether 'x-match' is set to 'all' or 'any'.


Exchanges have attributes beyond their type:
- **durable**: Survives to a broker restart
- **autodelete**: Is deleted when the last queue is unbound from it

### What is a queue?
It's an entity that stores messages that are consumed by applications. Like exchanges, queues have properties:

- **name**
- **durable**: Survives to a broker restart
- **autodelete**: Is deleted when the last connection is closed
- **exclusive**: used by only one connection and is deleted when this connection is closed
