using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CommunityIssueReporter.Models;

namespace CommunityIssueReporter.Data
{
    public static class ServiceRequestRepository
    {
        // In-memory storage for offline mode
        private static List<ServiceRequest> _requests = new List<ServiceRequest>();

        // Get all service requests
        public static List<ServiceRequest> GetAllServiceRequests()
        {
            List<ServiceRequest> requests = new List<ServiceRequest>();

            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Return in-memory requests
                    return new List<ServiceRequest>(_requests);
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT r.RequestID, r.UserID, r.ServiceType, r.Description,
                               r.SubmissionDate, r.Status, r.CompletionDate,
                               CONCAT(u.FirstName, ' ', u.LastName) AS UserName
                        FROM ServiceRequests r
                        JOIN Users u ON r.UserID = u.UserID
                        ORDER BY r.SubmissionDate DESC";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    requests.Add(new ServiceRequest
                                    {
                                        RequestID = Convert.ToInt32(reader["RequestID"]),
                                        UserID = Convert.ToInt32(reader["UserID"]),
                                        ServiceType = reader["ServiceType"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        SubmissionDate = Convert.ToDateTime(reader["SubmissionDate"]),
                                        Status = reader["Status"].ToString(),
                                        CompletionDate = reader["CompletionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CompletionDate"]),
                                        UserName = reader["UserName"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all service requests: {ex.Message}");
            }

            return requests;
        }

        // Get service requests for a user
        public static List<ServiceRequest> GetServiceRequestsByUserID(int userID)
        {
            List<ServiceRequest> requests = new List<ServiceRequest>();

            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Filter requests by user ID
                    return _requests.Where(r => r.UserID == userID).OrderByDescending(r => r.SubmissionDate).ToList();
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT r.RequestID, r.UserID, r.ServiceType, r.Description,
                               r.SubmissionDate, r.Status, r.CompletionDate,
                               CONCAT(u.FirstName, ' ', u.LastName) AS UserName
                        FROM ServiceRequests r
                        JOIN Users u ON r.UserID = u.UserID
                        WHERE r.UserID = @UserID
                        ORDER BY r.SubmissionDate DESC";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", userID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    requests.Add(new ServiceRequest
                                    {
                                        RequestID = Convert.ToInt32(reader["RequestID"]),
                                        UserID = Convert.ToInt32(reader["UserID"]),
                                        ServiceType = reader["ServiceType"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        SubmissionDate = Convert.ToDateTime(reader["SubmissionDate"]),
                                        Status = reader["Status"].ToString(),
                                        CompletionDate = reader["CompletionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CompletionDate"]),
                                        UserName = reader["UserName"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting service requests by user ID: {ex.Message}");
            }

            return requests;
        }

        // Get service request by ID
        public static ServiceRequest GetServiceRequestByID(int requestID)
        {
            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Return request from in-memory list
                    return _requests.FirstOrDefault(r => r.RequestID == requestID);
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT r.RequestID, r.UserID, r.ServiceType, r.Description,
                               r.SubmissionDate, r.Status, r.CompletionDate,
                               CONCAT(u.FirstName, ' ', u.LastName) AS UserName
                        FROM ServiceRequests r
                        JOIN Users u ON r.UserID = u.UserID
                        WHERE r.RequestID = @RequestID";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@RequestID", requestID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    return new ServiceRequest
                                    {
                                        RequestID = Convert.ToInt32(reader["RequestID"]),
                                        UserID = Convert.ToInt32(reader["UserID"]),
                                        ServiceType = reader["ServiceType"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        SubmissionDate = Convert.ToDateTime(reader["SubmissionDate"]),
                                        Status = reader["Status"].ToString(),
                                        CompletionDate = reader["CompletionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CompletionDate"]),
                                        UserName = reader["UserName"].ToString()
                                    };
                                }
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting service request by ID: {ex.Message}");
                return null;
            }
        }

        // Add a new service request
        public static int AddServiceRequest(ServiceRequest request)
        {
            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Add request to in-memory list
                    request.RequestID = _requests.Count > 0 ? _requests.Max(r => r.RequestID) + 1 : 1;

                    // Set user name
                    var user = UserRepository.GetUserByID(request.UserID);
                    if (user != null)
                    {
                        request.UserName = user.FullName;
                    }

                    _requests.Add(request);
                    return request.RequestID;
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string insertQuery = @"
                        INSERT INTO ServiceRequests (UserID, ServiceType, Description, SubmissionDate, Status)
                        VALUES (@UserID, @ServiceType, @Description, @SubmissionDate, @Status);
                        
                        SELECT SCOPE_IDENTITY();";

                        using (SqlCommand command = new SqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", request.UserID);
                            command.Parameters.AddWithValue("@ServiceType", request.ServiceType);
                            command.Parameters.AddWithValue("@Description", request.Description);
                            command.Parameters.AddWithValue("@SubmissionDate", request.SubmissionDate);
                            command.Parameters.AddWithValue("@Status", request.Status);

                            // Get the new request ID
                            return Convert.ToInt32(command.ExecuteScalar());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding service request: {ex.Message}");
                return -1;
            }
        }

        // Update service request status
        public static bool UpdateServiceRequestStatus(int requestID, string newStatus)
        {
            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Update status in in-memory list
                    int index = _requests.FindIndex(r => r.RequestID == requestID);
                    if (index >= 0)
                    {
                        _requests[index].Status = newStatus;

                        // Update completion date if status is "Completed"
                        if (newStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                        {
                            _requests[index].CompletionDate = DateTime.Now;
                        }
                        else
                        {
                            _requests[index].CompletionDate = null;
                        }

                        return true;
                    }

                    return false;
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string updateQuery = @"
                        UPDATE ServiceRequests 
                        SET Status = @Status, 
                            CompletionDate = CASE WHEN @Status = 'Completed' THEN GETDATE() ELSE NULL END
                        WHERE RequestID = @RequestID";

                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Status", newStatus);
                            command.Parameters.AddWithValue("@RequestID", requestID);

                            int rowsAffected = command.ExecuteNonQuery();
                            return rowsAffected > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating service request status: {ex.Message}");
                return false;
            }
        }

        // Initialize sample service requests for offline mode
        public static void InitializeOfflineServiceRequests()
        {
            if (_requests.Count == 0)
            {
                // Make sure we have sample users
                UserRepository.InitializeOfflineUsers();

                // Add some sample service requests
                _requests.Add(new ServiceRequest
                {
                    RequestID = 1,
                    UserID = 3, // Regular user
                    ServiceType = "Trash Collection",
                    Description = "Requesting additional trash pickup for large items (furniture).",
                    SubmissionDate = DateTime.Now.AddDays(-10),
                    Status = "Completed",
                    CompletionDate = DateTime.Now.AddDays(-5),
                    UserName = "Regular User"
                });

                _requests.Add(new ServiceRequest
                {
                    RequestID = 2,
                    UserID = 3, // Regular user
                    ServiceType = "Street Light Repair",
                    Description = "Street light out on Oak Street near house #42.",
                    SubmissionDate = DateTime.Now.AddDays(-7),
                    Status = "In Process",
                    UserName = "Regular User"
                });

                _requests.Add(new ServiceRequest
                {
                    RequestID = 3,
                    UserID = 2, // Staff member
                    ServiceType = "Tree Trimming",
                    Description = "Tree branches hanging over power lines on Maple Avenue.",
                    SubmissionDate = DateTime.Now.AddDays(-4),
                    Status = "Pending",
                    UserName = "Staff Member"
                });
            }
        }
    }
}