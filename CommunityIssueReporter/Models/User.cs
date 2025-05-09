using System;

namespace CommunityIssueReporter.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }

        // Only used for offline mode
        public string Password { get; set; }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string UserRole { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        // Convenience properties
        public string FullName => $"{FirstName} {LastName}";
        public bool IsAdmin => UserRole == "Admin";
        public bool IsStaff => UserRole == "Staff" || UserRole == "Admin";
    }
}