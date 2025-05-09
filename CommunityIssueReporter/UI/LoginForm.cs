using System;
using System.Drawing;
using System.Windows.Forms;
using CommunityIssueReporter.Data;
using CommunityIssueReporter.Models;
using CommunityIssueReporter.Utilities;

namespace CommunityIssueReporter.UI
{
    public class LoginForm : Form
    {
        private TextBox usernameTextBox;
        private TextBox passwordTextBox;
        private Button loginButton;
        private Button cancelButton;
        private LinkLabel registerLinkLabel;
        private Label errorLabel;
        private Form _parentForm;

        public LoginForm(Form parentForm = null)
        {
            _parentForm = parentForm;
            InitializeComponents();

            if (_parentForm != null)
            {
                this.FormClosed += LoginForm_FormClosed;
            }
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = "Login";
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create main panel with padding
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30)
            };

            // Header panel with logo/title
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = AppColors.AccentColor
            };

            Label titleLabel = new Label
            {
                Text = "Community Engagement Portal",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            Label subtitleLabel = new Label
            {
                Text = "Login to your account",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 30
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);

            // Login form controls
            FlowLayoutPanel formPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(20, 30, 20, 20)
            };

            // Username
            Label usernameLabel = new Label
            {
                Text = "Username:",
                Width = 340,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 10, 0, 5)
            };

            usernameTextBox = new TextBox
            {
                Width = 340,
                Height = 30,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 0, 0, 15)
            };

            // Password
            Label passwordLabel = new Label
            {
                Text = "Password:",
                Width = 340,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 10, 0, 5)
            };

            passwordTextBox = new TextBox
            {
                Width = 340,
                Height = 30,
                PasswordChar = '•',
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 0, 0, 25)
            };

            // Error message label
            errorLabel = new Label
            {
                ForeColor = Color.Red,
                Width = 340,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false,
                Margin = new Padding(0, 0, 0, 15)
            };

            // Login button
            loginButton = new Button
            {
                Text = "Login",
                Width = 340,
                Height = 40,
                BackColor = AppColors.ActionButton,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 0, 10)
            };
            loginButton.FlatAppearance.BorderSize = 0;
            loginButton.Click += LoginButton_Click;

            // Cancel button
            cancelButton = new Button
            {
                Text = "Cancel",
                Width = 340,
                Height = 40,
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Margin = new Padding(0, 0, 0, 20)
            };
            cancelButton.FlatAppearance.BorderSize = 0;

            // Register link
            registerLinkLabel = new LinkLabel
            {
                Text = "Don't have an account? Register here",
                Width = 340,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9)
            };
            registerLinkLabel.LinkClicked += RegisterLink_LinkClicked;

            // Add controls to form panel
            formPanel.Controls.Add(usernameLabel);
            formPanel.Controls.Add(usernameTextBox);
            formPanel.Controls.Add(passwordLabel);
            formPanel.Controls.Add(passwordTextBox);
            formPanel.Controls.Add(errorLabel);
            formPanel.Controls.Add(loginButton);
            formPanel.Controls.Add(cancelButton);
            formPanel.Controls.Add(registerLinkLabel);

            // Add panels to main panel
            mainPanel.Controls.Add(formPanel);

            // Add all to form
            this.Controls.Add(mainPanel);
            this.Controls.Add(headerPanel);

            // Set this button as the accept button (Enter key)
            this.AcceptButton = loginButton;
            this.CancelButton = cancelButton;

            // Set focus to username text box
            this.Load += (s, e) => usernameTextBox.Focus();
        }

        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK && _parentForm != null)
            {
                _parentForm.Show();
            }
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            // Clear previous error
            errorLabel.Visible = false;

            // Validate input
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text) || string.IsNullOrWhiteSpace(passwordTextBox.Text))
            {
                errorLabel.Text = "Please enter both username and password.";
                errorLabel.Visible = true;
                return;
            }

            try
            {
                // Attempt to login
                bool success = UserRepository.LoginUser(usernameTextBox.Text, passwordTextBox.Text);

                if (success)
                {
                    // Login successful
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    // Login failed
                    errorLabel.Text = "Invalid username or password. Please try again.";
                    errorLabel.Visible = true;
                    passwordTextBox.Clear();
                    passwordTextBox.Focus();
                }
            }
            catch (Exception ex)
            {
                errorLabel.Text = $"Error: {ex.Message}";
                errorLabel.Visible = true;
            }
        }

        private void RegisterLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Open registration form
            using (RegisterForm registerForm = new RegisterForm(this))
            {
                this.Hide();
                DialogResult result = registerForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // User registered and auto-logged in
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    // User cancelled registration
                    this.Show();
                }
            }
        }
    }
}