using System;

namespace TechNova.RMS.ConsoleApp.Domain;

public sealed record Proficiency
{
    public int Value { get; }

    public Proficiency(int value)
    {
        if (value < 1 || value > 5) throw new ArgumentOutOfRangeException(nameof(value), "Proficiency must be between 1 and 5");
        Value = value;
    }

    public static implicit operator int(Proficiency p) => p.Value;
    public static explicit operator Proficiency(int v) => new Proficiency(v);
}
