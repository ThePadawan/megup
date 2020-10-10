using System;
using CG.Web.MegaApiClient;

namespace Megup
{
    internal class MegaSession : IDisposable
    {
        private bool disposedValue;
        private readonly MegaApiClient client;

        private MegaSession(MegaApiClient client)
        {
            this.client = client;
        }

        public static MegaSession Create(MegaApiClient client, string username, string password)
        {
            client.Login(username, password);
            return new MegaSession(client);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client.Logout();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}