using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CommunityIssueReporter.Data;
using CommunityIssueReporter.Models;
using CommunityIssueReporter.Utilities;

namespace CommunityIssueReporter.UI
{
    public class IssueReportForm : Form
    {
        private TextBox locationTextBox;
        private ComboBox categoryComboBox;
        private RichTextBox descriptionTextBox;
        private Label attachmentLabel;
        private ProgressBar progressBar;
        private Label progressLabel;
        private Button attachButton;
        private Button submitButton;
        private Button cancelButton;
        private string _attachmentPath = string.Empty;

        public IssueReportForm()
        {
            InitializeComponents();
            UpdateProgress(this, EventArgs.Empty);
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = "Report an Issue";
            this.Size = new Size(700, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;

            // Header Panel
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = AppColors.AccentColor
            };

            Label titleLabel = new Label
            {
                Text = "Report an Issue",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = false
            };
            headerPanel.Controls.Add(titleLabel);

            // Main content panel
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.PrimaryBackground,
                Padding = new Padding(20)
            };

            // Form layout
            TableLayoutPanel formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 380,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(10),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            for (int i = 0; i < 5; i++)
            {
                formLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            }

            // Location Row
            Label locationLabel = CreateFormLabel("Location:");
            locationTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 5, 0, 5)
            };
            locationTextBox.TextChanged += UpdateProgress;
            formLayout.Controls.Add(locationLabel, 0, 0);
            formLayout.Controls.Add(locationTextBox, 1, 0);

            // Category Row
            Label categoryLabel = CreateFormLabel("Category:");
            categoryComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 5, 0, 5)
            };
            categoryComboBox.Items.AddRange(new object[] { "Sanitation", "Roads", "Utilities", "Public Safety", "Other" });
            categoryComboBox.SelectedIndex = 0;
            formLayout.Controls.Add(categoryLabel, 0, 1);
            formLayout.Controls.Add(categoryComboBox, 1, 1);

            // Description Row (spans 2 rows for more space)
            Label descriptionLabel = CreateFormLabel("Description:");
            descriptionTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 5, 0, 5)
            };
            descriptionTextBox.TextChanged += UpdateProgress;
            formLayout.Controls.Add(descriptionLabel, 0, 2);
            formLayout.Controls.Add(descriptionTextBox, 1, 2);
            formLayout.SetRowSpan(descriptionTextBox, 2);

            // Attachment Row
            Label attachmentLbl = CreateFormLabel("Attachment:");
            Panel attachmentPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            attachButton = new Button
            {
                Text = "Attach Image/Document",
                Width = 200,
                Height = 30,
                Left = 0,
                Top = 0,
                Font = new Font("Segoe UI", 9),
                BackColor = AppColors.SecondaryAccent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            attachButton.FlatAppearance.BorderSize = 0;
            attachButton.Click += AttachButton_Click;

            attachmentLabel = new Label
            {
                Text = "No file attached",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = true,
                Left = 0,
                Top = 35
            };

            attachmentPanel.Controls.Add(attachButton);
            attachmentPanel.Controls.Add(attachmentLabel);

            formLayout.Controls.Add(attachmentLbl, 0, 4);
            formLayout.Controls.Add(attachmentPanel, 1, 4);

            // Progress Section
            Panel progressPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(20, 10, 20, 10)
            };

            Label progressTitleLabel = new Label
            {
                Text = "Report Completion:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Width = 150,
                Height = 20,
                Left = 20,
                Top = 10,
                ForeColor = AppColors.PrimaryText
            };

            progressBar = new ProgressBar
            {
                Width = 400,
                Height = 20,
                Left = 170,
                Top = 10,
                Value = 0,
                Maximum = 100
            };

            progressLabel = new Label
            {
                Text = "Start filling in the details to complete your report!",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Width = 400,
                Height = 20,
                Left = 170,
                Top = 35,
                ForeColor = Color.Gray
            };

            progressPanel.Controls.Add(progressTitleLabel);
            progressPanel.Controls.Add(progressBar);
            progressPanel.Controls.Add(progressLabel);

            // Buttons Panel
            Panel buttonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(20, 10, 20, 10)
            };

            submitButton = new Button
            {
                Text = "Submit Report",
                Width = 150,
                Height = 40,
                Left = buttonsPanel.ClientSize.Width - 170,
                Top = 10,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = AppColors.ActionButton,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            submitButton.FlatAppearance.BorderSize = 0;
            submitButton.Click += SubmitButton_Click;

            cancelButton = new Button
            {
                Text = "Cancel",
                Width = 150,
                Height = 40,
                Left = 20,
                Top = 10,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = AppColors.SecondaryAccent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Click += CancelButton_Click;

            buttonsPanel.Controls.Add(submitButton);
            buttonsPanel.Controls.Add(cancelButton);

            // Add all panels to form
            contentPanel.Controls.Add(buttonsPanel);
            contentPanel.Controls.Add(progressPanel);
            contentPanel.Controls.Add(formLayout);

            this.Controls.Add(contentPanel);
            this.Controls.Add(headerPanel);
        }

        private Label CreateFormLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = AppColors.PrimaryText,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
        }

        private void AttachButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg, *.jpeg, *.png, *.gif, *.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Document Files (*.pdf, *.doc, *.docx)|*.pdf;*.doc;*.docx|All Files (*.*)|*.*",
                Title = "Select an Image or Document"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _attachmentPath = openFileDialog.FileName;
                attachmentLabel.Text = Path.GetFileName(_attachmentPath);
                attachmentLabel.ForeColor = AppColors.ProgressGood;
                UpdateProgress(this, EventArgs.Empty);
            }
        }

        private void UpdateProgress(object sender, EventArgs e)
        {
            int progress = 0;

            // Calculate progress based on filled fields
            if (!string.IsNullOrWhiteSpace(locationTextBox.Text))
                progress += 30;

            if (!string.IsNullOrWhiteSpace(descriptionTextBox.Text))
            {
                if (descriptionTextBox.Text.Length > 50)
                    progress += 40;
                else
                    progress += 20;
            }

            if (!string.IsNullOrEmpty(_attachmentPath))
                progress += 30;

            progressBar.Value = Math.Min(progress, 100);

            // Update progress label and color based on completion
            if (progress < 30)
            {
                progressLabel.Text = "Start filling in the details to complete your report!";
                progressLabel.ForeColor = Color.Gray;
            }
            else if (progress < 60)
            {
                progressLabel.Text = "Good start! Keep adding details to your report.";
                progressLabel.ForeColor = Color.DarkOrange;
            }
            else if (progress < 100)
            {
                progressLabel.Text = "Almost there! Your report is looking good.";
                progressLabel.ForeColor = Color.DarkGoldenrod;
            }
            else
            {
                progressLabel.Text = "Great job! Your report is complete and ready to submit.";
                progressLabel.ForeColor = AppColors.ProgressGood;
            }
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(locationTextBox.Text))
            {
                MessageBox.Show("Please enter a location for the issue.",
                               "Missing Information",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                locationTextBox.Focus();
                return;
            }

            if (!ValidationHelper.IsValidLocation(locationTextBox.Text))
            {
                MessageBox.Show("Please provide a more specific location (at least 3 characters).",
                               "Invalid Location",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                locationTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(descriptionTextBox.Text))
            {
                MessageBox.Show("Please provide a description of the issue.",
                               "Missing Information",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                descriptionTextBox.Focus();
                return;
            }

            if (!ValidationHelper.IsValidDescription(descriptionTextBox.Text))
            {
                MessageBox.Show("Please provide a more detailed description (at least 10 characters).",
                               "Invalid Description",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                descriptionTextBox.Focus();
                return;
            }

            try
            {
                // Create the issue
                Issue newIssue = new Issue
                {
                    UserID = UserRepository.CurrentUser?.UserID, // May be null for anonymous reports
                    Location = locationTextBox.Text,
                    Category = categoryComboBox.SelectedItem.ToString(),
                    Description = descriptionTextBox.Text,
                    AttachmentPath = _attachmentPath,
                    ReportedTime = DateTime.Now,
                    Status = "New",
                    Priority = "Medium"
                };

                // Save to repository
                int issueID = IssueRepository.AddIssue(newIssue);

                if (issueID > 0)
                {
                    MessageBox.Show($"Thank you for your report! Your issue has been submitted successfully.\n\n" +
                                   $"Issue ID: {issueID}\n" +
                                   $"Location: {newIssue.Location}\n" +
                                   $"Category: {newIssue.Category}\n" +
                                   $"Time: {newIssue.ReportedTime.ToString("g")}",
                                   "Report Submitted",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("There was a problem submitting your report. Please try again.",
                                   "Submission Error",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}",
                               "Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (progressBar.Value > 0)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to cancel? Your report will not be saved.",
                                                     "Confirm Cancel",
                                                     MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // IssueReportForm
            // 
            this.ClientSize = new System.Drawing.Size(419, 297);
            this.Name = "IssueReportForm";
            this.ResumeLayout(false);

        }
    }
}