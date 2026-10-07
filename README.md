# QMS

QMS is a .NET 10 starter application with a Razor Pages website, a login API, database-backed user authentication, and role-based access control. The website uses a protected authentication cookie after the API validates a user's credentials. The API does not issue JWTs or require a JWT signing key.

## Projects

- `QMSSystem.Web` - Razor Pages website with landing, login, and access-denied pages.
- `QMSSystem.Api` - API that validates login credentials against SQL Server.
- `QMSSystem.Shared` - shared login, user, and role models.
- `QMSSystem.Tests` - tests for the workflow that connects documents, deviations and change requests.

Before you build a module page or service, read [`docs/INTEGRATION.md`](docs/INTEGRATION.md): it lists the shared status words, how the tables link, and the workflow call your service must add. Test the full journey with [`docs/E2E-CHECKLIST.md`](docs/E2E-CHECKLIST.md).

## Prerequisites

- .NET 10 SDK
- Network access to SQL Server `cqmprddev1` (connect to your organization’s network or VPN if required)
- A Windows account with permission to access the `TRG_CORE` database

## Clone and database access

Clone the repository and open PowerShell in its directory:

```powershell
git clone https://github.com/Yash1026-hash/QMS-GP.git
cd QMS-GP
```

The Development API configuration connects to the shared database using Windows Integrated Authentication:

- Server: `cqmprddev1`
- Database: `TRG_CORE`
- Authentication: Windows / Integrated Security

Connect to `cqmprddev1` in SQL Server Management Studio using **Windows Authentication**, then select `TRG_CORE`. Your Windows account must already be granted server and database access. The application cannot grant permissions. If you cannot connect, contact your database administrator and ask for access; do not replace the shared database connection with personal credentials in committed files.

For another local database, override the connection in User Secrets:

```powershell
dotnet user-secrets set --project .\QMSSystem.Api\QMSSystem.Api.csproj "ConnectionStrings:DefaultConnection" "Server=<server>;Database=<database>;Integrated Security=True;TrustServerCertificate=True;"
```

## Login tables and accounts

The API reads login information from these tables in the `TRG_CORE` database:

| Table | Purpose |
| --- | --- |
| `dbo.KS_RecallUsers` | Usernames, passwords, account status, and profile details |
| `dbo.KS_Roles` | Available role names |
| `dbo.KS_UserRoles` | Links users to roles |

If the tables are missing, ask the database owner to review and run [`database/login-schema.sql`](database/login-schema.sql) against `TRG_CORE`. The script creates missing tables and adds `Admin`, `Operator`, and `Supervisor`; it does not create user accounts or modify existing tables.

Use an existing provisioned account with `RegistrationStatus = 'Registered'` and `IsActive = 1`, assigned to a role through `dbo.KS_UserRoles`. This starter has no self-registration or account-creation flow. Ask the QMS/database administrator to provision an account if you do not already have one.

## Run locally

Open two PowerShell terminals in the repository directory. In the first, start the API:

```powershell
dotnet run --project .\QMSSystem.Api\QMSSystem.Api.csproj
```

The API runs at `http://localhost:5070`; Swagger is at `http://localhost:5070/swagger`.

In the second terminal, start the website:

```powershell
dotnet run --project .\QMSSystem.Web\QMSSystem.Web.csproj
```

Open `http://localhost:5048` and sign in with your existing QMS account. The website sends your username and password to the local API for validation, then keeps the signed-in session in its protected cookie. Keep both processes running while using the site.

You can also test the API login directly with PowerShell and your own account:

```powershell
$loginBody = @{
    username = "<your username>"
    password = "<your password>"
} | ConvertTo-Json
$login = Invoke-RestMethod -Uri "http://localhost:5070/api/users/login" -Method Post -ContentType "application/json" -Body $loginBody
$login | Format-List
```

Do not share your password or the login response if it contains sensitive account information.

## Build

```powershell
dotnet build .\QMSSystem.Api\QMSSystem.Api.csproj
dotnet build .\QMSSystem.Web\QMSSystem.Web.csproj
```
