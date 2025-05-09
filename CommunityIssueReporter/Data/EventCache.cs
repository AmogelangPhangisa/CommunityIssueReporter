using System;
using System.Collections.Generic;
using System.Linq;
using CommunityIssueReporter.Models;

namespace CommunityIssueReporter.Data
{
    public static class EventCache
    {
        // Core data structures for optimized event access
        private static Dictionary<int, Event> _eventsById = new Dictionary<int, Event>();
        private static SortedDictionary<DateTime, List<Event>> _eventsByDate = new SortedDictionary<DateTime, List<Event>>();
        private static Dictionary<string, SortedSet<int>> _eventIdsByCategory = new Dictionary<string, SortedSet<int>>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<bool, HashSet<int>> _eventIdsByStatus = new Dictionary<bool, HashSet<int>>();

        // Track if cache is initialized
        private static bool _isInitialized = false;

        // Initialize cache from event repository
        public static void Initialize()
        {
            if (_isInitialized && !DatabaseManager.IsOfflineMode)
                return;

            Clear();

            // Get all events
            List<Event> allEvents = EventRepository.GetAllEvents(false);

            // Populate data structures
            foreach (var evt in allEvents)
            {
                AddEventToCache(evt);
            }

            _isInitialized = true;
        }

        // Clear cache
        public static void Clear()
        {
            _eventsById.Clear();
            _eventsByDate.Clear();
            _eventIdsByCategory.Clear();
            _eventIdsByStatus.Clear();

            // Initialize status collections
            _eventIdsByStatus[true] = new HashSet<int>();  // Active events
            _eventIdsByStatus[false] = new HashSet<int>(); // Inactive events

            _isInitialized = false;
        }

        // Add a single event to the cache
        public static void AddEventToCache(Event evt)
        {
            // Add to events by ID
            _eventsById[evt.EventID] = evt;

            // Add to events by date
            DateTime dateKey = evt.EventDate.Date; // Use just the date part as key
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

        // Remove a single event from the cache
        public static void RemoveEventFromCache(int eventId)
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

        // Update an event in the cache
        public static void UpdateEventInCache(Event updatedEvent)
        {
            if (_eventsById.ContainsKey(updatedEvent.EventID))
            {
                // Remove old event
                RemoveEventFromCache(updatedEvent.EventID);
            }

            // Add updated event
            AddEventToCache(updatedEvent);
        }

        // Get all events
        public static List<Event> GetAllEvents(bool activeOnly = true)
        {
            if (!_isInitialized)
                Initialize();

            if (activeOnly)
            {
                return _eventIdsByStatus[true].Select(id => _eventsById[id]).ToList();
            }
            else
            {
                return _eventsById.Values.ToList();
            }
        }

        // Get events by category
        public static List<Event> GetEventsByCategory(string category, bool activeOnly = true)
        {
            if (!_isInitialized)
                Initialize();

            if (string.IsNullOrEmpty(category) || !_eventIdsByCategory.ContainsKey(category))
                return new List<Event>();

            var eventIds = _eventIdsByCategory[category];

            if (activeOnly)
            {
                // Intersection of category events and active events
                return eventIds.Where(id => _eventIdsByStatus[true].Contains(id))
                               .Select(id => _eventsById[id])
                               .ToList();
            }
            else
            {
                return eventIds.Select(id => _eventsById[id]).ToList();
            }
        }

        // Get events by date range
        public static List<Event> GetEventsByDateRange(DateTime startDate, DateTime endDate, bool activeOnly = true)
        {
            if (!_isInitialized)
                Initialize();

            // Get all dates in range
            var result = new List<Event>();

            // Convert to date only
            DateTime start = startDate.Date;
            DateTime end = endDate.Date;

            // Find all events with start date in range
            foreach (var kvp in _eventsByDate.Where(kvp => kvp.Key >= start && kvp.Key <= end))
            {
                foreach (var evt in kvp.Value)
                {
                    // Skip inactive events if needed
                    if (activeOnly && !evt.IsActive)
                        continue;

                    result.Add(evt);
                }
            }

            // Also find events that span into the range
            foreach (var kvp in _eventsByDate.Where(kvp => kvp.Key < start))
            {
                foreach (var evt in kvp.Value)
                {
                    // Only include events that end after start date
                    if (evt.EndDate.HasValue && evt.EndDate.Value.Date >= start)
                    {
                        // Skip inactive events if needed
                        if (activeOnly && !evt.IsActive)
                            continue;

                        result.Add(evt);
                    }
                }
            }

            return result.Distinct().ToList();
        }

        // Search events with multiple criteria
        public static List<Event> SearchEvents(string category = null, DateTime? startDate = null,
                                            DateTime? endDate = null, bool includeInactive = false)
        {
            if (!_isInitialized)
                Initialize();

            // Start with all events
            IEnumerable<Event> results = _eventsById.Values;

            // Apply category filter if specified
            if (!string.IsNullOrEmpty(category) && category != "All Categories" && _eventIdsByCategory.ContainsKey(category))
            {
                var categoryEventIds = _eventIdsByCategory[category];
                results = results.Where(e => categoryEventIds.Contains(e.EventID));
            }

            // Apply date range filter if specified
            if (startDate.HasValue && endDate.HasValue)
            {
                // Include events that start within range or span into range
                results = results.Where(e =>
                    (e.EventDate >= startDate.Value && e.EventDate <= endDate.Value) || // Starts in range
                    (e.EventDate < startDate.Value && e.EndDate.HasValue && e.EndDate.Value >= startDate.Value) // Spans into range
                );
            }
            else if (startDate.HasValue)
            {
                results = results.Where(e => e.EventDate >= startDate.Value);
            }
            else if (endDate.HasValue)
            {
                results = results.Where(e => e.EventDate <= endDate.Value);
            }

            // Apply active status filter
            if (!includeInactive)
            {
                results = results.Where(e => e.IsActive);
            }

            // Return sorted by date
            return results.OrderBy(e => e.EventDate).ToList();
        }

        // Get all available categories
        public static List<string> GetAllCategories()
        {
            if (!_isInitialized)
                Initialize();

            return _eventIdsByCategory.Keys.OrderBy(c => c).ToList();
        }

        // Get upcoming events (optimized)
        public static List<Event> GetUpcomingEvents(int maxEvents = 10)
        {
            if (!_isInitialized)
                Initialize();

            DateTime today = DateTime.Now.Date;

            return _eventsByDate
                .Where(kvp => kvp.Key >= today)
                .SelectMany(kvp => kvp.Value)
                .Where(e => e.IsActive)
                .OrderBy(e => e.EventDate)
                .Take(maxEvents)
                .ToList();
        }

        // Get event by ID (optimized)
        public static Event GetEventById(int eventId)
        {
            if (!_isInitialized)
                Initialize();

            return _eventsById.TryGetValue(eventId, out Event evt) ? evt : null;
        }
    }
}