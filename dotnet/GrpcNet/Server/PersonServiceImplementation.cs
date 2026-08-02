using Grpc.Core;
using Protobufs;
using static Protobufs.PersonService;

namespace Server
{
    /// <summary>
    /// Implementación de del servicio definido en el .proto
    /// </summary>
    public class PersonServiceImplementation : PersonServiceBase
    {
        public override Task<PersonResponse> RegisterPerson(PersonRequest personRequest, ServerCallContext context)
        {
            string message = $"The user was registered. Name: {personRequest.Person.Name} {personRequest.Person.LastName}, Email: {personRequest.Person.Email}";
            
            PersonResponse response = new PersonResponse()
            {
                Result = message
            };

            return Task.FromResult(response);
        }

        public override async Task RegisterPersonStreaming(PersonRequestStream request, IServerStreamWriter<PersonResponseStream> responseStream, ServerCallContext context)
        {
            Console.WriteLine($"The server received the client's request: {request.ToString}");

            string message = $"The user was inserted successfully: {request.Person.Name} {request.Person.LastName}. {request.Person.Email}";

            foreach(int i in Enumerable.Range(0, 10))
            {
                PersonResponseStream response = new PersonResponseStream()
                {
                    Result = $"Response {i}, {message}"
                };

                await responseStream.WriteAsync(response);
            }
        }

        public override async Task<PersonClientResponseStream> RegisterPersonClientStreaming(IAsyncStreamReader<PersonClientRequestStream> requestStream, ServerCallContext context)
        {
            string result = string.Empty;

            while(await requestStream.MoveNext())
            {
                result += String.Format("Request received by the server: {0}. ", requestStream.Current.Person.Email, Environment.NewLine);
            }

            var responseStream = new PersonClientResponseStream()
            {
                Result = result
            };
            
            return responseStream;
        }

        public override async Task RegisterPersonBidirectional(IAsyncStreamReader<BidirectionalPersonRequest> requestStream, 
            IServerStreamWriter<BidirectionalPersonResponse> responseStream, ServerCallContext context)
        {
            while(await requestStream.MoveNext())
            {
                var message = $"Bidirectional response: {requestStream.Current.Person.Email}{Environment.NewLine}";

                var response = new BidirectionalPersonResponse()
                {
                    Result = message
                };

                await responseStream.WriteAsync(response);
            }
        }
    }
}
