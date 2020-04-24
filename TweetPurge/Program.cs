using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace TweetPurge
{
    //// Get your Twitter Consumer Key and Secret here:
    //// https://dev.twitter.com
    //// Setup a new app, and the info should be within said app.
    class Program
    {
        static async Task Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "TweetPurge";
            app.HelpOption("-?|-h|--help");

            app.OnExecute(() =>
            {
                Console.WriteLine("Please include your Consumer Key and Consumer Secret.");
                Console.WriteLine(@"Example: .\TweetPurge purge -ck MYKEY -cs MYSECRET");
                return 0;
            });

            app.Command("purge", (command) =>
            {
                command.Description = "Purge your entire Twitter history.";
                command.HelpOption("-?|-h|--help");

                var consumerKeyOption = command.Option("-ck|--ConsumerKey", "Consumer Key for Twitter Application.", CommandOptionType.SingleValue);
                var consumerSecretOption = command.Option("-cs|--ConsumerSecret", "Consumer Secret for Twitter Application.", CommandOptionType.SingleValue);

                command.OnExecute(async () =>
                {
                    var consumerKey = consumerKeyOption.Values;
                    var consumerSecret = consumerSecretOption.Values;

                    if(consumerKey.Count == 0 || consumerSecret.Count == 0)
                    {
                        Console.WriteLine("Invalid Consumer Key or Consumer Secret provided.");
                        return 0;
                    }

                    await ProcessTwitterAccount(consumerKey.FirstOrDefault(), consumerSecret.FirstOrDefault());

                    Console.WriteLine("\r\n\r\nSuccessfully purged your Twitter timeline. Enjoy!")
                    return 1;
                });
            });

            app.Execute(args);
        }

        private static async Task ProcessTwitterAccount(string consumerKey, string consumerSecret)
        {
            var appCredentials = new TwitterCredentials(consumerKey, consumerSecret);
            var authenticationContext = AuthFlow.InitAuthentication(appCredentials);

            Console.WriteLine("Copy the URL below. When it loads, and you are logged in, allow access to the application. Copy and paste the code below and hit enter.");
            Console.WriteLine($"\r\n{authenticationContext.AuthorizationURL}\r\n");
            Console.Write("Enter Code (and hit enter): ");

            var pinCode = Console.ReadLine();

            var authenticatedUser = GetAuthenticatedUser(pinCode, authenticationContext);

            await ProcessTweets(authenticatedUser);
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
