using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace Suctionator
{
    class Program
    {        
        private static string urlIssues = "https://github.com/VinceGusmini/Suctionator/issues";
        private static string urlInput;
        private static string baseUrlInput;
        private static string mediaName;
        private static string mediaSeason;
        private static int countTotalLinks;
        private static int numLastEpisode;
        private static List<string> uptoboxLinks = new List<string>();
        private static List<string> pubLinks = new List<string>();
        private static ILogger log;
        private static HtmlDocument htmlDoc; 
        private static HtmlNode nodeLinks; 

        static void Main(string[] args)
        {
            #region Log init
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });
            log = loggerFactory.CreateLogger<Program>();
            //log.LogInformation("Info ");
            //log.LogWarning("Warning ");
            //log.LogError("Error ");
            //log.LogCritical("Critical ");
            #endregion

            htmlDoc = new HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
            //urlInput = String.Empty; 
            //urlInput = "https://www2.tirexo.art/animes/674759-l-attaque-des-titans-WEB-DL%201080p-VOSTFR.html";
            urlInput = "https://www2.tirexo.art/telecharger-series/617584-le-bureau-des-legendes-saison-3-Blu-Ray%201080p-French.html";           
            mediaName = String.Empty;
            mediaSeason = String.Empty;
            countTotalLinks = 0;
            Console.WriteLine("Bienvenue sur Suctionator !");

            // 1
            Start:

            AskInput();

            if (CheckUrlAsync(urlInput).Result) //GET OK
            {
                Console.WriteLine("URL valid"); // TO EDIT / CHANGE

                ExtractInfos();

                GetPubLinks();

                Console.WriteLine // TO EDIT / CHANGE                                         
                    ($"My cells have detected {uptoboxLinks.Count} uptobox links to download in {countTotalLinks} available for {mediaName} (Saison {mediaSeason}). Is that correct ?");
                Console.WriteLine("'y' for YES || 'n' for NO :"); // TO EDIT / CHANGE
                char response = Console.ReadLine()[0];

                if (response == 'y') // OK
                {
                    Console.WriteLine("TOUT EST CARRE");
                }

                AskTicketIssue();
            }
            else
                WrongInput();

            goto Start;            
        }


        #region Brain

        /// <summary>
        /// Request the target url to see if it's online
        /// </summary>
        /// <param name="urlInput"></param>
        /// <returns></returns>
        private static async Task<bool> CheckUrlAsync(string urlInput)
        {
            if (!urlInput.Contains("tirexo"))
            {
                log.LogWarning("This is not Tirexo URL") ;                
                return false;
            }

            HttpClient httpClient = new HttpClient();
            try
            {
                string htmlStr = await httpClient.GetStringAsync(urlInput);
                
                htmlDoc.LoadHtml(htmlStr);

                if(!String.IsNullOrEmpty(htmlDoc.ParsedText))
                {
                    Console.WriteLine("Enter the Tirexo URL of your choice :");
                    return true;
                }

                log.LogError("Tirexo page is unreachable !");
                return false;
            }
            catch (Exception ex)
            {
                log.LogCritical(ex.Message);
                return false;
            }
        }


        /// <summary>
        /// Extract media name + media season
        /// </summary>
        private static void ExtractInfos()
        {
            try
            {
                var offset = urlInput.IndexOf('/');
                offset = urlInput.IndexOf('/', offset + 1);
                int endIndex = urlInput.IndexOf('/', offset + 1);
                
                var tmpLi = htmlDoc.DocumentNode.Descendants("li").ToList();
                var tmpH3 = htmlDoc.DocumentNode.Descendants("h3").ToList();
                var tmpDiv = htmlDoc.DocumentNode.Descendants("div").ToList();

                var tmpMediaName = tmpH3.First().InnerText;
                var tmpSeasonNode = tmpLi.FirstOrDefault(x => x.OuterHtml.StartsWith("<li><strong>Saison</strong> :"));

                baseUrlInput = urlInput.Substring(0, endIndex +1);
                mediaName = CleanHtmlCode(tmpMediaName);
                mediaSeason = tmpSeasonNode.InnerHtml.Substring(26, 1);
            }
            catch (Exception ex)
            {
                log.LogCritical("ExtractInfos: " + ex.Message);
            }
        }

        /// <summary>
        /// Recover and select pub site links
        /// </summary>
        private static void GetPubLinks()
        {
            try
            {
                int numEpisodeTarget = 0;
                bool firstTime = true;

                //var listtmp = htmlDoc.DocumentNode.Descendants("table").ToList();                
                var tmpUptoboxTable = htmlDoc.DocumentNode.Descendants("table").FirstOrDefault(x => x.XPath ==
                    "/html[1]/body[1]/div[3]/div[4]/div[1]/div[2]/div[1]/div[1]/div[1]/div[2]/div[3]/div[1]/div[1]/div[1]/div[3]/div[1]/div[1]/table[1]");
                nodeLinks = tmpUptoboxTable.ChildNodes.FirstOrDefault(child => child.Name == "tbody");

                var lstTr = nodeLinks.ChildNodes.ToList();
                lstTr.RemoveAt(lstTr.Count -1); //trash
                lstTr.RemoveAt(lstTr.Count -1); //footer
                lstTr.Reverse(); //index = reverse upload order

                countTotalLinks = lstTr.Count;                

                //TODO : choose link by size
                foreach (var tr in lstTr)
                {
                    if (firstTime)
                    {
                        firstTime = false;                                                
                        numLastEpisode = RecoverNumEpisode(tr);
                        pubLinks.Add(RecoverLinkEpisode(tr));
                        numEpisodeTarget = numLastEpisode -1;
                    }

                    if (RecoverNumEpisode(tr) == numEpisodeTarget)
                    {
                        pubLinks.Add(RecoverLinkEpisode(tr));
                        numEpisodeTarget--;
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogCritical("GetUptoboxLinks: " + ex.Message);
            }
        }

        private static int RecoverNumEpisode(HtmlNode node)
        {
            int res;
            string innerTextClean = node.InnerText.Remove(0, 1); // cut \n
            var splitResult = innerTextClean.Split(' ');

            string potentialRes = (splitResult[1].Any(char.IsDigit)) ? splitResult[1] : splitResult[2];
            
            int.TryParse(potentialRes, out res);
            return res;
        }

        private static string RecoverLinkEpisode(HtmlNode node)
        {
            return baseUrlInput + "link-" + node.Attributes.FirstOrDefault().Value + ".html";
        }


        private static string CleanHtmlCode(string tmpSeasonNumber)
        {
            tmpSeasonNumber = tmpSeasonNumber.Replace("&#039;", "'");
            return tmpSeasonNumber;
        }
        #endregion

        #region Console
        private static void AskInput()
        {
            Console.WriteLine("Enter the Tirexo URL of your choice :");
            //urlInput = Console.ReadLine();
        }

        private static void WrongInput()
        {
            Console.WriteLine("Wrong input, let's retry !");
        }

        private static void AskTicketIssue()
        {
            Console.WriteLine($"Mission Failed... Please go open a ticket issue with your requested url in : {urlIssues}");
            Console.WriteLine("In advance thank you for helping me improve this :)");
        }
        #endregion

    }
}
