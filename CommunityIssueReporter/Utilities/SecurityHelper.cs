using System;
using System.Security.Cryptography;
using System.Text;

namespace CommunityIssueReporter.Utilities
{
    public static class SecurityHelper
    {
        // Generate a random salt
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        // Hash a password with a salt
        public static string HashPassword(string password, string salt)
        {
            // Combine password and salt
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);

            // Compute hash
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        // Verify a password against a stored hash
        public static bool VerifyPassword(string password, string storedHash, string salt)
        {
            string computedHash = HashPassword(password, salt);
            return computedHash == storedHash;
        }

        // Validate email format
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Validate password strength
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            // Check for at least one uppercase letter
            bool hasUppercase = false;

            // Check for at least one lowercase letter
            bool hasLowercase = false;

            // Check for at least one digit
            bool hasDigit = false;

            // Check for at least one special character
            bool hasSpecial = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c))
                    hasUppercase = true;
                else if (char.IsLower(c))
                    hasLowercase = true;
                else if (char.IsDigit(c))
                    hasDigit = true;
                else if (!char.IsLetterOrDigit(c))
                    hasSpecial = true;
            }

            return hasUppercase && hasLowercase && hasDigit && hasSpecial;
        }
    }
}