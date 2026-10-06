# QMS

QMS is a .NET 10 starter containing a basic landing page, database-backed login, cookie authentication, and role-based access control. The previous domain-specific pages, controllers, data, and migrations have been removed so new features can be built on this foundation.

## Projects

- `QMSSystem.Web` - ASP.NET Core Razor Pages frontend with the landing, login, and access-denied pages.
- `QMSSystem.Api` - ASP.NET Core API with the database-backed login endpoint.
- `QMSSystem.Shared` - shared login and user/role models.

The configured roles are `Admin`, `Operator`, and `Supervisor`. Empty page folders for these roles are under `QMSSystem.Web/Pages`.

## Requirements

- .NET 10 SDK
- Network access to the shared SQL Server and Windows credentials with access to `TRG_CORE`

## Configuration

In Development, the API uses the shared `TRG_CORE` database on `cqmprddev1` with Windows Integrated Authentication. Each developer must be on a network that can reach that SQL Server and must have database permissions. The committed Development connection string contains no SQL username or password. Set `ConnectionStrings:DefaultConnection` in User Secrets or an environment variable to override it for another database.

Login sessions use the website's protected authentication cookie, so no per-clone signing secret needs to be configured.

### Login database tables

Login uses these SQL Server tables in the `dbo` schema:

| Table name | Purpose |
| --- | --- |
| `dbo.KS_RecallUsers` | Usernames, passwords, account status, and profile details |
| `dbo.KS_Roles` | Available role names |
| `dbo.KS_UserRoles` | Links users to their roles |

To create any of these tables that are missing and add the standard roles, connect to `TRG_CORE` in SQL Server Management Studio and run [`database/login-schema.sql`](database/login-schema.sql). The script leaves existing tables unchanged; it does not create users or modify existing accounts.

Login requires a user row with `RegistrationStatus = 'Registered'` and `IsActive = 1`. Assign at least one role through `KS_UserRoles`; supported role names are `Admin`, `Operator`, and `Supervisor`. There is no user registration flow, so accounts must be provisioned separately.

## Run locally

Start the API in one terminal:

```powershell
dotnet run --project QMSSystem.Api
```

The API listens at `http://localhost:5070` with the included HTTP launch profile. Swagger is at `http://localhost:5070/swagger`. The shared Development database setting is included in the repository.

Start the frontend in another terminal:

```powershell
dotnet run --project QMSSystem.Web
```

The frontend listens at `http://localhost:5048`. It sends login requests to the API at `http://localhost:5070` and keeps the successful login in its protected cookie.

## Build

```powershell
dotnet build QMSSystem.Api
dotnet build QMSSystem.Web
```
