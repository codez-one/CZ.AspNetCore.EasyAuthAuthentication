namespace CZ.AspNetCore.EasyAuthAuthentication
{
    /// <summary>
    /// This class contains all header names that are possible to make an authentication.
    /// The source of the list can find here: https://docs.microsoft.com/en-us/azure/app-service/app-service-authentication-how-to#retrieve-tokens-in-app-code .
    /// </summary>
    internal static class AuthTokenHeaderNames
    {
        // AzureAd
        public const string AADIdToken = "X-MS-TOKEN-AAD-ID-TOKEN";
        public const string AADAccessToken = "X-MS-TOKEN-AAD-ACCESS-TOKEN";
        public const string AADExpiresOn = "X-MS-TOKEN-AAD-EXPIRES-ON";
        public const string AADRefreshToken = "X-MS-TOKEN-AAD-REFRESH-TOKEN";

        // Facebook
        public const string FacebookAccessToken = "X-MS-TOKEN-FACEBOOK-ACCESS-TOKEN";
        public const string FacebookExpiresOn = "X-MS-TOKEN-FACEBOOK-EXPIRES-ON";

        // Google
        public const string GoogleIdToken = "X-MS-TOKEN-GOOGLE-ID-TOKEN";
        public const string GoogleAccessToken = "X-MS-TOKEN-GOOGLE-ACCESS-TOKEN";
        public const string GoogleExpiresOn = "X-MS-TOKEN-GOOGLE-EXPIRES-ON";
        public const string GoogleRefreshToken = "X-MS-TOKEN-GOOGLE-REFRESH-TOKEN";

        // Microsoft Account
        public const string MicrosoftAccessToken = "X-MS-TOKEN-MICROSOFTACCOUNT-ACCESS-TOKEN";
        public const string MicrosoftExpiresOn = "X-MS-TOKEN-MICROSOFTACCOUNT-EXPIRES-ON";
        public const string MicrosoftAuthenticationToken = "X-MS-TOKEN-MICROSOFTACCOUNT-AUTHENTICATION-TOKEN";
        public const string MicrosoftRefreshToken = "X-MS-TOKEN-MICROSOFTACCOUNT-REFRESH-TOKEN";

        // Twitter
        public const string TwitterAccessToken = "X-MS-TOKEN-TWITTER-ACCESS-TOKEN";
        public const string TwitterAccessTokenSecret = "X-MS-TOKEN-TWITTER-ACCESS-TOKEN-SECRET";
    }
}
