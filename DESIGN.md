# DESIGN (short)

Architecture: pragmatic single-project Clean style. Domain types in Domain folder; application services and repository interfaces in Application; JSON persistence in Infrastructure; composition root in Program/CompositionRoot.

Matching: SimpleMatcher scores candidates by matched skills and cost penalty; returns explanation of matched and missing skills.

Persistence: System.Text.Json to files under LocalAppData/TechNova.RMS; DTOs used to avoid graph issues.

Further details and ADRs should be expanded for the full assignment.
