using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Kuyam.WebUI.Properties;
using Kuyam.Database;
using System.Security.Policy;
using System.Web.Mvc;

namespace Kuyam.WebUI.Models
{
    public static class MyApp
    {
        public static class Platform
        {
            public static string DB;
            public static string DBUser;
            public static string Build;
            public static string Server;

            public static MvcHtmlString Dump()
            {
                return Util.FormatDebugValues("app", "db", DB, "dbuser", DBUser, "build", Build, "server", Server);
            }

            public static MvcHtmlString DumpSys()
            {
                return Util.FormatDebugValues("time", "local", DateTime.Now.ToLocalTime(), "utc", DateTime.UtcNow);
            }
        }

        //public static Platform Platform { get; set; }

        public static HttpApplicationState Application
        {
            get
            {
                if (HttpContext.Current != null)
                    return HttpContext.Current.Application;
                return null;
            }
        }

        public static void Restart()
        {
            string path = HttpContext.Current.Server.MapPath("") + @"\..\web.config";
            M2.Util.AspNet.Util.RestartApp(path);
        }

        public static AppSettings Settings
        {
            get
            {
                if (Application != null)
                    return (AppSettings)Application["settings"];               
                return new AppSettings();
            }

            private set
            {
                if (Application != null)
                    Application["settings"] = value;
            }
        }

        static MyApp()
        {
            Init();
        }

        public static void Init()
        {
            Settings = new AppSettings();            
        }

        public static void GetPlatformInfo()
        {
            string connStr = Kuyam.Database.DAL.DBContext.Database.Connection.ConnectionString;
            string[] toks = connStr.Split(new char[] { ';', '=' });

            string db = null;
            switch (toks[3])
            {
                case "db_32604_dev1":
                    db = "dev";
                    break;

                case "db_29601_kuyamdev":
                    db = "prod";
                    break;

                default:
                    db = "unknown";
                    break;
            }

            string dbuser = toks[7];
            MyApp.Platform.DB = db;
            MyApp.Platform.DBUser = dbuser;

            System.Configuration.Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/web.config");
            System.Web.Configuration.SystemWebSectionGroup systemWeb = config.GetSectionGroup("system.web") as System.Web.Configuration.SystemWebSectionGroup;
            Boolean debugOn = systemWeb.Compilation.Debug;
            string build = debugOn ? "debug" : "release";
            MyApp.Platform.Build = build;
        }

        public static void GetServerInfo(string server)
        {
            if (MyApp.Platform.Server != null)
                return;

            switch (server)
            {
                case "localhost":
                    server = "local";
                    break;

                case "dev.kuyam.com":
                    server = "dev";
                    break;

                case "staging.kuyam.com":
                    server = "staging";
                    break;

                case "kuyam.com":
                    server = "prod";
                    break;
            }

            MyApp.Platform.Server = server;
        }

        public static MvcHtmlString Dump()
        {
            return MyApp.Platform.Dump();
        }

    }
}