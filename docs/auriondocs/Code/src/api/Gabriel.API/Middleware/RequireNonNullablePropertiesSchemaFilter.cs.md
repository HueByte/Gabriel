# RequireNonNullablePropertiesSchemaFilter

> **File:** `src/api/Gabriel.API/Middleware/RequireNonNullablePropertiesSchemaFilter.cs`  
> **Kind:** class

```csharp
public class RequireNonNullablePropertiesSchemaFilter : ISchemaFilter
```


Adds non-nullable reference-type properties from C# into OpenAPI's required set, so generated TypeScript clients emit them as non-optional fields. The filter examines CLR properties of the target type, uses NullabilityInfoContext to determine which properties are non-nullable, and, when a matching OpenAPI property exists (by camel-cased name), marks it as required in the schema. It only mutates the concrete OpenApiSchema instance (the interface’s Required is read-only).

## Remarks
Sits in the Swagger/OpenAPI generation pipeline to align C# nullability with OpenAPI semantics. By mutating the concrete schema's Required collection based on CLR nullability, it ensures non-nullable properties surface as required in generated clients. The ToCamel naming ensures compatibility with common OpenAPI naming conventions.

## Notes
- Only affects properties that have a corresponding key in concrete.Properties; if no such property exists in the OpenAPI schema, nothing is changed.
- Initializes the Required collection if it is null before adding names, enabling the mutation on the concrete schema.
- Relies on a shared NullContext to inspect property nullability via reflection; the operation is read-only for the interface and mutates the concrete schema when applicable.