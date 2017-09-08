using System;
using System.Collections.Generic;
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
using System.Web.WebSockets;

namespace WebSocketApi.Controllers
{
    public class WebSocketController : ApiController, IRequiresSessionState
    {
        [Route("api/start-streaming")]
        [HttpGet]
        public HttpResponseMessage StartStreaming()
        {
            var context = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
            if (context.IsWebSocketRequest)
            {
                //If yes, we attach the asynchronous handler. 
                context.AcceptWebSocketRequest(WebSocketRequestHandler);
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
                response.Headers.Add("Upgrade", "websocket");
                response.Headers.Add("Connection", "Upgrade");
                byte[] buffer = Encoding.UTF8.GetBytes(Request.Headers.GetValues("Sec-WebSocket-Key").First() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
                string secWebSocketAcceptValue = Convert.ToBase64String(SHA1.Create().ComputeHash(buffer));
                response.Headers.Add("Sec-WebSocket-Accept", secWebSocketAcceptValue);
                return response;
            }
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }

        //Asynchronous request handler. 
        public async Task WebSocketRequestHandler(AspNetWebSocketContext webSocketContext)
        {
            //Gets the current WebSocket object. 
            WebSocket webSocket = webSocketContext.WebSocket;

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

                    //Because we know that is a string, we convert it. 
                    string receiveString =
                      System.Text.Encoding.UTF8.GetString(payloadData, 0, payloadData.Length);

                    //Converts string to byte array. 
                    var newString =
                      String.Format("Hello, " + receiveString + " ! Time {0}", DateTime.Now.ToString());
                    Byte[] bytes = System.Text.Encoding.UTF8.GetBytes(newString);

                    //Sends data back. 
                    await webSocket.SendAsync(new ArraySegment<byte>(bytes),
                      WebSocketMessageType.Text, true, cancellationToken);
                }
            }
        }
    }
}