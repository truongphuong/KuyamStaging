using Kuyam.WebUI.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kuyam.Domain.Seo;

namespace Kuyam.WebUI
{
    public class RouteConfig : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {              
            //default route
            routes.MapRoute("home", "", new { controller = "Home", action = "Index" });
            routes.MapRoute("about", "about", new { controller = "Home", action = "About" });
            routes.MapRoute("our-story", "our-story", new { controller = "Home", action = "About" });
            routes.MapRoute("contact", "contact", new { controller = "Home", action = "Contact" });

            routes.MapRoute("CompanyAppointmentPage", "CompanyAppointment", new { controller = "CompanyAppointment", action = "Index" });

            routes.MapRoute("AppointmentPage", "Appointment", new { controller = "Appointment", action = "Index" });

            routes.MapRoute("CompanyAppointmentCalendarPage", "CompanyAppointmentCalendar", new { controller = "CompanyAppointmentCalendar", action = "Index" });

            routes.MapRoute("calendarviewPage", "calendarview", new { controller = "calendarview", action = "Index" });

            routes.MapRoute("adminPage", "Admin",  new { controller = "Admin", action = "Index" });
            routes.MapRoute("ConciergePage", "Concierge", new { controller = "Concierge", action = "Index" });
            routes.MapRoute("AdminLandingPage", "AdminLandingPage", new { controller = "AdminLandingPage", action = "Index" });

            routes.MapRoute("CompanyPage", "Company", new { controller = "Company", action = "Index" });

            routes.MapRoute("CalendarSettingPage", "CalendarSetting", new { controller = "CalendarSetting", action = "Index" });

            routes.MapRoute("UserSettingPage", "Setting", new { controller = "Setting", action = "Index" });

            routes.MapRoute("TTASMS", "debug", new { controller = "SmsAtt", action = "Index" });

            routes.MapRoute("userlist", "admin/userlist", new { controller = "admin", action = "userlist" });

            routes.MapRoute("conciergeGustlist", "Concierge/Index", new { controller = "Concierge", action = "Index" });

            routes.MapRoute("conciergeAppointment", "Concierge/Appointment", new { controller = "Concierge", action = "Appointment" });

            routes.MapRoute("conciergeProposals", "Concierge/Proposals", new { controller = "Concierge", action = "Proposals" });            

            routes.MapRoute("ViewDetails", "Admin/AdminUserDetail/{id}/{userListPageIndex}/{key}/{searchType}",
                  new
                  {
                      controller = "Admin",
                      action = "AdminUserDetail",
                      id = UrlParameter.Optional,
                      userListPageIndex = UrlParameter.Optional,
                      key = UrlParameter.Optional,
                      searchType = UrlParameter.Optional
                  });

            routes.MapRoute("ViewZipCodeDetails", "Admin/ZipCodeDetail/{id}/{page}/{key}/{searchType}",
                             new
                             {
                                 controller = "Admin",
                                 action = "ZipCodeDetail",
                                 id = UrlParameter.Optional,
                                 userListPageIndex = UrlParameter.Optional,
                                 key = UrlParameter.Optional,
                                 searchType = UrlParameter.Optional
                             });

            routes.MapRoute("error", "error", new { controller = "Error", action = "Error404" });

            routes.MapRoute("Posts", "Post/{id}",
                             new
                             {
                                 controller = "Blog",
                                 action = "Post",
                                 id = UrlParameter.Optional
                             });


            routes.MapRoute(
                "landing page",
                "landing/{id}",
                new { controller = "Landing", action = "Index" }
                );

            routes.MapRoute(
                "Company Review",
                "CompanyProfile/Review",
                new { controller = "CompanyProfile", action = "Review" }
                );

           routes.MapRoute(
                "BookingDescription",
                "book/{companyUrlName}/Description",
                new { controller = "Book", action = "Description" }
                );
            routes.MapRoute(
                "BookingPhoto",
                "book/{companyUrlName}/Photo",
                new { controller = "Book", action = "Photo" }
                );
            routes.MapRoute(
                "BookingReview",
                "book/{companyUrlName}/Review",
                new { controller = "Book", action = "Review" }
                );

            routes.MapRoute(
                "BookingPackage",
                "book/{companyUrlName}/Package",
                new { controller = "Book", action = "Package" }
                );

            routes.MapRoute(
                "BookingAvailability",
                "book/{companyUrlName}/Availability",
                new { controller = "Book", action = "Availability" }
                );
            routes.MapRoute(
                "BookingClass",
                "book/{companyUrlName}/class",
                new { controller = "Book", action = "Class" }
                );

            routes.MapRoute(
                "Booking - Company",
                "book/{companyUrlName}",
                new { controller = "Book", action = "Index" }
                );
            

            // custom routes for seo
            routes.MapGenericPathRoute("GenericUrl", "{generic_se_name}", new { controller = "Error", action = "GenericUrl" });

            routes.MapGenericPathRoute("Slug", "{SeName}", new { controller = "CompanyProfile", action = "Availability" });

            routes.MapGenericPathRoute("Availability", "availability/{SeName}", new { controller = "CompanyProfile", action = "Availability" });
            routes.MapGenericPathRoute("Class", "class/{SeName}", new { controller = "CompanyProfile", action = "Class" });
            routes.MapGenericPathRoute("Description", "description/{SeName}", new { controller = "CompanyProfile", action = "Description" });
            routes.MapGenericPathRoute("Photo", "photo/{SeName}", new { controller = "CompanyProfile", action = "Photo" });
            routes.MapGenericPathRoute("Review", "review/{SeName}", new { controller = "CompanyProfile", action = "Review" });
            routes.MapGenericPathRoute("package", "package/{SeName}", new { controller = "CompanyProfile", action = "package" });

            routes.MapGenericPathRoute("SearchPage", "{SeName}", new { controller = "company", action = "companysearch" });

            routes.MapGenericPathRoute("HomePage", "{SeName}", new { controller = "Home", action = "Index" });

            routes.MapGenericPathRoute("blogPost", "{SeName}", new { controller = "Blog", action = "Post" });            
          
        }

        public int Priority
        {
            get { return 0; }
        }
    }
}