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
        private const int SaltSizeInBits = 128;
        private const int HashSizeInBits = 256;

        public string HashPassword(TUser user, string password)
        {
            byte[] saltBytes = GenerateSaltBytes();
            byte[] hashBytes = GetHashBytes(password, saltBytes);
            byte[] hashedPasswordBytes = ConcatenateBytes(saltBytes, hashBytes);
            string hashedPassword = Convert.ToBase64String(hashedPasswordBytes);
            return hashedPassword;
        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user,
            string hashedPassword, string providedPassword)
        {
            byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

            byte[] saltBytes = new byte[SaltSizeInBits / 8];
            Buffer.BlockCopy(
                hashedPasswordBytes, 0,
                saltBytes, 0,
                saltBytes.Length);

            byte[] expectedHashBytes = new byte[HashSizeInBits / 8];
            Buffer.BlockCopy(
                hashedPasswordBytes, saltBytes.Length,
                expectedHashBytes, 0,
                expectedHashBytes.Length);

            byte[] actualHashBytes = GetHashBytes(providedPassword, saltBytes);

            // Если providedPassword корректен, то в результате хэширования его с той же самой солью,
            // что и оригинальный пароль, должен получаться тот же самый хэш.
            return AreByteArraysEqual(actualHashBytes, expectedHashBytes)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }

        private byte[] GenerateSaltBytes()
        {
            byte[] saltBytes = new byte[SaltSizeInBits / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return saltBytes;
        }

        private static byte[] GetHashBytes(string password, byte[] saltBytes)
        {
            return KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: HashSizeInBits / 8);
        }

        private static byte[] ConcatenateBytes(byte[] leftBytes, byte[] rightBytes)
        {
            var resultBytes = new byte[leftBytes.Length + rightBytes.Length];
            
            Buffer.BlockCopy(
                leftBytes, 0, // байты источника и позиция в них
                resultBytes, 0, // байты назначения и начальная позиция в них
                leftBytes.Length); // количество байтов, которое надо скопировать

            Buffer.BlockCopy(
                rightBytes, 0, // байты источника и позиция в них
                resultBytes, leftBytes.Length, // байты назначения и начальная позиция в них
                rightBytes.Length); // количество байтов, которое надо скопировать
            
            return resultBytes;
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