TechNova RMS - Design Notes

Purpose

Provide a small training/sample console application demonstrating domain-driven design for employees, projects, and allocations with JSON-backed persistence.

Key components

- Domain: Employee, Project, Allocation, Skill, Proficiency
- Repositories: JSON file-backed repositories under Infrastructure/Json
- Services: AllocationService enforces allocation rules and notifies via INotificationService
- Matching: SimpleMatcher implements basic skill/availability matching
- Console UI: ConsoleMenu provides interactive operations and seeds data on first run

Persistence migration

- The app originally used Guid-based identifiers. The project optionally supports migration to integer ids for easier display; migration logic (FileStore) can convert existing GUID string ids in JSON to ints on first load.

Namespaces and placement

- Project files are placed under TechNova.RMS.Console (project root). Markdown files are added to the project root and do not participate in compilation.

Transfer guidance

- To move the project, copy the TechNova.RMS.Console folder and maintain the relative csproj path when running dotnet commands.
- No additional tooling required.

Extensibility

- Add unit tests under a separate test project.
- Replace FileStore with a database provider by implementing repository interfaces.

"Do no harm"

- The added markdown files are non-compiling assets and will not affect builds.
- If you revert ID changes or change persistence strategy, update SeedData and repositories accordingly.
