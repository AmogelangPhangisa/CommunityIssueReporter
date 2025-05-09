using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using CommunityIssueReporter.Data;
using CommunityIssueReporter.Models;
using CommunityIssueReporter.Utilities;

namespace CommunityIssueReporter.UI
{
    public class EventsForm : Form
    {
        // UI Controls
        private TabControl eventsTabControl;
        private TabPage upcomingEventsTab;
        private TabPage myEventsTab;
        private TabPage manageEventsTab;
        private ListView upcomingEventsListView;
        private ListView myEventsListView;
        private ListView manageEventsListView;
        private Button createEventButton;
        private Button refreshButton;
        private Button advancedSearchButton;
        private Label offlineModeLabel;

        // Current user info
        private int _currentUserID;
        private string _currentUserRole;

        public EventsForm(int currentUserID, string currentUserRole, bool showManagementTab = false)
        {
            _currentUserID = currentUserID;
            _currentUserRole = currentUserRole;

            InitializeComponents();
            LoadEvents();

            // Initialize sample data in offline mode
            if (DatabaseManager.IsOfflineMode)
            {
                EventRepository.InitializeOfflineEvents();
            }

            // Select the Management tab if requested
            if (showManagementTab && (_currentUserRole == "Admin" || _currentUserRole == "Staff"))
            {
                eventsTabControl.SelectedTab = manageEventsTab;
            }
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = "Local Events and Announcements";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = new Icon(SystemIcons.Information, 40, 40);

            // Create main layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Top panel for buttons
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            // Create event button
            createEventButton = new Button
            {
                Text = "Create New Event",
                Width = 150,
                Height = 30,
                Left = 10,
                Top = 5,
                BackColor = AppColors.ActionButton,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            createEventButton.FlatAppearance.BorderSize = 0;
            createEventButton.Click += CreateEventButton_Click;

            // Refresh button
            refreshButton = new Button
            {
                Text = "Refresh",
                Width = 100,
                Height = 30,
                Left = 170,
                Top = 5,
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += RefreshButton_Click;

            // Advanced search button
            advancedSearchButton = new Button
            {
                Text = "Advanced Search",
                Width = 150,
                Height = 30,
                Left = 280, // Position after the Refresh button
                Top = 5,
                BackColor = Color.LightBlue,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            advancedSearchButton.FlatAppearance.BorderSize = 0;
            advancedSearchButton.Click += AdvancedSearchButton_Click;

            // Offline mode label
            offlineModeLabel = new Label
            {
                Text = "OFFLINE MODE - Data will not be saved permanently",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleRight,
                Width = 350,
                Height = 30,
                Left = this.ClientSize.Width - 360,
                Top = 5,
                Visible = DatabaseManager.IsOfflineMode
            };

            topPanel.Controls.Add(createEventButton);
            topPanel.Controls.Add(refreshButton);
            topPanel.Controls.Add(advancedSearchButton);
            topPanel.Controls.Add(offlineModeLabel);

            // Tab control for different event views
            eventsTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9)
            };

            // Upcoming events tab
            upcomingEventsTab = new TabPage("Upcoming Events");
            upcomingEventsListView = CreateEventsListView();
            upcomingEventsTab.Controls.Add(upcomingEventsListView);

            // My events tab
            myEventsTab = new TabPage("My Events");
            myEventsListView = CreateEventsListView();
            myEventsTab.Controls.Add(myEventsListView);

            // Manage events tab (only visible for admin/staff)
            manageEventsTab = new TabPage("Manage Events");
            manageEventsListView = CreateEventsListView(true); // Include management options
            manageEventsTab.Controls.Add(manageEventsListView);

            // Add tabs to tab control
            eventsTabControl.TabPages.Add(upcomingEventsTab);
            eventsTabControl.TabPages.Add(myEventsTab);

            // Only show management tab for admin or staff
            if (_currentUserRole == "Admin" || _currentUserRole == "Staff")
            {
                eventsTabControl.TabPages.Add(manageEventsTab);
            }

            // Add controls to main layout
            mainLayout.Controls.Add(topPanel, 0, 0);
            mainLayout.Controls.Add(eventsTabControl, 0, 1);

            // Add layout to form
            this.Controls.Add(mainLayout);

            // Set up event handlers for list views
            upcomingEventsListView.DoubleClick += EventsListView_DoubleClick;
            myEventsListView.DoubleClick += EventsListView_DoubleClick;
            manageEventsListView.DoubleClick += EventsListView_DoubleClick;
        }

        private ListView CreateEventsListView(bool includeManagement = false)
        {
            ListView listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                HideSelection = false
            };

            // Add columns
            listView.Columns.Add("ID", 40);
            listView.Columns.Add("Title", 200);
            listView.Columns.Add("Date & Time", 150);
            listView.Columns.Add("Location", 150);
            listView.Columns.Add("Category", 100);
            listView.Columns.Add("Created By", 120);

            if (includeManagement)
            {
                listView.Columns.Add("Status", 80);
                listView.Columns.Add("Registrations", 80);
            }

            return listView;
        }

