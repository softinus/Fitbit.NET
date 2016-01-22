using Fitbit.Api;
using Fitbit.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SampleDesktop
{
    class Program
    {
        const string consumerKey = "1aade6a54fb93f3a7f6542bb00a775be";
        const string consumerSecret = "56e3b2872f7a97bad0c5686dc2db44e9";
        
        static void Main(string[] args)
        {
            //Example of getting the Auth credentials for the first time by directoring the
            //user to the fitbit site to get a PIN. 

            var credentials = LoadCredentials();

            if (credentials == null)
            {
                credentials = Authenticate();
                SaveCredentials(credentials);
            }

            var fitbit = new FitbitClient(consumerKey, consumerSecret, credentials.AuthToken, credentials.AuthTokenSecret);
           
            var profile = fitbit.GetUserProfile("3LKBDR");
            Console.WriteLine("표시이름 : {0}", profile.DisplayName);
            Console.WriteLine("몸무게 : {0}", profile.Weight);
            Console.WriteLine("이름 : {0}", profile.FullName);
            Console.WriteLine("성별 : {0}", profile.Gender);
            Console.WriteLine("키 : {0}{1}", profile.Height, profile.HeightUnit);
            Console.WriteLine("가입일자 : {0}", profile.MemberSince);

            //fitbit.GetHeartRates(new DateTime(2015, 7, 1));
            //List<UserProfile> listFriends= fitbit.GetFriends();
            //foreach(UserProfile UP in listFriends)
            //{
            //    Console.WriteLine("친구{0}의 이름 : {1}", 1, UP.DisplayName);
            //}
            

            Console.ReadLine();
        }

        static AuthCredential Authenticate()
        {
            var requestTokenUrl = "http://api.fitbit.com/oauth/request_token";
            var accessTokenUrl = "http://api.fitbit.com/oauth/access_token";
            var authorizeUrl = "http://www.fitbit.com/oauth/authorize";

            var a = new Authenticator(consumerKey, consumerSecret, requestTokenUrl, accessTokenUrl, authorizeUrl);

            RequestToken token = a.GetRequestToken();

            var url = a.GenerateAuthUrlFromRequestToken(token, false);

            Process.Start(url);

            Console.WriteLine("Enter the verification code from the website");
            var pin = Console.ReadLine();

            var credentials = a.GetAuthCredentialFromPin(pin, token);
            return credentials;
        }

        static void SaveCredentials(AuthCredential credentials)
        {
            try
            {
                var path = GetAppDataPath();
                var serializer = new XmlSerializer(typeof(AuthCredential));
                TextWriter writer = new StreamWriter(path);
                serializer.Serialize(writer, credentials);
                writer.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static AuthCredential LoadCredentials()
        {
            AuthCredential credentials = null;
            try
            {
                var path = GetAppDataPath();

                if (File.Exists(path))
                {
                    var serializer = new XmlSerializer(typeof(AuthCredential));
                    FileStream fs = new FileStream(path, FileMode.Open);

                    credentials = serializer.Deserialize(fs) as AuthCredential;
                    fs.Close();
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return credentials;
        }

        static string GetAppDataPath()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fitbit");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Path.Combine(path, "Credentials.xml");
        }
    }
}
