using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CommunityIssueReporter.Models;
using CommunityIssueReporter.Utilities;

namespace CommunityIssueReporter.Data
{
    public static class UserRepository
    {
        // Current user info (for session management)
        private static User _currentUser = null;
        public static User CurrentUser => _currentUser;

        // In-memory user list for offline mode
        private static List<User> _users = new List<User>();

        // Login user
        public static bool LoginUser(string username, string password)
        {
            // Check for offline mode
            if (DatabaseManager.IsOfflineMode)
            {
                // Find user in offline user list
                User user = _users.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (user != null)
                {
                    // Verify password (assuming password is already hashed in offline mode for simplicity)
                    if (user.Password == password)
                    {
                        // Set as current user
                        _currentUser = user;
                        return true;
                    }
                }
                return false;
            }
            else
            {
                // Online mode - check against database
                try
                {
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        // Get user and salt
                        string query = @"
                            SELECT UserID, Username, PasswordHash, Salt, Email, FirstName, LastName, 
                                   PhoneNumber, Address, UserRole, CreatedDate, LastLoginDate 
                            FROM Users 
                            WHERE Username = @Username";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Username", username);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    // Get stored hash and salt
                                    string storedHash = reader["PasswordHash"].ToString();
                                    string salt = reader["Salt"].ToString();

                                    // Verify password
                                    if (SecurityHelper.VerifyPassword(password, storedHash, salt))
                                    {
                                        // Create user object
                                        _currentUser = new User
                                        {
                                            UserID = Convert.ToInt32(reader["UserID"]),
                                            Username = reader["Username"].ToString(),
                                            Email = reader["Email"].ToString(),
                                            FirstName = reader["FirstName"].ToString(),
                                            LastName = reader["LastName"].ToString(),
                                            PhoneNumber = reader["PhoneNumber"] != DBNull.Value ? reader["PhoneNumber"].ToString() : string.Empty,
                                            Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : string.Empty,
                                            UserRole = reader["UserRole"].ToString(),
                                            CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                            LastLoginDate = reader["LastLoginDate"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginDate"]) : (DateTime?)null
                                        };

                                        // Update last login date
                                        UpdateLastLoginDate(_currentUser.UserID);

                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Login error: {ex.Message}");
                    throw;
                }
            }
        }

        // Logout current user
        public static void LogoutUser()
        {
            _currentUser = null;
        }

        // Update last login date
        private static void UpdateLastLoginDate(int userID)
        {
            if (DatabaseManager.IsOfflineMode)
            {
                // In offline mode, just update the in-memory user
                var user = _users.FirstOrDefault(u => u.UserID == userID);
                if (user != null)
                {
                    user.LastLoginDate = DateTime.Now;
                }
            }
            else
            {
                // In online mode, update the database
                try
                {
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string query = "UPDATE Users SET LastLoginDate = GETDATE() WHERE UserID = @UserID";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", userID);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Update login date error: {ex.Message}");
                    // Continue anyway - not critical
                }
            }
        }

        // Initialize sample users for offline mode
        public static void InitializeOfflineUsers()
        {
            if (_users.Count == 0)
            {
                // Add admin user
                _users.Add(new User
                {
                    UserID = 1,
                    Username = "admin",
                    Password = "Admin@123", // In offline mode, we store plain password for simplicity
                    Email = "admin@communityapp.org",
                    FirstName = "System",
                    LastName = "Administrator",
                    UserRole = "Admin",
                    CreatedDate = DateTime.Now.AddDays(-30)
                });

                // Add staff user
                _users.Add(new User
                {
                    UserID = 2,
                    Username = "staff",
                    Password = "Staff@123",
                    Email = "staff@communityapp.org",
                    FirstName = "Staff",
                    LastName = "Member",
                    UserRole = "Staff",
                    CreatedDate = DateTime.Now.AddDays(-25)
                });

                // Add regular user
                _users.Add(new User
                {
                    UserID = 3,
                    Username = "user",
                    Password = "User@123",
                    Email = "user@example.com",
                    FirstName = "Regular",
                    LastName = "User",
                    PhoneNumber = "555-1234",
                    Address = "123 Main St",
                    UserRole = "User",
                    CreatedDate = DateTime.Now.AddDays(-10)
                });
            }
        }

