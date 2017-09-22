using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.SessionState;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace WebSocketApi.Controllers
{
    public class WebSocketController : ApiController, IRequiresSessionState, IDependencyResolver
    {
        protected static ConcurrentDictionary<ConnectionCredentials, Connection> clientsConnections = new ConcurrentDictionary<ConnectionCredentials, Connection>();
        protected static ConcurrentDictionary<ServerEvents, Dictionary<string, ConnectionCredentials>> serverEventsSubscriptors = 
            new ConcurrentDictionary<ServerEvents, Dictionary<string, ConnectionCredentials>>();
        protected static object sendLocker = new object();

        [Route("api/request-ws-token")]
        [HttpGet]
        public IHttpActionResult RequestWebSocketToken()
        {
            try
            {
                var context = Request.Properties["MS_HttpContext"] as HttpContextWrapper;

                string clientId = Guid.NewGuid().ToString();
                string sessionId = Guid.NewGuid().ToString();

                ConnectionCredentials connectionCredentials = new ConnectionCredentials(clientId, sessionId);

                if (!AddNewCredentialsWithOutConnection(connectionCredentials, null))
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, "Error registering connection."));
                }
                //return token to be sent in every packet with the form { key: SHA1 of data field, data: { parameters in format {k : v} }
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Accepted, new WebSocketToken(connectionCredentials.ClientId, connectionCredentials.HashedKey)));
            }
            catch (Exception)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, "Unexpected error."));
            }            
        }    

        private bool AddNewCredentialsWithOutConnection(ConnectionCredentials connectionCredentials, Connection connection)
        {
            try
            {
                clientsConnections.AddOrUpdate(connectionCredentials, connection,
                    (creds, conn) =>
                    {
                        if (conn == null)
                        {
                            return connection;
                        }
                        return conn;
                    });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Route("api/subscribe-server-events/{clientId}")]
        [HttpPost]
        public IHttpActionResult SubscribeServerEvents(string clientId, [FromBody] List<ServerEventsDTO> serverEventsDTO)
        {
            ConnectionCredentials cc = GetConnectionCredential(clientId);
            if (cc == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, "Unknows clientId."));
            }

            if (!ParseServerEventsFromServerEventsDTO(serverEventsDTO, out List<ServerEvents> serverEvents))
            {
                return ResponseMessage(Request.CreateResponse(
                       HttpStatusCode.Conflict,
                       String.Format("Unknown server event.")));
            }
            
            SubscribeClientToEvents(clientId, serverEvents);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        [Route("api/unsubscribe-server-events/{clientId}")]
        [HttpPost]
        public IHttpActionResult UnsubscribeServerEvents(string clientId, [FromBody] List<ServerEventsDTO> serverEventsDTO)
        {
            ConnectionCredentials cc = GetConnectionCredential(clientId);
            if (cc == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, "Unknows clientId."));
            }
            if (!ParseServerEventsFromServerEventsDTO(serverEventsDTO, out List<ServerEvents> serverEvents))
            {
                return ResponseMessage(Request.CreateResponse(
                       HttpStatusCode.Conflict,
                       String.Format("Unknown server event.")));
            }

            UnsubscribeClientFromEvents(clientId, serverEvents);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        protected void NotifyServerEventAsync(ServerEvents serverEvent, object data)
        {
            serverEventsSubscriptors.TryGetValue(serverEvent, out Dictionary<string, ConnectionCredentials> subscriptors);
            WebsocketDataPackage package = GenerateSignedPackage("API", data);
            if (subscriptors != null)
            {
                subscriptors.Values.ToList().ForEach(cc =>
                {
                    if (cc.ConnectionSet)
                    {
                        NotifyToClient(cc, package);
                    }
                });
            }
           
        }

        private static bool NotifyToClient(ConnectionCredentials cc, WebsocketDataPackage package)
        {
            if (clientsConnections.TryGetValue(cc, out Connection connection)){
                string jsonData = JsonConvert.SerializeObject(package);
                Byte[] bytesToSend = System.Text.Encoding.UTF8.GetBytes(jsonData);
                try
                {
                    lock (sendLocker)
                    {
                        connection.webSocket.SendAsync(new ArraySegment<byte>(bytesToSend),
                                  WebSocketMessageType.Text, true, new CancellationToken());
                        return true;
                    }
                }catch(Exception ex)
                {
                    Exception e = ex;                    
                }
            }
            return false;
        }

        private bool ParseServerEventsFromServerEventsDTO(List<ServerEventsDTO> serverEventsDTO, out List<ServerEvents> serverEvents)
        {

            serverEvents = new List<ServerEvents>();
            int i = 0;
            while (i < serverEventsDTO.Count)
            {
                try
                {
                    ServerEvents serverEvent = (ServerEvents)serverEventsDTO[i].Id;
                    if (serverEvent.ToString().ToUpper() != serverEventsDTO[i].Name.ToUpper())
                    {
                        return false;
                    }
                    serverEvents.Add(serverEvent);
                }
                catch (Exception)
                {
                    return false;
                }
                i++;
            }

            return true;
        }

        private bool SubscribeClientToEvents(string clientId, List<ServerEvents> eventsToSubscribe)
        {
            ConnectionCredentials connectionCredentials = GetConnectionCredential(clientId);
            if (connectionCredentials == null)
            {
                return false;
            }

            eventsToSubscribe.ForEach(serverEvent =>
            {
                serverEventsSubscriptors.AddOrUpdate(serverEvent,
                    (subscriptors) =>
                    {
                        Dictionary<string, ConnectionCredentials> connections = new Dictionary<string, ConnectionCredentials>();
                        connections.Add(connectionCredentials.ClientId, connectionCredentials);
                        return connections;
                        
                    },
                    (ev, subscriptors) =>
                    {
                        if (!subscriptors.ContainsKey(connectionCredentials.ClientId))
                        {
                            subscriptors.Add(connectionCredentials.ClientId, connectionCredentials);
                        }
                        return subscriptors;
                    });
            });

            return true;
            
        }

        private bool UnsubscribeClientFromEvents(string clientId, List<ServerEvents> eventsToUnsubscribe)
        {
            ConnectionCredentials connectionCredentials = GetConnectionCredential(clientId);
            if (connectionCredentials == null)
            {
                return false;
            }
            eventsToUnsubscribe.ForEach(serverEvent =>
            {
                Dictionary<string, ConnectionCredentials> subscriptors = new Dictionary<string, ConnectionCredentials>();
                if (serverEventsSubscriptors.TryGetValue(serverEvent, out subscriptors))
                {
                    subscriptors.Remove(clientId);
                    if (subscriptors.Count == 0)
                    {
                        serverEventsSubscriptors.TryRemove(serverEvent, out Dictionary<string, ConnectionCredentials> value);
                    }
                }
            });
            return true;
        }

        private Boolean BindConnection(ConnectionCredentials connectionCredentials, Connection connection)
        {
            if (AddNewCredentialsWithOutConnection(connectionCredentials, connection))
            {
                connectionCredentials.connected();
                return true;
            }
            return false;
        }

        protected ConnectionCredentials GetConnectionCredential(string clientId)
        {
            return clientsConnections.Keys.FirstOrDefault(c => { return c.ClientId == clientId; });
        }

        public virtual IHttpActionResult GetAvailableServerEvents()
        {
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new List<ServerEvents>()));
        }

        [Route("api/connect-websocket")]
        [HttpGet]
        public IHttpActionResult AcceptWebSocketConnection()
        {
            var context = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
            if (context.IsWebSocketRequest)
            {
                context.AcceptWebSocketRequest(WebSocketRequestHandler);
             
                HttpResponseMessage response = GetRegisterResponse();
                return ResponseMessage(response);
            }
            return ResponseMessage(new HttpResponseMessage(HttpStatusCode.MethodNotAllowed));
        }
               
        private HttpResponseMessage GetRegisterResponse()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
            response.Headers.Add("Upgrade", "websocket");
            response.Headers.Add("Connection", "Upgrade");
            byte[] buffer = Encoding.UTF8.GetBytes(
                Request.Headers.GetValues("Sec-WebSocket-Key").First() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
            string secWebSocketAcceptValue = Convert.ToBase64String(SHA1.Create().ComputeHash(buffer));
            response.Headers.Add("Sec-WebSocket-Accept", secWebSocketAcceptValue);
            return response;
        }

        //Asynchronous request handler. 
        public async Task WebSocketRequestHandler(WebSocketContext webSocketContext)
        {
            //Gets the current WebSocket object. 
            WebSocket webSocket = webSocketContext.WebSocket;

            //check the integrity of the connection
            
            /*We define a certain constant which will represent 
            size of received data. It is established by us and  
            we can set any value. We know that in this case the size of the sent 
            data is very small. 
            */
            const int maxMessageSize = 512;

            //Buffer for received bits. 
            var receivedDataBuffer = new ArraySegment<Byte>(new Byte[maxMessageSize]);

            var cancellationToken = new CancellationToken();

            //Checks WebSocket state. 
            while (webSocket.State == WebSocketState.Open)
            {
                //Reads data. 
                WebSocketReceiveResult webSocketReceiveResult =
                  await webSocket.ReceiveAsync(receivedDataBuffer, cancellationToken);

                bool errorDetected = false;
                //If input frame is cancelation frame, send close command. 
                if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                      String.Empty, cancellationToken);
                }
                else
                {
                    byte[] payloadData = receivedDataBuffer.Array.ToList().GetRange(0, webSocketReceiveResult.Count).ToArray();
                    WebsocketDataPackage package = GetPackage(ref payloadData);

                    string clientWsToken = GetConnectionCredential(package.clientId).HashedKey;

                    if (!CheckDataIntegrity(package.Data, package.HashedKey, clientWsToken))
                    {
                        errorDetected = true;
                        byte[] serializedData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(package.Data) + clientWsToken);
                        string receivedHashedKey = Convert.ToBase64String(SHA1.Create().ComputeHash(serializedData));

                        string message =
                        String.Format("Corrupted package: SHA1 received: {0} SHA1 expected: {1}", receivedHashedKey, package.HashedKey);
                        Byte[] bytesToSend = System.Text.Encoding.UTF8.GetBytes(message);

                        //Sends data back. 
                        await webSocket.SendAsync(new ArraySegment<byte>(bytesToSend),
                          WebSocketMessageType.Text, true, cancellationToken);
                    }
                    
                    if (!errorDetected)
                    {
                        ConnectionCredentials cc = GetConnectionCredential(package.clientId);
                        if (!cc.ConnectionSet) {
                            Connection conn = new Connection(cc, webSocket);
                            BindConnection(GetConnectionCredential(package.clientId), conn);
                        }
                        var newString =
                          String.Format("Hello, package with hashed key: " + package.HashedKey + " validated! Time {0}", DateTime.Now.ToString());
                        Byte[] bytes = System.Text.Encoding.UTF8.GetBytes(newString);

                        //Sends data back. 
                        await webSocket.SendAsync(new ArraySegment<byte>(bytes),
                          WebSocketMessageType.Text, true, cancellationToken);
                    }
                }
            }
        }

        private WebsocketDataPackage GetPackage(ref byte[] payloadData)
        {
            try
            {
                //Because we know that is a string, we convert it. 
                string receiveString =
                  System.Text.Encoding.UTF8.GetString(payloadData, 0, payloadData.Length);

                return JsonConvert.DeserializeObject<WebsocketDataPackage>(receiveString);
            }catch(Exception ex)
            {
                Exception e = ex;
                throw ex;
            }
        }

        private WebsocketDataPackage GenerateSignedPackage(string clientId, object data)
        {
            //string clientWsToken = GetConnectionCredential(clientId).HashedKey;
            byte[] serializedData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            string hashedKey = Convert.ToBase64String(SHA1.Create().ComputeHash(serializedData));
            return new WebsocketDataPackage(clientId, hashedKey, data);
        }

        private bool CheckDataIntegrity(object Data, string expectedHashedKey, string clientWsToken)
        {
            //decimal values deserialization can cause error due decimal loss of precision. 
            //work around: send decimal values as string.
            byte[] serializedData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Data) + clientWsToken);
            string hashedKey = Convert.ToBase64String(SHA1.Create().ComputeHash(serializedData));
            return expectedHashedKey == hashedKey;
        }

        public IDependencyScope BeginScope()
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}