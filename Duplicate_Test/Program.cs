﻿using CoreTweet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Duplicate_Test
{
    class Program
    {
        private const int INTERVAL_MINUTES = 5;
        private const string DUPLICATE_TWEET_DOC = "Duplicate-Test tweet.";

        private static Tokens tokens;
        private static User user;

        private static bool tweetedInMinute = false;
        private static int count = 0;

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Pleased enter PIN Code.");

            string apiKey = "FcB5aQmrNvsq2YxsYR2tJSAka";
            string apiKeySecret = "09ref6yrgmBhJIGrGQaANzo3t41myIUtx35QDzZrrOHOBFnrNQ";

            var sessions = OAuth.Authorize(apiKey, apiKeySecret);
            Uri url = sessions.AuthorizeUri;
            System.Diagnostics.Process.Start(url.ToString());

            string pin = Console.ReadLine();

            Tokens t = OAuth.GetTokens(sessions, pin);
            tokens = t;
            user = t.Account.VerifyCredentials();

            TimerCallback tcb = new TimerCallback(timer_Tick);
            Timer timer = new Timer(tcb, null, 0, 5000);

            Console.WriteLine("Authentication was successful.");
            Console.WriteLine("Duplicate-Test start.");
            Console.WriteLine();

            string command = string.Empty;
            do {
                Console.Write("Command? > ");
                command = Console.ReadLine();

                switch (command)
                {
                    case "status":
                        Console.WriteLine("Tweet Count: {0}", count);
                        Console.WriteLine("Ellapsed Time: {0} min.", count * INTERVAL_MINUTES);
                        Console.WriteLine();
                        break;
                    default:
                        break;
                }
            } while (command != "exit");
        }

        /// <summary>
        /// Timer Event
        /// </summary>
        private static void timer_Tick(object o)
        {
            DateTime dt = DateTime.Now;
            if (dt.Minute % INTERVAL_MINUTES != 0)
            {
                tweetedInMinute = false;
                return;
            }

            if (tweetedInMinute)
            {
                return;
            }

            Console.WriteLine();
            string doc = string.Format(
@"Tweet count: {0}
Elapsed Time: {1} min.",
                count,
                count * INTERVAL_MINUTES);
            tweet(tokens, doc);
            count++;
            tweet(tokens, DUPLICATE_TWEET_DOC);
            Console.WriteLine();

            tweetedInMinute = true;

            Console.Write("Command? > ");
        }

        /// <summary>
        /// Tweet
        /// </summary>
        /// <param name="tokens">Authenticated token</param>
        /// <param name="doc">Tweet content document</param>
        private static void tweet(Tokens tokens, string doc)
        {
            var param = new Dictionary<string, object>();
            param.Add("status", doc);
            try
            {
                tokens.Statuses.Update(param);
                Console.WriteLine("{0}: {1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), doc);
            }
            catch (Exception ex)
            {
                string message = string.Format(
@"{0}
doc: {1}",
                    ex.Message,
                    doc);
                Console.WriteLine(message);
            }
        }
    }
}
