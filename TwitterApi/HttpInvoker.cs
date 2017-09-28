using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Utils;

namespace TwitterApi.Controllers
{
    public abstract class Header
    {
        public string Key { get; set; }
        public virtual string Value { get; set; }

        public class SingleValueHeadear : Header
        {
            public override string Value { get; set; }
            internal SingleValueHeadear(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }
        public class ListValuesHeader : Header
        {
            public new IEnumerable<string> Value { get; set; }
            internal ListValuesHeader(string key, IEnumerable<string> value)
            {
                Key = key;
                Value = value;
            }
        }

        public static Header CreateHeader(string key, string value)
        {
            return new SingleValueHeadear(key, value);
        }

        public static Header CreateHeader(string key, IEnumerable<string> value)
        {
            return new ListValuesHeader(key, value);
        }

    }

    public class HttpInvokerResponse
    {
        private HttpStatusCode httpStatusCode;
        public HttpStatusCode HttpStatusCode { get { return httpStatusCode; } }
        private string httpStatusMessage;
        public string HttpStatusMessage { get { return httpStatusMessage; } }
        private IEnumerable<Header> headers;
        public IEnumerable<Header> Headers { get { return headers; } }
        private IEnumerable<Header> contentHeaders;
        public IEnumerable<Header> ContentHeaders { get { return contentHeaders; } }
        private string httpContent;
        public string HttpContent { get { return httpContent; } }
        public bool connectionIsClosed;
        public bool ConnectinIsClosed { get { return connectionIsClosed; } }

        public HttpInvokerResponse(HttpStatusCode httpStatusCode, string httpStatusMessage,
            IEnumerable<Header> headers, IEnumerable<Header> contentHeaders, String httpContent,
            bool connectionIsClosed)
        {
            this.httpStatusCode = httpStatusCode;
            this.httpStatusMessage = httpStatusMessage;
            this.headers = headers;
            this.contentHeaders = contentHeaders;
            this.httpContent = httpContent;
            this.connectionIsClosed = connectionIsClosed;
        }
    }

    public class HttpInvokerResponseArgs : EventArgs
    {
        HttpInvokerResponse result;
        public HttpInvokerResponse Result { get { return result; } }
        public HttpInvokerResponseArgs(HttpInvokerResponse result)
        {
            this.result = result;
        }
    }
    public class HttpInvoker
    {
        private static object locker = new object();
        private static HttpInvoker instance;
        public delegate void HttpResponseHandler(HttpInvoker invoker, HttpInvokerResponseArgs e);
       
        private HttpInvoker()
        {
        }

