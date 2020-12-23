namespace KK.AspNetCore.EasyAuthAuthentication.Services
{
    using System.Security.Claims;
    using KK.AspNetCore.EasyAuthAuthentication.Interfaces;
    using KK.AspNetCore.EasyAuthAuthentication.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using KK.AspNetCore.EasyAuthAuthentication.Services.Base;

    public class EasyAuthGoogleService : EasyAuthWithHeaderService<EasyAuthGoogleService>, IEasyAuthAuthentificationService
    {
        private readonly ILogger<EasyAuthGoogleService> logger;
        public EasyAuthGoogleService(ILogger<EasyAuthGoogleService> logger) : base(logger)
        {
            this.logger = logger;
            this.defaultOptions = new ProviderOptions(typeof(EasyAuthGoogleService).Name, "name", ClaimTypes.Role);
        }

        public new AuthenticateResult AuthUser(HttpContext context)
        {
            this.logger.LogInformation("Try authentification with google account.");
            return base.AuthUser(context);
        }

        public new AuthenticateResult AuthUser(HttpContext context, ProviderOptions options)
        {
            this.logger.LogInformation("Try authentification with google account.");
            return base.AuthUser(context, options);
        }

        public new bool CanHandleAuthentification(HttpContext httpContext) => base.CanHandleAuthentification(httpContext) && httpContext.Request.Headers[PrincipalIdpHeaderName] == "google" && IsHeaderSet(httpContext.Request.Headers, AuthTokenHeaderNames.GoogleIdToken);
    }
}
