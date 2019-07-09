namespace KK.AspNetCore.EasyAuthAuthentication.Services
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text;
    using KK.AspNetCore.EasyAuthAuthentication.Interfaces;
    using KK.AspNetCore.EasyAuthAuthentication.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class EasyAuthWithHeaderService : IEasyAuthAuthentificationService
    {
        private const string PrincipalObjectHeader = "X-MS-CLIENT-PRINCIPAL";
        private const string PrincipalIdpHeaderName = "X-MS-CLIENT-PRINCIPAL-IDP";

        private static readonly Func<ClaimsPrincipal, bool> IsContextUserNotAuthenticated =
            user => user == null || user.Identity == null || user.Identity.IsAuthenticated == false;

        private static readonly Func<IHeaderDictionary, string, bool> IsHeaderSet =
            (headers, headerName) => !string.IsNullOrEmpty(headers[headerName].ToString());

        private readonly ProviderOptions defaultOptions = new ProviderOptions(typeof(EasyAuthWithHeaderService).Name)
        {
            NameClaimType = ClaimTypes.Email
        };

        public EasyAuthWithHeaderService(
            ILogger<EasyAuthWithHeaderService> logger) => this.Logger = logger;

        private ILogger Logger { get; }

        public bool CanHandleAuthentification(HttpContext httpContext)
        {
            var headers = httpContext.Request.Headers;
            var user = httpContext.User;
            return IsContextUserNotAuthenticated(user) &&
                IsHeaderSet(headers, AuthTokenHeaderNames.AADIdToken);
        }

        /// <summary>
        /// build up identity from X-MS-TOKEN-AAD-ID-TOKEN header set by EasyAuth filters if user openId connect session cookie or oauth bearer token authenticated ...
        /// </summary>
        /// <param name="context">Http context of the request.</param>
        /// <param name="options">The <c>EasyAuthAuthenticationOptions</c> to use.</param>
        /// <returns>An <see cref="AuthenticateResult" />.</returns>
        public AuthenticateResult AuthUser(HttpContext context, ProviderOptions options = null)
        {
            this.defaultOptions.ChangeModel(options);

            var ticket = this.BuildIdentityFromEasyAuthRequestHeaders(context.Request.Headers, this.defaultOptions);

            this.Logger.LogInformation("Set identity to user context object.");
            context.User = ticket.Principal;
            this.Logger.LogInformation("identity build was a success, returning ticket");

            return AuthenticateResult.Success(ticket);
        }

        private AuthenticationTicket BuildIdentityFromEasyAuthRequestHeaders(IHeaderDictionary headers, ProviderOptions options)
        {
            var providerName = headers[PrincipalIdpHeaderName][0];
            this.Logger.LogDebug($"payload was fetched from easyauth me json, provider: {providerName}");
            var headerContent = headers[PrincipalObjectHeader][0];
            this.Logger.LogInformation("building claims from payload...");
            var xMsClientPrincipal = JObject.Parse(
                        Encoding.UTF8.GetString(
                            Convert.FromBase64String(headerContent)
                        )
                    );
            var claims = JsonConvert.DeserializeObject<IEnumerable<AADClaimsModel>>(xMsClientPrincipal["claims"].ToString());
            return AuthenticationTicketBuilder.Build(claims, providerName, options);
        }
    }
}
