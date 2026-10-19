using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Services.Matching;

namespace TechNova.RMS.ConsoleApp.Application.Services.Matching;

public interface IMatcher
{
    Task<IEnumerable<MatchResult>> FindMatchesAsync(int projectId, DateOnly start, DateOnly end);
}
