using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Suctionator
{
    class Program
    {        
        private static string urlIssues = "https://github.com/VinceGusmini/Suctionator/issues";
        private static string urlInput = string.Empty;
        private static string result = string.Empty;
        private static string baseUrlInput = string.Empty;
        private static string mediaName = string.Empty;
        private static string mediaSeason = string.Empty;
        private static int countTotalLinks = 0;
        private static int numLastEpisode = 0;
        private static List<string> uptoboxLinks = new List<string>();
        private static List<string> pubLinks = new List<string>();
        private static ILogger log;
        private static HtmlDocument htmlDoc;
        private static string _uptoboxToken;         
        private static TextField entryGetToken;
        private static TextField entryLink;
        private static Stopwatch stopWatch;


        [STAThread]
        static async Task Main(string[] args)
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
            #endregion

            _uptoboxToken = Environment.GetEnvironmentVariable("Uptobox_Token");
            stopWatch = new Stopwatch();

            // GUI
            Application.Init();
           
            var topLevel = Application.Top;

            // Creates the top-level window to show
            var homeWindow = CreateHomeWindow();                                

            SetupHome(homeWindow);            
            
            var menuBarItem = new MenuBarItem[] {
                new MenuBarItem ("Home", new MenuItem [] {
                    new MenuItem ("Suctionator", "", () => SetupHome(homeWindow)),
                    new MenuItem ("Résultat", "", () => GiveResult()),
                    new MenuItem ("Quitter", "", () => { if (Quit()) Application.RequestStop(); } )
                }),
                new MenuBarItem ("Automatique", new MenuItem [] {
                    new MenuItem ("Special One Piece", "", () => Soon())
                }),
                new MenuBarItem ("", new MenuItem [] {}),
                new MenuBarItem ("", new MenuItem [] {})
            };

            if (string.IsNullOrEmpty(_uptoboxToken))
            {
                menuBarItem.SetValue(new MenuBarItem("Uptobox", new MenuItem[] {
                    new MenuItem ("Connexion", "", () => UptoboxConnexion())                   
                    }
                ), 2);
            }

            menuBarItem.SetValue(new MenuBarItem("Help", new MenuItem[] {                    
                    new MenuItem ("Get help", "", () => GetHelp() ) 
                    }
            ), 3);

            var menuBar = new MenuBar(menuBarItem);            
            
            topLevel.Add(menuBar, homeWindow);

            InitHtmlDoc();

            Application.Run();                                                                                                                                  
        }


        #region Front_end
        private static void Soon()
        {
            MessageBox.ErrorQuery(50, 7, "Prochainement", "Cette fonctionnalité sera disponible dans la Version 2.0", "J'attends");
        }

        private static Window CreateHomeWindow()
        {
            return new Window("Esc pour quitter")
            {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

                // automatically resize without manual intervention
                Width = Dim.Fill(-5),
                Height = Dim.Fill(-5)
            };
        }

        private static void UptoboxConnexion()
        {
            var okBtn = new Button(25, 14, "Ok");
            okBtn.Clicked += new Action(OkPressedUptobox);

            var cancelBtn = new Button(3, 14, "Annuler");
            cancelBtn.Clicked += () => Application.RequestStop();

            var dialogGetToken = new Dialog("Token Uptobox :", 60, 18, okBtn, cancelBtn);

            entryGetToken = new TextField()
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(),
                Height = 1
            };
            dialogGetToken.Add(entryGetToken);

            Application.Run(dialogGetToken);
        }

        private static void OkPressedUptobox()
        {
            string input = entryGetToken.Text.ToString();
            if (input.Length == 37)
            {
                Environment.SetEnvironmentVariable("Uptobox_Token", input);
                _uptoboxToken = input;
                MessageBox.Query(50, 7, "Info", "Token valide", "Ok");

                Application.RequestStop();
            }
            else
                MessageBox.ErrorQuery(50, 7, "Error", "Bad Uptobox_Token", "Ok");
        }

        private static void SetupHome(View home)
        {
            var label = new Label("(Tirexo) URL de la saison :")
            {
                X = Pos.Center(),
                Y = Pos.Center() - 10,
                Width = 30,
                Height = 1
            };

            entryLink = new TextField("")            
            {
                X = label.X,
                Y = label.Y + 2,
                Width = 110,
                Height = 1
            };

            var btn = new Button("GO !")
            {
                X = entryLink.X,
                Y = entryLink.Y + 2,
                Width = 10,
                Height = 1
            };

            btn.Clicked += new Action(GoClicked);

            home.Add(label, entryLink, btn);
        }

        private static void GoClicked()
        {
            stopWatch.Start();

            urlInput = entryLink.Text.ToString();
            if (CheckUrlAsync(urlInput).Result)
            {                
                ExtractInfos();
                MessageBox.Query(50, 5, "1/3 : Extracting data done", $"Aspiration pour {mediaName} (Saison {mediaSeason})", "Suivant");

                GetPubLinks();
                MessageBox.ErrorQuery(50, 5, "Links 1/2 : pub done", "NE TOUCHEZ PLUS A RIEN JUSQU'AU PROCHAIN MESSAGE", "Compris");

                GetUptoboxLinks();
                stopWatch.Stop();
                MessageBox.Query(50, 5, "Links 2/2 : Uptobox", $"Etape 3/3 ! Aspiration effectuée en {stopWatch.Elapsed.Minutes}m", "OK");
                MessageBox.Query(50, 5, "Informations", $"Une liste de {uptoboxLinks.Count} (sur {countTotalLinks} analysé)" +
                    " liens Uptobox vous attendent dans le menu \"Résultat\" ", "Super !");                                                           
            }
        }

        private static void GiveResult()
        {
            if (!string.IsNullOrEmpty(result))
            {
                bool success = Clipboard.TrySetClipboardData(result);
                if (success)
                    MessageBox.Query(50, 5, "Résultat récupéré", "La liste des liens Upotobox a été copié dans votre clipboard !", "Merci");
                else
                    MessageBox.ErrorQuery(50, 5, "ERROR", "Impossible de récupéré les résultats", "Ok");
            }
            else
                MessageBox.ErrorQuery(50, 5, "ERROR", "Aucun résultat à récupérer", "Ok");
        }

        private static void GetHelp()
        {
            bool success = Clipboard.TrySetClipboardData(urlIssues);
            if (success)
                MessageBox.Query(50, 5, "Url du projet récupéré", "En cas de problème merci de créer un post sur Github: le lien a directement été collé dans votre clipbopard", "Ok");
            else
                MessageBox.ErrorQuery(50, 5, "ERROR", "Impossible de récupéré le lien du projet", "Ok");

            MessageBox.Query(50, 5, "Message du dev", "La remontée de bug aide à améliorer ce projet :)", "<3");
        }

        private static bool Quit()
        {
            var n = MessageBox.Query(50, 5, "Quitter", "Voulez-vous vraiment quitter l'application", "Oui", "Non");
            return n == 0;
        }
        #endregion


        #region Back_end    
        /// <summary>
        /// Request the target url to see if it's online
        /// </summary>
        /// <param name="urlInput"></param>
        /// <returns></returns>
        private static async Task<bool> CheckUrlAsync(string urlInput)
        {
            if (!urlInput.Contains("tirexo"))
            {
                MessageBox.ErrorQuery(50, 7, "Error", "Merci de fournir un URL de Tirexo", "Ok");
                return false;
            }

            HttpClient httpClient = new HttpClient();
            try
            {
                string htmlStr = httpClient.GetStringAsync(urlInput).Result;
                
                htmlDoc.LoadHtml(htmlStr);

                if(!String.IsNullOrEmpty(htmlDoc.ParsedText))                                    
                    return true;

                MessageBox.ErrorQuery(50, 7, "Error", "Impossible d'accéder à la page", "Ok");
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

                baseUrlInput = urlInput.Substring(0, endIndex + 1);
                mediaName = CleanHtmlCode(tmpMediaName);
                mediaSeason = tmpSeasonNode.InnerHtml.Substring(26, 1);
            }
            catch (Exception ex)
            {
                log.LogCritical("ExtractInfos: " + ex.Message);
            }
        }

        private static void GetUptoboxLinks()
        {                       
            foreach (string pubLink in pubLinks)
            {
                IWebDriver browserDriver = CreateChromeBrowser();
                try
                {
                    browserDriver.Manage().Window.Maximize();
                    browserDriver.Navigate().GoToUrl(pubLink);                    

                    Actions actionProvider = new Actions(browserDriver);

                    var h3Result = browserDriver.FindElements(By.TagName("h3")).ToList();                    
                    var btnCaptcha = browserDriver.FindElement(By.Id("captcha"));
                    var btnValidate = browserDriver.FindElement(By.Id("sumbit_btn"));

                    var safeZone = h3Result.First(x => x.Text.Contains("Statistiques"));
                    actionProvider.MoveToElement(safeZone).Build().Perform();

                    actionProvider.MoveToElement(btnCaptcha).Build().Perform();
                    actionProvider.Click(btnCaptcha).Build().Perform();                    

                    actionProvider.MoveToElement(btnValidate).Build().Perform();
                    actionProvider.Click(btnValidate).Build().Perform();                    

                    h3Result = browserDriver.FindElements(By.TagName("h3")).ToList();
                    string htmlText = h3Result.First(x => x.Text.Contains("uptobox.com/")).Text;
                    string link = htmlText[7..]; // ==  htmlText.Substring(7);                     
                    uptoboxLinks.Add(link.Trim());

                    browserDriver.Quit();
                }
                catch (Exception ex)
                {
                    browserDriver.Quit();
                    log.LogCritical(ex.Message);
                }
            }
            result = string.Join(Environment.NewLine, uptoboxLinks);
        }

        private static IWebDriver CreateChromeBrowser()
        {
            // No log --> conflicts GUI
            ChromeDriverService silentService = ChromeDriverService.CreateDefaultService();
            silentService.EnableVerboseLogging = false;
            silentService.SuppressInitialDiagnosticInformation = true;
            silentService.HideCommandPromptWindow = true;

            return new ChromeDriver(silentService);
        }

        private static void GetPubLinks()
        {
            try
            {
                int numEpisodeTarget = 0;
                bool firstTime = true;
                          
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

        private static void InitHtmlDoc()
        {
            htmlDoc = new HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////
        /*
        "https://uptobox.com/api/link?token=[USR_TOKEN]&file_code=[FILE_CODE]";
        */
        /////////////////////////////////////////////////////////////////////////////////// 
    }
}
