using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSocketApi.Controllers
{
    public enum ServerEvents
    {
        GET_TWEETS = 0, // data needed { keywords: [string], languages: [string], [{ longitude1: double, latitude1: double }, { longitude2: double, latitude2: double }]}        
    }
}