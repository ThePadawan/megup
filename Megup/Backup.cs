using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CG.Web.MegaApiClient;
using Serilog;

namespace Megup
{
    internal static class Backup
    {
        public static async Task<int> Run(Config config, string[] args)
        {
            if (args.Length < 1)
            {
                Log.Logger.Error("You must specify a folder to back up");
                return 1;
            }

            var localFolder = args[0];
            if (!Directory.Exists(localFolder))
            {
                Log.Logger.Error($"Could not find local folder \"{localFolder}\"");
                return 2;
            }

            var client = new MegaApiClient();

            using (MegaSession.Create(client, config.MegaUser, config.MegaPassword))
            {
                var remoteDirectory = EnsureEmptyRemoteDirectory(config, client);

                foreach (var f in Directory.EnumerateFiles(localFolder).OrderBy(s => s))
                {
                    var shortFileName = Path.GetFileName(f);
                    Log.Logger.Information(
                        $"Uploading local file \"{f}\" (\"{shortFileName}\") to remote dir \"{remoteDirectory.Name}\"");

                    var progress = new Progress(shortFileName);

                    using (var fileStream = File.OpenRead(f))
                    {
                        await client.UploadAsync(
                            fileStream,
                            shortFileName,
                            remoteDirectory,
                            progress);
                    }
                }
            }

            return 0;
        }

        private static INode EnsureEmptyRemoteDirectory(Config config, MegaApiClient client)
        {
            var nodes = client.GetNodes();
            var root = nodes.Single(n => n.Type == NodeType.Root);

            var maybeExistingDirectory =
                nodes.SingleOrDefault(n => n.Type == NodeType.Directory && n.Name == config.RemoteDirectory);

            if (maybeExistingDirectory != null)
            {
                client.Delete(maybeExistingDirectory, moveToTrash: false);
            }

            return client.CreateFolder(config.RemoteDirectory, root);
        }

        private class Progress : IProgress<double>
        {
            private readonly string name;

            public Progress(string name)
            {
                this.name = name;
            }

            public void Report(double value)
            {
                Log.Logger.Information($"Progress for {this.name}: {value:N2}%");
            }
        }
    }
}