# QMS

QMS is a .NET 10 starter containing only the landing, login, and access-denied pages. New Admin, Operator, and Supervisor pages can be built from this baseline; role-specific page folders are protected by cookie-based authorization.

## Projects

- `QMSSystem.Web` - ASP.NET Core Razor Pages frontend with the landing, login, and access-denied pages.
- `QMSSystem.Api` - ASP.NET Core API with the login endpoint and cookie-based role authorization.
- `QMSSystem.Shared` - shared login and user/role models.

The configured roles are `Admin`, `Operator`, and `Supervisor`. After login, the role assignments loaded from the database are stored as claims in the protected `QMS.Auth` cookie. The Web app restricts `/Admin`, `/Operator`, and `/Supervisor` pages to their matching role. The API also requires an authenticated cookie by default and allows anonymous access only to login.

## Requirements

- .NET 10 SDK
- SQL Server with the user and role tables expected by the API

## Configuration

Login is database-backed. The API needs a connection to the user, role, and user-role tables. Configure a connection string with .NET User Secrets in the API project directory when the shared database is unavailable:

```powershell
cd QMSSystem.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your SQL Server connection string>"
```

The Web app and API share the `QMS.Auth` authentication cookie and Data Protection keys. Locally, both use the current user's local application data directory. In deployments where they run under different identities or machines, configure `DataProtection:KeysPath` to the same protected, shared directory for both apps. Do not expose or commit the key directory.

The API reads `dbo.KS_RecallUsers`, `dbo.KS_Roles`, and `dbo.KS_UserRoles`. The [login schema](database/login-schema.sql) creates any missing tables and the three role names; it does not create user accounts. Accounts must be active, registered, and assigned to roles in the database. There is no local-account fallback, migration, or registration flow, so login requires the database connection to be available.

## Run locally

Start the API in one terminal:

```powershell
dotnet run --project QMSSystem.Api
```

The API listens at `http://localhost:5070` with the included HTTP launch profile. Swagger is at `http://localhost:5070/swagger`.

Start the frontend in another terminal:

```powershell
dotnet run --project QMSSystem.Web
```

The frontend listens at `http://localhost:5048`. It sends login requests to the API at `http://localhost:5070` and forwards only its authenticated cookie on API requests. The API and Web app must share their Data Protection keys; locally they use the current user's local application data directory. For deployments on different identities or machines, configure `DataProtection:KeysPath` to the same protected directory for both apps.

## Build

```powershell
dotnet build QMSSystem.Api
dotnet build QMSSystem.Web
```
