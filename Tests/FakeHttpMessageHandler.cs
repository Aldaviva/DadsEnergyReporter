using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PowerGuideReporter
{
    public abstract class FakeHttpMessageHandler: HttpMessageHandler
    {
        protected sealed override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return FakeSendAsync(request);
        }

        public abstract Task<HttpResponseMessage> FakeSendAsync(HttpRequestMessage request);
    }
}
