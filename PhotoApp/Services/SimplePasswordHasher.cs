using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;

namespace PhotoApp.Services
{
    public class SimplePasswordHasher<TUser> : IPasswordHasher<TUser>
        where TUser : IdentityUser
    {
        private const int SaltSize = 128;
        private const int SubkeySize = 256;

        public string HashPassword(TUser user, string password)
        {
            byte[] saltBytes = new byte[SaltSize / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            byte[] subkeyBytes = GetSubkeyBytes(password, saltBytes);

            var hashedPasswordBytes = new byte[saltBytes.Length + subkeyBytes.Length];
            Buffer.BlockCopy(saltBytes, 0,
                hashedPasswordBytes, 0, saltBytes.Length);
            Buffer.BlockCopy(subkeyBytes, 0,
                hashedPasswordBytes, saltBytes.Length, subkeyBytes.Length);
            return Convert.ToBase64String(hashedPasswordBytes);
        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user,
            string hashedPassword, string providedPassword)
        {
            byte[] expectedSubkeyBytes = null;
            byte[] actualSubkeyBytes = null;

            throw new NotImplementedException();

            return AreByteArraysEqual(actualSubkeyBytes, expectedSubkeyBytes)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }

        private static byte[] GetSubkeyBytes(string password, byte[] saltBytes)
        {
            return KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: SubkeySize / 8);
        }

        private static bool AreByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null || a.Length != b.Length)
                return false;

            var areSame = true;
            for (var i = 0; i < a.Length; i++)
                areSame &= (a[i] == b[i]);
            return areSame;
        }
    }
}