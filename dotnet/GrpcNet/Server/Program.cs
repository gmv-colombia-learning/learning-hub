// See https://aka.ms/new-console-template for more information
using Grpc.Core;
using Protobufs;
using Server;
using GrpcServer = Grpc.Core.Server;

const int Port = 50008;

GrpcServer? server = null;

try
{
    server = GetServer(Port);
    server.Start();

    Console.Write($"Service in execution. Port: {Port}");
    Console.ReadKey();
}catch(IOException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
finally
{
    if( server != null)
    {
        server.ShutdownAsync().Wait();
    }
}

static GrpcServer GetServer(int port) => new ()
{
    Services = { PersonService.BindService(new PersonServiceImplementation())},
    Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
};
