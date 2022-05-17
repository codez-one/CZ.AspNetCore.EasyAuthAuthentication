namespace CZ.AspNetCore.EasyAuthAuthentication.Samples.Webs.Client
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public class Program
    {
        private static void Main(string[] args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            Work().GetAwaiter().GetResult();
        }

        private static async Task Work()
        {
            var authContext = new AuthenticationContext(System.Environment.GetEnvironmentVariable("authority"));
            var credentials = new ClientCredential(System.Environment.GetEnvironmentVariable("clientId"), System.Environment.GetEnvironmentVariable("clientSecret"));
            var token = await authContext.AcquireTokenAsync(System.Environment.GetEnvironmentVariable("resource"), credentials);
            Console.WriteLine(token.AccessTokenType + " " + token.AccessToken);
            var headerName = "Authorization";
            var httpRequest = WebRequest.Create("https://sampleappauth.azurewebsites.net/api/SampleData/UserName");
            httpRequest.Headers.Add(headerName, token.AccessTokenType + " " + token.AccessToken);
            var response = httpRequest.GetResponse();
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream);
            var stringBuilder = new StringBuilder();
            while(reader.Peek() >= 0)
            {
                _ = stringBuilder.Append(reader.ReadLine());
            }
            reader.Dispose();
            Console.WriteLine(stringBuilder.ToString());
            
        }
    }
}
