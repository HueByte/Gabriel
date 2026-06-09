# RequireNonNullablePropertiesSchemaFilter

> **File:** `src/api/Gabriel.API/Middleware/RequireNonNullablePropertiesSchemaFilter.cs`  
> **Kind:** class

```csharp
// Adds C# non-nullable reference-type properties to OpenAPI's `required` set
// so the generated TS client emits them as non-optional fields. SupportNonNullableReferenceTypes
// alone only affects `nullable:true/false`, not the `required` list.
public class RequireNonNullablePropertiesSchemaFilter : ISchemaFilter
{
    private static readonly NullabilityInfoContext NullContext = new();

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        // Required is read-only on the interface; cast to the concrete type to mutate.
        if (schema is not OpenApiSchema concrete) return;
        if (concrete.Properties is null || concrete.Properties.Count == 0) return;
        concrete.Required ??= new HashSet<string>();

        foreach (var prop in context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var info = NullContext.Create(prop);
            if (info.WriteState != NullabilityState.NotNull && info.ReadState != NullabilityState.NotNull)
                continue;

            var camelName = ToCamel(prop.Name);
            if (concrete.Properties.ContainsKey(camelName))
            {
                concrete.Required.Add(camelName);
            }
        }
    }

    private static string ToCamel(string s)
        => s.Length > 0 ? char.ToLowerInvariant(s[0]) + s[1..] : s;
}
```


Adds C# non-nullable reference-type properties to the OpenAPI schema's `required` set so generated TypeScript clients emit those fields as non-optional. Use this schema filter when you enable C# nullable reference types and want the OpenAPI `required` array to reflect non-nullable properties (the built-in `SupportNonNullableReferenceTypes` only affects `nullable: true/false`, not `required`).

## Remarks
This filter is intended for use with Swashbuckle/Swagger schema generation as an ISchemaFilter. It inspects the declaring .NET type's public instance properties with NullabilityInfoContext and, when a property's getter or setter is annotated as NotNull, adds the corresponding JSON property name (converted to camelCase) to the OpenAPI schema's Required set. The implementation casts the provided IOpenApiSchema to the concrete OpenApiSchema in order to mutate the Required collection.

## Example
```csharp
// Register the schema filter when configuring Swagger/Swashbuckle
services.AddSwaggerGen(c =>
{
    c.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
});
```

## Notes
- The filter converts CLR property names to camelCase before matching schema properties; if your serializer uses a different naming policy the names may not match and required entries won't be added.
- A property is treated as required if either its getter or setter nullability is NotNull (the code requires at least one of ReadState or WriteState to be NotNull).
- This mutates the OpenApiSchema by casting the interface to OpenApiSchema; it requires the concrete type used by Swashbuckle and will no-op if the cast fails.
- Ensure nullable reference types are enabled and annotations are present in the compiled assembly — otherwise nullability metadata will not indicate non-nullable references.