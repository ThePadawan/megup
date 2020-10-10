using Serilog;

namespace Megup
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var config = ConfigLoader.Load();
            InitializeLogger(config);

            return Backup.Run(config, args);
        }

        private static void InitializeLogger(Config config)
        {
            if (config.SentryDsn == null)
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();

                Log.Logger.Information("No Sentry DSN set. Logging to stdout.");
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Sentry(options =>
                    {
                        options.Dsn = new Sentry.Dsn(config.SentryDsn);
                    })
                    .CreateLogger();
                Log.Logger.Information("Logging to Sentry.");
            }
        }
    }
}
