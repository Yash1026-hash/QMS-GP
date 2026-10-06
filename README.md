# QMS

QMS is a .NET 10 starter containing a basic landing page, login page, JWT authentication, and role-based access control. The previous domain-specific pages, controllers, data, and migrations have been removed so new features can be built on this foundation.

## Projects

- `QMSSystem.Web` - ASP.NET Core Razor Pages frontend with the landing, login, and access-denied pages.
- `QMSSystem.Api` - ASP.NET Core API with the login endpoint, JWT issuance, and role authorization.
- `QMSSystem.Shared` - shared login and user/role models.

The configured roles are `Admin`, `Operator`, and `Supervisor`. Empty page folders for these roles are under `QMSSystem.Web/Pages`.

## Requirements

- .NET 10 SDK
- SQL Server with the user and role tables expected by the API

## Configuration

The API requires a database connection string and a JWT signing key. Configure both as .NET User Secrets in the API project directory:

```powershell
cd QMSSystem.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your SQL Server connection string>"
dotnet user-secrets set "Jwt:SigningKey" "<a private key of at least 32 bytes>"
```

Do not commit connection strings, signing keys, or other secrets. JWT issuer and audience default to `QMS.Api` and `QMS.Web`; configure `Jwt:Issuer` and `Jwt:Audience` in User Secrets if needed.

The repository currently contains no migrations, seed users, or registration flow. Provide compatible user, role, and user-role tables and accounts in the database before signing in.

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

The frontend listens at `http://localhost:5048`. It sends login requests to the API at `http://localhost:5070`, and forwards the issued bearer token on authenticated API requests.

## Build

```powershell
dotnet build QMSSystem.Api
dotnet build QMSSystem.Web
```