        // Get user by ID
        public static User GetUserByID(int userID)
        {
            if (DatabaseManager.IsOfflineMode)
            {
                return _users.FirstOrDefault(u => u.UserID == userID);
            }
            else
            {
                try
                {
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string query = @"
                            SELECT UserID, Username, Email, FirstName, LastName, 
                                  PhoneNumber, Address, UserRole, CreatedDate, LastLoginDate 
                            FROM Users 
                            WHERE UserID = @UserID";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", userID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    return new User
                                    {
                                        UserID = Convert.ToInt32(reader["UserID"]),
                                        Username = reader["Username"].ToString(),
                                        Email = reader["Email"].ToString(),
                                        FirstName = reader["FirstName"].ToString(),
                                        LastName = reader["LastName"].ToString(),
                                        PhoneNumber = reader["PhoneNumber"] != DBNull.Value ? reader["PhoneNumber"].ToString() : string.Empty,
                                        Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : string.Empty,
                                        UserRole = reader["UserRole"].ToString(),
                                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                        LastLoginDate = reader["LastLoginDate"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginDate"]) : (DateTime?)null
                                    };
                                }
                            }
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Get user error: {ex.Message}");
                    return null;
                }
            }
        }

        // Register a new user
        public static bool RegisterUser(string username, string password, string email, string firstName, string lastName, string phoneNumber = null, string address = null)
        {
            if (DatabaseManager.IsOfflineMode)
            {
                // Check if username or email already exists
                if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) ||
                                   u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                // Create new user
                User newUser = new User
                {
                    UserID = _users.Count > 0 ? _users.Max(u => u.UserID) + 1 : 1,
                    Username = username,
                    Password = password,  // In offline mode, we store plain password for simplicity
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phoneNumber,
                    Address = address,
                    UserRole = "User",  // Default role
                    CreatedDate = DateTime.Now
                };

                _users.Add(newUser);

                // Auto-login
                _currentUser = newUser;

                return true;
            }
            else
            {
                try
                {
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        // Check if username or email already exists
                        string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email";

                        using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@Username", username);
                            checkCommand.Parameters.AddWithValue("@Email", email);

                            int existingCount = (int)checkCommand.ExecuteScalar();
                            if (existingCount > 0)
                            {
                                return false;
                            }
                        }

                        // Generate salt and hash password
                        string salt = SecurityHelper.GenerateSalt();
                        string passwordHash = SecurityHelper.HashPassword(password, salt);

                        // Insert new user
                        string insertQuery = @"
                            INSERT INTO Users (Username, PasswordHash, Salt, Email, FirstName, LastName, PhoneNumber, Address, UserRole, CreatedDate)
                            VALUES (@Username, @PasswordHash, @Salt, @Email, @FirstName, @LastName, @PhoneNumber, @Address, @UserRole, GETDATE());
                            
                            SELECT SCOPE_IDENTITY();";

                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@Username", username);
                            insertCommand.Parameters.AddWithValue("@PasswordHash", passwordHash);
                            insertCommand.Parameters.AddWithValue("@Salt", salt);
                            insertCommand.Parameters.AddWithValue("@Email", email);
                            insertCommand.Parameters.AddWithValue("@FirstName", firstName);
                            insertCommand.Parameters.AddWithValue("@LastName", lastName);
                            insertCommand.Parameters.AddWithValue("@PhoneNumber", phoneNumber ?? (object)DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@Address", address ?? (object)DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@UserRole", "User"); // Default role

                            // Get new user ID
                            int newUserID = Convert.ToInt32(insertCommand.ExecuteScalar());

                            // Auto-login
                            _currentUser = new User
                            {
                                UserID = newUserID,
                                Username = username,
                                Email = email,
                                FirstName = firstName,
                                LastName = lastName,
                                PhoneNumber = phoneNumber,
                                Address = address,
                                UserRole = "User",
                                CreatedDate = DateTime.Now
                            };

                            // Update last login date
                            UpdateLastLoginDate(newUserID);

                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Registration error: {ex.Message}");
                    throw;
                }
            }
        }
    }
}