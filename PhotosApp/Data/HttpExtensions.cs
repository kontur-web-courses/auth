using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PhotosApp.Data
{
    public static class HttpExtensions
    {
        public static async Task<HttpRequestMessage> CopyAsync(this HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version
            };

            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            foreach (var option in request.Options)
                clone.Options.Set(new HttpRequestOptionsKey<object>(option.Key), option.Value);

            var memoryStream = new MemoryStream();
            if (request.Content != null)
            {
                await request.Content.CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStream.Position = 0;
                clone.Content = new StreamContent(memoryStream);

                if (request.Content.Headers != null)
                    foreach (var header in request.Content.Headers)
                        clone.Content.Headers.Add(header.Key, header.Value);
            }

            return clone;
        }
    }
}
