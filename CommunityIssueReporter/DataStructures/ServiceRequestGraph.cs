using System;
using System.Collections.Generic;
using System.Linq;
using CommunityIssueReporter.Models;

namespace CommunityIssueReporter.DataStructures
{
    /// <summary>
    /// Graph implementation for service request dependencies and workflow management
    /// Part 3 Requirement: Graphs, Graph Traversal
    /// </summary>
    public class ServiceRequestGraph
    {
        private Dictionary<int, List<int>> adjacencyList;
        private Dictionary<int, ServiceRequest> requests;
        private Dictionary<int, List<int>> reverseAdjacencyList; // For finding dependents

        public int VertexCount => requests.Count;
        public int EdgeCount { get; private set; }

        public ServiceRequestGraph()
        {
            adjacencyList = new Dictionary<int, List<int>>();
            requests = new Dictionary<int, ServiceRequest>();
            reverseAdjacencyList = new Dictionary<int, List<int>>();
            EdgeCount = 0;
        }

        /// <summary>
        /// Add a service request as a vertex in the graph
        /// </summary>
        public void AddRequest(ServiceRequest request)
        {
            if (request == null) return;

            requests[request.RequestID] = request;

            if (!adjacencyList.ContainsKey(request.RequestID))
                adjacencyList[request.RequestID] = new List<int>();

            if (!reverseAdjacencyList.ContainsKey(request.RequestID))
                reverseAdjacencyList[request.RequestID] = new List<int>();
        }

        /// <summary>
        /// Add dependency: fromRequest must be completed before toRequest can start
        /// </summary>
        public void AddDependency(int fromRequestID, int toRequestID)
        {
            if (!requests.ContainsKey(fromRequestID) || !requests.ContainsKey(toRequestID))
                return;

            if (!adjacencyList[fromRequestID].Contains(toRequestID))
            {
                adjacencyList[fromRequestID].Add(toRequestID);
                reverseAdjacencyList[toRequestID].Add(fromRequestID);
                EdgeCount++;
            }
        }

        /// <summary>
        /// Remove dependency between two requests
        /// </summary>
        public bool RemoveDependency(int fromRequestID, int toRequestID)
        {
            if (!adjacencyList.ContainsKey(fromRequestID) ||
                !reverseAdjacencyList.ContainsKey(toRequestID))
                return false;

            bool removed = adjacencyList[fromRequestID].Remove(toRequestID);
            if (removed)
            {
                reverseAdjacencyList[toRequestID].Remove(fromRequestID);
                EdgeCount--;
            }
            return removed;
        }

        /// <summary>
        /// Depth-First Search traversal starting from a specific request
        /// </summary>
        public List<ServiceRequest> DFS(int startRequestID)
        {
            if (!requests.ContainsKey(startRequestID))
                return new List<ServiceRequest>();

            HashSet<int> visited = new HashSet<int>();
            List<ServiceRequest> result = new List<ServiceRequest>();

            DFSUtil(startRequestID, visited, result);
            return result;
        }

        private void DFSUtil(int requestID, HashSet<int> visited, List<ServiceRequest> result)
        {
            visited.Add(requestID);
            result.Add(requests[requestID]);

            if (adjacencyList.ContainsKey(requestID))
            {
                foreach (int neighbor in adjacencyList[requestID])
                {
                    if (!visited.Contains(neighbor))
                        DFSUtil(neighbor, visited, result);
                }
            }
        }

