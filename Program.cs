using System;
using System.Collections.Generic;
using System.Net;

namespace Suctionator
{
    class Program
    {
        private static string urlIssues = "https://github.com/VinceGusmini/Suctionator/issues";
        private static string urlInput;
        private static string mediaName;
        private static string mediaSeason;
        private static List<string> uptoboxLinks = new List<string>();

        static void Main(string[] args)
        {
            urlInput = String.Empty;
            mediaName = String.Empty; //[h3 p-2 : name]
            mediaSeason = String.Empty;
            Console.WriteLine("Bienvenue sur Suctionator !");

            // dossier LOG
            // couleur ?
            // mediaSeason handle both case



            // 1
            Start:

            AskInput();

            if (CheckUrl(urlInput)) //GET OK
            {
                Console.WriteLine("URL valid"); // TO EDIT / CHANGE

                //GetUptoboxLinks(); // TODO 

                //Console.WriteLine($"My cells have detected {uptoboxLinks} uptobox links to download for {mediaName}{mediaSeason}. Is that correct ?"); // TO EDIT / CHANGE                
                Console.WriteLine($"My cells have detected 55 uptobox links to download for Toto saison 9. Is that correct ?"); // TO EDIT / CHANGE                
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
        private static Boolean CheckUrl(string urlInput)
        {
            if (!urlInput.Contains("tirexo"))
            {
                Console.WriteLine("This is not Tirexo URL");
                return false;
            }

            Uri urlCheck = new Uri(urlInput);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlCheck);
            request.Timeout = 15000;

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse) request.GetResponse();
                return response.StatusCode == HttpStatusCode.OK;                
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region WriteLine
        private static void AskInput()
        {
            Console.WriteLine("Enter the Tirexo URL of your choice :");
            urlInput = Console.ReadLine();
        }

        private static void WrongInput()
        {
            Console.WriteLine("ERROR : Wrong input");
        }

        private static void AskTicketIssue()
        {
            Console.WriteLine($"Mission Failed... If possible go open a ticket issue with your requested URL in : {urlIssues}");
            Console.WriteLine("In advance thank you for helping me improve this little exe :)");
        }
        #endregion

    }
}
