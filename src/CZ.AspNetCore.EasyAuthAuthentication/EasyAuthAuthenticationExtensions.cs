namespace CZ.AspNetCore.EasyAuthAuthentication
{
    using System;
    using System.Linq;
    using CZ.AspNetCore.EasyAuthAuthentication.Interfaces;
    using CZ.AspNetCore.EasyAuthAuthentication.Models;
    using CZ.AspNetCore.EasyAuthAuthentication.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension methods for <see cref="AuthenticationBuilder"/> to add the <see cref="EasyAuthAuthenticationHandler"/> to the pipeline.
    /// </summary>
    public static class EasyAuthAuthenticationExtensions
    {
        /// <summary>
        /// Adds the <see cref="EasyAuthAuthenticationHandler"/> for authentication.
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder)
            => builder.AddEasyAuth(EasyAuthAuthenticationDefaults.AuthenticationScheme);

        /// <summary>
        /// Adds the <see cref="EasyAuthAuthenticationHandler"/> for authentication.
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        /// <param name="authenticationScheme">The schema for the Easy Auth handler.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, string authenticationScheme)
            => builder.AddEasyAuth(authenticationScheme, configureOptions: (d) => { });

        /// <summary>
        /// Adds the <see cref="EasyAuthAuthenticationHandler"/> for authentication.
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        /// <param name="configureOptions">A callback to configure <see cref="EasyAuthAuthenticationOptions"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, Action<EasyAuthAuthenticationOptions> configureOptions)
            => builder.AddEasyAuth(EasyAuthAuthenticationDefaults.AuthenticationScheme, configureOptions);

        /// <summary>
        /// Adds the <see cref="EasyAuthAuthenticationHandler"/> for authentication.
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        /// <param name="authenticationScheme">The schema for the Easy Auth handler.</param>
        /// <param name="configureOptions">A callback to configure <see cref="EasyAuthAuthenticationOptions"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, string authenticationScheme, Action<EasyAuthAuthenticationOptions> configureOptions)
            => builder.AddEasyAuth(authenticationScheme, displayName: EasyAuthAuthenticationDefaults.DisplayName, configureOptions: configureOptions);
        /// <summary>
        /// Adds the <see cref="EasyAuthAuthenticationHandler"/> for authentication.
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        /// <param name="configuration">The configuration object of the application.</param>        
        /// <param name="displayName">The display name for the Easy Auth handler.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddEasyAuth(
            this AuthenticationBuilder builder,
            IConfiguration configuration,
            string displayName) => AddEasyAuth(builder, configuration, EasyAuthAuthenticationDefaults.AuthenticationScheme, displayName);

        /// <summary>
        /// Adds the <see cref="EasyAuthAuthenticationHandler"/> for authentication.
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        /// <param name="configuration">The configuration object of the application.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddEasyAuth(
            this AuthenticationBuilder builder,
            IConfiguration configuration) => AddEasyAuth(builder, configuration, EasyAuthAuthenticationDefaults.AuthenticationScheme, EasyAuthAuthenticationDefaults.DisplayName);

        /// <summary>
        /// Adds the <see cref="EasyAuthAuthenticationHandler"/> for authentication.
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        /// <param name="configuration">The configuration object of the application.</param>
        /// <param name="authenticationScheme">The schema for the Easy Auth handler.</param>
        /// <param name="displayName">The display name for the Easy Auth handler.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddEasyAuth(
            this AuthenticationBuilder builder,
            IConfiguration configuration,
            string authenticationScheme,
            string displayName)
        {
            var options = new EasyAuthAuthenticationOptions
            {                
                ProviderOptions = configuration
                    .GetSection("easyAuthOptions:providerOptions")
                    .GetChildren()
                    .Select(d =>
                    {
                        var name = d.GetValue<string>("ProviderName");
                        var providerOptions = new ProviderOptions(name);
                        d.Bind(providerOptions);
                        return providerOptions;
                    }).ToList(),
                LocalProviderOption = configuration
                    .GetSection("easyAuthOptions:localProviderOption")
                    .Get<LocalProviderOption>()
                    
            };            
            return builder.AddEasyAuth(authenticationScheme, displayName, o =>
            {
                o.LocalProviderOption = options.LocalProviderOption;
                o.ProviderOptions = options.ProviderOptions;
            });
        }

        /// <summary>
        /// Adds the <see cref="EasyAuthAuthenticationHandler"/> for authentication.
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        /// <param name="authenticationScheme">The schema for the Easy Auth handler.</param>
        /// <param name="displayName">The display name for the Easy Auth handler.</param>
        /// <param name="configureOptions">A callback to configure <see cref="EasyAuthAuthenticationOptions"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddEasyAuth(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            string displayName,
            Action<EasyAuthAuthenticationOptions> configureOptions)
        {
            var allAuthServicesToRegister = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(IEasyAuthAuthentificationService).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
            foreach (var authService in allAuthServicesToRegister)
            {
                _ = builder.Services.AddSingleton(typeof(IEasyAuthAuthentificationService), authService);
            }

            return builder
                .AddScheme<EasyAuthAuthenticationOptions, EasyAuthAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
