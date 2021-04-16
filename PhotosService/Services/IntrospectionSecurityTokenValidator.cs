using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using PhotosService.Services.Exceptions;

namespace PhotosService.Services
{
    public class IntrospectionSecurityTokenValidator : ISecurityTokenValidator
    {
        private readonly JwtSecurityTokenHandler tokenHandler;
        private readonly string authorityAddress;
        private readonly string apiResourceId;
        private readonly string apiResourceSecret;

        public IntrospectionSecurityTokenValidator(string authorityAddress, string apiResourceId, string apiResourceSecret)
        {
            tokenHandler = new JwtSecurityTokenHandler();
            this.authorityAddress = authorityAddress;
            this.apiResourceId = apiResourceId;
            this.apiResourceSecret = apiResourceSecret;
        }

        public bool CanValidateToken => true;

        public int MaximumTokenSizeInBytes { get; set; } = TokenValidationParameters.DefaultMaximumTokenSizeInBytes;

        public bool CanReadToken(string securityToken) => tokenHandler.CanReadToken(securityToken);

        public ClaimsPrincipal ValidateToken(
            string securityToken,
            TokenValidationParameters validationParameters,
            out SecurityToken validatedToken)
        {
            // NOTE: стандартная проверка токена, чтобы не проверять на сервере авторизации заведомо некорректные токены
            var principal = tokenHandler.ValidateToken(securityToken, validationParameters, out validatedToken);
            
            // NOTE: проверка токена через сервер авторизации
            var (isActive, _) = IntrospectTokenAsync(securityToken).Result;
            if (!isActive)
                throw new TokenNotActiveException();
            
            return principal;
        }

        async Task<(bool isActive, Claim[] claims)> IntrospectTokenAsync(string securityToken)
        {
            var client = new HttpClient();

            // NOTE: запрашивается конфигурация сервера авторизации, внутри она кэшируется
            var disco = await client.GetDiscoveryDocumentAsync(authorityAddress);

            var response = await client.IntrospectTokenAsync(new TokenIntrospectionRequest
            {
                Address = disco.IntrospectionEndpoint,
                // NOTE: хоть поле называется clientId, но это идентификатор ресурса, а не клиента
                ClientId = apiResourceId,
                // NOTE: для защиты от несанкционированных запросов на проверку токенов используется секрет ресурса
                ClientSecret = apiResourceSecret,

                Token = securityToken
            });

            if (response.IsError)
                throw new TokenIntrospectionException(response.Error);
            
            
            return (
                isActive: response.IsActive, // NOTE: Не прошло ли время действия токена? Не отозван ли он?
                claims: response.Claims.ToArray() // NOTE: Сервер авторизации расшифровывает содержимое токена
            );
        }
    }
}
