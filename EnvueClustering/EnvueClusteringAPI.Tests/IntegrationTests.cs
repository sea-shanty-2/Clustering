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
        public void InitializeClient()
        {
            _client = new HttpClient {BaseAddress = new Uri("http://localhost:5000/")};
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [Test]
        public void JsonTest()
        {
            string json = JsonConvert.SerializeObject(_streamer, Formatting.None, _jsonSettings);
            JObject back = JsonConvert.DeserializeObject(json) as JObject;

            string id = back["id"].ToString();
            int longitude = int.Parse(back["longitude"].ToString());
            int latitude = int.Parse(back["latitude"].ToString());
            
            string[] str = Regex.Replace(back["streamDescription"].ToString(), @"\[*\]*\n* *", "").Split(",");
            float[] floats = Array.ConvertAll(str, float.Parse);

            int timeStamp = int.Parse(back["timeStamp"].ToString());

            Streamer stream = new Streamer(longitude, latitude, floats, timeStamp, id);

            Assert.AreEqual(_streamer.ToString(), stream.ToString());
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
        public async Task ClusteringEvent_OnePoint_OnePoint()
        {
            await AddStreamer();
            HttpResponseMessage response = await _client.GetAsync("clustering/events");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string test = await response.Content.ReadAsStringAsync();
            
            JObject json = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), _jsonSettings) as JObject;

            string id = json["id"].ToString();
            int longitude = int.Parse(json["longitude"].ToString());
            int latitude = int.Parse(json["latitude"].ToString());
            
            string[] str = Regex.Replace(json["streamDescription"].ToString(), @"\[*\]*\n* *", "").Split(",");
            float[] floats = Array.ConvertAll(str, float.Parse);

            int timeStamp = int.Parse(json["timeStamp"].ToString());

            Streamer stream = new Streamer(longitude, latitude, floats, timeStamp, id);

            Assert.AreEqual(_streamer.ToString(), stream.ToString());
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

        [TearDown]
        public async Task ClearData()
        {
            await _client.GetAsync("data/clear");
        }
    }
}