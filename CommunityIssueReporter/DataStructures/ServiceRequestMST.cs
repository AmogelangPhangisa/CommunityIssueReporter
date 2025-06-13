using System;
using System.Collections.Generic;
using System.Linq;
using CommunityIssueReporter.Models;

namespace CommunityIssueReporter.DataStructures
{
    /// <summary>
    /// Minimum Spanning Tree implementation for optimizing service routes and resource allocation
    /// Part 3 Requirement: Minimum Spanning Tree
    /// </summary>
    public class ServiceRequestMST
    {
        public class Edge : IComparable<Edge>
        {
            public int Source { get; set; }
            public int Destination { get; set; }
            public double Weight { get; set; }
            public string EdgeType { get; set; } // "Distance", "Time", "Cost", "Priority"

            public Edge(int source, int destination, double weight, string edgeType = "Distance")
            {
                Source = source;
                Destination = destination;
                Weight = weight;
                EdgeType = edgeType;
            }

            public int CompareTo(Edge other)
            {
                return Weight.CompareTo(other.Weight);
            }

            public override string ToString()
            {
                return $"{Source} -> {Destination} ({Weight:F2} {EdgeType})";
            }
        }

        public class Location
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Address { get; set; }

            public Location(double latitude, double longitude, string address = "")
            {
                Latitude = latitude;
                Longitude = longitude;
                Address = address;
            }

            /// <summary>
            /// Calculate distance using Haversine formula (in kilometers)
            /// </summary>
            public double DistanceTo(Location other)
            {
                const double R = 6371; // Earth's radius in kilometers

                double lat1Rad = Latitude * Math.PI / 180;
                double lat2Rad = other.Latitude * Math.PI / 180;
                double deltaLatRad = (other.Latitude - Latitude) * Math.PI / 180;
                double deltaLonRad = (other.Longitude - Longitude) * Math.PI / 180;

                double a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                          Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                          Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

                double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

                return R * c;
            }
        }

        private Dictionary<int, ServiceRequest> requests;
        private Dictionary<int, Location> locations;
        private List<Edge> edges;
        private List<Edge> mstEdges;

        public int RequestCount => requests.Count;
        public int EdgeCount => edges.Count;
        public double TotalMSTWeight { get; private set; }

        public ServiceRequestMST()
        {
            requests = new Dictionary<int, ServiceRequest>();
            locations = new Dictionary<int, Location>();
            edges = new List<Edge>();
            mstEdges = new List<Edge>();
            TotalMSTWeight = 0;
        }

        /// <summary>
        /// Add a service request with its location
        /// </summary>
        public void AddRequest(ServiceRequest request, Location location)
        {
            if (request == null || location == null) return;

            requests[request.RequestID] = request;
            locations[request.RequestID] = location;
        }

        /// <summary>
        /// Add a service request with coordinates
        /// </summary>
        public void AddRequest(ServiceRequest request, double latitude, double longitude, string address = "")
        {
            AddRequest(request, new Location(latitude, longitude, address));
        }

        /// <summary>
        /// Parse location from service request description or use default
        /// </summary>
        public void AddRequest(ServiceRequest request)
        {
            if (request == null) return;

            // Try to parse coordinates from location string or use random coordinates for demo
            Location location = ParseLocationFromDescription(request.Description) ??
                               GenerateRandomLocationInPretoria();

            AddRequest(request, location);
        }

        private Location ParseLocationFromDescription(string description)
        {
            // Simple parsing - in real implementation, use geocoding service
            // For demo, return null to use random locations
            return null;
        }

        private Location GenerateRandomLocationInPretoria()
        {
            // Pretoria approximate bounds
            Random rand = new Random();
            double lat = -25.7479 + (rand.NextDouble() * 0.1); // ±0.05 degrees
            double lon = 28.2293 + (rand.NextDouble() * 0.1);  // ±0.05 degrees
            return new Location(lat, lon, "Pretoria Area");
        }

