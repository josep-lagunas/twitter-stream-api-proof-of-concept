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

        private ITwitterApiClient twitterApiClient;
        
        public TwitterWSController(ITwitterApiClient twitterApiClient)
        {
            this.twitterApiClient = twitterApiClient;
        }

        [AllowAnonymous]
        [Route("api/available-events")]
        [HttpGet]
        public override IHttpActionResult GetAvailableServerEvents()
        {
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.Accepted,
                Enum.GetValues(typeof(ServerEvents)).Cast<ServerEvents>()
                .Select(e => { return new ServerEventsDTO((int)e, e.ToString()); })));
        }

        [Route("api/start-streaming-tweets")]
        [HttpPost]
        public IHttpActionResult StartSTreamingTweets([FromBody] SearchSettings searchSettings)
        {
            string clientId = GetClientToken();

            if (!twitterApiClient.
                StartStreamingTweets(clientId, searchSettings.KeyWords,
                searchSettings.Languages, searchSettings.MapBoxCoordinates, (object sender, TweetStreamArgs e) =>
                {
                    NotifyServerEventAsync(clientId, ServerEvents.GET_TWEETS, e.Tweet);
                }))
            {
                return ResponseMessage(new HttpResponseMessage(HttpStatusCode.Continue));
            }
            else
            {
                return ResponseMessage(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }

        [Route("api/stop-streaming-tweets")]
        [RequiresAuthorization]
        [HttpPost]
        public IHttpActionResult StopSTreamingTweets()
        {
            string clientId = GetClientToken();
          
            if (!twitterApiClient.StopStreamingTweetsByHashTags(clientId))
            {
                return ResponseMessage(new HttpResponseMessage(HttpStatusCode.Forbidden)
                { ReasonPhrase = String.Format("no client identified by {0} or service already stopped.", clientId) });
            }
            return ResponseMessage(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}