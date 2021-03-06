﻿using AdventureWorks.SkiResort.Infrastructure.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;
using System;

namespace AdventureWorks.SkiResort.Infrastructure.AzureSearch
{
    public class RestaurantsSearchService
    {
        private readonly string _serviceName = string.Empty;
        private readonly string _apiKey = string.Empty;
        private readonly string _indexer = string.Empty;
        private readonly string _suggesterName = string.Empty;


        public RestaurantsSearchService(IConfigurationRoot configuration)
        {
            _serviceName = configuration.Get<string>("SearchConfig:ServiceName");
            _apiKey = configuration.Get<string>("SearchConfig:ApiKey");
            _indexer = configuration.Get<string>("SearchConfig:Indexer");
            _suggesterName = configuration.Get<string>("SearchConfig:Suggester");
        }

        public async Task<List<Restaurant>> GetNearByAsync(int count)
        {
            string uri = $"https://{_serviceName}.search.windows.net/indexes/{_indexer}/docs?api-version=2015-02-28&$top={count}&$orderby=RestaurantId";
            using (var _httpClient = new HttpClient())
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

                var response = await _httpClient.GetAsync(uri);
                var jsonString = await response.Content.ReadAsStringAsync();
                var restaurants = JsonConvert.DeserializeObject<RootObject>(jsonString);
                return restaurants.value.ToList();
            }
        }

        public async Task<List<int>> GetRecommendationsAsync(string searchtext, int count)
        {
            int id = int.Parse(searchtext); // roundtrip through int to ensure it's a number
            string uri = $"https://{_serviceName}.search.windows.net/indexes/{_indexer}/docs/{id.ToString("00")}?api-version=2015-02-28";

            using (var _httpClient = new HttpClient())
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

                var response = await _httpClient.GetAsync(uri);
                var jsonString = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(jsonString);
                var obj = result?.RecommendedIds;
                return obj == null ? new List<int>() : ((JArray)obj).Select(t => int.Parse((string)t)).ToList();
            }
        }

    }

    class RootObject
    {
        public string odatacontext { get; set; }
        public Restaurant[] value { get; set; }
    }

    class SuggestionsRootObject
    {
        public string odatacontext { get; set; }
        public Suggestion[] value { get; set; }
    }

    class Suggestion
    {
        public string searchtext { get; set; }
        public int RestaurantId { get; set; }
    }

}
