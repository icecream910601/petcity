using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PetCityApi1
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }



        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            var response = context.Response;
            //allow-origin直接用* 代表網域全開，或是這邊是要設定看對接的人網域是多少


            ////新版
            //if (context.Request.HttpMethod == "OPTIONS")
            //{
            //    response.AddHeader("Access-Control-Allow-Origin", "*");
            //    response.AddHeader("Access-Control-Allow-Methods", "GET, POST, DELETE, PATCH, PUT");
            //    response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
            //    response.AddHeader("Access-Control-Max-Age", "1000000");
            //    response.End();
            //}


            //原本的
            if (context.Request.HttpMethod == "OPTIONS")
            {
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, DELETE, PATCH, PUT");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
                response.AddHeader("Access-Control-Max-Age", "1000000");
                response.End();
            }
        }
    }
}
