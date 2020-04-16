using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;

namespace PhotosApp.Services
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

    [TestFixture]
    public class SimplePasswordHasherSpecification
    {
        private SimplePasswordHasher<IdentityUser> hasher;
        private readonly IdentityUser emptyUser = new IdentityUser();
        private readonly string correctPassword = "correct";
        private readonly string incorrectPassword = "incorrect";

        [SetUp]
        public void SetUp()
        {
            hasher = new SimplePasswordHasher<IdentityUser>();
        }

        [Test]
        public void HashPassword_ShouldGenerateHash()
        {
            Assert.IsNotEmpty(hasher.HashPassword(emptyUser, correctPassword));
        }

        [Test]
        public void HashPassword_ShouldGenerateDifferentHashes()
        {
            var hash1 = hasher.HashPassword(emptyUser, correctPassword);
            var hash2 = hasher.HashPassword(emptyUser, incorrectPassword);
            Assert.IsFalse(hash1.SequenceEqual(hash2));
        }

        [Test]
        public void HashPassword_ShouldGenerateHashesWithSalt()
        {
            var hash1 = hasher.HashPassword(emptyUser, correctPassword);
            var hash2 = hasher.HashPassword(emptyUser, correctPassword);
            Assert.IsFalse(hash1.SequenceEqual(hash2));
        }

        [Test]
        public void VerifyHashedPassword_ShouldReturnSuccess_WhenCorrectPassword()
        {
            var hash = hasher.HashPassword(emptyUser, correctPassword);
            var verificationResult = hasher.VerifyHashedPassword(emptyUser, hash, correctPassword);
            Assert.AreEqual(PasswordVerificationResult.Success, verificationResult);
        }

        [Test]
        public void VerifyHashedPassword_ShouldReturnFailed_WhenIncorrectPassword()
        {
            var hash = hasher.HashPassword(emptyUser, correctPassword);
            var verificationResult = hasher.VerifyHashedPassword(emptyUser, hash, incorrectPassword);
            Assert.AreEqual(PasswordVerificationResult.Failed, verificationResult);
        }
    }
}