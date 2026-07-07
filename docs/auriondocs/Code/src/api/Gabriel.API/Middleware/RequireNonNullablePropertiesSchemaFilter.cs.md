# RequireNonNullablePropertiesSchemaFilter

> **File:** `src/api/Gabriel.API/Middleware/RequireNonNullablePropertiesSchemaFilter.cs`  
> **Kind:** class

```csharp
public class RequireNonNullablePropertiesSchemaFilter : ISchemaFilter
```


Adds C# non-nullable reference-type properties to OpenAPI's required set so the generated TS client emits them as non-optional fields. This schema filter mutates the OpenApiSchema to reflect C# nullability semantics during OpenAPI generation, ensuring that non-nullable references are treated as required.

## Remarks
Conceptually, it bridges the gap between C# nullability and OpenAPI's contract, ensuring the generated client enforces non-nullability at the type level. It inspects public properties, uses NullabilityInfoContext to determine whether a property is NotNull in the context of the type, and, when the property is represented in the OpenAPI schema under its camelCase name, marks it as required. The mutation happens in-place on the concrete OpenApiSchema to preserve compatibility with existing schema generation pipelines.

## Notes
- Relies on being able to cast the schema to OpenApiSchema; if the concrete type changes, this could break.
- Only adds to Required when a corresponding camelCase property is present in the OpenAPI schema.
- Does not alter the OpenAPI 'nullable' flags; it complements them by affecting 'required' instead.