using MovieCentral.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MovieCentral.Functions
{
    public class UserSecurity : ActionFilterAttribute, IActionFilter
    {
        public void RedirectUser(ActionExecutingContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
               new RouteValueDictionary(new { controller = "Home", action = "Login" }));

            filterContext.Result.ExecuteResult(filterContext.Controller.ControllerContext);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpCookie cookie = filterContext.HttpContext.Request.Cookies["_usr"];
           
            if (cookie == null)
            {
                RedirectUser(filterContext);
            }
            else
            {
            }
        }
    }
    public class AdminSecurity : ActionFilterAttribute, IActionFilter
    {
        public void RedirectUser(ActionExecutingContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
               new RouteValueDictionary(new { controller = "Home", action = "Index" }));

            filterContext.Result.ExecuteResult(filterContext.Controller.ControllerContext);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpCookie cookie = filterContext.HttpContext.Request.Cookies["_usr"];
            if (cookie == null)
            {
                RedirectUser(filterContext);
            }
            else
            {
                if (cookie.Value.Split(',')[2] != "Admin")
                {
                    RedirectUser(filterContext);
                }
            }
        }
    }
}