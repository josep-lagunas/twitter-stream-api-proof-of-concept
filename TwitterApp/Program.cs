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
            Console.Write("Write pairs of coordinates for tweets located from the inbox(es) created, separated with ',' (ex. -122.75,36.8,-121.75,37.8 for tweets from San Francisco):");
            List<string> boxCoordinates = Console.ReadLine().Split(',').ToList();

            while (boxCoordinates.Count % 4 != 0)
            {
                Console.Write("Non multiple of 4 number of coordinates detected, please add a pair number of coordinates (or none for avoid) (ex. -122.75,36.8,-121.75,37.8 for tweets from San Francisco):");
                boxCoordinates = Console.ReadLine().Split(',').ToList();
            }

            List<MapBoxCoordinates> mapBoxCoordinates = new List<MapBoxCoordinates>();
            for (int i = 0; i < boxCoordinates.Count; i = i + 4)
            {
                mapBoxCoordinates.Add(
                    new MapBoxCoordinates(decimal.Parse(boxCoordinates[i].Replace('.',',')), 
                                          decimal.Parse(boxCoordinates[i + 1].Replace('.', ',')), 
                                          decimal.Parse(boxCoordinates[i + 2].Replace('.', ',')), 
                                          decimal.Parse(boxCoordinates[i + 3].Replace('.', ','))));
            }

            TwitterApiClient twitterApiClient = new TwitterApiClient(new HTTP.Helpers.HttpInvoker());
            twitterApiClient.SetCredentials("jkuG56zlta1exJJ3kGi2mlXRM"
                    , "kPHXBkmLqOV9thDnFE4QJpvzND7hkJBp8AYtwcIts9l64LEmt8"
                    , "430727651-vHPtvToq1UK3RHm3tMrQmQA4BW3PdJlxAopL53We"
                    , "rEArJ1vb8Uuh24WTeh9tW8DKFPNWfEvEFte3jdfUkXaPC");

            twitterApiClient.StartStreamingTweets("dummykey", keywords, languages, mapBoxCoordinates, (object sender, TweetStreamArgs e) =>
            {
                if (e.Tweet != null)
                {
                    Console.WriteLine(e.Tweet.text);
                    Console.WriteLine();
                }
            });


                //Task<string> result = TwitterApiClient.getInstance().Tweet(input);
                //Task<string> result2 = TwitterApiClient.getInstance().DirectMessage("testaccountjlc", input);
                //Task<string> result3 = TwitterApiClient.getInstance().ReTweetLastMessage("josep_lagunas");
                //Task<string> result4 = TwitterApiClient.getInstance().ReTweetMessage(903541646029185024);
                //Task<string> result5 = TwitterApiClient.getInstance().ReTweetMessage(903619996970094592, "Aixó és NOU  retweet amb missatge:");
                //Task.WaitAll(result, result2, result3, result4, result5);

                //Console.WriteLine(result.Result);
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine(result2.Result);
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine(result3.Result);
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine(result4.Result);
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine(result5.Result);
                Console.ReadKey();

        }

       
    }
}
