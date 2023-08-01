using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotOrmLib.GrpcModels.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Net.Client;
using DotOrmLib.GrpcModels.Interfaces;
using ProtoBuf.Grpc.Client;
using System.Numerics;
using DotMpi;

namespace DotOrmLib.GrpcModels.Services.Tests
{
    [TestClass()]
    public class GrpcScalarTestServiceTests : IDisposable
    {
        private GrpcChannel channel;
        private IGrpcScalarTestService client;

        public GrpcScalarTestServiceTests()
        {
            channel = GrpcChannel.ForAddress("https://localhost:57057/");
            client = channel.CreateGrpcService<IGrpcScalarTestService>();
        }


        [TestMethod()]
        public async Task EchoNullableStringTest()
        {
            string? expected = null;
            var actual = await client.EchoNullableString(expected);
            Assert.AreEqual(expected, (string?) actual);
        }

        [TestMethod()]
        public async Task EchoNullableStringTest1()
        {
            string? expected = "Hello World";
            var actual = await client.EchoNullableString(expected);
            Assert.AreEqual(expected, (string?)actual);
        }

        [TestMethod()]
        public async Task EchoNullableIntTest()
        {
            int? expected = null;
            var actual = await client.EchoNullableInt(expected);
            Assert.AreEqual(expected, (int?)actual);
        }

        [TestMethod()]
        public async Task EchoNullableIntTest2()
        {
            int? expected = 1;
            var actual = await client.EchoNullableInt(expected);
            Assert.AreEqual(expected, (int?)actual);
        }

        [TestMethod()]
        public async Task EchoStringTest()
        {
            string expected = "Hello World";
            var actual = await client.EchoString(expected);
            Assert.AreEqual(expected, (string)actual);
        }

        [TestMethod()]
        public async Task EchoIntTest()
        {
            int expected = 1;
            var actual = await client.EchoInt(expected);
            Assert.AreEqual(expected, (int)actual);

        }

        [TestMethod()]
        public async Task EchoByteArrayTest()
        {
            byte[] expected = { 1, 2 };
            var actual = await client.EchoByteArray(expected);
            Assert.IsTrue(expected.SequenceEqual((byte[])actual));
        }

        [TestMethod()]
        public async Task EchoByteArrayTest1()
        {
            byte[]? expected = null;       
            var actual = await client.EchoByteArray(expected);
            Assert.AreEqual(expected, (byte[]?)actual);
        }

        [TestMethod()]
        public async Task EchoValueOfBigInteger()
        {
            BigInteger expected = 1;
            var actual = await client.EchoValueOfBigInteger(expected);
            Assert.AreEqual(expected, (BigInteger)actual);
        }

        [TestMethod()]
        public async Task EchoGrpcValueOfBigInteger()
        {
            BigInteger expected = 1;
            var actual = await client.EchoGrpcValueOfBigInteger(expected);
            Assert.AreEqual(expected, (BigInteger)actual);
        }

        [TestMethod()]
        public async Task EchoGrpcSerializableOfBigInteger()
        {
            BigInteger expected = 1;
            var valueExplicit= new GrpcSerializableValue<BigInteger>(expected);
            var actual = await client.EchoGrpcSerializableOfBigInteger(valueExplicit);
            Assert.AreEqual(expected, (BigInteger)actual);
        }


        [TestMethod()]
        public async Task EchoRefOfSerializable()
        {
            BigInteger expected = 1;
            var ser= new SerializableValue<BigInteger>(expected);
       
            var actual = await client.EchoRefOfSerializable(ser);
            Assert.AreEqual(expected, (BigInteger)actual.Item.ObjectValue);
        }

        [TestMethod()]
        public async Task EchoGrpcSerializable()
        {
            BigInteger expected = 1;
            GrpcSerializableValue<BigInteger> ser = expected;
            var actual = await client.EchoGrpcSerializable(ser);
            Assert.AreEqual(expected, (BigInteger)actual.ObjectValue);
        }

   
        public void Dispose()
        {
            channel.Dispose();
        }
    }
}