using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.SessionState;
using System.Collections.Concurrent;
using TwitterApi;
using Newtonsoft.Json;
using System.IO;
//using System.Web.WebSockets;

namespace WebSocketApi.Controllers
{
    public class WebSocketController : ApiController, IRequiresSessionState
    {
        static ConcurrentDictionary<ConnectionCredentials, Connection> clientsConnections = new ConcurrentDictionary<ConnectionCredentials, Connection>();

        [Route("api/request-ws-token")]
        [HttpGet]
        public IHttpActionResult RequestWebSocketToken()
        {
            try
            {
                var context = Request.Properties["MS_HttpContext"] as HttpContextWrapper;

                string sessionId = Guid.NewGuid().ToString();
                string clientId = Guid.NewGuid().ToString();

                if (CheckExistClientConnection(clientId, sessionId))
                {
                    var response = Request.CreateResponse(HttpStatusCode.Conflict, "Connection already in use for client.");
                    return ResponseMessage(response);
                }
                
                ConnectionCredentials connectionCredentials = new ConnectionCredentials(clientId, sessionId);

                if (!AddNewCredentialsWithOutConnection(connectionCredentials, null))
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, "Error registering connection."));
                }
                //return token to be sent in every packet with the form { key: SHA1 of data field, data: { parameters in format {k : v} }
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Accepted, new WebSocketToken(connectionCredentials.ClientId, connectionCredentials.HashedKey)));
            }
            catch (Exception ex)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, "Unexpected error."));
            }            
        }    

        private bool AddNewCredentialsWithOutConnection(ConnectionCredentials connectionCredentials, Connection connection)
        {
            try
            {
                clientsConnections.AddOrUpdate(connectionCredentials, connection,
                    (creds, conn) => { return conn; });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private Boolean BindConnection(ConnectionCredentials connectionCredentials, Connection connection)
        {
            return AddNewCredentialsWithOutConnection(connectionCredentials, connection);
        }

        private ConnectionCredentials GetConnectionCredential(string clientId)
        {
            return clientsConnections.Keys.First(c => { return c.ClientId == clientId; });
        }

        private bool CheckExistClientConnection(string clientId, string sessionId)
        {            
            return clientsConnections.Keys.FirstOrDefault(c => { return c.SessionId == sessionId; }) != null;
        }

        [Route("api/connect-websocket")]
        [HttpGet]
        public IHttpActionResult AcceptWebSocketConnection()
        {
            var context = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
            if (context.IsWebSocketRequest)
            {
                
                //clientID, not the ws-token which must be used to calculated SHA1 of the Data sent.
                //string clientId = Request.Headers.GetValues("clientid").FirstOrDefault();
                //string wsToken = Request.Headers.GetValues("wstoken").FirstOrDefault();

                //ConnectionCredentials connCred = GetConnectionCredential(clientId);

                //trying to register connection (WebSocket) with and unknown client id
                /*if (connCred == null)
                {
                    ResponseMessage(Request.CreateResponse(HttpStatusCode.Forbidden, "Unknown clientid."));
                }*/
                                
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

            string clientId = webSocketContext.Headers.Get("client-connection-id");
            string hashedKey = webSocketContext.Headers.Get("client-key");

            //check the integrity of the connection
            
            //Connection streamConnection = new Connection();
            //streamConnections.Add(streamConnection);

            /*We define a certain constant which will represent 
            size of received data. It is established by us and  
            we can set any value. We know that in this case the size of the sent 
            data is very small. 
            */
            const int maxMessageSize = 1024;

            //Buffer for received bits. 
            var receivedDataBuffer = new ArraySegment<Byte>(new Byte[maxMessageSize]);

            var cancellationToken = new CancellationToken();

            //Checks WebSocket state. 
            while (webSocket.State == WebSocketState.Open)
            {
                //Reads data. 
                WebSocketReceiveResult webSocketReceiveResult =
                  await webSocket.ReceiveAsync(receivedDataBuffer, cancellationToken);

                //If input frame is cancelation frame, send close command. 
                if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                      String.Empty, cancellationToken);
                }
                else
                {
                    byte[] payloadData = receivedDataBuffer.Array.Where(b => b != 0).ToArray();
                    
                    WebsocketDataPackage package = GetPackage(ref payloadData);

                    string clientWsToken = GetConnectionCredential(package.clientId).HashedKey;

                    if (!CheckDataIntegrity(package.Data, package.HashedKey, clientWsToken))
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "corrupted package received", cancellationToken);
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

        private WebsocketDataPackage GetPackage(ref byte[] payloadData)
        {
            //Because we know that is a string, we convert it. 
            string receiveString =
              System.Text.Encoding.UTF8.GetString(payloadData, 0, payloadData.Length);

            return JsonConvert.DeserializeObject<WebsocketDataPackage>(receiveString);
        }

        private bool CheckDataIntegrity(object Data, string expectedHashedKey, string clientWsToken)
        {
            byte[] serializedData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Data) + clientWsToken);
            string hashedKey = Convert.ToBase64String(SHA1.Create().ComputeHash(serializedData));
            return expectedHashedKey == hashedKey;
        }
        
    }
}