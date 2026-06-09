# MessageResponse.cs

> **Source:** `src/api/Gabriel.API/Contracts/Messages/MessageResponse.cs`

## Contents

- [MessageResponse](#messageresponse)
- [MessageToolCall](#messagetoolcall)

---

## MessageResponse

> **File:** `src/api/Gabriel.API/Contracts/Messages/MessageResponse.cs`  
> **Kind:** record

Represents a single message returned by the API, including the authoring role, optional textual content, creation timestamp, and metadata used to group and order regenerated variants. Use this record when consuming message results from conversation endpoints where messages may have multiple regenerated variants or include tool invocation details.

## Remarks
This record encodes both the message payload and the regeneration/variant model used by the system. VariantGroupId ties together messages that are alternative generations (regenerations) of the same turn; for messages that were never regenerated the VariantGroupId equals the message Id. VariantIndex is a 0-based position within the variant group when ordered by CreatedAt, and VariantSiblingIds contains all sibling Ids (including self) sorted by CreatedAt. ToolCallId is present for messages with role "tool"; ToolCalls is populated on assistant messages that caused or requested tool calls. Content is nullable to cover cases such as assistant messages that contain only tool call information and no textual content. ReasoningContent holds optional model "thinking" output for providers that stream internal reasoning.

## Example
```csharp
var id = Guid.NewGuid();
var msg = new MessageResponse(
    Id: id,
    Role: "assistant",
    Content: "Here is the result.",
    CreatedAt: DateTimeOffset.UtcNow,
    VariantGroupId: id,
    VariantIndex: 0,
    VariantCount: 1,
    VariantSiblingIds: new[] { id },
    ToolCallId: null,
    ToolCalls: null,
    ReasoningContent: null
);
```

## Notes
- Content may be null for assistant messages that only include tool call information; callers should handle nulls.
- VariantSiblingIds is ordered by CreatedAt and includes the message's own Id; rely on VariantIndex and this ordering for presentation or comparison logic.
- ReasoningContent may contain streaming or large diagnostic output from the model — treat it as optional and potentially large.


---

## MessageToolCall

> **File:** `src/api/Gabriel.API/Contracts/Messages/MessageResponse.cs`  
> **Kind:** record

Represents a tool invocation attached to a message response, containing the tool's identifier, a human-readable name, and a JSON-encoded arguments payload. Use this record when conveying tool-call metadata across API boundaries or serializing tool requests/responses in message contracts.

## Remarks
This is a lightweight, immutable data transfer object implemented as a positional record. It favors a raw JSON string for the arguments payload to avoid coupling the contract to a specific CLR type; callers are expected to parse or validate ArgumentsJson as needed. The positional record gives value-based equality, deconstruction, and concise construction syntax.

## Example
```csharp
using System.Text.Json;

var call = new MessageToolCall(
    Id: "tool-123",
    Name: "web_search",
    ArgumentsJson: "{\"query\":\"weather in Seattle\",\"limit\":5}"
);

string json = JsonSerializer.Serialize(call);
Console.WriteLine(json);

// Deserialize (ensure your serializer supports parameterized records)
var deserialized = JsonSerializer.Deserialize<MessageToolCall>(json);
```

## Notes
- ArgumentsJson is a raw JSON string; it must be valid JSON and should be parsed by the consumer before use.
- Being a positional record, instances are immutable and compared by value; deconstruction and equality semantics follow record behavior.
- Some serializers require special settings to deserialize positional records (parameterized constructors); verify your serializer supports this pattern or provide a custom converter.

---