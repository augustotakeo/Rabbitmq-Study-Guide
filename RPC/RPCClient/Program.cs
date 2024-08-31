var rpcClient = new RPCClient();

var n = args.Length > 0 ? args[0] : "10";

Console.WriteLine(" [x] Requesting fib({0})", n);
var response = await rpcClient.CallAsync(n);
Console.WriteLine(" [.] Got '{0}'", response);