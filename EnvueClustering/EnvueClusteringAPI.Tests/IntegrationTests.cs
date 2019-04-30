using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EnvueClusteringAPI.Tests
{
    public class StreamRequest
    {
        public string id { get; }
        public int longitude { get; }
        public int latitude { get; }
        public int[] streamDescription { get; }
        public int timeStamp { get; }
        
        public StreamRequest(string id, int longitude, int latitude, int[] streamDescription, int timestamp)
        {
            this.id = id;
            this.longitude = longitude;
            this.latitude = latitude;
            this.streamDescription = streamDescription;
            timeStamp = timestamp;
        }

        public override bool Equals(object obj)
        {
            return obj is StreamRequest other && (id == other.id &&
                                                  longitude == other.longitude &&
                                                  latitude == other.latitude &&
                                                  streamDescription == other.streamDescription &&
                                                  timeStamp == other.timeStamp);
        }
    }
    
    [TestFixture]
    public class IntegrationTests
    {
        private readonly StreamRequest _stream = new StreamRequest("TestStream", 10, 20, new[] {0, 1, 0}, 0);
        private readonly string _clusterResult = "[[{\"longitude\": 10,\"latitude\": 20,\"streamDescription\":[0,1,0],\"id\": \"TestStream\",\"timeStamp\": 0}]]";
        
        private HttpClient _client;

        private async Task<HttpResponseMessage> AddStream()
        {
            return await _client.PostAsync("data/add", new StringContent(JsonConvert.SerializeObject
                (new List<StreamRequest> {_stream}), Encoding.UTF8, "application/json"));
        }
        private async Task RemoveStream()
        {
            await _client.PostAsync("data/remove", new StringContent(JsonConvert.SerializeObject
                (_stream), Encoding.UTF8, "application/json"));
        }

        [SetUp]
        public void InitializeClient()
        {
            _client = new HttpClient {BaseAddress = new Uri("http://localhost:5000/")};
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [Test]
        public async Task DataAdd_OnePoint_OK()
        {
            HttpResponseMessage response = await AddStream();
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
            await AddStream();
            HttpResponseMessage response = await _client.GetAsync("clustering/events");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(_stream.Equals(JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync()) as StreamRequest));
        }

        [Test]
        public async Task ClusteringEvent_AddAndRemoveOnePoint_BadRequest()
        {
            await AddStream();
            await RemoveStream();

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