        private void LoadEvents()
        {
            try
            {
                // Clear existing items
                upcomingEventsListView.Items.Clear();
                myEventsListView.Items.Clear();
                manageEventsListView.Items.Clear();

                // Load upcoming events
                List<Event> upcomingEvents = EventRepository.GetUpcomingEvents();
                foreach (var evt in upcomingEvents)
                {
                    ListViewItem item = CreateEventListViewItem(evt);
                    upcomingEventsListView.Items.Add(item);
                }

                // Load user's events
                List<Event> myEvents = EventRepository.GetEventsByUser(_currentUserID);
                foreach (var evt in myEvents)
                {
                    ListViewItem item = CreateEventListViewItem(evt);
                    myEventsListView.Items.Add(item);
                }

                // Load all events for management (admin/staff only)
                if (_currentUserRole == "Admin" || _currentUserRole == "Staff")
                {
                    List<Event> allEvents = EventRepository.GetAllEvents(false);
                    foreach (var evt in allEvents)
                    {
                        ListViewItem item = CreateEventListViewItem(evt, true);
                        manageEventsListView.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading events: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private ListViewItem CreateEventListViewItem(Event evt, bool includeManagement = false)
        {
            ListViewItem item = new ListViewItem(evt.EventID.ToString());
            item.SubItems.Add(evt.Title);
            item.SubItems.Add(evt.EventDate.ToString("MMM dd, yyyy h:mm tt"));
            item.SubItems.Add(evt.Location);
            item.SubItems.Add(evt.Category);
            item.SubItems.Add(evt.CreatedByName);

            if (includeManagement)
            {
                item.SubItems.Add(evt.IsActive ? "Active" : "Inactive");

                // Get registration count
                List<EventRegistration> registrations = EventRepository.GetEventRegistrations(evt.EventID);
                item.SubItems.Add(registrations.Count.ToString());
            }

            // Store the event object in the tag for easy access
            item.Tag = evt;

            // Set background color based on event date
            if (evt.EventDate < DateTime.Now)
            {
                item.BackColor = Color.LightGray; // Past event
            }
            else if (evt.EventDate.Date == DateTime.Now.Date)
            {
                item.BackColor = Color.LightYellow; // Today's event
            }

            return item;
        }

        private void CreateEventButton_Click(object sender, EventArgs e)
        {
            EventDetailsForm detailsForm = new EventDetailsForm(_currentUserID, -1);
            if (detailsForm.ShowDialog() == DialogResult.OK)
            {
                LoadEvents();
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadEvents();
        }

        private void AdvancedSearchButton_Click(object sender, EventArgs e)
        {
            // Open the advanced search form
            EventSearchForm searchForm = new EventSearchForm(_currentUserID, _currentUserRole);

            // Show the form as a dialog
            if (searchForm.ShowDialog() == DialogResult.OK)
            {
                // Refresh the event list after closing the search form
                LoadEvents();
            }
        }

        private void EventsListView_DoubleClick(object sender, EventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem selected = listView.SelectedItems[0];
                Event selectedEvent = (Event)selected.Tag;

                // Open event details form
                EventDetailsForm detailsForm = new EventDetailsForm(_currentUserID, selectedEvent.EventID);
                if (detailsForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh if changes were made
                    LoadEvents();
                }
            }
        }
    }

    // Event Details Form to view/edit event details and register
    public class EventDetailsForm : Form
    {
        // UI controls
        private TextBox titleTextBox;
        private DateTimePicker eventDatePicker;
        private DateTimePicker eventTimePicker;
        private DateTimePicker endDatePicker;
        private DateTimePicker endTimePicker;
        private TextBox locationTextBox;
        private ComboBox categoryComboBox;
        private TextBox descriptionTextBox;
        private PictureBox eventImagePictureBox;
        private Button browseImageButton;
        private CheckBox isActiveCheckBox;
        private Button saveButton;
        private Button cancelButton;
        private Button registerButton;
        private Button cancelRegistrationButton;
        private ListView registrationsListView;
        private Label registrationsLabel;

        // Event data
        private int _eventID;
        private int _currentUserID;
        private Event _event;
        private string _imagePath;
        private bool _isRegistered;
        private bool _isEditable;
        private List<EventRegistration> _registrations;

        public EventDetailsForm(int currentUserID, int eventID)
        {
            _currentUserID = currentUserID;
            _eventID = eventID;

            // Load event data
            if (_eventID > 0)
            {
                _event = EventRepository.GetEventByID(_eventID);
                if (_event == null)
                {
                    MessageBox.Show("Event not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }

                // Check if user is creator or has admin/staff role
                _isEditable = (_event.CreatedByUserID == _currentUserID) ||
                              (UserRepository.GetUserByID(_currentUserID)?.UserRole == "Admin") ||
                              (UserRepository.GetUserByID(_currentUserID)?.UserRole == "Staff");

                // Load registrations
                _registrations = EventRepository.GetEventRegistrations(_eventID);

                // Check if user is registered
                _isRegistered = _registrations.Exists(r => r.UserID == _currentUserID);
            }
            else
            {
                // New event
                _event = new Event
                {
                    EventDate = DateTime.Now.AddDays(7).Date.AddHours(18), // Default to a week from now at 6 PM
                    EndDate = DateTime.Now.AddDays(7).Date.AddHours(20),   // Default to 2 hours duration
                    IsActive = true,
                    CreatedByUserID = _currentUserID,
                    CreatedDate = DateTime.Now
                };

                _isEditable = true;
                _registrations = new List<EventRegistration>();
            }

            InitializeComponents();
            LoadEventData();
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = (_eventID > 0) ? "Event Details" : "Create New Event";
            this.Size = new Size(750, 750); // Increase form height
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Use a flow layout approach instead of overlapping panels
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            // Create a single-column layout for all form elements
            FlowLayoutPanel flowLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Width = 680,
                Height = 650,
                AutoSize = true,
                AutoScroll = true,
                WrapContents = false
            };

            // Title field
            flowLayout.Controls.Add(CreateHeaderLabel("Title:"));
            titleTextBox = new TextBox
            {
                Width = 650,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(5, 3, 5, 15),
                Enabled = _isEditable
            };
            flowLayout.Controls.Add(titleTextBox);

            // Event Date & Time
            flowLayout.Controls.Add(CreateHeaderLabel("Event Date & Time:"));
            Panel dateTimePanel = new Panel
            {
                Width = 650,
                Height = 35,
                Margin = new Padding(5, 3, 5, 15)
            };
            eventDatePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 150,
                Top = 0,
                Left = 0,
                Enabled = _isEditable
            };
            eventTimePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                Width = 150,
                Top = 0,
                Left = 200,
                ShowUpDown = true,
                Enabled = _isEditable
            };
            dateTimePanel.Controls.Add(eventDatePicker);
            dateTimePanel.Controls.Add(eventTimePicker);
            flowLayout.Controls.Add(dateTimePanel);

            // End Date & Time
            flowLayout.Controls.Add(CreateHeaderLabel("End Date & Time:"));
            Panel endDateTimePanel = new Panel
            {
                Width = 650,
                Height = 35,
                Margin = new Padding(5, 3, 5, 15)
            };
            endDatePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 150,
                Top = 0,
                Left = 0,
                Enabled = _isEditable
            };
            endTimePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                Width = 150,
                Top = 0,
                Left = 200,
                ShowUpDown = true,
                Enabled = _isEditable
            };
            endDateTimePanel.Controls.Add(endDatePicker);
            endDateTimePanel.Controls.Add(endTimePicker);
            flowLayout.Controls.Add(endDateTimePanel);

            // Location
            flowLayout.Controls.Add(CreateHeaderLabel("Location:"));
            locationTextBox = new TextBox
            {
                Width = 650,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(5, 3, 5, 15),
                Enabled = _isEditable
            };
            flowLayout.Controls.Add(locationTextBox);

            // Category
            flowLayout.Controls.Add(CreateHeaderLabel("Category:"));
            categoryComboBox = new ComboBox
            {
                Width = 650,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(5, 3, 5, 15),
                Enabled = _isEditable
            };
            categoryComboBox.Items.AddRange(new string[]
            {
            "Community Service",
            "Government",
            "Safety",
            "Recreation",
            "Education",
            "Health",
            "Environment",
            "Social",
            "Other"
            });
            flowLayout.Controls.Add(categoryComboBox);

            // Description
            flowLayout.Controls.Add(CreateHeaderLabel("Description:"));
            descriptionTextBox = new TextBox
            {
                Width = 650,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(5, 3, 5, 15),
                Enabled = _isEditable
            };
            flowLayout.Controls.Add(descriptionTextBox);

            // Event Image
            flowLayout.Controls.Add(CreateHeaderLabel("Event Image:"));
            Panel imagePanel = new Panel
            {
                Width = 650,
                Height = 110,
                Margin = new Padding(5, 3, 5, 15)
            };
            eventImagePictureBox = new PictureBox
            {
                Width = 150,
                Height = 100,
                Left = 0,
                Top = 0,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };
            browseImageButton = new Button
            {
                Text = "Browse...",
                Width = 100,
                Height = 30,
                Left = 170,
                Top = 35,
                Enabled = _isEditable,
                BackColor = SystemColors.Control
            };
            browseImageButton.Click += BrowseImageButton_Click;
            imagePanel.Controls.Add(eventImagePictureBox);
            imagePanel.Controls.Add(browseImageButton);
            flowLayout.Controls.Add(imagePanel);

            // Status (for existing events)
            if (_eventID > 0)
            {
                isActiveCheckBox = new CheckBox
                {
                    Text = "Event is active",
                    Width = 650,
                    Checked = true,
                    Margin = new Padding(5, 3, 5, 15),
                    Enabled = _isEditable
                };
                flowLayout.Controls.Add(isActiveCheckBox);
            }
            else
            {
                isActiveCheckBox = new CheckBox { Checked = true, Visible = false };
            }

            // Buttons panel
            Panel buttonPanel = new Panel
            {
                Width = 650,
                Height = 50,
                Margin = new Padding(5, 20, 5, 0)
            };

            // Create buttons
            saveButton = new Button
            {
                Text = "Save Event",
                Width = 120,
                Height = 35,
                Left = buttonPanel.Width - 240,
                Top = 0,
                BackColor = AppColors.ActionButton,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = _isEditable
            };
            saveButton.Click += SaveButton_Click;

            cancelButton = new Button
            {
                Text = "Cancel",
                Width = 100,
                Height = 35,
                Left = buttonPanel.Width - 110,
                Top = 0,
                DialogResult = DialogResult.Cancel
            };

            // Add buttons to panel
            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(cancelButton);

            // For existing events, add registration buttons
            if (_eventID > 0)
            {
                registerButton = new Button
                {
                    Text = _isRegistered ? "Already Registered" : "Register for Event",
                    Width = 160,
                    Height = 35,
                    Left = 0,
                    Top = 0,
                    BackColor = _isRegistered ? Color.LightGray : AppColors.ActionButton,
                    ForeColor = Color.White,
                    Enabled = !_isRegistered && _event.IsActive && _event.EventDate > DateTime.Now
                };
                registerButton.Click += RegisterButton_Click;

                cancelRegistrationButton = new Button
                {
                    Text = "Cancel Registration",
                    Width = 160,
                    Height = 35,
                    Left = 170,
                    Top = 0,
                    BackColor = Color.IndianRed,
                    ForeColor = Color.White,
                    Visible = _isRegistered,
                    Enabled = _isRegistered && _event.EventDate > DateTime.Now
                };
                cancelRegistrationButton.Click += CancelRegistrationButton_Click;

                buttonPanel.Controls.Add(registerButton);
                buttonPanel.Controls.Add(cancelRegistrationButton);
            }

            // Add button panel to flow layout
            flowLayout.Controls.Add(buttonPanel);

            // Initialize registrations controls to null by default
            registrationsLabel = null;
            registrationsListView = null;

            // Add registrations section for admins/creators of existing events
            if (_isEditable && _eventID > 0)
            {
                Panel registrationsHeader = new Panel
                {
                    Width = 650,
                    Height = 30,
                    Margin = new Padding(5, 20, 5, 5)
                };

                registrationsLabel = new Label
                {
                    Text = "Registrations",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(0, 0)
                };

                registrationsHeader.Controls.Add(registrationsLabel);
                flowLayout.Controls.Add(registrationsHeader);

                registrationsListView = new ListView
                {
                    Width = 650,
                    Height = 150,
                    View = View.Details,
                    FullRowSelect = true,
                    GridLines = true,
                    Margin = new Padding(5, 0, 5, 15)
                };

                registrationsListView.Columns.Add("ID", 50);
                registrationsListView.Columns.Add("User", 170);
                registrationsListView.Columns.Add("Registration Date", 180);
                registrationsListView.Columns.Add("Status", 100);

                // Add context menu for admin functions
                ContextMenuStrip registrationsMenu = new ContextMenuStrip();

                ToolStripMenuItem markAttendedItem = new ToolStripMenuItem("Mark as Attended");
                markAttendedItem.Click += (s, e) => UpdateAttendanceStatus("Attended");

                ToolStripMenuItem markNoShowItem = new ToolStripMenuItem("Mark as No-Show");
                markNoShowItem.Click += (s, e) => UpdateAttendanceStatus("No-Show");

                registrationsMenu.Items.Add(markAttendedItem);
                registrationsMenu.Items.Add(markNoShowItem);

                registrationsListView.ContextMenuStrip = registrationsMenu;

                flowLayout.Controls.Add(registrationsListView);
            }

            // Add the flow layout to the main panel
            mainPanel.Controls.Add(flowLayout);
            this.Controls.Add(mainPanel);

            // Set accept/cancel buttons
            this.AcceptButton = saveButton;
            this.CancelButton = cancelButton;
        }

        // Helper method for creating header labels
        private Label CreateHeaderLabel(string text)
        {
            return new Label
            {
                Text = text,
                Width = 650,
                AutoSize = false,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(5, 5, 5, 0)
            };
        }

        private void LoadEventData()
        {
            if (_event != null)
            {
                titleTextBox.Text = _event.Title;

                // Set date and time pickers
                eventDatePicker.Value = _event.EventDate.Date;
                eventTimePicker.Value = _event.EventDate;

                if (_event.EndDate.HasValue)
                {
                    endDatePicker.Value = _event.EndDate.Value.Date;
                    endTimePicker.Value = _event.EndDate.Value;
                }
                else
                {
                    // Default to event date + 2 hours
                    endDatePicker.Value = _event.EventDate.Date;
                    endTimePicker.Value = _event.EventDate.AddHours(2);
                }

                locationTextBox.Text = _event.Location;

                // Set category
                if (!string.IsNullOrEmpty(_event.Category) && categoryComboBox.Items.Contains(_event.Category))
                {
                    categoryComboBox.SelectedItem = _event.Category;
                }
                else
                {
                    categoryComboBox.SelectedIndex = categoryComboBox.Items.Count - 1; // "Other"
                }

                descriptionTextBox.Text = _event.Description;

                // Load image if available
                if (!string.IsNullOrEmpty(_event.ImagePath) && File.Exists(_event.ImagePath))
                {
                    try
                    {
                        eventImagePictureBox.Image = Image.FromFile(_event.ImagePath);
                        _imagePath = _event.ImagePath;
                    }
                    catch
                    {
                        eventImagePictureBox.Image = null;
                    }
                }

                isActiveCheckBox.Checked = _event.IsActive;

                // Load registrations list
                LoadRegistrationsList();
            }
        }

        private void LoadRegistrationsList()
        {
            // Only attempt to load registrations if the ListView exists
            if (registrationsListView == null)
                return;

            registrationsListView.Items.Clear();

            foreach (var reg in _registrations)
            {
                ListViewItem item = new ListViewItem(reg.RegistrationID.ToString());
                item.SubItems.Add(reg.UserName);
                item.SubItems.Add(reg.RegistrationDate.ToString("MMM dd, yyyy h:mm tt"));
                item.SubItems.Add(reg.AttendanceStatus);

                // Color code by attendance status
                if (reg.AttendanceStatus == "Attended")
                {
                    item.BackColor = Color.LightGreen;
                }
                else if (reg.AttendanceStatus == "No-Show")
                {
                    item.BackColor = Color.LightPink;
                }

                // Store registration object in tag
                item.Tag = reg;

                registrationsListView.Items.Add(item);
            }

            // Update registrations label
            if (registrationsLabel != null)
            {
                registrationsLabel.Text = $"Registrations ({_registrations.Count})";
            }
        }

        private void BrowseImageButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select an Event Image";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Load and display the image
                        eventImagePictureBox.Image = Image.FromFile(openFileDialog.FileName);
                        _imagePath = openFileDialog.FileName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(titleTextBox.Text))
                {
                    MessageBox.Show("Please enter a title for the event.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    titleTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(locationTextBox.Text))
                {
                    MessageBox.Show("Please enter a location for the event.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    locationTextBox.Focus();
                    return;
                }

                if (categoryComboBox.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a category for the event.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    categoryComboBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(descriptionTextBox.Text))
                {
                    MessageBox.Show("Please enter a description for the event.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    descriptionTextBox.Focus();
                    return;
                }

                // Combine date and time
                DateTime eventDateTime = eventDatePicker.Value.Date.Add(eventTimePicker.Value.TimeOfDay);
                DateTime? endDateTime = endDatePicker.Value.Date.Add(endTimePicker.Value.TimeOfDay);

                // Validate dates
                if (eventDateTime < DateTime.Now && _eventID <= 0)
                {
                    MessageBox.Show("Event date cannot be in the past.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    eventDatePicker.Focus();
                    return;
                }

                if (endDateTime <= eventDateTime)
                {
                    MessageBox.Show("End time must be after the start time.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    endTimePicker.Focus();
                    return;
                }

                // Update event object
                _event.Title = titleTextBox.Text.Trim();
                _event.Description = descriptionTextBox.Text.Trim();
                _event.EventDate = eventDateTime;
                _event.EndDate = endDateTime;
                _event.Location = locationTextBox.Text.Trim();
                _event.Category = categoryComboBox.SelectedItem.ToString();
                _event.IsActive = isActiveCheckBox.Checked;
                _event.ImagePath = _imagePath;

                bool success;

                if (_eventID > 0)
                {
                    // Update existing event
                    success = EventRepository.UpdateEvent(_event);
                }
                else
                {
                    // Create new event
                    _event.CreatedDate = DateTime.Now;
                    int newID = EventRepository.AddEvent(_event);
                    success = (newID > 0);
                }

                if (success)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save event. Please try again.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving event: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (EventRepository.RegisterForEvent(_eventID, _currentUserID))
                {
                    MessageBox.Show("You have successfully registered for this event!", "Registration Successful",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Update UI
                    registerButton.Text = "Already Registered";
                    registerButton.Enabled = false;

                    // Show cancel registration button
                    cancelRegistrationButton.Visible = true;
                    cancelRegistrationButton.Enabled = true;

                    // Update registration status
                    _isRegistered = true;

                    // Refresh registrations list if visible
                    if (_isEditable)
                    {
                        _registrations = EventRepository.GetEventRegistrations(_eventID);
                        LoadRegistrationsList();
                    }
                }
                else
                {
                    MessageBox.Show("Failed to register for event. You may already be registered.", "Registration Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registering for event: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelRegistrationButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to cancel your registration for this event?",
                "Confirm Cancellation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (EventRepository.CancelRegistration(_eventID, _currentUserID))
                    {
                        MessageBox.Show("Your registration has been cancelled.", "Cancellation Successful",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Update UI
                        registerButton.Text = "Register for Event";
                        registerButton.Enabled = true;

                        // Hide cancel registration button
                        cancelRegistrationButton.Visible = false;

                        // Update registration status
                        _isRegistered = false;

                        // Refresh registrations list if visible
                        if (_isEditable)
                        {
                            _registrations = EventRepository.GetEventRegistrations(_eventID);
                            LoadRegistrationsList();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to cancel registration.", "Cancellation Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error cancelling registration: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateAttendanceStatus(string newStatus)
        {
            // Check if registrationsListView exists and has a selected item
            if (registrationsListView == null || registrationsListView.SelectedItems.Count <= 0)
                return;

            try
            {
                ListViewItem selected = registrationsListView.SelectedItems[0];
                EventRegistration registration = (EventRegistration)selected.Tag;

                if (EventRepository.UpdateAttendanceStatus(registration.RegistrationID, newStatus))
                {
                    // Refresh registrations list
                    _registrations = EventRepository.GetEventRegistrations(_eventID);
                    LoadRegistrationsList();
                }
                else
                {
                    MessageBox.Show("Failed to update attendance status.", "Update Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating attendance status: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}