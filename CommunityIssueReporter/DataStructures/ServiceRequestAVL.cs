using System;
using System.Collections.Generic;
using CommunityIssueReporter.Models;

namespace CommunityIssueReporter.DataStructures
{
    /// <summary>
    /// AVL Tree implementation for self-balancing BST with guaranteed O(log n) operations
    /// Part 3 Requirement: AVL Trees
    /// </summary>
    public class ServiceRequestAVL
    {
        private class AVLNode
        {
            public int RequestID { get; set; }
            public ServiceRequest Request { get; set; }
            public AVLNode Left { get; set; }
            public AVLNode Right { get; set; }
            public int Height { get; set; }

            public AVLNode(ServiceRequest request)
            {
                RequestID = request.RequestID;
                Request = request;
                Height = 1;
                Left = null;
                Right = null;
            }
        }

        private AVLNode root;
        public int Count { get; private set; }

        public ServiceRequestAVL()
        {
            root = null;
            Count = 0;
        }

        /// <summary>
        /// Get height of node (0 for null nodes)
        /// </summary>
        private int GetHeight(AVLNode node)
        {
            return node?.Height ?? 0;
        }

        /// <summary>
        /// Calculate balance factor of node
        /// </summary>
        private int GetBalance(AVLNode node)
        {
            return node == null ? 0 : GetHeight(node.Left) - GetHeight(node.Right);
        }

        /// <summary>
        /// Update height of node based on children
        /// </summary>
        private void UpdateHeight(AVLNode node)
        {
            if (node != null)
            {
                node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
            }
        }

        /// <summary>
        /// Right rotation for balancing
        /// </summary>
        private AVLNode RotateRight(AVLNode y)
        {
            AVLNode x = y.Left;
            AVLNode T2 = x.Right;

            // Perform rotation
            x.Right = y;
            y.Left = T2;

            // Update heights
            UpdateHeight(y);
            UpdateHeight(x);

            return x; // New root
        }

        /// <summary>
        /// Left rotation for balancing
        /// </summary>
        private AVLNode RotateLeft(AVLNode x)
        {
            AVLNode y = x.Right;
            AVLNode T2 = y.Left;

            // Perform rotation
            y.Left = x;
            x.Right = T2;

            // Update heights
            UpdateHeight(x);
            UpdateHeight(y);

            return y; // New root
        }

        /// <summary>
        /// Insert a service request - guaranteed O(log n)
        /// </summary>
        public void Insert(ServiceRequest request)
        {
            if (request == null) return;

            root = InsertRec(root, request);
            Count++;
        }

        private AVLNode InsertRec(AVLNode node, ServiceRequest request)
        {
            // Step 1: Perform normal BST insertion
            if (node == null)
                return new AVLNode(request);

            if (request.RequestID < node.RequestID)
                node.Left = InsertRec(node.Left, request);
            else if (request.RequestID > node.RequestID)
                node.Right = InsertRec(node.Right, request);
            else
            {
                // Equal keys - update existing node
                node.Request = request;
                Count--; // Don't increment count for updates
                return node;
            }

            // Step 2: Update height of current node
            UpdateHeight(node);

            // Step 3: Get balance factor
            int balance = GetBalance(node);

            // Step 4: Perform rotations if unbalanced

            // Left Left Case
            if (balance > 1 && request.RequestID < node.Left.RequestID)
                return RotateRight(node);

            // Right Right Case
            if (balance < -1 && request.RequestID > node.Right.RequestID)
                return RotateLeft(node);

            // Left Right Case
            if (balance > 1 && request.RequestID > node.Left.RequestID)
            {
                node.Left = RotateLeft(node.Left);
                return RotateRight(node);
            }

            // Right Left Case
            if (balance < -1 && request.RequestID < node.Right.RequestID)
            {
                node.Right = RotateRight(node.Right);
                return RotateLeft(node);
            }

            // Return unchanged node
            return node;
        }

        /// <summary>
        /// Search for service request by ID - guaranteed O(log n)
        /// </summary>
        public ServiceRequest Search(int requestID)
        {
            return SearchRec(root, requestID)?.Request;
        }

