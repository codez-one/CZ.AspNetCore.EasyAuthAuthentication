namespace CZ.AspNetCore.EasyAuthAuthentication.Services
{
    using System.Security.Claims;
    using CZ.AspNetCore.EasyAuthAuthentication.Interfaces;
    using CZ.AspNetCore.EasyAuthAuthentication.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using CZ.AspNetCore.EasyAuthAuthentication.Services.Base;

    public class EasyAuthAzureAdService: EasyAuthWithHeaderService<EasyAuthAzureAdService>, IEasyAuthAuthentificationService
    {
        private readonly ILogger<EasyAuthAzureAdService> logger;

        public EasyAuthAzureAdService(ILogger<EasyAuthAzureAdService> logger) : base(logger)
        {
            this.logger = logger;
            this.defaultOptions = new ProviderOptions(typeof(EasyAuthAzureAdService).Name, "name", ClaimTypes.Role);
        }

        private new AuthenticateResult AuthUser(HttpContext context)
        {
            this.logger.LogInformation("Try authentification with azure ad account.");
            return base.AuthUser(context);
        }

        private new AuthenticateResult AuthUser(HttpContext context, ProviderOptions options)
        {
            this.logger.LogInformation("Try authentification with azure ad account.");
            return base.AuthUser(context, options);
        }

        private new bool CanHandleAuthentification(HttpContext httpContext) => base.CanHandleAuthentification(httpContext) && httpContext.Request.Headers[PrincipalIdpHeaderName] == "aad" && IsHeaderSet(httpContext.Request.Headers, AuthTokenHeaderNames.AADIdToken);


        bool IEasyAuthAuthentificationService.CanHandleAuthentification(HttpContext httpContext) => this.CanHandleAuthentification(httpContext);
        AuthenticateResult IEasyAuthAuthentificationService.AuthUser(HttpContext context) => this.AuthUser(context);
        AuthenticateResult IEasyAuthAuthentificationService.AuthUser(HttpContext context, ProviderOptions options) => this.AuthUser(context, options);
    }
}
