# RequireNonNullablePropertiesSchemaFilter

> **File:** `src/api/Gabriel.API/Middleware/RequireNonNullablePropertiesSchemaFilter.cs`  
> **Kind:** class

Adds C# non-nullable reference-type properties to the OpenAPI schema's `required` set so generated TypeScript clients treat them as non-optional fields. Reach for this filter when your project uses C# nullable-reference-type annotations and you want those non-nullable properties to be represented as required in the emitted OpenAPI document (and thus in downstream client code), because the built-in nullable metadata alone only controls `nullable: true/false`, not the `required` list.

## Remarks
This schema filter examines each public instance property on the reflected CLR type using NullabilityInfoContext to determine whether the property is annotated as non-nullable (ReadState or WriteState == NotNull). When a matching property exists in the OpenAPI schema (matched by a simple camelCase conversion of the CLR property name), the filter adds that property name to the schema's Required set. The filter mutates the concrete OpenApiSchema (it is a no-op if the provided schema isn't the concrete type) and is intended to run during OpenAPI/Swagger generation so client generators see the required information.

## Example
```csharp
// Register the schema filter when configuring Swagger/Swashbuckle
services.AddSwaggerGen(c =>
{
    c.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
});
```

## Notes
- Requires runtime support for NullabilityInfoContext (introduced in .NET 5+) and meaningful nullable annotations in the source code for the nullability checks to be accurate.
- Property name matching uses a simple camelCase conversion of the CLR property name and does not respect JsonPropertyName attributes or serializer naming policies; if your JSON contract uses different naming, some properties may be missed.
- Only public instance properties are inspected; fields, non-public properties, or explicit interface implementations are ignored.