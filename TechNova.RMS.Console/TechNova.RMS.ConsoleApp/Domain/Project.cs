using System;
using System.Collections.Generic;
using System.Linq;

namespace TechNova.RMS.ConsoleApp.Domain;

public enum ProjectStatus { Proposed, Active, Completed, OnHold }

public sealed class Project
{
    public int Id { get; }
    public string Name { get; private set; }
    public string Client { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public ProjectStatus Status { get; private set; }

    private readonly List<(string SkillName, Proficiency MinProficiency)> _requirements = new();
    public IReadOnlyCollection<(string SkillName, Proficiency MinProficiency)> Requirements => _requirements.AsReadOnly();

    public Project(int id, string name, string client, DateOnly start, DateOnly end, ProjectStatus status)
    {
        Id = id > 0 ? id : throw new ArgumentException("Id required", nameof(id));
        Name = !string.IsNullOrWhiteSpace(name) ? name.Trim() : throw new ArgumentException("Name required", nameof(name));
        Client = !string.IsNullOrWhiteSpace(client) ? client.Trim() : throw new ArgumentException("Client required", nameof(client));
        if (end < start) throw new ArgumentException("End date must be on or after start date");
        StartDate = start;
        EndDate = end;
        Status = status;
    }

    public void Update(string? name, string? client, DateOnly? start, DateOnly? end, ProjectStatus? status)
    {
        if (name is not null && string.IsNullOrWhiteSpace(name) == false) Name = name.Trim();
        if (client is not null && string.IsNullOrWhiteSpace(client) == false) Client = client.Trim();
        if (start is not null) StartDate = start.Value;
        if (end is not null) EndDate = end.Value;
        if (StartDate > EndDate) throw new ArgumentException("Start must be on or before end");
        if (status is not null) Status = status.Value;
    }

    public void AddOrUpdateRequirement(string skillName, Proficiency minProf)
    {
        if (string.IsNullOrWhiteSpace(skillName)) throw new ArgumentException("Skill required", nameof(skillName));
        var idx = _requirements.FindIndex(r => string.Equals(r.SkillName, skillName, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) _requirements[idx] = (skillName.Trim(), minProf);
        else _requirements.Add((skillName.Trim(), minProf));
    }

    public void RemoveRequirement(string skillName)
    {
        var idx = _requirements.FindIndex(r => string.Equals(r.SkillName, skillName, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) _requirements.RemoveAt(idx);
    }
}
