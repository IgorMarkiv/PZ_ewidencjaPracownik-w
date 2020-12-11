using DBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;


namespace ProjectZespolowy
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var db = new DB();

            SessionManagerDll.ManagersInitializer.InitAll(db);
            LoginManager.Init(db);
            UserManager.Init(db);
            LogManager.Init(db);
            MessengerManager.Init(db);
            CloudManager.Init(db);
            RegistrationManager.Init(db);
        }
    }
}