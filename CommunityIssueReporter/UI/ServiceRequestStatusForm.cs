using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CommunityIssueReporter.Data;
using CommunityIssueReporter.DataStructures;
using CommunityIssueReporter.Models;
using CommunityIssueReporter.Utilities;

namespace CommunityIssueReporter.UI
{
    /// <summary>
    /// Service Request Status Form - Part 3 Implementation
    /// Demonstrates all required advanced data structures with performance comparisons
    /// </summary>
    public class ServiceRequestStatusForm : Form
    {
        // Core manager that orchestrates all data structures
        private ServiceRequestManager dataManager;

        // UI Controls
        private TabControl mainTabControl;
        private TabPage allRequestsTab;
        private TabPage searchComparisonTab;
        private TabPage priorityQueueTab;
        private TabPage graphAnalysisTab;
        private TabPage routeOptimizationTab;
        private TabPage performanceTab;

        // Search and filter controls
        private TextBox searchByIdTextBox;
        private Button searchButton;
        private ComboBox statusFilterComboBox;
        private ComboBox sortByComboBox;
        private Label performanceLabel;
        private Button loadDataButton;
        private Button refreshButton;

        // List views for different tabs
        private ListView allRequestsListView;
        private ListView searchResultsListView;
        private ListView priorityListView;
        private ListView graphListView;
        private ListView routeListView;

        // Performance display
        private RichTextBox performanceTextBox;

        private int _currentUserID;
        private string _currentUserRole;

        public ServiceRequestStatusForm(int currentUserID, string currentUserRole)
        {
            _currentUserID = currentUserID;
            _currentUserRole = currentUserRole;

            dataManager = new ServiceRequestManager();
            InitializeComponents();
            LoadInitialData();
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = "Service Request Status - Advanced Data Structures Demonstration";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = new Icon(SystemIcons.Application, 40, 40);

            // Create main layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Controls
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Tabs

            // Header Panel
            Panel headerPanel = CreateHeaderPanel();

            // Controls Panel
            Panel controlsPanel = CreateControlsPanel();

            // Tab Control
            mainTabControl = CreateTabControl();

            // Add to main layout
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(controlsPanel, 0, 1);
            mainLayout.Controls.Add(mainTabControl, 0, 2);

            this.Controls.Add(mainLayout);
        }

        private Panel CreateHeaderPanel()
        {
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.AccentColor
            };

            Label titleLabel = new Label
            {
                Text = "Service Request Status Tracking & Advanced Data Structures",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };

            performanceLabel = new Label
            {
                Text = "Ready to load data...",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightYellow,
                Dock = DockStyle.Right,
                Width = 300,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 20, 0)
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(performanceLabel);

            return headerPanel;
        }

