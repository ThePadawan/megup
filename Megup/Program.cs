using System.Threading.Tasks;
using Serilog;

namespace Megup
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var config = ConfigLoader.Load();
            InitializeLogger(config);

            return await Backup.Run(config, args);
        }

        private static void InitializeLogger(Config config)
        {
            var loggerConfiguration = new LoggerConfiguration().WriteTo.Console();

            if (config.SentryDsn != null)
            {
                loggerConfiguration = loggerConfiguration
                    .WriteTo.Sentry(options =>
                    {
                        options.Dsn = new Sentry.Dsn(config.SentryDsn);
                    });
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }
    }
}
