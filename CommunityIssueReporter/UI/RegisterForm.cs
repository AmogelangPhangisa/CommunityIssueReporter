using System;
using System.Drawing;
using System.Windows.Forms;
using CommunityIssueReporter.Data;
using CommunityIssueReporter.Utilities;

namespace CommunityIssueReporter.UI
{
    // Changed from partial to regular class since we're creating UI programmatically
    public class RegisterForm : Form
    {
        // UI Controls
        private TextBox firstNameTextBox;
        private TextBox lastNameTextBox;
        private TextBox usernameTextBox;
        private TextBox emailTextBox;
        private TextBox phoneTextBox;
        private TextBox addressTextBox;
        private TextBox passwordTextBox;
        private TextBox confirmPasswordTextBox;
        private Label errorLabel;
        private Button registerButton;
        private LinkLabel loginLink;

        private Form _parentForm;

        // Constructor without parent form
        public RegisterForm()
        {
            InitializeComponents();
            CustomInitialize();
        }

        // Constructor with parent form
        public RegisterForm(Form parentForm)
        {
            _parentForm = parentForm;
            InitializeComponents();
            CustomInitialize();

            // Add FormClosed event if parent form exists
            if (_parentForm != null)
            {
                this.FormClosed += RegisterForm_FormClosed;
            }
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = "Register New Account";
            this.Size = new Size(500, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Header Panel
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = AppColors.AccentColor
            };

            Label titleLabel = new Label
            {
                Text = "Create New Account",
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
                AutoScroll = true,
                Padding = new Padding(40, 20, 40, 20)
            };

            // Registration form
            TableLayoutPanel formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Width = 420,
                Height = 420,
                ColumnCount = 2,
                RowCount = 8,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            for (int i = 0; i < 8; i++)
            {
                formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            }

            // First Name
            Label firstNameLabel = CreateLabel("First Name:");
            firstNameTextBox = CreateTextBox();
            formLayout.Controls.Add(firstNameLabel, 0, 0);
            formLayout.Controls.Add(firstNameTextBox, 1, 0);

            // Last Name
            Label lastNameLabel = CreateLabel("Last Name:");
            lastNameTextBox = CreateTextBox();
            formLayout.Controls.Add(lastNameLabel, 0, 1);
            formLayout.Controls.Add(lastNameTextBox, 1, 1);

            // Username
            Label usernameLabel = CreateLabel("Username:");
            usernameTextBox = CreateTextBox();
            formLayout.Controls.Add(usernameLabel, 0, 2);
            formLayout.Controls.Add(usernameTextBox, 1, 2);

            // Email
            Label emailLabel = CreateLabel("Email:");
            emailTextBox = CreateTextBox();
            formLayout.Controls.Add(emailLabel, 0, 3);
            formLayout.Controls.Add(emailTextBox, 1, 3);

            // Phone
            Label phoneLabel = CreateLabel("Phone (optional):");
            phoneTextBox = CreateTextBox();
            formLayout.Controls.Add(phoneLabel, 0, 4);
            formLayout.Controls.Add(phoneTextBox, 1, 4);

            // Address
            Label addressLabel = CreateLabel("Address (optional):");
            addressTextBox = CreateTextBox();
            formLayout.Controls.Add(addressLabel, 0, 5);
            formLayout.Controls.Add(addressTextBox, 1, 5);

            // Password
            Label passwordLabel = CreateLabel("Password:");
            passwordTextBox = CreateTextBox(true);
            formLayout.Controls.Add(passwordLabel, 0, 6);
            formLayout.Controls.Add(passwordTextBox, 1, 6);

            // Confirm Password
            Label confirmPasswordLabel = CreateLabel("Confirm Password:");
            confirmPasswordTextBox = CreateTextBox(true);
            formLayout.Controls.Add(confirmPasswordLabel, 0, 7);
            formLayout.Controls.Add(confirmPasswordTextBox, 1, 7);

            // Register Button
            registerButton = new Button
            {
                Text = "Register",
                Width = 200,
                Height = 40,
                Left = (this.ClientSize.Width - 200) / 2 - 40,
                Top = 450,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = AppColors.ActionButton,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            registerButton.FlatAppearance.BorderSize = 0;

            // Set as Accept button
            this.AcceptButton = registerButton;

            // Login Link
            loginLink = new LinkLabel
            {
                Text = "Already have an account? Login here",
                Width = 300,
                Height = 20,
                Left = (this.ClientSize.Width - 300) / 2 - 40,
                Top = 500,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Error Label
            errorLabel = new Label
            {
                Width = 400,
                Height = 40,
                Left = (this.ClientSize.Width - 400) / 2 - 40,
                Top = 530,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            // Add controls to content panel
            contentPanel.Controls.Add(formLayout);
            contentPanel.Controls.Add(registerButton);
            contentPanel.Controls.Add(loginLink);
            contentPanel.Controls.Add(errorLabel);

            // Add panels to form
            this.Controls.Add(contentPanel);
            this.Controls.Add(headerPanel);
        }

        private void CustomInitialize()
        {
            // Set form properties that aren't handled by the designer

            // Add event handlers
            registerButton.Click += RegisterButton_Click;
            loginLink.LinkClicked += LoginLink_LinkClicked;
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                ForeColor = AppColors.PrimaryText,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoSize = false
            };
        }

        private TextBox CreateTextBox(bool isPassword = false)
        {
            TextBox textBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 0, 5)
            };

            if (isPassword)
            {
                textBox.PasswordChar = '•';
            }

            return textBox;
        }

