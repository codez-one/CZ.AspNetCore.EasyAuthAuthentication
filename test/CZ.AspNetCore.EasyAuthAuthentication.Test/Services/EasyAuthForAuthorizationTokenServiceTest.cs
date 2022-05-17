namespace CZ.AspNetCore.EasyAuthAuthentication.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text;
    using CZ.AspNetCore.EasyAuthAuthentication.Interfaces;
    using CZ.AspNetCore.EasyAuthAuthentication.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    public class EasyAuthForAuthorizationTokenServiceTest
    {
        private readonly ILoggerFactory loggerFactory = new NullLoggerFactory();
        private readonly string testJwt = @"Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IkN0ZlFDOExlLThOc0M3b0MyelFrWnBjcmZPYyIsImtpZCI6IkN0ZlFDOExlLThOc0M3b0MyelFrWnBjcmZPYyJ9.eyJhdWQiOiIwN2Q2ZDE1YS1jZTg5LTQ4MmMtOTcxYi01NDMxYjc1MTkxNjciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9lOTgxZGNhZi05MTk3LTQ3N2YtYmQwNi0wZTU3MmIwYjMzNDcvIiwiaWF0IjoxNTYyNjk4ODc2LCJuYmYiOjE1NjI2OTg4NzYsImV4cCI6MTU2MjcwMjc3NiwiYWlvIjoiNDJaZ1lPQmZzRzd0ZEg1ZVBpSHZvNU9QdDdyT0J3QT0iLCJhcHBpZCI6ImQzMTViZmFmLTYzMDQtNGY5Zi04MjFjLTU0NmJkYzAwYjViMCIsImFwcGlkYWNyIjoiMSIsImlkcCI6Imh0dHBzOi8vc3RzLndpbmRvd3MubmV0L2U5ODFkY2FmLTkxOTctNDc3Zi1iZDA2LTBlNTcyYjBiMzM0Ny8iLCJvaWQiOiJmNWY5ZmE4Ni00NDE0LTQ0YzctODBmOC1mNzgwYWUwYWJmMjEiLCJyb2xlcyI6WyJTeXN0ZW1BZG1pbiJdLCJzdWIiOiJmNWY5ZmE4Ni00NDE0LTQ0YzctODBmOC1mNzgwYWUwYWJmMjEiLCJ0aWQiOiJlOTgxZGNhZi05MTk3LTQ3N2YtYmQwNi0wZTU3MmIwYjMzNDciLCJ1dGkiOiJoQ0p1M29oN3dVZVphaTVRSk9ZQUFBIiwidmVyIjoiMS4wIn0.fni_mCHCFWVXn1RtOvTWC5OgNU3xD_xyG38Bc1BfdcdoWP1p2N69HsW76rkk-IruDMdsiJaYKekK6RwUbBvbYii_S-fcT1FbdXCtYdFWm892Z4VGk8UFmS5HIApJd6WK4iHHwBv_R8n2juXyHKWfpZNaOldgaU0bRePSe3wu9_ZGOZ5et0_bbs1Y0UrwgaycZWwBSkah5s7fnLRBtMXmsEQlWPqnEzvwjieoqYs-YIndvau39ZE_0HT55kpbgE2HFJ2E62jyzJnMf60l8LlA_aXN6naNNm-SBepDoWVkUjZ-uQZbAdAr-MS-BIP4wS-1jrOvAfD6m7qOVcDaGYIv5A";
        private readonly string testJwtAppId = "d315bfaf-6304-4f9f-821c-546bdc00b5b0";

        [Fact]
        public void IfTheAuthorizationHeaderIsSetTheCanUseMethodMustReturnTrue()
        {
            // Arrange
            var handler = new EasyAuthForAuthorizationTokenService(this.loggerFactory.CreateLogger<EasyAuthForAuthorizationTokenService>()) as IEasyAuthAuthentificationService;
            var httpcontext = new DefaultHttpContext();
            httpcontext.Request.Headers.Add("Authorization", "Bearer sgölkfsögölfsg");
            // Act
            var result = handler.CanHandleAuthentification(httpcontext);
            // Arrange
            Assert.True(result);
        }

        [Fact]
        public void IfTheAuthorizationHeaderIsNotAJWTTokenTheCanUseMethodMustReturnFalse()
        {
            // Arrange
            var handler = new EasyAuthForAuthorizationTokenService(this.loggerFactory.CreateLogger<EasyAuthForAuthorizationTokenService>()) as IEasyAuthAuthentificationService;
            var httpcontext = new DefaultHttpContext();
            httpcontext.Request.Headers.Add("Authorization", "sgölkfsögölfsg");
            // Act
            var result = handler.CanHandleAuthentification(httpcontext);
            // Arrange
            Assert.False(result);
        }

        [Fact]
        public void IfTheAuthorizationHeaderIsNotSetTheCanUseMethodMustReturnFalse()
        {
            // Arrange
            var handler = new EasyAuthForAuthorizationTokenService(this.loggerFactory.CreateLogger<EasyAuthForAuthorizationTokenService>()) as IEasyAuthAuthentificationService;
            var httpcontext = new DefaultHttpContext();
            // Act
            var result = handler.CanHandleAuthentification(httpcontext);
            // Arrange
            Assert.False(result);
        }

        [Fact]
        public void IfAValidJwtTokenIsInTheHeaderTheResultIsSuccsess()
        {
            // Arrange
            var handler = new EasyAuthForAuthorizationTokenService(this.loggerFactory.CreateLogger<EasyAuthForAuthorizationTokenService>()) as IEasyAuthAuthentificationService;
            var httpcontext = new DefaultHttpContext();
            httpcontext.Request.Headers.Add("Authorization", this.testJwt);
            // Act
            var result = handler.AuthUser(httpcontext);
            // Arrange
            Assert.True(result.Succeeded);
            Assert.True(result.Principal.HasClaim(ClaimTypes.Role, "SystemAdmin"));
            Assert.Equal(this.testJwtAppId, result.Principal.Identity.Name);
        }

        [Fact]
        public void IfAValidJwtTokenWithoutIdpPropertyIsInTheHeaderTheResultIsSuccsess()
        {
            // Arrange
            var handler = new EasyAuthForAuthorizationTokenService(this.loggerFactory.CreateLogger<EasyAuthForAuthorizationTokenService>()) as IEasyAuthAuthentificationService;
            var httpcontext = new DefaultHttpContext();
            var jwtWithoutIdpProperty = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiIwN2Q2ZDE1YS1jZTg5LTQ4MmMtOTcxYi01NDMxYjc1MTkxNjciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9lOTgxZGNhZi05MTk3LTQ3N2YtYmQwNi0wZTU3MmIwYjMzNDcvIiwiaWF0IjoxNTYyNjk4ODc2LCJuYmYiOjE1NjI2OTg4NzYsImV4cCI6MTU2MjcwMjc3NiwiYWlvIjoiNDJaZ1lPQmZzRzd0ZEg1ZVBpSHZvNU9QdDdyT0J3QT0iLCJhcHBpZCI6ImQzMTViZmFmLTYzMDQtNGY5Zi04MjFjLTU0NmJkYzAwYjViMCIsImFwcGlkYWNyIjoiMSIsIm9pZCI6ImY1ZjlmYTg2LTQ0MTQtNDRjNy04MGY4LWY3ODBhZTBhYmYyMSIsInJvbGVzIjpbIlN5c3RlbUFkbWluIl0sInN1YiI6ImY1ZjlmYTg2LTQ0MTQtNDRjNy04MGY4LWY3ODBhZTBhYmYyMSIsInRpZCI6ImU5ODFkY2FmLTkxOTctNDc3Zi1iZDA2LTBlNTcyYjBiMzM0NyIsInV0aSI6ImhDSnUzb2g3d1VlWmFpNVFKT1lBQUEiLCJ2ZXIiOiIxLjAifQ.aBHe6c3INsMsQVlANkW9b-w2IhQaiQQIoEcWfobea5A";
            httpcontext.Request.Headers.Add("Authorization", jwtWithoutIdpProperty);
            // Act
            var result = handler.AuthUser(httpcontext);
            // Arrange
            Assert.True(result.Succeeded);
            Assert.True(result.Principal.HasClaim(ClaimTypes.Role, "SystemAdmin"));
            Assert.Equal(this.testJwtAppId, result.Principal.Identity.Name);
        }

        [Fact]
        public void IfAValidJwtTokenWithAnUpnPropertyIsInTheHeaderTheResultContainsTheUpnAsIdentity()
        {
            // Arrange
            var handler = new EasyAuthForAuthorizationTokenService(this.loggerFactory.CreateLogger<EasyAuthForAuthorizationTokenService>()) as IEasyAuthAuthentificationService;
            var httpcontext = new DefaultHttpContext();
            var jwtWithoutIdpProperty = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiIwN2Q2ZDE1YS1jZTg5LTQ4MmMtOTcxYi01NDMxYjc1MTkxNjciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9lOTgxZGNhZi05MTk3LTQ3N2YtYmQwNi0wZTU3MmIwYjMzNDcvIiwiaWF0IjoxNTYyNjk4ODc2LCJuYmYiOjE1NjI2OTg4NzYsImV4cCI6MTU2MjcwMjc3NiwiYWlvIjoiNDJaZ1lPQmZzRzd0ZEg1ZVBpSHZvNU9QdDdyT0J3QT0iLCJhcHBpZCI6ImQzMTViZmFmLTYzMDQtNGY5Zi04MjFjLTU0NmJkYzAwYjViMCIsInVwbiI6InRlc3R1c2VyQHRlc3QuZGUiLCJhcHBpZGFjciI6IjEiLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9lOTgxZGNhZi05MTk3LTQ3N2YtYmQwNi0wZTU3MmIwYjMzNDcvIiwib2lkIjoiZjVmOWZhODYtNDQxNC00NGM3LTgwZjgtZjc4MGFlMGFiZjIxIiwicm9sZXMiOlsiU3lzdGVtQWRtaW4iXSwic3ViIjoiZjVmOWZhODYtNDQxNC00NGM3LTgwZjgtZjc4MGFlMGFiZjIxIiwidGlkIjoiZTk4MWRjYWYtOTE5Ny00NzdmLWJkMDYtMGU1NzJiMGIzMzQ3IiwidXRpIjoiaENKdTNvaDd3VWVaYWk1UUpPWUFBQSIsInZlciI6IjEuMCJ9.T2vYwRaOFtISgoaMgg6XJ-pZEA5SOhqW09mF7TsGDBY";
            httpcontext.Request.Headers.Add("Authorization", jwtWithoutIdpProperty);
            // Act
            var result = handler.AuthUser(httpcontext);
            // Arrange
            Assert.True(result.Succeeded);
            Assert.True(result.Principal.HasClaim(ClaimTypes.Role, "SystemAdmin"));
            Assert.Equal("testuser@test.de", result.Principal.Identity.Name);
        }

        [Fact]
        public void IfAValidJwtTokenWithoutIdpAndIssPropertyIsInTheHeaderItsThrowsAnError()
        {
            // Arrange
            var handler = new EasyAuthForAuthorizationTokenService(this.loggerFactory.CreateLogger<EasyAuthForAuthorizationTokenService>()) as IEasyAuthAuthentificationService;
            var httpcontext = new DefaultHttpContext();
            var jwtWithoutIdpProperty = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiIwN2Q2ZDE1YS1jZTg5LTQ4MmMtOTcxYi01NDMxYjc1MTkxNjciLCJpYXQiOjE1NjI2OTg4NzYsIm5iZiI6MTU2MjY5ODg3NiwiZXhwIjoxNTYyNzAyNzc2LCJhaW8iOiI0MlpnWU9CZnNHN3RkSDVlUGlIdm81T1B0N3JPQndBPSIsImFwcGlkIjoiZDMxNWJmYWYtNjMwNC00ZjlmLTgyMWMtNTQ2YmRjMDBiNWIwIiwiYXBwaWRhY3IiOiIxIiwib2lkIjoiZjVmOWZhODYtNDQxNC00NGM3LTgwZjgtZjc4MGFlMGFiZjIxIiwicm9sZXMiOlsiU3lzdGVtQWRtaW4iXSwic3ViIjoiZjVmOWZhODYtNDQxNC00NGM3LTgwZjgtZjc4MGFlMGFiZjIxIiwidGlkIjoiZTk4MWRjYWYtOTE5Ny00NzdmLWJkMDYtMGU1NzJiMGIzMzQ3IiwidXRpIjoiaENKdTNvaDd3VWVaYWk1UUpPWUFBQSIsInZlciI6IjEuMCJ9.6hcHmq8VahVMqtvA9DJdoY-NIUjkPgMEfryGuLVJMHw";
            httpcontext.Request.Headers.Add("Authorization", jwtWithoutIdpProperty);
            // Act && Arrange
            _ = Assert.Throws<ArgumentException>(() => handler.AuthUser(httpcontext));
        }

        [Fact]
        public void IfAValidJwtTokenWithoutTheClaimPropertyIsInTheHeaderItsNotThrowAnError()
        {
            // Arrange
            var handler = new EasyAuthForAuthorizationTokenService(this.loggerFactory.CreateLogger<EasyAuthForAuthorizationTokenService>());
            var httpcontext = new DefaultHttpContext();
            var jwtWithoutIdpProperty = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiIwN2Q2ZDE1YS1jZTg5LTQ4MmMtOTcxYi01NDMxYjc1MTkxNjciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9lOTgxZGNhZi05MTk3LTQ3N2YtYmQwNi0wZTU3MmIwYjMzNDcvIiwiaWF0IjoxNTYyNjk4ODc2LCJuYmYiOjE1NjI2OTg4NzYsImV4cCI6MTU2MjcwMjc3NiwiYWlvIjoiNDJaZ1lPQmZzRzd0ZEg1ZVBpSHZvNU9QdDdyT0J3QT0iLCJhcHBpZCI6ImQzMTViZmFmLTYzMDQtNGY5Zi04MjFjLTU0NmJkYzAwYjViMCIsImFwcGlkYWNyIjoiMSIsIm9pZCI6ImY1ZjlmYTg2LTQ0MTQtNDRjNy04MGY4LWY3ODBhZTBhYmYyMSIsInN1YiI6ImY1ZjlmYTg2LTQ0MTQtNDRjNy04MGY4LWY3ODBhZTBhYmYyMSIsInRpZCI6ImU5ODFkY2FmLTkxOTctNDc3Zi1iZDA2LTBlNTcyYjBiMzM0NyIsInV0aSI6ImhDSnUzb2g3d1VlWmFpNVFKT1lBQUEiLCJ2ZXIiOiIxLjAifQ.HFVt3Moojs3G7J5CoqfJ8lDtxUf3SsO1bGb8_9O-314";
            httpcontext.Request.Headers.Add("Authorization", jwtWithoutIdpProperty);

            // Act
            var result = handler.AuthUser(httpcontext);
            // Arrange
            Assert.True(result.Succeeded);
            Assert.Equal(this.testJwtAppId, result.Principal.Identity.Name);
        }
    }
}
