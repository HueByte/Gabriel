using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gabriel.API.Middleware;

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
