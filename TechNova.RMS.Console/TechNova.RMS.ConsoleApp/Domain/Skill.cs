using System;
using System.Collections.Generic;

namespace TechNova.RMS.ConsoleApp.Domain;

public sealed class Skill : IEquatable<Skill>
{
    public string Name { get; }
    public Proficiency Proficiency { get; private set; }

    public Skill(string name, Proficiency proficiency)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Skill name required", nameof(name));
        Name = name.Trim();
        Proficiency = proficiency ?? throw new ArgumentNullException(nameof(proficiency));
    }

    public void UpdateProficiency(Proficiency p)
    {
        Proficiency = p ?? throw new ArgumentNullException(nameof(p));
    }

    public bool Equals(Skill? other)
    {
        if (other is null) return false;
        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
}
