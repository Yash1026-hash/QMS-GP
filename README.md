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

## System Architecture & Specifications

### 1. Target Models (Domain Entities)

1. **`DocControlRequest`** (Document Control Request / Master)
   - Primary entity for controlled documents (e.g., SOPs, policies).
   - Tracks document number, title, department, current version, status (`Draft`, `PendingApproval`, `Approved`, `Obsolete`/`Superseded`), and active change request link.
2. **`DocControlApproval`** (Document Control Approval)
   - Stores approval decisions (`Approved`, `Rejected`), approver user ID, decision timestamp, and review comments for document registrations and revisions.
3. **`DeviationRequest`** (Deviation Request)
   - Tracks non-conformance / deviations raised against controlled documents.
   - Fields include deviation number, target document ID, title, description, severity (`Minor`, `Major`, `Critical`), status (`Open`, `PendingReview`, `Approved`/`Investigating`, `Rejected`, `Closed`), and reporter ID.
4. **`DeviationApproval`** (Deviation Approval / Initial Review)
   - Stores supervisor initial review decision (`Accepted`, `Rejected`), reviewer ID, review timestamp, and comments.
5. **`DeviationReportRequest`** (Deviation Investigation & CAPA Report)
   - Investigation and CAPA report for an accepted deviation.
   - Fields include deviation ID, attempt number (1, 2, ...), root cause, corrective action (CAPA), `ChangeRequired` flag (boolean), summary, status, and submitter ID.
6. **`DeviationReportApproval`** (Deviation Report Approval)
   - Stores supervisor review decisions (`Accepted`, `Rejected`), approver ID, review date, and comments for the investigation report.
7. **`ChangeRequest`** (Change Request)
   - Formal engineering/process change request triggered by an accepted deviation report.
   - Fields include change request number, target document ID, originating deviation ID, deviation report ID, change title, change type (`Process`, `Equipment`, `Software`, `Documentation`), status (`Draft`, `Submitted`, `Approved`, `Rejected`, `Implemented`), and requester ID.
8. **`ChangeRequestApproval`** (Change Request Approval)
   - Stores review decision (`Approved`, `Rejected`), approver ID, decision date, and review comments for the change request.

---

### 2. Target DTOs (Data Transfer Objects)

1. **`ControlledDocDto`** (Controlled Document DTO)
   - Used for document registration submission and general document view.
   - Contains document number, title, department ID, initial version, summary, file details, and creation metadata.
2. **`DeviationRequestDto`** (Deviation Request DTO)
   - Used for raising a deviation against an approved controlled document.
   - Contains target document ID, deviation title, description, severity, and reporter details.
3. **`ChangeRequestDto`** (Change Request DTO)
   - Used for creating and viewing change requests.
   - Contains title, description, change type, target document ID, and originating deviation/report identifiers.
4. **`ControlledDocHistoryDto`** (History DOC DTO)
   - Used for displaying version history and audit trail of a controlled document.
   - Contains history/revision ID, document ID, version number, change summary, linked change request ID/number, approval status, approved by, approval date, and file download URL.
5. **`ChangeRequestDeviationLinkDto`** (Change Request Deviation Child Table DTO)
   - Child table mapping linking a change request back to its source deviation and investigation report.
   - Contains change request ID, deviation ID, deviation number, deviation title, deviation report ID, attempt number, root cause, and proposed corrective action.
6. **`DeviationReportDto`** (Deviation Report DTO)
   - Used for submitting investigation findings and CAPA proposals.
   - Contains deviation ID, attempt number, root cause, corrective action, change required flag, and report summary.
7. **`DocModificationChangeRequestSelectorDto`** (Controlled Doc Child Table for Change Request Selection)
   - Child table/picker DTO used during **Document Modification**.
   - Lists only *Approved* change requests applicable to the selected document so the operator can select which approved change request authorizes the new version revision.

---

### 3. API Endpoints

#### Document Control APIs
- `POST /api/documents/register` — Initial Document registration submission (v1.0 / Draft).
- `POST /api/documents/modify` — Document modification submission (submitting a new version revision linked to an approved Change Request).
- `GET /api/documents` — List records API for controlled documents (with status, department, and search filters).
- `GET /api/documents/{id}` — View details API for a controlled document.
- `GET /api/documents/{id}/history` — View document version history audit trail (`ControlledDocHistoryDto`).
- `GET /api/documents/{id}/approved-change-requests` — List approved change requests available for this document to populate the modification selector table (`DocModificationChangeRequestSelectorDto`).
- `POST /api/documents/{id}/approve` — Approve or reject document registration / revision.

#### Deviation Management APIs
- `POST /api/deviations` — Deviation submission API (raise deviation against an approved controlled document).
- `GET /api/deviations` — List records API for deviations.
- `GET /api/deviations/{id}` — View details API for a deviation (including investigation reports and approvals).
- `POST /api/deviations/{id}/approve` — Initial supervisor review approval/rejection.
- `POST /api/deviations/{id}/reports` — Submit deviation investigation report (CAPA, root cause, change required flag).
- `POST /api/deviations/reports/{reportId}/approve` — Approve/reject deviation report.

#### Change Request APIs
- `POST /api/changerequests` — Change request submission API (linked to an accepted deviation report).
- `GET /api/changerequests` — List records API for change requests.
- `GET /api/changerequests/{id}` — View details API for a change request (including child deviation link information).
- `POST /api/changerequests/{id}/approve` — Approve or reject change request.

