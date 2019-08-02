using System;
using System.Net.WebSockets;
using WebSocketApi.Controllers;

namespace WebSocketApi
{
    public class Connection : IEquatable<Connection>
    {
        public ConnectionCredentials connectionCredentials { get; }
        public WebSocket webSocket { get; set; }

        public Connection(ConnectionCredentials connectionCredentials, WebSocket webSocket)
        {
            this.connectionCredentials = connectionCredentials;
            this.webSocket = webSocket;
        }

        public Connection(ConnectionCredentials connectionCredentials)
        {
            this.connectionCredentials = connectionCredentials;
        }

        public bool Equals(Connection other)
        {
            if (other == null) return false;

            return this.connectionCredentials.Equals(other.connectionCredentials) &&
                   this.webSocket.Equals(other.webSocket);
        }
    }
}