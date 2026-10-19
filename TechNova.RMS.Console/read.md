TechNova RMS Console

Overview

This is the TechNova RMS console application. It provides a simple in-memory/JSON-backed resource management system for employees, projects, and allocations used for training and demo purposes.

Quick start

- Build: dotnet build
- Run: dotnet run --project TechNova.RMS.Console/TechNova.RMS.Console.csproj

Data files

Persistent data files are stored under your local app data folder by default:
%LocalAppData%/TechNova.RMS (or change via TECHNOVA_RMS_DATA_DIR env variable)

Notes

- Markdown files placed at project root for easy transfer; they do not affect the build.
- See design.md for architecture notes.
