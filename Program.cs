using System;
using System.Net;

namespace Suctionator
{
    class Program
    {
        private static string urlInput;

        static void Main(string[] args)
        {
            urlInput = String.Empty;
            Console.WriteLine("Bienvenue sur Suctionator !");

        Start:

            AskInput();

            if (CheckUrl(urlInput)) //GET OK
            {
                Console.WriteLine("Je tape les Pick-Ups !!!!!!!!");
            }
            else
            {
                WrongInput();
                goto Start;
            }
        }


        #region Brain

        /// <summary>
        /// Request the target url to see if it's online
        /// </summary>
        /// <param name="urlInput"></param>
        /// <returns></returns>
        private static Boolean CheckUrl(string urlInput)
        {
            Uri urlCheck = new Uri(urlInput);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlCheck);
            request.Timeout = 15000;

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region WriteLine
        private static void AskInput()
        {
            Console.WriteLine("Merci d'indiquer l'url du média souhaité :");
            urlInput = Console.ReadLine();
        }

        private static void WrongInput()
        {
            Console.WriteLine("L'url n'est pas valide !");
        }
        #endregion

    }
}
