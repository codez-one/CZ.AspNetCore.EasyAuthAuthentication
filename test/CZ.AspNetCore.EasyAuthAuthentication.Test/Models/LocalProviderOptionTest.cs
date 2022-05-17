namespace CZ.AspNetCore.EasyAuthAuthentication.Test.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using CZ.AspNetCore.EasyAuthAuthentication.Models;
    using CZ.AspNetCore.EasyAuthAuthentication.Services;
    using Xunit;

    public class LocalProviderOptionTest
    {
        [Fact]
        public void TestIfTheEmptyCtorCreatesEmptyStrings() // not null!
        {
            // Arrange
            // Act
            var options = new LocalProviderOption();
            // Assert
            Assert.Equal(string.Empty, options.AuthEndpoint);
            Assert.Equal(string.Empty, options.NameClaimType);
            Assert.Equal(string.Empty, options.RoleClaimType);
        }

        [Fact]
        public void ChangeNameClaimTypeIfItsNotAnEmptyString()
        {
            // Arrange
            var options = new LocalProviderOption();
            // Act
            options.ChangeModel(new LocalProviderOption()
            {
                NameClaimType = "not empty"
            });
            // assert
            Assert.Equal("not empty", options.NameClaimType);
        }
        [Fact]
        public void ChangeAuthEndpointIfItsNotAnEmptyString()
        {
            // Arrange
            var options = new LocalProviderOption();
            // Act
            options.ChangeModel(new LocalProviderOption()
            {
                AuthEndpoint = "not empty"
            });
            // assert
            Assert.Equal("not empty", options.AuthEndpoint);
        }
        [Fact]
        public void ChangeRoleClaimTypeIfItsNotAnEmptyString()
        {
            // Arrange
            var options = new LocalProviderOption();
            // Act
            options.ChangeModel(new LocalProviderOption()
            {
                RoleClaimType = "not empty"
            });
            // assert
            Assert.Equal("not empty", options.RoleClaimType);
        }
        [Fact]
        public void NothingChangegIfThereOnlyEmptyStrings()
        {
            // Arrange
            var options = new LocalProviderOption("endpoint", "claim", "role");
            // Act
            options.ChangeModel(new LocalProviderOption());
            // assert
            Assert.Equal("endpoint", options.AuthEndpoint);
            Assert.Equal("claim", options.NameClaimType);
            Assert.Equal("role", options.RoleClaimType);
        }

        [Fact]
        public void GetProviderOptionsForLocalAuthMeService()
        {
            // Arrange
            var options = new LocalProviderOption("endpoint", "claim", "role");
            // Act;
            var providerOptions = options.GetProviderOptions();
            // Assert
            Assert.Equal(typeof(LocalAuthMeService).Name, providerOptions.ProviderName);            
            Assert.Equal("claim", providerOptions.NameClaimType);
            Assert.Equal("role", providerOptions.RoleClaimType);
        }
    }
}
