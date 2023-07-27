using DotMpi;
using DotOrmLib;
using DotOrmLib.Proxy;
using DotOrmLib.Proxy.Scc1.Interfaces;
using DotOrmLib.Proxy.Scc1.Services;
using Grpc.Net.Client;
using Microsoft.Data.SqlClient;
using ProtoBuf.Grpc.Client;

namespace DotOrmGrpcClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:57057/");
            var client = channel.CreateGrpcService<IHealthCheckService>();
            var healthCheck = await client.HealthCheck();
            Console.WriteLine($"Health Check Result: {healthCheck.Result}");

            var scoringClient = channel.CreateGrpcService<ITblScoringController1>();
            var scoringHealthCheck = await scoringClient.HealthCheck();
            Console.WriteLine($"Scoring Health Check Result: {healthCheck.Result}");

            var studyId = 2144933293;
            var intRequest = new IntValue { Value = studyId };
           
            var getResult = await scoringClient.GetById(intRequest);
            Console.WriteLine($"Scoring Health Check Result: {healthCheck.Result}");

            var col = new Dictionary<string, object?>();
            col.Add("@p_0", new SerializableValue(studyId-10000));
            col.Add("@p_1", new SerializableValue(studyId +1));

            var filter = new FilterRequest("[Study_ID] between @p_0 and @p_1", col);
            var listResult =await scoringClient.GetList(filter);
            //var reply = await client.GetById(studyId);
            Console.WriteLine($"Retrieved {listResult.Count} results");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

}