using System.Collections.Generic;
using System.Threading.Tasks;
using static TwitterApi.TwitterApiClient;

namespace TwitterApi
{
    public interface ITwitterApiClient
    {
        event TwitterApiClient.StreamTweetByHashtagHandler StreamTweetByHashTagEvent;

        Task<string> DirectMessage(string user, string message);
        void GetTweetsByHashtags(IEnumerable<string> hashtags, StreamTweetByHashtagHandler streamTweetHandler);
        void GetTweetsByHashtags(IEnumerable<string> hashtags, IEnumerable<string> languages, StreamTweetByHashtagHandler streamTweetHandler);
        Task<string> ReTweetLastMessage(string user);
        Task<string> ReTweetMessage(long tweetId);
        Task<string> ReTweetMessage(long tweetId, string message);
        void SetCredentials(string consumerKey, string consumerSecretKey, string accessToken, string accessTokenSecret);
        bool StartStreamingTweetsByHashtags(IEnumerable<string> hashtags, IEnumerable<string> languages, IEnumerable<MapBoxCoordinates> mapBoxCoordinates, 
            StreamTweetByHashtagHandler streamTweetHandler);
        bool StopStreamingTweetsByHashTags();
        Task<string> Tweet(string message);
    }
}