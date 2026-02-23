using Microsoft.AspNetCore.Identity;
using ProjectManagementApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagementApplication_UnitTests
{
    public class PasswordHelperTests
    {
        [Fact]
        public void GeneratePassword_ContainsRequiredCharacters()
        {
            var options = new PasswordOptions
            {
                RequiredLength = 6,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true
            };

            var password = PasswordHelper.GeneratePassword(options);

            Assert.NotNull(password);
            Assert.Contains(password, c => char.IsDigit(c));
            Assert.Contains(password, c => char.IsLower(c));
            Assert.Contains(password, c => char.IsUpper(c));
            Assert.Contains(password, c => !char.IsLetterOrDigit(c));
        }
        [Fact]
        public void GeneratePassword_RespectsRequiredLength()
        {
            var options = new PasswordOptions
            {
                RequiredLength = 6
            };

            var password = PasswordHelper.GeneratePassword(options);

            Assert.NotNull(password);
            Assert.True(password.Length >= options.RequiredLength);
        }
    }
}
