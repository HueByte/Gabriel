# MeResponse

> **File:** `src/api/Gabriel.API/Contracts/Auth/MeResponse.cs`  
> **Kind:** record

Represents a minimal, immutable data contract containing an authenticated user's identifier and email address. Reach for this record when sending or receiving basic "current user" information from authentication or profile API endpoints.

## Remarks
This is a positional C# record that exposes two init-only properties (Id and Email) and is intended as a lightweight DTO/contract between the API and its clients. Using a record gives value-based equality, concise construction/deconstruction syntax, and support for non-destructive mutation via the `with` expression, making it convenient for tests and response shaping.

## Example
```csharp
// Create a response
var me = new MeResponse(Guid.NewGuid(), "user@example.com");

// Deconstruct
var (id, email) = me;

// Create a modified copy
var updated = me with { Email = "new@example.com" };
```

## Notes
- Equality compares the values of Id and Email (record's value-based equality).
- Properties are init-only; mutate by creating a new instance with `with` rather than changing state.
- Nullability of Email depends on the project's nullable reference types setting; verify callers/consumers if nulls are possible.