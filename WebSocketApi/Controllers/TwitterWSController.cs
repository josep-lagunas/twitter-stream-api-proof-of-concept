using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using TwitterApi;

namespace WebSocketApi.Controllers
{    
    public class TwitterWSController : WebSocketController
    {
        private static bool streamingTweets = false;

        public TwitterWSController()
        {
            if (!streamingTweets)
            {
                ThreadStart start = new ThreadStart(() =>
                {
                    TwitterApiClient.getInstance()
                 .setCredentials("jkuG56zlta1exJJ3kGi2mlXRM"
                     , "kPHXBkmLqOV9thDnFE4QJpvzND7hkJBp8AYtwcIts9l64LEmt8"
                     , "430727651-vHPtvToq1UK3RHm3tMrQmQA4BW3PdJlxAopL53We"
                     , "rEArJ1vb8Uuh24WTeh9tW8DKFPNWfEvEFte3jdfUkXaPC");

                    TwitterApiClient.getInstance().StreamTweetByHashTagEvent += TwitterClient_streamTweetByHashTagEvent;

                    List<string> keyWords = new List<string>(); 
                    List<string> languages = new List<string>();
                    List<MapBoxCoordinates> mapBoxCoordinates = new List<MapBoxCoordinates>() { new MapBoxCoordinates(-180, -90, 180, 90) };
                    TwitterApiClient.getInstance().GetTweetsByHashtags(keyWords, languages, mapBoxCoordinates);
                });
                Thread th = new Thread(start);
                th.Start();
            }
            streamingTweets = true;
        }

        private void TwitterClient_streamTweetByHashTagEvent(object sender, TweetStreamArgs e)
        {
            NotifyServerEventAsync(ServerEvents.GET_TWEETS, e.Tweet);
        }

        [Route("api/available-events")]
        [HttpGet]
        public override IHttpActionResult GetAvailableServerEvents()
        {
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.Accepted,
                Enum.GetValues(typeof(ServerEvents)).Cast<ServerEvents>()
                .Select(e => { return new ServerEventsDTO((int)e, e.ToString()); })));
        }


    }
}