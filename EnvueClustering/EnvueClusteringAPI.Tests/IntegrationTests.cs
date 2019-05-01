using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using EnvueClusteringAPI.Models;
using Newtonsoft.Json.Serialization;

namespace EnvueClusteringAPI.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private readonly Streamer _streamer = new Streamer(10, 20, new[] {0f, 1f, 0f}, 0, "Test");
        private readonly string _clusterResult = "[[{\"longitude\": 10,\"latitude\": 20,\"streamDescription\":[0,1,0],\"id\": \"TestStream\",\"timeStamp\": 0}]]";
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings() {ContractResolver = new CamelCasePropertyNamesContractResolver()};
        
        private HttpClient _client;

        private async Task<HttpResponseMessage> AddStreamer()
        {
            string test = JsonConvert.SerializeObject
                (new List<Streamer> {_streamer}, Formatting.None, _jsonSettings);
            
            return await _client.PostAsync("data/add", new StringContent(JsonConvert.SerializeObject
                (new List<Streamer> {_streamer}, Formatting.None, _jsonSettings), Encoding.UTF8, "application/json"));
        }
        private async Task RemoveStreamer()
        {
            string test = JsonConvert.SerializeObject
                (_streamer, Formatting.None, _jsonSettings);
            
            await _client.PostAsync("data/remove", new StringContent(JsonConvert.SerializeObject
                (_streamer, Formatting.None, _jsonSettings), Encoding.UTF8, "application/json"));
        }

        [SetUp]
        public async Task InitializeClient()
        {
            _client = new HttpClient {BaseAddress = new Uri("http://localhost:5000/")};
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            await _client.GetAsync("data/clear");
        }

        [Test]
        public void JsonTest()
        {
            string serialized = JsonConvert.SerializeObject(_streamer, Formatting.None, _jsonSettings);
            Streamer streamer = JsonConvert.DeserializeObject<Streamer>(serialized);
            Assert.AreEqual(_streamer.ToString(), streamer.ToString());
        }

        [Test]
        public async Task DataAdd_OnePoint_OK()
        {
            HttpResponseMessage response = await AddStreamer();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task ClusteringEvents_NoPoints_BadRequest()
        {
            HttpResponseMessage response = await _client.GetAsync("clustering/events");
            
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("\"No micro clusters available, aborting DBSCAN clustering.\"", await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task ClusteringEvent_TwoPoints_OneCluster()
        {
            await AddStreamer();
            await AddStreamer();
            HttpResponseMessage response = await _client.GetAsync("clustering/events");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var MCs = JsonConvert.DeserializeObject<List<List<Streamer>>>(await response.Content.ReadAsStringAsync(), _jsonSettings);

            Assert.AreEqual(1, MCs.Count);
            //Assert.AreEqual(_streamer.ToString(), streamers[0].ToString());
            //Assert.AreEqual(_streamer.ToString(), streamers[1].ToString());
        }

        [Test]
        public async Task ClusteringEvent_AddAndRemoveOnePoint_BadRequest()
        {
            await AddStreamer();
            await RemoveStreamer();

            HttpResponseMessage response = await _client.GetAsync("clustering/events");
            
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("\"No micro clusters available, aborting DBSCAN clustering.\"", await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task ClusteringEvent_OnePoint_BadRequest()
        {
            await AddStreamer();
            HttpResponseMessage response = await _client.GetAsync("clustering/events");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TearDown]
        public async Task ClearData()
        {
            await _client.GetAsync("data/clear");
        }
    }
}