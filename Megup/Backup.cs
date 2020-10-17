using System.Threading;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ByteSizeLib;
using CG.Web.MegaApiClient;

namespace Megup
{
    internal static class Backup
    {
        public static async Task<int> Run(Config config, string[] args)
        {
            if (args.Length < 1)
            {
                MegupLog.SentryLogger.Error("You must specify a folder to back up");
                return 1;
            }

            var localFolder = args[0];
            if (!Directory.Exists(localFolder))
            {
                MegupLog.SentryLogger.Error($"Could not find local folder \"{localFolder}\"");
                return 2;
            }

            MegupLog.ReadableLogger.Information($"Starting backup at {DateTime.Now}");

            var client = new MegaApiClient();
            var fileCount = 0;
            var totalSize = 0L;

            using (MegaSession.Create(client, config.MegaUser, config.MegaPassword))
            {
                var remoteDirectory = EnsureEmptyRemoteDirectory(config, client);

                foreach (var f in Directory.EnumerateFiles(localFolder).OrderBy(s => s))
                {
                    totalSize += await Download(client, remoteDirectory, f);
                }
            }

            MegupLog.ReadableLogger.Information($"Completed backup at {DateTime.Now}");
            MegupLog.ReadableLogger.Information($"Uploaded {fileCount} files ({ByteSize.FromBytes(totalSize)})");

            return 0;
        }

        private static async Task<long> Download(MegaApiClient client, INode remoteDirectory, string f)
        {
            var shortFileName = Path.GetFileName(f);
            MegupLog.SentryLogger.Information(
                $"Uploading local file \"{shortFileName}\" to remote dir \"{remoteDirectory.Name}\"");

            var progress = new Progress(shortFileName);

            using (var fileStream = File.OpenRead(f))
            {
                var retryCount = 0;
                var uploadSuccessful = false;
                while (retryCount < 3 && !uploadSuccessful)
                {
                    try
                    {
                        await client.UploadAsync(
                            fileStream,
                            shortFileName,
                            remoteDirectory,
                            progress);
                        uploadSuccessful = true;
                    }
                    catch (ApiException)
                    {
                        // The MEGA API isn't all that reliable. Often, you might get a
                        // "ResourceNotFound" or similar, and then it just goes away.
                        retryCount++;
                        Thread.Sleep(Convert.ToInt32(TimeSpan.FromMinutes(2).TotalMilliseconds));
                    }
                }

                return fileStream.Length;
            }
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
            private double lastValue;

            public Progress(string name)
            {
                this.name = name;
                this.lastValue = 0.0d;
            }

            public void Report(double value)
            {
                // Don't spam logs by printing every update, only in 5% steps.
                if (value < lastValue + 5)
                    return;

                MegupLog.SentryLogger.Information($"Progress for {this.name}: {value:N2}%");
                lastValue = value;
            }
        }
    }
}