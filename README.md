# TechNova.RMS — Week 1

Lightweight Resource Management System (RMS) console application.

Quick start

- Build: dotnet build -c Release
- Test: dotnet test tests/TechNova.RMS.Tests/TechNova.RMS.Tests.csproj -c Release
- Run: dotnet run --project TechNova.RMS.Console -c Release

Storage

Data is persisted to JSON files under your Local Application Data folder: %LOCALAPPDATA%/TechNova.RMS

Project structure

- TechNova.RMS.Console — single project containing Domain, Application, Infrastructure and Console UI
- tests/TechNova.RMS.Tests — xUnit tests exercising allocation rules and matching

Notes

- Program.cs is a composition root; business logic lives in services and domain classes.
- Seed data is deterministic and applied only when no existing runtime data files are present.
