using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CommunityIssueReporter.Models;

namespace CommunityIssueReporter.Data
{
    public static class IssueRepository
    {
        // In-memory storage for offline mode
        private static List<Issue> _issues = new List<Issue>();
        private static List<IssueStatusHistory> _issueHistory = new List<IssueStatusHistory>();
        private static List<IssueComment> _issueComments = new List<IssueComment>();

        // Get all issues
        public static List<Issue> GetAllIssues()
        {
            List<Issue> issues = new List<Issue>();

            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Return in-memory issues
                    return new List<Issue>(_issues);
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT i.IssueID, i.UserID, i.Location, i.Category, i.Description, 
                               i.AttachmentPath, i.ReportedTime, i.Status, i.Priority, 
                               i.AssignedTo, i.ResolutionDetails, i.ResolutionDate,
                               CONCAT(u1.FirstName, ' ', u1.LastName) AS ReporterName,
                               CONCAT(u2.FirstName, ' ', u2.LastName) AS AssignedToName
                        FROM Issues i
                        LEFT JOIN Users u1 ON i.UserID = u1.UserID
                        LEFT JOIN Users u2 ON i.AssignedTo = u2.UserID
                        ORDER BY i.ReportedTime DESC";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    issues.Add(new Issue
                                    {
                                        IssueID = Convert.ToInt32(reader["IssueID"]),
                                        UserID = reader["UserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["UserID"]),
                                        Location = reader["Location"].ToString(),
                                        Category = reader["Category"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        AttachmentPath = reader["AttachmentPath"] == DBNull.Value ? null : reader["AttachmentPath"].ToString(),
                                        ReportedTime = Convert.ToDateTime(reader["ReportedTime"]),
                                        Status = reader["Status"].ToString(),
                                        Priority = reader["Priority"].ToString(),
                                        AssignedTo = reader["AssignedTo"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["AssignedTo"]),
                                        ResolutionDetails = reader["ResolutionDetails"] == DBNull.Value ? null : reader["ResolutionDetails"].ToString(),
                                        ResolutionDate = reader["ResolutionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ResolutionDate"]),
                                        ReporterName = reader["ReporterName"] == DBNull.Value ? null : reader["ReporterName"].ToString(),
                                        AssignedToName = reader["AssignedToName"] == DBNull.Value ? null : reader["AssignedToName"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all issues: {ex.Message}");
            }

            return issues;
        }

        // Get issues by status
        public static List<Issue> GetIssuesByStatus(string status)
        {
            List<Issue> issues = new List<Issue>();

            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Filter in-memory issues by status
                    return _issues.Where(i => i.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT i.IssueID, i.UserID, i.Location, i.Category, i.Description, 
                               i.AttachmentPath, i.ReportedTime, i.Status, i.Priority, 
                               i.AssignedTo, i.ResolutionDetails, i.ResolutionDate,
                               CONCAT(u1.FirstName, ' ', u1.LastName) AS ReporterName,
                               CONCAT(u2.FirstName, ' ', u2.LastName) AS AssignedToName
                        FROM Issues i
                        LEFT JOIN Users u1 ON i.UserID = u1.UserID
                        LEFT JOIN Users u2 ON i.AssignedTo = u2.UserID
                        WHERE i.Status = @Status
                        ORDER BY i.ReportedTime DESC";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Status", status);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    issues.Add(new Issue
                                    {
                                        IssueID = Convert.ToInt32(reader["IssueID"]),
                                        UserID = reader["UserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["UserID"]),
                                        Location = reader["Location"].ToString(),
                                        Category = reader["Category"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        AttachmentPath = reader["AttachmentPath"] == DBNull.Value ? null : reader["AttachmentPath"].ToString(),
                                        ReportedTime = Convert.ToDateTime(reader["ReportedTime"]),
                                        Status = reader["Status"].ToString(),
                                        Priority = reader["Priority"].ToString(),
                                        AssignedTo = reader["AssignedTo"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["AssignedTo"]),
                                        ResolutionDetails = reader["ResolutionDetails"] == DBNull.Value ? null : reader["ResolutionDetails"].ToString(),
                                        ResolutionDate = reader["ResolutionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ResolutionDate"]),
                                        ReporterName = reader["ReporterName"] == DBNull.Value ? null : reader["ReporterName"].ToString(),
                                        AssignedToName = reader["AssignedToName"] == DBNull.Value ? null : reader["AssignedToName"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting issues by status: {ex.Message}");
            }

            return issues;
        }

        // Get issues by user ID
        public static List<Issue> GetIssuesByUserID(int userID)
        {
            List<Issue> issues = new List<Issue>();

            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Filter in-memory issues by user ID
                    return _issues.Where(i => i.UserID == userID).ToList();
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT i.IssueID, i.UserID, i.Location, i.Category, i.Description, 
                               i.AttachmentPath, i.ReportedTime, i.Status, i.Priority, 
                               i.AssignedTo, i.ResolutionDetails, i.ResolutionDate,
                               CONCAT(u1.FirstName, ' ', u1.LastName) AS ReporterName,
                               CONCAT(u2.FirstName, ' ', u2.LastName) AS AssignedToName
                        FROM Issues i
                        LEFT JOIN Users u1 ON i.UserID = u1.UserID
                        LEFT JOIN Users u2 ON i.AssignedTo = u2.UserID
                        WHERE i.UserID = @UserID
                        ORDER BY i.ReportedTime DESC";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", userID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    issues.Add(new Issue
                                    {
                                        IssueID = Convert.ToInt32(reader["IssueID"]),
                                        UserID = reader["UserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["UserID"]),
                                        Location = reader["Location"].ToString(),
                                        Category = reader["Category"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        AttachmentPath = reader["AttachmentPath"] == DBNull.Value ? null : reader["AttachmentPath"].ToString(),
                                        ReportedTime = Convert.ToDateTime(reader["ReportedTime"]),
                                        Status = reader["Status"].ToString(),
                                        Priority = reader["Priority"].ToString(),
                                        AssignedTo = reader["AssignedTo"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["AssignedTo"]),
                                        ResolutionDetails = reader["ResolutionDetails"] == DBNull.Value ? null : reader["ResolutionDetails"].ToString(),
                                        ResolutionDate = reader["ResolutionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ResolutionDate"]),
                                        ReporterName = reader["ReporterName"] == DBNull.Value ? null : reader["ReporterName"].ToString(),
                                        AssignedToName = reader["AssignedToName"] == DBNull.Value ? null : reader["AssignedToName"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting issues by user ID: {ex.Message}");
            }

            return issues;
        }

        // Get issue by ID
        public static Issue GetIssueByID(int issueID)
        {
            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Return issue from in-memory list
                    return _issues.FirstOrDefault(i => i.IssueID == issueID);
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT i.IssueID, i.UserID, i.Location, i.Category, i.Description, 
                               i.AttachmentPath, i.ReportedTime, i.Status, i.Priority, 
                               i.AssignedTo, i.ResolutionDetails, i.ResolutionDate,
                               CONCAT(u1.FirstName, ' ', u1.LastName) AS ReporterName,
                               CONCAT(u2.FirstName, ' ', u2.LastName) AS AssignedToName
                        FROM Issues i
                        LEFT JOIN Users u1 ON i.UserID = u1.UserID
                        LEFT JOIN Users u2 ON i.AssignedTo = u2.UserID
                        WHERE i.IssueID = @IssueID";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@IssueID", issueID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    return new Issue
                                    {
                                        IssueID = Convert.ToInt32(reader["IssueID"]),
                                        UserID = reader["UserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["UserID"]),
                                        Location = reader["Location"].ToString(),
                                        Category = reader["Category"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        AttachmentPath = reader["AttachmentPath"] == DBNull.Value ? null : reader["AttachmentPath"].ToString(),
                                        ReportedTime = Convert.ToDateTime(reader["ReportedTime"]),
                                        Status = reader["Status"].ToString(),
                                        Priority = reader["Priority"].ToString(),
                                        AssignedTo = reader["AssignedTo"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["AssignedTo"]),
                                        ResolutionDetails = reader["ResolutionDetails"] == DBNull.Value ? null : reader["ResolutionDetails"].ToString(),
                                        ResolutionDate = reader["ResolutionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ResolutionDate"]),
                                        ReporterName = reader["ReporterName"] == DBNull.Value ? null : reader["ReporterName"].ToString(),
                                        AssignedToName = reader["AssignedToName"] == DBNull.Value ? null : reader["AssignedToName"].ToString()
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
                Console.WriteLine($"Error getting issue by ID: {ex.Message}");
                return null;
            }
        }

        // Add a new issue
        public static int AddIssue(Issue issue)
        {
            try
            {
                // Process attachment if there is one
                string finalAttachmentPath = ProcessAttachment(issue.AttachmentPath);
                issue.AttachmentPath = finalAttachmentPath;

                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Add issue to in-memory list
                    issue.IssueID = _issues.Count > 0 ? _issues.Max(i => i.IssueID) + 1 : 1;

                    // Set reporter name
                    if (issue.UserID.HasValue)
                    {
                        var user = UserRepository.GetUserByID(issue.UserID.Value);
                        if (user != null)
                        {
                            issue.ReporterName = user.FullName;
                        }
                    }

                    _issues.Add(issue);
                    return issue.IssueID;
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string insertQuery = @"
                        INSERT INTO Issues (UserID, Location, Category, Description, AttachmentPath, 
                                           ReportedTime, Status, Priority)
                        VALUES (@UserID, @Location, @Category, @Description, @AttachmentPath, 
                                @ReportedTime, @Status, @Priority);
                        
                        SELECT SCOPE_IDENTITY();";

                        using (SqlCommand command = new SqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", issue.UserID ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@Location", issue.Location);
                            command.Parameters.AddWithValue("@Category", issue.Category);
                            command.Parameters.AddWithValue("@Description", issue.Description);
                            command.Parameters.AddWithValue("@AttachmentPath", string.IsNullOrEmpty(issue.AttachmentPath) ? (object)DBNull.Value : issue.AttachmentPath);
                            command.Parameters.AddWithValue("@ReportedTime", issue.ReportedTime);
                            command.Parameters.AddWithValue("@Status", issue.Status);
                            command.Parameters.AddWithValue("@Priority", issue.Priority);

                            // Get the new issue ID
                            return Convert.ToInt32(command.ExecuteScalar());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding issue: {ex.Message}");
                return -1;
            }
        }

        // Update an issue
        public static bool UpdateIssue(Issue issue)
        {
            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Update issue in in-memory list
                    int index = _issues.FindIndex(i => i.IssueID == issue.IssueID);
                    if (index >= 0)
                    {
                        _issues[index] = issue;
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
                        UPDATE Issues 
                        SET Location = @Location, 
                            Category = @Category, 
                            Description = @Description, 
                            Status = @Status, 
                            Priority = @Priority, 
                            AssignedTo = @AssignedTo, 
                            ResolutionDetails = @ResolutionDetails, 
                            ResolutionDate = @ResolutionDate
                        WHERE IssueID = @IssueID";

                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Location", issue.Location);
                            command.Parameters.AddWithValue("@Category", issue.Category);
                            command.Parameters.AddWithValue("@Description", issue.Description);
                            command.Parameters.AddWithValue("@Status", issue.Status);
                            command.Parameters.AddWithValue("@Priority", issue.Priority);
                            command.Parameters.AddWithValue("@AssignedTo", issue.AssignedTo ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@ResolutionDetails", issue.ResolutionDetails ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@ResolutionDate", issue.ResolutionDate ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@IssueID", issue.IssueID);

                            int rowsAffected = command.ExecuteNonQuery();
                            return rowsAffected > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating issue: {ex.Message}");
                return false;
            }
        }

        // Update issue status
        public static bool UpdateIssueStatus(int issueID, string oldStatus, string newStatus, int changedByUserID, string comments)
        {
            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Update issue status in in-memory list
                    int index = _issues.FindIndex(i => i.IssueID == issueID);
                    if (index >= 0)
                    {
                        // Update status
                        _issues[index].Status = newStatus;

                        // Add to history
                        _issueHistory.Add(new IssueStatusHistory
                        {
                            HistoryID = _issueHistory.Count > 0 ? _issueHistory.Max(h => h.HistoryID) + 1 : 1,
                            IssueID = issueID,
                            StatusFrom = oldStatus,
                            StatusTo = newStatus,
                            ChangedByUserID = changedByUserID,
                            ChangeDate = DateTime.Now,
                            Comments = comments,
                            ChangedByName = UserRepository.GetUserByID(changedByUserID)?.FullName
                        });

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

                        // Start a transaction
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // Update issue status
                                string updateQuery = "UPDATE Issues SET Status = @Status WHERE IssueID = @IssueID";

                                using (SqlCommand command = new SqlCommand(updateQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@Status", newStatus);
                                    command.Parameters.AddWithValue("@IssueID", issueID);

                                    int rowsAffected = command.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        // Record the status change in history
                                        string historyQuery = @"
                                        INSERT INTO IssueStatusHistory (IssueID, StatusFrom, StatusTo, ChangedBy, Comments)
                                        VALUES (@IssueID, @StatusFrom, @StatusTo, @ChangedBy, @Comments)";

                                        using (SqlCommand historyCommand = new SqlCommand(historyQuery, connection, transaction))
                                        {
                                            historyCommand.Parameters.AddWithValue("@IssueID", issueID);
                                            historyCommand.Parameters.AddWithValue("@StatusFrom", oldStatus);
                                            historyCommand.Parameters.AddWithValue("@StatusTo", newStatus);
                                            historyCommand.Parameters.AddWithValue("@ChangedBy", changedByUserID);
                                            historyCommand.Parameters.AddWithValue("@Comments", string.IsNullOrEmpty(comments) ? (object)DBNull.Value : comments);

                                            historyCommand.ExecuteNonQuery();
                                        }

                                        // If status is "Resolved", update resolution date
                                        if (newStatus.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
                                        {
                                            string resolutionQuery = "UPDATE Issues SET ResolutionDate = GETDATE() WHERE IssueID = @IssueID";

                                            using (SqlCommand resolutionCommand = new SqlCommand(resolutionQuery, connection, transaction))
                                            {
                                                resolutionCommand.Parameters.AddWithValue("@IssueID", issueID);
                                                resolutionCommand.ExecuteNonQuery();
                                            }
                                        }

                                        // Commit the transaction
                                        transaction.Commit();
                                        return true;
                                    }
                                }

                                // If we get here, no rows were affected
                                transaction.Rollback();
                                return false;
                            }
                            catch (Exception ex)
                            {
                                // An error occurred, roll back the transaction
                                transaction.Rollback();
                                Console.WriteLine($"Transaction error: {ex.Message}");
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating issue status: {ex.Message}");
                return false;
            }
        }

        // Get issue status history
        public static List<IssueStatusHistory> GetIssueStatusHistory(int issueID)
        {
            List<IssueStatusHistory> history = new List<IssueStatusHistory>();

            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Return history from in-memory list
                    return _issueHistory.Where(h => h.IssueID == issueID).OrderByDescending(h => h.ChangeDate).ToList();
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT h.HistoryID, h.IssueID, h.StatusFrom, h.StatusTo, 
                               h.ChangedBy, h.ChangeDate, h.Comments,
                               CONCAT(u.FirstName, ' ', u.LastName) AS ChangedByName
                        FROM IssueStatusHistory h
                        JOIN Users u ON h.ChangedBy = u.UserID
                        WHERE h.IssueID = @IssueID
                        ORDER BY h.ChangeDate DESC";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@IssueID", issueID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    history.Add(new IssueStatusHistory
                                    {
                                        HistoryID = Convert.ToInt32(reader["HistoryID"]),
                                        IssueID = Convert.ToInt32(reader["IssueID"]),
                                        StatusFrom = reader["StatusFrom"].ToString(),
                                        StatusTo = reader["StatusTo"].ToString(),
                                        ChangedByUserID = Convert.ToInt32(reader["ChangedBy"]),
                                        ChangeDate = Convert.ToDateTime(reader["ChangeDate"]),
                                        Comments = reader["Comments"] == DBNull.Value ? null : reader["Comments"].ToString(),
                                        ChangedByName = reader["ChangedByName"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting issue status history: {ex.Message}");
            }

            return history;
        }

        // Add a comment to an issue
        public static int AddIssueComment(IssueComment comment)
        {
            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Add comment to in-memory list
                    comment.CommentID = _issueComments.Count > 0 ? _issueComments.Max(c => c.CommentID) + 1 : 1;

                    // Set user name
                    var user = UserRepository.GetUserByID(comment.UserID);
                    if (user != null)
                    {
                        comment.UserName = user.FullName;
                    }

                    _issueComments.Add(comment);
                    return comment.CommentID;
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string insertQuery = @"
                        INSERT INTO IssueComments (IssueID, UserID, CommentText, IsPublic)
                        VALUES (@IssueID, @UserID, @CommentText, @IsPublic);
                        
                        SELECT SCOPE_IDENTITY();";

                        using (SqlCommand command = new SqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@IssueID", comment.IssueID);
                            command.Parameters.AddWithValue("@UserID", comment.UserID);
                            command.Parameters.AddWithValue("@CommentText", comment.CommentText);
                            command.Parameters.AddWithValue("@IsPublic", comment.IsPublic);

                            // Get the new comment ID
                            return Convert.ToInt32(command.ExecuteScalar());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding issue comment: {ex.Message}");
                return -1;
            }
        }

        // Get comments for an issue
        public static List<IssueComment> GetIssueComments(int issueID, bool includePrivate = false)
        {
            List<IssueComment> comments = new List<IssueComment>();

            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Filter by issue ID and public/private status
                    var query = _issueComments.Where(c => c.IssueID == issueID);
                    if (!includePrivate)
                    {
                        query = query.Where(c => c.IsPublic);
                    }
                    return query.OrderByDescending(c => c.CommentDate).ToList();
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT c.CommentID, c.IssueID, c.UserID, c.CommentText, 
                               c.CommentDate, c.IsPublic,
                               CONCAT(u.FirstName, ' ', u.LastName) AS UserName
                        FROM IssueComments c
                        JOIN Users u ON c.UserID = u.UserID
                        WHERE c.IssueID = @IssueID";

                        if (!includePrivate)
                        {
                            selectQuery += " AND c.IsPublic = 1";
                        }

                        selectQuery += " ORDER BY c.CommentDate DESC";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@IssueID", issueID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    comments.Add(new IssueComment
                                    {
                                        CommentID = Convert.ToInt32(reader["CommentID"]),
                                        IssueID = Convert.ToInt32(reader["IssueID"]),
                                        UserID = Convert.ToInt32(reader["UserID"]),
                                        CommentText = reader["CommentText"].ToString(),
                                        CommentDate = Convert.ToDateTime(reader["CommentDate"]),
                                        IsPublic = Convert.ToBoolean(reader["IsPublic"]),
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
                Console.WriteLine($"Error getting issue comments: {ex.Message}");
            }

            return comments;
        }

        // Process attachment file
        private static string ProcessAttachment(string sourcePath)
        {
            if (string.IsNullOrEmpty(sourcePath))
                return null;

            try
            {
                // Get attachment storage path from config
                string attachmentStoragePath = ConfigurationManager.AppSettings["AttachmentStoragePath"];
                bool saveAttachmentsToDatabase = false;
                bool.TryParse(ConfigurationManager.AppSettings["SaveAttachmentsToDatabase"], out saveAttachmentsToDatabase);

                // If we're not saving attachments to disk, just return the original path
                if (saveAttachmentsToDatabase)
                {
                    return sourcePath;
                }

                // Create attachment directory if it doesn't exist
                if (!Directory.Exists(attachmentStoragePath))
                {
                    Directory.CreateDirectory(attachmentStoragePath);
                }

                // Create a unique filename to avoid collisions
                string fileName = Path.GetFileName(sourcePath);
                string uniqueFileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{fileName}";
                string destinationPath = Path.Combine(attachmentStoragePath, uniqueFileName);

                // Copy the file to the storage location
                File.Copy(sourcePath, destinationPath, true);

                return destinationPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing attachment: {ex.Message}");
                return null;
            }
        }

        // Initialize sample issues for offline mode
        public static void InitializeOfflineIssues()
        {
            if (_issues.Count == 0)
            {
                // Make sure we have sample users
                UserRepository.InitializeOfflineUsers();

                // Add some sample issues
                _issues.Add(new Issue
                {
                    IssueID = 1,
                    UserID = 3, // Regular user
                    Location = "Main Street and Park Avenue",
                    Category = "Roads",
                    Description = "Large pothole in the road causing traffic hazards.",
                    ReportedTime = DateTime.Now.AddDays(-7),
                    Status = "In Progress",
                    Priority = "High",
                    AssignedTo = 2, // Staff member
                    ReporterName = "Regular User",
                    AssignedToName = "Staff Member"
                });

                _issues.Add(new Issue
                {
                    IssueID = 2,
                    UserID = 3, // Regular user
                    Location = "Central Park Playground",
                    Category = "Public Safety",
                    Description = "Broken swing set with sharp edges. Risk to children.",
                    ReportedTime = DateTime.Now.AddDays(-5),
                    Status = "New",
                    Priority = "High",
                    ReporterName = "Regular User"
                });

                _issues.Add(new Issue
                {
                    IssueID = 3,
                    UserID = null, // Anonymous
                    Location = "Oak Street, between 5th and 6th Avenue",
                    Category = "Sanitation",
                    Description = "Overflowing trash bin not collected for over a week.",
                    ReportedTime = DateTime.Now.AddDays(-3),
                    Status = "New",
                    Priority = "Medium"
                });

                _issues.Add(new Issue
                {
                    IssueID = 4,
                    UserID = 3, // Regular user
                    Location = "West End Apartments",
                    Category = "Utilities",
                    Description = "Streetlight out for several days causing safety concerns at night.",
                    ReportedTime = DateTime.Now.AddDays(-10),
                    Status = "Resolved",
                    Priority = "Medium",
                    AssignedTo = 2, // Staff member
                    ResolutionDetails = "Replaced bulb and repaired wiring issue.",
                    ResolutionDate = DateTime.Now.AddDays(-1),
                    ReporterName = "Regular User",
                    AssignedToName = "Staff Member"
                });

                // Add some sample issue history
                _issueHistory.Add(new IssueStatusHistory
                {
                    HistoryID = 1,
                    IssueID = 1,
                    StatusFrom = "New",
                    StatusTo = "In Progress",
                    ChangedByUserID = 2, // Staff member
                    ChangeDate = DateTime.Now.AddDays(-5),
                    Comments = "Assigned to maintenance team for repair",
                    ChangedByName = "Staff Member"
                });

                _issueHistory.Add(new IssueStatusHistory
                {
                    HistoryID = 2,
                    IssueID = 4,
                    StatusFrom = "New",
                    StatusTo = "In Progress",
                    ChangedByUserID = 2, // Staff member
                    ChangeDate = DateTime.Now.AddDays(-8),
                    Comments = "Scheduled for repair",
                    ChangedByName = "Staff Member"
                });

                _issueHistory.Add(new IssueStatusHistory
                {
                    HistoryID = 3,
                    IssueID = 4,
                    StatusFrom = "In Progress",
                    StatusTo = "Resolved",
                    ChangedByUserID = 2, // Staff member
                    ChangeDate = DateTime.Now.AddDays(-1),
                    Comments = "Repairs completed",
                    ChangedByName = "Staff Member"
                });

                // Add some sample comments
                _issueComments.Add(new IssueComment
                {
                    CommentID = 1,
                    IssueID = 1,
                    UserID = 3, // Regular user
                    CommentText = "This pothole has gotten bigger since I first reported it.",
                    CommentDate = DateTime.Now.AddDays(-4),
                    IsPublic = true,
                    UserName = "Regular User"
                });

                _issueComments.Add(new IssueComment
                {
                    CommentID = 2,
                    IssueID = 1,
                    UserID = 2, // Staff member
                    CommentText = "We have scheduled repairs for next week. Thank you for your patience.",
                    CommentDate = DateTime.Now.AddDays(-3),
                    IsPublic = true,
                    UserName = "Staff Member"
                });

                _issueComments.Add(new IssueComment
                {
                    CommentID = 3,
                    IssueID = 1,
                    UserID = 2, // Staff member
                    CommentText = "Need to order additional materials. Might delay the repair by a day or two.",
                    CommentDate = DateTime.Now.AddDays(-2),
                    IsPublic = false, // Internal note
                    UserName = "Staff Member"
                });

                _issueComments.Add(new IssueComment
                {
                    CommentID = 4,
                    IssueID = 4,
                    UserID = 3, // Regular user
                    CommentText = "Thank you for fixing this so quickly!",
                    CommentDate = DateTime.Now.AddHours(-12),
                    IsPublic = true,
                    UserName = "Regular User"
                });
            }
        }
    }
}