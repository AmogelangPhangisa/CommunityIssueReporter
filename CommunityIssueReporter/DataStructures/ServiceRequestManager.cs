using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommunityIssueReporter.Data;
using CommunityIssueReporter.Models;

namespace CommunityIssueReporter.DataStructures
{
    /// <summary>
    /// Central manager that orchestrates all advanced data structures for optimal performance
    /// Demonstrates Part 3 requirements with performance comparisons
    /// </summary>
    public class ServiceRequestManager
    {
        // Core data structures
        private ServiceRequestBST binarySearchTree;
        private ServiceRequestAVL avlTree;
        private ServiceRequestHeap priorityQueue;
        private ServiceRequestGraph dependencyGraph;
        private ServiceRequestMST routeOptimizer;

        // Performance tracking
        private Dictionary<string, long> operationTimes;
        private Dictionary<string, int> operationCounts;

        // Cache for frequent operations
        private List<ServiceRequest> cachedRequests;
        private DateTime lastCacheUpdate;
        private readonly TimeSpan cacheValidDuration = TimeSpan.FromMinutes(5);

        public ServiceRequestManager()
        {
            InitializeDataStructures();
            operationTimes = new Dictionary<string, long>();
            operationCounts = new Dictionary<string, int>();
            lastCacheUpdate = DateTime.MinValue;
        }

        private void InitializeDataStructures()
        {
            binarySearchTree = new ServiceRequestBST();
            avlTree = new ServiceRequestAVL();
            priorityQueue = new ServiceRequestHeap();
            dependencyGraph = new ServiceRequestGraph();
            routeOptimizer = new ServiceRequestMST();
        }

        /// <summary>
        /// Load all service requests into data structures with performance measurement
        /// </summary>
        public PerformanceMetrics LoadAllRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            var metrics = new PerformanceMetrics();

            try
            {
                // Get all requests from repository
                var allRequests = ServiceRequestRepository.GetAllServiceRequests();
                metrics.TotalRequestsLoaded = allRequests.Count;

                if (allRequests.Count == 0)
                {
                    InitializeSampleData();
                    allRequests = ServiceRequestRepository.GetAllServiceRequests();
                    metrics.TotalRequestsLoaded = allRequests.Count;
                }

                // Clear existing data
                ClearAllStructures();

                // Load into BST
                var bstTime = MeasureOperation(() =>
                {
                    foreach (var request in allRequests)
                        binarySearchTree.Insert(request);
                });
                metrics.BSTLoadTime = bstTime;

                // Load into AVL Tree
                var avlTime = MeasureOperation(() =>
                {
                    foreach (var request in allRequests)
                        avlTree.Insert(request);
                });
                metrics.AVLLoadTime = avlTime;

                // Load into Priority Queue
                var heapTime = MeasureOperation(() =>
                {
                    priorityQueue.BuildHeap(allRequests);
                });
                metrics.HeapLoadTime = heapTime;

                // Load into Graph with sample dependencies
                var graphTime = MeasureOperation(() =>
                {
                    foreach (var request in allRequests)
                        dependencyGraph.AddRequest(request);

                    CreateSampleDependencies(allRequests);
                });
                metrics.GraphLoadTime = graphTime;

                // Load into MST
                var mstTime = MeasureOperation(() =>
                {
                    foreach (var request in allRequests)
                        routeOptimizer.AddRequest(request);

                    routeOptimizer.CalculateDistanceEdges();
                    routeOptimizer.KruskalMST();
                });
                metrics.MSTLoadTime = mstTime;

                // Update cache
                cachedRequests = allRequests;
                lastCacheUpdate = DateTime.Now;

                stopwatch.Stop();
                metrics.TotalLoadTime = stopwatch.ElapsedMilliseconds;

                return metrics;
            }
            catch (Exception ex)
            {
                metrics.ErrorMessage = ex.Message;
                return metrics;
            }
        }

        private void InitializeSampleData()
        {
            // Initialize sample data if repository is empty
            ServiceRequestRepository.InitializeOfflineServiceRequests();
        }

        private void CreateSampleDependencies(List<ServiceRequest> requests)
        {
            // Create logical dependencies between requests
            for (int i = 0; i < requests.Count - 1; i++)
            {
                var current = requests[i];
                var next = requests[i + 1];

                // Create dependencies based on service types
                if (ShouldCreateDependency(current, next))
                {
                    dependencyGraph.AddDependency(current.RequestID, next.RequestID);
                }
            }

            // Add some random dependencies for demonstration
            var random = new Random(42); // Fixed seed for consistency
            for (int i = 0; i < Math.Min(requests.Count / 3, 10); i++)
            {
                var req1 = requests[random.Next(requests.Count)];
                var req2 = requests[random.Next(requests.Count)];

                if (req1.RequestID != req2.RequestID)
                {
                    dependencyGraph.AddDependency(req1.RequestID, req2.RequestID);
                }
            }
        }

