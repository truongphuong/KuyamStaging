using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Kuyam.Domain;
using Kuyam.Domain.GiftCardServices;
using Kuyam.Domain.HotelVisits;
using Kuyam.Domain.LandingePageServices;
using Kuyam.Domain.MediaServices;
using Kuyam.Domain.PromoCodeServices;
using Kuyam.Domain.RequestTimeSlotServices;
using Kuyam.Domain.SmsServices;
using Kuyam.Repository;
using Kuyam.Repository.Interface;
using Kuyam.Repository.Base;
using System.Data.Entity;
using Kuyam.Database;
using Kuyam.Repository.Infrastructure.DependencyManagement;
using Kuyam.Repository.Infrastructure;
using Kuyam.WebUI.Models;
using System.Web.Security;
using Kuyam.Domain.BlogServices;
using Kuyam.Domain.KuyamServices;
using Kuyam.WebUI.Helpers;
using Kuyam.Domain.MessageServcies;
using Kuyam.Domain.HipmobServices;
using Kuyam.WebUI.Routes;
using Kuyam.Domain.Seo;
using Kuyam.WebUI.Sitemap.Autofac.Modules;
using Kuyam.Domain.HomeServices;
using Kuyam.Domain.CompanyProfileServices;
using Kuyam.Domain.SearchServices;
using Kuyam.Domain.OfferServices;

namespace Kuyam.WebUI.App_Start
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            //web helper
            builder.RegisterType<WebHelper>().As<IWebHelper>().InstancePerHttpRequest();

            //controllers
            builder.RegisterControllers(typeFinder.GetAssemblies().ToArray());

            //data layer           

            builder.Register<DbContext>(c => Instance()).InstancePerHttpRequest();

            //builder.RegisterType<EFUnitOfWork>().As<IUnitOfWork>().InstancePerHttpRequest();

            builder.RegisterGeneric(typeof(EFRepository<>)).As(typeof(IRepository<>)).InstancePerHttpRequest();

            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().SingleInstance();

            //services
            builder.RegisterType<AppointmentService>().As<IAppointmentService>().InstancePerHttpRequest();
            builder.RegisterType<EmailSender>().InstancePerHttpRequest();
            builder.RegisterType<CustService>().InstancePerHttpRequest();
            builder.RegisterType<CompanyProfileService>().InstancePerHttpRequest();
            builder.RegisterType<HomeService>().As<IHomeService>().InstancePerHttpRequest();
            builder.RegisterType<FormsAuthenticationService>().As<IFormsAuthenticationService>().InstancePerHttpRequest();
            builder.RegisterType<AccountMembershipService>().As<IMembershipService>().InstancePerHttpRequest();
            builder.RegisterType<AdminService>().InstancePerHttpRequest();
            builder.RegisterType<CompanySearchService>().InstancePerHttpRequest();
            builder.RegisterType<ClassService>().InstancePerHttpRequest();
            builder.RegisterType<OrderService>().InstancePerHttpRequest();
            builder.RegisterType<PdfService>().InstancePerHttpRequest();
            builder.RegisterType<ImportService>().InstancePerHttpRequest();
            builder.RegisterType<CalendarService>().InstancePerHttpRequest();
            builder.RegisterType<NotificationService>().InstancePerHttpRequest();
            builder.RegisterType<GettyImageService>().InstancePerHttpRequest();
            builder.RegisterType<CustGettyImageService>().InstancePerHttpRequest();
            builder.RegisterType<HotelService>().InstancePerHttpRequest();
            builder.RegisterType<BlogService>().InstancePerHttpRequest();
            builder.RegisterType<ExportImportService>().InstancePerLifetimeScope();
            builder.RegisterType<ProfileCompanyService>().As<IProfileCompanyService>().InstancePerHttpRequest();

            //Blog Services
            builder.RegisterType<BlogPostService>().As<IBlogPostService>().InstancePerHttpRequest();
            builder.RegisterType<BlogCategoryService>().As<IBlogCategoryService>().InstancePerHttpRequest();
            builder.RegisterType<BlogPostCategoryService>().As<IBlogPostCategoryService>().InstancePerHttpRequest();
            builder.RegisterType<BlogUserService>().As<IBlogUserService>().InstancePerHttpRequest();
            builder.RegisterType<BlogCommentService>().As<IBlogCommentService>().InstancePerHttpRequest();
            builder.RegisterType<BlogBookmarkService>().As<IBookmarkService>().InstancePerHttpRequest();
            builder.RegisterType<CategoryFeaturedService>().As<ICategoryFeaturedService>().InstancePerHttpRequest();
            builder.RegisterType<BlogCategoryService>().As<IBlogCategoryService>().InstancePerHttpRequest();
            builder.RegisterType<CategoryFeaturedHotelService>().As<ICategoryFeaturedHotelService>().InstancePerHttpRequest();
            builder.RegisterType<FeaturedCompanyService>().As<IFeaturedCompanyService>().InstancePerHttpRequest();
            builder.RegisterType<BlogAuthorHelper>().InstancePerLifetimeScope();

            //Gift Card Services
            builder.RegisterType<GiftCardServices>().As<IGiftCardServices>().InstancePerHttpRequest();

            //Promo code Services
            builder.RegisterType<PromoCodeServices>().As<IPromoCodeServices>().InstancePerHttpRequest();

            builder.RegisterType<ATTSMSProvider>().As<ISMSProvider>().InstancePerHttpRequest();

            builder.RegisterType<SmsServices>().As<ISmsServices>().InstancePerHttpRequest();

            //Time slot
            builder.RegisterType<RequestTimeSlotServices>().As<IRequestTimeSlotServices>().InstancePerHttpRequest();

            //Landing page
            builder.RegisterType<LandingPageServices>().As<ILandingPageServices>().InstancePerHttpRequest();

            builder.RegisterType<HipmobService>().As<IHipmobService>().InstancePerHttpRequest();

            builder.RegisterType<SeoFriendlyUrlService>().As<ISeoFriendlyUrlService>().InstancePerHttpRequest();
            builder.RegisterType<HotelVisitService>().As<IHotelVisitService>().InstancePerHttpRequest();
            builder.RegisterType<SearchService>().As<ISearchService>().InstancePerHttpRequest();

            builder.RegisterType<OfferService>().As<IOfferService>().InstancePerHttpRequest();

            
            
            #region Sitemap
            // Register modules
            builder.RegisterModule(new MvcSiteMapProviderModule()); // Required
            builder.RegisterModule(new MvcModule()); // Required by MVC. Typically already part of your setup (double check the contents of the module).

            #endregion
            

            

        }

        private kuyamEntities Instance()
        {
            var dbContext = new kuyamEntities(DAL.GetConnectionString());
            return dbContext;
        }

        public int Order
        {
            get { return 0; }
        }
    }
}