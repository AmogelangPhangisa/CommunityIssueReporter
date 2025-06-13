using System;
using System.Collections.Generic;
using System.Linq;
using CommunityIssueReporter.Models;

namespace CommunityIssueReporter.DataStructures
{
    /// <summary>
    /// Priority Queue implementation using Min-Heap for urgent service request processing
    /// Part 3 Requirement: Heaps
    /// </summary>
    public class ServiceRequestHeap
    {
        private List<ServiceRequest> heap;
        private readonly Dictionary<string, int> statusPriority;
        private readonly Dictionary<string, int> serviceTypePriority;

        public int Count => heap.Count;
        public bool IsEmpty => heap.Count == 0;

        public ServiceRequestHeap()
        {
            heap = new List<ServiceRequest>();

            // Define priority values (lower number = higher priority)
            statusPriority = new Dictionary<string, int>
            {
                { "Critical", 0 },
                { "Urgent", 1 },
                { "High", 2 },
                { "In Process", 3 },
                { "Pending", 4 },
                { "Low", 5 },
                { "Completed", 6 },
                { "Cancelled", 7 }
            };

            serviceTypePriority = new Dictionary<string, int>
            {
                { "Emergency Response", 0 },
                { "Utilities", 1 },
                { "Public Safety", 2 },
                { "Sanitation", 3 },
                { "Roads", 4 },
                { "Parks and Recreation", 5 },
                { "General", 6 }
            };
        }

        /// <summary>
        /// Calculate priority score for a service request (lower = higher priority)
        /// </summary>
        private int GetPriority(ServiceRequest request)
        {
            if (request == null) return int.MaxValue;

            // Base priority from status
            int statusScore = (statusPriority.ContainsKey(request.Status) ? statusPriority[request.Status] : 4) * 1000;

            // Service type priority

            int serviceScore = (serviceTypePriority.ContainsKey(request.ServiceType) ? serviceTypePriority[request.ServiceType] : 6) * 100;

            // Age factor (older requests get higher priority)
            int daysSinceSubmission = (DateTime.Now - request.SubmissionDate).Days;
            int ageScore = Math.Max(0, 30 - daysSinceSubmission); // Inverse age (newer = higher number)

            return statusScore + serviceScore + ageScore;
        }

        /// <summary>
        /// Insert a service request into the priority queue - O(log n)
        /// </summary>
        public void Enqueue(ServiceRequest request)
        {
            if (request == null) return;

            heap.Add(request);
            HeapifyUp(heap.Count - 1);
        }

        /// <summary>
        /// Remove and return the highest priority request - O(log n)
        /// </summary>
        public ServiceRequest Dequeue()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Priority queue is empty");

            ServiceRequest result = heap[0];

            // Move last element to root and remove last
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);

            // Restore heap property
            if (heap.Count > 0)
                HeapifyDown(0);