        private bool ShouldCreateDependency(ServiceRequest req1, ServiceRequest req2)
        {
            // Utilities should be completed before other services
            if (req1.ServiceType == "Utilities" && req2.ServiceType != "Emergency Response")
                return true;

            // Emergency Response has no dependencies
            if (req1.ServiceType == "Emergency Response")
                return false;

            // Roads should be done before Parks and Recreation
            if (req1.ServiceType == "Roads" && req2.ServiceType == "Parks and Recreation")
                return true;

            return false;
        }

        /// <summary>
        /// Search for a request by ID with performance comparison
        /// </summary>
        public SearchComparisonResult SearchByID(int requestID)
        {
            var result = new SearchComparisonResult { RequestID = requestID };

            // Linear search (O(n) - for comparison)
            result.LinearSearchTime = MeasureOperation(() =>
            {
                result.LinearSearchResult = GetCachedRequests()
                    .FirstOrDefault(r => r.RequestID == requestID);
            });

            // BST search (O(log n) average)
            result.BSTSearchTime = MeasureOperation(() =>
            {
                result.BSTSearchResult = binarySearchTree.Search(requestID);
            });

            // AVL search (O(log n) guaranteed)
            result.AVLSearchTime = MeasureOperation(() =>
            {
                result.AVLSearchResult = avlTree.Search(requestID);
            });

            result.Found = result.BSTSearchResult != null;
            return result;
        }

        /// <summary>
        /// Get priority queue operations results
        /// </summary>
        public PriorityQueueResult GetPriorityOperations()
        {
            var result = new PriorityQueueResult();

            result.ProcessingTime = MeasureOperation(() =>
            {
                result.TopPriorityRequests = priorityQueue.GetTopPriority(10);
                result.UrgentRequests = priorityQueue.GetUrgentRequests();
                result.OverdueRequests = priorityQueue.GetOverdueRequests();
                result.HeapStatistics = priorityQueue.GetStatistics();
            });

            return result;
        }

        /// <summary>
        /// Get graph traversal and analysis results
        /// </summary>
        public GraphAnalysisResult GetGraphAnalysis()
        {
            var result = new GraphAnalysisResult();

            if (dependencyGraph.VertexCount == 0)
                return result;

            result.ProcessingTime = MeasureOperation(() =>
            {
                // Get a random starting point for traversals
                var allRequests = GetCachedRequests();
                if (allRequests.Count > 0)
                {
                    int startID = allRequests.First().RequestID;

                    result.DFSTraversal = dependencyGraph.DFS(startID);
                    result.BFSTraversal = dependencyGraph.BFS(startID);
                }

                result.HasCycles = dependencyGraph.HasCycles();
                result.ConnectedComponents = dependencyGraph.FindConnectedComponents();

                if (!result.HasCycles)
                {
                    result.TopologicalOrder = dependencyGraph.TopologicalSort();
                    result.CriticalPath = dependencyGraph.GetCriticalPath();
                }

                result.RootRequests = dependencyGraph.GetRootRequests();
                result.LeafRequests = dependencyGraph.GetLeafRequests();
                result.GraphStatistics = dependencyGraph.GetStatistics();
            });

            return result;
        }

        /// <summary>
        /// Get MST optimization results
        /// </summary>
        public MSTOptimizationResult GetMSTOptimization()
        {
            var result = new MSTOptimizationResult();

            result.ProcessingTime = MeasureOperation(() =>
            {
                var kruskalTime = MeasureOperation(() =>
                {
                    result.KruskalMST = routeOptimizer.KruskalMST();
                });

                var primTime = MeasureOperation(() =>
                {
                    result.PrimMST = routeOptimizer.PrimMST();
                });

                result.KruskalTime = kruskalTime;
                result.PrimTime = primTime;
                result.OptimalRoute = routeOptimizer.GetOptimalRoute();
                result.ServiceClusters = routeOptimizer.GetServiceClusters();
                result.Statistics = routeOptimizer.GetStatistics();
            });

            return result;
        }

