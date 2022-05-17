namespace CZ.AspNetCore.EasyAuthAuthentication
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Security.Claims;
    using CZ.AspNetCore.EasyAuthAuthentication.Models;
    using Microsoft.AspNetCore.Authentication;

    internal static class AuthenticationTicketBuilder
    {
        /// <summary>
        /// Build a `AuthenticationTicket` from the given payload, the principal name and the provider name.
        /// </summary>
        /// <param name="claimsPayload">A array of JObjects that have a `type` and a `val` property.</param>
        /// <param name="providerName">The provider name of the current auth provider.</param>
        /// <param name="options">The <c>EasyAuthAuthenticationOptions</c> to use.</param>
        /// <returns>A `AuthenticationTicket`.</returns>
        public static AuthenticationTicket Build(IEnumerable<AADClaimsModel> claimsPayload, string? providerName, ProviderOptions options)
        {
            // setting ClaimsIdentity.AuthenticationType to value that Azure AD non-EasyAuth setups use
            var identity = new ClaimsIdentity(
                            CreateClaims(claimsPayload, options),
                            AuthenticationTypesNames.Federation,
                            ClaimTypes.Name,
                            ClaimTypes.Role
                        );
            AddScopeClaim(identity);
            AddProviderNameClaim(identity, providerName);
            var genericPrincipal = new ClaimsPrincipal(identity);
            return new AuthenticationTicket(genericPrincipal, EasyAuthAuthenticationDefaults.AuthenticationScheme);
        }

        private static IEnumerable<Claim> CreateClaims(IEnumerable<AADClaimsModel> claimsAsJson, ProviderOptions options)
        {
            foreach (var claim in claimsAsJson)
            {
                var claimType = claim.Typ;
                if (claimType == Schemas.AuthMethod)
                {
                    yield return new Claim(ClaimTypes.Authentication, claim.Values);
                }
                else if (claimType == options.RoleClaimType)
                {
                    yield return new Claim(ClaimTypes.Role, claim.Values);
                }
                else if (claimType == options.NameClaimType)
                {
                    yield return new Claim(ClaimTypes.Name, claim.Values);
                }
                else
                {
                    yield return new Claim(claimType, claim.Values);
                }
            }
        }

        private static void AddScopeClaim(ClaimsIdentity identity)
        {
            if (!identity.Claims.Any(claim => claim.Type == "scp"))
            {
                // We are not sure if we should add this in to match what non-EasyAuth authenticated result would look like
                // with EasyAuth + Express based application configurations the scope claim will always be `user_impersonation`
                identity.AddClaim(new Claim("scp", "user_impersonation"));
            }
        }

        private static void AddProviderNameClaim(ClaimsIdentity identity, string? providerName)
        {
            if (providerName == null)
            {
                identity.AddClaim(new Claim("provider_name", "unknow"));
            }
            else
            {
                if (!identity.Claims.Any(claim => claim.Type == "provider_name"))
                {
                    identity.AddClaim(new Claim("provider_name", providerName));
                }
            }
        }
    }
}
