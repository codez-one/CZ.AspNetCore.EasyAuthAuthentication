namespace KK.AspNetCore.EasyAuthAuthentication.Test.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text;
    using KK.AspNetCore.EasyAuthAuthentication.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Newtonsoft.Json;
    using Xunit;
    using KK.AspNetCore.EasyAuthAuthentication.Services.Base;
    using KK.AspNetCore.EasyAuthAuthentication.Interfaces;
    using System.Linq;
    using KK.AspNetCore.EasyAuthAuthentication.Models;

    public class EasyAuthWithHeaderServiceTest
    {
        private readonly ILoggerFactory loggerFactory = new NullLoggerFactory();

        [Theory]
        [InlineData(typeof(EasyAuthAzureAdService), "aad", AuthTokenHeaderNames.AADIdToken)]
        [InlineData(typeof(EasyAuthTwitterService), "twitter", AuthTokenHeaderNames.TwitterAccessToken)]
        [InlineData(typeof(EasyAuthFacebookService), "facebook", AuthTokenHeaderNames.FacebookAccessToken)]
        [InlineData(typeof(EasyAuthGoogleService), "google", AuthTokenHeaderNames.GoogleIdToken)]
        [InlineData(typeof(EasyAuthMicrosoftService), "microsoftaccount", AuthTokenHeaderNames.MicrosoftAccessToken)]

        public void IfTheAADIdTokenHeaderIsSetTheCanUseMethodMustReturnTrue(Type authServiceType, string idpName, string requiredHeader)
        {
            // Arrange
            var handler = this.CreateServiceInstance(authServiceType);
            var httpcontext = new DefaultHttpContext();
            httpcontext.Request.Headers.Add(requiredHeader, "blup");
            httpcontext.Request.Headers.Add("X-MS-CLIENT-PRINCIPAL-IDP", idpName);
            // Act
            var result = handler.CanHandleAuthentification(httpcontext);
            // Arrange
            Assert.True(result);
        }



        [Theory]
        [InlineData(typeof(EasyAuthAzureAdService))]
        [InlineData(typeof(EasyAuthTwitterService))]
        [InlineData(typeof(EasyAuthFacebookService))]
        [InlineData(typeof(EasyAuthGoogleService))]
        [InlineData(typeof(EasyAuthMicrosoftService))]
        public void IfTheAuthorizationHeaderIsNotSetTheCanUseMethodMustReturnFalse(Type authServiceType)
        {
            // Arrange
            var handler = this.CreateServiceInstance(authServiceType);
            var httpcontext = new DefaultHttpContext();
            // Act
            var result = handler.CanHandleAuthentification(httpcontext);
            // Arrange
            Assert.False(result);
        }

        [Theory]
        [InlineData(typeof(EasyAuthAzureAdService), "aad", AuthTokenHeaderNames.AADIdToken)]
        [InlineData(typeof(EasyAuthTwitterService), "twitter", AuthTokenHeaderNames.TwitterAccessToken)]
        [InlineData(typeof(EasyAuthFacebookService), "facebook", AuthTokenHeaderNames.FacebookAccessToken)]
        [InlineData(typeof(EasyAuthGoogleService), "google", AuthTokenHeaderNames.GoogleIdToken)]
        [InlineData(typeof(EasyAuthMicrosoftService), "microsoftaccount", AuthTokenHeaderNames.MicrosoftAccessToken)]
        public void IfAValidJwtTokenIsInTheHeaderTheResultIsSuccsess(Type authServiceType, string idpName, string requiredHeader)
        {
            // Arrange
            var handler = this.CreateServiceInstance(authServiceType);
            var httpcontext = new DefaultHttpContext();
            var inputObject = new InputJson()
            {
                Claims = new List<InputClaims>()
                {
                    new InputClaims() {Typ=  "x", Value= "y"},
                    new InputClaims() {Typ=  "name", Value= "PrincipalName"},
                    new InputClaims() {Typ = ClaimTypes.Role, Value = "Admin"}
                }
            };
            var json = JsonConvert.SerializeObject(inputObject);
            httpcontext.Request.Headers.Add("X-MS-TOKEN-AAD-ID-TOKEN", "Blup");
            httpcontext.Request.Headers.Add("X-MS-CLIENT-PRINCIPAL-IDP", "providername");
            httpcontext.Request.Headers.Add("X-MS-CLIENT-PRINCIPAL", Base64Encode(json));
            // Act
            var result = handler.AuthUser(httpcontext);
            // Arrange
            Assert.True(result.Succeeded);
            Assert.Equal("PrincipalName", result.Principal.Identity.Name);
            Assert.True(result.Principal.IsInRole("Admin"));
        }

        [Theory]
        [InlineData(typeof(EasyAuthAzureAdService), "aad", AuthTokenHeaderNames.AADIdToken)]
        [InlineData(typeof(EasyAuthTwitterService), "twitter", AuthTokenHeaderNames.TwitterAccessToken)]
        [InlineData(typeof(EasyAuthFacebookService), "facebook", AuthTokenHeaderNames.FacebookAccessToken)]
        [InlineData(typeof(EasyAuthGoogleService), "google", AuthTokenHeaderNames.GoogleIdToken)]
        [InlineData(typeof(EasyAuthMicrosoftService), "microsoftaccount", AuthTokenHeaderNames.MicrosoftAccessToken)]
        public void IfACustomOptionsIsInTheHeaderTheResultIsSuccsess(Type authServiceType, string idpName, string requiredHeader)
        {
            // Arrange
            var handler = this.CreateServiceInstance(authServiceType);
            var httpcontext = new DefaultHttpContext();
            var inputObject = new InputJson()
            {
                Claims = new List<InputClaims>()
                {
                    new InputClaims() {Typ=  "x", Value= "y"},
                    new InputClaims() {Typ=  "name", Value= "PrincipalName"},
                    new InputClaims() {Typ = "hannes", Value = "Admin"}
                }
            };
            var json = JsonConvert.SerializeObject(inputObject);
            httpcontext.Request.Headers.Add("X-MS-TOKEN-AAD-ID-TOKEN", "Blup");
            httpcontext.Request.Headers.Add("X-MS-CLIENT-PRINCIPAL-IDP", "providername");
            httpcontext.Request.Headers.Add("X-MS-CLIENT-PRINCIPAL", Base64Encode(json));
            // Act
            var result = handler.AuthUser(httpcontext, new ProviderOptions(authServiceType.Name , nameClaimType: "name",roleNameClaimType: "hannes"));
            // Arrange
            Assert.True(result.Succeeded);
            Assert.Equal("PrincipalName", result.Principal.Identity.Name);
            Assert.True(result.Principal.IsInRole("Admin"));
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        internal class InputJson
        {
            [JsonProperty("claims")]
            public IEnumerable<InputClaims> Claims { get; set; }
        }

        internal class InputClaims
        {
            [JsonProperty("typ")]
            public string Typ { get; set; }
            [JsonProperty("val")]
            public string Value { get; set; }
        }

        private IEasyAuthAuthentificationService CreateServiceInstance(Type AuthServiceType)
        {
            var loggerMethod = typeof(LoggerFactoryExtensions).GetMethods()
                            .Where(m => m.Name == nameof(LoggerFactoryExtensions.CreateLogger) && m.IsGenericMethod == true)
                            .Single();

            var logger = loggerMethod
                    .MakeGenericMethod(AuthServiceType)
                    .Invoke(null, new object[] { this.loggerFactory });
            var ctorOfAuthService = AuthServiceType.GetConstructor(new[] { logger.GetType() });
            var handler = Activator.CreateInstance(AuthServiceType, new object[] { logger }) as IEasyAuthAuthentificationService;
            return handler;
        }
    }
}
