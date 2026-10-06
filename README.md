# QMS

QMS is a .NET 10 starter containing a basic landing page, login page, JWT authentication, and role-based access control. The previous domain-specific pages, controllers, data, and migrations have been removed so new features can be built on this foundation.

## Projects

- `QMSSystem.Web` - ASP.NET Core Razor Pages frontend with the landing, login, and access-denied pages.
- `QMSSystem.Api` - ASP.NET Core API with the login endpoint, JWT issuance, and role authorization.
- `QMSSystem.Shared` - shared login and user/role models.

The configured roles are `Admin`, `Operator`, and `Supervisor`. Empty page folders for these roles are under `QMSSystem.Web/Pages`.

## Requirements

- .NET 10 SDK
- Network access to the shared SQL Server and Windows credentials with access to `TRG_CORE`

## Configuration

In Development, the API uses the shared `TRG_CORE` database on `cqmprddev1` with Windows Integrated Authentication. Each developer must be on a network that can reach that SQL Server and must have database permissions. The committed Development connection string contains no SQL username or password. Set `ConnectionStrings:DefaultConnection` in User Secrets or an environment variable to override it for another database.

The API also requires a private JWT signing key. Generate a unique key and save it in .NET User Secrets from the repository root:

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

Do not commit signing keys or other credentials. JWT issuer and audience default to `QMS.Api` and `QMS.Web`; configure `Jwt:Issuer` and `Jwt:Audience` in User Secrets if needed.

### Login database tables

The API maps login data to `dbo.KS_RecallUsers`, `dbo.KS_Roles`, and `dbo.KS_UserRoles`. To create any of these tables that are missing and add the standard roles, connect to `TRG_CORE` in SQL Server Management Studio and run [`database/login-schema.sql`](database/login-schema.sql). The script leaves existing tables unchanged; it does not create users or modify existing accounts.

Login requires a user row with `RegistrationStatus = 'Registered'` and `IsActive = 1`. Assign at least one role through `KS_UserRoles`; supported role names are `Admin`, `Operator`, and `Supervisor`. There is no user registration flow, so accounts must be provisioned separately.

## Run locally

Start the API in one terminal:

```powershell
dotnet run --project QMSSystem.Api
```

The API listens at `http://localhost:5070` with the included HTTP launch profile. Swagger is at `http://localhost:5070/swagger`. Each cloned developer checkout needs its own JWT signing key in User Secrets; the shared Development database setting is already in the repository.

Start the frontend in another terminal:

```powershell
dotnet run --project QMSSystem.Web
```

The frontend listens at `http://localhost:5048`. It sends login requests to the API at `http://localhost:5070`, and forwards the issued bearer token on authenticated API requests.

## Build

```powershell
dotnet build QMSSystem.Api
dotnet build QMSSystem.Web
```
