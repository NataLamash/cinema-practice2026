cinema-practice2026
Cinema management system

How to Connect to Azure SQL Database Using SQL Server Management Studio (SSMS)
------------------------------------------------------------------------------

1. Install SQL Server Management Studio (SSMS)

If SSMS is not installed:

1) Download Microsoft SQL Server Management Studio
2) Use the official installer from Microsoft
3) Install with default settings

2. Connect to the Database in SSMS

1) Open SQL Server Management Studio

In the Connect to Server window:
Server type: Database Engine
Server name:  cinema-sqlserver.database.windows.net
Authentication:  SQL Server Authentication
Login: (enter the login)
Password: (enter the password)

Click Options >>

In the field Connect to database, enter: CinemaDb
Click Connect

3. If the connection is successful, the database will appear in Object Explorer.

4. After successful connection, the user can:
1) View and manage tables
2) Run SQL queries
3) Work with the database simultaneously with other team members


Database Setup Guide (User Secrets & Migrations)
------------------------------------------------

1) Move to the CinemaWeb folder
cd CinemaWeb
2) Initialize User Secrets (only once)
dotnet user-secrets init
3) Set the connection string
dotnet user-secrets set 'ConnectionStrings:DefaultConnection' 'Server=cinema-sqlserver.database.windows.net;Database=CinemaDb;User Id=team_user;Password=YOUR_PASSWORD;Encrypt=False;TrustServerCertificate=True;'

The YOUR_PASSWORD you wil recieve personally
4) Verify that the secret was saved
dotnet user-secrets list


Applying Entity Framework Core Migrations
-----------------------------------------

1) Apply existing migrations to the database

From the solution root directory:

dotnet ef database update --project CinemaInfrastructure --startup-project CinemaWeb

2) Create a new migration

When the model changes:

dotnet ef migrations add MigrationName --project CinemaInfrastructure --startup-project CinemaWeb

Then:

dotnet ef database update --project CinemaInfrastructure --startup-project CinemaWeb

Handling Migration Conflicts
-----------------------------

1) Migration exists locally but not in the database

Run:

dotnet ef database update --project CinemaInfrastructure --startup-project CinemaWeb

2) Migration applied in database but missing locally

Update your local repository:

git pull

Then run:

dotnet ef database update --project CinemaInfrastructure --startup-project CinemaWeb

3) Conflicting migrations created by different developers

Pull latest changes:

git pull


Remove your local migration (if not applied yet):

dotnet ef migrations remove --project CinemaInfrastructure --startup-project CinemaWeb

Recreate the migration after updating the model:

dotnet ef migrations add NewMigrationName --project CinemaInfrastructure --startup-project CinemaWeb

4) Conflicting migrations created by different developers

⚠️ Do this only after the team approval!

You can reset the database:

dotnet ef database drop --project CinemaInfrastructure --startup-project CinemaWeb


Then reapply all migrations:

dotnet ef database update --project CinemaInfrastructure --startup-project CinemaWeb