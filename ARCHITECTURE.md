# IEMS School Management System — Architecture

A .NET 8 **WPF desktop** application for a single school, using **EF Core 8 + SQLite**
(offline-first, single-user). This document describes the current architecture and records
the architectural hardening done in June 2026 (before → after), so future maintainers know
why things are the way they are.

---

## 1. Solution layout

Clean/Onion-style layering. Dependencies point **inward** — outer layers depend on inner ones,
and the domain core depends on nothing.

```
        ┌─────────────────────────────────────────────┐
        │  IEMS.WPF  (presentation + composition root) │
        │  Views (code-behind), App.xaml.cs DI wiring  │
        └───────────────┬───────────────┬─────────────┘
                        │               │
                        ▼               ▼
        ┌───────────────────────┐   ┌────────────────────────────┐
        │   IEMS.Application     │   │     IEMS.Infrastructure     │
        │  Services (use cases)  │   │  EF Core DbContext,         │
        │  DTOs                  │   │  Repositories, Migrations   │
        └───────────┬───────────┘   └──────────────┬─────────────┘
                    │                               │
                    ▼                               ▼
                ┌───────────────────────────────────────┐
                │              IEMS.Core                  │
                │  Entities, Enums, Repository INTERFACES,│
                │  Domain services, Configuration         │
                │  (depends on nothing)                   │
                └───────────────────────────────────────┘
```

| Project | Responsibility | Key contents |
|---------|----------------|--------------|
| **IEMS.Core** | Domain core, no outward dependencies | Entities, enums, **repository interfaces** (`I*Repository`), domain services (`FeeCalculationService`, `AmountToWordsService`, `PasswordHashingService`, `PasswordPolicy`, promotion validators), `Configuration` (`BulkPromotionConfiguration`, `DatabaseLocation`) |
| **IEMS.Application** | Use-case orchestration; DTOs | Application services (`StudentService`, `FeePaymentService`, `UserService`, `BulkPromotionService`, `SystemSettingsService`, `BackupService`, …), DTOs, service interfaces. **Depends only on Core.** |
| **IEMS.Infrastructure** | Persistence | `ApplicationDbContext`, `Migrations`, repository **implementations**, design-time factory |
| **IEMS.WPF** | UI + composition root | XAML views + code-behind, `App.xaml.cs` (Generic Host DI), logging, global exception handling |
| **IEMS.FunctionalTests** | Verification harness | Console app (in the solution) that drives the real Application + Infrastructure layers against a throwaway SQLite DB — **130 checks** |

> The repository **interfaces live in Core**; their **implementations live in Infrastructure**.
> Application services depend on the Core interfaces, never on the concrete `DbContext`.

---

## 2. Key runtime decisions

### Composition root & DI
`App.xaml.cs` builds a Generic Host (`Host.CreateDefaultBuilder`) and registers repositories,
domain/application services, and windows. `App.ServiceProvider` exposes the root provider.

### Database location
`IEMS.Core.Configuration.DatabaseLocation` is the **single source of truth** for the SQLite
path: an absolute path next to the executable (`AppContext.BaseDirectory/school.db`). EF Core,
the design-time factory, and the backup service all use it, so they always target the same file.

### Schema management — **migrations**
Startup calls `Database.Migrate()` (not `EnsureCreated()`), via `App.MigrateDatabase()`, which
also **baselines** a legacy `EnsureCreated`-built database (schema present, no migration history)
by recording the existing migrations as applied before migrating. This means schema changes ship
as ordinary migrations and existing installs upgrade without data loss.

### DbContext lifetime — **scope per window**
`MainWindow` opens each module window inside its **own `IServiceScope`** (and therefore its own
short-lived `DbContext`), disposed when the modal window closes. No window shares a long-lived
context, so there's no cross-screen stale data or unbounded change-tracker growth.

### Logging & error handling
Serilog writes rolling daily logs to `%LOCALAPPDATA%\IEMS\logs` (31-day retention).
Global handlers (`DispatcherUnhandledException`, `AppDomain.UnhandledException`,
`TaskScheduler.UnobservedTaskException`) log and show a friendly dialog instead of crashing.

### Security
- Passwords: **PBKDF2-HMAC-SHA256, 600,000 iterations** (OWASP 2026), versioned hash format
  (`[iterations][salt][hash]`) so the cost can be raised again later; legacy 10k hashes still
  verify and are **transparently upgraded on next login**. Constant-time comparison.
- The seeded default admin (`admin` / `admin123`) ships with `MustChangePassword = true`, enforced
  by the login flow.
- Lockout protection: the last active administrator can't be disabled or demoted through **any**
  path (enforced in the service layer).

