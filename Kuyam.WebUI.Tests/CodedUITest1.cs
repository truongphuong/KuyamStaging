using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using NUnit.Framework;
using Moq;
using Kuyam.WebUI;
using Kuyam.WebUI.Controllers;
using Kuyam.Domain.BlogServices;
using Kuyam.Domain.KuyamServices;
using Kuyam.Domain.HomeServices;
using System.Web.Mvc;
using Kuyam.WebUI.Models;
using System.Web;
using System.Web.Routing;
using System.Collections.Specialized;
using System.Configuration;
using Kuyam.Domain.Services;
using Kuyam.Database;
using System.Net;


namespace Kuyam.WebUI.Tests
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [TestFixture]
    public class CodedUITest1
    {
        private HomeController _controller;

        [SetUp]
        public virtual void SetUp()
        {

            var _postService = new Mock<IBlogPostService>();
            var _featuredCompanyService = new Mock<IFeaturedCompanyService>();
            var _profileCompanyService = new Mock<IProfileCompanyService>();
            var _homeService = new Mock<IHomeService>();
            _controller = new HomeController(_postService.Object, _featuredCompanyService.Object, _profileCompanyService.Object, _homeService.Object);

        }

        [TearDown]
        public virtual void TearDown()
        {

        }        

        [Test]
        public void WebUITestMethod1()
        {           
            var request = new Mock<HttpRequestBase>();
            // Not working - IsAjaxRequest() is static extension method and cannot be mocked
            // request.Setup(x => x.IsAjaxRequest()).Returns(true /* or false */);
            // use this
            request.SetupGet(x => x.Headers).Returns(new WebHeaderCollection { { "X-Requested-With", "XMLHttpRequest" } });
            request.Setup(x => x.ApplicationPath).Returns("/");
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);

            _controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), _controller);
            var result = _controller.Index(1) as ViewResult;
            Assert.IsNotNull(result, "Expected view");
        }

    }
}
