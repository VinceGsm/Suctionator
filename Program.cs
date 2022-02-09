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
        private static string mediaName;
        private static string mediaSeason;
        private static int countTotalLinks;
        private static List<string> uptoboxLinks = new List<string>();
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
            urlInput = "https://www2.tirexo.art/animes/674759-l-attaque-des-titans-WEB-DL%201080p-VOSTFR.html";
            //urlInput = "https://www2.tirexo.art/telecharger-series/617584-le-bureau-des-legendes-saison-3-Blu-Ray%201080p-French.html";           
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

                GetUptoboxLinks(); // TODO 

                Console.WriteLine($"My cells have detected {uptoboxLinks.Count} uptobox links to download for {mediaName} (Saison {mediaSeason}). Is that correct ?"); // TO EDIT / CHANGE                                         
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

        private static void GetUptoboxLinks()
        {
            //var listtmp = htmlDoc.DocumentNode.Descendants("table").ToList();                
            var tmpUptoboxTable = htmlDoc.DocumentNode.Descendants("table").FirstOrDefault(x => x.XPath ==
                "/html[1]/body[1]/div[3]/div[4]/div[1]/div[2]/div[1]/div[1]/div[1]/div[2]/div[3]/div[1]/div[1]/div[1]/div[3]/div[1]/div[1]/table[1]");
            nodeLinks = tmpUptoboxTable.ChildNodes.FirstOrDefault(child => child.Name == "tbody");

            var toto = "";
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
                var tmpLi = htmlDoc.DocumentNode.Descendants("li").ToList();
                var tmpH3 = htmlDoc.DocumentNode.Descendants("h3").ToList();
                var tmpDiv = htmlDoc.DocumentNode.Descendants("div").ToList();

                var tmpMediaName = tmpH3.First().InnerText;
                var tmpSeasonNode = tmpLi.FirstOrDefault(x => x.OuterHtml.StartsWith("<li><strong>Saison</strong> :"));

                mediaName = CleanHtmlCode(tmpMediaName);
                mediaSeason = tmpSeasonNode.InnerHtml.Substring(26, 1);

                //var testo = tmpDiv.FirstOrDefault(x => x.XPath == "/html[1]/body[1]/div[3]/div[4]/div[1]/div[2]/div[1]/div[1]/div[1]/div[2]/div[3]/div[1]/div[1]/div[1]/div[3]/div[1]");
                // xpath potentiel autre node links :              /html[1]/body[1]/div[3]/div[4]/div[1]/div[2]/div[1]/div[1]/div[1]/div[2]/div[3]/div[1]/div[1]/div[1]/div[3]/div[1]               

                //Console.WriteLine($"TOTAL = "+tmpDiv.Count);
                //int i = 0;
                //foreach (var node2merde in tmpDiv)
                //{                    
                //    //Console.WriteLine($"#{i} : "+ node2merde.OuterHtml);
                //    Console.WriteLine("--------");
                //    Console.WriteLine($"#{i} : " + node2merde.InnerText);
                //    i++;
                //}

                //List<string> ids = new List<string>();
                //foreach (var node in htmlDoc.SelectNodes("//div/@id"))
                //{
                //    ids.Add(node.InnerText);
                //}

                //var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//dataTables_info");
                //var htmlNodessss = htmlDoc.DocumentNode.SelectNodes("//DataTables_Table_0_info"); 
                //var htmlNodezdzqdssss = htmlDoc.DocumentNode.SelectNodes("//row");

                //var tmpRow = .DocumentNode.Descendants("row").ToList();

                //var testt = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class,'col-sm-12 col-md-5')]");

                //var divWithAttributes = tmpDiv.Where(x => x.Attributes.Count >= 1).ToList();
                //var divWithOutAttributes = tmpDiv.Where(x => x.Attributes.Count == 0).ToList();

                //var testtt = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class,'dataTables_info.DataTables_Table_0_info')]");
                //var testttt = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class,'dataTables_info')]");
                //var testdttt = htmlDoc.DocumentNode.SelectNodes("//div[contains(@id,'DataTables_Table_0_info')]");

                //var id = "DataTables_Table_0_info";
                //var query = $"//manifest/item[@id='{id}']";
                //HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode(query);

                //var tmpSpan = htmlDoc.DocumentNode.Descendants("span").ToList();

                //var tesBIS = htmlDoc.DocumentNode.SelectNodes("//span[contains(@class,'masha_index masha_index112')]");

                ////var titi = htmlDoc.DocumentNode.Descendants("DataTables_Table_0_info");
                //var ezrtyj = htmlDoc.DocumentNode.Descendants("col-sm-12 col-md-5");
                //var DONC  = tmpDiv.FirstOrDefault(w => w.OuterHtml.Contains("dataTables_info"));
            }
            catch (Exception ex)
            {
                log.LogCritical("ExtractInfos: " + ex.Message);
            }
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