        private AVLNode SearchRec(AVLNode node, int requestID)
        {
            if (node == null || node.RequestID == requestID)
                return node;

            if (requestID < node.RequestID)
                return SearchRec(node.Left, requestID);

            return SearchRec(node.Right, requestID);
        }

        /// <summary>
        /// Delete a service request - guaranteed O(log n)
        /// </summary>
        public bool Delete(int requestID)
        {
            int initialCount = Count;
            root = DeleteRec(root, requestID);
            return Count < initialCount;
        }

        private AVLNode DeleteRec(AVLNode root, int requestID)
        {
            // Step 1: Perform standard BST delete
            if (root == null)
                return root;

            if (requestID < root.RequestID)
                root.Left = DeleteRec(root.Left, requestID);
            else if (requestID > root.RequestID)
                root.Right = DeleteRec(root.Right, requestID);
            else
            {
                // Node to be deleted found
                Count--;

                // Node with only right child or no child
                if (root.Left == null)
                    return root.Right;

                // Node with only left child
                if (root.Right == null)
                    return root.Left;

                // Node with two children
                AVLNode successor = GetMinNode(root.Right);
                root.RequestID = successor.RequestID;
                root.Request = successor.Request;
                root.Right = DeleteRec(root.Right, successor.RequestID);
            }

            // Step 2: Update height of current node
            UpdateHeight(root);

            // Step 3: Get balance factor
            int balance = GetBalance(root);

            // Step 4: Perform rotations if unbalanced

            // Left Left Case
            if (balance > 1 && GetBalance(root.Left) >= 0)
                return RotateRight(root);

            // Left Right Case
            if (balance > 1 && GetBalance(root.Left) < 0)
            {
                root.Left = RotateLeft(root.Left);
                return RotateRight(root);
            }

            // Right Right Case
            if (balance < -1 && GetBalance(root.Right) <= 0)
                return RotateLeft(root);

            // Right Left Case
            if (balance < -1 && GetBalance(root.Right) > 0)
            {
                root.Right = RotateRight(root.Right);
                return RotateLeft(root);
            }

            return root;
        }

        private AVLNode GetMinNode(AVLNode node)
        {
            while (node.Left != null)
                node = node.Left;
            return node;
        }

        /// <summary>
        /// In-order traversal - returns sorted list
        /// </summary>
        public List<ServiceRequest> InOrderTraversal()
        {
            List<ServiceRequest> result = new List<ServiceRequest>();
            InOrderRec(root, result);
            return result;
        }

        private void InOrderRec(AVLNode root, List<ServiceRequest> result)
        {
            if (root != null)
            {
                InOrderRec(root.Left, result);
                result.Add(root.Request);
                InOrderRec(root.Right, result);
            }
        }

        /// <summary>
        /// Level-order traversal for tree visualization
        /// </summary>
        public List<ServiceRequest> LevelOrderTraversal()
        {
            List<ServiceRequest> result = new List<ServiceRequest>();
            if (root == null) return result;

            Queue<AVLNode> queue = new Queue<AVLNode>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                AVLNode current = queue.Dequeue();
                result.Add(current.Request);

                if (current.Left != null)
                    queue.Enqueue(current.Left);
                if (current.Right != null)
                    queue.Enqueue(current.Right);
            }

            return result;
        }

        /// <summary>
        /// Find minimum request
        /// </summary>
        public ServiceRequest FindMin()
        {
            if (root == null) return null;
            return GetMinNode(root).Request;
        }

        /// <summary>
        /// Find maximum request
        /// </summary>
        public ServiceRequest FindMax()
        {
            if (root == null) return null;

            AVLNode current = root;
            while (current.Right != null)
                current = current.Right;

            return current.Request;
        }

        /// <summary>
        /// Range search with guaranteed O(log n + k) where k is result size
        /// </summary>
        public List<ServiceRequest> RangeSearch(int minID, int maxID)
        {
            List<ServiceRequest> result = new List<ServiceRequest>();
            RangeSearchRec(root, minID, maxID, result);
            return result;
        }