        public static HttpInvoker GetInstance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new HttpInvoker();
                    }
                }
            }
            return instance;
        }

        public void HttpPostStreamInvoke(string url, IEnumerable<Header> headers,
           IEnumerable<Header> contentHeaders, HttpCompletionOption httpCompletetionOption,
           string postParameters, TimeSpan timeOut, CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                await HttpStreamInvoke(url, HttpMethod.Post, headers, contentHeaders,
                    httpCompletetionOption, postParameters, timeOut, null, cancellationToken);
            }, cancellationToken);
        }
        public void HttpPostStreamInvoke(string url, IEnumerable<Header> headers,
          IEnumerable<Header> contentHeaders, HttpCompletionOption httpCompletetionOption,
          string postParameters, TimeSpan timeOut, HttpResponseHandler httpResponseHandler, 
          CancellationToken cancellationToken)
        {
            Task.Run(async () =>
             {
                 await HttpStreamInvoke(url, HttpMethod.Post, headers, contentHeaders,
                 httpCompletetionOption, postParameters, timeOut, httpResponseHandler,
                 cancellationToken);
             }, cancellationToken);
        }
        public void HttpGetStreamInvoke(string url, IEnumerable<Header> headers,
           HttpCompletionOption httpCompletetionOption, TimeSpan timeOut, 
           CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                await HttpStreamInvoke(url, HttpMethod.Get, headers, null,
                httpCompletetionOption, null, timeOut, null, cancellationToken);
            }, cancellationToken);
        }
        public void HttpGetStreamInvoke(string url, IEnumerable<Header> headers,
            HttpCompletionOption httpCompletetionOption, TimeSpan timeOut,
          HttpResponseHandler httpResponseHandler, CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                await HttpStreamInvoke(url, HttpMethod.Get, headers, null,
                httpCompletetionOption, null, timeOut, httpResponseHandler, cancellationToken);
            }, cancellationToken);
        }
        private async Task HttpStreamInvoke(string url, HttpMethod httpMethod, IEnumerable<Header> headers,
            IEnumerable<Header> contentHeaders, HttpCompletionOption httpCompletetionOption,
            string postParameters, TimeSpan timeOut, HttpResponseHandler httpResponseHandler, 
            CancellationToken cancellationToken)
        {
            postParameters = postParameters ?? String.Empty;
            byte[] bufferPostParameters = Encoding.UTF8.GetBytes(postParameters);            

            using (StreamContent streamContent = new StreamContent(new MemoryStream(bufferPostParameters)))
            {
                if (httpMethod == HttpMethod.Post)
                {
                    contentHeaders.ToList().ForEach(h => { streamContent.Headers.Add(h.Key, h.Value); });
                }

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.Timeout = timeOut;
                    headers.ToList().ForEach(h => { httpClient.DefaultRequestHeaders.Add(h.Key, h.Value); });
                    httpClient.BaseAddress = new Uri(url);

                    var request = new HttpRequestMessage(httpMethod, url);

                    if (httpMethod == HttpMethod.Post)
                    {
                        request.Content = streamContent;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    await httpClient.SendAsync(request, httpCompletetionOption)
                        .ContinueWith(async responseTask =>
                        {
                            using (HttpResponseMessage response = responseTask.Result)
                            {
                                await response.Content.ReadAsStreamAsync().ContinueWith(streamTask =>
                                {
                                    using (StreamReader streamReader = new StreamReader(streamTask.Result))
                                    {
                                        List<Header> responseHeaders = new List<Header>();
                                        responseHeaders.AddRange(
                                            response.Headers.Select(c => { return Header.CreateHeader(c.Key, c.Value); }));
                                        List<Header> responseContentHeaders = new List<Header>();
                                        responseContentHeaders.AddRange(
                                            response.Content.Headers.Select(c => { return Header.CreateHeader(c.Key, c.Value); }));
                                        try
                                        {
                                            while (!streamReader.EndOfStream && !cancellationToken.IsCancellationRequested)
                                            {
                                                if (httpResponseHandler != null)
                                                {
                                                    string json = streamReader.ReadLine();

                                                    httpResponseHandler(this,
                                                        new HttpInvokerResponseArgs(new HttpInvokerResponse(response.StatusCode,
                                                        response.ReasonPhrase, headers, contentHeaders, json, false)));
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Exception e = ex;
                                        }
                                        finally
                                        {if (!cancellationToken.IsCancellationRequested)
                                            {
                                                httpResponseHandler?.Invoke(this,
                                                    new HttpInvokerResponseArgs(new HttpInvokerResponse(response.StatusCode,
                                                    response.ReasonPhrase, headers, contentHeaders, String.Empty, true)));
                                            }
                                        }
                                    }
                                });
                            }
                        });
                }
            }
        }
        public async Task HttpPostInvoke(string url, IEnumerable<Header> headers, IEnumerable<Header> contentHeaders,
          Dictionary<string, string> postParameters, TimeSpan timeOut)
        {
            await HttpPostInvoke(url, headers, contentHeaders, postParameters, timeOut, null);
        }
        public async Task HttpPostInvoke(string url, IEnumerable<Header> headers, IEnumerable<Header> contentHeader,
           Dictionary<string, string> postParameters, TimeSpan timeOut,
           HttpResponseHandler httpResponseHandler)
        {
            // Build the form data (exclude OAuth stuff that's already in the header).
            var formData = new FormUrlEncodedContent(postParameters);
            contentHeader.ToList().ForEach(h => { formData.Headers.Add(h.Key, h.Value); });
            using (var httpClient = new HttpClient())
            {
                headers.ToList().ForEach(h => { httpClient.DefaultRequestHeaders.Add(h.Key, h.Value); });

                HttpResponseMessage httpResponse = await httpClient.PostAsync(url, formData);
                string respBody = await httpResponse.Content.ReadAsStringAsync();

                List<Header> responseHeaders = new List<Header>();
                responseHeaders.AddRange(
                    httpResponse.Headers.Select(c =>
                    {
                        return Header.CreateHeader(c.Key, c.Value);
                    }));
                List<Header> responseContentHeaders = new List<Header>();
                responseContentHeaders.AddRange(
                    httpResponse.Content.Headers.Select(c =>
                    {
                        return Header.CreateHeader(c.Key, c.Value);
                    }));

                httpResponseHandler?.Invoke(this,
                    new HttpInvokerResponseArgs(new HttpInvokerResponse(httpResponse.StatusCode,
                                                httpResponse.ReasonPhrase, responseHeaders, responseContentHeaders, String.Empty, true)));
            }
        }

        public async Task HttpGetInvoke(string url, IEnumerable<Header> headers, TimeSpan timeOut)
        {
            await HttpGetInvoke(url, headers, timeOut, null);
        }
        public async Task HttpGetInvoke(string url, IEnumerable<Header> headers, TimeSpan timeOut, HttpResponseHandler httpResponseHandler)
        {

            using (var httpClient = new HttpClient())
            {
                headers.ToList().ForEach(h => { httpClient.DefaultRequestHeaders.Add(h.Key, h.Value); });

                HttpResponseMessage httpResponse = await httpClient.GetAsync(url);
                string respBody = await httpResponse.Content.ReadAsStringAsync();

                List<Header> responseHeaders = new List<Header>();
                responseHeaders.AddRange(
                    httpResponse.Headers.Select(c =>
                    {
                        return Header.CreateHeader(c.Key, c.Value);
                    }));
                List<Header> responseContentHeaders = new List<Header>();
                responseContentHeaders.AddRange(
                    httpResponse.Content.Headers.Select(c =>
                    {
                        return Header.CreateHeader(c.Key, c.Value);
                    }));

                httpResponseHandler?.Invoke(this,
                    new HttpInvokerResponseArgs(new HttpInvokerResponse(httpResponse.StatusCode,
                                                httpResponse.ReasonPhrase, responseHeaders, responseContentHeaders, String.Empty, true)));
            }
        }

    }
}