        /// <summary>
        /// Calculate all pairwise distances and create edges
        /// </summary>
        public void CalculateDistanceEdges()
        {
            edges.Clear();
            var requestIDs = requests.Keys.ToList();

            for (int i = 0; i < requestIDs.Count; i++)
            {
                for (int j = i + 1; j < requestIDs.Count; j++)
                {
                    int id1 = requestIDs[i];
                    int id2 = requestIDs[j];

                    if (locations.ContainsKey(id1) && locations.ContainsKey(id2))
                    {
                        double distance = locations[id1].DistanceTo(locations[id2]);
                        edges.Add(new Edge(id1, id2, distance, "Distance"));
                    }
                }
            }
        }

        /// <summary>
        /// Calculate edges based on service time estimates
        /// </summary>
        public void CalculateTimeEdges()
        {
            edges.Clear();
            var requestIDs = requests.Keys.ToList();

            for (int i = 0; i < requestIDs.Count; i++)
            {
                for (int j = i + 1; j < requestIDs.Count; j++)
                {
                    int id1 = requestIDs[i];
                    int id2 = requestIDs[j];

                    if (locations.ContainsKey(id1) && locations.ContainsKey(id2))
                    {
                        double distance = locations[id1].DistanceTo(locations[id2]);
                        double travelTime = CalculateTravelTime(distance, requests[id1], requests[id2]);
                        edges.Add(new Edge(id1, id2, travelTime, "Time"));
                    }
                }
            }
        }

        private double CalculateTravelTime(double distance, ServiceRequest req1, ServiceRequest req2)
        {
            // Estimate travel time based on distance and service type
            double baseSpeed = 40; // km/h average city speed

            // Adjust speed based on service type urgency
            if (req1.Status == "Critical" || req2.Status == "Critical")
                baseSpeed = 60; // Emergency speed
            else if (req1.Status == "Urgent" || req2.Status == "Urgent")
                baseSpeed = 50; // Higher priority speed

            double travelTimeHours = distance / baseSpeed;
            double serviceTime = EstimateServiceTime(req1) + EstimateServiceTime(req2);

            return (travelTimeHours * 60) + serviceTime; // Return time in minutes
        }

        private double EstimateServiceTime(ServiceRequest request)
        {
            // Estimate service time in minutes based on service type  
            switch (request.ServiceType)
            {
                case "Emergency Response":
                    return 30;
                case "Utilities":
                    return 120;
                case "Public Safety":
                    return 60;
                case "Sanitation":
                    return 45;
                case "Roads":
                    return 180;
                case "Parks and Recreation":
                    return 90;
                default:
                    return 60;
            }
        }

        /// <summary>
        /// Calculate edges based on cost estimates
        /// </summary>
        public void CalculateCostEdges()
        {
            edges.Clear();
            var requestIDs = requests.Keys.ToList();

            for (int i = 0; i < requestIDs.Count; i++)
            {
                for (int j = i + 1; j < requestIDs.Count; j++)
                {
                    int id1 = requestIDs[i];
                    int id2 = requestIDs[j];

                    if (locations.ContainsKey(id1) && locations.ContainsKey(id2))
                    {
                        double distance = locations[id1].DistanceTo(locations[id2]);
                        double cost = CalculateRouteCost(distance, requests[id1], requests[id2]);
                        edges.Add(new Edge(id1, id2, cost, "Cost"));
                    }
                }
            }
        }

        private double CalculateRouteCost(double distance, ServiceRequest req1, ServiceRequest req2)
        {
            double fuelCostPerKm = 2.5; // Estimated fuel cost in currency units
            double laborCostPerHour = 150; // Labor cost per hour

            double fuelCost = distance * fuelCostPerKm;
            double travelTime = distance / 40; // hours
            double laborCost = travelTime * laborCostPerHour;

            // Add service-specific costs
            double serviceCost = EstimateServiceCost(req1) + EstimateServiceCost(req2);

            return fuelCost + laborCost + serviceCost;
        }

