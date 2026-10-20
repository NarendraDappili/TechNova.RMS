# TechNova.RMS — Week 1

TechNova RMS is a .NET 10 console application for managing employees, projects, skills, project staffing allocations, employee matching, and utilization reports. It is intentionally lightweight: data is stored locally as JSON and the application runs without a database or network service.

## Features

- List, add, update, deactivate, and manage skills for employees.
- List and manage projects, project status, dates, and required skills.
- Create and release allocations with employee availability and project-date validation.
- Prevent overlapping active allocations from exceeding 100% on any calendar day.
- Find available employees for a project with matched-skill, missing-skill, and score explanations.
- View bench employees below the 50% allocation threshold for today.
- View project staffing totals and department utilization for today.
- Persist employees, projects, allocations, skills, and requirements as readable JSON.
- Seed a fresh data directory with a realistic demonstration dataset.

## Prerequisites

- .NET 10 SDK, available from `dotnet --version`.
- Windows, macOS, or Linux with permission to create the application data directory.
- A terminal, or Visual Studio Community 2026 with the solution opened.

No database server, container runtime, cloud account, or additional environment is required.

## Build and test

From the repository root:

```text
dotnet restore TechNova.RMS.slnx
dotnet build TechNova.RMS.slnx -c Release
dotnet test TechNova.RMS.slnx -c Release
```

The test project is `TechNova.RMS.Tests` and uses in-memory repository doubles, so tests do not modify the runtime JSON files.

## Run the application

Run from the repository root:

```text
dotnet run --project TechNova.RMS.Console/TechNova.RMS.Console.csproj -c Release
```

The main menu is:

```text
1. Employee management
2. Project management
3. Allocation and release
4. Employee matching
5. Bench report
6. Project staffing summary
7. Department utilization
9. Exit
```

Select a number and follow the prompts. Dates are entered as `yyyy-MM-dd`; enum prompts accept names such as `Active`, `FullTime`, or `Contract`. Within employee and project management, blank optional update fields preserve their current values. Use `b` to return from a management submenu.

### Isolate a run's data

By default, files are written to `%LOCALAPPDATA%/TechNova.RMS` on Windows or the platform equivalent of the .NET Local Application Data folder. Set `TECHNOVA_RMS_DATA_DIR` to use a disposable or project-local directory.

PowerShell:

```powershell
$env:TECHNOVA_RMS_DATA_DIR = "$PWD/.rms-data"
dotnet run --project TechNova.RMS.Console/TechNova.RMS.Console.csproj
```

Bash:

```bash
export TECHNOVA_RMS_DATA_DIR="$PWD/.rms-data"
dotnet run --project TechNova.RMS.Console/TechNova.RMS.Console.csproj
```

To reset a demonstration run, exit the application and delete the configured data directory. The next run will seed it again.

## Sample console session

The exact dates and report values depend on the day the seed data was first created. The following shows the interaction shape rather than a fixed transcript:

```text
TechNova RMS - Main Menu
1. Employee management
2. Project management
3. Allocation and release
4. Employee matching
5. Bench report
6. Project staffing summary
7. Department utilization
9. Exit
4
Project ID: 1
Start (yyyy-MM-dd): 2026-01-10
End (yyyy-MM-dd): 2026-02-10
Alice Smith | Score: 12 | Matched: C#, SQL | Missing:
Bob Jones | Score: 10 | Matched: C# | Missing: SQL (need 3)
9
```

An allocation example:

```text
3
c) Create  r) Release  b) Back
c
Employee ID: 6
Project ID: 1
Start (yyyy-MM-dd): 2026-02-01
End (yyyy-MM-dd): 2026-02-15
Percentage (1-100): 50
Allocation 11 created.
```

If the requested dates fall outside the project or the employee's capacity would exceed 100%, the operation is rejected with an explanatory message and no allocation is saved.

## Seed data

On first startup, each empty collection is seeded independently:

- **15 employees** across Engineering, Quality, Services, Architecture, Operations, Design, Business, and Support, with employment type, designation, cost rate, and sample skills.
- **5 projects** (`Project Apollo`, `Project Beacon`, `Project Comet`, `Project Delta`, and `Project Echo`) with clients, statuses, relative dates, and requirements such as C#, SQL, Testing, Consulting, and Azure.
- **10 allocations** covering active staffing examples from 20% to 100%.

Employee and project identifiers are stable. Project and allocation dates are calculated relative to the current UTC date when the seed is first created. Seeding is non-destructive: if a collection already contains records, that collection is left unchanged. There is no automatic reset or merge of later seed changes into existing files.

## Storage and project structure

The application uses `System.Text.Json` through repository interfaces and DTOs. The `FileStore` writes `employees.json`, `projects.json`, and `allocations.json` using a temporary file before replacing the target file. It also supports migration of older JSON files that contain GUID-style identifiers.

- `TechNova.RMS.Console/Program.cs` — process entry point.
- `TechNova.RMS.Console/TechNova.RMS.ConsoleApp/Domain` — domain entities, value objects, and invariants.
- `TechNova.RMS.Console/TechNova.RMS.ConsoleApp/Application` — repository contracts and application services.
- `TechNova.RMS.Console/TechNova.RMS.ConsoleApp/Infrastructure` — JSON persistence and console notifications.
- `TechNova.RMS.Console/TechNova.RMS.ConsoleApp/ConsoleMenu.cs` — interactive presentation layer.
- `TechNova.RMS.Tests` — xUnit tests for allocation, matching, and reports.

See [DESIGN.md](DESIGN.md) for architecture diagrams, data flows, dependency details, and architecture decision records.

## Known limitations

- The application is single-user and local; JSON files do not provide database transactions or multi-process concurrency control.
- Reports are calculated on demand and are not historical snapshots.
- Allocation capacity checks iterate through every date and allocation, which is appropriate for the small demo dataset but not optimized for very large portfolios.
- Employee and project update prompts currently expose only selected fields; advanced bulk editing and deletion are not provided.
- There is no authentication, authorization, audit trail, or role-based access control.
- Notifications are console messages only; there is no email, queue, or external integration.
- Matching is a simple explainable score, not an optimization algorithm. It does not account for preferences, calendars, location, seniority, or travel constraints.
- Seed dates are relative to the first run and existing data is not automatically refreshed when the seed definition changes.
- Invalid or manually edited JSON causes an error rather than offering an interactive repair workflow.