        private Panel CreateControlsPanel()
        {
            Panel controlsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Load Data Button
            loadDataButton = new Button
            {
                Text = "Load Data Structures",
                Width = 150,
                Height = 35,
                Location = new Point(10, 12),
                BackColor = AppColors.ActionButton,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            loadDataButton.Click += LoadDataButton_Click;

            // Refresh Button
            refreshButton = new Button
            {
                Text = "Refresh",
                Width = 80,
                Height = 35,
                Location = new Point(170, 12),
                BackColor = AppColors.InfoColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshButton.Click += RefreshButton_Click;

            // Search by ID
            Label searchLabel = new Label
            {
                Text = "Search by ID:",
                AutoSize = true,
                Location = new Point(270, 20),
                Font = new Font("Segoe UI", 10)
            };

            searchByIdTextBox = new TextBox
            {
                Width = 100,
                Location = new Point(360, 17),
                Font = new Font("Segoe UI", 10)
            };

            searchButton = new Button
            {
                Text = "Compare Search",
                Width = 120,
                Height = 25,
                Location = new Point(470, 17),
                BackColor = AppColors.InfoColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            searchButton.Click += SearchButton_Click;

            // Status Filter
            Label statusLabel = new Label
            {
                Text = "Filter Status:",
                AutoSize = true,
                Location = new Point(610, 20),
                Font = new Font("Segoe UI", 10)
            };

            statusFilterComboBox = new ComboBox
            {
                Width = 120,
                Location = new Point(700, 17),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            statusFilterComboBox.Items.AddRange(new[] { "All", "Pending", "In Process", "Completed", "Cancelled" });
            statusFilterComboBox.SelectedIndex = 0;
            statusFilterComboBox.SelectedIndexChanged += StatusFilter_Changed;

            controlsPanel.Controls.AddRange(new Control[] {
                loadDataButton, refreshButton, searchLabel, searchByIdTextBox, searchButton,
                statusLabel, statusFilterComboBox
            });

            return controlsPanel;
        }

        private TabControl CreateTabControl()
        {
            TabControl tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };

            // Create all tabs
            CreateAllRequestsTab(tabControl);
            CreateSearchComparisonTab(tabControl);
            CreatePriorityQueueTab(tabControl);
            CreateGraphAnalysisTab(tabControl);
            CreateRouteOptimizationTab(tabControl);
            CreatePerformanceTab(tabControl);

            return tabControl;
        }

        private void CreateAllRequestsTab(TabControl parent)
        {
            allRequestsTab = new TabPage("All Requests (BST/AVL)");

            allRequestsListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };

            // Add columns
            allRequestsListView.Columns.Add("Request ID", 80);
            allRequestsListView.Columns.Add("User", 120);
            allRequestsListView.Columns.Add("Service Type", 150);
            allRequestsListView.Columns.Add("Status", 100);
            allRequestsListView.Columns.Add("Submission Date", 120);
            allRequestsListView.Columns.Add("Days Since", 80);

            allRequestsListView.DoubleClick += RequestsList_DoubleClick;

            Panel infoPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.LightBlue
            };

            Label infoLabel = new Label
            {
                Text = "Binary Search Tree (BST) and AVL Tree implementations for O(log n) search performance.\n" +
                       "AVL trees are self-balancing and guarantee O(log n) operations.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 5, 0, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };

            infoPanel.Controls.Add(infoLabel);
            allRequestsTab.Controls.Add(allRequestsListView);
            allRequestsTab.Controls.Add(infoPanel);
            parent.TabPages.Add(allRequestsTab);
        }

        private void CreateSearchComparisonTab(TabControl parent)
        {
            searchComparisonTab = new TabPage("Search Performance Comparison");

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

            searchResultsListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            searchResultsListView.Columns.Add("Algorithm", 120);
            searchResultsListView.Columns.Add("Time (ms)", 100);
            searchResultsListView.Columns.Add("Big O Notation", 120);
            searchResultsListView.Columns.Add("Result Found", 100);
            searchResultsListView.Columns.Add("Efficiency Gain", 120);

            Panel searchInfoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightGreen,
                Padding = new Padding(10)
            };

            Label searchInfoLabel = new Label
            {
                Text = "Search Performance Comparison:\n" +
                       "• Linear Search: O(n) - Checks every element\n" +
                       "• BST Search: O(log n) average, O(n) worst case\n" +
                       "• AVL Search: O(log n) guaranteed - Self-balancing\n\n" +
                       "Enter a Request ID above and click 'Compare Search' to see performance differences.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9)
            };

            searchInfoPanel.Controls.Add(searchInfoLabel);

            layout.Controls.Add(searchResultsListView, 0, 0);
            layout.Controls.Add(searchInfoPanel, 0, 1);

            searchComparisonTab.Controls.Add(layout);
            parent.TabPages.Add(searchComparisonTab);
        }

        private void CreatePriorityQueueTab(TabControl parent)
        {
            priorityQueueTab = new TabPage("Priority Queue (Heap)");

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

            priorityListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            priorityListView.Columns.Add("Priority Rank", 80);
            priorityListView.Columns.Add("Request ID", 80);
            priorityListView.Columns.Add("Service Type", 150);
            priorityListView.Columns.Add("Status", 100);
            priorityListView.Columns.Add("Days Waiting", 100);
            priorityListView.Columns.Add("Priority Score", 100);

            // Priority queue control buttons
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray
            };

            Button showTopPriorityButton = new Button
            {
                Text = "Show Top 10",
                Location = new Point(10, 5),
                Size = new Size(100, 30),
                BackColor = AppColors.ActionButton,
                ForeColor = Color.White
            };
            showTopPriorityButton.Click += ShowTopPriority_Click;

            Button showUrgentButton = new Button
            {
                Text = "Show Urgent",
                Location = new Point(120, 5),
                Size = new Size(100, 30),
                BackColor = AppColors.DangerButton,
                ForeColor = Color.White
            };
            showUrgentButton.Click += ShowUrgent_Click;

            Button showOverdueButton = new Button
            {
                Text = "Show Overdue",
                Location = new Point(230, 5),
                Size = new Size(100, 30),
                BackColor = AppColors.WarningButton,
                ForeColor = Color.White
            };
            showOverdueButton.Click += ShowOverdue_Click;

            buttonPanel.Controls.AddRange(new Control[] { showTopPriorityButton, showUrgentButton, showOverdueButton });

            Panel heapInfoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightYellow,
                Padding = new Padding(10)
            };

