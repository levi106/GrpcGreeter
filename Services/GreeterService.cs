using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace GrpcGreeter
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        private int _count;

        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override Task<CounterReply> IncrementCount(Empty request, ServerCallContext context)
        {
            _logger.LogInformation("Incrementing count by 1");
            _count += 1;
            return Task.FromResult(new CounterReply { Count = _count });
        }

        public override async Task<CounterReply> AccumulateCount(IAsyncStreamReader<CounterRequest> requestStream, ServerCallContext context)
        {
            await foreach (var message in requestStream.ReadAllAsync())
            {
                _logger.LogInformation($"Incrementing count by {message.Count}");
                _count += message.Count;
            }

            return new CounterReply { Count = _count };
        }

        public override async Task Countdown(Empty request, IServerStreamWriter<CounterReply> responseStream, ServerCallContext context)
        {
            for (var i = _count; i >= 0; i--)
            {
                await responseStream.WriteAsync(new CounterReply { Count = i });
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
