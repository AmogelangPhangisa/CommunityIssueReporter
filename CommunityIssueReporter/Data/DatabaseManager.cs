using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace CommunityIssueReporter.Data
{
    public static class DatabaseManager
    {
        private static bool _isOfflineMode = false;

        // Get connection state
        public static bool IsOfflineMode => _isOfflineMode;

        // Set offline mode
        public static void SetOfflineMode(bool isOffline)
        {
            _isOfflineMode = isOffline;
        }

        // Get connection string safely
        private static string GetConnectionString()
        {
            try
            {
                return ConfigurationManager.ConnectionStrings["CommunityAppDB"].ConnectionString;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading connection string: {ex.Message}", ex);
            }
        }

        // Get connection
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        // Test database connection with detailed error reporting
        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (_isOfflineMode)
                return false;

            try
            {
                return ExecuteWithRetry(() =>
                {
                    using (SqlConnection connection = GetConnection())
                    {
                        connection.Open();
                    }
                }, 3);
            }
            catch (ConfigurationException cfgEx)
            {
                errorMessage = $"Configuration error: {cfgEx.Message}. Please check your App.config file.";
                return false;
            }
            catch (SqlException sqlEx)
            {
                errorMessage = $"SQL Server error ({sqlEx.Number}): {sqlEx.Message}";

                // Handle specific Azure SQL errors
                switch (sqlEx.Number)
                {
                    case 40613: // Database unavailable
                        errorMessage += "\nThe database is currently unavailable. It may be paused.";
                        break;
                    case 18456: // Login failed
                        errorMessage += "\nLogin failed. Please check your username and password.";
                        break;
                    case 4060: // Cannot open database
                        errorMessage += "\nThe database name may be incorrect or not accessible.";
                        break;
                    case 40615: // Firewall rule
                        errorMessage += "\nYour IP address is not allowed. Please add it to Azure SQL firewall rules.";
                        break;
                }

                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Connection error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nDetails: {ex.InnerException.Message}";
                }
                return false;
            }
        }

        // Overload for backward compatibility
        public static bool TestConnection()
        {
            string errorMessage;
            return TestConnection(out errorMessage);
        }

        // Execute with retry logic for transient errors
        private static bool ExecuteWithRetry(Action action, int maxRetries = 3)
        {
            int retries = 0;
            while (retries < maxRetries)
            {
                try
                {
                    action();
                    return true;
                }
                catch (SqlException ex) when (IsTransientError(ex.Number))
                {
                    retries++;
                    if (retries < maxRetries)
                    {
                        int delay = (int)Math.Pow(2, retries) * 100; // Exponential backoff
                        System.Threading.Thread.Sleep(delay);
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception)
                {
                    // Non-transient error, throw immediately
                    throw;
                }
            }
            return false;
        }

        private static bool IsTransientError(int errorNumber)
        {
            // List of transient error numbers
            int[] transientErrors = { 4060, 40197, 40501, 40613, 49918, 49919, 49920, 4221 };
            return Array.IndexOf(transientErrors, errorNumber) >= 0;
        }

        // Initialize database - create tables if they don't exist
        public static void InitializeDatabase()
        {
            if (_isOfflineMode) return;

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();

                    // Create Users table if it doesn't exist
                    ExecuteNonQuery(connection, @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                    BEGIN
                        CREATE TABLE Users (
                            UserID INT IDENTITY(1,1) PRIMARY KEY,
                            Username NVARCHAR(50) NOT NULL UNIQUE,
                            PasswordHash NVARCHAR(128) NOT NULL,
                            Salt NVARCHAR(128) NOT NULL,
                            Email NVARCHAR(100) NOT NULL UNIQUE,
                            FirstName NVARCHAR(50) NOT NULL,
                            LastName NVARCHAR(50) NOT NULL,
                            PhoneNumber NVARCHAR(20) NULL,
                            Address NVARCHAR(200) NULL,
                            UserRole NVARCHAR(20) NOT NULL DEFAULT 'User',
                            CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
                            LastLoginDate DATETIME NULL
                        )
                    END");

                    // Create Issues table if it doesn't exist
                    ExecuteNonQuery(connection, @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Issues')
                    BEGIN
                        CREATE TABLE Issues (
                            IssueID INT IDENTITY(1,1) PRIMARY KEY,
                            UserID INT NULL,
                            Location NVARCHAR(255) NOT NULL,
                            Category NVARCHAR(100) NOT NULL,
                            Description NVARCHAR(MAX) NOT NULL,
                            AttachmentPath NVARCHAR(500) NULL,
                            ReportedTime DATETIME NOT NULL,
                            Status NVARCHAR(50) NOT NULL DEFAULT 'New',
                            Priority NVARCHAR(20) NOT NULL DEFAULT 'Medium',
                            AssignedTo INT NULL,
                            ResolutionDetails NVARCHAR(MAX) NULL,
                            ResolutionDate DATETIME NULL,
                            FOREIGN KEY (UserID) REFERENCES Users(UserID),
                            FOREIGN KEY (AssignedTo) REFERENCES Users(UserID)
                        )
                    END");

                    // Create IssueStatusHistory table if it doesn't exist
                    ExecuteNonQuery(connection, @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IssueStatusHistory')
                    BEGIN
                        CREATE TABLE IssueStatusHistory (
                            HistoryID INT IDENTITY(1,1) PRIMARY KEY,
                            IssueID INT NOT NULL,
                            StatusFrom NVARCHAR(50) NOT NULL,
                            StatusTo NVARCHAR(50) NOT NULL,
                            ChangedBy INT NOT NULL,
                            ChangeDate DATETIME NOT NULL DEFAULT GETDATE(),
                            Comments NVARCHAR(500) NULL,
                            FOREIGN KEY (IssueID) REFERENCES Issues(IssueID),
                            FOREIGN KEY (ChangedBy) REFERENCES Users(UserID)
                        )
                    END");

                    // Create IssueComments table if it doesn't exist
                    ExecuteNonQuery(connection, @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IssueComments')
                    BEGIN
                        CREATE TABLE IssueComments (
                            CommentID INT IDENTITY(1,1) PRIMARY KEY,
                            IssueID INT NOT NULL,
                            UserID INT NOT NULL,
                            CommentText NVARCHAR(MAX) NOT NULL,
                            CommentDate DATETIME NOT NULL DEFAULT GETDATE(),
                            IsPublic BIT NOT NULL DEFAULT 1,
                            FOREIGN KEY (IssueID) REFERENCES Issues(IssueID),
                            FOREIGN KEY (UserID) REFERENCES Users(UserID)
                        )
                    END");

                    // Create Events table if it doesn't exist
                    ExecuteNonQuery(connection, @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Events')
                    BEGIN
                        CREATE TABLE Events (
                            EventID INT IDENTITY(1,1) PRIMARY KEY,
                            Title NVARCHAR(200) NOT NULL,
                            Description NVARCHAR(MAX) NOT NULL,
                            EventDate DATETIME NOT NULL,
                            EndDate DATETIME NULL,
                            Location NVARCHAR(200) NOT NULL,
                            Category NVARCHAR(100) NOT NULL,
                            CreatedBy INT NOT NULL,
                            CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
                            IsActive BIT NOT NULL DEFAULT 1,
                            ImagePath NVARCHAR(500) NULL,
                            FOREIGN KEY (CreatedBy) REFERENCES Users(UserID)
                        )
                    END");

                    // Create EventRegistrations table if it doesn't exist
                    ExecuteNonQuery(connection, @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EventRegistrations')
                    BEGIN
                        CREATE TABLE EventRegistrations (
                            RegistrationID INT IDENTITY(1,1) PRIMARY KEY,
                            EventID INT NOT NULL,
                            UserID INT NOT NULL,
                            RegistrationDate DATETIME NOT NULL DEFAULT GETDATE(),
                            AttendanceStatus NVARCHAR(20) NOT NULL DEFAULT 'Registered',
                            FOREIGN KEY (EventID) REFERENCES Events(EventID),
                            FOREIGN KEY (UserID) REFERENCES Users(UserID),
                            CONSTRAINT UQ_EventRegistration UNIQUE (EventID, UserID)
                        )
                    END");

                    // Create ServiceRequests table if it doesn't exist
                    ExecuteNonQuery(connection, @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ServiceRequests')
                    BEGIN
                        CREATE TABLE ServiceRequests (
                            RequestID INT IDENTITY(1,1) PRIMARY KEY,
                            UserID INT NOT NULL,
                            ServiceType NVARCHAR(100) NOT NULL,
                            Description NVARCHAR(MAX) NOT NULL,
                            SubmissionDate DATETIME NOT NULL DEFAULT GETDATE(),
                            Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
                            CompletionDate DATETIME NULL,
                            FOREIGN KEY (UserID) REFERENCES Users(UserID)
                        )
                    END");

                    // Create an admin user if no users exist
                    if (!UserExists(connection, "admin"))
                    {
                        // Generate a salt and hash for default password "Admin@123"
                        string salt = Utilities.SecurityHelper.GenerateSalt();
                        string passwordHash = Utilities.SecurityHelper.HashPassword("Admin@123", salt);

                        ExecuteNonQuery(connection, @"
                        INSERT INTO Users (Username, PasswordHash, Salt, Email, FirstName, LastName, UserRole)
                        VALUES (@Username, @PasswordHash, @Salt, @Email, @FirstName, @LastName, @UserRole)",
                        new SqlParameter("@Username", "admin"),
                        new SqlParameter("@PasswordHash", passwordHash),
                        new SqlParameter("@Salt", salt),
                        new SqlParameter("@Email", "admin@communityapp.org"),
                        new SqlParameter("@FirstName", "System"),
                        new SqlParameter("@LastName", "Administrator"),
                        new SqlParameter("@UserRole", "Admin"));
                    }

                    // Ensure attachments directory exists
                    string attachmentPath = ConfigurationManager.AppSettings["AttachmentStoragePath"];
                    if (!string.IsNullOrEmpty(attachmentPath))
                    {
                        try
                        {
                            if (!Directory.Exists(attachmentPath))
                            {
                                Directory.CreateDirectory(attachmentPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log the error but don't throw - attachment storage is not critical
                            Console.WriteLine($"Warning: Could not create attachments directory: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database initialization failed: {ex.Message}", ex);
            }
        }

        // Helper method to execute non-query SQL commands with retry
        public static void ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
        {
            if (_isOfflineMode) return;

            try
            {
                ExecuteWithRetry(() =>
                {
                    using (SqlConnection connection = GetConnection())
                    {
                        connection.Open();
                        ExecuteNonQuery(connection, commandText, parameters);
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Database operation failed: {ex.Message}", ex);
            }
        }

        // Helper method to execute non-query SQL commands with an existing connection
        private static void ExecuteNonQuery(SqlConnection connection, string commandText, params SqlParameter[] parameters)
        {
            using (SqlCommand command = new SqlCommand(commandText, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                command.ExecuteNonQuery();
            }
        }

        // Execute SQL query and return a DataTable
        public static DataTable ExecuteQuery(string commandText, params SqlParameter[] parameters)
        {
            if (_isOfflineMode)
            {
                // Return empty table in offline mode
                return new DataTable();
            }

            DataTable dataTable = new DataTable();

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(commandText, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database query failed: {ex.Message}", ex);
            }

            return dataTable;
        }

        // Execute scalar query
        public static object ExecuteScalar(string commandText, params SqlParameter[] parameters)
        {
            if (_isOfflineMode)
            {
                return null;
            }

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(commandText, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        return command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database operation failed: {ex.Message}", ex);
            }
        }

        // Check if a user exists
        private static bool UserExists(SqlConnection connection, string username)
        {
            using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username", connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                return (int)command.ExecuteScalar() > 0;
            }
        }

        // Insert record and return the identity value
        public static int InsertAndGetId(string commandText, params SqlParameter[] parameters)
        {
            if (_isOfflineMode)
            {
                return -1; // Indicate offline mode
            }

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(commandText + "; SELECT SCOPE_IDENTITY();", connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database operation failed: {ex.Message}", ex);
            }
        }

        // Begin a transaction
        public static SqlTransaction BeginTransaction()
        {
            if (_isOfflineMode)
            {
                return null;
            }

            SqlConnection connection = GetConnection();
            try
            {
                connection.Open();
                return connection.BeginTransaction();
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        // Execute command with transaction
        public static void ExecuteNonQuery(SqlTransaction transaction, string commandText, params SqlParameter[] parameters)
        {
            if (transaction == null) return;

            using (SqlCommand command = new SqlCommand(commandText, transaction.Connection, transaction))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                command.ExecuteNonQuery();
            }
        }
    }
}