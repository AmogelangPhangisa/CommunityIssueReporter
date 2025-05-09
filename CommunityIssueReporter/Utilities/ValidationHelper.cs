using System;
using System.Text.RegularExpressions;

namespace CommunityIssueReporter.Utilities
{
    public static class ValidationHelper
    {
        // Validate email format
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Regular expression for basic email validation
                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }

        // Validate password strength
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Check length
            if (password.Length < 8)
                return false;

            // Check for at least one letter
            if (!Regex.IsMatch(password, @"[a-zA-Z]"))
                return false;

            // Check for at least one digit
            if (!Regex.IsMatch(password, @"\d"))
                return false;

            // Check for at least one special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_\-+=\[\]{};:'"",.<>/?\\|]"))
                return false;

            return true;
        }

        // Validate phone number
        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // Phone is optional

            // Allow digits, spaces, dashes, parentheses, and plus sign
            return Regex.IsMatch(phone, @"^[\d\s\-\(\)\+]+$");
        }

        // Validate username
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            // Username must be at least 4 characters and contain only letters, numbers, and underscores
            return Regex.IsMatch(username, @"^[a-zA-Z0-9_]{4,}$");
        }

        // Validate name (first name, last name)
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Name should contain only letters, spaces, hyphens, and apostrophes
            return Regex.IsMatch(name, @"^[a-zA-Z\s\-']+$");
        }

        // Validate issue description (minimum length)
        public static bool IsValidDescription(string description, int minLength = 10)
        {
            if (string.IsNullOrWhiteSpace(description))
                return false;

            return description.Trim().Length >= minLength;
        }

        // Validate location (minimum length)
        public static bool IsValidLocation(string location, int minLength = 3)
        {
            if (string.IsNullOrWhiteSpace(location))
                return false;

            return location.Trim().Length >= minLength;
        }

        // Validate file extension
        public static bool IsValidFileExtension(string filePath, params string[] allowedExtensions)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            string extension = System.IO.Path.GetExtension(filePath).ToLower();

            foreach (string allowedExtension in allowedExtensions)
            {
                if (extension == allowedExtension.ToLower() || extension == "." + allowedExtension.ToLower())
                    return true;
            }

            return false;
        }

        // Validate image file
        public static bool IsValidImageFile(string filePath)
        {
            return IsValidFileExtension(filePath, "jpg", "jpeg", "png", "gif", "bmp");
        }

        // Validate document file
        public static bool IsValidDocumentFile(string filePath)
        {
            return IsValidFileExtension(filePath, "pdf", "doc", "docx", "txt", "rtf");
        }
    }
}