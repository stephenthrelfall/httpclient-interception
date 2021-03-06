// Copyright (c) Just Eat, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace JustEat.HttpClientInterception
{
    public static class HttpClientInterceptorOptionsTests
    {
        [Fact]
        public static async Task GetResponseAsync_Returns_Null_If_Request_Not_Registered()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://google.com");

            // Act
            HttpResponseMessage actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldBeNull();
        }

        [Fact]
        public static async Task GetResponseAsync_Returns_Null_If_Request_Method_Does_Not_Match()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions()
                .Register(HttpMethod.Get, new Uri("https://google.com/"), contentFactory: EmptyContent);

            var request = new HttpRequestMessage(HttpMethod.Delete, "https://google.com/");

            // Act
            HttpResponseMessage actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldBeNull();
        }

        [Fact]
        public static async Task GetResponseAsync_Returns_Null_If_Request_Uri_Does_Not_Match()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions()
                .Register(HttpMethod.Get, new Uri("https://google.co.uk/"), contentFactory: EmptyContent);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://google.com/");

            // Act
            HttpResponseMessage actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldBeNull();
        }

        [Fact]
        public static async Task GetResponseAsync_Returns_Null_If_Request_Uri_Does_Not_Match_Blank_Request()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions()
                .Register(HttpMethod.Get, new Uri("https://google.co.uk/"), contentFactory: EmptyContent);

            var request = new HttpRequestMessage();

            // Act
            HttpResponseMessage actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldBeNull();
        }

        [Fact]
        public static async Task GetResponseAsync_Returns_Response_If_Request_Matches_Default_Constructor()
        {
            // Arrange
            var method = HttpMethod.Get;
            var uri = new Uri("https://google.com/");

            var options = new HttpClientInterceptorOptions()
                .Register(method, uri, contentFactory: EmptyContent);

            var request = new HttpRequestMessage(method, uri);

            // Act
            HttpResponseMessage actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldNotBeNull();
            actual.RequestMessage.ShouldBe(request);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public static async Task GetResponseAsync_Returns_Response_If_Request_Matches(bool caseSensitive, bool expectNull)
        {
            // Arrange
            var method = HttpMethod.Get;
            var uriToRegister = new Uri("https://google.com/uk");
            var uriToRequest = new Uri("https://google.com/UK");

            var options = new HttpClientInterceptorOptions(caseSensitive)
                .Register(method, uriToRegister, contentFactory: EmptyContent);

            var request = new HttpRequestMessage(method, uriToRequest);

            // Act
            HttpResponseMessage actual = await options.GetResponseAsync(request);

            // Assert
            (actual == null).ShouldBe(expectNull);
        }

        [Fact]
        public static async Task GetResponseAsync_Returns_Empty_Response_If_ContentFactory_Returns_Null_Synchronous()
        {
            // Arrange
            var method = HttpMethod.Get;
            var uri = new Uri("https://google.com/");

            var options = new HttpClientInterceptorOptions()
                .Register(method, uri, () => null as byte[]);

            var request = new HttpRequestMessage(method, uri);

            // Act
            HttpResponseMessage actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldNotBeNull();
            actual.RequestMessage.ShouldBe(request);
            actual.Content.ShouldNotBeNull();
            actual.Content.Headers?.ContentLength.ShouldBe(0);
            actual.Content.Headers?.ContentType?.MediaType.ShouldBe("application/json");
        }

        [Fact]
        public static async Task GetResponseAsync_Returns_Empty_Response_If_ContentFactory_Returns_Null_Asynchronous()
        {
            // Arrange
            var method = HttpMethod.Get;
            var uri = new Uri("https://google.com/");

            var options = new HttpClientInterceptorOptions()
                .Register(method, uri, () => Task.FromResult<byte[]>(null));

            var request = new HttpRequestMessage(method, uri);

            // Act
            HttpResponseMessage actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldNotBeNull();
            actual.RequestMessage.ShouldBe(request);
            actual.Content.ShouldNotBeNull();
            actual.Content.Headers?.ContentLength.ShouldBe(0);
            actual.Content.Headers?.ContentType?.MediaType.ShouldBe("application/json");
        }

        [Fact]
        public static async Task GetResponseAsync_Returns_Empty_Response_If_ContentStream_Returns_Null_Synchronous()
        {
            // Arrange
            var method = HttpMethod.Get;
            var uri = new Uri("https://google.com/");

            var options = new HttpClientInterceptorOptions()
                .Register(method, uri, () => null as Stream);

            var request = new HttpRequestMessage(method, uri);

            // Act
            HttpResponseMessage actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldNotBeNull();
            actual.RequestMessage.ShouldBe(request);
            actual.Content.ShouldNotBeNull();
            actual.Content.Headers?.ContentLength.ShouldBe(0);
            actual.Content.Headers?.ContentType?.MediaType.ShouldBe("application/json");
        }

        [Fact]
        public static async Task GetResponseAsync_Returns_Empty_Response_If_ContentStream_Returns_Null_Asynchronous()
        {
            // Arrange
            var method = HttpMethod.Get;
            var uri = new Uri("https://google.com/");

            var options = new HttpClientInterceptorOptions()
                .Register(method, uri, () => Task.FromResult<Stream>(null));

            var request = new HttpRequestMessage(method, uri);

            // Act
            HttpResponseMessage actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldNotBeNull();
            actual.RequestMessage.ShouldBe(request);
            actual.Content.ShouldNotBeNull();
            actual.Content.Headers?.ContentLength.ShouldBe(0);
            actual.Content.Headers?.ContentType?.MediaType.ShouldBe("application/json");
        }

        [Fact]
        public static async Task GetResponseAsync_Returns_Empty_Response_If_Custom_Response_Header()
        {
            // Arrange
            var method = HttpMethod.Get;
            var uri = new Uri("https://google.com/");

            var headers = new Dictionary<string, string>()
            {
                { "a", "b" },
                { "c", "d" },
            };

            var options = new HttpClientInterceptorOptions()
                .Register(method, uri, contentFactory: Array.Empty<byte>, responseHeaders: headers);

            var request = new HttpRequestMessage(method, uri);

            // Act
            HttpResponseMessage actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldNotBeNull();
            actual.RequestMessage.ShouldBe(request);
            actual.Content.ShouldNotBeNull();
            actual.Content.Headers.ContentLength.ShouldBe(0);
            actual.Headers.GetValues("a").ShouldBe(new[] { "b" });
            actual.Headers.GetValues("c").ShouldBe(new[] { "d" });
        }

        [Fact]
        public static async Task GetResponseAsync_Returns_Empty_Response_If_Custom_Response_Header_Is_Null()
        {
            // Arrange
            var method = HttpMethod.Get;
            var uri = new Uri("https://google.com/");

            IDictionary<string, string> responseHeaders = null;

            var options = new HttpClientInterceptorOptions()
                .Register(method, uri, contentFactory: Array.Empty<byte>, responseHeaders: responseHeaders);

            var request = new HttpRequestMessage(method, uri);

            // Act
            HttpResponseMessage actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldNotBeNull();
            actual.RequestMessage.ShouldBe(request);
            actual.Content.ShouldNotBeNull();
            actual.Content.Headers.ContentLength.ShouldBe(0);
        }

        [Fact]
        public static async Task GetResponseAsync_Returns_Empty_Response_If_Custom_Response_Headers()
        {
            // Arrange
            var method = HttpMethod.Get;
            var uri = new Uri("https://google.com/");

            var responseHeaders = new Dictionary<string, IEnumerable<string>>()
            {
                { "a", new[] { "b" } },
                { "c", new[] { "d", "e" } },
            };

            var options = new HttpClientInterceptorOptions()
                .Register(method, uri, contentFactory: EmptyContent, responseHeaders: responseHeaders);

            var request = new HttpRequestMessage(method, uri);

            // Act
            var actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldNotBeNull();
            actual.RequestMessage.ShouldBe(request);
            actual.Content.ShouldNotBeNull();
            actual.Content.Headers.ContentLength.ShouldBe(0);
            actual.Headers.GetValues("a").ShouldBe(new[] { "b" });
            actual.Headers.GetValues("c").ShouldBe(new[] { "d", "e" });
        }

        [Fact]
        public static async Task GetResponseAsync_Returns_Empty_Response_If_Custom_Response_Header_Using_Stream()
        {
            // Arrange
            var method = HttpMethod.Get;
            var uri = new Uri("https://google.com/");

            var headers = new Dictionary<string, string>()
            {
                { "a", "b" },
                { "c", "d" },
            };

            var options = new HttpClientInterceptorOptions()
                .Register(method, uri, contentStream: () => Stream.Null, responseHeaders: headers);

            var request = new HttpRequestMessage(method, uri);

            // Act
            HttpResponseMessage actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldNotBeNull();
            actual.RequestMessage.ShouldBe(request);
            actual.Content.ShouldNotBeNull();
            actual.Content.Headers.ContentLength.ShouldBe(0);
            actual.Headers.GetValues("a").ShouldBe(new[] { "b" });
            actual.Headers.GetValues("c").ShouldBe(new[] { "d" });
        }

        [Fact]
        public static async Task GetResponseAsync_Returns_Empty_Response_If_Custom_Response_Headers_Using_Stream()
        {
            // Arrange
            var method = HttpMethod.Get;
            var uri = new Uri("https://google.com/");

            var responseHeaders = new Dictionary<string, IEnumerable<string>>()
            {
                { "a", new[] { "b" } },
                { "c", new[] { "d", "e" } },
            };

            var options = new HttpClientInterceptorOptions()
                .Register(method, uri, contentStream: EmptyStream, responseHeaders: responseHeaders);

            var request = new HttpRequestMessage(method, uri);

            // Act
            var actual = await options.GetResponseAsync(request);

            // Assert
            actual.ShouldNotBeNull();
            actual.RequestMessage.ShouldBe(request);
            actual.Content.ShouldNotBeNull();
            actual.Content.Headers.ContentLength.ShouldBe(0);
            actual.Headers.GetValues("a").ShouldBe(new[] { "b" });
            actual.Headers.GetValues("c").ShouldBe(new[] { "d", "e" });
        }

        [Fact]
        public static async Task HttpClient_Registrations_Can_Be_Removed()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions()
                .RegisterGet("https://google.com/", new { message = "Hello world!" })
                .RegisterGet("https://google.co.uk/", new { message = "Hello world!" });

            // Act and Assert
            await HttpAssert.GetAsync(options, "https://google.com/");
            await HttpAssert.GetAsync(options, "https://google.com/");
            await HttpAssert.GetAsync(options, "https://google.co.uk/");
            await HttpAssert.GetAsync(options, "https://google.co.uk/");

            // Arrange
            options.DeregisterGet("https://google.com/")
                   .DeregisterGet("https://google.com/");

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => HttpAssert.GetAsync(options, "https://google.com/"));
            await Assert.ThrowsAsync<HttpRequestException>(() => HttpAssert.GetAsync(options, "https://google.com/"));
            await HttpAssert.GetAsync(options, "https://google.co.uk/");
            await HttpAssert.GetAsync(options, "https://google.co.uk/");
        }

        [Fact]
        public static async Task Options_Can_Be_Scoped()
        {
            // Arrange
            var url = "https://google.com/";
            var payload = new MyObject() { Message = "Hello world!" };

            var options = new HttpClientInterceptorOptions()
                .RegisterGet(url, payload, statusCode: HttpStatusCode.NotFound);

            // Act - begin first scope
            using (options.BeginScope())
            {
                // Assert - original registration
                await HttpAssert.GetAsync(options, url, HttpStatusCode.NotFound);

                // Arrange - first update to registration
                options.RegisterGet(url, payload, statusCode: HttpStatusCode.InternalServerError);

                // Assert - first updated registration
                await HttpAssert.GetAsync(options, url, HttpStatusCode.InternalServerError);

                // Act - begin second scope
                using (options.BeginScope())
                {
                    // Arrange - second update to registration
                    options.RegisterGet(url, payload, statusCode: HttpStatusCode.RequestTimeout);

                    // Assert - second updated registration
                    await HttpAssert.GetAsync(options, url, HttpStatusCode.RequestTimeout);
                }

                // Assert - first updated registration
                await HttpAssert.GetAsync(options, url, HttpStatusCode.InternalServerError);
            }

            // Assert - original registration
            await HttpAssert.GetAsync(options, url, HttpStatusCode.NotFound);
        }

        [Fact]
        public static async Task Options_Can_Be_Cloned()
        {
            // Arrange
            var url = "https://google.com/";
            var payload = new MyObject() { Message = "Hello world!" };

            var options = new HttpClientInterceptorOptions()
                .RegisterGet(url, payload, statusCode: HttpStatusCode.NotFound);

            // Act
            var clone = options.Clone()
                .RegisterGet(url, payload, statusCode: HttpStatusCode.InternalServerError);

            // Assert
            clone.ThrowOnMissingRegistration.ShouldBe(options.ThrowOnMissingRegistration);
            await HttpAssert.GetAsync(options, url, HttpStatusCode.NotFound);
            await HttpAssert.GetAsync(clone, url, HttpStatusCode.InternalServerError);

            // Arrange
            options.ThrowOnMissingRegistration = true;
            options.Clear();

            // Act and Assert
            clone.ThrowOnMissingRegistration.ShouldNotBe(options.ThrowOnMissingRegistration);
            await Assert.ThrowsAsync<InvalidOperationException>(() => HttpAssert.GetAsync(options, url, HttpStatusCode.InternalServerError));
            await HttpAssert.GetAsync(clone, url, HttpStatusCode.InternalServerError);

            // Arrange
            options = new HttpClientInterceptorOptions()
            {
                ThrowOnMissingRegistration = true,
            };

            // Act
            clone = options.Clone();

            // Assert
            clone.ThrowOnMissingRegistration.ShouldBe(options.ThrowOnMissingRegistration);
        }

        [Fact]
        public static void Deregister_Throws_If_Method_Is_Null()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();

            HttpMethod method = null;
            Uri uri = new Uri("https://www.just-eat.co.uk");

            // Act and Assert
            Assert.Throws<ArgumentNullException>("method", () => options.Deregister(method, uri));
        }

        [Fact]
        public static void Deregister_Throws_If_Uri_Is_Null()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();

            HttpMethod method = HttpMethod.Get;
            Uri uri = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("uri", () => options.Deregister(method, uri));
        }

        [Fact]
        public static void Register_Throws_If_Method_Is_Null()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();

            HttpMethod method = null;
            Uri uri = new Uri("https://www.just-eat.co.uk");

            // Act and Assert
            Assert.Throws<ArgumentNullException>("method", () => options.Register(method, uri, contentFactory: EmptyContent));
            Assert.Throws<ArgumentNullException>("method", () => options.Register(method, uri, contentStream: EmptyStream));
        }

        [Fact]
        public static void Register_Throws_If_Uri_Is_Null()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();

            HttpMethod method = HttpMethod.Get;
            Uri uri = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("uri", () => options.Register(method, uri, contentFactory: EmptyContent));
            Assert.Throws<ArgumentNullException>("uri", () => options.Register(method, uri, contentStream: EmptyStream));
        }

        [Fact]
        public static void Register_Throws_If_ContentFactory_Is_Null_Synchronous()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();

            HttpMethod method = HttpMethod.Get;
            Uri uri = new Uri("https://www.just-eat.co.uk");
            Func<byte[]> contentFactory = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("contentFactory", () => options.Register(method, uri, contentFactory));
        }

        [Fact]
        public static void Register_Throws_If_ContentFactory_Is_Null_Asynchronous()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();

            HttpMethod method = HttpMethod.Get;
            Uri uri = new Uri("https://www.just-eat.co.uk");
            Func<Task<byte[]>> contentFactory = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("contentFactory", () => options.Register(method, uri, contentFactory));
        }

        [Fact]
        public static void Register_Throws_If_ContentStream_Is_Null_Synchronous()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();

            HttpMethod method = HttpMethod.Get;
            Uri uri = new Uri("https://www.just-eat.co.uk");
            Func<Stream> contentStream = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("contentStream", () => options.Register(method, uri, contentStream));
        }

        [Fact]
        public static void Register_Throws_If_ContentStream_Is_Null_Asynchronous()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();

            HttpMethod method = HttpMethod.Get;
            Uri uri = new Uri("https://www.just-eat.co.uk");
            Func<Task<Stream>> contentStream = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("contentStream", () => options.Register(method, uri, contentStream));
        }

        [Fact]
        public static async Task GetResponseAsync_Throws_If_Request_Is_Null()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();
            HttpRequestMessage request = null;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>("request", () => options.GetResponseAsync(request));
        }

        [Fact]
        public static void Register_Throws_If_Builder_Is_Null()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();
            HttpRequestInterceptionBuilder builder = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("builder", () => options.Register(builder));
        }

        [Fact]
        public static async Task GetResponseAsync_Throws_If_Content_Cannot_Be_Created()
        {
            // Arrange
            var method = HttpMethod.Get;
            var uri = new Uri("https://google.com/");

            var options = new HttpClientInterceptorOptions()
                .Register(method, uri, contentFactory: () => throw new NotImplementedException());

            var request = new HttpRequestMessage(method, uri);

            // Act and Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => options.GetResponseAsync(request));
        }

        [Fact]
        public static async Task GetResponseAsync_Throws_If_ContentStream_Cannot_Be_Created()
        {
            // Arrange
            var method = HttpMethod.Get;
            var uri = new Uri("https://google.com/");

            var options = new HttpClientInterceptorOptions()
                .Register(method, uri, contentStream: () => throw new NotImplementedException());

            var request = new HttpRequestMessage(method, uri);

            // Act and Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => options.GetResponseAsync(request));
        }

        [Fact]
        public static async Task Can_Use_Extensibility_For_Request_Header_Matching()
        {
            // Arrange
            var builder = new HttpRequestInterceptionBuilder()
                .ForHttps()
                .ForHost("api.github.com")
                .ForPath("orgs/justeat/repos")
                .ForQuery("type=private");

            var options = new HeaderMatchingOptions(
                (p) => p.Accept.FirstOrDefault()?.MediaType == "application/vnd.github.v3+json" &&
                       p.Authorization?.Scheme == "token" &&
                       p.Authorization?.Parameter == "my-token" &&
                       p.UserAgent?.FirstOrDefault()?.Product.Name == "My-App" &&
                       p.UserAgent?.FirstOrDefault()?.Product.Version == "1.0.0");

            options.Register(builder);

            var url = "https://api.github.com/orgs/justeat/repos?type=private";
            var headers = new Dictionary<string, string>()
            {
                { "accept", "application/vnd.github.v3+json" },
                { "authorization", "token my-token" },
                { "user-agent", "My-App/1.0.0" },
            };

            // Act and Assert
            await HttpAssert.GetAsync(options, url, responseHeaders: headers);
            await Assert.ThrowsAsync<HttpRequestException>(() => HttpAssert.GetAsync(options, url));

            // Arrange
            headers = new Dictionary<string, string>()
            {
                { "accept", "application/vnd.github.v3+json" },
                { "authorization", "token my-token" },
                { "user-agent", "My-App/2.0.0" },
            };

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => HttpAssert.GetAsync(options, url, responseHeaders: headers));

            // Arrange
            headers = new Dictionary<string, string>()
            {
                { "accept", "application/vnd.github.v3+json" },
                { "user-agent", "My-App/1.0.0" },
            };

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => HttpAssert.GetAsync(options, url, responseHeaders: headers));
        }

        private static Task<byte[]> EmptyContent() => Task.FromResult(Array.Empty<byte>());

        private static Task<Stream> EmptyStream() => Task.FromResult(Stream.Null);

        private sealed class MyObject
        {
            public string Message { get; set; }
        }

        private sealed class HeaderMatchingOptions : HttpClientInterceptorOptions
        {
            private readonly Predicate<HttpRequestHeaders> _matchHeaders;

            internal HeaderMatchingOptions(Predicate<HttpRequestHeaders> matchHeaders)
            {
                _matchHeaders = matchHeaders;
            }

            public override async Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request)
            {
                HttpResponseMessage response = await base.GetResponseAsync(request);

                if (response != null && _matchHeaders(request.Headers))
                {
                    return response;
                }

                return null;
            }
        }
    }
}