        /// <summary>
        /// Get comprehensive performance comparison
        /// </summary>
        public PerformanceComparison GetPerformanceComparison()
        {
            var comparison = new PerformanceComparison();
            var requests = GetCachedRequests();

            if (requests.Count == 0)
                return comparison;

            // Test multiple search operations
            var testIDs = requests.Take(10).Select(r => r.RequestID).ToList();

            // Linear search performance
            var linearTimes = new List<long>();
            foreach (int id in testIDs)
            {
                var time = MeasureOperation(() =>
                {
                    requests.FirstOrDefault(r => r.RequestID == id);
                });
                linearTimes.Add(time);
            }

            // BST search performance
            var bstTimes = new List<long>();
            foreach (int id in testIDs)
            {
                var time = MeasureOperation(() =>
                {
                    binarySearchTree.Search(id);
                });
                bstTimes.Add(time);
            }

            // AVL search performance
            var avlTimes = new List<long>();
            foreach (int id in testIDs)
            {
                var time = MeasureOperation(() =>
                {
                    avlTree.Search(id);
                });
                avlTimes.Add(time);
            }

            comparison.LinearSearchAverage = linearTimes.Average();
            comparison.BSTSearchAverage = bstTimes.Average();
            comparison.AVLSearchAverage = avlTimes.Average();

            // Calculate efficiency gains
            comparison.BSTEfficiencyGain = comparison.LinearSearchAverage / comparison.BSTSearchAverage;
            comparison.AVLEfficiencyGain = comparison.LinearSearchAverage / comparison.AVLSearchAverage;

            // Memory usage estimation
            comparison.LinearMemoryUsage = requests.Count * 64; // Estimated bytes per request object
            comparison.BSTMemoryUsage = binarySearchTree.Count * 96; // Additional overhead for tree nodes
            comparison.AVLMemoryUsage = avlTree.Count * 104; // Additional overhead for AVL nodes
            comparison.HeapMemoryUsage = priorityQueue.Count * 72; // Heap array overhead

            return comparison;
        }

        /// <summary>
        /// Get requests filtered by status using different data structures
        /// </summary>
        public FilteredResults GetRequestsByStatus(string status)
        {
            var result = new FilteredResults { FilterCriteria = $"Status: {status}" };

            // Linear filtering
            result.LinearFilterTime = MeasureOperation(() =>
            {
                result.LinearResults = GetCachedRequests()
                    .Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            });

            // BST filtering (in-order traversal + filter)
            result.BSTFilterTime = MeasureOperation(() =>
            {
                result.BSTResults = binarySearchTree.GetRequestsByStatus(status);
            });

            // Heap filtering
            result.HeapFilterTime = MeasureOperation(() =>
            {
                result.HeapResults = priorityQueue.GetByStatus(status);
            });

            return result;
        }

        /// <summary>
        /// Get tree structure statistics
        /// </summary>
        public TreeStatisticsComparison GetTreeStatistics()
        {
            return new TreeStatisticsComparison
            {
                BSTStatistics = binarySearchTree.GetStatistics(),
                AVLStatistics = avlTree.GetStatistics(),
                BSTHeight = binarySearchTree.GetHeight(),
                AVLHeight = avlTree.GetTreeHeight(),
                BSTIsBalanced = binarySearchTree.GetStatistics().Height <= binarySearchTree.GetStatistics().TheoreticalOptimalHeight + 1,
                AVLIsBalanced = avlTree.IsBalanced() // Should always be true
            };
        }

        /// <summary>
        /// Demonstrate range queries on sorted data
        /// </summary>
        public RangeQueryResult GetRequestsInRange(int minID, int maxID)
        {
            var result = new RangeQueryResult { MinID = minID, MaxID = maxID };

            // Linear range search
            result.LinearTime = MeasureOperation(() =>
            {
                result.LinearResults = GetCachedRequests()
                    .Where(r => r.RequestID >= minID && r.RequestID <= maxID)
                    .ToList();
            });

            // BST range search
            result.BSTTime = MeasureOperation(() =>
            {
                result.BSTResults = binarySearchTree.RangeSearch(minID, maxID);
            });

            // AVL range search
            result.AVLTime = MeasureOperation(() =>
            {
                result.AVLResults = avlTree.RangeSearch(minID, maxID);
            });

            return result;
        }

        /// <summary>
        /// Clear all data structures
        /// </summary>
        public void ClearAllStructures()
        {
            binarySearchTree.Clear();
            avlTree.Clear();
            priorityQueue.Clear();
            dependencyGraph.Clear();
            routeOptimizer.Clear();
            cachedRequests = null;
            lastCacheUpdate = DateTime.MinValue;
        }

