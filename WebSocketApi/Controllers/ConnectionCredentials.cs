using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WebSocketApi.Controllers
{
    public class ConnectionCredentials : IEquatable<ConnectionCredentials>
    {
        public string ClientId { get; }
        public string SessionId { get; }
        public string HashedKey { get; set; }
        private bool _connectionSet;

        public bool ConnectionSet
        {
            get { return _connectionSet; }
        }

        private List<ServerEvents> eventSubscriptions { get; set; }

        public void connected()
        {
            _connectionSet = true;
        }

        public ConnectionCredentials(string clientId, string sessionId)
        {
            this.ClientId = clientId;
            this.SessionId = sessionId;
            this.HashedKey = GetHashedKey(clientId, sessionId);
            _connectionSet = false;
            this.eventSubscriptions = new List<ServerEvents>();
        }


        private static string GetHashedKey(string clientId, string sessionId)
        {
            return Convert.ToBase64String(SHA1.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(clientId + sessionId)));
        }

        public bool Equals(ConnectionCredentials other)
        {
            if (other == null)
            {
                return false;
            }

            return this.ClientId == other.ClientId
                   && this.SessionId == other.SessionId
                   && this.ConnectionSet == other.ConnectionSet;
        }

        public static bool IsValid(string clientId, string sessionId, string hashedKey)
        {
            return GetHashedKey(clientId, sessionId) == hashedKey;
        }
    }
}