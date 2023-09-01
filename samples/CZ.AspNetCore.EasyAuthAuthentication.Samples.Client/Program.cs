namespace CZ.AspNetCore.EasyAuthAuthentication.Samples.Webs.Client
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;

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
            var authContext = ConfidentialClientApplicationBuilder
                .Create(Environment.GetEnvironmentVariable("clientId"))
                .WithAuthority(Environment.GetEnvironmentVariable("authority"))
                .WithClientSecret(Environment.GetEnvironmentVariable("clientSecret"))
                .Build();
            var token = await authContext.AcquireTokenForClient(new string[] { $"{Environment.GetEnvironmentVariable("resource")}/.default" }).ExecuteAsync();
            Console.WriteLine("Bearer " + token.AccessToken);
            var headerName = "Authorization";
            var httpRequest = WebRequest.Create("https://sampleappauth.azurewebsites.net/api/SampleData/UserName");
            httpRequest.Headers.Add(headerName, "Bearer " + token.AccessToken);
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
