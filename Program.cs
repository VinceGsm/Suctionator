using HtmlAgilityPack;
using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Suctionator
{
    class Program
    {
        private static string _version = "Version " + Assembly.GetEntryAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        private static string urlIssues = "https://github.com/VinceGusmini/Suctionator/issues";                        
        private static string _pathDl;
        private static string _uptoboxToken;        
        private static int numLastEpisode = 0;
        private static int countTotalLinks = 0;
        private static Label labelAutoDl;
        private static Stopwatch stopWatch;        
        private static TextField entryLink;
        private static HtmlDocument htmlDoc;
        private static TextField entryGetToken;
        private static bool _isAutoDlOn = false;
        private static string urlInput = string.Empty;
        private static string mediaName = string.Empty;
        private static string mediaSeason = string.Empty;
        private static string baseUrlInput = string.Empty;
        private static string resultUptobox = string.Empty;
        private static List<string> pubLinks = new List<string>();
        private static List<string> uptoboxLinks = new List<string>();


        [STAThread]
        static async Task Main(string[] args)
        {
            _uptoboxToken = Environment.GetEnvironmentVariable("Suctionator_Token");
            _pathDl = Environment.GetEnvironmentVariable("Suctionator_Path");

            if (string.IsNullOrEmpty(_pathDl))
            {
                _pathDl = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders",
                    "{374DE290-123F-4565-9164-39C4925E467B}", string.Empty).ToString();
            }
            else if (_pathDl == @"E:\Downloads")
                _isAutoDlOn = true; //Home
            
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
                new MenuBarItem ("Téléchargement", new MenuItem [] {
                    new MenuItem ("Récupération automatique des résultats", "", () => RequestDownload())
                }),
                new MenuBarItem ("Special", new MenuItem [] {
                    new MenuItem ("One Piece", "", () => Soon())
                }),
                new MenuBarItem ("", new MenuItem [] {}),
                new MenuBarItem ("", new MenuItem [] {})
            };

            if (string.IsNullOrEmpty(_uptoboxToken))
            {
                menuBarItem.SetValue(new MenuBarItem("Uptobox", new MenuItem[] {
                    new MenuItem ("Connexion", "", () => UptoboxConnexion())                   
                    }
                ), 3);
                menuBarItem.SetValue(new MenuBarItem("Help", new MenuItem[] {
                    new MenuItem ("Get help", "", () => GetHelp() )
                    }
                ), 4);
            }
            else
            {
                menuBarItem.SetValue(new MenuBarItem("Help", new MenuItem[] {
                    new MenuItem ("Get help", "", () => GetHelp() )
                    }
                ), 3);
            }

            var menuBar = new MenuBar(menuBarItem);            
            
            topLevel.Add(menuBar, homeWindow);

            InitHtmlDoc();

            Application.Run();                                                                                                                                  
        }


        #region Front_end + Gui Action
        private static void Soon()
        {
            MessageBox.ErrorQuery(50, 7, "Prochainement", "Cette fonctionnalité sera disponible dans la Version 2.1", "J'attends");
        }

        /// <summary>
        /// Automatically resize without manual intervention
        /// </summary>
        /// <returns></returns>
        private static Window CreateHomeWindow()
        {
            return new Window(_version)
            {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

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
            #region Main Input
            var labelInput = new Label("URL Tirexo de la saison :")
            {
                X = Pos.Center(),
                Y = Pos.Center() - 10,
                Width = 30,
                Height = 1
            };

            entryLink = new TextField("")
            {
                X = labelInput.X,
                Y = labelInput.Y + 2,
                Width = 110,
                Height = 1
            };

            var btnInput = new Button("GO !")
            {
                X = entryLink.X - 10,
                Y = entryLink.Y + 2,
                Width = 10,
                Height = 1
            };

            btnInput.Clicked += new Action(GoClicked);
            #endregion

            #region "Auto" Input
            labelAutoDl = new Label($"Mode Auto Download : {_isAutoDlOn}")
            {
                X = btnInput.X - 5,
                Y = btnInput.Y + 5,
                Width = 30,
                Height = 1
            };
            
            labelAutoDl.Clicked += new Action(AutoDLClicked);            
            #endregion

            home.Add(labelInput, entryLink, btnInput, labelAutoDl);            
        }

        private static void GoClicked()
        {            
            ResetResult();
            stopWatch.Start();

            urlInput = entryLink.Text.ToString();
            if (CheckUrl(urlInput))
            {
                if (_isAutoDlOn)
                {
                    ExtractInfos();
                    MessageBox.Query(50, 5, "MODE AUTO : ON", $"Aspiration + Download pour {mediaName} (Saison {mediaSeason})", "Suivant");
                    MessageBox.ErrorQuery(50, 5, "MODE AUTO : ON", "NE TOUCHEZ PLUS A RIEN JUSQU'AU PROCHAIN MESSAGE", "Compris");
                    GetPubLinks();
                    GetUptoboxLinks();
                    DownloadProcess();
                }
                else
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
        }

        private static void AutoDLClicked()
        {
            _isAutoDlOn = !_isAutoDlOn;
            labelAutoDl.Text = $"Mode Auto Download : {_isAutoDlOn}";
        }
        
        private static void GiveResult()
        {
            if (!string.IsNullOrEmpty(resultUptobox))
            {                
                bool success = Clipboard.TrySetClipboardData(resultUptobox);
                if (success)
                    MessageBox.Query(50, 5, "Résultat récupéré", "La liste des liens Upotobox a été copié dans votre clipboard !", "Merci");
                else
                    MessageBox.ErrorQuery(50, 5, "ERROR", "Impossible de récupéré les résultats", "Ok");
            }
            else
                MessageBox.ErrorQuery(50, 5, "Alerte", "Aucun résultat à récupérer", "Ok");
        }

        private static void RequestDownload()
        {
            if (string.IsNullOrEmpty(_uptoboxToken))
            {
                MessageBox.ErrorQuery(50, 7, "Alerte", "Impossible de télécharger sans Token Uptobox", "Ok");
            }
            else if (uptoboxLinks.Count == 0)
            {
                MessageBox.ErrorQuery(50, 5, "Alerte", "Aucun résultat à récupérer", "Ok");
            }
            else
            {
                MessageBox.Query(50, 5, "Download in progress", "Merci de laisser tournée en fond l'application.", "Ok");
                MessageBox.Query(50, 5, "Download in progress", "Un message vous donnera le nombre d'épisode téléchargé ou si une erreur survient", "Ok");

                DownloadProcess();
            }
        }

        private static void DownloadProcess()
        {
            stopWatch.Restart();
            stopWatch.Start();

            int countSucceedDL = DownloadResult();
            if (countSucceedDL > 1)
            {
                stopWatch.Stop();
                MessageBox.Query(50, 5, "Terminé", $"Un total de {countSucceedDL} épisodes téléchargés sur {uptoboxLinks.Count} dans vos résultats " +
                    $"(en {stopWatch.Elapsed.Minutes}m)", "Ok");
            }
            else
                MessageBox.ErrorQuery(50, 5, "ERROR", "RequestDownload failed, please contact Vince on Github or Discord", "Ok");
        }

        private static void GetHelp()
        {
            bool success = Clipboard.TrySetClipboardData(urlIssues);
            if (success)
                MessageBox.Query(50, 5, "Url du projet récupéré", "En cas de problème merci de créer un post sur Github: " +
                    "le lien a directement été collé dans votre clipbopard", "Ok");
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

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Back_end    
        /// <summary>
        /// Use token Uptobox to download all the links in the result var
        /// </summary>
        /// <returns>number of dowloaded episode</returns>
        private static int DownloadResult()
        {
            int res = 0;         

            foreach (var link in uptoboxLinks)
            {
                var isDlSucceed = DownloadLink(link);
                if (isDlSucceed)
                    res++;
            }
            return res;
        }

        private static bool DownloadLink(string link)
        {
            string codeToken = GetCodeToken(link);
            string requestTokenUrl = $"https://uptobox.com/api/link?token={_uptoboxToken}&file_code={codeToken}";
            try
            {
                using (WebClient client = new WebClient())
                {
                    // Get the final url to DL with the Token
                    var responseToken = client.DownloadString(requestTokenUrl);
                    var responseResult = JsonSerializer.Deserialize<ResponseWaitToken>(responseToken);
                    string fileName =_pathDl + '\\' + GetFileName(responseResult.data.dlLink);
                    //DL 
                    client.DownloadFile(responseResult.data.dlLink, fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery(50, 7, "Error", $"{ex.Message}", "Ok");                
                return false;
            }
            return true;
        }

        private static string GetCodeToken(string link)
        {
            var tmp = link.Split("/");
            var bonsoir = tmp.Last();

            if (bonsoir.Contains('?'))
            {
                tmp = bonsoir.Split("?");
                bonsoir = tmp.First();
            }            
            return bonsoir;                                    
        }

        private static string GetFileName(string dlLink)
        {
            var tmp = dlLink.Split("/");
            return tmp.Last().Replace(@"%20", " ");
        }

        /// <summary>
        /// Request the target url to see if it's online
        /// </summary>
        /// <param name="urlInput"></param>
        /// <returns></returns>
        private static bool CheckUrl(string urlInput)
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

                if (!string.IsNullOrEmpty(htmlDoc.ParsedText))
                    return true;
                else
                    throw new Exception();
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery(50, 7, "Error", "Impossible d'accéder à la page", "Ok");                
            }
            return false;
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
                MessageBox.ErrorQuery(50, 7, "Error", $"ExtractInfos failed : {ex.Message}", "Ok");
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
                }
            }
            resultUptobox = string.Join(Environment.NewLine, uptoboxLinks);
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

                var tmpPubTable = htmlDoc.DocumentNode.Descendants("table").First();
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
                MessageBox.ErrorQuery(50, 7, "Error", $"GetPubLinks failed : {ex.Message}", "Ok");
            }
        }

        private static void ResetResult()
        {
            resultUptobox = string.Empty;
            uptoboxLinks.Clear();
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
    }
}