        private double EstimateServiceCost(ServiceRequest request)
        {
            switch (request.ServiceType)
            {
                case "Emergency Response":
                    return 500;
                case "Utilities":
                    return 800;
                case "Public Safety":
                    return 300;
                case "Sanitation":
                    return 200;
                case "Roads":
                    return 1200;
                case "Parks and Recreation":
                    return 400;
                default:
                    return 300;
            }
        }

        /// <summary>
        /// Kruskal's algorithm for finding Minimum Spanning Tree
        /// </summary>
        public List<Edge> KruskalMST()
        {
            if (requests.Count < 2) return new List<Edge>();

            // Sort edges by weight
            var sortedEdges = edges.OrderBy(e => e.Weight).ToList();

            // Initialize Union-Find data structure
            Dictionary<int, int> parent = new Dictionary<int, int>();
            Dictionary<int, int> rank = new Dictionary<int, int>();

            foreach (int requestID in requests.Keys)
            {
                parent[requestID] = requestID;
                rank[requestID] = 0;
            }

            mstEdges = new List<Edge>();
            TotalMSTWeight = 0;

            foreach (Edge edge in sortedEdges)
            {
                int rootSource = Find(parent, edge.Source);
                int rootDest = Find(parent, edge.Destination);

                // If including this edge doesn't cause cycle, include it
                if (rootSource != rootDest)
                {
                    mstEdges.Add(edge);
                    TotalMSTWeight += edge.Weight;
                    Union(parent, rank, rootSource, rootDest);

                    // MST complete when we have n-1 edges
                    if (mstEdges.Count == requests.Count - 1)
                        break;
                }
            }

            return mstEdges;
        }

        /// <summary>
        /// Prim's algorithm for finding Minimum Spanning Tree
        /// </summary>
        public List<Edge> PrimMST()
        {
            if (requests.Count < 2) return new List<Edge>();

            var result = new List<Edge>();
            var visited = new HashSet<int>();
            var minHeap = new SortedSet<Edge>(Comparer<Edge>.Create((e1, e2) =>
            {
                int cmp = e1.Weight.CompareTo(e2.Weight);
                return cmp != 0 ? cmp : e1.Source.CompareTo(e2.Source) != 0 ?
                       e1.Source.CompareTo(e2.Source) : e1.Destination.CompareTo(e2.Destination);
            }));

            // Start with first request
            int startVertex = requests.Keys.First();
            visited.Add(startVertex);

            // Add all edges from start vertex
            foreach (var edge in edges.Where(e => e.Source == startVertex || e.Destination == startVertex))
            {
                minHeap.Add(edge);
            }

            TotalMSTWeight = 0;

            while (minHeap.Count > 0 && visited.Count < requests.Count)
            {
                Edge minEdge = minHeap.Min;
                minHeap.Remove(minEdge);

                int newVertex = -1;
                if (visited.Contains(minEdge.Source) && !visited.Contains(minEdge.Destination))
                {
                    newVertex = minEdge.Destination;
                }
                else if (visited.Contains(minEdge.Destination) && !visited.Contains(minEdge.Source))
                {
                    newVertex = minEdge.Source;
                }

                if (newVertex != -1)
                {
                    visited.Add(newVertex);
                    result.Add(minEdge);
                    TotalMSTWeight += minEdge.Weight;

                    // Add new edges from the newly added vertex
                    foreach (var edge in edges.Where(e =>
                        (e.Source == newVertex || e.Destination == newVertex) &&
                        !(visited.Contains(e.Source) && visited.Contains(e.Destination))))
                    {
                        minHeap.Add(edge);
                    }
                }
            }

            mstEdges = result;
            return result;
        }

        private int Find(Dictionary<int, int> parent, int vertex)
        {
            if (parent[vertex] != vertex)
                parent[vertex] = Find(parent, parent[vertex]); // Path compression
            return parent[vertex];
        }

        private void Union(Dictionary<int, int> parent, Dictionary<int, int> rank, int x, int y)
        {
            // Union by rank
            if (rank[x] < rank[y])
                parent[x] = y;
            else if (rank[x] > rank[y])
                parent[y] = x;
            else
            {
                parent[y] = x;
                rank[x]++;
            }
        }

