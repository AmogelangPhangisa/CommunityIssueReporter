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
    public static class EventRepository
    {
        // In-memory storage for offline mode
        private static List<Event> _events = new List<Event>();
        private static List<EventRegistration> _registrations = new List<EventRegistration>();

        // Advanced data structures for optimized event access
        private static Dictionary<int, Event> _eventsById = new Dictionary<int, Event>();
        private static SortedDictionary<DateTime, List<Event>> _eventsByDate = new SortedDictionary<DateTime, List<Event>>();
        private static Dictionary<string, SortedSet<int>> _eventIdsByCategory = new Dictionary<string, SortedSet<int>>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<bool, HashSet<int>> _eventIdsByStatus = new Dictionary<bool, HashSet<int>>();

        // Track if cache is initialized
        private static bool _isCacheInitialized = false;

        // Initialize optimized cache
        private static void InitializeCache()
        {
            // Skip if already initialized and not in offline mode
            if (_isCacheInitialized && !DatabaseManager.IsOfflineMode)
                return;

            // Clear existing cache
            _eventsById.Clear();
            _eventsByDate.Clear();
            _eventIdsByCategory.Clear();
            _eventIdsByStatus.Clear();

            // Initialize status collections
            _eventIdsByStatus[true] = new HashSet<int>();   // Active events
            _eventIdsByStatus[false] = new HashSet<int>();  // Inactive events

            // Get all events
            List<Event> allEvents = GetAllEventsRaw();

            // Populate optimized data structures
            foreach (var evt in allEvents)
            {
                // Add to events by ID
                _eventsById[evt.EventID] = evt;

                // Add to events by date (use date part as key)
                DateTime dateKey = evt.EventDate.Date;
                if (!_eventsByDate.ContainsKey(dateKey))
                {
                    _eventsByDate[dateKey] = new List<Event>();
                }
                _eventsByDate[dateKey].Add(evt);

                // Add to events by category
                if (!string.IsNullOrEmpty(evt.Category))
                {
                    if (!_eventIdsByCategory.ContainsKey(evt.Category))
                    {
                        _eventIdsByCategory[evt.Category] = new SortedSet<int>();
                    }
                    _eventIdsByCategory[evt.Category].Add(evt.EventID);
                }

                // Add to events by status
                _eventIdsByStatus[evt.IsActive].Add(evt.EventID);
            }

            _isCacheInitialized = true;
        }

        // Get all events without filters (for internal use)
        private static List<Event> GetAllEventsRaw()
        {
            List<Event> events = new List<Event>();

            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Return all events from in-memory list
                    return _events.ToList();
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT e.EventID, e.Title, e.Description, e.EventDate, e.EndDate, 
                               e.Location, e.Category, e.CreatedBy, e.CreatedDate, 
                               e.IsActive, e.ImagePath,
                               CONCAT(u.FirstName, ' ', u.LastName) AS CreatedByName
                        FROM Events e
                        JOIN Users u ON e.CreatedBy = u.UserID
                        ORDER BY e.EventDate DESC";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    events.Add(new Event
                                    {
                                        EventID = Convert.ToInt32(reader["EventID"]),
                                        Title = reader["Title"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        EventDate = Convert.ToDateTime(reader["EventDate"]),
                                        EndDate = reader["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EndDate"]),
                                        Location = reader["Location"].ToString(),
                                        Category = reader["Category"].ToString(),
                                        CreatedByUserID = Convert.ToInt32(reader["CreatedBy"]),
                                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                                        ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString(),
                                        CreatedByName = reader["CreatedByName"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all events: {ex.Message}");
            }

            return events;
        }

        // Get all events (optimized)
        public static List<Event> GetAllEvents(bool activeOnly = true)
        {
            // Initialize cache if needed
            if (!_isCacheInitialized)
                InitializeCache();

            try
            {
                // Use optimized data structures
                if (activeOnly)
                {
                    return _eventIdsByStatus[true].Select(id => _eventsById[id])
                                                  .OrderByDescending(e => e.EventDate)
                                                  .ToList();
                }
                else
                {
                    return _eventsById.Values.OrderByDescending(e => e.EventDate).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting events from cache: {ex.Message}");

                // Fallback to old implementation
                if (DatabaseManager.IsOfflineMode)
                {
                    var query = _events.AsEnumerable();

                    if (activeOnly)
                    {
                        query = query.Where(e => e.IsActive);
                    }

                    return query.OrderByDescending(e => e.EventDate).ToList();
                }
                else
                {
                    // Use original database query
                    return GetAllEventsRaw().Where(e => !activeOnly || e.IsActive)
                                           .OrderByDescending(e => e.EventDate)
                                           .ToList();
                }
            }
        }

        // Get upcoming events (optimized)
        public static List<Event> GetUpcomingEvents(int maxCount = 50)
        {
            // Initialize cache if needed
            if (!_isCacheInitialized)
                InitializeCache();

            try
            {
                DateTime today = DateTime.Now.Date;

                // Use optimized data structures to find upcoming dates
                return _eventsByDate.Where(kvp => kvp.Key >= today)
                                   .SelectMany(kvp => kvp.Value)
                                   .Where(e => e.IsActive && e.EventDate > DateTime.Now)
                                   .OrderBy(e => e.EventDate)
                                   .Take(maxCount)
                                   .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting upcoming events from cache: {ex.Message}");

                // Fallback to old implementation
                if (DatabaseManager.IsOfflineMode)
                {
                    return _events.Where(e => e.IsActive && e.EventDate > DateTime.Now)
                                 .OrderBy(e => e.EventDate)
                                 .Take(maxCount)
                                 .ToList();
                }
                else
                {
                    // Online mode, use database
                    List<Event> events = new List<Event>();

                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT TOP (@MaxCount) e.EventID, e.Title, e.Description, e.EventDate, e.EndDate, 
                               e.Location, e.Category, e.CreatedBy, e.CreatedDate, 
                               e.IsActive, e.ImagePath,
                               CONCAT(u.FirstName, ' ', u.LastName) AS CreatedByName
                        FROM Events e
                        JOIN Users u ON e.CreatedBy = u.UserID
                        WHERE e.IsActive = 1 AND e.EventDate > GETDATE()
                        ORDER BY e.EventDate ASC";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@MaxCount", maxCount);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    events.Add(new Event
                                    {
                                        EventID = Convert.ToInt32(reader["EventID"]),
                                        Title = reader["Title"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        EventDate = Convert.ToDateTime(reader["EventDate"]),
                                        EndDate = reader["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EndDate"]),
                                        Location = reader["Location"].ToString(),
                                        Category = reader["Category"].ToString(),
                                        CreatedByUserID = Convert.ToInt32(reader["CreatedBy"]),
                                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                                        ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString(),
                                        CreatedByName = reader["CreatedByName"].ToString()
                                    });
                                }
                            }
                        }
                    }

                    return events;
                }
            }
        }

        // Get event by ID (optimized)
        public static Event GetEventByID(int eventID)
        {
            // Initialize cache if needed
            if (!_isCacheInitialized)
                InitializeCache();

            try
            {
                // Use optimized lookup by ID
                if (_eventsById.TryGetValue(eventID, out Event evt))
                {
                    return evt;
                }

                // Event not in cache, fetch from database if online
                if (!DatabaseManager.IsOfflineMode)
                {
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT e.EventID, e.Title, e.Description, e.EventDate, e.EndDate, 
                               e.Location, e.Category, e.CreatedBy, e.CreatedDate, 
                               e.IsActive, e.ImagePath,
                               CONCAT(u.FirstName, ' ', u.LastName) AS CreatedByName
                        FROM Events e
                        JOIN Users u ON e.CreatedBy = u.UserID
                        WHERE e.EventID = @EventID";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EventID", eventID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    Event newEvent = new Event
                                    {
                                        EventID = Convert.ToInt32(reader["EventID"]),
                                        Title = reader["Title"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        EventDate = Convert.ToDateTime(reader["EventDate"]),
                                        EndDate = reader["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EndDate"]),
                                        Location = reader["Location"].ToString(),
                                        Category = reader["Category"].ToString(),
                                        CreatedByUserID = Convert.ToInt32(reader["CreatedBy"]),
                                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                                        ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString(),
                                        CreatedByName = reader["CreatedByName"].ToString()
                                    };

                                    // Add to cache
                                    AddToCache(newEvent);
                                    return newEvent;
                                }
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting event by ID: {ex.Message}");

                // Fallback to old implementation
                if (DatabaseManager.IsOfflineMode)
                {
                    return _events.FirstOrDefault(e => e.EventID == eventID);
                }
                else
                {
                    // Use original database query (already implemented above)
                    return null;
                }
            }
        }

        // Add a new event (with cache update)
        public static int AddEvent(Event newEvent)
        {
            try
            {
                // Process image if there is one
                string finalImagePath = ProcessImage(newEvent.ImagePath);
                newEvent.ImagePath = finalImagePath;

                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Add event to in-memory list
                    newEvent.EventID = _events.Count > 0 ? _events.Max(e => e.EventID) + 1 : 1;

                    // Set created by name
                    var user = UserRepository.GetUserByID(newEvent.CreatedByUserID);
                    if (user != null)
                    {
                        newEvent.CreatedByName = user.FullName;
                    }

                    _events.Add(newEvent);

                    // Add to cache
                    if (_isCacheInitialized)
                    {
                        AddToCache(newEvent);
                    }

                    return newEvent.EventID;
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string insertQuery = @"
                        INSERT INTO Events (Title, Description, EventDate, EndDate, Location, 
                                           Category, CreatedBy, CreatedDate, IsActive, ImagePath)
                        VALUES (@Title, @Description, @EventDate, @EndDate, @Location, 
                                @Category, @CreatedBy, @CreatedDate, @IsActive, @ImagePath);
                        
                        SELECT SCOPE_IDENTITY();";

                        using (SqlCommand command = new SqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Title", newEvent.Title);
                            command.Parameters.AddWithValue("@Description", newEvent.Description);
                            command.Parameters.AddWithValue("@EventDate", newEvent.EventDate);
                            command.Parameters.AddWithValue("@EndDate", newEvent.EndDate ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@Location", newEvent.Location);
                            command.Parameters.AddWithValue("@Category", newEvent.Category);
                            command.Parameters.AddWithValue("@CreatedBy", newEvent.CreatedByUserID);
                            command.Parameters.AddWithValue("@CreatedDate", newEvent.CreatedDate);
                            command.Parameters.AddWithValue("@IsActive", newEvent.IsActive);
                            command.Parameters.AddWithValue("@ImagePath", string.IsNullOrEmpty(newEvent.ImagePath) ? (object)DBNull.Value : newEvent.ImagePath);

                            // Get the new event ID
                            int newEventId = Convert.ToInt32(command.ExecuteScalar());
                            newEvent.EventID = newEventId;

                            // Set created by name
                            string createdByQuery = "SELECT CONCAT(FirstName, ' ', LastName) FROM Users WHERE UserID = @UserID";
                            using (SqlCommand nameCommand = new SqlCommand(createdByQuery, connection))
                            {
                                nameCommand.Parameters.AddWithValue("@UserID", newEvent.CreatedByUserID);
                                object result = nameCommand.ExecuteScalar();
                                if (result != null && result != DBNull.Value)
                                {
                                    newEvent.CreatedByName = result.ToString();
                                }
                            }

                            // Add to cache
                            if (_isCacheInitialized)
                            {
                                AddToCache(newEvent);
                            }

                            return newEventId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding event: {ex.Message}");
                return -1;
            }
        }

        // Update an event (with cache update)
        public static bool UpdateEvent(Event updatedEvent)
        {
            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Update event in in-memory list
                    int index = _events.FindIndex(e => e.EventID == updatedEvent.EventID);
                    if (index >= 0)
                    {
                        // Preserve the original image path if no new one is provided
                        if (string.IsNullOrEmpty(updatedEvent.ImagePath))
                        {
                            updatedEvent.ImagePath = _events[index].ImagePath;
                        }
                        else if (updatedEvent.ImagePath != _events[index].ImagePath)
                        {
                            // Process new image
                            updatedEvent.ImagePath = ProcessImage(updatedEvent.ImagePath);
                        }

                        _events[index] = updatedEvent;

                        // Update cache
                        if (_isCacheInitialized)
                        {
                            RemoveFromCache(updatedEvent.EventID);
                            AddToCache(updatedEvent);
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

                        // Get current image path if not changing it
                        if (string.IsNullOrEmpty(updatedEvent.ImagePath))
                        {
                            string selectQuery = "SELECT ImagePath FROM Events WHERE EventID = @EventID";

                            using (SqlCommand command = new SqlCommand(selectQuery, connection))
                            {
                                command.Parameters.AddWithValue("@EventID", updatedEvent.EventID);

                                object result = command.ExecuteScalar();
                                if (result != null && result != DBNull.Value)
                                {
                                    updatedEvent.ImagePath = result.ToString();
                                }
                            }
                        }
                        else
                        {
                            // Process new image
                            updatedEvent.ImagePath = ProcessImage(updatedEvent.ImagePath);
                        }

                        string updateQuery = @"
                        UPDATE Events 
                        SET Title = @Title,
                            Description = @Description,
                            EventDate = @EventDate,
                            EndDate = @EndDate,
                            Location = @Location,
                            Category = @Category,
                            IsActive = @IsActive,
                            ImagePath = @ImagePath
                        WHERE EventID = @EventID";

                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Title", updatedEvent.Title);
                            command.Parameters.AddWithValue("@Description", updatedEvent.Description);
                            command.Parameters.AddWithValue("@EventDate", updatedEvent.EventDate);
                            command.Parameters.AddWithValue("@EndDate", updatedEvent.EndDate ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@Location", updatedEvent.Location);
                            command.Parameters.AddWithValue("@Category", updatedEvent.Category);
                            command.Parameters.AddWithValue("@IsActive", updatedEvent.IsActive);
                            command.Parameters.AddWithValue("@ImagePath", string.IsNullOrEmpty(updatedEvent.ImagePath) ? (object)DBNull.Value : updatedEvent.ImagePath);
                            command.Parameters.AddWithValue("@EventID", updatedEvent.EventID);

                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                // Update cache
                                if (_isCacheInitialized)
                                {
                                    RemoveFromCache(updatedEvent.EventID);
                                    AddToCache(updatedEvent);
                                }

                                return true;
                            }

                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating event: {ex.Message}");
                return false;
            }
        }

        // Add to cache
        private static void AddToCache(Event evt)
        {
            // Add to events by ID
            _eventsById[evt.EventID] = evt;

            // Add to events by date
            DateTime dateKey = evt.EventDate.Date;
            if (!_eventsByDate.ContainsKey(dateKey))
            {
                _eventsByDate[dateKey] = new List<Event>();
            }
            _eventsByDate[dateKey].Add(evt);

            // Add to events by category
            if (!string.IsNullOrEmpty(evt.Category))
            {
                if (!_eventIdsByCategory.ContainsKey(evt.Category))
                {
                    _eventIdsByCategory[evt.Category] = new SortedSet<int>();
                }
                _eventIdsByCategory[evt.Category].Add(evt.EventID);
            }

            // Add to events by status
            _eventIdsByStatus[evt.IsActive].Add(evt.EventID);
        }

        // Remove from cache
        private static void RemoveFromCache(int eventId)
        {
            if (!_eventsById.ContainsKey(eventId))
                return;

            Event evt = _eventsById[eventId];

            // Remove from events by ID
            _eventsById.Remove(eventId);

            // Remove from events by date
            DateTime dateKey = evt.EventDate.Date;
            if (_eventsByDate.ContainsKey(dateKey))
            {
                _eventsByDate[dateKey].RemoveAll(e => e.EventID == eventId);
                if (_eventsByDate[dateKey].Count == 0)
                {
                    _eventsByDate.Remove(dateKey);
                }
            }

            // Remove from events by category
            if (!string.IsNullOrEmpty(evt.Category) && _eventIdsByCategory.ContainsKey(evt.Category))
            {
                _eventIdsByCategory[evt.Category].Remove(eventId);
                if (_eventIdsByCategory[evt.Category].Count == 0)
                {
                    _eventIdsByCategory.Remove(evt.Category);
                }
            }

            // Remove from events by status
            _eventIdsByStatus[evt.IsActive].Remove(eventId);
        }

        // Check if user is registered for an event
        public static bool IsUserRegistered(int eventID, int userID)
        {
            if (DatabaseManager.IsOfflineMode)
            {
                return _registrations.Any(r => r.EventID == eventID && r.UserID == userID);
            }
            else
            {
                try
                {
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string query = "SELECT COUNT(*) FROM EventRegistrations WHERE EventID = @EventID AND UserID = @UserID";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@EventID", eventID);
                            command.Parameters.AddWithValue("@UserID", userID);
                            return (int)command.ExecuteScalar() > 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking registration: {ex.Message}");
                    return false;
                }
            }
        }

        // Register for an event
        public static bool RegisterForEvent(int eventID, int userID)
        {
            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Check if already registered
                    if (_registrations.Any(r => r.EventID == eventID && r.UserID == userID))
                    {
                        return false;
                    }

                    // Get event and user names
                    string eventTitle = _events.FirstOrDefault(e => e.EventID == eventID)?.Title ?? "Unknown Event";
                    string userName = UserRepository.GetUserByID(userID)?.FullName ?? "Unknown User";

                    // Add registration to in-memory list
                    _registrations.Add(new EventRegistration
                    {
                        RegistrationID = _registrations.Count > 0 ? _registrations.Max(r => r.RegistrationID) + 1 : 1,
                        EventID = eventID,
                        UserID = userID,
                        RegistrationDate = DateTime.Now,
                        AttendanceStatus = "Registered",
                        EventTitle = eventTitle,
                        UserName = userName
                    });

                    return true;
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        // Check if already registered
                        string checkQuery = "SELECT COUNT(*) FROM EventRegistrations WHERE EventID = @EventID AND UserID = @UserID";

                        using (SqlCommand command = new SqlCommand(checkQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EventID", eventID);
                            command.Parameters.AddWithValue("@UserID", userID);

                            int count = Convert.ToInt32(command.ExecuteScalar());
                            if (count > 0)
                            {
                                return false;
                            }
                        }

                        // Add registration
                        string insertQuery = @"
                        INSERT INTO EventRegistrations (EventID, UserID, RegistrationDate, AttendanceStatus)
                        VALUES (@EventID, @UserID, GETDATE(), 'Registered')";

                        using (SqlCommand command = new SqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EventID", eventID);
                            command.Parameters.AddWithValue("@UserID", userID);

                            int rowsAffected = command.ExecuteNonQuery();
                            return rowsAffected > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering for event: {ex.Message}");
                return false;
            }
        }

        // Get registrations for an event
        public static List<EventRegistration> GetEventRegistrations(int eventID)
        {
            List<EventRegistration> registrations = new List<EventRegistration>();

            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Filter registrations from in-memory list
                    return _registrations.Where(r => r.EventID == eventID).ToList();
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT r.RegistrationID, r.EventID, r.UserID, r.RegistrationDate, r.AttendanceStatus,
                               CONCAT(u.FirstName, ' ', u.LastName) AS UserName,
                               e.Title AS EventTitle
                        FROM EventRegistrations r
                        JOIN Users u ON r.UserID = u.UserID
                        JOIN Events e ON r.EventID = e.EventID
                        WHERE r.EventID = @EventID
                        ORDER BY r.RegistrationDate";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EventID", eventID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    registrations.Add(new EventRegistration
                                    {
                                        RegistrationID = Convert.ToInt32(reader["RegistrationID"]),
                                        EventID = Convert.ToInt32(reader["EventID"]),
                                        UserID = Convert.ToInt32(reader["UserID"]),
                                        RegistrationDate = Convert.ToDateTime(reader["RegistrationDate"]),
                                        AttendanceStatus = reader["AttendanceStatus"].ToString(),
                                        UserName = reader["UserName"].ToString(),
                                        EventTitle = reader["EventTitle"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting event registrations: {ex.Message}");
            }

            return registrations;
        }

        // Get events registered by a user
        public static List<Event> GetEventsByUser(int userID)
        {
            List<Event> events = new List<Event>();

            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Get event IDs registered by user
                    var eventIDs = _registrations.Where(r => r.UserID == userID).Select(r => r.EventID).ToList();

                    // Get events
                    return _events.Where(e => eventIDs.Contains(e.EventID)).OrderByDescending(e => e.EventDate).ToList();
                }
                else
                {
                    // Online mode, use database
                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string selectQuery = @"
                        SELECT e.EventID, e.Title, e.Description, e.EventDate, e.EndDate, 
                               e.Location, e.Category, e.CreatedBy, e.CreatedDate, 
                               e.IsActive, e.ImagePath,
                               CONCAT(u.FirstName, ' ', u.LastName) AS CreatedByName
                        FROM Events e
                        JOIN EventRegistrations r ON e.EventID = r.EventID
                        JOIN Users u ON e.CreatedBy = u.UserID
                        WHERE r.UserID = @UserID
                        ORDER BY e.EventDate DESC";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", userID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    events.Add(new Event
                                    {
                                        EventID = Convert.ToInt32(reader["EventID"]),
                                        Title = reader["Title"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        EventDate = Convert.ToDateTime(reader["EventDate"]),
                                        EndDate = reader["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EndDate"]),
                                        Location = reader["Location"].ToString(),
                                        Category = reader["Category"].ToString(),
                                        CreatedByUserID = Convert.ToInt32(reader["CreatedBy"]),
                                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                                        ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString(),
                                        CreatedByName = reader["CreatedByName"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting events by user: {ex.Message}");
            }

            return events;
        }

        // Update attendance status
        public static bool UpdateAttendanceStatus(int registrationID, string newStatus)
        {
            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Update status in in-memory list
                    int index = _registrations.FindIndex(r => r.RegistrationID == registrationID);
                    if (index >= 0)
                    {
                        _registrations[index].AttendanceStatus = newStatus;
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

                        string updateQuery = "UPDATE EventRegistrations SET AttendanceStatus = @Status WHERE RegistrationID = @RegistrationID";

                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Status", newStatus);
                            command.Parameters.AddWithValue("@RegistrationID", registrationID);

                            int rowsAffected = command.ExecuteNonQuery();
                            return rowsAffected > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating attendance status: {ex.Message}");
                return false;
            }
        }

        // Cancel registration
        public static bool CancelRegistration(int eventID, int userID)
        {
            try
            {
                // Check if in offline mode
                if (DatabaseManager.IsOfflineMode)
                {
                    // Remove registration from in-memory list
                    int index = _registrations.FindIndex(r => r.EventID == eventID && r.UserID == userID);
                    if (index >= 0)
                    {
                        _registrations.RemoveAt(index);
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

                        string deleteQuery = "DELETE FROM EventRegistrations WHERE EventID = @EventID AND UserID = @UserID";

                        using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EventID", eventID);
                            command.Parameters.AddWithValue("@UserID", userID);

                            int rowsAffected = command.ExecuteNonQuery();
                            return rowsAffected > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error canceling registration: {ex.Message}");
                return false;
            }
        }

        // Process image file
        private static string ProcessImage(string sourcePath)
        {
            if (string.IsNullOrEmpty(sourcePath))
                return null;

            try
            {
                // Get attachment storage path from config
                string attachmentStoragePath = ConfigurationManager.AppSettings["AttachmentStoragePath"];

                // Create attachment directory if it doesn't exist
                if (!Directory.Exists(attachmentStoragePath))
                {
                    Directory.CreateDirectory(attachmentStoragePath);
                }

                // Create a folder for event images
                string eventImagesPath = Path.Combine(attachmentStoragePath, "EventImages");
                if (!Directory.Exists(eventImagesPath))
                {
                    Directory.CreateDirectory(eventImagesPath);
                }

                // Create a unique filename to avoid collisions
                string fileName = Path.GetFileName(sourcePath);
                string uniqueFileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{fileName}";
                string destinationPath = Path.Combine(eventImagesPath, uniqueFileName);

                // Copy the file to the storage location
                File.Copy(sourcePath, destinationPath, true);

                return destinationPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
                return null;
            }
        }

        // Get all available categories (optimized)
        public static List<string> GetAllCategories()
        {
            // Initialize cache if needed
            if (!_isCacheInitialized)
                InitializeCache();

            try
            {
                // Return sorted categories from cache
                return _eventIdsByCategory.Keys.OrderBy(c => c).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting categories from cache: {ex.Message}");

                // Fallback to direct query
                if (DatabaseManager.IsOfflineMode)
                {
                    return _events.Select(e => e.Category)
                                 .Where(c => !string.IsNullOrEmpty(c))
                                 .Distinct()
                                 .OrderBy(c => c)
                                 .ToList();
                }
                else
                {
                    List<string> categories = new List<string>();

                    using (SqlConnection connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();

                        string query = "SELECT DISTINCT Category FROM Events WHERE Category IS NOT NULL ORDER BY Category";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    categories.Add(reader["Category"].ToString());
                                }
                            }
                        }
                    }

                    return categories;
                }
            }
        }

        // Advanced search function with multiple criteria
        public static List<Event> SearchEvents(string category = null, DateTime? startDate = null,
                                          DateTime? endDate = null, string keyword = null, bool includeInactive = false)
        {
            // Initialize cache if needed
            if (!_isCacheInitialized)
                InitializeCache();

            try
            {
                // Start with all events (or only active ones)
                IEnumerable<Event> results = includeInactive
                    ? _eventsById.Values
                    : _eventIdsByStatus[true].Select(id => _eventsById[id]);

                // Filter by category if specified
                if (!string.IsNullOrEmpty(category) && category != "All Categories" && _eventIdsByCategory.ContainsKey(category))
                {
                    HashSet<int> categoryIds = new HashSet<int>(_eventIdsByCategory[category]);
                    results = results.Where(e => categoryIds.Contains(e.EventID));
                }

                // Filter by date range
                if (startDate.HasValue)
                {
                    DateTime startDateValue = startDate.Value.Date;
                    results = results.Where(e =>
                        e.EventDate.Date >= startDateValue ||
                        (e.EndDate.HasValue && e.EndDate.Value.Date >= startDateValue));
                }

                if (endDate.HasValue)
                {
                    DateTime endDateValue = endDate.Value.Date.AddDays(1).AddSeconds(-1); // End of day
                    results = results.Where(e => e.EventDate <= endDateValue);
                }

                // Filter by keyword in title, description, or location
                if (!string.IsNullOrEmpty(keyword))
                {
                    string keywordLower = keyword.ToLower();
                    results = results.Where(e =>
                        (e.Title != null && e.Title.ToLower().Contains(keywordLower)) ||
                        (e.Description != null && e.Description.ToLower().Contains(keywordLower)) ||
                        (e.Location != null && e.Location.ToLower().Contains(keywordLower)));
                }

                // Return as list, sorted by date
                return results.OrderBy(e => e.EventDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching events: {ex.Message}");

                // Fallback to direct filtering
                List<Event> allEvents = GetAllEventsRaw();

                // Apply filters
                IEnumerable<Event> filteredEvents = allEvents;

                if (!includeInactive)
                {
                    filteredEvents = filteredEvents.Where(e => e.IsActive);
                }

                if (!string.IsNullOrEmpty(category) && category != "All Categories")
                {
                    filteredEvents = filteredEvents.Where(e => e.Category == category);
                }

                if (startDate.HasValue)
                {
                    DateTime startDateValue = startDate.Value.Date;
                    filteredEvents = filteredEvents.Where(e =>
                        e.EventDate.Date >= startDateValue ||
                        (e.EndDate.HasValue && e.EndDate.Value.Date >= startDateValue));
                }

                if (endDate.HasValue)
                {
                    DateTime endDateValue = endDate.Value.Date.AddDays(1).AddSeconds(-1); // End of day
                    filteredEvents = filteredEvents.Where(e => e.EventDate <= endDateValue);
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    string keywordLower = keyword.ToLower();
                    filteredEvents = filteredEvents.Where(e =>
                        (e.Title != null && e.Title.ToLower().Contains(keywordLower)) ||
                        (e.Description != null && e.Description.ToLower().Contains(keywordLower)) ||
                        (e.Location != null && e.Location.ToLower().Contains(keywordLower)));
                }

                return filteredEvents.OrderBy(e => e.EventDate).ToList();
            }
        }

        // Get event count by category (for dashboards/charts)
        public static Dictionary<string, int> GetEventCountsByCategory(bool activeOnly = true)
        {
            // Initialize cache if needed
            if (!_isCacheInitialized)
                InitializeCache();

            try
            {
                Dictionary<string, int> result = new Dictionary<string, int>();

                if (activeOnly)
                {
                    // Only count active events
                    HashSet<int> activeIds = _eventIdsByStatus[true];

                    foreach (var kvp in _eventIdsByCategory)
                    {
                        // Count intersection of this category with active events
                        int count = kvp.Value.Count(id => activeIds.Contains(id));
                        if (count > 0)
                        {
                            result[kvp.Key] = count;
                        }
                    }
                }
                else
                {
                    // Count all events by category
                    foreach (var kvp in _eventIdsByCategory)
                    {
                        result[kvp.Key] = kvp.Value.Count;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting event counts by category: {ex.Message}");

                // Fallback to direct counting
                return GetAllEventsRaw()
                    .Where(e => !activeOnly || e.IsActive)
                    .GroupBy(e => e.Category)
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .ToDictionary(g => g.Key, g => g.Count());
            }
        }

        // Get upcoming events by date range
        public static SortedDictionary<DateTime, List<Event>> GetUpcomingEventsByDate(DateTime startDate, int daysAhead)
        {
            // Initialize cache if needed
            if (!_isCacheInitialized)
                InitializeCache();

            try
            {
                // Create result dictionary
                SortedDictionary<DateTime, List<Event>> result = new SortedDictionary<DateTime, List<Event>>();

                // Calculate end date
                DateTime endDate = startDate.AddDays(daysAhead);

                // Get all dates in the range
                foreach (var kvp in _eventsByDate)
                {
                    if (kvp.Key >= startDate && kvp.Key <= endDate)
                    {
                        // Only include active events
                        List<Event> activeEvents = kvp.Value.Where(e => e.IsActive).ToList();
                        if (activeEvents.Count > 0)
                        {
                            result[kvp.Key] = activeEvents;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting upcoming events by date: {ex.Message}");

                // Fallback to direct filtering
                SortedDictionary<DateTime, List<Event>> result = new SortedDictionary<DateTime, List<Event>>();

                // Calculate end date
                DateTime endDate = startDate.AddDays(daysAhead);

                // Filter and group events
                var events = GetAllEventsRaw()
                    .Where(e => e.IsActive && e.EventDate.Date >= startDate && e.EventDate.Date <= endDate)
                    .ToList();

                foreach (var evt in events)
                {
                    DateTime dateKey = evt.EventDate.Date;
                    if (!result.ContainsKey(dateKey))
                    {
                        result[dateKey] = new List<Event>();
                    }
                    result[dateKey].Add(evt);
                }

                return result;
            }
        }

        // Initialize sample events for offline mode
        public static void InitializeOfflineEvents()
        {
            if (_events.Count == 0)
            {
                // Make sure we have sample users
                UserRepository.InitializeOfflineUsers();

                // Add some sample events
                _events.Add(new Event
                {
                    EventID = 1,
                    Title = "Community Clean-up Day",
                    Description = "Join us for a day of community service as we clean up Central Park. Gloves and bags will be provided. Lunch will be served to all volunteers.",
                    EventDate = DateTime.Now.AddDays(7),
                    EndDate = DateTime.Now.AddDays(7).AddHours(4),
                    Location = "Central Park, Main Entrance",
                    Category = "Community Service",
                    CreatedByUserID = 1, // Admin
                    CreatedDate = DateTime.Now.AddDays(-14),
                    IsActive = true,
                    CreatedByName = "System Administrator"
                });

                _events.Add(new Event
                {
                    EventID = 2,
                    Title = "Town Hall Meeting",
                    Description = "Discuss upcoming infrastructure projects and provide feedback on community priorities. The mayor and city council will be present to answer questions.",
                    EventDate = DateTime.Now.AddDays(14).AddHours(18),
                    EndDate = DateTime.Now.AddDays(14).AddHours(20),
                    Location = "City Hall, Council Chambers",
                    Category = "Government",
                    CreatedByUserID = 1, // Admin
                    CreatedDate = DateTime.Now.AddDays(-10),
                    IsActive = true,
                    CreatedByName = "System Administrator"
                });

                _events.Add(new Event
                {
                    EventID = 3,
                    Title = "Neighborhood Watch Meeting",
                    Description = "Monthly meeting to discuss neighborhood safety concerns and coordinate watch schedules.",
                    EventDate = DateTime.Now.AddDays(-5).AddHours(19),
                    EndDate = DateTime.Now.AddDays(-5).AddHours(20),
                    Location = "Community Center, Room 102",
                    Category = "Safety",
                    CreatedByUserID = 2, // Staff
                    CreatedDate = DateTime.Now.AddDays(-20),
                    IsActive = true,
                    CreatedByName = "Staff Member"
                });

                _events.Add(new Event
                {
                    EventID = 4,
                    Title = "Summer Festival Planning",
                    Description = "Help plan this year's summer festival. We need volunteers for various committees including food, entertainment, and logistics.",
                    EventDate = DateTime.Now.AddDays(3).AddHours(17),
                    EndDate = DateTime.Now.AddDays(3).AddHours(19),
                    Location = "Public Library, Conference Room",
                    Category = "Recreation",
                    CreatedByUserID = 2, // Staff
                    CreatedDate = DateTime.Now.AddDays(-7),
                    IsActive = true,
                    CreatedByName = "Staff Member"
                });

                // Add some sample registrations
                _registrations.Add(new EventRegistration
                {
                    RegistrationID = 1,
                    EventID = 1,
                    UserID = 3, // Regular user
                    RegistrationDate = DateTime.Now.AddDays(-10),
                    AttendanceStatus = "Registered",
                    UserName = "Regular User",
                    EventTitle = "Community Clean-up Day"
                });

                _registrations.Add(new EventRegistration
                {
                    RegistrationID = 2,
                    EventID = 2,
                    UserID = 3, // Regular user
                    RegistrationDate = DateTime.Now.AddDays(-8),
                    AttendanceStatus = "Registered",
                    UserName = "Regular User",
                    EventTitle = "Town Hall Meeting"
                });

                _registrations.Add(new EventRegistration
                {
                    RegistrationID = 3,
                    EventID = 3,
                    UserID = 2, // Staff member
                    RegistrationDate = DateTime.Now.AddDays(-15),
                    AttendanceStatus = "Attended",
                    UserName = "Staff Member",
                    EventTitle = "Neighborhood Watch Meeting"
                });

                _registrations.Add(new EventRegistration
                {
                    RegistrationID = 4,
                    EventID = 3,
                    UserID = 3, // Regular user
                    RegistrationDate = DateTime.Now.AddDays(-12),
                    AttendanceStatus = "Attended",
                    UserName = "Regular User",
                    EventTitle = "Neighborhood Watch Meeting"
                });

                // Initialize cache with sample data
                InitializeCache();
            }
        }

        // Refresh the cache (call this after bulk operations)
        public static void RefreshCache()
        {
            _isCacheInitialized = false;
            InitializeCache();
        }
    }
}