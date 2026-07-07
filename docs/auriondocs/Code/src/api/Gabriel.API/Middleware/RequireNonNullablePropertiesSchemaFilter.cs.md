# RequireNonNullablePropertiesSchemaFilter

> **File:** `src/api/Gabriel.API/Middleware/RequireNonNullablePropertiesSchemaFilter.cs`  
> **Kind:** class

```csharp
public class RequireNonNullablePropertiesSchemaFilter : ISchemaFilter
```


Implements a schema filter that marks non-nullable C# reference-type properties as required in the OpenAPI schema so generated TypeScript clients treat them as non-optional fields. It mutates the concrete OpenApiSchema properties by adding camel-cased property names to the Required collection when the corresponding C# property is non-nullable.

## Remarks

By aligning C# non-nullability with OpenAPI required constraints, this filter helps keep generated clients in sync with the server model. It reads nullability information via NullabilityInfoContext and mutates the Required set, without changing the underlying nullable flag. It only affects the properties that exist in the generated schema and uses a simple camelCase mapping to property names.

## Example
```csharp
// Typical usage in SwaggerGen setup
services.AddSwaggerGen(c =>
{
    c.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
});
```

## Notes
- Matches property keys using camelCase transform of the C# property name; ensure the OpenAPI schema uses matching keys (naming attributes like JsonPropertyName may affect alignment).
- Relies on the project's nullable reference type context to determine nullability; enable nullable reference types to get meaningful results.
- Only runs when the schema contains properties; if a property is not present in the OpenAPI schema, it will not be added to Required.