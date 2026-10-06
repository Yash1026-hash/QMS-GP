# QMS

QMS is a .NET 10 application with a Razor Pages website, an API, database-backed login, JWT authentication, and role-based access control.

## Projects

- `QMSSystem.Web` - Razor Pages website.
- `QMSSystem.Api` - login API and JWT issuer.
- `QMSSystem.Shared` - shared login, user, and role models.

## Prerequisites

- .NET 10 SDK
- Network access to SQL Server `cqmprddev1` (connect to your organization’s network or VPN if required)
- A Windows account granted access to the `TRG_CORE` database

## Clone and configure

Clone the repository and open PowerShell in the repository directory:

```powershell
git clone https://github.com/Yash1026-hash/QMS-GP.git
cd QMS-GP
```

The Development API configuration points to the shared database using Windows Integrated Authentication:

- Server: `cqmprddev1`
- Database: `TRG_CORE`
- Authentication: Windows / Integrated Security

In SQL Server Management Studio, connect using the server name `cqmprddev1`, choose **Windows Authentication**, and select `TRG_CORE`. Your Windows account must already have permission to access the server and database; the application cannot grant database permissions.

If you need to use a different server or database locally, override the connection string with your own value in User Secrets. Do not commit credentials:

```powershell
dotnet user-secrets set --project .\QMSSystem.Api\QMSSystem.Api.csproj "ConnectionStrings:DefaultConnection" "Server=<server>;Database=<database>;Integrated Security=True;TrustServerCertificate=True;"
```

## Create a local JWT signing key

Each developer creates a private signing key in .NET User Secrets. Run this from the repository directory in PowerShell; it generates a random key and does not put the key in the repository:

```powershell
$bytes = New-Object byte[] 32
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$key = [Convert]::ToBase64String($bytes)
dotnet user-secrets set --project .\QMSSystem.Api\QMSSystem.Api.csproj "Jwt:SigningKey" $key
$rng.Dispose()
[Array]::Clear($bytes, 0, $bytes.Length)
$key = $null
```

The API requires this key at startup. Do not share or commit it. JWT issuer and audience default to `QMS.Api` and `QMS.Web`.

## Login database tables

The API reads login information from these tables in the `TRG_CORE` database:

| Table | Purpose |
| --- | --- |
| `dbo.KS_RecallUsers` | Usernames, password hashes, account status, and profile details |
| `dbo.KS_Roles` | Available role names |
| `dbo.KS_UserRoles` | Links users to roles |

If these tables are not present, ask the database owner to review and run [`database/login-schema.sql`](database/login-schema.sql) against `TRG_CORE`. Do not run a schema script against a shared database unless you have permission. The script creates missing tables and inserts the standard roles (`Admin`, `Operator`, and `Supervisor`); it does not create user accounts.

To sign in, use an existing provisioned account with `RegistrationStatus = 'Registered'` and `IsActive = 1`, assigned to a role through `dbo.KS_UserRoles`. This starter does not include user registration or account creation.

## Run locally

Open two PowerShell terminals at the repository directory. In the first terminal, start the API:

```powershell
dotnet run --project .\QMSSystem.Api\QMSSystem.Api.csproj
```

The API runs at `http://localhost:5070`; Swagger is at `http://localhost:5070/swagger`.

The API signing key is not an access token. After the API is running, a successful login issues a JWT. To test the login endpoint from PowerShell, use your own account:

```powershell
$loginBody = @{
    username = "<your username>"
    password = "<your password>"
} | ConvertTo-Json
$login = Invoke-RestMethod -Uri "http://localhost:5070/api/users/login" -Method Post -ContentType "application/json" -Body $loginBody
$login.AccessToken
```

Keep the returned token private. The website handles this API login for you.

In the second terminal, start the website:

```powershell
dotnet run --project .\QMSSystem.Web\QMSSystem.Web.csproj
```

Open `http://localhost:5048` and sign in with an account from the shared user list. Keep both terminal processes running while using the site.

## Build

```powershell
dotnet build .\QMSSystem.Api\QMSSystem.Api.csproj
dotnet build .\QMSSystem.Web\QMSSystem.Web.csproj
```
