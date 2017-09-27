using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSocketApi.Controllers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    internal class RequiresAuthorizationAttribute : Attribute
    {
    }
}