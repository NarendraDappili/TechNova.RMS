using System;
using System.Collections.Generic;
using System.Linq;

namespace TechNova.RMS.ConsoleApp.Domain;

public sealed class Employee
{
    public Guid Id { get; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Designation { get; private set; }
    public string Department { get; private set; }
    public EmploymentType EmploymentType { get; private set; }
    public decimal CostRate { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Skill> _skills = new();
    public IReadOnlyCollection<Skill> Skills => _skills.AsReadOnly();

    public Employee(Guid id, string name, string email, string designation, string department, EmploymentType employmentType, decimal costRate)
    {
        Id = id != Guid.Empty ? id : throw new ArgumentException("Id required", nameof(id));
        Name = !string.IsNullOrWhiteSpace(name) ? name.Trim() : throw new ArgumentException("Name required", nameof(name));
        Email = !string.IsNullOrWhiteSpace(email) ? email.Trim() : throw new ArgumentException("Email required", nameof(email));
        Designation = !string.IsNullOrWhiteSpace(designation) ? designation.Trim() : throw new ArgumentException("Designation required", nameof(designation));
        Department = !string.IsNullOrWhiteSpace(department) ? department.Trim() : throw new ArgumentException("Department required", nameof(department));
        EmploymentType = employmentType;
        if (costRate <= 0) throw new ArgumentOutOfRangeException(nameof(costRate));
        CostRate = costRate;
        IsActive = true;
    }

    public void UpdateDetails(string? name, string? email, string? designation, string? department, EmploymentType? employmentType, decimal? costRate)
    {
        if (name is not null && string.IsNullOrWhiteSpace(name) == false) Name = name.Trim();
        if (email is not null && string.IsNullOrWhiteSpace(email) == false) Email = email.Trim();
        if (designation is not null && string.IsNullOrWhiteSpace(designation) == false) Designation = designation.Trim();
        if (department is not null && string.IsNullOrWhiteSpace(department) == false) Department = department.Trim();
        if (employmentType is not null) EmploymentType = employmentType.Value;
        if (costRate is not null)
        {
            if (costRate <= 0) throw new ArgumentOutOfRangeException(nameof(costRate));
            CostRate = costRate.Value;
        }
    }

    public void AddOrUpdateSkill(string name, Proficiency proficiency)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Skill required", nameof(name));
        var existing = _skills.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            _skills.Add(new Skill(name.Trim(), proficiency));
        }
        else
        {
            existing.UpdateProficiency(proficiency);
        }
    }

    public void RemoveSkill(string name)
    {
        var existing = _skills.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        if (existing is not null) _skills.Remove(existing);
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
