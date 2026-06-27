# RefreshTokenRequest

> **File:** `src/api/Gabriel.API/Contracts/Auth/RefreshTokenRequest.cs`  
> **Kind:** record

Represents the request payload sent by a client when exchanging a refresh token for a new access (and optionally refresh) token. Use this record as the DTO in the authentication refresh endpoint or any internal API that accepts a single refresh-token string.

## Remarks
This is a minimal positional record intended to be serialized/deserialized (for example to/from JSON) as part of an API contract. It carries exactly one value: the RefreshToken string. Validation (presence, expiry, signature, etc.) is intentionally out of scope for this type and should be performed by the receiving layer (controller/service) or a validation pipeline.

## Example
```csharp
// Constructing the request
var request = new RefreshTokenRequest("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");

// Sending with HttpClient (requires System.Net.Http.Json)
// await httpClient.PostAsJsonAsync("/auth/refresh", request);

// Destructuring or accessing the token
string token = request.RefreshToken;
```

## Notes
- The record does not perform or enforce any validation; callers must ensure the token is present and well-formed before sending.
- Depending on the project's nullable reference types setting, RefreshToken may be null; treat it accordingly in validation logic.
- Being a positional record, the property is immutable (init-only) and supports record-style copying with the `with` expression if needed.