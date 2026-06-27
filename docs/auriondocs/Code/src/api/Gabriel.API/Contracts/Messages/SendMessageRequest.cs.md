# SendMessageRequest

> **File:** `src/api/Gabriel.API/Contracts/Messages/SendMessageRequest.cs`  
> **Kind:** record

Represents the request DTO for sending a message and carries the message text in the Content property. Use this record when accepting or sending message payloads over the Gabriel API (for example as an action parameter or the body of an HTTP POST).

## Remarks
This is a positional record with a single string property, Content. It is intended as a simple, immutable contract between clients and the API: it provides value-based equality, Deconstruct, and supports the record `with` expression to create modified copies. Keep it as a focused transport type and avoid adding business logic to the record.

## Example
```csharp
// Creating the request
var request = new SendMessageRequest("Hello, world!");

// ASP.NET Core controller action example
[HttpPost("/messages")]
public IActionResult SendMessage([FromBody] SendMessageRequest request)
{
    // use request.Content
    return Ok();
}

// Sending with HttpClient
await httpClient.PostAsJsonAsync("/messages", request);
```

## Notes
- The record is immutable by design: set values at construction time and use the `with` expression to produce modified copies.
- If nullable reference types are enabled in the project, Content is non‑nullable; otherwise it may be null at runtime—validate as needed on input.
- Serialization behavior (property name casing) depends on the configured JSON serializer settings.