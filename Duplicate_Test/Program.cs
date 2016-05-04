using CoreTweet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Duplicate_Test
{
    class Program
    {
        private const int INTERVAL_MINUTES = 5;
        private const string DUPLICATE_TWEET_DOC = "Duplicate-Test tweet.";

        private static Tokens tokens;
        private static User user;

        private static bool tweetedInMinute = false;
        private static int tweetCount = 0;
        private static int resetCount = 0;
        private static string filePath = string.Empty;
        private static bool isNotTweeted = true;

        private static Logger logger = LogManager.GetCurrentClassLogger();

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

            // destroy tweet
            filePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\since_id.txt";
            long sinceId = getStatusId(filePath);
            destroyTweet(tokens, sinceId);

            Console.WriteLine("Duplicate-Test start.");
            logger.Debug("Duplicate-Test start.");
            Console.WriteLine();

            string command = string.Empty;
            do {
                Console.Write("Command? > ");
                command = Console.ReadLine();

                switch (command)
                {
                    case "status":
                        Console.WriteLine("Tweet Count: {0}", tweetCount);
                        Console.WriteLine("Ellapsed Time: {0} min.", tweetCount * INTERVAL_MINUTES);
                        Console.WriteLine();
                        break;
                    default:
                        break;
                }
            } while (command != "exit");
        }

        /// <summary>
        /// Read First-Tweet ID from .txt
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static long getStatusId(string filePath)
        {
            StreamReader sr = null;

            try
            {
                sr = new StreamReader(filePath, Encoding.Default);
                string doc = sr.ReadLine();
                long id = long.Parse(doc);

                return id;
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                }
            }
        }

        /// <summary>
        /// Write First-Tweet ID to .txt
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="statusId"></param>
        private static void setStatusId(string filePath, long statusId)
        {
            StreamWriter sw = null;

            try
            {
                sw = new StreamWriter(filePath, false, Encoding.Default);
                sw.WriteLine(statusId);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
            }
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
            string doc = string.Format("Duplicate-Test. Tweet Count: {0}, Reset Count: {1}, Elapsed Time: {2} min.",
                tweetCount,
                resetCount,
                tweetCount * INTERVAL_MINUTES);
            tweet(tokens, doc);
            tweetCount++;

            if (tweet(tokens, DUPLICATE_TWEET_DOC))
            {
                tweetCount = 1;
                resetCount++;
            }

            Console.WriteLine();
            tweetedInMinute = true;
            Console.Write("Command? > ");
        }

        /// <summary>
        /// Tweet
        /// </summary>
        /// <param name="tokens">Authenticated token</param>
        /// <param name="doc">Tweet content document</param>
        private static bool tweet(Tokens tokens, string doc)
        {
            var param = new Dictionary<string, object>();
            param.Add("status", doc);
            try
            {
                var status = tokens.Statuses.Update(param);
                Console.WriteLine("{0}: {1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), doc);
                logger.Debug("Tweeted: {0}", doc);

                if (isNotTweeted)
                {
                    isNotTweeted = true;
                    setStatusId(filePath, status.Id);
                }

                return true;
            }
            catch (Exception ex)
            {
                string message = string.Format(
@"{0}
doc: {1}",
                    ex.Message,
                    doc);
                Console.WriteLine(message);

                return false;
            }
        }

        /// <summary>
        /// Delete
        /// </summary>
        private static void destroyTweet(Tokens tokens, long sinceId)
        {
            var param= new Dictionary<string, object>();
            param.Add("q", "Test");
            param.Add("result_type", "recent");
            param.Add("since_id", sinceId);

            var searchResult = tokens.Search.Tweets(param);

            int deleteCount = 0;
            foreach (var status in searchResult)
            {
                if (status.User.Id != user.Id)
                {
                    continue;
                }

                var destroyParam = new Dictionary<string, object>();
                destroyParam.Add("id", status.Id);
                tokens.Statuses.Destroy(destroyParam);
                deleteCount++;
            }

            Console.WriteLine("Destoryed tweet count: {0}", deleteCount);
            logger.Debug("Destoryed tweet count: {0}", deleteCount);
        }
    }
}
