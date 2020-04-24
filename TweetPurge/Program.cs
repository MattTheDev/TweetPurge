using System;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace TweetPurge
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Get your Twitter Consumer Key and Secret here:
            // https://dev.twitter.com
            // Setup a new app, and the info should be within said app.
            var appCredentials = new TwitterCredentials("CONSUMERKEY","CONSUMERSECRET");
            var authenticationContext = AuthFlow.InitAuthentication(appCredentials);
            
            Console.WriteLine("Copy the URL below. When it loads, and you are logged in, allow access to the application. Copy and paste the code below and hit enter.");
            Console.WriteLine($"\r\n{authenticationContext.AuthorizationURL}\r\n");
            Console.Write("Enter Code (and hit enter): ");
            
            var pinCode = Console.ReadLine();
            
            var authenticatedUser = GetAuthenticatedUser(pinCode, authenticationContext);

            await ProcessTweets(authenticatedUser);

            Console.WriteLine("All set.");
        }

        private static IAuthenticatedUser GetAuthenticatedUser(string pinCode, IAuthenticationContext authenticationContext)
        {
            var userCredentials = AuthFlow.CreateCredentialsFromVerifierCode(pinCode, authenticationContext);
            Auth.SetCredentials(userCredentials);
            var authenticatedUser = User.GetAuthenticatedUser();
            return authenticatedUser;
        }

        private static async Task ProcessTweets(IAuthenticatedUser authenticatedUser)
        {
            var tweets = Timeline.GetUserTimeline(authenticatedUser.Id).ToList();
            while (tweets.Any())
            {
                foreach (var tweet in tweets)
                {
                    Console.WriteLine($"Processing: {tweet.Text}");
                    var results = await tweet.DestroyAsync();

                    if (!results)
                    {
                        Console.WriteLine("Unable to delete tweet.");
                    }
                    else
                    {
                        Console.WriteLine("Deleted Successfully!");
                    }
                }

                tweets = Timeline.GetUserTimeline(authenticatedUser.Id).ToList();
            }
        }
    }
}
