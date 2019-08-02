using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterApi;

namespace TwitterApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Write hashtags words, separated with ',' and without '#' char:");
            List<string> keywords = Console.ReadLine().Split(',').ToList();
            Console.Write("Write languages for tweets, separated with ',' (ex. en, es):");
            List<string> languages = Console.ReadLine().Split(',').ToList();
            Console.Write(
                "Write pairs of coordinates for tweets located from the inbox(es) created, separated with ',' (ex. -122.75,36.8,-121.75,37.8 for tweets from San Francisco):");
            List<string> boxCoordinates = Console.ReadLine().Split(',').ToList();

            while (boxCoordinates.Count % 4 != 0)
            {
                Console.Write(
                    "Non multiple of 4 number of coordinates detected, please add a pair number of coordinates (or none for avoid) (ex. -122.75,36.8,-121.75,37.8 for tweets from San Francisco):");
                boxCoordinates = Console.ReadLine().Split(',').ToList();
            }

            List<MapBoxCoordinates> mapBoxCoordinates = new List<MapBoxCoordinates>();
            for (int i = 0; i < boxCoordinates.Count; i = i + 4)
            {
                mapBoxCoordinates.Add(
                    new MapBoxCoordinates(decimal.Parse(boxCoordinates[i].Replace('.', ',')),
                        decimal.Parse(boxCoordinates[i + 1].Replace('.', ',')),
                        decimal.Parse(boxCoordinates[i + 2].Replace('.', ',')),
                        decimal.Parse(boxCoordinates[i + 3].Replace('.', ','))));
            }

            TwitterApiClient twitterApiClient =
                new TwitterApiClient(new HTTP.Helpers.HttpInvoker());
            twitterApiClient.SetCredentials(""
                , ""
                , ""
                , "");

            twitterApiClient.StartStreamingTweets("dummykey", keywords, languages,
                mapBoxCoordinates, (object sender, TweetStreamArgs e) =>
                {
                    if (e.Tweet != null)
                    {
                        Console.WriteLine(e.Tweet.text);
                        Console.WriteLine();
                    }
                });

            Console.ReadKey();
        }
    }
}