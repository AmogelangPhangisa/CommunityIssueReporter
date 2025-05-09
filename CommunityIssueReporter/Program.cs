using System;
using System.Windows.Forms;
using CommunityIssueReporter.Data;
using CommunityIssueReporter.UI;

namespace CommunityIssueReporter
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Test database connection with detailed error info  
                string connectionError;
                if (DatabaseManager.TestConnection(out connectionError))
                {
                    try
                    {
                        // Initialize database structures  
                        DatabaseManager.InitializeDatabase();

                        // Start with the main form  
                        Application.Run(new MainForm());
                    }
                    catch (Exception ex)
                    {
                        // Handle database initialization errors  
                        DialogResult result = MessageBox.Show($"Database initialization error: {ex.Message}\n\n" +
                                       "Would you like to run in offline mode?",
                                       "Database Error",
                                       MessageBoxButtons.YesNo,
                                       MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            DatabaseManager.SetOfflineMode(true);
                            Application.Run(new MainForm(true));
                        }
                    }
                }
                else
                {
                    // Show the detailed connection error message  
                    DialogResult result = MessageBox.Show(
                        $"Database connection error: {connectionError}\n\n" +
                        "Would you like to run in offline mode?\n\n" +
                        "Note: In offline mode, your data will not be saved permanently.",
                        "Database Connection Error",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        // Set offline mode and run the application  
                        DatabaseManager.SetOfflineMode(true);
                        Application.Run(new MainForm(true));
                    }
                }
            }
            catch (Exception ex)
            {
                // General application initialization error  
                MessageBox.Show($"Application initialization error: {ex.Message}\n\n" +
                                (ex.InnerException != null ? $"Inner error: {ex.InnerException.Message}\n\n" : "") +
                                "The application will now exit.",
                                "Startup Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
    }
}