# MemoryEntryType

> **File:** `src/api/Gabriel.Core/Entities/MemoryEntryType.cs`  
> **Kind:** enum

```csharp
public enum MemoryEntryType
{
    User = 0,
    Feedback = 1,
    Project = 2,
    Reference = 3,
}
```


MemoryEntryType is an enumeration that labels the category of a memory entry used by the agent's memory system. It defines four categories—User, Feedback, Project, and Reference—so memory can be categorized consistently and retrieved with a clear intent. Use MemoryEntryType when creating a memory record to indicate what kind of information is being stored: User captures facts about the person the agent is interacting with (role, expertise, preferences); Feedback records corrections or validations that should influence future behavior; Project stores information about ongoing work constraints, deadlines, or stakeholder considerations; Reference points to external information sources such as dashboards, ticket systems, Slack channels, or docs. The explicit mapping (User = 0, Feedback = 1, Project = 2, Reference = 3) helps with deterministic serialization across components.

## Remarks
MemoryEntryType unifies memory classification and enables targeted retrieval and behavior tuning. By isolating these categories behind a single enum, the system can route memory entries to the appropriate subsystems (personalization, feedback loop, project planning, or external references) and extend the taxonomy in one place without touching business logic everywhere.

## Notes
- Be careful to classify content in the most specific category to avoid confusing retrieval.
- If interop with other languages or services relies on numeric values, keep the explicit assignments in sync to prevent serialization drift.