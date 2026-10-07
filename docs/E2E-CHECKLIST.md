# End-to-end checklist

Run this in the browser after every merge to `master`, with two operators and two supervisors. When a step fails, open a GitHub issue for the owner of that page and link it here.

Automated version: `dotnet test .\QMSSystem.Tests\QMSSystem.Tests.csproj` (the test `Full_chain_closes_the_deviation_when_the_new_revision_is_approved` runs the same journey).

## Full journey

- [ ] Operator A creates SOP-100 with a version 1 file → status Draft, revision Pending
- [ ] Supervisor A sees it in the inbox, approves → SOP-100 is Approved and shows on the operator home page
- [ ] Operator A clicks "Raise Deviation" on SOP-100, fills title, priority, proof → status Open
- [ ] Supervisor A accepts the deviation → status Investigating; it shows in the Submit Report picker
- [ ] Operator A submits report attempt 1 → Supervisor A rejects it with a comment
- [ ] Operator A submits report attempt 2 (attempt number shows 2) → Supervisor A accepts with "change required"
- [ ] The report shows in the Change Request picker; Operator A raises a change request (document and deviation filled automatically)
- [ ] Operator A submits it → change request Submitted, deviation AwaitingChange
- [ ] Supervisor A rejects it → deviation back to Investigating; the report shows in the picker again
- [ ] Operator A raises a new change request and submits it → Supervisor A approves
- [ ] The change request shows in the Upload Revision picker of SOP-100 (and not of other SOPs)
- [ ] Operator B uploads SOP-100 version 2 linked to the change request → Pending
- [ ] Supervisor B approves version 2 → SOP-100 is on version 2
- [ ] The deviation is **Closed automatically**
- [ ] Deviation Details shows the full timeline with names and dates, oldest first
- [ ] Audit trail shows every step; supervisor inbox counts are back to 0; admin reports updated

## Short path

- [ ] A deviation whose report is accepted with "no change needed" closes at once

## Rules

- [ ] A deviation cannot be raised on a Draft or Obsolete SOP
- [ ] A second report cannot be submitted while one is Pending
- [ ] A second change request cannot be raised while one is active for the same report
- [ ] A revision of an approved SOP cannot be uploaded without an approved change request
- [ ] A change request for SOP-100 cannot be used to revise SOP-200
- [ ] A supervisor cannot review an item they raised
- [ ] A deviation cannot be closed by hand while its change request is Submitted, or Approved without an approved revision
- [ ] An operator opening a `/Supervisor/...` page sees Access Denied
- [ ] Closed and Rejected deviations, approved revisions and submitted change requests are read-only
