using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Suctionator
{
    class Program
    {
        // https://www2.tirexo.work/telecharger-series/617578-le-bureau-des-legendes-saison-1-Blu-Ray%201080p-French.html
        private static string urlIssues = "https://github.com/VinceGusmini/Suctionator/issues";
        private static string urlInput;
        private static string mediaName;
        private static string mediaSeason;
        private static List<string> uptoboxLinks = new List<string>();
        private static ILogger log;

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

            urlInput = String.Empty;
            mediaName = String.Empty; //[h3 p-2 : name]
            mediaSeason = String.Empty;
            Console.WriteLine("Bienvenue sur Suctionator !");

            // 1
            Start:

            AskInput();

            if (CheckUrlAsync(urlInput).Result) //GET OK
            {
                Console.WriteLine("URL valid"); // TO EDIT / CHANGE

                GetUptoboxLinks(); // TODO 

                Console.WriteLine($"My cells have detected {uptoboxLinks} uptobox links to download for {mediaName}{mediaSeason}. Is that correct ?"); // TO EDIT / CHANGE                                         
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
            throw new NotImplementedException();
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
                HtmlDocument htmlDoc = new HtmlDocument();
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
        #endregion

        #region Console
        private static void AskInput()
        {
            Console.WriteLine("Enter the Tirexo URL of your choice :");
            urlInput = Console.ReadLine();
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
