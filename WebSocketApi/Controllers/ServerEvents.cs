using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSocketApi.Controllers
{
    public enum ServerEvents
    {
        GET_TWEETS =
            0, // data needed { keywords: [string], languages: [string], [{ longitude1: double, latitude1: double }, { longitude2: double, latitude2: double }]}        
    }

    public class ServerEventsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ServerEventsDTO(int id, string name)
        {
            this.Id = Id;
            this.Name = name;
        }
    }
}