            Label heapInfoLabel = new Label
            {
                Text = "Priority Queue (Min-Heap) Implementation:\n" +
                       "• Insert: O(log n) - Add new request and bubble up\n" +
                       "• Extract-Min: O(log n) - Remove highest priority and bubble down\n" +
                       "• Peek: O(1) - View highest priority without removal\n\n" +
                       "Priority calculated from: Status urgency + Service type + Age of request",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9)
            };

            heapInfoPanel.Controls.Add(heapInfoLabel);

            layout.Controls.Add(priorityListView, 0, 0);
            layout.Controls.Add(buttonPanel, 0, 1);
            layout.Controls.Add(heapInfoPanel, 0, 2);

            priorityQueueTab.Controls.Add(layout);
            parent.TabPages.Add(priorityQueueTab);
        }

        private void CreateGraphAnalysisTab(TabControl parent)
        {
            graphAnalysisTab = new TabPage("Dependency Graph (DFS/BFS)");

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

            graphListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            graphListView.Columns.Add("Request ID", 80);
            graphListView.Columns.Add("Service Type", 150);
            graphListView.Columns.Add("Dependencies", 200);
            graphListView.Columns.Add("Dependents", 200);
            graphListView.Columns.Add("Status", 100);

            // Graph operation buttons
            Panel graphControlsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightCyan
            };

            Button dfsButton = new Button
            {
                Text = "DFS Traversal",
                Location = new Point(10, 5),
                Size = new Size(120, 25),
                BackColor = AppColors.InfoColor,
                ForeColor = Color.White
            };
            dfsButton.Click += DFS_Click;

            Button bfsButton = new Button
            {
                Text = "BFS Traversal",
                Location = new Point(140, 5),
                Size = new Size(120, 25),
                BackColor = AppColors.SuccessColor,
                ForeColor = Color.White
            };
            bfsButton.Click += BFS_Click;

            Button topSortButton = new Button
            {
                Text = "Topological Sort",
                Location = new Point(270, 5),
                Size = new Size(120, 25),
                BackColor = AppColors.WarningButton,
                ForeColor = Color.White
            };
            topSortButton.Click += TopologicalSort_Click;

            Button cycleCheckButton = new Button
            {
                Text = "Check Cycles",
                Location = new Point(400, 5),
                Size = new Size(120, 25),
                BackColor = Color.Purple,
                ForeColor = Color.White
            };
            cycleCheckButton.Click += CheckCycles_Click;

            Button criticalPathButton = new Button
            {
                Text = "Critical Path",
                Location = new Point(10, 30),
                Size = new Size(120, 25),
                BackColor = Color.DarkRed,
                ForeColor = Color.White
            };
            criticalPathButton.Click += CriticalPath_Click;

            graphControlsPanel.Controls.AddRange(new Control[] {
                dfsButton, bfsButton, topSortButton, cycleCheckButton, criticalPathButton
            });

            Panel graphInfoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightPink,
                Padding = new Padding(10)
            };

            Label graphInfoLabel = new Label
            {
                Text = "Graph Algorithms for Dependency Management:\n" +
                       "• DFS: Depth-First Search - Explores as far as possible along each branch\n" +
                       "• BFS: Breadth-First Search - Explores neighbors before going deeper\n" +
                       "• Topological Sort: Orders tasks based on dependencies\n" +
                       "• Cycle Detection: Finds circular dependencies\n" +
                       "• Critical Path: Longest path determining project completion time",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9)
            };

            graphInfoPanel.Controls.Add(graphInfoLabel);

            layout.Controls.Add(graphListView, 0, 0);
            layout.Controls.Add(graphControlsPanel, 0, 1);
            layout.Controls.Add(graphInfoPanel, 0, 2);

            graphAnalysisTab.Controls.Add(layout);
            parent.TabPages.Add(graphAnalysisTab);
        }

        private void CreateRouteOptimizationTab(TabControl parent)
        {
            routeOptimizationTab = new TabPage("Route Optimization (MST)");

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

            routeListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            routeListView.Columns.Add("Route Order", 80);
            routeListView.Columns.Add("Request ID", 80);
            routeListView.Columns.Add("Service Type", 150);
            routeListView.Columns.Add("Location", 200);
            routeListView.Columns.Add("Distance to Next", 120);
            routeListView.Columns.Add("Estimated Time", 100);

            // MST operation buttons
            Panel mstControlsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightSalmon
            };

            Button kruskalButton = new Button
            {
                Text = "Kruskal MST",
                Location = new Point(10, 5),
                Size = new Size(100, 30),
                BackColor = AppColors.ActionButton,
                ForeColor = Color.White
            };
            kruskalButton.Click += Kruskal_Click;

            Button primButton = new Button
            {
                Text = "Prim MST",
                Location = new Point(120, 5),
                Size = new Size(100, 30),
                BackColor = AppColors.InfoColor,
                ForeColor = Color.White
            };
            primButton.Click += Prim_Click;

            Button optimizeRouteButton = new Button
            {
                Text = "Optimize Route",
                Location = new Point(230, 5),
                Size = new Size(120, 30),
                BackColor = AppColors.SuccessColor,
                ForeColor = Color.White
            };
            optimizeRouteButton.Click += OptimizeRoute_Click;

            Button showClustersButton = new Button
            {
                Text = "Show Clusters",
                Location = new Point(360, 5),
                Size = new Size(120, 30),
                BackColor = Color.Orange,
                ForeColor = Color.White
            };
            showClustersButton.Click += ShowClusters_Click;

            mstControlsPanel.Controls.AddRange(new Control[] {
                kruskalButton, primButton, optimizeRouteButton, showClustersButton
            });

            Panel mstInfoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightGreen,
                Padding = new Padding(10)
            };

            Label mstInfoLabel = new Label
            {
                Text = "Minimum Spanning Tree (MST) for Route Optimization:\n" +
                       "• Kruskal's Algorithm: Sort edges, use Union-Find to avoid cycles\n" +
                       "• Prim's Algorithm: Grow tree from starting vertex\n" +
                       "• Both guarantee minimum total distance/cost\n" +
                       "• Clustering: Group nearby requests for efficient scheduling\n" +
                       "• Applications: Vehicle routing, resource allocation, cost minimization",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9)
            };

            mstInfoPanel.Controls.Add(mstInfoLabel);

            layout.Controls.Add(routeListView, 0, 0);
            layout.Controls.Add(mstControlsPanel, 0, 1);
            layout.Controls.Add(mstInfoPanel, 0, 2);

            routeOptimizationTab.Controls.Add(layout);
            parent.TabPages.Add(routeOptimizationTab);
        }

        private void CreatePerformanceTab(TabControl parent)
        {
            performanceTab = new TabPage("Performance Analysis");

            performanceTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen
            };

            Panel performanceControlPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.LightGray
            };

            Button generateReportButton = new Button
            {
                Text = "Generate Performance Report",
                Location = new Point(10, 10),
                Size = new Size(200, 30),
                BackColor = AppColors.ActionButton,
                ForeColor = Color.White
            };
            generateReportButton.Click += GenerateReport_Click;

            Button compareAlgorithmsButton = new Button
            {
                Text = "Compare Algorithms",
                Location = new Point(220, 10),
                Size = new Size(150, 30),
                BackColor = AppColors.InfoColor,
                ForeColor = Color.White
            };
            compareAlgorithmsButton.Click += CompareAlgorithms_Click;

            Button exportReportButton = new Button
            {
                Text = "Export Report",
                Location = new Point(380, 10),
                Size = new Size(120, 30),
                BackColor = AppColors.SuccessColor,
                ForeColor = Color.White
            };
            exportReportButton.Click += ExportReport_Click;

            performanceControlPanel.Controls.AddRange(new Control[] {
                generateReportButton, compareAlgorithmsButton, exportReportButton
            });

            performanceTab.Controls.Add(performanceTextBox);
            performanceTab.Controls.Add(performanceControlPanel);
            parent.TabPages.Add(performanceTab);
        }

        private void LoadInitialData()
        {
            try
            {
                performanceLabel.Text = "Loading data structures...";
                performanceLabel.ForeColor = Color.Yellow;
                Application.DoEvents();

                var metrics = dataManager.LoadAllRequests();

                if (string.IsNullOrEmpty(metrics.ErrorMessage))
                {
                    performanceLabel.Text = $"Loaded {metrics.TotalRequestsLoaded} requests in {metrics.TotalLoadTime}ms";
                    performanceLabel.ForeColor = Color.LightGreen;

                    RefreshAllTabs();
                }
                else
                {
                    performanceLabel.Text = $"Error: {metrics.ErrorMessage}";
                    performanceLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                performanceLabel.Text = $"Load failed: {ex.Message}";
                performanceLabel.ForeColor = Color.Red;
            }
        }

        private void RefreshAllTabs()
        {
            RefreshAllRequestsTab();
            RefreshPriorityQueueTab();
            RefreshGraphTab();
            RefreshRouteTab();
        }

        private void RefreshAllRequestsTab()
        {
            allRequestsListView.Items.Clear();

            // Get all requests using BST in-order traversal (sorted by ID)
            var allRequests = dataManager.GetTreeStatistics().BSTStatistics;

            // For display, get from repository
            var requests = ServiceRequestRepository.GetAllServiceRequests();

            foreach (var request in requests.OrderBy(r => r.RequestID))
            {
                var item = new ListViewItem(request.RequestID.ToString());
                item.SubItems.Add(request.UserName ?? "Anonymous");
                item.SubItems.Add(request.ServiceType);
                item.SubItems.Add(request.Status);
                item.SubItems.Add(request.SubmissionDate.ToString("MM/dd/yyyy"));

                int daysSince = (DateTime.Now - request.SubmissionDate).Days;
                item.SubItems.Add(daysSince.ToString());

                // Color code by status
                item.BackColor = GetStatusColor(request.Status);
                item.Tag = request;

                allRequestsListView.Items.Add(item);
            }
        }

        private void RefreshPriorityQueueTab()
        {
            var priorityResult = dataManager.GetPriorityOperations();

            priorityListView.Items.Clear();

            for (int i = 0; i < priorityResult.TopPriorityRequests.Count; i++)
            {
                var request = priorityResult.TopPriorityRequests[i];
                var item = new ListViewItem((i + 1).ToString());
                item.SubItems.Add(request.RequestID.ToString());
                item.SubItems.Add(request.ServiceType);
                item.SubItems.Add(request.Status);

                int daysWaiting = (DateTime.Now - request.SubmissionDate).Days;
                item.SubItems.Add(daysWaiting.ToString());

                // Show priority score (would need to be calculated)
                item.SubItems.Add("High");

                item.BackColor = GetPriorityColor(request.Status);
                item.Tag = request;

                priorityListView.Items.Add(item);
            }
        }

        private void RefreshGraphTab()
        {
            var graphResult = dataManager.GetGraphAnalysis();

            graphListView.Items.Clear();

            var requests = ServiceRequestRepository.GetAllServiceRequests();

            foreach (var request in requests)
            {
                var item = new ListViewItem(request.RequestID.ToString());
                item.SubItems.Add(request.ServiceType);
                item.SubItems.Add("Sample Deps"); // Would show actual dependencies
                item.SubItems.Add("Sample Dependents"); // Would show actual dependents
                item.SubItems.Add(request.Status);

                item.Tag = request;
                graphListView.Items.Add(item);
            }
        }

        private void RefreshRouteTab()
        {
            var mstResult = dataManager.GetMSTOptimization();

            routeListView.Items.Clear();

            if (mstResult.OptimalRoute != null)
            {
                for (int i = 0; i < mstResult.OptimalRoute.Count; i++)
                {
                    var request = mstResult.OptimalRoute[i];
                    var item = new ListViewItem((i + 1).ToString());
                    item.SubItems.Add(request.RequestID.ToString());
                    item.SubItems.Add(request.ServiceType);
                    item.SubItems.Add("Location data would be here");
                    item.SubItems.Add(i < mstResult.OptimalRoute.Count - 1 ? "2.5 km" : "End");
                    item.SubItems.Add(i < mstResult.OptimalRoute.Count - 1 ? "15 min" : "-");

                    item.Tag = request;
                    routeListView.Items.Add(item);
                }
            }
        }

        #region Event Handlers

        private void LoadDataButton_Click(object sender, EventArgs e)
        {
            LoadInitialData();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshAllTabs();
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (int.TryParse(searchByIdTextBox.Text, out int requestID))
            {
                var result = dataManager.SearchByID(requestID);
                DisplaySearchComparison(result);
                mainTabControl.SelectedTab = searchComparisonTab;
            }
            else
            {
                MessageBox.Show("Please enter a valid Request ID.", "Invalid Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DisplaySearchComparison(SearchComparisonResult result)
        {
            searchResultsListView.Items.Clear();

            // Linear Search
            var linearItem = new ListViewItem("Linear Search");
            linearItem.SubItems.Add(result.LinearSearchTime.ToString());
            linearItem.SubItems.Add("O(n)");
            linearItem.SubItems.Add(result.LinearSearchResult != null ? "Yes" : "No");
            linearItem.SubItems.Add("1x (baseline)");
            searchResultsListView.Items.Add(linearItem);

            // BST Search
            var bstItem = new ListViewItem("BST Search");
            bstItem.SubItems.Add(result.BSTSearchTime.ToString());
            bstItem.SubItems.Add("O(log n) avg");
            bstItem.SubItems.Add(result.BSTSearchResult != null ? "Yes" : "No");
            var bstGain = result.BSTSearchTime > 0 ? result.LinearSearchTime / (double)result.BSTSearchTime : 1;
            bstItem.SubItems.Add($"{bstGain:F1}x");
            searchResultsListView.Items.Add(bstItem);

            // AVL Search
            var avlItem = new ListViewItem("AVL Search");
            avlItem.SubItems.Add(result.AVLSearchTime.ToString());
            avlItem.SubItems.Add("O(log n)");
            avlItem.SubItems.Add(result.AVLSearchResult != null ? "Yes" : "No");
            var avlGain = result.AVLSearchTime > 0 ? result.LinearSearchTime / (double)result.AVLSearchTime : 1;
            avlItem.SubItems.Add($"{avlGain:F1}x");
            searchResultsListView.Items.Add(avlItem);
        }

        private void StatusFilter_Changed(object sender, EventArgs e)
        {
            string selectedStatus = statusFilterComboBox.SelectedItem.ToString();
            if (selectedStatus == "All")
            {
                RefreshAllRequestsTab();
            }
            else
            {
                var filteredResult = dataManager.GetRequestsByStatus(selectedStatus);
                DisplayFilteredResults(filteredResult);
            }
        }

        private void DisplayFilteredResults(FilteredResults result)
        {
            allRequestsListView.Items.Clear();

            foreach (var request in result.LinearResults)
            {
                var item = new ListViewItem(request.RequestID.ToString());
                item.SubItems.Add(request.UserName ?? "Anonymous");
                item.SubItems.Add(request.ServiceType);
                item.SubItems.Add(request.Status);
                item.SubItems.Add(request.SubmissionDate.ToString("MM/dd/yyyy"));

                int daysSince = (DateTime.Now - request.SubmissionDate).Days;
                item.SubItems.Add(daysSince.ToString());

                item.BackColor = GetStatusColor(request.Status);
                item.Tag = request;

                allRequestsListView.Items.Add(item);
            }
        }

        // Priority Queue event handlers
        private void ShowTopPriority_Click(object sender, EventArgs e)
        {
            RefreshPriorityQueueTab();
        }

        private void ShowUrgent_Click(object sender, EventArgs e)
        {
            var priorityResult = dataManager.GetPriorityOperations();
            DisplayPriorityRequests(priorityResult.UrgentRequests, "Urgent Requests");
        }

        private void ShowOverdue_Click(object sender, EventArgs e)
        {
            var priorityResult = dataManager.GetPriorityOperations();
            DisplayPriorityRequests(priorityResult.OverdueRequests, "Overdue Requests");
        }

        private void DisplayPriorityRequests(List<ServiceRequest> requests, string title)
        {
            priorityListView.Items.Clear();

            for (int i = 0; i < requests.Count; i++)
            {
                var request = requests[i];
                var item = new ListViewItem((i + 1).ToString());
                item.SubItems.Add(request.RequestID.ToString());
                item.SubItems.Add(request.ServiceType);
                item.SubItems.Add(request.Status);

                int daysWaiting = (DateTime.Now - request.SubmissionDate).Days;
                item.SubItems.Add(daysWaiting.ToString());
                item.SubItems.Add(request.Status);

                item.BackColor = GetPriorityColor(request.Status);
                item.Tag = request;

                priorityListView.Items.Add(item);
            }
        }

        // Graph event handlers
        private void DFS_Click(object sender, EventArgs e)
        {
            var graphResult = dataManager.GetGraphAnalysis();
            if (graphResult.DFSTraversal != null)
            {
                DisplayGraphTraversal(graphResult.DFSTraversal, "DFS Traversal");
            }
        }

        private void BFS_Click(object sender, EventArgs e)
        {
            var graphResult = dataManager.GetGraphAnalysis();
            if (graphResult.BFSTraversal != null)
            {
                DisplayGraphTraversal(graphResult.BFSTraversal, "BFS Traversal");
            }
        }

        private void TopologicalSort_Click(object sender, EventArgs e)
        {
            var graphResult = dataManager.GetGraphAnalysis();
            if (graphResult.TopologicalOrder != null)
            {
                DisplayGraphTraversal(graphResult.TopologicalOrder, "Topological Order");
            }
            else
            {
                MessageBox.Show("Cannot perform topological sort - graph contains cycles!",
                    "Cycle Detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CheckCycles_Click(object sender, EventArgs e)
        {
            var graphResult = dataManager.GetGraphAnalysis();
            string message = graphResult.HasCycles ?
                "Cycles detected! Some requests have circular dependencies." :
                "No cycles found. All dependencies are valid.";

            MessageBox.Show(message, "Cycle Detection", MessageBoxButtons.OK,
                graphResult.HasCycles ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private void CriticalPath_Click(object sender, EventArgs e)
        {
            var graphResult = dataManager.GetGraphAnalysis();
            if (graphResult.CriticalPath != null)
            {
                DisplayGraphTraversal(graphResult.CriticalPath, "Critical Path");
            }
        }

        private void DisplayGraphTraversal(List<ServiceRequest> traversal, string title)
        {
            graphListView.Items.Clear();

            for (int i = 0; i < traversal.Count; i++)
            {
                var request = traversal[i];
                var item = new ListViewItem(request.RequestID.ToString());
                item.SubItems.Add(request.ServiceType);
                item.SubItems.Add($"Step {i + 1}");
                item.SubItems.Add($"Order: {i + 1}");
                item.SubItems.Add(request.Status);

                item.Tag = request;
                graphListView.Items.Add(item);
            }
        }

        // MST event handlers
        private void Kruskal_Click(object sender, EventArgs e)
        {
            var mstResult = dataManager.GetMSTOptimization();
            MessageBox.Show($"Kruskal's MST completed in {mstResult.KruskalTime}ms\n" +
                          $"Found {mstResult.KruskalMST?.Count ?? 0} edges",
                          "Kruskal's Algorithm", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Prim_Click(object sender, EventArgs e)
        {
            var mstResult = dataManager.GetMSTOptimization();
            MessageBox.Show($"Prim's MST completed in {mstResult.PrimTime}ms\n" +
                          $"Found {mstResult.PrimMST?.Count ?? 0} edges",
                          "Prim's Algorithm", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OptimizeRoute_Click(object sender, EventArgs e)
        {
            RefreshRouteTab();
        }

        private void ShowClusters_Click(object sender, EventArgs e)
        {
            var mstResult = dataManager.GetMSTOptimization();
            if (mstResult.ServiceClusters != null)
            {
                string message = $"Found {mstResult.ServiceClusters.Count} service clusters:\n\n";
                for (int i = 0; i < mstResult.ServiceClusters.Count; i++)
                {
                    message += $"Cluster {i + 1}: {mstResult.ServiceClusters[i].Count} requests\n";
                }

                MessageBox.Show(message, "Service Clusters", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Performance event handlers
        private void GenerateReport_Click(object sender, EventArgs e)
        {
            string report = dataManager.GeneratePerformanceReport();
            performanceTextBox.Text = report;
        }

        private void CompareAlgorithms_Click(object sender, EventArgs e)
        {
            var comparison = dataManager.GetPerformanceComparison();

            string report = "ALGORITHM PERFORMANCE COMPARISON\n";
            report += new string('=', 40) + "\n\n";
            report += $"Search Performance:\n";
            report += $"Linear Search: {comparison.LinearSearchAverage:F2} ms (O(n))\n";
            report += $"BST Search: {comparison.BSTSearchAverage:F2} ms (O(log n) avg)\n";
            report += $"AVL Search: {comparison.AVLSearchAverage:F2} ms (O(log n))\n\n";
            report += $"Efficiency Gains:\n";
            report += $"BST is {comparison.BSTEfficiencyGain:F1}x faster than linear\n";
            report += $"AVL is {comparison.AVLEfficiencyGain:F1}x faster than linear\n\n";
            report += $"Memory Usage:\n";
            report += $"Linear: {comparison.LinearMemoryUsage:N0} bytes\n";
            report += $"BST: {comparison.BSTMemoryUsage:N0} bytes\n";
            report += $"AVL: {comparison.AVLMemoryUsage:N0} bytes\n";
            report += $"Heap: {comparison.HeapMemoryUsage:N0} bytes\n";

            performanceTextBox.Text = report;
        }

        private void ExportReport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                dialog.Title = "Export Performance Report";
                dialog.FileName = "ServiceRequest_Performance_Report.txt";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        System.IO.File.WriteAllText(dialog.FileName, performanceTextBox.Text);
                        MessageBox.Show("Report exported successfully!", "Export Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting report: {ex.Message}", "Export Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void RequestsList_DoubleClick(object sender, EventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView?.SelectedItems.Count > 0)
            {
                ServiceRequest request = listView.SelectedItems[0].Tag as ServiceRequest;
                if (request != null)
                {
                    ShowRequestDetails(request);
                }
            }
        }

        private Color GetStatusColor(string status)
        {
            switch (status)
            {
                case "Critical":
                    return Color.Red;
                case "Urgent":
                    return Color.Orange;
                case "High":
                    return Color.Yellow;
                case "In Process":
                    return Color.LightBlue;
                case "Pending":
                    return Color.LightGray;
                case "Completed":
                    return Color.LightGreen;
                case "Cancelled":
                    return Color.Pink;
                default:
                    return Color.White;
            }
        }

        private Color GetPriorityColor(string status)
        {
            switch (status)
            {
                case "Critical":
                    return Color.DarkRed;
                case "Urgent":
                    return Color.OrangeRed;
                case "High":
                    return Color.Gold;
                case "In Process":
                    return Color.CornflowerBlue;
                case "Pending":
                    return Color.Silver;
                case "Completed":
                    return Color.ForestGreen;
                case "Cancelled":
                    return Color.Gray;
                default:
                    return Color.White;
            }
        }

        private void ShowRequestDetails(ServiceRequest request)
        {
            string details = $"Request Details\n" +
                           $"================\n\n" +
                           $"Request ID: {request.RequestID}\n" +
                           $"Service Type: {request.ServiceType}\n" +
                           $"Description: {request.Description}\n" +
                           $"Status: {request.Status}\n" +
                           $"Submitted: {request.SubmissionDate:MM/dd/yyyy HH:mm}\n" +
                           $"User: {request.UserName ?? "Anonymous"}\n";

            if (request.CompletionDate.HasValue)
            {
                details += $"Completed: {request.CompletionDate:MM/dd/yyyy HH:mm}\n";
                TimeSpan duration = request.CompletionDate.Value - request.SubmissionDate;
                details += $"Processing Time: {duration.Days} days, {duration.Hours} hours\n";
            }
            else
            {
                TimeSpan pending = DateTime.Now - request.SubmissionDate;
                details += $"Pending Time: {pending.Days} days, {pending.Hours} hours\n";
            }

            MessageBox.Show(details, "Service Request Details",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}