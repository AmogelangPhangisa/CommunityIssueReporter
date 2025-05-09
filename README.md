# Community Issue Reporter

## Overview
The Community Issue Reporter is a Windows Forms application for tracking, managing, and addressing local community issues and events. This centralized platform allows communities to report concerns, organize events, and monitor resolution progress for neighborhood matters ranging from infrastructure problems to community gatherings.

## Features
- **Event Management**: Create, view, edit, and manage community events
- **Advanced Search**: Find events by category, date range, and keywords
- **User Registration**: Register for events and track attendance
- **Role-Based Access**: Different permissions for users, staff, and administrators
- **Reporting**: Export search results and event data to CSV files
- **Media Support**: Add images to event descriptions

## System Requirements
- Windows 7 or later
- .NET Framework 4.7.2 or later
- Internet connection for Azure SQL database access
- Minimum 4GB RAM
- 50MB available disk space

## Installation
1. Download the latest release package
2. Extract the ZIP file to your preferred location
3. Run CommunityIssueReporter.exe to start the application
4. No installation required - the application runs directly from the extracted folder

## Database Connection
The application connects to an Azure SQL database. The connection string is configured in the app.config file. In offline mode, the application uses a local cache of data.

## Login Credentials
Use the following credentials to access the application:

### Admin Access
Username: admin
Password: Admin@123

### Staff Access
Username: staff
Password: Staff@123

### Regular User Access
Username: user
Password: User@123

## Running the Application
1. Double-click on CommunityIssueReporter.exe
2. Log in using one of the credentials above
3. Use the tabbed interface to navigate between:
   - Upcoming Events
   - My Events
   - Manage Events (admin/staff only)
4. Click "Advanced Search" to find specific events

## Quick Start Guide

### To Create an Event:
1. Click "Create New Event"
2. Fill in title, date, location, category, and description
3. Optionally add an image
4. Click "Save Event"

### To Search for Events:
1. Click "Advanced Search"
2. Set filters (date range, category, keywords)
3. Click "Search"
4. Double-click any result to view details

### To Register for an Event:
1. Find an event through search or browsing
2. Double-click to open its details
3. Click "Register for Event"

## Updating Dependencies
The application uses Azure SQL for its database. If you need to update connection strings or other dependencies:

1. Edit the `CommunityIssueReporter.exe.config` file
2. Modify the connection string to point to your Azure SQL instance:

<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=CommunityDB;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>

## Troubleshooting

### Common Issues
1. **Cannot Connect to Database**
   - Verify internet connection
   - Check Azure SQL server is running
   - Confirm connection string in app.config is correct

2. **Login Issues**
   - Ensure you're using correct credentials from the list above
   - Check for caps lock or typos
   - Try the offline mode if Azure SQL is unavailable

3. **Advanced Search Not Working**
   - Ensure EventCache is initialized (restart application)
   - Clear all filters and try again
   - Check for database connectivity

### Offline Mode
To enable offline mode when Azure SQL is unavailable:
1. Edit `CommunityIssueReporter.exe.config`
2. Set `<add key="OfflineMode" value="true" />`
3. Restart the application

## Support
For support or inquiries, please contact support@communityreporter.org

---

**Note**: This application uses plaintext passwords in offline mode for demonstration purposes only. In a production environment, passwords would be stored using secure hashing techniques.
