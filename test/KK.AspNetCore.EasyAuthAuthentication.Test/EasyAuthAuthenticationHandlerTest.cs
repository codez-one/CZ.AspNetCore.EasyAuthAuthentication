namespace KK.AspNetCore.EasyAuthAuthentication.Test
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using KK.AspNetCore.EasyAuthAuthentication.Interfaces;
    using KK.AspNetCore.EasyAuthAuthentication.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.EnvironmentVariables;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.WebEncoders.Testing;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
    using Xunit;

    public class EasyAuthAuthenticationHandlerTest
    {
        private readonly IEnumerable<IEasyAuthAuthentificationService> providers = new List<IEasyAuthAuthentificationService>() { new TestProvider() };
        private readonly ILoggerFactory loggerFactory = new NullLoggerFactory();
        private readonly UrlEncoder urlEncoder = new UrlTestEncoder();
        private readonly ISystemClock clock = new SystemClock();

        [Fact]
        public async Task IfAnProviderIsEnabledUseEnabledProvider()
        {
            // Arrange
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_ENABLED", "True");
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_UNAUTHENTICATED_ACTION", "RedirectToLoginPage");            
            var configBuilder = new ConfigurationBuilder();
            _ = configBuilder.AddEnvironmentVariables();
            var config = configBuilder.Build();
            var options = new EasyAuthAuthenticationOptions();
            options.AddProviderOptions(new ProviderOptions("TestProvider") { Enabled = true });
            
            var services = new ServiceCollection().AddOptions()
                .AddSingleton<IOptionsFactory<EasyAuthAuthenticationOptions>, OptionsFactory<EasyAuthAuthenticationOptions>>()                
                .Configure<EasyAuthAuthenticationOptions>(EasyAuthAuthenticationDefaults.AuthenticationScheme , o => o.ProviderOptions = options.ProviderOptions)
                .BuildServiceProvider();
            var monitor = services.GetRequiredService<IOptionsMonitor<EasyAuthAuthenticationOptions>>();


            var handler = new EasyAuthAuthenticationHandler(monitor, this.providers, this.loggerFactory, this.urlEncoder, this.clock, config);
            var schema = new AuthenticationScheme(EasyAuthAuthenticationDefaults.AuthenticationScheme, EasyAuthAuthenticationDefaults.DisplayName, typeof(EasyAuthAuthenticationHandler));            
            var httpContext = new DefaultHttpContext();
            await handler.InitializeAsync(schema, httpContext);
            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            Assert.Equal("testName", result.Principal.Identity.Name);
            Assert.True(result.Succeeded);
            Assert.Equal("testType", result.Principal.Identity.AuthenticationType);
            Assert.True(result.Principal.Identity.IsAuthenticated);
        }

        [Fact]
        public async Task IfAnProviderIsdisabledSkipProvider()
        {
            // Arrange
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_ENABLED", "True");
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_UNAUTHENTICATED_ACTION", "RedirectToLoginPage");
            var configBuilder = new ConfigurationBuilder();
            _ = configBuilder.AddEnvironmentVariables();
            var config = configBuilder.Build();
            var options = new EasyAuthAuthenticationOptions();
            options.AddProviderOptions(new ProviderOptions("TestProvider") { Enabled = false });


            var services = new ServiceCollection().AddOptions()
                .AddSingleton<IOptionsFactory<EasyAuthAuthenticationOptions>, OptionsFactory<EasyAuthAuthenticationOptions>>()                
                .Configure<EasyAuthAuthenticationOptions>(EasyAuthAuthenticationDefaults.AuthenticationScheme, o => o.ProviderOptions = options.ProviderOptions)
                .BuildServiceProvider();
            var monitor = services.GetRequiredService<IOptionsMonitor<EasyAuthAuthenticationOptions>>();


            var handler = new EasyAuthAuthenticationHandler(monitor, this.providers, this.loggerFactory, this.urlEncoder, this.clock, config);
            var schema = new AuthenticationScheme(EasyAuthAuthenticationDefaults.AuthenticationScheme, EasyAuthAuthenticationDefaults.DisplayName, typeof(EasyAuthAuthenticationHandler));           
            var context = new DefaultHttpContext();
            // If this header is set the fallback with the local authme.json isn't used.
            context.Request.Headers.Add("X-MS-TOKEN-AAD-ID-TOKEN", "test");
            
            await handler.InitializeAsync(schema, context);

            // Act
            var result = await handler.AuthenticateAsync();
            // Assert
            Assert.False(result.Succeeded);            
        }
        [Fact]
        public async Task IfTheUserIsAlreadyAuthorizedTheAuthResultIsSuccess()
        {
            // Arrange
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_ENABLED", "True");
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_UNAUTHENTICATED_ACTION", "RedirectToLoginPage");
            var configBuilder = new ConfigurationBuilder();
            _ = configBuilder.AddEnvironmentVariables();
            var config = configBuilder.Build();
            var options = new EasyAuthAuthenticationOptions();
            options.AddProviderOptions(new ProviderOptions("TestProvider") { Enabled = false });


            var services = new ServiceCollection().AddOptions()
                .AddSingleton<IOptionsFactory<EasyAuthAuthenticationOptions>, OptionsFactory<EasyAuthAuthenticationOptions>>()
                .Configure<EasyAuthAuthenticationOptions>(EasyAuthAuthenticationDefaults.AuthenticationScheme, o => o.ProviderOptions = options.ProviderOptions)
                .BuildServiceProvider();
            var monitor = services.GetRequiredService<IOptionsMonitor<EasyAuthAuthenticationOptions>>();


            var handler = new EasyAuthAuthenticationHandler(monitor, this.providers, this.loggerFactory, this.urlEncoder, this.clock,config);
            var schema = new AuthenticationScheme(EasyAuthAuthenticationDefaults.AuthenticationScheme, EasyAuthAuthenticationDefaults.DisplayName, typeof(EasyAuthAuthenticationHandler));
            var context = new DefaultHttpContext();
            // If this header is set the fallback with the local authme.json isn't used.
            context.Request.Headers.Add("X-MS-TOKEN-AAD-ID-TOKEN", "test");
            var authResult = new TestProvider().AuthUser(context);
            context.User = authResult.Principal;
            // Act
            await handler.InitializeAsync(schema, context);
            var result = await handler.AuthenticateAsync();
            // Assert
            Assert.False(result.Succeeded);
            Assert.True(context.User.Identity.IsAuthenticated);
        }

        [Fact]
        public async Task DontCallTheFallBackIfTheRequestUrlEqualsTheAuthEndPoint()
        {
            // Arrange
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_ENABLED", "True");
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_UNAUTHENTICATED_ACTION", "RedirectToLoginPage");
            var configBuilder = new ConfigurationBuilder();
            _ = configBuilder.AddEnvironmentVariables();
            var config = configBuilder.Build();
            var options = new EasyAuthAuthenticationOptions();
            options.AddProviderOptions(new ProviderOptions("TestProvider") { Enabled = false });
            options.LocalProviderOption = new LocalProviderOption(".auth/me", string.Empty, string.Empty);


            var services = new ServiceCollection().AddOptions()
                .AddSingleton<IOptionsFactory<EasyAuthAuthenticationOptions>, OptionsFactory<EasyAuthAuthenticationOptions>>()
                .Configure<EasyAuthAuthenticationOptions>(EasyAuthAuthenticationDefaults.AuthenticationScheme, o => o.ProviderOptions = options.ProviderOptions)
                .BuildServiceProvider();
            var monitor = services.GetRequiredService<IOptionsMonitor<EasyAuthAuthenticationOptions>>();


            var handler = new EasyAuthAuthenticationHandler(monitor, this.providers, this.loggerFactory, this.urlEncoder, this.clock, config);
            var schema = new AuthenticationScheme(EasyAuthAuthenticationDefaults.AuthenticationScheme, EasyAuthAuthenticationDefaults.DisplayName, typeof(EasyAuthAuthenticationHandler));
            var context = new DefaultHttpContext();            
            
            context.Request.Path = "/" + options.LocalProviderOption.AuthEndpoint;
            // Act
            await handler.InitializeAsync(schema, context);
            var result = await handler.AuthenticateAsync();
            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task CallTheFallBackIfTheRequestUrlNotEqualsTheAuthEndPoint()
        {
            // Arrange
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_ENABLED", "True");
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_UNAUTHENTICATED_ACTION", "RedirectToLoginPage");
            var configBuilder = new ConfigurationBuilder();
            _ = configBuilder.AddEnvironmentVariables();
            var config = configBuilder.Build();
            var options = new EasyAuthAuthenticationOptions();
            options.AddProviderOptions(new ProviderOptions("TestProvider") { Enabled = false });


            var services = new ServiceCollection().AddOptions()
                .AddSingleton<IOptionsFactory<EasyAuthAuthenticationOptions>, OptionsFactory<EasyAuthAuthenticationOptions>>()
                .Configure<EasyAuthAuthenticationOptions>(EasyAuthAuthenticationDefaults.AuthenticationScheme, o => o.ProviderOptions = options.ProviderOptions)
                .BuildServiceProvider();
            var monitor = services.GetRequiredService<IOptionsMonitor<EasyAuthAuthenticationOptions>>();


            var handler = new EasyAuthAuthenticationHandler(monitor, this.providers, this.loggerFactory, this.urlEncoder, this.clock, config);
            var schema = new AuthenticationScheme(EasyAuthAuthenticationDefaults.AuthenticationScheme, EasyAuthAuthenticationDefaults.DisplayName, typeof(EasyAuthAuthenticationHandler));
            var context = new DefaultHttpContext();

            context.Request.Path = string.Empty;
            // Act
            await handler.InitializeAsync(schema, context);
            var result = await handler.AuthenticateAsync();
            // Assert
            Assert.False(result.Succeeded);
            Assert.True(result.None);
        }

        [Fact]
        public async Task DontCallAProviderIfNotProviderIsRegistered()
        {
            // Arrange
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_ENABLED", "True");
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_UNAUTHENTICATED_ACTION", "RedirectToLoginPage");
            var configBuilder = new ConfigurationBuilder();
            _ = configBuilder.AddEnvironmentVariables();
            var config = configBuilder.Build();
            var options = new EasyAuthAuthenticationOptions();
            options.AddProviderOptions(new ProviderOptions("TestProvider") { Enabled = false });


            var services = new ServiceCollection().AddOptions()
                .AddSingleton<IOptionsFactory<EasyAuthAuthenticationOptions>, OptionsFactory<EasyAuthAuthenticationOptions>>()
                .Configure<EasyAuthAuthenticationOptions>(EasyAuthAuthenticationDefaults.AuthenticationScheme, o => o.ProviderOptions = options.ProviderOptions)
                .BuildServiceProvider();
            var monitor = services.GetRequiredService<IOptionsMonitor<EasyAuthAuthenticationOptions>>();


            var handler = new EasyAuthAuthenticationHandler(monitor, new List<IEasyAuthAuthentificationService>(), this.loggerFactory, this.urlEncoder, this.clock, config);
            var schema = new AuthenticationScheme(EasyAuthAuthenticationDefaults.AuthenticationScheme, EasyAuthAuthenticationDefaults.DisplayName, typeof(EasyAuthAuthenticationHandler));
            var context = new DefaultHttpContext();            
            // Act
            await handler.InitializeAsync(schema, context);
            var result = await handler.AuthenticateAsync();
            // Assert
            Assert.False(result.Succeeded);            
        }

        [Fact]
        public void ErrorIfTheAuthIsEnabledButAnonymousRequestsAreAllowed()
        {
            // Arrange
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_ENABLED", "True");
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_UNAUTHENTICATED_ACTION", "AllowAnonymous");
            var configBuilder = new ConfigurationBuilder();
            _ = configBuilder.AddEnvironmentVariables();
            var config = configBuilder.Build();
            var options = new EasyAuthAuthenticationOptions();
            options.AddProviderOptions(new ProviderOptions("TestProvider") { Enabled = false });


            var services = new ServiceCollection().AddOptions()
                .AddSingleton<IOptionsFactory<EasyAuthAuthenticationOptions>, OptionsFactory<EasyAuthAuthenticationOptions>>()
                .Configure<EasyAuthAuthenticationOptions>(EasyAuthAuthenticationDefaults.AuthenticationScheme, o => o.ProviderOptions = options.ProviderOptions)
                .BuildServiceProvider();
            var monitor = services.GetRequiredService<IOptionsMonitor<EasyAuthAuthenticationOptions>>();

            _ = Assert.Throws<ArgumentException>(() => new EasyAuthAuthenticationHandler(monitor, new List<IEasyAuthAuthentificationService>(), this.loggerFactory, this.urlEncoder, this.clock, config));           
        }

        [Fact]
        public async Task UseEasyAuthProviderIfAuthIsDisabled()
        {
            // Arrange
            System.Environment.SetEnvironmentVariable("APPSETTING_WEBSITE_AUTH_ENABLED", "False");            
            var configBuilder = new ConfigurationBuilder();
            _ = configBuilder.AddEnvironmentVariables();
            var config = configBuilder.Build();
            var options = new EasyAuthAuthenticationOptions();
            options.AddProviderOptions(new ProviderOptions("TestProvider") { Enabled = false });


            var services = new ServiceCollection().AddOptions()
                .AddSingleton<IOptionsFactory<EasyAuthAuthenticationOptions>, OptionsFactory<EasyAuthAuthenticationOptions>>()
                .Configure<EasyAuthAuthenticationOptions>(EasyAuthAuthenticationDefaults.AuthenticationScheme, o => o.ProviderOptions = options.ProviderOptions)
                .BuildServiceProvider();
            var monitor = services.GetRequiredService<IOptionsMonitor<EasyAuthAuthenticationOptions>>();

            var handler = new EasyAuthAuthenticationHandler(monitor, new List<IEasyAuthAuthentificationService>(), this.loggerFactory, this.urlEncoder, this.clock, config);
            var schema = new AuthenticationScheme(EasyAuthAuthenticationDefaults.AuthenticationScheme, EasyAuthAuthenticationDefaults.DisplayName, typeof(EasyAuthAuthenticationHandler));
            var context = new DefaultHttpContext();
            // Act
            await handler.InitializeAsync(schema, context);
            var result = await handler.AuthenticateAsync();
            // Assert
            Assert.False(result.Succeeded); // The EasyAuth me service is currently hard to test, so we only can check if it's fails
            Assert.NotNull(result.Failure);
            Assert.Equal("An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.", result.Failure.Message);
        }

        private class TestProvider : IEasyAuthAuthentificationService
        {
            public AuthenticateResult AuthUser(HttpContext context) => this.AuthUser(context, null);
            public AuthenticateResult AuthUser(HttpContext context, ProviderOptions options)
            {
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, "testName")
                };
                var roleClaim = options == null ? ClaimTypes.Role : options.RoleClaimType;
                var identity =  new ClaimsIdentity(
                            claims,
                            "testType",
                            ClaimTypes.NameIdentifier,
                            roleClaim
                        );
                return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), EasyAuthAuthenticationDefaults.AuthenticationScheme));
            }

            public bool CanHandleAuthentification(HttpContext httpContext) => true;

            bool IEasyAuthAuthentificationService.CanHandleAuthentification(HttpContext httpContext) => this.CanHandleAuthentification(httpContext);
            AuthenticateResult IEasyAuthAuthentificationService.AuthUser(HttpContext context) => this.AuthUser(context);
            AuthenticateResult IEasyAuthAuthentificationService.AuthUser(HttpContext context, ProviderOptions options) => this.AuthUser(context, options);
        }
    }
}
