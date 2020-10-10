using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using FluentValidation;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Text;

namespace Megup
{
    internal class Config
    {
        public string SentryDsn { get; set; }
        public string RemoteDirectory { get; set; }

        public string MegaUser { get; set; }
        public string MegaPassword { get; set; }
    }

    internal class ConfigValidator : AbstractValidator<Config>
    {
        private static readonly Regex folderNameRegex = new Regex("^[a-zA-Z0-9_-]+$");

        public ConfigValidator()
        {
            RuleFor(c => c.RemoteDirectory)
                .Must(BeValidDirectoryName)
                .WithMessage(c => GetDirectoryNameError(c.RemoteDirectory));

            RuleFor(c => c.MegaUser)
                .NotEmpty().NotNull();

            RuleFor(c => c.MegaPassword)
                .NotEmpty().NotNull();
        }

        private static bool BeValidDirectoryName(string name)
        {
            return GetDirectoryNameError(name) == null;
        }

        private static string GetDirectoryNameError(string name)
        {
            if (name == null || name.Length == 0)
                return "Remote directory name is empty.";

            if (name.Length > 64)
            {
                // Not an error, but probably not what you want to do.
                return "Remote directory name is too long.";
            }

            if (!folderNameRegex.IsMatch(name))
                return "Remote directory name contains invalid characters.";

            return null;
        }
    }

    internal static class ConfigLoader
    {
        public static Config Load()
        {
            var configStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Megup.config.yml");

            var configText = new StreamReader(configStream).ReadToEnd();

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var config = deserializer.Deserialize<Config>(configText);

            var validationResult = new ConfigValidator().Validate(config);

            if (!validationResult.IsValid)
            {
                var message = new StringBuilder();
                foreach (var error in validationResult.Errors)
                    message.AppendLine(error.ToString());
                throw new System.Exception(message.ToString());
            }

            return config;
        }
    }
}