        /// <summary>
        /// Get optimal service route using MST
        /// </summary>
        public List<ServiceRequest> GetOptimalRoute()
        {
            if (mstEdges.Count == 0)
                KruskalMST();

            if (mstEdges.Count == 0)
                return requests.Values.ToList();

            // Convert MST to route using DFS traversal
            var adjacencyList = new Dictionary<int, List<int>>();
            foreach (var edge in mstEdges)
            {
                if (!adjacencyList.ContainsKey(edge.Source))
                    adjacencyList[edge.Source] = new List<int>();
                if (!adjacencyList.ContainsKey(edge.Destination))
                    adjacencyList[edge.Destination] = new List<int>();

                adjacencyList[edge.Source].Add(edge.Destination);
                adjacencyList[edge.Destination].Add(edge.Source);
            }

            // Start from highest priority request
            int startVertex = GetHighestPriorityRequest();

            var route = new List<ServiceRequest>();
            var visited = new HashSet<int>();

            DFSRoute(startVertex, adjacencyList, visited, route);

            return route;
        }

        private int GetHighestPriorityRequest()
        {
            return requests.Values
                .OrderBy(r => r.Status == "Critical" ? 0 :
                             r.Status == "Urgent" ? 1 :
                             r.Status == "High" ? 2 : 3)
                .ThenBy(r => r.SubmissionDate)
                .First().RequestID;
        }

        private void DFSRoute(int vertex, Dictionary<int, List<int>> adjacencyList,
                             HashSet<int> visited, List<ServiceRequest> route)
        {
            visited.Add(vertex);
            route.Add(requests[vertex]);

            if (adjacencyList.ContainsKey(vertex))
            {
                foreach (int neighbor in adjacencyList[vertex])
                {
                    if (!visited.Contains(neighbor))
                        DFSRoute(neighbor, adjacencyList, visited, route);
                }
            }
        }

        /// <summary>
        /// Get clusters of nearby requests
        /// </summary>
        public List<List<ServiceRequest>> GetServiceClusters(double maxClusterDistance = 5.0)
        {
            var clusters = new List<List<ServiceRequest>>();
            var visited = new HashSet<int>();

            foreach (var request in requests.Values)
            {
                if (!visited.Contains(request.RequestID))
                {
                    var cluster = new List<ServiceRequest>();
                    BuildCluster(request.RequestID, maxClusterDistance, visited, cluster);
                    if (cluster.Count > 0)
                        clusters.Add(cluster);
                }
            }

            return clusters;
        }

        private void BuildCluster(int requestID, double maxDistance, HashSet<int> visited, List<ServiceRequest> cluster)
        {
            visited.Add(requestID);
            cluster.Add(requests[requestID]);

            foreach (var otherID in requests.Keys)
            {
                if (!visited.Contains(otherID))
                {
                    double distance = locations[requestID].DistanceTo(locations[otherID]);
                    if (distance <= maxDistance)
                    {
                        BuildCluster(otherID, maxDistance, visited, cluster);
                    }
                }
            }
        }

        /// <summary>
        /// Calculate savings by using MST vs visiting all locations individually
        /// </summary>
        public MSTSavings CalculateSavings()
        {
            if (mstEdges.Count == 0)
                KruskalMST();

            // Calculate total distance if visiting each location from depot
            var depot = new Location(-25.7479, 28.2293, "Municipal Depot"); // Pretoria center
            double totalIndividualDistance = 0;

            foreach (var location in locations.Values)
            {
                totalIndividualDistance += depot.DistanceTo(location) * 2; // Round trip
            }

            // MST distance + connections to depot
            double mstDistance = TotalMSTWeight;
            if (locations.Count > 0)
            {
                var closestToDepot = locations.Values.OrderBy(l => depot.DistanceTo(l)).First();
                mstDistance += depot.DistanceTo(closestToDepot) * 2; // Round trip to closest point
            }

            return new MSTSavings
            {
                IndividualTripsDistance = totalIndividualDistance,
                MSTRouteDistance = mstDistance,
                DistanceSaved = totalIndividualDistance - mstDistance,
                PercentageSaved = totalIndividualDistance > 0 ?
                    (totalIndividualDistance - mstDistance) / totalIndividualDistance * 100 : 0,
                EstimatedTimeSaved = (totalIndividualDistance - mstDistance) / 40 * 60, // Minutes saved at 40 km/h
                EstimatedCostSaved = (totalIndividualDistance - mstDistance) * 2.5 // Cost saved at 2.5 per km
            };
        }

