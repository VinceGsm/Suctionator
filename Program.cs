using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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

        static void Main(string[] args)
        {
            #region Log init
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning)
                    .AddFilter("System", Microsoft.Extensions.Logging.LogLevel.Warning)
                    .AddFilter("NonHostConsoleApp.Program", Microsoft.Extensions.Logging.LogLevel.Debug)
                    .AddConsole();
            });
            log = loggerFactory.CreateLogger<Program>();
            //log.LogInformation("Info ");
            //log.LogWarning("Warning ");
            //log.LogError("Error ");
            //log.LogCritical("Critical ");
            #endregion

            ResetHtmlDoc();
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

                //ExtractInfos();

                //GetPubLinks();

                GetUptoboxLinksAsync();

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

        private static async Task GetUptoboxLinksAsync()
        {

            //foreach (string pubLink in pubLinks)
            //{
                //ResetHtmlDoc();
            //}
            string test = "https://www2.tirexo.art/link-10457599.html";
            ResetHtmlDoc();

            
            try
            {
                //HttpClient httpClient = new HttpClient();
                //string htmlStr = await httpClient.GetStringAsync(test);

                //htmlDoc.LoadHtml(htmlStr);

                //var btn = htmlDoc.GetElementbyId("sumbit_btn");

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// SCRAPY
                ///
                //ScrapingBrowser browser = new ScrapingBrowser();
                //browser.AllowAutoRedirect = true;
                //browser.AllowMetaRedirect = true;
                //ResetHtmlDoc();
                //WebPage webPage = await browser.NavigateToPageAsync(new Uri(test), HttpVerb.Get, "", "text/html; charset=UTF-8");

                //htmlDoc.LoadHtml(webPage.Content);

                //var h3Init = htmlDoc.DocumentNode.Descendants("h3").ToList();

                //var form = webPage.FindFormById("get_link");

                //form.Method = HttpVerb.Post;

                ////form["getlink"] = "0";

                ////var etet = form.FormFields;

                //WebPage webPageAfter = form.Submit();

                //htmlDoc.LoadHtml(webPageAfter.Content);

                //var h3After = htmlDoc.DocumentNode.Descendants("h3").ToList();

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PUPPPP


                //var connectOptions = new ConnectOptions()
                //{
                //    BrowserWSEndpoint = "$wss://chrome.browserless.io/"
                //};

                //using (var browser = await Puppeteer.ConnectAsync(connectOptions))
                //{
                //    Page page = await browser.NewPageAsync();
                //    await page.GoToAsync(test);

                //    var toto = page.Frames;

                //    await page.ClickAsync("");
                //}

                //RevisionInfo toto = await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

                //Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions
                //{
                //    Headless = true
                //});


                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Simple

                //var browser = new Browser();

                //// log the browser request/response data to files so we can interrogate them in case of an issue with our scraping
                //browser.RequestLogged += OnBrowserMessageLogged;
                //browser.MessageLogged += new Action<Browser, string>(OnBrowserMessageLogged);

                //// we'll fake the user agent for websites that alter their content for unrecognised browsers
                //browser.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.10 (KHTML, like Gecko) Chrome/8.0.552.224 Safari/534.10";

                //browser.Navigate(test);

                ////var toto = browser.Select("geetest_wait"); 
                ////var todddto = browser.Select(".geetest_wait"); 
                ////var todto = browser.Select("div.geetest_wait"); 

                ////var toto = browser.FindElements(By.ClassName("question-hyperlink"));

                //var firstBtn = browser.Find("geetest_radar_tip");                
                //var secondBtn = browser.Find("sumbit_btn");

                //var clikRes = firstBtn.Click();
                //var resFinal = secondBtn.Click();

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////  Selenium
                IWebDriver browserDriver = new ChromeDriver();
                browserDriver.Navigate().GoToUrl(test);
                browserDriver.Manage().Window.Maximize();

                Actions actionProvider = new Actions(browserDriver);

                var safeZone = browserDriver.FindElement(By.Id("kt_subheader"));
                var btnCaptcha = browserDriver.FindElement(By.Id("captcha"));
                var btnValidate = browserDriver.FindElement(By.Id("sumbit_btn"));

                actionProvider.MoveToElement(btnCaptcha).Build().Perform();
                actionProvider.DoubleClick(btnCaptcha).Build().Perform();
                actionProvider.Click(btnValidate).Build().Perform();

                var h3Result = browserDriver.FindElements(By.TagName("h3")).ToList();
                string htmlText = h3Result.First(x => x.Text.Contains("uptobox.com/")).Text;                
                string link = htmlText[7..]; // ==  htmlText.Substring(7); 

                browserDriver.Quit();
                uptoboxLinks.Add(link.Trim());                
            }
            catch (Exception ex)
            {
                log.LogCritical(ex.Message);                
            }
        }

        private static void GetPubLinks()
        {
            try
            {
                int numEpisodeTarget = 0;
                bool firstTime = true;

                //var listtmp = htmlDoc.DocumentNode.Descendants("table").ToList();                
                var tmpPubTable = htmlDoc.DocumentNode.Descendants("table").FirstOrDefault(x => x.XPath ==
                    "/html[1]/body[1]/div[3]/div[4]/div[1]/div[2]/div[1]/div[1]/div[1]/div[2]/div[3]/div[1]/div[1]/div[1]/div[3]/div[1]/div[1]/table[1]");
                HtmlNode nodePubLinks = tmpPubTable.ChildNodes.FirstOrDefault(child => child.Name == "tbody");

                var lstTr = nodePubLinks.ChildNodes.ToList();
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

        private static void ResetHtmlDoc()
        {
            htmlDoc = new HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
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
