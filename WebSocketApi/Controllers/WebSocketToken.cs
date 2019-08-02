using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSocketApi.Controllers
{
    public class WebSocketToken
    {
        [JsonProperty(PropertyName = "clientid")]
        public string ClientId { get; }

        [JsonProperty(PropertyName = "wstoken")]
        public string Token { get; }

        public WebSocketToken(string clientId, string token)
        {
            this.ClientId = clientId;
            this.Token = token;
        }
    }
}