        private void RegisterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK && _parentForm != null)
            {
                _parentForm.Show();
            }
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            // Clear previous error
            errorLabel.Visible = false;

            // Validate input
            if (string.IsNullOrWhiteSpace(firstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(lastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(usernameTextBox.Text) ||
                string.IsNullOrWhiteSpace(emailTextBox.Text) ||
                string.IsNullOrWhiteSpace(passwordTextBox.Text) ||
                string.IsNullOrWhiteSpace(confirmPasswordTextBox.Text))
            {
                errorLabel.Text = "Please fill in all required fields.";
                errorLabel.Visible = true;
                return;
            }

            // Validate email format
            if (!ValidationHelper.IsValidEmail(emailTextBox.Text))
            {
                errorLabel.Text = "Please enter a valid email address.";
                errorLabel.Visible = true;
                emailTextBox.Focus();
                return;
            }

            // Validate password strength
            if (!ValidationHelper.IsValidPassword(passwordTextBox.Text))
            {
                errorLabel.Text = "Password must be at least 8 characters and contain letters, numbers, and special characters.";
                errorLabel.Visible = true;
                passwordTextBox.Focus();
                return;
            }

            // Check password match
            if (passwordTextBox.Text != confirmPasswordTextBox.Text)
            {
                errorLabel.Text = "Passwords do not match.";
                errorLabel.Visible = true;
                confirmPasswordTextBox.Clear();
                confirmPasswordTextBox.Focus();
                return;
            }

            try
            {
                // Register the user
                bool success = UserRepository.RegisterUser(
                    usernameTextBox.Text,
                    passwordTextBox.Text,
                    emailTextBox.Text,
                    firstNameTextBox.Text,
                    lastNameTextBox.Text,
                    phoneTextBox.Text,
                    addressTextBox.Text);

                if (success)
                {
                    // Automatically log in the user
                    if (UserRepository.LoginUser(usernameTextBox.Text, passwordTextBox.Text))
                    {
                        MessageBox.Show("Registration successful! You are now logged in.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Registration successful! Please log in.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.Cancel;
                        this.Close();
                    }
                }
                else
                {
                    errorLabel.Text = "Username or email already exists. Please try another.";
                    errorLabel.Visible = true;
                    usernameTextBox.Focus();
                }
            }
            catch (Exception ex)
            {
                errorLabel.Text = "Error: " + ex.Message;
                errorLabel.Visible = true;
            }
        }

        private void LoginLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}