        private void RangeSearchRec(AVLNode root, int minID, int maxID, List<ServiceRequest> result)
        {
            if (root == null) return;

            if (root.RequestID >= minID && root.RequestID <= maxID)
                result.Add(root.Request);

            if (root.RequestID > minID)
                RangeSearchRec(root.Left, minID, maxID, result);

            if (root.RequestID < maxID)
                RangeSearchRec(root.Right, minID, maxID, result);
        }

        /// <summary>
        /// Get tree height
        /// </summary>
        public int GetTreeHeight()
        {
            return GetHeight(root);
        }

        /// <summary>
        /// Check if tree is balanced (should always be true for AVL)
        /// </summary>
        public bool IsBalanced()
        {
            return IsBalancedRec(root);
        }

        private bool IsBalancedRec(AVLNode node)
        {
            if (node == null) return true;

            int balance = GetBalance(node);
            return Math.Abs(balance) <= 1 &&
                   IsBalancedRec(node.Left) &&
                   IsBalancedRec(node.Right);
        }

        /// <summary>
        /// Get k-th smallest element (1-indexed)
        /// </summary>
        public ServiceRequest GetKthSmallest(int k)
        {
            if (k <= 0 || k > Count) return null;

            int[] counter = { 0 };
            return GetKthSmallestRec(root, k, counter);
        }

        private ServiceRequest GetKthSmallestRec(AVLNode node, int k, int[] counter)
        {
            if (node == null) return null;

            ServiceRequest left = GetKthSmallestRec(node.Left, k, counter);
            if (left != null) return left;

            counter[0]++;
            if (counter[0] == k) return node.Request;

            return GetKthSmallestRec(node.Right, k, counter);
        }

        /// <summary>
        /// Get predecessor of a given request ID
        /// </summary>
        public ServiceRequest GetPredecessor(int requestID)
        {
            AVLNode predecessor = null;
            AVLNode current = root;

            while (current != null)
            {
                if (current.RequestID < requestID)
                {
                    predecessor = current;
                    current = current.Right;
                }
                else
                {
                    current = current.Left;
                }
            }

            return predecessor?.Request;
        }

        /// <summary>
        /// Get successor of a given request ID
        /// </summary>
        public ServiceRequest GetSuccessor(int requestID)
        {
            AVLNode successor = null;
            AVLNode current = root;

            while (current != null)
            {
                if (current.RequestID > requestID)
                {
                    successor = current;
                    current = current.Left;
                }
                else
                {
                    current = current.Right;
                }
            }

            return successor?.Request;
        }

        /// <summary>
        /// Clear the tree
        /// </summary>
        public void Clear()
        {
            root = null;
            Count = 0;
        }

        /// <summary>
        /// Check if tree is empty
        /// </summary>
        public bool IsEmpty()
        {
            return root == null;
        }

        /// <summary>
        /// Get detailed statistics about the AVL tree
        /// </summary>
        public AVLStatistics GetStatistics()
        {
            return new AVLStatistics
            {
                NodeCount = Count,
                Height = GetTreeHeight(),
                IsBalanced = IsBalanced(),
                MinRequestID = FindMin()?.RequestID ?? 0,
                MaxRequestID = FindMax()?.RequestID ?? 0,
                AverageDepth = CalculateAverageDepth()
            };
        }

        private double CalculateAverageDepth()
        {
            if (root == null) return 0;

            int totalDepth = CalculateTotalDepth(root, 0);
            return Count > 0 ? (double)totalDepth / Count : 0;
        }

        private int CalculateTotalDepth(AVLNode node, int depth)
        {
            if (node == null) return 0;

            return depth +
                   CalculateTotalDepth(node.Left, depth + 1) +
                   CalculateTotalDepth(node.Right, depth + 1);
        }
    }

    /// <summary>
    /// Statistics for AVL tree performance analysis
    /// </summary>
    public class AVLStatistics
    {
        public int NodeCount { get; set; }
        public int Height { get; set; }
        public bool IsBalanced { get; set; }
        public int MinRequestID { get; set; }
        public int MaxRequestID { get; set; }
        public double AverageDepth { get; set; }

        public double TheoreticalOptimalHeight => Math.Log(NodeCount + 1) / Math.Log(2);
        public double BalanceFactor => NodeCount > 0 ? Height / TheoreticalOptimalHeight : 0;
    }
}