        /// <summary>
        /// Get statistics about the MST
        /// </summary>
        public MSTStatistics GetStatistics()
        {
            if (mstEdges.Count == 0)
                KruskalMST();

            var stats = new MSTStatistics
            {
                RequestCount = RequestCount,
                EdgeCount = EdgeCount,
                MSTEdgeCount = mstEdges.Count,
                TotalWeight = TotalMSTWeight,
                AverageEdgeWeight = mstEdges.Count > 0 ? mstEdges.Average(e => e.Weight) : 0,
                MinEdgeWeight = mstEdges.Count > 0 ? mstEdges.Min(e => e.Weight) : 0,
                MaxEdgeWeight = mstEdges.Count > 0 ? mstEdges.Max(e => e.Weight) : 0,
                IsConnected = mstEdges.Count == RequestCount - 1,
                Savings = CalculateSavings()
            };

            return stats;
        }

        /// <summary>
        /// Clear all data
        /// </summary>
        public void Clear()
        {
            requests.Clear();
            locations.Clear();
            edges.Clear();
            mstEdges.Clear();
            TotalMSTWeight = 0;
        }

        /// <summary>
        /// Export MST to DOT format for visualization
        /// </summary>
        public string ToMSTDotFormat()
        {
            var dot = new System.Text.StringBuilder();
            dot.AppendLine("graph MST {");
            dot.AppendLine("  node [shape=circle];");

            // Add nodes
            foreach (var request in requests.Values)
            {
                string nodeColor = ""; // Renamed variable to avoid conflict and unnecessary assignment
                switch (request.Status)
                {
                    case "Critical":
                        nodeColor = "red";
                        break;
                    case "Urgent":
                        nodeColor = "orange";
                        break;
                    case "High":
                        nodeColor = "yellow";
                        break;
                    case "In Process":
                        nodeColor = "lightblue";
                        break;
                    case "Completed":
                        nodeColor = "lightgreen";
                        break;
                    default:
                        nodeColor = "white";
                        break;
                }

                dot.AppendLine($"  {request.RequestID} [label=\"{request.RequestID}\\n{request.ServiceType}\", fillcolor={nodeColor}, style=filled];");
            }

            // Add MST edges
            foreach (var edge in mstEdges)
            {
                dot.AppendLine($"  {edge.Source} -- {edge.Destination} [label=\"{edge.Weight:F1}\", weight={edge.Weight}];");
            }

            dot.AppendLine("}");
            return dot.ToString();
        }
    }

    /// <summary>
    /// Savings calculation for MST optimization
    /// </summary>
    public class MSTSavings
    {
        public double IndividualTripsDistance { get; set; }
        public double MSTRouteDistance { get; set; }
        public double DistanceSaved { get; set; }
        public double PercentageSaved { get; set; }
        public double EstimatedTimeSaved { get; set; }
        public double EstimatedCostSaved { get; set; }
    }

    /// <summary>
    /// Statistics for MST analysis
    /// </summary>
    public class MSTStatistics
    {
        public int RequestCount { get; set; }
        public int EdgeCount { get; set; }
        public int MSTEdgeCount { get; set; }
        public double TotalWeight { get; set; }
        public double AverageEdgeWeight { get; set; }
        public double MinEdgeWeight { get; set; }
        public double MaxEdgeWeight { get; set; }
        public bool IsConnected { get; set; }
        public MSTSavings Savings { get; set; }

        public double Efficiency => EdgeCount > 0 ? (double)MSTEdgeCount / EdgeCount * 100 : 0;
    }
}