        /// <summary>
        /// Get cached requests (with automatic refresh)
        /// </summary>
        private List<ServiceRequest> GetCachedRequests()
        {
            if (cachedRequests == null || DateTime.Now - lastCacheUpdate > cacheValidDuration)
            {
                cachedRequests = ServiceRequestRepository.GetAllServiceRequests();
                lastCacheUpdate = DateTime.Now;
            }
            return cachedRequests ?? new List<ServiceRequest>();
        }

        /// <summary>
        /// Measure operation execution time in milliseconds
        /// </summary>
        private long MeasureOperation(Action operation)
        {
            var stopwatch = Stopwatch.StartNew();
            operation();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Add a new service request to all structures
        /// </summary>
        public void AddRequest(ServiceRequest request)
        {
            if (request == null) return;

            binarySearchTree.Insert(request);
            avlTree.Insert(request);
            priorityQueue.Enqueue(request);
            dependencyGraph.AddRequest(request);
            routeOptimizer.AddRequest(request);

            // Invalidate cache
            cachedRequests = null;
        }

        /// <summary>
        /// Update a service request in all structures
        /// </summary>
        public void UpdateRequest(ServiceRequest request)
        {
            if (request == null) return;

            // For trees, delete and re-insert
            binarySearchTree.Delete(request.RequestID);
            binarySearchTree.Insert(request);

            avlTree.Delete(request.RequestID);
            avlTree.Insert(request);

            // For heap, update priority
            priorityQueue.UpdatePriority(request.RequestID);

            // Graph and MST don't need updates for basic request changes

            // Invalidate cache
            cachedRequests = null;
        }

        /// <summary>
        /// Remove a service request from all structures
        /// </summary>
        public void RemoveRequest(int requestID)
        {
            binarySearchTree.Delete(requestID);
            avlTree.Delete(requestID);
            priorityQueue.Remove(requestID);
            // Note: Graph and MST removal would require more complex operations

            // Invalidate cache
            cachedRequests = null;
        }

        /// <summary>
        /// Export performance report
        /// </summary>
        public string GeneratePerformanceReport()
        {
            var report = new System.Text.StringBuilder();
            var comparison = GetPerformanceComparison();
            var treeStats = GetTreeStatistics();

            report.AppendLine("SERVICE REQUEST MANAGEMENT - PERFORMANCE REPORT");
            report.AppendLine("=" + new string('=', 50));
            report.AppendLine();

            report.AppendLine("SEARCH PERFORMANCE COMPARISON:");
            report.AppendLine($"Linear Search Average: {comparison.LinearSearchAverage:F2} ms");
            report.AppendLine($"BST Search Average: {comparison.BSTSearchAverage:F2} ms");
            report.AppendLine($"AVL Search Average: {comparison.AVLSearchAverage:F2} ms");
            report.AppendLine($"BST Efficiency Gain: {comparison.BSTEfficiencyGain:F1}x faster");
            report.AppendLine($"AVL Efficiency Gain: {comparison.AVLEfficiencyGain:F1}x faster");
            report.AppendLine();

            report.AppendLine("TREE STRUCTURE ANALYSIS:");
            report.AppendLine($"BST Height: {treeStats.BSTHeight}, Balanced: {treeStats.BSTIsBalanced}");
            report.AppendLine($"AVL Height: {treeStats.AVLHeight}, Balanced: {treeStats.AVLIsBalanced}");
            report.AppendLine();

            report.AppendLine("MEMORY USAGE ESTIMATION:");
            report.AppendLine($"Linear Storage: {comparison.LinearMemoryUsage:N0} bytes");
            report.AppendLine($"BST Storage: {comparison.BSTMemoryUsage:N0} bytes");
            report.AppendLine($"AVL Storage: {comparison.AVLMemoryUsage:N0} bytes");
            report.AppendLine($"Heap Storage: {comparison.HeapMemoryUsage:N0} bytes");
            report.AppendLine();

            var graphStats = dependencyGraph.GetStatistics();
            report.AppendLine("GRAPH ANALYSIS:");
            report.AppendLine($"Vertices: {graphStats.VertexCount}, Edges: {graphStats.EdgeCount}");
            report.AppendLine($"Connected Components: {graphStats.ConnectedComponents}");
            report.AppendLine($"Has Cycles: {graphStats.HasCycles}");
            report.AppendLine($"Is DAG: {graphStats.IsDAG}");
            report.AppendLine();

            var mstStats = routeOptimizer.GetStatistics();
            report.AppendLine("ROUTE OPTIMIZATION (MST):");
            report.AppendLine($"Total Requests: {mstStats.RequestCount}");
            report.AppendLine($"MST Total Weight: {mstStats.TotalWeight:F2}");
            report.AppendLine($"Efficiency: {mstStats.Efficiency:F1}%");
            if (mstStats.Savings != null)
            {
                report.AppendLine($"Distance Saved: {mstStats.Savings.DistanceSaved:F2} km");
                report.AppendLine($"Percentage Saved: {mstStats.Savings.PercentageSaved:F1}%");
            }

            return report.ToString();
        }
    }

