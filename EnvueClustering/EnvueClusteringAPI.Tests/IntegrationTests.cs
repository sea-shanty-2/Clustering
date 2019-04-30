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
        public int longitude { get; }
        public int latitude { get; }
        public int[] streamDescription { get; }
        public int timeStamp { get; }
        
        public StreamRequest(int longitude, int latitude, int[] streamDescription, int timestamp)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.streamDescription = streamDescription;
            timeStamp = timestamp;
        }
    }
    
    [TestFixture]
    public class IntegrationTests
    {
        private StreamRequest _stream = new StreamRequest(10, 20, new int[] {0, 1, 0}, 0);
        
        private HttpClient _client;

        private async Task<HttpResponseMessage> AddStream()
        {
            return await _client.PostAsync("data/add", new StringContent(JsonConvert.SerializeObject
                (new List<StreamRequest> {_stream}), Encoding.UTF8, "application/json"));
        }
        private async Task<HttpResponseMessage> RemoveStream()
        {
            return await _client.PostAsync("data/remove", new StringContent(JsonConvert.SerializeObject
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
        public async Task DataAdd_OnePoint_OnePoint()
        {
            HttpResponseMessage response = await AddStream();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task ClusteringOneStream_AddAndCluster_OnePoint()
        {
            await AddStream();
        
            HttpResponseMessage response = await _client.GetAsync("clustering/events");
            
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("[]", await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task ClusteringOneStream_AddAndRemoveAndCluster_ZeroPoints()
        {
            await AddStream();
            await RemoveStream();

            HttpResponseMessage response = await _client.GetAsync("clustering/events");
            
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("[]", await response.Content.ReadAsStringAsync());
        }
    }
}