using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PhotosApp.Services
{
    public static class TemporaryTokens
    {
        public static SymmetricSecurityKey SigningKey => new(Encoding.ASCII.GetBytes("Ne!0_0!vzlomayesh!^_^!nikogda!"));

        public const string CookieName = "TemporaryToken";

        public static string GenerateEncoded()
        {
            var claims = new Claim[]
            {
                new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new (ClaimsIdentity.DefaultNameClaimType, "default"),
                new (ClaimsIdentity.DefaultRoleClaimType, "Dev")
            };

            var jwt = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.Now.ToUniversalTime(),
                expires: DateTime.Now.ToUniversalTime() + TimeSpan.FromSeconds(30),
                signingCredentials: new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }
    }
}