    // Result classes for different operations
    public class PerformanceMetrics
    {
        public int TotalRequestsLoaded { get; set; }
        public long BSTLoadTime { get; set; }
        public long AVLLoadTime { get; set; }
        public long HeapLoadTime { get; set; }
        public long GraphLoadTime { get; set; }
        public long MSTLoadTime { get; set; }
        public long TotalLoadTime { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class SearchComparisonResult
    {
        public int RequestID { get; set; }
        public bool Found { get; set; }
        public long LinearSearchTime { get; set; }
        public long BSTSearchTime { get; set; }
        public long AVLSearchTime { get; set; }
        public ServiceRequest LinearSearchResult { get; set; }
        public ServiceRequest BSTSearchResult { get; set; }
        public ServiceRequest AVLSearchResult { get; set; }
    }

    public class PriorityQueueResult
    {
        public long ProcessingTime { get; set; }
        public List<ServiceRequest> TopPriorityRequests { get; set; }
        public List<ServiceRequest> UrgentRequests { get; set; }
        public List<ServiceRequest> OverdueRequests { get; set; }
        public HeapStatistics HeapStatistics { get; set; }
    }

    public class GraphAnalysisResult
    {
        public long ProcessingTime { get; set; }
        public List<ServiceRequest> DFSTraversal { get; set; }
        public List<ServiceRequest> BFSTraversal { get; set; }
        public bool HasCycles { get; set; }
        public List<List<ServiceRequest>> ConnectedComponents { get; set; }
        public List<ServiceRequest> TopologicalOrder { get; set; }
        public List<ServiceRequest> CriticalPath { get; set; }
        public List<ServiceRequest> RootRequests { get; set; }
        public List<ServiceRequest> LeafRequests { get; set; }
        public GraphStatistics GraphStatistics { get; set; }
    }

    public class MSTOptimizationResult
    {
        public long ProcessingTime { get; set; }
        public long KruskalTime { get; set; }
        public long PrimTime { get; set; }
        public List<ServiceRequestMST.Edge> KruskalMST { get; set; }
        public List<ServiceRequestMST.Edge> PrimMST { get; set; }
        public List<ServiceRequest> OptimalRoute { get; set; }
        public List<List<ServiceRequest>> ServiceClusters { get; set; }
        public MSTStatistics Statistics { get; set; }
    }

    public class PerformanceComparison
    {
        public double LinearSearchAverage { get; set; }
        public double BSTSearchAverage { get; set; }
        public double AVLSearchAverage { get; set; }
        public double BSTEfficiencyGain { get; set; }
        public double AVLEfficiencyGain { get; set; }
        public long LinearMemoryUsage { get; set; }
        public long BSTMemoryUsage { get; set; }
        public long AVLMemoryUsage { get; set; }
        public long HeapMemoryUsage { get; set; }
    }

    public class FilteredResults
    {
        public string FilterCriteria { get; set; }
        public long LinearFilterTime { get; set; }
        public long BSTFilterTime { get; set; }
        public long HeapFilterTime { get; set; }
        public List<ServiceRequest> LinearResults { get; set; }
        public List<ServiceRequest> BSTResults { get; set; }
        public List<ServiceRequest> HeapResults { get; set; }
    }

    public class TreeStatisticsComparison
    {
        public BSTStatistics BSTStatistics { get; set; }
        public AVLStatistics AVLStatistics { get; set; }
        public int BSTHeight { get; set; }
        public int AVLHeight { get; set; }
        public bool BSTIsBalanced { get; set; }
        public bool AVLIsBalanced { get; set; }
    }

    public class RangeQueryResult
    {
        public int MinID { get; set; }
        public int MaxID { get; set; }
        public long LinearTime { get; set; }
        public long BSTTime { get; set; }
        public long AVLTime { get; set; }
        public List<ServiceRequest> LinearResults { get; set; }
        public List<ServiceRequest> BSTResults { get; set; }
        public List<ServiceRequest> AVLResults { get; set; }
    }
}