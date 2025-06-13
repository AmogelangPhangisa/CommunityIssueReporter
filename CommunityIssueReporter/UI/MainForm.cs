using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CommunityIssueReporter.Data;
using CommunityIssueReporter.Models;
using CommunityIssueReporter.Utilities;

namespace CommunityIssueReporter.UI
{
    public class MainForm : Form
    {
        private Panel headerPanel;
        private Panel sidebarPanel;
        private Panel contentPanel;
        private Label titleLabel;
        private bool isOfflineMode;

        public MainForm(bool offlineMode = false)
        {
            isOfflineMode = offlineMode;
            InitializeComponents();
            SetupInitialState();
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = "Community Engagement Portal";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.Icon = new Icon(SystemIcons.Application, 40, 40);

            // Create panels
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = AppColors.AccentColor
            };

            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = AppColors.SidebarBackground
            };

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.PrimaryBackground
            };

            // Add title to header
            titleLabel = new Label
            {
                Text = "Community Engagement Portal",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            // Add user info panel to header
            Panel userInfoPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = AppColors.AccentColor
            };

            // Create a clickable status label
            Label userStatusLabel = new Label
            {
                Text = "Not Logged In",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                Name = "userStatusLabel",
                Padding = new Padding(0, 0, 20, 0),
                Cursor = Cursors.Hand
            };
            userStatusLabel.Click += UserStatusLabel_Click;

            Button loginButton = new Button
            {
                Text = "Login",
                ForeColor = Color.White,
                BackColor = AppColors.SecondaryAccent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 30),
                Location = new Point(userInfoPanel.Width - 100, 15),
                Name = "loginButton",
                Cursor = Cursors.Hand
            };
            loginButton.FlatAppearance.BorderSize = 0;
            loginButton.Click += LoginButton_Click;

            userInfoPanel.Controls.Add(userStatusLabel);
            userInfoPanel.Controls.Add(loginButton);

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(userInfoPanel);

            // Create sidebar menu items
            CreateSidebarMenu();

            // Create initial content
            CreateWelcomeContent();

            // Add panels to form
            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebarPanel);
            this.Controls.Add(headerPanel);

            // Set up offline mode indicator if needed
            if (isOfflineMode)
            {
                Label offlineModeLabel = new Label
                {
                    Text = "OFFLINE MODE",
                    ForeColor = Color.Yellow,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(20, headerPanel.Height - 20)
                };
                headerPanel.Controls.Add(offlineModeLabel);
            }
        }
        private void CreateSidebarMenu()
        {
            // Clear existing controls
            sidebarPanel.Controls.Clear();

            // Panel for app logo/branding
            Panel brandPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(40, 50, 60)
            };

            Label brandLabel = new Label
            {
                Text = "Community App",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            brandPanel.Controls.Add(brandLabel);
            sidebarPanel.Controls.Add(brandPanel);

            // Add menu buttons
            AddSidebarButton("Dashboard", "dashboardButton", DashboardButton_Click);
            AddSidebarButton("Report Issues", "reportIssuesButton", ReportIssuesButton_Click);
            AddSidebarButton("My Issues", "myIssuesButton", MyIssuesButton_Click);
            AddSidebarButton("Events", "eventsButton", EventsButton_Click);
            AddSidebarButton("Service Requests", "serviceRequestsButton", ServiceRequestsButton_Click);
            AddSidebarButton("Request Status", "requestStatusButton", RequestStatusButton_Click);

            // Add admin panel if user is admin
            bool isAdmin = UserRepository.CurrentUser?.IsAdmin ?? false;
            if (isAdmin)
            {
                // Separator
                Panel separatorPanel = new Panel
                {
                    Height = 1,
                    Dock = DockStyle.Top,
                    BackColor = Color.FromArgb(70, 80, 90),
                    Margin = new Padding(10)
                };
                sidebarPanel.Controls.Add(separatorPanel);

                // Admin section label
                Label adminLabel = new Label
                {
                    Text = "Administration",
                    ForeColor = Color.Silver,
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    Dock = DockStyle.Top,
                    Height = 30,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(20, 0, 0, 0)
                };
                sidebarPanel.Controls.Add(adminLabel);

                // Admin buttons
                AddSidebarButton("Manage Issues", "manageIssuesButton", ManageIssuesButton_Click);
                AddSidebarButton("Manage Events", "manageEventsButton", ManageEventsButton_Click);
                AddSidebarButton("Manage Users", "manageUsersButton", ManageUsersButton_Click);
            }
        }

        private void AddSidebarButton(string text, string name, EventHandler clickHandler)
        {
            Button button = new Button
            {
                Text = text,
                Name = name,
                Dock = DockStyle.Top,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand,
                BackColor = AppColors.SidebarBackground
            };

            button.FlatAppearance.BorderSize = 0;
            button.Click += clickHandler;

            // Add hover effect
            button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(70, 80, 90);
            button.MouseLeave += (s, e) => button.BackColor = AppColors.SidebarBackground;

            sidebarPanel.Controls.Add(button);
        }

        private void CreateWelcomeContent()
        {
            // Clear existing content
            contentPanel.Controls.Clear();

            // Add welcome panel
            Panel welcomePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40)
            };

            // Welcome message
            Label welcomeLabel = new Label
            {
                Text = "Welcome to the Community Engagement Portal",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = AppColors.PrimaryText,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            Label descriptionLabel = new Label
            {
                Text = "This application allows you to engage with your community by reporting issues, " +
                       "viewing local events, and making service requests.",
                Font = new Font("Segoe UI", 12),
                ForeColor = AppColors.PrimaryText,
                AutoSize = false,
                Width = 700,
                Height = 50,
                Location = new Point(0, 50)
            };

            // Quick actions panel
            GroupBox quickActionsBox = new GroupBox
            {
                Text = "Quick Actions",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = AppColors.PrimaryText,
                Size = new Size(700, 250),
                Location = new Point(0, 120)
            };

            // Report Issues Button
            Button reportIssueButton = new Button
            {
                Text = "Report an Issue",
                Size = new Size(200, 60),
                Location = new Point(40, 40),
                BackColor = AppColors.SecondaryAccent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            reportIssueButton.FlatAppearance.BorderSize = 0;
            reportIssueButton.Click += ReportIssuesButton_Click;

            // View Events Button
            Button viewEventsButton = new Button
            {
                Text = "View Local Events",
                Size = new Size(200, 60),
                Location = new Point(250, 40),
                BackColor = AppColors.InfoColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            viewEventsButton.FlatAppearance.BorderSize = 0;
            viewEventsButton.Click += EventsButton_Click;

            // Service Request Button
            Button serviceRequestButton = new Button
            {
                Text = "Make Service Request",
                Size = new Size(200, 60),
                Location = new Point(460, 40),
                BackColor = AppColors.SuccessColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            serviceRequestButton.FlatAppearance.BorderSize = 0;
            serviceRequestButton.Click += ServiceRequestsButton_Click;

            Button requestStatusButton = new Button
            {
                Text = "View Request Status",
                Size = new Size(200, 60),
                Location = new Point(460, 120),
                BackColor = Color.Purple,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            requestStatusButton.FlatAppearance.BorderSize = 0;
            requestStatusButton.Click += RequestStatusButton_Click;

            // View My Issues Button
            Button viewMyIssuesButton = new Button
            {
                Text = "View My Issues",
                Size = new Size(200, 60),
                Location = new Point(40, 120),
                BackColor = AppColors.WarningColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            viewMyIssuesButton.FlatAppearance.BorderSize = 0;
            viewMyIssuesButton.Click += MyIssuesButton_Click;

            // View Dashboard Button
            Button viewDashboardButton = new Button
            {
                Text = "View Dashboard",
                Size = new Size(200, 60),
                Location = new Point(250, 120),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            viewDashboardButton.FlatAppearance.BorderSize = 0;
            viewDashboardButton.Click += DashboardButton_Click;

            // Add controls to the quick actions box
            quickActionsBox.Controls.Add(reportIssueButton);
            quickActionsBox.Controls.Add(viewEventsButton);
            quickActionsBox.Controls.Add(serviceRequestButton);
            quickActionsBox.Controls.Add(viewMyIssuesButton);
            quickActionsBox.Controls.Add(viewDashboardButton);
            quickActionsBox.Controls.Add(requestStatusButton);

            // Status panel
            Panel statusPanel = new Panel
            {
                Size = new Size(700, 80),
                Location = new Point(0, 390),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label statusLabel = new Label
            {
                Text = isOfflineMode ?
                    "Application is running in OFFLINE MODE. Some features may be limited." :
                    "Application is connected to the server.",
                Font = new Font("Segoe UI", 10),
                ForeColor = isOfflineMode ? AppColors.WarningColor : AppColors.SuccessColor,
                AutoSize = false,
                Size = new Size(670, 30),
                Location = new Point(15, 15)
            };

            Label loginStatusLabel = new Label
            {
                Text = UserRepository.CurrentUser == null ?
                    "You are not logged in. Some features require login." :
                    $"Logged in as {UserRepository.CurrentUser.FullName}",
                Font = new Font("Segoe UI", 10),
                ForeColor = UserRepository.CurrentUser == null ? AppColors.WarningColor : AppColors.SuccessColor,
                AutoSize = false,
                Size = new Size(670, 30),
                Location = new Point(15, 45)
            };

            statusPanel.Controls.Add(statusLabel);
            statusPanel.Controls.Add(loginStatusLabel);

            // Add everything to welcome panel
            welcomePanel.Controls.Add(welcomeLabel);
            welcomePanel.Controls.Add(descriptionLabel);
            welcomePanel.Controls.Add(quickActionsBox);
            welcomePanel.Controls.Add(statusPanel);

            // Add welcome panel to content panel
            contentPanel.Controls.Add(welcomePanel);
        }
        private void SetupInitialState()
        {
            // Initialize repositories for offline mode if needed
            if (isOfflineMode)
            {
                DatabaseManager.SetOfflineMode(true);
                UserRepository.InitializeOfflineUsers();
                IssueRepository.InitializeOfflineIssues();
                EventRepository.InitializeOfflineEvents();
                ServiceRequestRepository.InitializeOfflineServiceRequests();
            }

            // Update UI based on login status
            UpdateUIForLoginStatus();
        }

        private void UpdateUIForLoginStatus()
        {
            // Update sidebar menu
            CreateSidebarMenu();

            // Update header user info
            Label userStatusLabel = (Label)this.Controls.Find("userStatusLabel", true)[0];
            Button loginButton = (Button)this.Controls.Find("loginButton", true)[0];

            if (UserRepository.CurrentUser != null)
            {
                userStatusLabel.Text = $"Logged in as {UserRepository.CurrentUser.FullName}";
                userStatusLabel.ForeColor = Color.White;
                userStatusLabel.Cursor = Cursors.Hand;
                loginButton.Text = "Logout";
            }
            else
            {
                userStatusLabel.Text = "Not Logged In - Click to Login";
                userStatusLabel.ForeColor = Color.LightYellow;
                userStatusLabel.Cursor = Cursors.Hand;
                loginButton.Text = "Login";
            }

            // Refresh current content
            CreateWelcomeContent();
        }

        private void SetActiveButton(string buttonName)
        {
            // Reset all button colors
            foreach (Control control in sidebarPanel.Controls)
            {
                if (control is Button)
                {
                    control.BackColor = AppColors.SidebarBackground;
                }
            }

            // Set active button color
            Button activeButton = (Button)this.Controls.Find(buttonName, true)[0];
            if (activeButton != null)
            {
                activeButton.BackColor = AppColors.SecondaryAccent;
            }
        }

        private void ShowServiceRequestStatus()
        {
            int currentUserID = UserRepository.CurrentUser?.UserID ?? -1;
            string currentUserRole = UserRepository.CurrentUser?.UserRole ?? "Guest";

            // For now, we'll show a placeholder since ServiceRequestStatusForm isn't implemented yet
            MessageBox.Show("Service Request Status form will be implemented with Part 3 of the project.",
                "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowReportIssueForm()
        {
            this.Hide();
            using (IssueReportForm reportForm = new IssueReportForm())
            {
                reportForm.ShowDialog();
            }
            this.Show();
        }

        private void LoadEventsContent()
        {
            if (UserRepository.CurrentUser == null)
            {
                DialogResult result = MessageBox.Show(
                    "You can view events without logging in, but you'll need to log in to register for events. Would you like to log in now?",
                    "Login Recommended",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    LoginButton_Click(this.Controls.Find("loginButton", true)[0], EventArgs.Empty);
                }
            }

            int currentUserID = UserRepository.CurrentUser?.UserID ?? -1;
            string currentUserRole = UserRepository.CurrentUser?.UserRole ?? "Guest";

            this.Hide();
            using (EventsForm eventsForm = new EventsForm(currentUserID, currentUserRole))
            {
                eventsForm.ShowDialog();
            }
            this.Show();
        }

        private void LoadManageEventsContent()
        {
            if (UserRepository.CurrentUser == null ||
                (UserRepository.CurrentUser.UserRole != "Admin" && UserRepository.CurrentUser.UserRole != "Staff"))
            {
                MessageBox.Show("You must be an administrator or staff member to manage events.",
                    "Permission Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int currentUserID = UserRepository.CurrentUser.UserID;
            string currentUserRole = UserRepository.CurrentUser.UserRole;

            this.Hide();
            using (EventsForm eventsForm = new EventsForm(currentUserID, currentUserRole, true))
            {
                eventsForm.ShowDialog();
            }
            this.Show();
        }

        // Statistics helper methods
        private string GetTotalIssuesCount()
        {
            try
            {
                return IssueRepository.GetAllIssues().Count.ToString();
            }
            catch
            {
                return "N/A";
            }
        }

        private string GetUserIssuesCount()
        {
            try
            {
                if (UserRepository.CurrentUser == null)
                    return "0";

                return IssueRepository.GetIssuesByUserID(UserRepository.CurrentUser.UserID).Count.ToString();
            }
            catch
            {
                return "N/A";
            }
        }

        private string GetUpcomingEventsCount()
        {
            try
            {
                return EventRepository.GetUpcomingEvents().Count.ToString();
            }
            catch
            {
                return "N/A";
            }
        }

        private string GetServiceRequestsCount()
        {
            try
            {
                if (UserRepository.CurrentUser == null)
                    return "0";

                return ServiceRequestRepository.GetServiceRequestsByUserID(UserRepository.CurrentUser.UserID).Count.ToString();
            }
            catch
            {
                return "N/A";
            }
        }
        #region Event Handlers

        private void UserStatusLabel_Click(object sender, EventArgs e)
        {
            if (UserRepository.CurrentUser == null)
            {
                LoginButton_Click(this.Controls.Find("loginButton", true)[0], EventArgs.Empty);
            }
            else
            {
                MessageBox.Show($"Logged in as {UserRepository.CurrentUser.FullName}\nRole: {UserRepository.CurrentUser.UserRole}",
                    "User Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Text == "Login" || UserRepository.CurrentUser == null)
                {
                    using (LoginForm loginForm = new LoginForm(this))
                    {
                        this.Hide();
                        DialogResult result = loginForm.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            UpdateUIForLoginStatus();
                        }
                        this.Show();
                    }
                }
                else
                {
                    DialogResult result = MessageBox.Show("Are you sure you want to log out?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        UserRepository.LogoutUser();
                        UpdateUIForLoginStatus();
                    }
                }
            }
            else
            {
                if (UserRepository.CurrentUser == null)
                {
                    using (LoginForm loginForm = new LoginForm(this))
                    {
                        this.Hide();
                        DialogResult result = loginForm.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            UpdateUIForLoginStatus();
                        }
                        this.Show();
                    }
                }
            }
        }

        private void DashboardButton_Click(object sender, EventArgs e)
        {
            LoadDashboardContent();
        }

        private void ReportIssuesButton_Click(object sender, EventArgs e)
        {
            if (UserRepository.CurrentUser == null)
            {
                DialogResult result = MessageBox.Show("You must be logged in to report issues. Would you like to log in now?", "Login Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    LoginButton_Click(this.Controls.Find("loginButton", true)[0], EventArgs.Empty);
                    if (UserRepository.CurrentUser != null)
                    {
                        ShowReportIssueForm();
                    }
                }
            }
            else
            {
                ShowReportIssueForm();
            }
        }

        private void MyIssuesButton_Click(object sender, EventArgs e)
        {
            if (UserRepository.CurrentUser == null)
            {
                DialogResult result = MessageBox.Show("You must be logged in to view your issues. Would you like to log in now?", "Login Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    LoginButton_Click(this.Controls.Find("loginButton", true)[0], EventArgs.Empty);
                    if (UserRepository.CurrentUser != null)
                    {
                        LoadMyIssuesContent();
                    }
                }
            }
            else
            {
                LoadMyIssuesContent();
            }
        }

        private void EventsButton_Click(object sender, EventArgs e)
        {
            LoadEventsContent();
        }

        private void ServiceRequestsButton_Click(object sender, EventArgs e)
        {
            if (UserRepository.CurrentUser == null)
            {
                DialogResult result = MessageBox.Show("You must be logged in to make service requests. Would you like to log in now?", "Login Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    LoginButton_Click(this.Controls.Find("loginButton", true)[0], EventArgs.Empty);
                    if (UserRepository.CurrentUser != null)
                    {
                        LoadServiceRequestsContent();
                    }
                }
            }
            else
            {
                LoadServiceRequestsContent();
            }
        }

        private void RequestStatusButton_Click(object sender, EventArgs e)
        {
            if (UserRepository.CurrentUser == null)
            {
                DialogResult result = MessageBox.Show("You must be logged in to view service request status. Would you like to log in now?", "Login Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    LoginButton_Click(this.Controls.Find("loginButton", true)[0], EventArgs.Empty);
                    if (UserRepository.CurrentUser != null)
                    {
                        ShowServiceRequestStatus();
                    }
                }
            }
            else
            {
                ShowServiceRequestStatus();
            }
        }

        private void ManageIssuesButton_Click(object sender, EventArgs e)
        {
            LoadManageIssuesContent();
        }

        private void ManageEventsButton_Click(object sender, EventArgs e)
        {
            LoadManageEventsContent();
        }

        private void ManageUsersButton_Click(object sender, EventArgs e)
        {
            LoadManageUsersContent();
        }

        #endregion
        #region Content Loading Methods

        private void LoadDashboardContent()
        {
            contentPanel.Controls.Clear();
            SetActiveButton("dashboardButton");

            Panel dashboardPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            Label titleLabel = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = AppColors.PrimaryText,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            Label welcomeLabel = new Label
            {
                Text = UserRepository.CurrentUser != null
                    ? $"Welcome back, {UserRepository.CurrentUser.FirstName}!"
                    : "Welcome to the Community Portal!",
                Font = new Font("Segoe UI", 14),
                ForeColor = AppColors.PrimaryText,
                AutoSize = true,
                Location = new Point(0, 40)
            };

            Panel statsPanel = new Panel
            {
                Width = 760,
                Height = 120,
                Location = new Point(0, 80)
            };

            CreateStatCard(statsPanel, "Total Issues", GetTotalIssuesCount(), AppColors.InfoColor, new Point(0, 0));
            CreateStatCard(statsPanel, "My Issues", GetUserIssuesCount(), AppColors.WarningColor, new Point(190, 0));
            CreateStatCard(statsPanel, "Upcoming Events", GetUpcomingEventsCount(), AppColors.SuccessColor, new Point(380, 0));
            CreateStatCard(statsPanel, "Service Requests", GetServiceRequestsCount(), AppColors.SecondaryAccent, new Point(570, 0));

            GroupBox recentActivityBox = new GroupBox
            {
                Text = "Recent Activity",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = AppColors.PrimaryText,
                Width = 760,
                Height = 200,
                Location = new Point(0, 220)
            };

            ListView recentActivityList = CreateRecentActivityList();
            recentActivityList.Dock = DockStyle.Fill;
            recentActivityList.Margin = new Padding(10);
            recentActivityBox.Controls.Add(recentActivityList);

            GroupBox quickActionsBox = new GroupBox
            {
                Text = "Quick Actions",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = AppColors.PrimaryText,
                Width = 760,
                Height = 150,
                Location = new Point(0, 440)
            };

            CreateQuickActionButtons(quickActionsBox);

            GroupBox statusBox = new GroupBox
            {
                Text = "System Status",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = AppColors.PrimaryText,
                Width = 760,
                Height = 100,
                Location = new Point(0, 610)
            };

            CreateSystemStatusPanel(statusBox);

            dashboardPanel.Controls.Add(titleLabel);
            dashboardPanel.Controls.Add(welcomeLabel);
            dashboardPanel.Controls.Add(statsPanel);
            dashboardPanel.Controls.Add(recentActivityBox);
            dashboardPanel.Controls.Add(quickActionsBox);
            dashboardPanel.Controls.Add(statusBox);

            contentPanel.Controls.Add(dashboardPanel);
        }

        private void CreateStatCard(Panel parent, string title, string value, Color color, Point location)
        {
            Panel card = new Panel
            {
                Width = 180,
                Height = 100,
                Location = location,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            Panel colorBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = color
            };

            Label valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 160,
                Height = 40,
                Location = new Point(10, 20)
            };

            Label titleLabelCard = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10),
                ForeColor = AppColors.PrimaryText,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 160,
                Height = 30,
                Location = new Point(10, 65)
            };

            card.Controls.Add(colorBar);
            card.Controls.Add(valueLabel);
            card.Controls.Add(titleLabelCard);
            parent.Controls.Add(card);
        }

        private ListView CreateRecentActivityList()
        {
            ListView listView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };

            listView.Columns.Add("Type", 100);
            listView.Columns.Add("Description", 300);
            listView.Columns.Add("Date", 120);
            listView.Columns.Add("Status", 80);

            PopulateRecentActivity(listView);
            return listView;
        }

        private void PopulateRecentActivity(ListView listView)
        {
            try
            {
                List<ListViewItem> activities = new List<ListViewItem>();

                var recentIssues = IssueRepository.GetAllIssues()
                    .OrderByDescending(i => i.ReportedTime)
                    .Take(5)
                    .ToList();

                foreach (var issue in recentIssues)
                {
                    ListViewItem item = new ListViewItem("Issue");
                    item.SubItems.Add($"{issue.Category} at {issue.Location}");
                    item.SubItems.Add(issue.ReportedTime.ToString("MMM dd, HH:mm"));
                    item.SubItems.Add(issue.Status);

                    switch (issue.Status.ToLower())
                    {
                        case "new":
                            item.BackColor = Color.LightBlue;
                            break;
                        case "in progress":
                            item.BackColor = Color.LightYellow;
                            break;
                        case "resolved":
                            item.BackColor = Color.LightGreen;
                            break;
                    }

                    activities.Add(item);
                }

                var recentEvents = EventRepository.GetUpcomingEvents(3);
                foreach (var evt in recentEvents)
                {
                    ListViewItem item = new ListViewItem("Event");
                    item.SubItems.Add($"{evt.Title} at {evt.Location}");
                    item.SubItems.Add(evt.EventDate.ToString("MMM dd, HH:mm"));
                    item.SubItems.Add(evt.IsActive ? "Active" : "Inactive");

                    if (evt.EventDate.Date == DateTime.Now.Date)
                    {
                        item.BackColor = Color.LightCoral;
                    }

                    activities.Add(item);
                }

                foreach (var item in activities.OrderByDescending(i => i.SubItems[2].Text).Take(10))
                {
                    listView.Items.Add(item);
                }

                if (listView.Items.Count == 0)
                {
                    ListViewItem noActivityItem = new ListViewItem("No Activity");
                    noActivityItem.SubItems.Add("No recent activity found");
                    noActivityItem.SubItems.Add("-");
                    noActivityItem.SubItems.Add("-");
                    listView.Items.Add(noActivityItem);
                }
            }
            catch (Exception ex)
            {
                ListViewItem errorItem = new ListViewItem("Error");
                errorItem.SubItems.Add($"Error loading activity: {ex.Message}");
                errorItem.SubItems.Add("-");
                errorItem.SubItems.Add("-");
                listView.Items.Add(errorItem);
            }
        }
        private void CreateQuickActionButtons(GroupBox parent)
        {
            Button reportIssueBtn = new Button
            {
                Text = "Report Issue",
                Width = 140,
                Height = 50,
                Location = new Point(30, 30),
                BackColor = AppColors.SecondaryAccent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            reportIssueBtn.FlatAppearance.BorderSize = 0;
            reportIssueBtn.Click += (s, e) => ReportIssuesButton_Click(s, e);

            Button viewEventsBtn = new Button
            {
                Text = "View Events",
                Width = 140,
                Height = 50,
                Location = new Point(190, 30),
                BackColor = AppColors.InfoColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            viewEventsBtn.FlatAppearance.BorderSize = 0;
            viewEventsBtn.Click += (s, e) => EventsButton_Click(s, e);

            Button myIssuesBtn = new Button
            {
                Text = "My Issues",
                Width = 140,
                Height = 50,
                Location = new Point(350, 30),
                BackColor = AppColors.WarningColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            myIssuesBtn.FlatAppearance.BorderSize = 0;
            myIssuesBtn.Click += (s, e) => MyIssuesButton_Click(s, e);

            Button serviceReqBtn = new Button
            {
                Text = "Service Requests",
                Width = 140,
                Height = 50,
                Location = new Point(510, 30),
                BackColor = AppColors.SuccessColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            serviceReqBtn.FlatAppearance.BorderSize = 0;
            serviceReqBtn.Click += (s, e) => ServiceRequestsButton_Click(s, e);

            parent.Controls.Add(reportIssueBtn);
            parent.Controls.Add(viewEventsBtn);
            parent.Controls.Add(myIssuesBtn);
            parent.Controls.Add(serviceReqBtn);
        }

        private void CreateSystemStatusPanel(GroupBox parent)
        {
            Label dbStatusLabel = new Label
            {
                Text = "Database: " + (DatabaseManager.IsOfflineMode ? "OFFLINE" : "Connected"),
                Font = new Font("Segoe UI", 10),
                ForeColor = DatabaseManager.IsOfflineMode ? AppColors.DangerColor : AppColors.SuccessColor,
                Location = new Point(20, 25),
                AutoSize = true
            };

            Label userStatusLabel = new Label
            {
                Text = UserRepository.CurrentUser != null
                    ? $"Logged in as: {UserRepository.CurrentUser.FullName} ({UserRepository.CurrentUser.UserRole})"
                    : "Not logged in",
                Font = new Font("Segoe UI", 10),
                ForeColor = UserRepository.CurrentUser != null ? AppColors.SuccessColor : AppColors.WarningColor,
                Location = new Point(20, 50),
                AutoSize = true
            };

            Label lastUpdatedLabel = new Label
            {
                Text = $"Dashboard updated: {DateTime.Now.ToString("MMM dd, yyyy HH:mm")}",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(500, 50),
                AutoSize = true
            };

            parent.Controls.Add(dbStatusLabel);
            parent.Controls.Add(userStatusLabel);
            parent.Controls.Add(lastUpdatedLabel);
        }

        private void LoadMyIssuesContent()
        {
            contentPanel.Controls.Clear();
            SetActiveButton("myIssuesButton");

            Panel myIssuesPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            Label titleLabel = new Label
            {
                Text = "My Issues",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = AppColors.PrimaryText,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            Panel toolbarPanel = new Panel
            {
                Width = 800,
                Height = 50,
                Location = new Point(0, 40)
            };

            Button refreshButton = new Button
            {
                Text = "Refresh",
                Width = 100,
                Height = 35,
                Location = new Point(0, 5),
                BackColor = AppColors.InfoColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += (s, e) => LoadMyIssuesContent();

            Button newIssueButton = new Button
            {
                Text = "Report New Issue",
                Width = 150,
                Height = 35,
                Location = new Point(110, 5),
                BackColor = AppColors.ActionButton,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            newIssueButton.FlatAppearance.BorderSize = 0;
            newIssueButton.Click += (s, e) => ReportIssuesButton_Click(s, e);

            Label filterLabel = new Label
            {
                Text = "Filter by Status:",
                Location = new Point(280, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            ComboBox statusFilterCombo = new ComboBox
            {
                Width = 120,
                Location = new Point(380, 8),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            statusFilterCombo.Items.AddRange(new string[] { "All", "New", "In Progress", "Resolved", "Closed" });
            statusFilterCombo.SelectedIndex = 0;
            statusFilterCombo.SelectedIndexChanged += (s, e) => FilterMyIssues(statusFilterCombo.SelectedItem.ToString());

            toolbarPanel.Controls.Add(refreshButton);
            toolbarPanel.Controls.Add(newIssueButton);
            toolbarPanel.Controls.Add(filterLabel);
            toolbarPanel.Controls.Add(statusFilterCombo);

            ListView issuesListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Width = 800,
                Height = 400,
                Location = new Point(0, 100),
                Name = "myIssuesListView"
            };

            issuesListView.Columns.Add("ID", 50);
            issuesListView.Columns.Add("Category", 100);
            issuesListView.Columns.Add("Location", 150);
            issuesListView.Columns.Add("Description", 200);
            issuesListView.Columns.Add("Status", 80);
            issuesListView.Columns.Add("Priority", 70);
            issuesListView.Columns.Add("Date Reported", 120);

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem viewDetailsItem = new ToolStripMenuItem("View Details");
            viewDetailsItem.Click += ViewIssueDetails;
            contextMenu.Items.Add(viewDetailsItem);
            issuesListView.ContextMenuStrip = contextMenu;

            issuesListView.DoubleClick += ViewIssueDetails;

            PopulateMyIssues(issuesListView);

            Panel summaryPanel = new Panel
            {
                Width = 800,
                Height = 80,
                Location = new Point(0, 520),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label summaryLabel = new Label
            {
                Text = GetMyIssuesSummary(),
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 10),
                Width = 780,
                Height = 60
            };

            summaryPanel.Controls.Add(summaryLabel);

            myIssuesPanel.Controls.Add(titleLabel);
            myIssuesPanel.Controls.Add(toolbarPanel);
            myIssuesPanel.Controls.Add(issuesListView);
            myIssuesPanel.Controls.Add(summaryPanel);

            contentPanel.Controls.Add(myIssuesPanel);
        }

        private void PopulateMyIssues(ListView listView, string statusFilter = "All")
        {
            try
            {
                listView.Items.Clear();

                if (UserRepository.CurrentUser == null)
                {
                    ListViewItem noUserItem = new ListViewItem("N/A");
                    noUserItem.SubItems.Add("Please log in to view your issues");
                    listView.Items.Add(noUserItem);
                    return;
                }

                var userIssues = IssueRepository.GetIssuesByUserID(UserRepository.CurrentUser.UserID);

                if (statusFilter != "All")
                {
                    userIssues = userIssues.Where(i => i.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                foreach (var issue in userIssues.OrderByDescending(i => i.ReportedTime))
                {
                    ListViewItem item = new ListViewItem(issue.IssueID.ToString());
                    item.SubItems.Add(issue.Category);
                    item.SubItems.Add(issue.Location);
                    item.SubItems.Add(issue.Description.Length > 50 ? issue.Description.Substring(0, 47) + "..." : issue.Description);
                    item.SubItems.Add(issue.Status);
                    item.SubItems.Add(issue.Priority);
                    item.SubItems.Add(issue.ReportedTime.ToString("MMM dd, yyyy"));

                    item.BackColor = AppColors.GetStatusColor(issue.Status);
                    if (item.BackColor != AppColors.PrimaryText)
                    {
                        item.BackColor = Color.FromArgb(240, item.BackColor.R, item.BackColor.G, item.BackColor.B);
                    }

                    item.Tag = issue;
                    listView.Items.Add(item);
                }

                if (listView.Items.Count == 0)
                {
                    ListViewItem noIssuesItem = new ListViewItem("No Issues");
                    noIssuesItem.SubItems.Add("You haven't reported any issues yet");
                    listView.Items.Add(noIssuesItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading issues: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterMyIssues(string status)
        {
            ListView listView = (ListView)contentPanel.Controls.Find("myIssuesListView", true).FirstOrDefault();
            if (listView != null)
            {
                PopulateMyIssues(listView, status);
            }
        }

        private void ViewIssueDetails(object sender, EventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView == null && sender is ToolStripMenuItem menuItem && menuItem.Owner is ContextMenuStrip contextMenu)
            {
                listView = contextMenu.SourceControl as ListView;
            }

            if (listView?.SelectedItems.Count > 0)
            {
                var selectedIssue = listView.SelectedItems[0].Tag as Issue;
                if (selectedIssue != null)
                {
                    ShowIssueDetailsDialog(selectedIssue);
                }
            }
        }

        private string GetMyIssuesSummary()
        {
            if (UserRepository.CurrentUser == null)
                return "Please log in to view issue statistics.";

            try
            {
                var issues = IssueRepository.GetIssuesByUserID(UserRepository.CurrentUser.UserID);
                var newCount = issues.Count(i => i.Status == "New");
                var inProgressCount = issues.Count(i => i.Status == "In Progress");
                var resolvedCount = issues.Count(i => i.Status == "Resolved");

                return $"Total Issues: {issues.Count} | New: {newCount} | In Progress: {inProgressCount} | Resolved: {resolvedCount}";
            }
            catch
            {
                return "Error loading issue statistics.";
            }
        }

        private void ShowIssueDetailsDialog(Issue issue)
        {
            string details = $"Issue #{issue.IssueID}\n" +
                            $"Category: {issue.Category}\n" +
                            $"Location: {issue.Location}\n" +
                            $"Description: {issue.Description}\n" +
                            $"Status: {issue.Status}\n" +
                            $"Priority: {issue.Priority}\n" +
                            $"Reporter: {issue.ReporterName ?? "Anonymous"}\n" +
                            $"Reported: {issue.ReportedTime}\n" +
                            $"Assigned To: {issue.AssignedToName ?? "Unassigned"}";

            MessageBox.Show(details, "Issue Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void LoadServiceRequestsContent()
        {
            contentPanel.Controls.Clear();
            SetActiveButton("serviceRequestsButton");

            Panel serviceRequestsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            Label titleLabel = new Label
            {
                Text = "Service Requests",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = AppColors.PrimaryText,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            Panel toolbarPanel = new Panel
            {
                Width = 800,
                Height = 50,
                Location = new Point(0, 40)
            };

            Button refreshButton = new Button
            {
                Text = "Refresh",
                Width = 100,
                Height = 35,
                Location = new Point(0, 5),
                BackColor = AppColors.InfoColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += (s, e) => LoadServiceRequestsContent();

            Button newRequestButton = new Button
            {
                Text = "New Service Request",
                Width = 160,
                Height = 35,
                Location = new Point(110, 5),
                BackColor = AppColors.ActionButton,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            newRequestButton.FlatAppearance.BorderSize = 0;
            newRequestButton.Click += ShowNewServiceRequestDialog;

            toolbarPanel.Controls.Add(refreshButton);
            toolbarPanel.Controls.Add(newRequestButton);

            ListView requestsListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Width = 800,
                Height = 400,
                Location = new Point(0, 100),
                Name = "serviceRequestsListView"
            };

            requestsListView.Columns.Add("ID", 50);
            requestsListView.Columns.Add("Service Type", 150);
            requestsListView.Columns.Add("Description", 250);
            requestsListView.Columns.Add("Status", 100);
            requestsListView.Columns.Add("Submitted", 120);
            requestsListView.Columns.Add("Completed", 120);

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem viewDetailsItem = new ToolStripMenuItem("View Details");
            viewDetailsItem.Click += ViewServiceRequestDetails;
            contextMenu.Items.Add(viewDetailsItem);
            requestsListView.ContextMenuStrip = contextMenu;

            requestsListView.DoubleClick += ViewServiceRequestDetails;

            PopulateServiceRequests(requestsListView);

            serviceRequestsPanel.Controls.Add(titleLabel);
            serviceRequestsPanel.Controls.Add(toolbarPanel);
            serviceRequestsPanel.Controls.Add(requestsListView);

            contentPanel.Controls.Add(serviceRequestsPanel);
        }

        private void PopulateServiceRequests(ListView listView)
        {
            try
            {
                listView.Items.Clear();

                if (UserRepository.CurrentUser == null)
                {
                    ListViewItem noUserItem = new ListViewItem("N/A");
                    noUserItem.SubItems.Add("Please log in to view service requests");
                    listView.Items.Add(noUserItem);
                    return;
                }

                var requests = ServiceRequestRepository.GetServiceRequestsByUserID(UserRepository.CurrentUser.UserID);

                foreach (var request in requests.OrderByDescending(r => r.SubmissionDate))
                {
                    ListViewItem item = new ListViewItem(request.RequestID.ToString());
                    item.SubItems.Add(request.ServiceType);
                    item.SubItems.Add(request.Description.Length > 50 ? request.Description.Substring(0, 47) + "..." : request.Description);
                    item.SubItems.Add(request.Status);
                    item.SubItems.Add(request.SubmissionDate.ToString("MMM dd, yyyy"));
                    item.SubItems.Add(request.CompletionDate?.ToString("MMM dd, yyyy") ?? "-");

                    switch (request.Status.ToLower())
                    {
                        case "pending":
                            item.BackColor = Color.LightYellow;
                            break;
                        case "in process":
                            item.BackColor = Color.LightBlue;
                            break;
                        case "completed":
                            item.BackColor = Color.LightGreen;
                            break;
                    }

                    item.Tag = request;
                    listView.Items.Add(item);
                }

                if (listView.Items.Count == 0)
                {
                    ListViewItem noRequestsItem = new ListViewItem("No Requests");
                    noRequestsItem.SubItems.Add("You haven't made any service requests yet");
                    listView.Items.Add(noRequestsItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading service requests: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowNewServiceRequestDialog(object sender, EventArgs e)
        {
            using (ServiceRequestForm form = new ServiceRequestForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadServiceRequestsContent();
                }
            }
        }

        private void ViewServiceRequestDetails(object sender, EventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView == null && sender is ToolStripMenuItem menuItem && menuItem.Owner is ContextMenuStrip contextMenu)
            {
                listView = contextMenu.SourceControl as ListView;
            }

            if (listView?.SelectedItems.Count > 0)
            {
                var selectedRequest = listView.SelectedItems[0].Tag as ServiceRequest;
                if (selectedRequest != null)
                {
                    ShowServiceRequestDetailsDialog(selectedRequest);
                }
            }
        }

        private void ShowServiceRequestDetailsDialog(ServiceRequest request)
        {
            string details = $"Service Request #{request.RequestID}\n" +
                            $"Type: {request.ServiceType}\n" +
                            $"Description: {request.Description}\n" +
                            $"Status: {request.Status}\n" +
                            $"Submitted: {request.SubmissionDate}\n" +
                            $"Completed: {request.CompletionDate?.ToString() ?? "Not completed"}";

            MessageBox.Show(details, "Service Request Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadManageIssuesContent()
        {
            if (UserRepository.CurrentUser == null || !UserRepository.CurrentUser.IsStaff)
            {
                MessageBox.Show("You must be an administrator or staff member to manage issues.",
                    "Permission Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            contentPanel.Controls.Clear();
            SetActiveButton("manageIssuesButton");

            Panel manageIssuesPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            Label titleLabel = new Label
            {
                Text = "Manage Issues",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = AppColors.PrimaryText,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            Panel toolbarPanel = new Panel
            {
                Width = 800,
                Height = 50,
                Location = new Point(0, 40)
            };

            Button refreshButton = new Button
            {
                Text = "Refresh",
                Width = 100,
                Height = 35,
                Location = new Point(0, 5),
                BackColor = AppColors.InfoColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += (s, e) => LoadManageIssuesContent();

            Label filterLabel = new Label
            {
                Text = "Filter:",
                Location = new Point(120, 12),
                AutoSize = true
            };

            ComboBox statusFilter = new ComboBox
            {
                Width = 120,
                Location = new Point(160, 8),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "statusFilter"
            };
            statusFilter.Items.AddRange(new string[] { "All", "New", "In Progress", "Resolved", "Closed" });
            statusFilter.SelectedIndex = 0;

            ComboBox priorityFilter = new ComboBox
            {
                Width = 100,
                Location = new Point(290, 8),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "priorityFilter"
            };
            priorityFilter.Items.AddRange(new string[] { "All", "Low", "Medium", "High" });
            priorityFilter.SelectedIndex = 0;

            toolbarPanel.Controls.Add(refreshButton);
            toolbarPanel.Controls.Add(filterLabel);
            toolbarPanel.Controls.Add(statusFilter);
            toolbarPanel.Controls.Add(priorityFilter);

            ListView allIssuesListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Width = 800,
                Height = 400,
                Location = new Point(0, 100),
                Name = "allIssuesListView"
            };

            allIssuesListView.Columns.Add("ID", 40);
            allIssuesListView.Columns.Add("Reporter", 100);
            allIssuesListView.Columns.Add("Category", 100);
            allIssuesListView.Columns.Add("Location", 120);
            allIssuesListView.Columns.Add("Description", 150);
            allIssuesListView.Columns.Add("Status", 80);
            allIssuesListView.Columns.Add("Priority", 70);
            allIssuesListView.Columns.Add("Assigned To", 100);
            allIssuesListView.Columns.Add("Date", 100);

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem viewDetailsItem = new ToolStripMenuItem("View Details");
            viewDetailsItem.Click += ViewIssueDetails;

            ToolStripMenuItem changeStatusItem = new ToolStripMenuItem("Change Status");
            changeStatusItem.Click += ChangeIssueStatus;

            contextMenu.Items.Add(viewDetailsItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(changeStatusItem);

            allIssuesListView.ContextMenuStrip = contextMenu;

            PopulateAllIssues(allIssuesListView);

            Panel statsPanel = new Panel
            {
                Width = 800,
                Height = 60,
                Location = new Point(0, 520),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label statsLabel = new Label
            {
                Text = GetAllIssuesStatistics(),
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 10),
                Width = 780,
                Height = 40
            };

            statsPanel.Controls.Add(statsLabel);

            manageIssuesPanel.Controls.Add(titleLabel);
            manageIssuesPanel.Controls.Add(toolbarPanel);
            manageIssuesPanel.Controls.Add(allIssuesListView);
            manageIssuesPanel.Controls.Add(statsPanel);

            contentPanel.Controls.Add(manageIssuesPanel);
        }

        private void PopulateAllIssues(ListView listView)
        {
            try
            {
                listView.Items.Clear();

                var allIssues = IssueRepository.GetAllIssues();

                foreach (var issue in allIssues.OrderByDescending(i => i.ReportedTime))
                {
                    ListViewItem item = new ListViewItem(issue.IssueID.ToString());
                    item.SubItems.Add(issue.ReporterName ?? "Anonymous");
                    item.SubItems.Add(issue.Category);
                    item.SubItems.Add(issue.Location);
                    item.SubItems.Add(issue.Description.Length > 30 ? issue.Description.Substring(0, 27) + "..." : issue.Description);
                    item.SubItems.Add(issue.Status);
                    item.SubItems.Add(issue.Priority);
                    item.SubItems.Add(issue.AssignedToName ?? "Unassigned");
                    item.SubItems.Add(issue.ReportedTime.ToString("MMM dd"));

                    item.BackColor = AppColors.GetStatusColor(issue.Status);
                    if (item.BackColor != AppColors.PrimaryText)
                    {
                        item.BackColor = Color.FromArgb(240, item.BackColor.R, item.BackColor.G, item.BackColor.B);
                    }

                    item.Tag = issue;
                    listView.Items.Add(item);
                }

                if (listView.Items.Count == 0)
                {
                    ListViewItem noIssuesItem = new ListViewItem("No Issues");
                    noIssuesItem.SubItems.Add("No issues match the current filter");
                    listView.Items.Add(noIssuesItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading issues: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetAllIssuesStatistics()
        {
            try
            {
                var allIssues = IssueRepository.GetAllIssues();
                var newCount = allIssues.Count(i => i.Status == "New");
                var inProgressCount = allIssues.Count(i => i.Status == "In Progress");
                var resolvedCount = allIssues.Count(i => i.Status == "Resolved");
                var unassignedCount = allIssues.Count(i => !i.AssignedTo.HasValue);

                return $"Total: {allIssues.Count} | New: {newCount} | In Progress: {inProgressCount} | Resolved: {resolvedCount} | Unassigned: {unassignedCount}";
            }
            catch
            {
                return "Error loading statistics.";
            }
        }

        private void ChangeIssueStatus(object sender, EventArgs e)
        {
            ListView listView = null;
            if (sender is ToolStripMenuItem menuItem && menuItem.Owner is ContextMenuStrip contextMenu)
            {
                listView = contextMenu.SourceControl as ListView;
            }

            if (listView?.SelectedItems.Count > 0)
            {
                var selectedIssue = listView.SelectedItems[0].Tag as Issue;
                if (selectedIssue != null)
                {
                    MessageBox.Show($"Change status for Issue #{selectedIssue.IssueID}\nCurrent Status: {selectedIssue.Status}",
                        "Change Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void LoadManageUsersContent()
        {
            if (UserRepository.CurrentUser == null || !UserRepository.CurrentUser.IsAdmin)
            {
                MessageBox.Show("You must be an administrator to manage users.",
                    "Permission Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            contentPanel.Controls.Clear();
            SetActiveButton("manageUsersButton");

            Panel manageUsersPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            Label titleLabel = new Label
            {
                Text = "Manage Users",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = AppColors.PrimaryText,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            Panel toolbarPanel = new Panel
            {
                Width = 800,
                Height = 50,
                Location = new Point(0, 40)
            };

            Button refreshButton = new Button
            {
                Text = "Refresh",
                Width = 100,
                Height = 35,
                Location = new Point(0, 5),
                BackColor = AppColors.InfoColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += (s, e) => LoadManageUsersContent();

            Button addUserButton = new Button
            {
                Text = "Add User",
                Width = 100,
                Height = 35,
                Location = new Point(110, 5),
                BackColor = AppColors.ActionButton,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            addUserButton.FlatAppearance.BorderSize = 0;
            addUserButton.Click += ShowAddUserDialog;

            Label roleFilterLabel = new Label
            {
                Text = "Role:",
                Location = new Point(230, 12),
                AutoSize = true
            };

            ComboBox roleFilter = new ComboBox
            {
                Width = 100,
                Location = new Point(270, 8),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "roleFilter"
            };
            roleFilter.Items.AddRange(new string[] { "All", "Admin", "Staff", "User" });
            roleFilter.SelectedIndex = 0;

            toolbarPanel.Controls.Add(refreshButton);
            toolbarPanel.Controls.Add(addUserButton);
            toolbarPanel.Controls.Add(roleFilterLabel);
            toolbarPanel.Controls.Add(roleFilter);

            ListView usersListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Width = 800,
                Height = 400,
                Location = new Point(0, 100),
                Name = "usersListView"
            };

            usersListView.Columns.Add("ID", 50);
            usersListView.Columns.Add("Username", 120);
            usersListView.Columns.Add("Full Name", 150);
            usersListView.Columns.Add("Email", 180);
            usersListView.Columns.Add("Role", 80);
            usersListView.Columns.Add("Created", 100);
            usersListView.Columns.Add("Last Login", 100);

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem viewUserItem = new ToolStripMenuItem("View User Details");
            viewUserItem.Click += ViewUserDetails;

            ToolStripMenuItem editUserItem = new ToolStripMenuItem("Edit User");
            editUserItem.Click += EditUser;

            contextMenu.Items.Add(viewUserItem);
            contextMenu.Items.Add(editUserItem);

            usersListView.ContextMenuStrip = contextMenu;

            PopulateUsers(usersListView);

            Panel userStatsPanel = new Panel
            {
                Width = 800,
                Height = 60,
                Location = new Point(0, 520),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label userStatsLabel = new Label
            {
                Text = GetUserStatistics(),
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 10),
                Width = 780,
                Height = 40
            };

            userStatsPanel.Controls.Add(userStatsLabel);

            manageUsersPanel.Controls.Add(titleLabel);
            manageUsersPanel.Controls.Add(toolbarPanel);
            manageUsersPanel.Controls.Add(usersListView);
            manageUsersPanel.Controls.Add(userStatsPanel);

            contentPanel.Controls.Add(manageUsersPanel);
        }

        private void PopulateUsers(ListView listView)
        {
            try
            {
                listView.Items.Clear();

                if (DatabaseManager.IsOfflineMode)
                {
                    ShowOfflineUserManagement(listView);
                }
                else
                {
                    ListViewItem item = new ListViewItem("Feature");
                    item.SubItems.Add("User management requires additional database methods");
                    item.SubItems.Add("This will be implemented with proper user queries");
                    listView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowOfflineUserManagement(ListView listView)
        {
            var sampleUsers = new[]
            {
                new { ID = 1, Username = "admin", FullName = "System Administrator", Email = "admin@communityapp.org", Role = "Admin", Created = DateTime.Now.AddDays(-30), LastLogin = DateTime.Now.AddHours(-2) },
                new { ID = 2, Username = "staff", FullName = "Staff Member", Email = "staff@communityapp.org", Role = "Staff", Created = DateTime.Now.AddDays(-25), LastLogin = DateTime.Now.AddHours(-5) },
                new { ID = 3, Username = "user", FullName = "Regular User", Email = "user@example.com", Role = "User", Created = DateTime.Now.AddDays(-10), LastLogin = DateTime.Now.AddHours(-1) }
            };

            foreach (var user in sampleUsers)
            {
                ListViewItem item = new ListViewItem(user.ID.ToString());
                item.SubItems.Add(user.Username);
                item.SubItems.Add(user.FullName);
                item.SubItems.Add(user.Email);
                item.SubItems.Add(user.Role);
                item.SubItems.Add(user.Created.ToString("MMM dd, yyyy"));
                item.SubItems.Add(user.LastLogin == default(DateTime) ? "Never" : user.LastLogin.ToString("MMM dd, HH:mm"));

                switch (user.Role)
                {
                    case "Admin":
                        item.BackColor = Color.LightCoral;
                        break;
                    case "Staff":
                        item.BackColor = Color.LightBlue;
                        break;
                    default:
                        item.BackColor = Color.White;
                        break;
                }

                item.Tag = user;
                listView.Items.Add(item);
            }
        }

        private string GetUserStatistics()
        {
            try
            {
                return "Total Users: 3 | Admins: 1 | Staff: 1 | Regular Users: 1 | Active in last 24h: 3";
            }
            catch
            {
                return "Error loading user statistics.";
            }
        }

        private void ShowAddUserDialog(object sender, EventArgs e)
        {
            MessageBox.Show("Add User functionality will open a registration form for admins.",
                "Add User", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ViewUserDetails(object sender, EventArgs e)
        {
            MessageBox.Show("View User Details functionality will show comprehensive user information.",
                "User Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EditUser(object sender, EventArgs e)
        {
            MessageBox.Show("Edit User functionality will allow modifying user profile information.",
                "Edit User", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        // Service Request Form
        public class ServiceRequestForm : Form
        {
            private ComboBox serviceTypeComboBox;
            private TextBox descriptionTextBox;
            private Button submitButton;
            private Button cancelButton;

            public ServiceRequestForm()
            {
                InitializeComponents();
            }

            private void InitializeComponents()
            {
                this.Text = "New Service Request";
                this.Size = new Size(500, 400);
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;

                Label serviceTypeLabel = new Label
                {
                    Text = "Service Type:",
                    Location = new Point(20, 20),
                    AutoSize = true
                };

                serviceTypeComboBox = new ComboBox
                {
                    Location = new Point(20, 45),
                    Width = 400,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                serviceTypeComboBox.Items.AddRange(new string[]
                {
                    "Trash Collection",
                    "Street Light Repair",
                    "Tree Trimming",
                    "Road Maintenance",
                    "Water Issues",
                    "Electrical Issues",
                    "Building Permits",
                    "Other"
                });

                Label descriptionLabel = new Label
                {
                    Text = "Description:",
                    Location = new Point(20, 85),
                    AutoSize = true
                };

                descriptionTextBox = new TextBox
                {
                    Location = new Point(20, 110),
                    Width = 400,
                    Height = 150,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical
                };

                submitButton = new Button
                {
                    Text = "Submit Request",
                    Location = new Point(250, 300),
                    Width = 120,
                    Height = 35,
                    BackColor = AppColors.ActionButton,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                submitButton.Click += SubmitButton_Click;

                cancelButton = new Button
                {
                    Text = "Cancel",
                    Location = new Point(380, 300),
                    Width = 80,
                    Height = 35,
                    DialogResult = DialogResult.Cancel
                };

                this.Controls.Add(serviceTypeLabel);
                this.Controls.Add(serviceTypeComboBox);
                this.Controls.Add(descriptionLabel);
                this.Controls.Add(descriptionTextBox);
                this.Controls.Add(submitButton);
                this.Controls.Add(cancelButton);

                this.AcceptButton = submitButton;
                this.CancelButton = cancelButton;
            }

            private void SubmitButton_Click(object sender, EventArgs e)
            {
                if (serviceTypeComboBox.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a service type.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(descriptionTextBox.Text))
                {
                    MessageBox.Show("Please provide a description.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    ServiceRequest newRequest = new ServiceRequest
                    {
                        UserID = UserRepository.CurrentUser.UserID,
                        ServiceType = serviceTypeComboBox.SelectedItem.ToString(),
                        Description = descriptionTextBox.Text.Trim(),
                        SubmissionDate = DateTime.Now,
                        Status = "Pending"
                    };

                    int requestId = ServiceRequestRepository.AddServiceRequest(newRequest);

                    if (requestId > 0)
                    {
                        MessageBox.Show("Service request submitted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to submit service request. Please try again.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error submitting request: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}