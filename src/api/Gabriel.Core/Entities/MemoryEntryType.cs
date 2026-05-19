namespace Gabriel.Core.Entities;

// Categories for a memory entry. The choice of categories mirrors Claude
// Code's auto-memory schema so the agent's mental model of "what kind of
// thing am I writing down?" lines up with conventions users may already
// know.
//
//   User      — facts about the person we're talking to (role, expertise,
//               preferences). Used to tailor explanations and tone.
//   Feedback  — corrections or validations of approach ("don't do X", "yes,
//               keep doing Y"). Drives future behaviour without retraining.
//   Project   — info about ongoing work that isn't derivable from the code
//               (deadlines, stakeholder constraints, recent incidents).
//   Reference — pointers to where information lives outside the project
//               (dashboards, ticket systems, Slack channels, docs sites).
public enum MemoryEntryType
{
    User = 0,
    Feedback = 1,
    Project = 2,
    Reference = 3,
}
