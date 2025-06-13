using System;
using System.Collections.Generic;
using CommunityIssueReporter.Models;

namespace CommunityIssueReporter.DataStructures
{
    /// <summary>
    /// Binary Search Tree implementation for efficient O(log n) service request lookup by ID
    /// Part 3 Requirement: Binary Trees, Binary Search Trees
    /// </summary>
    public class ServiceRequestBST
    {
        private class BSTNode
        {
            public int RequestID { get; set; }
            public ServiceRequest Request { get; set; }
            public BSTNode Left { get; set; }
            public BSTNode Right { get; set; }

            public BSTNode(ServiceRequest request)
            {
                RequestID = request.RequestID;
                Request = request;
                Left = null;
                Right = null;
            }
        }

        private BSTNode root;
        public int Count { get; private set; }

        public ServiceRequestBST()
        {
            root = null;
            Count = 0;
        }

        /// <summary>
        /// Insert a service request into the BST - O(log n) average, O(n) worst case
        /// </summary>
        public void Insert(ServiceRequest request)
        {
            if (request == null) return;

            root = InsertRec(root, request);
            Count++;
        }

        private BSTNode InsertRec(BSTNode root, ServiceRequest request)
        {
            // Base case: empty tree
            if (root == null)
                return new BSTNode(request);

            // Recursive case: traverse left or right
            if (request.RequestID < root.RequestID)
                root.Left = InsertRec(root.Left, request);
            else if (request.RequestID > root.RequestID)
                root.Right = InsertRec(root.Right, request);
            // If RequestID already exists, update the request
            else
                root.Request = request;

            return root;
        }

        /// <summary>
        /// Search for a service request by ID - O(log n) average case
        /// </summary>
        public ServiceRequest Search(int requestID)
        {
            return SearchRec(root, requestID)?.Request;
        }

        private BSTNode SearchRec(BSTNode root, int requestID)
        {
            // Base case: empty tree or found
            if (root == null || root.RequestID == requestID)
                return root;

            // Recursive case: search left or right subtree
            if (requestID < root.RequestID)
                return SearchRec(root.Left, requestID);

            return SearchRec(root.Right, requestID);
        }

        /// <summary>
        /// In-order traversal returns requests sorted by ID - O(n)
        /// </summary>
        public List<ServiceRequest> InOrderTraversal()
        {
            List<ServiceRequest> result = new List<ServiceRequest>();
            InOrderRec(root, result);
            return result;
        }

        private void InOrderRec(BSTNode root, List<ServiceRequest> result)
        {
            if (root != null)
            {
                InOrderRec(root.Left, result);
                result.Add(root.Request);
                InOrderRec(root.Right, result);
            }
        }

        /// <summary>
        /// Pre-order traversal - useful for creating a copy of the tree
        /// </summary>
        public List<ServiceRequest> PreOrderTraversal()
        {
            List<ServiceRequest> result = new List<ServiceRequest>();
            PreOrderRec(root, result);
            return result;
        }

        private void PreOrderRec(BSTNode root, List<ServiceRequest> result)
        {
            if (root != null)
            {
                result.Add(root.Request);
                PreOrderRec(root.Left, result);
                PreOrderRec(root.Right, result);
            }
        }

        /// <summary>
        /// Post-order traversal - useful for deletion operations
        /// </summary>
        public List<ServiceRequest> PostOrderTraversal()
        {
            List<ServiceRequest> result = new List<ServiceRequest>();
            PostOrderRec(root, result);
            return result;
        }

        private void PostOrderRec(BSTNode root, List<ServiceRequest> result)
        {
            if (root != null)
            {
                PostOrderRec(root.Left, result);
                PostOrderRec(root.Right, result);
                result.Add(root.Request);
            }
        }

        /// <summary>
        /// Find minimum request ID in the tree
        /// </summary>
        public ServiceRequest FindMin()
        {
            if (root == null) return null;

            BSTNode current = root;
            while (current.Left != null)
                current = current.Left;

            return current.Request;
        }

        /// <summary>
        /// Find maximum request ID in the tree
        /// </summary>
        public ServiceRequest FindMax()
        {
            if (root == null) return null;

            BSTNode current = root;
            while (current.Right != null)
                current = current.Right;

            return current.Request;
        }