        /// <summary>
        /// Breadth-First Search traversal starting from a specific request
        /// </summary>
        public List<ServiceRequest> BFS(int startRequestID)
        {
            if (!requests.ContainsKey(startRequestID))
                return new List<ServiceRequest>();

            HashSet<int> visited = new HashSet<int>();
            List<ServiceRequest> result = new List<ServiceRequest>();
            Queue<int> queue = new Queue<int>();

            queue.Enqueue(startRequestID);
            visited.Add(startRequestID);

            while (queue.Count > 0)
            {
                int currentID = queue.Dequeue();
                result.Add(requests[currentID]);

                if (adjacencyList.ContainsKey(currentID))
                {
                    foreach (int neighbor in adjacencyList[currentID])
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Find all connected components in the graph
        /// </summary>
        public List<List<ServiceRequest>> FindConnectedComponents()
        {
            HashSet<int> visited = new HashSet<int>();
            List<List<ServiceRequest>> components = new List<List<ServiceRequest>>();

            foreach (int requestID in requests.Keys)
            {
                if (!visited.Contains(requestID))
                {
                    List<ServiceRequest> component = new List<ServiceRequest>();
                    DFSUtil(requestID, visited, component);
                    components.Add(component);
                }
            }

            return components;
        }

        /// <summary>
        /// Detect cycles in the dependency graph (circular dependencies)
        /// </summary>
        public bool HasCycles()
        {
            HashSet<int> visited = new HashSet<int>();
            HashSet<int> recursionStack = new HashSet<int>();

            foreach (int requestID in requests.Keys)
            {
                if (!visited.Contains(requestID))
                {
                    if (HasCycleUtil(requestID, visited, recursionStack))
                        return true;
                }
            }
            return false;
        }

        private bool HasCycleUtil(int requestID, HashSet<int> visited, HashSet<int> recursionStack)
        {
            visited.Add(requestID);
            recursionStack.Add(requestID);

            if (adjacencyList.ContainsKey(requestID))
            {
                foreach (int neighbor in adjacencyList[requestID])
                {
                    if (!visited.Contains(neighbor))
                    {
                        if (HasCycleUtil(neighbor, visited, recursionStack))
                            return true;
                    }
                    else if (recursionStack.Contains(neighbor))
                    {
                        return true; // Back edge found - cycle detected
                    }
                }
            }

            recursionStack.Remove(requestID);
            return false;
        }

        /// <summary>
        /// Find a cycle in the graph (returns the cycle if found)
        /// </summary>
        public List<ServiceRequest> FindCycle()
        {
            HashSet<int> visited = new HashSet<int>();
            HashSet<int> recursionStack = new HashSet<int>();
            Dictionary<int, int> parent = new Dictionary<int, int>();

            foreach (int requestID in requests.Keys)
            {
                if (!visited.Contains(requestID))
                {
                    List<int> cycle = FindCycleUtil(requestID, visited, recursionStack, parent);
                    if (cycle != null)
                    {
                        return cycle.Select(id => requests[id]).ToList();
                    }
                }
            }
            return null;
        }

        private List<int> FindCycleUtil(int requestID, HashSet<int> visited,
            HashSet<int> recursionStack, Dictionary<int, int> parent)
        {
            visited.Add(requestID);
            recursionStack.Add(requestID);

            if (adjacencyList.ContainsKey(requestID))
            {
                foreach (int neighbor in adjacencyList[requestID])
                {
                    if (!visited.Contains(neighbor))
                    {
                        parent[neighbor] = requestID;
                        List<int> cycle = FindCycleUtil(neighbor, visited, recursionStack, parent);
                        if (cycle != null) return cycle;
                    }
                    else if (recursionStack.Contains(neighbor))
                    {
                        // Cycle found, reconstruct it
                        List<int> cycle = new List<int>();
                        int current = requestID;

                        while (current != neighbor)
                        {
                            cycle.Add(current);
                            parent.TryGetValue(current, out current); // Replace GetValueOrDefault with TryGetValue
                            if (current == -1) break;
                        }
                        cycle.Add(neighbor);
                        cycle.Reverse();
                        return cycle;
                    }
                }
            }

            recursionStack.Remove(requestID);
            return null;
        }

        /// <summary>
        /// Topological sort for processing order (only works if no cycles)
        /// </summary>
        public List<ServiceRequest> TopologicalSort()
        {
            if (HasCycles())
                throw new InvalidOperationException("Cannot perform topological sort on graph with cycles");

            Stack<int> stack = new Stack<int>();
            HashSet<int> visited = new HashSet<int>();

            foreach (int requestID in requests.Keys)
            {
                if (!visited.Contains(requestID))
                    TopologicalSortUtil(requestID, visited, stack);
            }

            List<ServiceRequest> result = new List<ServiceRequest>();
            while (stack.Count > 0)
            {
                int requestID = stack.Pop();
                result.Add(requests[requestID]);
            }

            return result;
        }

        private void TopologicalSortUtil(int requestID, HashSet<int> visited, Stack<int> stack)
        {
            visited.Add(requestID);

            if (adjacencyList.ContainsKey(requestID))
            {
                foreach (int neighbor in adjacencyList[requestID])
                {
                    if (!visited.Contains(neighbor))
                        TopologicalSortUtil(neighbor, visited, stack);
                }
            }

            stack.Push(requestID);
        }

        /// <summary>
        /// Kahn's algorithm for topological sorting (alternative implementation)
        /// </summary>
        public List<ServiceRequest> KahnsTopologicalSort()
        {
            if (HasCycles())
                throw new InvalidOperationException("Cannot perform topological sort on graph with cycles");

            Dictionary<int, int> inDegree = new Dictionary<int, int>();
            Queue<int> queue = new Queue<int>();
            List<ServiceRequest> result = new List<ServiceRequest>();

            // Initialize in-degree count
            foreach (int requestID in requests.Keys)
            {
                inDegree[requestID] = reverseAdjacencyList[requestID].Count;
                if (inDegree[requestID] == 0)
                    queue.Enqueue(requestID);
            }

            // Process nodes with no incoming edges
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                result.Add(requests[current]);

                // Reduce in-degree of adjacent nodes
                foreach (int neighbor in adjacencyList[current])
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                        queue.Enqueue(neighbor);
                }
            }

            return result;
        }

        /// <summary>
        /// Find shortest path between two requests (unweighted)
        /// </summary>
        public List<ServiceRequest> ShortestPath(int startID, int endID)
        {
            if (!requests.ContainsKey(startID) || !requests.ContainsKey(endID))
                return null;

            if (startID == endID)
                return new List<ServiceRequest> { requests[startID] };

            Queue<int> queue = new Queue<int>();
            HashSet<int> visited = new HashSet<int>();
            Dictionary<int, int> parent = new Dictionary<int, int>();

            queue.Enqueue(startID);
            visited.Add(startID);
            parent[startID] = -1;

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();

                if (current == endID)
                {
                    // Reconstruct path
                    List<int> path = new List<int>();
                    int temp = endID;

                    while (temp != -1)
                    {
                        path.Add(temp);
                        temp = parent[temp];
                    }

                    path.Reverse();
                    return path.Select(id => requests[id]).ToList();
                }

                if (adjacencyList.ContainsKey(current))
                {
                    foreach (int neighbor in adjacencyList[current])
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            parent[neighbor] = current;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return null; // No path found
        }

        /// <summary>
        /// Get all dependencies for a request (what must be completed first)
        /// </summary>
        public List<ServiceRequest> GetDependencies(int requestID)
        {
            if (!reverseAdjacencyList.ContainsKey(requestID))
                return new List<ServiceRequest>();

            return reverseAdjacencyList[requestID]
                .Select(id => requests[id])
                .ToList();
        }

        /// <summary>
        /// Get all dependents for a request (what depends on this request)
        /// </summary>
        public List<ServiceRequest> GetDependents(int requestID)
        {
            if (!adjacencyList.ContainsKey(requestID))
                return new List<ServiceRequest>();

            return adjacencyList[requestID]
                .Select(id => requests[id])
                .ToList();
        }

        /// <summary>
        /// Get requests that have no dependencies (can be started immediately)
        /// </summary>
        public List<ServiceRequest> GetRootRequests()
        {
            return requests.Values
                .Where(r => !reverseAdjacencyList.ContainsKey(r.RequestID) ||
                           reverseAdjacencyList[r.RequestID].Count == 0)
                .ToList();
        }

        /// <summary>
        /// Get requests that have no dependents (terminal requests)
        /// </summary>
        public List<ServiceRequest> GetLeafRequests()
        {
            return requests.Values
                .Where(r => !adjacencyList.ContainsKey(r.RequestID) ||
                           adjacencyList[r.RequestID].Count == 0)
                .ToList();
        }

        /// <summary>
        /// Calculate the longest path from any root to any leaf (critical path)
        /// </summary>
        public List<ServiceRequest> GetCriticalPath()
        {
            if (HasCycles())
                throw new InvalidOperationException("Cannot calculate critical path in graph with cycles");

            List<ServiceRequest> longestPath = new List<ServiceRequest>();

            foreach (var root in GetRootRequests())
            {
                var path = GetLongestPathFrom(root.RequestID);
                if (path.Count > longestPath.Count)
                    longestPath = path;
            }

            return longestPath;
        }

        private List<ServiceRequest> GetLongestPathFrom(int startID)
        {
            Dictionary<int, int> distances = new Dictionary<int, int>();
            Dictionary<int, int> parent = new Dictionary<int, int>();

            // Initialize distances
            foreach (int id in requests.Keys)
            {
                distances[id] = int.MinValue;
                parent[id] = -1;
            }
            distances[startID] = 0;

            // Topological sort
            var sortedRequests = TopologicalSort();

            // Process in topological order
            foreach (var request in sortedRequests)
            {
                int currentID = request.RequestID;

                if (distances[currentID] != int.MinValue)
                {
                    if (adjacencyList.ContainsKey(currentID))
                    {
                        foreach (int neighbor in adjacencyList[currentID])
                        {
                            if (distances[currentID] + 1 > distances[neighbor])
                            {
                                distances[neighbor] = distances[currentID] + 1;
                                parent[neighbor] = currentID;
                            }
                        }
                    }
                }
            }

            // Find the node with maximum distance
            int maxDistance = distances.Values.Max();
            int endNode = distances.First(kvp => kvp.Value == maxDistance).Key;

            // Reconstruct path
            List<int> path = new List<int>();
            int current = endNode;

            while (current != -1)
            {
                path.Add(current);
                current = parent[current];
            }

            path.Reverse();
            return path.Select(id => requests[id]).ToList();
        }

        /// <summary>
        /// Clear the graph
        /// </summary>
        public void Clear()
        {
            adjacencyList.Clear();
            requests.Clear();
            reverseAdjacencyList.Clear();
            EdgeCount = 0;
        }

        /// <summary>
        /// Get graph statistics
        /// </summary>
        public GraphStatistics GetStatistics()
        {
            var components = FindConnectedComponents();

            return new GraphStatistics
            {
                VertexCount = VertexCount,
                EdgeCount = EdgeCount,
                HasCycles = HasCycles(),
                ConnectedComponents = components.Count,
                LargestComponentSize = components.Count > 0 ? components.Max(c => c.Count) : 0,
                RootRequests = GetRootRequests().Count,
                LeafRequests = GetLeafRequests().Count,
                AverageOutDegree = VertexCount > 0 ? (double)EdgeCount / VertexCount : 0,
                MaxOutDegree = adjacencyList.Values.Count > 0 ? adjacencyList.Values.Max(list => list.Count) : 0
            };
        }

        /// <summary>
        /// Export graph to DOT format for visualization
        /// </summary>
        public string ToDotFormat()
        {
            var dot = new System.Text.StringBuilder();
            dot.AppendLine("digraph ServiceRequests {");
            dot.AppendLine("  node [shape=box];");

            // Add nodes
            foreach (var request in requests.Values)
            {
                string label = $"{request.RequestID}: {request.ServiceType}";
                string color;

                switch (request.Status)
                {
                    case "Completed":
                        color = "lightgreen";
                        break;
                    case "In Process":
                        color = "lightyellow";
                        break;
                    case "Pending":
                        color = "lightblue";
                        break;
                    case "Cancelled":
                        color = "lightgray";
                        break;
                    default:
                        color = "white";
                        break;
                }

                dot.AppendLine($"  {request.RequestID} [label=\"{label}\", fillcolor=\"{color}\", style=filled];");
            }


            // Add edges
            foreach (var kvp in adjacencyList)
            {
                foreach (int target in kvp.Value)
                {
                    dot.AppendLine($"  {kvp.Key} -> {target};");
                }
            }

            dot.AppendLine("}");
            return dot.ToString();
        }
    }

    /// <summary>
    /// Statistics for graph analysis
    /// </summary>
    public class GraphStatistics
    {
        public int VertexCount { get; set; }
        public int EdgeCount { get; set; }
        public bool HasCycles { get; set; }
        public int ConnectedComponents { get; set; }
        public int LargestComponentSize { get; set; }
        public int RootRequests { get; set; }
        public int LeafRequests { get; set; }
        public double AverageOutDegree { get; set; }
        public int MaxOutDegree { get; set; }

        public bool IsConnected => ConnectedComponents == 1;
        public bool IsDAG => !HasCycles;
        public double Density => VertexCount > 1 ? (double)EdgeCount / (VertexCount * (VertexCount - 1)) : 0;
    }
}