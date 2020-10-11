using System.Threading.Tasks;

namespace Megup
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var config = ConfigLoader.Load();
            MegupLog.InitializeSentry(config);
            return await Backup.Run(config, args);
        }
    }
}
