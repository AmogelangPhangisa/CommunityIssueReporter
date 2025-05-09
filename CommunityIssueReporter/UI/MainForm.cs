using System;
using System.Drawing;
using System.Windows.Forms;
using CommunityIssueReporter.Data;
using CommunityIssueReporter.Utilities;

namespace CommunityIssueReporter.UI
{
    public class MainForm : Form  // Removed "partial"
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
                Cursor = Cursors.Hand  // Add cursor to show it's clickable
            };
            // Add click event handler
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
                BackColor = Color.FromArgb(108, 117, 125), // Bootstrap Secondary
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
                // Keep the hand cursor so users know they can click for more info
                userStatusLabel.Cursor = Cursors.Hand;

                loginButton.Text = "Logout";
            }
            else
            {
                userStatusLabel.Text = "Not Logged In - Click to Login";
                userStatusLabel.ForeColor = Color.LightYellow; // Highlight to make more noticeable
                userStatusLabel.Cursor = Cursors.Hand;

                loginButton.Text = "Login";
            }

            // Refresh current content
            CreateWelcomeContent();
        }

        #region Event Handlers

        private void UserStatusLabel_Click(object sender, EventArgs e)
        {
            // If user is not logged in, show login form
            if (UserRepository.CurrentUser == null)
            {
                // Use the same login button click handler
                LoginButton_Click(this.Controls.Find("loginButton", true)[0], EventArgs.Empty);
            }
            else
            {
                // Later you could add user profile functionality here
                // For now, just show a message
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
                    // Show login form
                    using (LoginForm loginForm = new LoginForm(this))
                    {
                        this.Hide();
                        DialogResult result = loginForm.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            // User logged in successfully
                            UpdateUIForLoginStatus();
                        }

                        this.Show();
                    }
                }
                else
                {
                    // Logout user
                    DialogResult result = MessageBox.Show(
                        "Are you sure you want to log out?",
                        "Confirm Logout",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        UserRepository.LogoutUser();
                        UpdateUIForLoginStatus();
                    }
                }
            }
            else
            {
                // Called from UserStatusLabel_Click or programmatically
                if (UserRepository.CurrentUser == null)
                {
                    // Show login form
                    using (LoginForm loginForm = new LoginForm(this))
                    {
                        this.Hide();
                        DialogResult result = loginForm.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            // User logged in successfully
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
            // Check if user is logged in
            if (UserRepository.CurrentUser == null)
            {
                DialogResult result = MessageBox.Show(
                    "You must be logged in to report issues. Would you like to log in now?",
                    "Login Required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    LoginButton_Click(this.Controls.Find("loginButton", true)[0], EventArgs.Empty);

                    // If user is now logged in, continue to report issues
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
            // Check if user is logged in
            if (UserRepository.CurrentUser == null)
            {
                DialogResult result = MessageBox.Show(
                    "You must be logged in to view your issues. Would you like to log in now?",
                    "Login Required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    LoginButton_Click(this.Controls.Find("loginButton", true)[0], EventArgs.Empty);

                    // If user is now logged in, continue to my issues
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
            // Check if user is logged in
            if (UserRepository.CurrentUser == null)
            {
                DialogResult result = MessageBox.Show(
                    "You must be logged in to make service requests. Would you like to log in now?",
                    "Login Required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    LoginButton_Click(this.Controls.Find("loginButton", true)[0], EventArgs.Empty);

                    // If user is now logged in, continue to service requests
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
            // Clear existing content
            contentPanel.Controls.Clear();

            // Set active button
            SetActiveButton("dashboardButton");

            // Create dashboard content panel
            Panel dashboardPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Add title
            Label titleLabel = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = AppColors.PrimaryText,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Add dashboard content
            // This can be expanded to include charts, statistics, etc.
            Label contentLabel = new Label
            {
                Text = "Dashboard content will be implemented in a future update.",
                Font = new Font("Segoe UI", 12),
                ForeColor = AppColors.PrimaryText,
                AutoSize = true,
                Location = new Point(20, 80)
            };

            dashboardPanel.Controls.Add(titleLabel);
            dashboardPanel.Controls.Add(contentLabel);

            // Add dashboard panel to content panel
            contentPanel.Controls.Add(dashboardPanel);
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

        private void LoadMyIssuesContent()
        {
            // To be implemented
            MessageBox.Show("My Issues will be implemented in a future update.");
        }

        private void LoadEventsContent()
        {
            // Check if user is logged in for full functionality
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

            // Open the Events form regardless of login status
            int currentUserID = UserRepository.CurrentUser?.UserID ?? -1;
            string currentUserRole = UserRepository.CurrentUser?.UserRole ?? "Guest";

            this.Hide();
            using (EventsForm eventsForm = new EventsForm(currentUserID, currentUserRole))
            {
                eventsForm.ShowDialog();
            }
            this.Show();
        }

        private void LoadServiceRequestsContent()
        {
            // To be implemented
            MessageBox.Show("Service Requests will be implemented in a future update.");
        }

        private void LoadManageIssuesContent()
        {
            // To be implemented
            MessageBox.Show("Manage Issues will be implemented in a future update.");
        }

        private void LoadManageEventsContent()
        {
            // Check if user has proper permissions
            if (UserRepository.CurrentUser == null ||
                (UserRepository.CurrentUser.UserRole != "Admin" && UserRepository.CurrentUser.UserRole != "Staff"))
            {
                MessageBox.Show("You must be an administrator or staff member to manage events.",
                    "Permission Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Open the Events form with the management tab active
            int currentUserID = UserRepository.CurrentUser.UserID;
            string currentUserRole = UserRepository.CurrentUser.UserRole;

            this.Hide();
            using (EventsForm eventsForm = new EventsForm(currentUserID, currentUserRole, true))
            {
                eventsForm.ShowDialog();
            }
            this.Show();
        }

        private void LoadManageUsersContent()
        {
            // To be implemented
            MessageBox.Show("Manage Users will be implemented in a future update.");
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

        #endregion
    }
}