### Backup & restore (durability-safe)
`BackupService` copies the DB + WAL/SHM with a checksum, and on restore validates the checksum,
takes a pre-restore safety backup, **deletes the stale live WAL/SHM** before replacing the file
(preventing "old WAL replayed onto restored DB" corruption), and retries with a temp rollback copy.

---

## 3. Architectural hardening — before → after (June 2026)

The codebase was reviewed in depth (multi-agent) and the following structural issues were fixed.
Each is covered by the verification harness and/or a live UI test.

| Area | Before | After |
|------|--------|-------|
| **Layering** | `IEMS.Application` referenced `IEMS.Infrastructure` and injected the concrete `ApplicationDbContext` (inner layer depended on the ORM; not unit-testable in isolation) | Application depends only on Core. `SystemSettingsService` → `ISystemSettingRepository`; `BulkPromotionService` → `IStudentPromotionRepository` (transactions owned by Infrastructure). Project reference removed; an **architecture-guard test** asserts it can't return |
| **Schema** | `EnsureCreated()` — never records migration history, no-op once the DB exists; committed migrations were dead code → **no safe way to ship schema changes** | `Database.Migrate()` with legacy-DB baselining; deterministic `HasData` seed; a single regenerated `InitialCreate` matching the model |
| **DbContext lifetime** | One session-long `DbContext` shared by every window (and resolved off the root provider for Backup/Settings — a captive dependency) | Scope-per-window: each module window gets its own short-lived context, disposed on close |
| **Logging / crashes** | No file logging; audit events went to `Debug.WriteLine` (lost in release); **no global exception handler** → silent crashes | Serilog rolling-file logs; three global exception handlers; framework log noise filtered |
| **Security** | PBKDF2 **10,000** iterations, "locked for compatibility"; seeded admin with `MustChangePassword = false`; the secure random-admin path was dead code | 600k iterations, versioned + upgrade-on-login; seeded admin must change password at first login; last-admin lockout protection in the service layer |
| **DB path** | `"Data Source=school.db"` relative to the working directory (duplicated in 3 places) → launching from another folder opened a *different* DB | One absolute `DatabaseLocation` next to the exe |
| **Duplication** | Password rules copy-pasted into 3 places; amount-to-words duplicated | Password rules centralised in `IEMS.Core.Services.PasswordPolicy`; receipt amount-in-words uses the shared `AmountToWordsService` |
| **Hygiene / CI** | Empty `IEMS.Shared` project referenced for nothing; the test harness wasn't in the solution; no CI | `IEMS.Shared` removed; harness added to the solution; **GitHub Actions** builds + runs all 130 checks on every push |

### Functional bugs fixed earlier in the same effort
(See `TESTING.md` for the full list.) Highlights: Teacher save dropping 8 fields; fee-payment
cheque/transaction column cross-contamination; bulk-promotion rollback corruption; Leaving-Certificate
text corruption; Finance "₹72 crore" salary; dead Electricity-bill Edit/Delete buttons;
last-admin lockout via the Edit User form; friendly FK delete guards (student/class/academic-year).

---

## 4. Known residuals / future work

These are deliberately deferred — they're quality/scale improvements, not correctness issues,
and the acute risks are already mitigated (global crash handling, scope-per-window):

- **MVVM** — the WPF layer is large code-behind (no ViewModels/bindings). The biggest windows
  (`StudentsManagementWindow` ~2,400 LOC, `FinanceManagementWindow`) mix CRUD, reporting, Excel
  and printing. Extracting reporting/export/printing into services and migrating high-churn
  windows to MVVM (incrementally, e.g. CommunityToolkit.Mvvm) would help maintainability.
- **Optimistic concurrency** — no `RowVersion` tokens; last-write-wins. Low impact for a
  single-user app.
- **PII at rest** — Aadhaar/PAN/bank numbers are stored in plaintext in the SQLite file.
  Consider SQLCipher / column encryption if the file may leave the premises.
- **Repository unit-of-work** — repositories still `SaveChanges` per call; multi-entity business
  operations that should be atomic use ad-hoc transactions (bulk promotion now does this correctly
  via `IStudentPromotionRepository`).

---

## 5. Building, testing, running

```bash
# Build everything
dotnet build IEMS.WindowsApp.sln -c Release

# Run the 130-check verification suite
dotnet run -c Release --project IEMS.FunctionalTests

# Publish a standalone build (no .NET install needed on the target) and run it
dotnet publish IEMS.WPF/IEMS.WPF.csproj -c Release -r win-x64 --self-contained true -o ./publish
./publish/IEMS.exe        # login: admin / admin123  (change at first run)
```

Logs: `%LOCALAPPDATA%\IEMS\logs\iems-*.log` · Database: `school.db` next to the executable.
