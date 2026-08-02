// See https://aka.ms/new-console-template for more information
using Grpc.Core;
using Protobufs;

const string serverPort = "127.0.0.1:50008";

Channel channel = new Channel(serverPort, ChannelCredentials.Insecure);

await channel.ConnectAsync().ContinueWith((task) =>
{
    if(task.Status == TaskStatus.RanToCompletion)
    {
        Console.WriteLine("The client connected to the gRPC server");
    }
});

// Unary
var person = new Person()
{
    Name = "Prueba",
    LastName = "T",
    Email = "prueba@correo.com"
};

var request = new PersonRequest()
{
    Person = person,
};

var client = new PersonService.PersonServiceClient(channel);

var response = await client.RegisterPersonAsync(request);

//var client = new Operations.OperationsClient(channel);

Console.WriteLine($"Server response: {response.ToString()}");

// Server Streaming
var requestStream = new PersonRequestStream
{
    Person = person
};

var responseStream = client.RegisterPersonStreaming(requestStream);

while (await responseStream.ResponseStream.MoveNext())
{
    Console.WriteLine(responseStream.ResponseStream.Current.Result);
    await Task.Delay(250);
}

// Client Streaming
var requestClientStream = new PersonClientRequestStream
{
    Person = person
};

var clientStream = client.RegisterPersonClientStreaming();

foreach (int i in Enumerable.Range(1, 10))
{
    requestClientStream.Person.Name += $" {i}";
    await clientStream.RequestStream.WriteAsync(requestClientStream);
}

await clientStream.RequestStream.CompleteAsync();

var responseClientStream = await clientStream.ResponseAsync;

Console.WriteLine($"Client stream response: {responseClientStream}");

// Bidirectional Streaming
ICollection<Person> persons = new List<Person>()
{
    new Person{ Email = "person1@correo.com"},
    new Person{ Email = "person2@correo.com"},
    new Person{ Email = "person3@correo.com"}
};

var stream = client.RegisterPersonBidirectional();

foreach (var per in persons)
{
    Console.WriteLine($"Send {per.Email}");

    var bidirectionalRequest = new BidirectionalPersonRequest()
    {
        Person = per
    };

    await stream.RequestStream.WriteAsync(bidirectionalRequest);
}

await stream.RequestStream.CompleteAsync();

var responseCollection = Task.Run(async () =>
{
    while (await responseStream.ResponseStream.MoveNext())
    {
        Console.WriteLine($"Stream response {stream.ResponseStream.Current.Result}{Environment.NewLine}");
    }
});
await responseCollection;


await channel.ShutdownAsync();
Console.WriteLine("Press any key");
Console.ReadLine();
