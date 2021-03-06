using System.Web;
using System.Web.Mvc;
using Elmah;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.Raven;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;

namespace RaccoonBlog.Web.Helpers.Attributes
{
    public class ElmahHandleErrorAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
			ErrorLog.GetDefault(HttpContext.Current).Log(new Error(context.Exception, HttpContext.Current));

			BlogConfig blogConfig;
			using(var session = DocumentStoreHolder.DocumentStore.OpenSession())
			{
				blogConfig = session.Load<BlogConfig>("Blog/Config");
			}

			var controllerName = (string) context.RouteData.Values["controller"];
            var actionName = (string) context.RouteData.Values["action"];

        	var viewResult = new ViewResult
        	{
        		ViewName = "500",
        		ViewData = new ViewDataDictionary(new HandleErrorInfo(context.Exception, controllerName, actionName)),
        		ViewBag =
        			{
                        BlogConfig = blogConfig.MapTo<BlogConfigViewModel>()
        			}
        	};

        	context.ExceptionHandled = true;
        	context.HttpContext.Response.StatusCode = 500;
			context.HttpContext.Response.TrySkipIisCustomErrors = true;
			context.HttpContext.Response.Clear();

			context.Result = viewResult;
		}
    }
}
