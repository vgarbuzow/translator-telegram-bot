using Newtonsoft.Json;
using ReceivedTranslated;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BotTranslator
{
    internal static class Translator
    {
        public static string GetTranslationText(Translations translation)
        {
            var request = CreateRequest(translation);
            try 
            { 
               var body = GetResponseBody(request).Result;
                var builder = new StringBuilder(body);
                builder.Replace("[", string.Empty).Replace("]", string.Empty);
                var translatedMessage = JsonConvert.DeserializeObject<TranslatedMessage>(builder.ToString());
                return translatedMessage.Translations.Text;
            }
            catch(AggregateException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

            
        }

        private static async Task<string> GetResponseBody(HttpRequestMessage request)
        {
            var client = new HttpClient();
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return body;
        }

        private static HttpRequestMessage CreateRequest(Translations translation)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(
                    ConfigurationManager.AppSettings.Get("TranslatorURI")
                      + translation.To),
                Headers =
                 {
                    { "x-rapidapi-key", ConfigurationManager.AppSettings.Get("x-rapidapi-key") },
                    { "x-rapidapi-host", ConfigurationManager.AppSettings.Get("x-rapidapi-host") },
                 },
                Content = new StringContent(CreateTextParameter(translation.Text))
                {
                    Headers =
                    {
                     ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };
            return request;
        }

        private static string CreateTextParameter(string userMessage)
        {
            var builder = new StringBuilder();
            builder.Append("[{\"Text\": \"");
            builder.Append(userMessage.Replace("\"", "\\\""));
            builder.Append("\"}]");
            return builder.ToString();
        }
    }
}



