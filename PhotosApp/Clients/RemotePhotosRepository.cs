using AutoMapper;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
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
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> oidcConfigurationManager;

        public RemotePhotosRepository(IOptions<PhotosServiceOptions> options,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IConfigurationManager<OpenIdConnectConfiguration> oidcConfigurationManager)
        {
            serviceUrl = options.Value.ServiceUrl;
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
            this.oidcConfigurationManager = oidcConfigurationManager;
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
            var httpContext = httpContextAccessor.HttpContext;
            
            var accessToken = await httpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            if (accessToken == null)
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);

            if ((await ValidateTokenAsync(accessToken)).IsValid)
            {
                var httpClient = new HttpClient();
                request.SetBearerToken(accessToken);
                var response = await httpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.Unauthorized)
                    return response;
            }

            var refreshToken = await httpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);
            if (refreshToken == null)
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);

            // NOTE: запрос нового access token
            var newAccessToken = await RefreshAccessTokenAsync(refreshToken);
            if (newAccessToken != null)
            {
                // NOTE: повторный запрос
                var newHttpClient = new HttpClient();
                // NOTE: HttpRequestMessage нельзя использовать два раза, поэтому он копируется
                var secondRequest = await request.CopyAsync();
                secondRequest.SetBearerToken(newAccessToken);
                var secondResponse = await newHttpClient.SendAsync(secondRequest);
                return secondResponse;
            }

            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        private async Task<string> RefreshAccessTokenAsync(string refreshToken)
        {
            var httpContext = httpContextAccessor.HttpContext;

            // NOTE: получение конфигурации сервера авторизации
            // NOTE: если исходный запрос будет отменен, то использование RequestAborted отменит запрос конфигурации
            var oidcConfiguration = await oidcConfigurationManager.GetConfigurationAsync(httpContext.RequestAborted);

            // NOTE: запрос токенов с помощью IdentityModel
            var tokenResponse = await new HttpClient().RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = oidcConfiguration.TokenEndpoint,
                ClientId = "Photos App by OIDC",
                ClientSecret = "secret",
                RefreshToken = refreshToken,
            });

            // NOTE: обновление access token и refresh token в аутентификационной cookie
            // NOTE: Схему можно не указывать, потому что DefaultScheme подходит, DefaultAuthenticateScheme не задана
            var authResult = await httpContext.AuthenticateAsync();
            if (tokenResponse.RefreshToken != null)
                authResult.Properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, tokenResponse.RefreshToken);
            if (tokenResponse.AccessToken != null)
                authResult.Properties.UpdateTokenValue(OpenIdConnectParameterNames.AccessToken, tokenResponse.AccessToken);
            // NOTE: Схему можно не указывать, потому что DefaultSignInScheme подходит
            await httpContext.SignInAsync(authResult.Principal, authResult.Properties);

            return tokenResponse.AccessToken;
        }

        private async Task<TokenValidationResult> ValidateTokenAsync(string accessToken)
        {
            var httpContext = httpContextAccessor.HttpContext;
            var oidcConfiguration = await oidcConfigurationManager.GetConfigurationAsync(httpContext.RequestAborted);
            var issuerSigningKeys = oidcConfiguration.SigningKeys;

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                // NOTE: Переопределение проверки подписи токена, чтобы подпись не проверялась,
                // ведь ее не получится проверить без закрытого ключа
                SignatureValidator = (token, validationParameters) => new JsonWebToken(token)
            };
            // NOTE: если все же хочется проверить подпись, то переопределять не нужно
            validationParameters.SignatureValidator = null;
            // NOTE: для проверки подписи нужен открытый ключ сервера авторизации
            validationParameters.IssuerSigningKeys = issuerSigningKeys;
            // NOTE: токены совсем без подписи вообще-то надо всегда отбрасывать — они ничтожны
            validationParameters.RequireSignedTokens = true;

            var tokenHandler = new JsonWebTokenHandler();
            var validationResult = tokenHandler.ValidateToken(accessToken, validationParameters);
            return validationResult;
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
