﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Indice.AspNetCore.Identity.Models;
using Indice.AspNetCore.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Indice.Common.Tests
{
    public class UserNameAsPasswordValidatorTests
    {
        [Theory]
        [InlineData("gmanoltzas")]
        [InlineData("gman")]
        [InlineData("mano")]
        [InlineData("anol")]
        [InlineData("nolt")]
        [InlineData("oltz")]
        [InlineData("ltza")]
        [InlineData("tzas")]
        public async Task CheckInvalidUserNames(string password) {
            const string UserName = "gmanoltzas";
            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>($"{nameof(PasswordOptions)}:{nameof(UserNameAsPasswordValidator<User>.MaxAllowedUserNameSubset)}", "3")
            });
            var validator = new UserNameAsPasswordValidator<User>(configurationBuilder.Build());
            var identityResult = await validator.ValidateAsync(null, new User { UserName = UserName }, password);
            Assert.False(identityResult.Succeeded);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("dummy")]
        [InlineData("gma")]
        [InlineData("man")]
        [InlineData("ano")]
        [InlineData("nol")]
        [InlineData("olt")]
        [InlineData("ltz")]
        [InlineData("tza")]
        [InlineData("zas")]
        public async Task CheckValidUserNames(string password) {
            const string UserName = "gmanoltzas";
            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>($"{nameof(PasswordOptions)}:{nameof(UserNameAsPasswordValidator<User>.MaxAllowedUserNameSubset)}", "3")
            });
            var validator = new UserNameAsPasswordValidator<User>(configurationBuilder.Build());
            var identityResult = await validator.ValidateAsync(null, new User { UserName = UserName }, password);
            Assert.True(identityResult.Succeeded);
        }
    }
}
