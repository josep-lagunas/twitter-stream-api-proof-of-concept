using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net;
using System.Web.Http.Filters;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace WebSocketApi.Controllers
{
    internal class RequiresAuthorizationFilter : IActionFilter
    {
        object validationObject;

        public bool AllowMultiple => true;

        public RequiresAuthorizationFilter(object validationObject)
        {
            this.validationObject = validationObject;
        }

        public Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext,
            CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            if (MethodAllowAnonymous(actionContext))
            {
                return continuation();
            }

            if (ControllerRequiresAuthorization(actionContext)
                || MethodRequiresAuthorization(actionContext))
            {
                string clientId = HttpContext.Current.Request.Headers["client-id"];

                if (IsAuthorized(clientId))
                {
                    return UnauthorizedResponse();
                }
            }

            return continuation();
        }

        private static Task<HttpResponseMessage> UnauthorizedResponse()
        {
            return Task.Run(() => { return new HttpResponseMessage(HttpStatusCode.Unauthorized); });
        }

        private bool IsAuthorized(string clientId)
        {
            return ((ConcurrentDictionary<ConnectionCredentials, Connection>) validationObject)
                   .Keys.FirstOrDefault(c => { return c.ClientId == clientId; }) == null;
        }

        private static bool MethodRequiresAuthorization(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor
                       .GetCustomAttributes<RequiresAuthorizationAttribute>()
                       .FirstOrDefault() != null;
        }

        private static bool ControllerRequiresAuthorization(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor
                       .ControllerDescriptor
                       .GetCustomAttributes<RequiresAuthorizationAttribute>().FirstOrDefault() !=
                   null;
        }

        private static bool MethodAllowAnonymous(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor
                       .GetCustomAttributes<AllowAnonymousAttribute>()
                       .FirstOrDefault() != null;
        }
    }
}