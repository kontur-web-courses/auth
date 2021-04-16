using AutoMapper;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using PhotosApp.Clients.Exceptions;
using PhotosApp.Clients.Models;
using PhotosApp.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace PhotosApp.Clients
{
    public class RemotePhotosRepository : IPhotosRepository
    {
        private readonly string serviceUrl;
        private readonly IMapper mapper;

        public RemotePhotosRepository(IOptions<PhotosServiceOptions> options, IMapper mapper)
        {
            serviceUrl = options.Value.ServiceUrl;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<PhotoEntity>> GetPhotosAsync(string ownerId)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = BuildUri($"/api/photos", $"ownerId={UrlEncode(ownerId)}");
            request.Headers.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);

            var response = await SendAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var content = await response.Content.ReadAsStringAsync();
                    var photos = JsonConvert.DeserializeObject<PhotoDto[]>(content);
                    return mapper.Map<PhotoEntity[]>(photos);
                case HttpStatusCode.NotFound:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    return new PhotoEntity[0];
                default:
                    throw new UnexpectedStatusCodeException(response.StatusCode);
            }
        }

        public async Task<PhotoEntity> GetPhotoMetaAsync(Guid id)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = BuildUri($"/api/photos/{UrlEncode(id)}/meta");
            request.Headers.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);

            var response = await SendAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var content = await response.Content.ReadAsStringAsync();
                    var photo = JsonConvert.DeserializeObject<PhotoDto>(content);
                    return mapper.Map<PhotoEntity>(photo);
                case HttpStatusCode.NotFound:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    return null;
                default:
                    throw new UnexpectedStatusCodeException(response.StatusCode);
            }
        }

        public async Task<PhotoContent> GetPhotoContentAsync(Guid id)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = BuildUri($"/api/photos/{UrlEncode(id)}/content");
            request.Headers.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);

            var response = await SendAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var content = await response.Content.ReadAsByteArrayAsync();
                    var fileName = response.Content.Headers.ContentDisposition.FileName;
                    var contentType = response.Content.Headers.ContentType.MediaType;
                    return new PhotoContent
                    {
                        FileName = fileName,
                        ContentType = contentType,
                        Content = content
                    };
                case HttpStatusCode.NotFound:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    return null;
                default:
                    throw new UnexpectedStatusCodeException(response.StatusCode);
            }
        }
        
        public async Task<bool> AddPhotoAsync(string title, string ownerId, byte[] content)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = BuildUri($"/api/photos");
            request.Headers.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);
            request.Content = SerializeToJsonContent(new PhotoToAddDto
            {
                Title = title,
                OwnerId = ownerId,
                Base64Content = Convert.ToBase64String(content)
            });

            var response = await SendAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.NoContent:
                    return true;
                case HttpStatusCode.Conflict:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    return false;
                default:
                    throw new UnexpectedStatusCodeException(response.StatusCode);
            }
        }

        public async Task<bool> UpdatePhotoAsync(PhotoEntity photo)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Put;
            request.RequestUri = BuildUri($"/api/photos/{UrlEncode(photo.Id)}");
            request.Headers.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);
            request.Content = SerializeToJsonContent(new PhotoToUpdateDto
            {
                Title = photo.Title
            });

            var response = await SendAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.NoContent:
                case HttpStatusCode.NotFound:
                    return true;
                case HttpStatusCode.Conflict:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    return false;
                default:
                    throw new UnexpectedStatusCodeException(response.StatusCode);
            }
        }

        public async Task<bool> DeletePhotoAsync(PhotoEntity photo)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Delete;
            request.RequestUri = BuildUri($"/api/photos/{UrlEncode(photo.Id)}");
            request.Headers.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);

            var response = await SendAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.NoContent:
                case HttpStatusCode.NotFound:
                    return true;
                case HttpStatusCode.Conflict:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    return false;
                default:
                    throw new UnexpectedStatusCodeException(response.StatusCode);
            }
        }

        private Uri BuildUri(string path, string query = null)
            => new UriBuilder(serviceUrl) { Path = path, Query = query }.Uri;
        private string UrlEncode(object arg) => arg != null ? HttpUtility.UrlEncode(arg.ToString()) : null;

        private static ByteArrayContent SerializeToJsonContent(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            var bytes = Encoding.UTF8.GetBytes(json);
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);
            return content;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var accessToken = await GetAccessTokenByClientCredentialsAsync();
            var httpClient = new HttpClient();
            httpClient.SetBearerToken(accessToken);
            var response = await httpClient.SendAsync(request);
            return response;
        }

        private static async Task<string> GetAccessTokenByClientCredentialsAsync()
        {
            var httpClient = new HttpClient();
            // NOTE: Получение информации о сервере авторизации, в частности, адреса token endpoint.
            var disco = await httpClient.GetDiscoveryDocumentAsync("https://localhost:7001");
            if (disco.IsError)
                throw new Exception(disco.Error);

            // NOTE: Получение access token по реквизитам клиента
            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "Photos App by OAuth",
                ClientSecret = "secret",
                Scope = "photos"
            });

            if (tokenResponse.IsError)
                throw new Exception(tokenResponse.Error);

            return tokenResponse.AccessToken;
        }
    }
}
