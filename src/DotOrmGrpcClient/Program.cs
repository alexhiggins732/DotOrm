using DotMpi;
using DotOrmLib;
using DotOrmLib.GrpcModels.Interfaces;
using DotOrmLib.GrpcModels.Scalars;
using DotOrmLib.GrpcServices;
using DotOrmLib.Proxy;
using DotOrmLib.Proxy.Scc1.Interfaces;
using DotOrmLib.Proxy.Scc1.Services;
using Grpc.Net.Client;
using Microsoft.Data.SqlClient;
using ProtoBuf.Grpc.Client;
using System.Numerics;

namespace DotOrmGrpcClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:57057/");

            var scalarClient = channel.CreateGrpcService<IGrpcScalarTestService>();
            var s = await scalarClient.EchoString("hello world");
            var s1 = await scalarClient.EchoNullableString((string?)null);
            var s2 = await scalarClient.EchoInt(1);
            var s3 = await scalarClient.EchoNullableInt((int?)null);

            string ts = await scalarClient.EchoString("hello world");
            string? ts1 = await scalarClient.EchoNullableString((string?)null);
            int ts2 = await scalarClient.EchoInt(1);
            int? ts3 = await scalarClient.EchoNullableInt((int?)null);


            //var b1 = await scalarClient.EchoBigInt(b);
            BigInteger b = 1;



            var bigSerializable = new GrpcValue<BigInteger>(b);
            var br = await scalarClient.EchoGrpcValueOfBigInteger(bigSerializable);
            var brValue = br.Item;
            var bigSerializableT = new SerializableValue<BigInteger>(b);
            var ba = (SerializableValue<BigInteger>)(await scalarClient.EchoRefOfSerializable(bigSerializableT));

            var barray = new byte[] { 0, 1 };
            var b2 = await scalarClient.EchoByteArray(barray);

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
            col.Add("@p_0", new SerializableValue(studyId - 10000));
            col.Add("@p_1", new SerializableValue(studyId + 1));

            var filter = new FilterRequest("[Study_ID] between @p_0 and @p_1", col);
            var listResult = await scoringClient.GetList(filter);
            //var reply = await client.GetById(studyId);
            Console.WriteLine($"Retrieved {listResult.Value.Count} results");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

}