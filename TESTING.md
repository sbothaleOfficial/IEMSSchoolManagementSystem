# IEMS School Management System — QA & Verification Report

This document records the end-to-end testing of the IEMS School Management System:
every module was reviewed at the **backend logic**, **UI**, and **UX** level, with a
focus on **data integrity, durability, and consistency** (the data is real student and
financial records). Findings were fixed, captured as automated regression tests, and
verified in the running application.

- **Automated suite:** 128 checks, all passing (`IEMS.FunctionalTests`)
- **Build:** `IEMS.WindowsApp.sln` compiles clean (0 errors)
- **Install path verified:** .NET 8 SDK → `dotnet publish` (self-contained) → `publish\IEMS.exe`
- **Default login:** `admin` / `admin123` (the DB seeds this; see the doc-fix note below)

---

## 1. Test methodology

Three complementary layers were used:

1. **Automated integration harness** (`IEMS.FunctionalTests/`) — a console app that builds the
   same dependency-injection container as the WPF app (`App.xaml.cs`) and drives the **real**
   service + EF Core/SQLite layers against a fresh seeded database. It covers DB creation, seed
   integrity, authentication, CRUD across all modules, fee math, password hashing, unique
   constraints, foreign-key/delete guards, referential integrity, durability (persist across a
   new connection), and transaction atomicity.
2. **Source review** — every WPF window, service, repository, and the `ApplicationDbContext`
   were read for correctness, validation, FK handling, threading, and null-safety.
3. **Live UI testing** — the published app was driven through each module: login, navigation,
   grids, CRUD forms, validation paths, and the destructive/guarded operations.

> The harness is **not** part of `IEMS.WindowsApp.sln`, so it never affects a normal build.
> Run it with: `dotnet run -c Release --project IEMS.FunctionalTests`.

---

## 2. Module-by-module results

| Module | Tabs / areas | Verdict |
|--------|--------------|---------|
| **Students Management** | Dashboard, Students, Classes, Fee Management, Bonafide, Leaving Certificate, Bulk Promotion | Defects found & fixed |
| **Staff Management** | Teachers, Other Staff, Payslip Generation, Dashboard | Defects found & fixed |
| **Finance Management** | Electricity Bills, Other Expenses, Expense Analytics | Defects found & fixed |
| **Fee Management** (within Students) | Fee Payments, Fee Structure | Defects found & fixed |
| **User Management** | Users grid, Add/Edit, Reset Password, Toggle Status | Defects found & fixed |
| **Academic Year Management** | List, Add/Edit, Set Current, Delete | Defect found & fixed |
| **Transport Management** | Vehicles, Expenses, Dashboard | ✅ Clean — no defects |
| **Backup & Restore** | Manual Backup, Restore, Automatic Backup, History | ✅ Clean — durability-safe |
| **System Settings** | Settings editor, Reset, Clear Test Data | ✅ Clean — well hardened |

---

## 3. Bugs found and fixed

Listed newest-first with the commit that fixed each.

### Data integrity / correctness
- **Teacher save dropped 8 fields** (`67d646c`, merge `4013ea3`) — `TeacherService.Add/UpdateTeacherAsync`
  persisted only first/last name + employee ID, silently discarding salary, phone, address,
  joining date, email, bank account, Aadhaar and PAN. The Add/Edit Teacher form collected them
  but they never reached the DB.
- **Fee payment cheque/transaction column cross-contamination** (`90cbb0c`) — the single
  reference field was written into **both** `TransactionId` and `ChequeNumber` for every payment,
  regardless of method, so receipts showed the value in the wrong column. Now routed to the
  correct column only. Same commit consolidated the receipt's amount-in-words onto the corrected
  shared `AmountToWordsService` (the receipt previously used a buggy local copy that truncated paise).
- **Fee-structure soft-delete vs. unique index** (`eea1cb0`) — deleting a fee structure (soft
  delete) and re-creating the same class/fee-type/year failed with a cryptic
  `UNIQUE constraint failed`. `CreateFeeStructureAsync` now reactivates the soft-deleted row.
- **Bulk-promotion rollback corruption** (`6934ff4`) — the promotion transaction could leave
  students moved without matching history (or vice-versa) on partial failure. Made atomic.
- **Leaving Certificate "Standard" corruption** (`2e5d5ef`) — the certificate blindly appended
  `"th"` to the already-formatted standard, producing `Nurseryth`, `1stth`, `2ndth` on the
  official document. Also fixed the word-conversion that never matched the data format.
- **Finance "Staff Salaries ₹72 crore"** (`67d646c`) — the Overall analytics multiplied each
  salary by full tenure in months. Corrected to a sane period figure.
- **Finance analytics dropped the last day** (`b0123e3`) — the reporting period used an exclusive
  upper bound, so same-day records on the end date were excluded from totals.
- **Bulk Promotion UI FK failure** (`d756148`) — UI promotions didn't pass `AcademicYearId`,
  causing an FK error on execute.

