using System.Drawing;

namespace CommunityIssueReporter.Utilities
{
    public static class AppColors
    {
        // Primary color palette
        public static readonly Color PrimaryBackground = Color.White;
        public static readonly Color PrimaryText = Color.FromArgb(51, 51, 51);
        public static readonly Color AccentColor = Color.FromArgb(65, 105, 225); // Royal Blue
        public static readonly Color SecondaryAccent = Color.FromArgb(70, 130, 180); // Steel Blue
        public static readonly Color ActionButton = Color.FromArgb(50, 205, 50); // Lime Green
        public static readonly Color DangerButton = Color.FromArgb(220, 53, 69); // Bootstrap Danger
        public static readonly Color WarningButton = Color.FromArgb(255, 193, 7); // Bootstrap Warning

        // Status colors
        public static readonly Color SuccessColor = Color.FromArgb(40, 167, 69); // Bootstrap Success
        public static readonly Color WarningColor = Color.FromArgb(255, 193, 7); // Bootstrap Warning
        public static readonly Color DangerColor = Color.FromArgb(220, 53, 69); // Bootstrap Danger
        public static readonly Color InfoColor = Color.FromArgb(23, 162, 184); // Bootstrap Info

        // UI element colors
        public static readonly Color DisabledColor = Color.FromArgb(224, 224, 224);
        public static readonly Color ProgressGood = Color.FromArgb(46, 139, 87); // Sea Green
        public static readonly Color SidebarBackground = Color.FromArgb(52, 58, 64); // Dark Gray
        public static readonly Color LightBackground = Color.FromArgb(248, 249, 250); // Light Gray

        // Status-specific colors
        public static readonly Color NewStatusColor = Color.FromArgb(0, 123, 255); // Bootstrap Primary
        public static readonly Color InProgressColor = Color.FromArgb(255, 193, 7); // Bootstrap Warning
        public static readonly Color ResolvedColor = Color.FromArgb(40, 167, 69); // Bootstrap Success
        public static readonly Color ClosedColor = Color.FromArgb(108, 117, 125); // Bootstrap Secondary

        // Get color by issue status
        public static Color GetStatusColor(string status)
        {
            switch (status?.ToLower())
            {
                case "new":
                    return NewStatusColor;
                case "in progress":
                case "under review":
                    return InProgressColor;
                case "resolved":
                    return ResolvedColor;
                case "closed":
                    return ClosedColor;
                default:
                    return PrimaryText;
            }
        }
    }
}