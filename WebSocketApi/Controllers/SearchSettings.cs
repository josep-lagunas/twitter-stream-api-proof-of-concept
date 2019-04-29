using System.Collections.Generic;
using Newtonsoft.Json;
using TwitterApi;

namespace WebSocketApi.Controllers
{
    public class SearchSettings
    {
        [JsonProperty(PropertyName = "keywords")]
        public List<string> KeyWords { get; set; }
        [JsonProperty(PropertyName = "languages")]
        public List<string> Languages { get; set; }
        [JsonProperty(PropertyName = "mapboxcoordinates")]
        public List<MapBoxCoordinates> MapBoxCoordinates { get; set; }

    }
}