        /// <summary>
        /// Delete a service request by ID
        /// </summary>
        public bool Delete(int requestID)
        {
            int initialCount = Count;
            root = DeleteRec(root, requestID);
            return Count < initialCount;
        }

        private BSTNode DeleteRec(BSTNode root, int requestID)
        {
            if (root == null) return root;

            if (requestID < root.RequestID)
                root.Left = DeleteRec(root.Left, requestID);
            else if (requestID > root.RequestID)
                root.Right = DeleteRec(root.Right, requestID);
            else
            {
                // Node to be deleted found
                Count--;

                // Case 1: Node with only right child or no child
                if (root.Left == null)
                    return root.Right;

                // Case 2: Node with only left child
                if (root.Right == null)
                    return root.Left;

                // Case 3: Node with two children
                // Get inorder successor (smallest in right subtree)
                BSTNode successor = GetMinNode(root.Right);

                // Copy successor's content to this node
                root.RequestID = successor.RequestID;
                root.Request = successor.Request;

                // Delete the successor
                root.Right = DeleteRec(root.Right, successor.RequestID);
            }

            return root;
        }

        private BSTNode GetMinNode(BSTNode node)
        {
            while (node.Left != null)
                node = node.Left;
            return node;
        }

        /// <summary>
        /// Get tree height for performance analysis
        /// </summary>
        public int GetHeight()
        {
            return GetHeightRec(root);
        }

        private int GetHeightRec(BSTNode root)
        {
            if (root == null) return -1;

            return 1 + Math.Max(GetHeightRec(root.Left), GetHeightRec(root.Right));
        }

        /// <summary>
        /// Check if tree is empty
        /// </summary>
        public bool IsEmpty()
        {
            return root == null;
        }

        /// <summary>
        /// Clear all nodes from the tree
        /// </summary>
        public void Clear()
        {
            root = null;
            Count = 0;
        }

        /// <summary>
        /// Range search: find all requests with IDs between min and max (inclusive)
        /// </summary>
        public List<ServiceRequest> RangeSearch(int minID, int maxID)
        {
            List<ServiceRequest> result = new List<ServiceRequest>();
            RangeSearchRec(root, minID, maxID, result);
            return result;
        }

        private void RangeSearchRec(BSTNode root, int minID, int maxID, List<ServiceRequest> result)
        {
            if (root == null) return;

            // If current node is in range, add it
            if (root.RequestID >= minID && root.RequestID <= maxID)
                result.Add(root.Request);

            // Recursively search left subtree if needed
            if (root.RequestID > minID)
                RangeSearchRec(root.Left, minID, maxID, result);

            // Recursively search right subtree if needed
            if (root.RequestID < maxID)
                RangeSearchRec(root.Right, minID, maxID, result);
        }

        /// <summary>
        /// Get requests by status using in-order traversal
        /// </summary>
        public List<ServiceRequest> GetRequestsByStatus(string status)
        {
            List<ServiceRequest> allRequests = InOrderTraversal();
            List<ServiceRequest> filteredRequests = new List<ServiceRequest>();

            foreach (var request in allRequests)
            {
                if (request.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                    filteredRequests.Add(request);
            }

            return filteredRequests;
        }

        /// <summary>
        /// Get performance statistics for analysis
        /// </summary>
        public BSTStatistics GetStatistics()
        {
            return new BSTStatistics
            {
                NodeCount = Count,
                Height = GetHeight(),
                MinRequestID = FindMin()?.RequestID ?? 0,
                MaxRequestID = FindMax()?.RequestID ?? 0
            };
        }
    }

    /// <summary>
    /// Statistics class for BST performance analysis
    /// </summary>
    public class BSTStatistics
    {
        public int NodeCount { get; set; }
        public int Height { get; set; }
        public int MinRequestID { get; set; }
        public int MaxRequestID { get; set; }

        public double AverageDepth => NodeCount > 0 ? (double)Height / NodeCount : 0;
        public double TheoreticalOptimalHeight => Math.Log(NodeCount + 1) / Math.Log(2);
    }
}