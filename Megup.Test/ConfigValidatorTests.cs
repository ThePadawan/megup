using System;
using Xunit;
using FluentAssertions;
using FluentValidation.Results;

namespace Megup.Test
{
    public class ConfigValidatorTests
    {
        [Theory]
        [InlineData("!!!")]
        [InlineData("$$_backup")]
        public void InvalidRemoteDirectoryDoesNotValidate(string remoteDirectoryName)
        {
            ValidationResultFor(ValidConfigurationWith(c => c.RemoteDirectory = remoteDirectoryName))
                .IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("backup_1580")]
        [InlineData("_foo")]
        public void ValidRemoteDirectoryValidates(string remoteDirectoryName)
        {
            ValidationResultFor(ValidConfigurationWith(c => c.RemoteDirectory = remoteDirectoryName))
                .IsValid.Should().BeTrue();
        }

        private Config ValidConfigurationWith(Action<Config> action)
        {
            var config = new Config
            {
                SentryDsn = "Example",
                RemoteDirectory = "testing",
                MegaUser = "foo@example.org",
                MegaPassword = "password"
            };
            action(config);
            return config;
        }

        private ValidationResult ValidationResultFor(Config c)
        {
            return new ConfigValidator().Validate(c);
        }
    }
}