            return result;
        }

        /// <summary>
        /// Peek at the highest priority request without removing it - O(1)
        /// </summary>
        public ServiceRequest Peek()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Priority queue is empty");

            return heap[0];
        }

        /// <summary>
        /// Restore heap property upward from given index
        /// </summary>
        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;

                // If current element has higher priority than parent, stop
                if (GetPriority(heap[index]) >= GetPriority(heap[parentIndex]))
                    break;

                // Swap with parent and continue
                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        /// <summary>
        /// Restore heap property downward from given index
        /// </summary>
        private void HeapifyDown(int index)
        {
            while (true)
            {
                int highest = index;
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;

                // Check if left child has higher priority
                if (leftChild < heap.Count &&
                    GetPriority(heap[leftChild]) < GetPriority(heap[highest]))
                {
                    highest = leftChild;
                }

                // Check if right child has higher priority
                if (rightChild < heap.Count &&
                    GetPriority(heap[rightChild]) < GetPriority(heap[highest]))
                {
                    highest = rightChild;
                }

                // If no child has higher priority, heap property is satisfied
                if (highest == index)
                    break;

                // Swap with child and continue
                Swap(index, highest);
                index = highest;
            }
        }

        /// <summary>
        /// Swap two elements in the heap
        /// </summary>
        private void Swap(int i, int j)
        {
            ServiceRequest temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }

        /// <summary>
        /// Update priority of a specific request (useful when status changes)
        /// </summary>
        public bool UpdatePriority(int requestID)
        {
            for (int i = 0; i < heap.Count; i++)
            {
                if (heap[i].RequestID == requestID)
                {
                    // Try heapifying both up and down
                    HeapifyUp(i);
                    HeapifyDown(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Remove a specific request from the heap
        /// </summary>
        public bool Remove(int requestID)
        {
            for (int i = 0; i < heap.Count; i++)
            {
                if (heap[i].RequestID == requestID)
                {
                    // Replace with last element
                    heap[i] = heap[heap.Count - 1];
                    heap.RemoveAt(heap.Count - 1);

                    // Restore heap property if not empty
                    if (i < heap.Count)
                    {
                        HeapifyUp(i);
                        HeapifyDown(i);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get all requests in priority order (without removing them)
        /// </summary>
        public List<ServiceRequest> GetAllByPriority()
        {
            if (IsEmpty) return new List<ServiceRequest>();

            // Create a copy and repeatedly dequeue
            ServiceRequestHeap tempHeap = new ServiceRequestHeap();
            foreach (var request in heap)
            {
                tempHeap.Enqueue(request);
            }

            List<ServiceRequest> result = new List<ServiceRequest>();
            while (!tempHeap.IsEmpty)
            {
                result.Add(tempHeap.Dequeue());
            }

            return result;
        }

        /// <summary>
        /// Get top N highest priority requests
        /// </summary>
        public List<ServiceRequest> GetTopPriority(int n)
        {
            if (n <= 0) return new List<ServiceRequest>();

            List<ServiceRequest> result = new List<ServiceRequest>();
            ServiceRequestHeap tempHeap = new ServiceRequestHeap();

            // Copy heap for non-destructive operation
            foreach (var request in heap)
            {
                tempHeap.Enqueue(request);
            }

            // Get top N elements
            int count = Math.Min(n, tempHeap.Count);
            for (int i = 0; i < count; i++)
            {
                result.Add(tempHeap.Dequeue());
            }

            return result;
        }

        /// <summary>
        /// Get requests by status in priority order
        /// </summary>
        public List<ServiceRequest> GetByStatus(string status)
        {
            return heap.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                      .OrderBy(GetPriority)
                      .ToList();
        }

        /// <summary>
        /// Get requests by service type in priority order
        /// </summary>
        public List<ServiceRequest> GetByServiceType(string serviceType)
        {
            return heap.Where(r => r.ServiceType.Equals(serviceType, StringComparison.OrdinalIgnoreCase))
                      .OrderBy(GetPriority)
                      .ToList();
        }

        /// <summary>
        /// Get urgent requests (Critical, Urgent, High priority)
        /// </summary>
        public List<ServiceRequest> GetUrgentRequests()
        {
            return heap.Where(r => r.Status == "Critical" ||
                                  r.Status == "Urgent" ||
                                  r.Status == "High")
                      .OrderBy(GetPriority)
                      .ToList();
        }

        /// <summary>
        /// Get overdue requests (pending for more than X days)
        /// </summary>
        public List<ServiceRequest> GetOverdueRequests(int maxDays = 7)
        {
            DateTime cutoffDate = DateTime.Now.AddDays(-maxDays);
            return heap.Where(r => r.SubmissionDate <= cutoffDate &&
                                  r.Status != "Completed" &&
                                  r.Status != "Cancelled")
                      .OrderBy(GetPriority)
                      .ToList();
        }

        /// <summary>
        /// Build heap from existing collection - O(n)
        /// </summary>
        public void BuildHeap(IEnumerable<ServiceRequest> requests)
        {
            heap.Clear();
            heap.AddRange(requests.Where(r => r != null));

            // Heapify from last parent down to root
            for (int i = (heap.Count / 2) - 1; i >= 0; i--)
            {
                HeapifyDown(i);
            }
        }

        /// <summary>
        /// Clear all requests from the heap
        /// </summary>
        public void Clear()
        {
            heap.Clear();
        }

        /// <summary>
        /// Check if heap contains a specific request
        /// </summary>
        public bool Contains(int requestID)
        {
            return heap.Any(r => r.RequestID == requestID);
        }

        /// <summary>
        /// Get heap as array (for debugging/visualization)
        /// </summary>
        public ServiceRequest[] ToArray()
        {
            return heap.ToArray();
        }

        /// <summary>
        /// Validate heap property (for testing)
        /// </summary>
        public bool IsValidHeap()
        {
            for (int i = 0; i < heap.Count; i++)
            {
                int leftChild = 2 * i + 1;
                int rightChild = 2 * i + 2;

                // Check left child
                if (leftChild < heap.Count &&
                    GetPriority(heap[i]) > GetPriority(heap[leftChild]))
                {
                    return false;
                }

                // Check right child
                if (rightChild < heap.Count &&
                    GetPriority(heap[i]) > GetPriority(heap[rightChild]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get statistics about the heap
        /// </summary>
        public HeapStatistics GetStatistics()
        {
            if (IsEmpty)
            {
                return new HeapStatistics();
            }

            var statusCounts = heap.GroupBy(r => r.Status)
                                  .ToDictionary(g => g.Key, g => g.Count());

            var serviceTypeCounts = heap.GroupBy(r => r.ServiceType)
                                       .ToDictionary(g => g.Key, g => g.Count());

            return new HeapStatistics
            {
                TotalRequests = Count,
                UrgentRequests = GetUrgentRequests().Count,
                OverdueRequests = GetOverdueRequests().Count,
                StatusCounts = statusCounts,
                ServiceTypeCounts = serviceTypeCounts,
                OldestRequestDate = heap.Min(r => r.SubmissionDate),
                NewestRequestDate = heap.Max(r => r.SubmissionDate),
                AverageWaitTime = CalculateAverageWaitTime(),
                IsValidHeap = IsValidHeap()
            };
        }

        private double CalculateAverageWaitTime()
        {
            if (IsEmpty) return 0;

            var activerequests = heap.Where(r => r.Status != "Completed" && r.Status != "Cancelled");
            if (!activerequests.Any()) return 0;

            return activerequests.Average(r => (DateTime.Now - r.SubmissionDate).TotalDays);
        }

        /// <summary>
        /// Get priority score for external use (debugging)
        /// </summary>
        public int GetRequestPriority(ServiceRequest request)
        {
            return GetPriority(request);
        }
    }

    /// <summary>
    /// Statistics for heap performance and content analysis
    /// </summary>
    public class HeapStatistics
    {
        public int TotalRequests { get; set; }
        public int UrgentRequests { get; set; }
        public int OverdueRequests { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ServiceTypeCounts { get; set; } = new Dictionary<string, int>();
        public DateTime OldestRequestDate { get; set; }
        public DateTime NewestRequestDate { get; set; }
        public double AverageWaitTime { get; set; }
        public bool IsValidHeap { get; set; }

        public double UrgentPercentage => TotalRequests > 0 ? (double)UrgentRequests / TotalRequests * 100 : 0;
        public double OverduePercentage => TotalRequests > 0 ? (double)OverdueRequests / TotalRequests * 100 : 0;
        public TimeSpan RequestSpan => NewestRequestDate - OldestRequestDate;
    }
}