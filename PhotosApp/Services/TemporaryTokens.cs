using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PhotosApp.Services
{
    public static class TemporaryTokens
    {
        public static SymmetricSecurityKey SigningKey =>
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes("Ne!0_0!vzlomayesh!^_^!nikogda!"));

        public const string CookieName = "TemporaryToken";

        public static string GenerateEncoded()
        {
            var claims = new Claim[]
            {
                new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new(ClaimTypes.Name, "dasdas"),
                new(ClaimTypes.Role, "Dev")
            };
            var activeTime = TimeSpan.FromSeconds(30);
            var utcCurrentTime = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                claims: claims,
                notBefore: utcCurrentTime,
                expires: utcCurrentTime+activeTime,
                signingCredentials: new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }
    }
}