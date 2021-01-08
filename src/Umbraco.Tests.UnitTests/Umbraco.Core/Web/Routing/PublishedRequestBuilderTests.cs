using System;
using System.Collections.Generic;
using System.Globalization;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Web.Routing
{
    [TestFixture]
    public class PublishedRequestBuilderTests
    {
        private readonly Uri _baseUri = new Uri("https://example.com");

        private IPublishedRequestBuilder GetBuilder() => new PublishedRequestBuilder(
            _baseUri,
            Mock.Of<IFileService>());

        [Test]
        public void Setting_Published_Content_Clears_Template_And_Redirect()
        {
            IPublishedRequestBuilder sut = GetBuilder();
            sut.SetTemplate(Mock.Of<ITemplate>());

            Assert.IsNotNull(sut.Template);

            sut.SetInternalRedirect(Mock.Of<IPublishedContent>());

            Assert.IsNull(sut.Template);
            Assert.IsTrue(sut.IsInternalRedirect);

            sut.SetTemplate(Mock.Of<ITemplate>());
            sut.SetPublishedContent(Mock.Of<IPublishedContent>());

            Assert.IsNull(sut.Template);
            Assert.IsFalse(sut.IsInternalRedirect);
        }

        [Test]
        public void Setting_Domain_Also_Sets_Culture()
        {
            IPublishedRequestBuilder sut = GetBuilder();

            Assert.IsNull(sut.Culture);

            sut.SetDomain(
                new DomainAndUri(
                    new Domain(1, "test", 2, CultureInfo.GetCultureInfo("en-AU"), false), new Uri("https://example.com/en-au")));

            Assert.IsNotNull(sut.Domain);
            Assert.IsNotNull(sut.Culture);
        }

        [Test]
        public void Builds_All_Values()
        {
            IPublishedRequestBuilder sut = GetBuilder();

            IPublishedContent content = Mock.Of<IPublishedContent>(x => x.Id == 1);
            ITemplate template = Mock.Of<ITemplate>(x => x.Id == 1);
            string[] cacheExt = new[] { "must-revalidate" };
            var auCulture = CultureInfo.GetCultureInfo("en-AU");
            var usCulture = CultureInfo.GetCultureInfo("en-US");
            var domain = new DomainAndUri(
                    new Domain(1, "test", 2, auCulture, false), new Uri("https://example.com/en-au"));
            IReadOnlyDictionary<string, string> headers = new Dictionary<string, string> { ["Hello"] = "world" };
            var redirect = "https://test.com";

            sut
                .SetNoCacheHeader(true)
                .SetCacheExtensions(cacheExt)
                .SetDomain(domain)
                .SetCulture(usCulture)
                .SetHeaders(headers)
                .SetInternalRedirect(content)
                .SetRedirect(redirect)
                .SetTemplate(template);

            IPublishedRequest request = sut.Build();

            Assert.AreEqual(true, request.SetNoCacheHeader);
            Assert.AreEqual(cacheExt, request.CacheExtensions);
            Assert.AreEqual(usCulture, request.Culture);
            Assert.AreEqual(domain, request.Domain);
            Assert.AreEqual(headers, request.Headers);
            Assert.AreEqual(true, request.IsInternalRedirect);
            Assert.AreEqual(content, request.PublishedContent);
            Assert.AreEqual(redirect, request.RedirectUrl);
            Assert.AreEqual(302, request.ResponseStatusCode);
            Assert.AreEqual(template, request.Template);
            Assert.AreEqual(_baseUri, request.Uri);
        }
    }
}