using System.Collections.Generic;
using System.Threading.Tasks;

namespace TwitterApi
{
    public interface ITwitterApiClient
    {
        event TwitterApiClient.StreamTweetByHashtagHandler StreamTweetByHashTagEvent;

        Task<string> DirectMessage(string user, string message);
        Task<string> ReTweetLastMessage(string user);
        Task<string> ReTweetMessage(long tweetId);
        Task<string> ReTweetMessage(long tweetId, string message);
        void SetCredentials(string consumerKey, string consumerSecretKey, string accessToken, string accessTokenSecret);
        bool StartStreamingTweets(string id, IEnumerable<string> hashtags, IEnumerable<string> languages, IEnumerable<MapBoxCoordinates> mapBoxCoordinates, TwitterApiClient.StreamTweetByHashtagHandler streamTweetHandler);
        void StartStreamingTweets(string id, IEnumerable<string> hashtags, IEnumerable<string> languages, TwitterApiClient.StreamTweetByHashtagHandler streamTweetHandler);
        void StartStreamingTweets(string id, IEnumerable<string> hashtags, TwitterApiClient.StreamTweetByHashtagHandler streamTweetHandler);
        bool StopStreamingTweetsByHashTags(string id);
        Task<string> Tweet(string message);
    }
}