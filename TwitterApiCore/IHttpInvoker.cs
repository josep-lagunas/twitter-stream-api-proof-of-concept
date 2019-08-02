using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HTTP.Helpers
{
    public interface IHttpInvoker
    {
        Task HttpGetInvokeAsync(string url, IEnumerable<Header> headers, TimeSpan timeOut);
        void HttpGetStreamInvoke(string url, IEnumerable<Header> headers, HttpInvocationCompletionOption httpInvocationCompletionOption, TimeSpan timeOut, CancellationToken cancellationToken);
        void HttpGetStreamInvoke(string url, IEnumerable<Header> headers, HttpInvocationCompletionOption httpInvocationCompletionOption, TimeSpan timeOut, HttpInvoker.HttpResponseHandler httpResponseHandler, CancellationToken cancellationToken);
        Task HttpGetStreamInvokeAsync(string url, IEnumerable<Header> headers, TimeSpan timeOut, HttpInvoker.HttpResponseHandler httpResponseHandler);
        Task HttpPostInvoke(string url, IEnumerable<Header> headers, IEnumerable<Header> contentHeaders, Dictionary<string, string> postParameters, TimeSpan timeOut);
        Task HttpPostInvoke(string url, IEnumerable<Header> headers, IEnumerable<Header> contentHeader, Dictionary<string, string> postParameters, TimeSpan timeOut, HttpInvoker.HttpResponseHandler httpResponseHandler);
        void HttpPostStreamInvoke(string url, IEnumerable<Header> headers, IEnumerable<Header> contentHeaders, HttpInvocationCompletionOption httpCompletetionOption, string postParameters, TimeSpan timeOut, CancellationToken cancellationToken);
        void HttpPostStreamInvoke(string url, IEnumerable<Header> headers, IEnumerable<Header> contentHeaders, HttpInvocationCompletionOption httpCompletetionOption, string postParameters, TimeSpan timeOut, HttpInvoker.HttpResponseHandler httpResponseHandler, CancellationToken cancellationToken);
    }
}