### Security / lockout protection
- **Last-admin lockout via the Edit User form** (`a5a2439`) — the Edit form set `IsActive`/`Role`
  directly through `UpdateUserAsync`, bypassing the last-admin guard in `DisableUserAsync` and the
  self-disable guard in the UI. Disabling or demoting the last admin could lock everyone out.
  Now enforced in the **service layer** so no UI path can leave zero active admins.

### Robustness / friendly delete guards
- **Student / Class / Academic-Year delete FK guards** (`df5eb26`, `ebcc196`, `ad27792`) —
  deleting a record still referenced by child rows (fee payments, fee structures, promotion
  history, students) produced a raw `FOREIGN KEY constraint failed`. Each now surfaces a clear
  message instead.
- **Search null-reference crashes** (`df5eb26`, `a5a2439`) — student/user search called
  `.ToLower()` on nullable optional fields. Null-guarded.

### UI / UX defects
- **Electricity Bills Edit/Delete buttons did nothing** (`22b6aa2`) — they were wired to handlers
  that only acted on a `Tag` the toolbar buttons never carried, so Edit was reachable only by
  double-click and Delete had no working path at all. Switched to grid selection.
- **Payslip math unvalidated** (`eea03ab`) — negative deductions inflated net pay and deductions
  exceeding salary produced a negative net on the official payslip. Both now blocked.
- **Invisible form errors** (`d9931ab`) — `AddEditUser` showed errors at the bottom of a scroll
  panel; failed saves gave no feedback. Errors now also surface as a dialog.
- **Staff Dashboard off-thread + null-safety** (`7c92544`) — the dashboard ran heavy aggregation
  on the UI thread via a fake-async wrapper and could NRE on null gender. Reworked.
- **Staff position filter** (`86ab3a6`) — populated from a hard-coded list instead of actual data.
- **Bonafide certificate wording** (`757c49e`) — spelling/grammar in the official document.

### Documentation / packaging
- **Wrong documented password** (`e3e9972`) — README and the installer dialog said `Admin@123`,
  but the seeded password is `admin123`. Corrected.
- **Wrong executable name** (`e3e9972`) — `PublishAndRun.cmd`, `RunLatest.cmd` and
  `CreateInstaller.iss` referenced `IEMS.WPF.exe`; the real output is `IEMS.exe`. Corrected.

---

## 4. Modules verified clean (no defects)

- **Transport Management** — CRUD uses grid selection with friendly delete guards (vehicle with
  expenses is blocked at both UI and service); the expense form correctly enables fuel type only
  for FUEL, guards divide-by-zero on price-per-unit, validates amount/quantity with sane caps,
  and the date filter is inclusive of the end day.
- **Backup & Restore** — durability-safe. Restore validates the backup checksum, creates a
  pre-restore safety backup, **deletes the stale live `-wal`/`-shm`** before replacing the DB
  (preventing the classic "old WAL replayed onto restored DB" corruption), copies with retry and
  a temp rollback copy, and restarts. Backup creation was verified to produce a valid SQLite file
  (`SQLite format 3` header) on disk.
- **System Settings** — settings save is transaction-wrapped and validates each value (type,
  range, path, email, phone, pincode). **Clear Test Data** is triple-gated (seed-fingerprint
  safety check that refuses if real data was added, a warning, and a type-"DELETE" confirmation)
  and deletes children-before-parents in a single transactional `SaveChanges`.

---

## 5. Data integrity / durability / consistency checks (automated)

The harness asserts, among the 128 checks:

- **Seed integrity** — exact row counts across all 14 tables; exactly one current academic year.
- **Authentication** — valid/invalid/disabled/unknown all behave correctly; no user-enumeration leak.
- **Unique constraints** — duplicate student number, employee ID, vehicle number, receipt number,
  username, academic year, fee structure (class+type+year), and electricity bill (month+year) are
  all rejected.
- **Foreign-key / delete guards** — teacher-in-class, class-with-students, current academic year,
  student-with-payments, class-with-fee-structures, and academic-year-with-fee-structures all
  blocked with friendly errors; legitimately empty records still delete.
- **Referential integrity** — no orphaned fee payments / students / fee structures.
- **Durability** — data written through a service is visible through a brand-new DB connection.
- **Atomicity** — bulk promotion moves students and writes history together or not at all.
- **Money math** — fee calculation (remaining balance, overpayment, discount/late fee, flooring),
  late-fee accrual, and receipt amount-in-words (including paise) are correct.
- **Lockout protection** — the last admin cannot be disabled or demoted through any path.

---

## 6. How to reproduce

```bash
# Build
dotnet build IEMS.WindowsApp.sln -c Release

# Run the automated verification suite (128 checks)
dotnet run -c Release --project IEMS.FunctionalTests

# Publish a standalone build and run it
dotnet publish IEMS.WPF/IEMS.WPF.csproj -c Release -r win-x64 --self-contained true -o ./publish
./publish/IEMS.exe
```

Login with `admin` / `admin123`, then change the password immediately.
