using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSocketApi.Controllers
{
    public class WebsocketDataPackage
    {
        public string clientId { get; set; }
        public string HashedKey { get; set; }
        public object Data { get; set; }

        public WebsocketDataPackage(string clientId, string hashedKey, object data)
        {
            this.clientId = clientId;
            this.HashedKey = hashedKey;
            this.Data = data;
        }
        
    }
}