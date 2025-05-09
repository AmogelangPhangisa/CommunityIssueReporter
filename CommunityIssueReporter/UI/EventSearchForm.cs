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
    public class EventSearchForm : Form
    {
        // UI Controls
        private ComboBox categoryComboBox;
        private DateTimePicker startDatePicker;
        private DateTimePicker endDatePicker;
        private CheckBox includeInactiveCheckBox;
        private TextBox keywordTextBox;
        private ComboBox sortByComboBox;
        private Button searchButton;
        private Button resetButton;
        private ListView resultsListView;
        private Label resultsCountLabel;
        private CheckBox upcomingOnlyCheckBox;

        // Event Data
        private List<Event> _searchResults;
        private int _currentUserID;
        private string _currentUserRole;

        public EventSearchForm(int currentUserID, string currentUserRole)
        {
            _currentUserID = currentUserID;
            _currentUserRole = currentUserRole;

            // Initialize the event cache
            EventCache.Initialize();

            // Initialize the UI
            InitializeComponents();
            PopulateCategories();

            // Set defaults and perform initial search
            ResetFilters();
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = "Advanced Event Search";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterParent;
      
            this.Icon = new Icon(SystemIcons.Application, 40, 40);
            // Create main layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20)
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Title
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140)); // Search panel
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Results

            // Title
            Label titleLabel = new Label
            {
                Text = "Advanced Event Search",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = AppColors.PrimaryText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Search panel
            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Category filter
            Label categoryLabel = new Label
            {
                Text = "Category:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(15, 15)
            };

            categoryComboBox = new ComboBox
            {
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(90, 12)
            };

            // Keyword filter
            Label keywordLabel = new Label
            {
                Text = "Keyword:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(320, 15)
            };

            keywordTextBox = new TextBox
            {
                Width = 200,
                Location = new Point(390, 12)
            };

            // Date range filter
            Label dateRangeLabel = new Label
            {
                Text = "Date Range:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(15, 50)
            };

            startDatePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Location = new Point(90, 48)
            };

            Label toLabel = new Label
            {
                Text = "to",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(220, 50)
            };

            endDatePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Location = new Point(240, 48)
            };

            // Upcoming Only checkbox
            upcomingOnlyCheckBox = new CheckBox
            {
                Text = "Upcoming events only",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(390, 50)
            };
            upcomingOnlyCheckBox.CheckedChanged += (s, e) => {
                if (upcomingOnlyCheckBox.Checked)
                {
                    startDatePicker.Value = DateTime.Now.Date;
                    startDatePicker.Enabled = false;
                }
                else
                {
                    startDatePicker.Enabled = true;
                }
            };

            // Include inactive events (Admin/Staff only)
            includeInactiveCheckBox = new CheckBox
            {
                Text = "Include inactive events",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(15, 85),
                Visible = (_currentUserRole == "Admin" || _currentUserRole == "Staff")
            };

            // Sort By dropdown
            Label sortByLabel = new Label
            {
                Text = "Sort By:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(320, 85)
            };

            sortByComboBox = new ComboBox
            {
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(390, 82)
            };

            // Add sort options
            sortByComboBox.Items.AddRange(new string[]
            {
                "Date (Soonest First)",
                "Date (Latest First)",
                "Title (A-Z)",
                "Title (Z-A)",
                "Category"
            });

            // Search button
            searchButton = new Button
            {
                Text = "Search",
                Width = 100,
                Height = 35,
                Location = new Point(650, 15),
                BackColor = AppColors.ActionButton,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            searchButton.FlatAppearance.BorderSize = 0;
            searchButton.Click += SearchButton_Click;

            // Reset button
            resetButton = new Button
            {
                Text = "Reset Filters",
                Width = 100,
                Height = 35,
                Location = new Point(650, 60),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            resetButton.Click += ResetButton_Click;

            // Export button (optional)
            Button exportButton = new Button
            {
                Text = "Export Results",
                Width = 100,
                Height = 35,
                Location = new Point(760, 15),
                BackColor = AppColors.InfoColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            exportButton.Click += ExportButton_Click;

            // Add controls to search panel
            searchPanel.Controls.Add(categoryLabel);
            searchPanel.Controls.Add(categoryComboBox);
            searchPanel.Controls.Add(keywordLabel);
            searchPanel.Controls.Add(keywordTextBox);
            searchPanel.Controls.Add(dateRangeLabel);
            searchPanel.Controls.Add(startDatePicker);
            searchPanel.Controls.Add(toLabel);
            searchPanel.Controls.Add(endDatePicker);
            searchPanel.Controls.Add(upcomingOnlyCheckBox);
            searchPanel.Controls.Add(includeInactiveCheckBox);
            searchPanel.Controls.Add(sortByLabel);
            searchPanel.Controls.Add(sortByComboBox);
            searchPanel.Controls.Add(searchButton);
            searchPanel.Controls.Add(resetButton);
            searchPanel.Controls.Add(exportButton);

            // Results panel
            Panel resultsPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            // Results count label
            resultsCountLabel = new Label
            {
                Text = "Search for events using the filters above",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(5, 5)
            };

            // Results list view
            resultsListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(5, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(resultsPanel.Width - 10, resultsPanel.Height - 35)
            };

            // Add columns
            resultsListView.Columns.Add("ID", 40);
            resultsListView.Columns.Add("Title", 200);
            resultsListView.Columns.Add("Date & Time", 150);
            resultsListView.Columns.Add("Location", 150);
            resultsListView.Columns.Add("Category", 100);
            resultsListView.Columns.Add("Created By", 120);

            if (_currentUserRole == "Admin" || _currentUserRole == "Staff")
            {
                resultsListView.Columns.Add("Status", 80);
            }

            // Double-click to view event details
            resultsListView.DoubleClick += ResultsListView_DoubleClick;

            // Right-click menu
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem viewItem = new ToolStripMenuItem("View Event Details");
            viewItem.Click += (s, e) => ResultsListView_DoubleClick(resultsListView, EventArgs.Empty);
            ToolStripMenuItem registerItem = new ToolStripMenuItem("Register for Event");
            registerItem.Click += RegisterMenuItem_Click;

            contextMenu.Items.Add(viewItem);
            contextMenu.Items.Add(registerItem);

            if (_currentUserRole == "Admin" || _currentUserRole == "Staff")
            {
                ToolStripMenuItem editItem = new ToolStripMenuItem("Edit Event");
                editItem.Click += EditMenuItem_Click;
                contextMenu.Items.Add(editItem);
            }

            resultsListView.ContextMenuStrip = contextMenu;

            resultsPanel.Controls.Add(resultsCountLabel);
            resultsPanel.Controls.Add(resultsListView);

            // Add to main layout
            mainLayout.Controls.Add(titleLabel, 0, 0);
            mainLayout.Controls.Add(searchPanel, 0, 1);
            mainLayout.Controls.Add(resultsPanel, 0, 2);

            // Add to form
            this.Controls.Add(mainLayout);

            // Set up event handlers for live searching
            keywordTextBox.TextChanged += (s, e) => {
                // Perform search as user types (after a short delay)
                if (!string.IsNullOrEmpty(keywordTextBox.Text) && keywordTextBox.Text.Length > 2)
                {
                    Timer searchTimer = new Timer();
                    searchTimer.Interval = 500; // Wait 500ms after typing stops
                    searchTimer.Tick += (st, et) => {
                        PerformSearch();
                        searchTimer.Stop();
                        searchTimer.Dispose();
                    };
                    searchTimer.Start();
                }
            };

            sortByComboBox.SelectedIndexChanged += (s, e) => {
                if (_searchResults != null && _searchResults.Count > 0)
                {
                    SortResults();
                    DisplayResults();
                }
            };
        }

        private void PopulateCategories()
        {
            // Add "All Categories" option
            categoryComboBox.Items.Add("All Categories");

            // Get unique categories from cache
            List<string> categories = EventCache.GetAllCategories();

            // Add to combo box
            foreach (var category in categories)
            {
                categoryComboBox.Items.Add(category);
            }

            // Set default selection
            categoryComboBox.SelectedIndex = 0;
        }

        private void ResetFilters()
        {
            // Set default filter values
            categoryComboBox.SelectedIndex = 0;
            keywordTextBox.Text = "";
            startDatePicker.Value = DateTime.Now.AddMonths(-1);
            endDatePicker.Value = DateTime.Now.AddMonths(3);
            upcomingOnlyCheckBox.Checked = true;
            includeInactiveCheckBox.Checked = false;
            sortByComboBox.SelectedIndex = 0;

            // Apply search to show default results
            PerformSearch();
        }

        private void PerformSearch()
        {
            // Get filter values
            string categoryFilter = categoryComboBox.SelectedItem.ToString();
            string keyword = keywordTextBox.Text.Trim();
            DateTime startDate = startDatePicker.Value.Date;
            DateTime endDate = endDatePicker.Value.Date.AddDays(1).AddSeconds(-1); // End of the selected day
            bool includeInactive = includeInactiveCheckBox.Checked;

            // Get filtered events from cache
            List<Event> results = EventCache.SearchEvents(
                categoryFilter == "All Categories" ? null : categoryFilter,
                startDate,
                endDate,
                includeInactive
            );

            // Apply keyword filter if specified
            if (!string.IsNullOrEmpty(keyword))
            {
                results = results.Where(e =>
                    e.Title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    e.Description.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    e.Location.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }

            // Save results for sorting
            _searchResults = results;

            // Sort and display results
            SortResults();
            DisplayResults();
        }

        private void SortResults()
        {
            if (_searchResults == null || _searchResults.Count == 0)
                return;

            // Apply selected sort order
            switch (sortByComboBox.SelectedIndex)
            {
                case 0: // Date (Soonest First)
                    _searchResults = _searchResults.OrderBy(e => e.EventDate).ToList();
                    break;
                case 1: // Date (Latest First) 
                    _searchResults = _searchResults.OrderByDescending(e => e.EventDate).ToList();
                    break;
                case 2: // Title (A-Z)
                    _searchResults = _searchResults.OrderBy(e => e.Title).ToList();
                    break;
                case 3: // Title (Z-A)
                    _searchResults = _searchResults.OrderByDescending(e => e.Title).ToList();
                    break;
                case 4: // Category
                    _searchResults = _searchResults.OrderBy(e => e.Category).ThenBy(e => e.EventDate).ToList();
                    break;
                default:
                    _searchResults = _searchResults.OrderBy(e => e.EventDate).ToList();
                    break;
            }
        }

        private void DisplayResults()
        {
            // Clear previous results
            resultsListView.Items.Clear();

            // Display results
            foreach (var evt in _searchResults)
            {
                ListViewItem item = new ListViewItem(evt.EventID.ToString());
                item.SubItems.Add(evt.Title);
                item.SubItems.Add(evt.EventDate.ToString("MMM dd, yyyy h:mm tt"));
                item.SubItems.Add(evt.Location);
                item.SubItems.Add(evt.Category);
                item.SubItems.Add(evt.CreatedByName);

                if (_currentUserRole == "Admin" || _currentUserRole == "Staff")
                {
                    item.SubItems.Add(evt.IsActive ? "Active" : "Inactive");

                    // Color inactive events
                    if (!evt.IsActive)
                    {
                        item.BackColor = Color.LightGray;
                    }
                }

                // Color past events
                if (evt.EventDate < DateTime.Now)
                {
                    item.ForeColor = Color.Gray;
                }
                // Color today's events
                else if (evt.EventDate.Date == DateTime.Now.Date)
                {
                    item.BackColor = Color.LightYellow;
                }

                // Store event in tag
                item.Tag = evt;

                resultsListView.Items.Add(item);
            }

            // Update results count
            resultsCountLabel.Text = $"Found {_searchResults.Count} events matching your criteria";

            // Auto-size columns
            resultsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            ResetFilters();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (_searchResults == null || _searchResults.Count == 0)
            {
                MessageBox.Show("No results to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Show save dialog
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt",
                Title = "Export Search Results",
                FileName = "Event_Search_Results.csv"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Create CSV content
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(saveDialog.FileName))
                    {
                        // Write header
                        writer.WriteLine("Event ID,Title,Date,End Date,Location,Category,Created By,Status");

                        // Write data
                        foreach (var evt in _searchResults)
                        {
                            string endDate = evt.EndDate.HasValue ? evt.EndDate.Value.ToString("yyyy-MM-dd HH:mm") : "";
                            writer.WriteLine($"{evt.EventID},\"{evt.Title.Replace("\"", "\"\"")}\",{evt.EventDate.ToString("yyyy-MM-dd HH:mm")},{endDate},\"{evt.Location.Replace("\"", "\"\"")}\",{evt.Category},{evt.CreatedByName},{(evt.IsActive ? "Active" : "Inactive")}");
                        }
                    }

                    MessageBox.Show($"Successfully exported {_searchResults.Count} events to {saveDialog.FileName}",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ResultsListView_DoubleClick(object sender, EventArgs e)
        {
            if (resultsListView.SelectedItems.Count > 0)
            {
                ListViewItem selected = resultsListView.SelectedItems[0];
                Event selectedEvent = (Event)selected.Tag;

                // Open event details form
                EventDetailsForm detailsForm = new EventDetailsForm(_currentUserID, selectedEvent.EventID);
                if (detailsForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh search results if changes were made
                    EventCache.Initialize(); // Refresh the cache
                    PerformSearch();
                }
            }
        }

        private void RegisterMenuItem_Click(object sender, EventArgs e)
        {
            if (resultsListView.SelectedItems.Count > 0)
            {
                ListViewItem selected = resultsListView.SelectedItems[0];
                Event selectedEvent = (Event)selected.Tag;

                if (_currentUserID <= 0)
                {
                    MessageBox.Show("You must be logged in to register for events.",
                        "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!selectedEvent.IsActive)
                {
                    MessageBox.Show("Cannot register for inactive events.",
                        "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (selectedEvent.EventDate < DateTime.Now)
                {
                    MessageBox.Show("Cannot register for past events.",
                        "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if already registered
                bool isRegistered = EventRepository.IsUserRegistered(selectedEvent.EventID, _currentUserID);
                if (isRegistered)
                {
                    MessageBox.Show("You are already registered for this event.",
                        "Already Registered", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Confirm registration
                DialogResult result = MessageBox.Show(
                    $"Would you like to register for the event '{selectedEvent.Title}'?",
                    "Confirm Registration", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (EventRepository.RegisterForEvent(selectedEvent.EventID, _currentUserID))
                    {
                        MessageBox.Show("You have successfully registered for this event!",
                            "Registration Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to register for event. Please try again.",
                            "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void EditMenuItem_Click(object sender, EventArgs e)
        {
            if (resultsListView.SelectedItems.Count > 0)
            {
                ListViewItem selected = resultsListView.SelectedItems[0];
                Event selectedEvent = (Event)selected.Tag;

                // Open event details form in edit mode
                EventDetailsForm detailsForm = new EventDetailsForm(_currentUserID, selectedEvent.EventID);
                if (detailsForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh search results if changes were made
                    EventCache.Initialize(); // Refresh the cache
                    PerformSearch();
                }
            }
        }
    }
}