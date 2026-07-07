# IConfigSection.cs

> **Source:** `src/api/Gabriel.Core/Configuration/IConfigSection.cs`

## Contents

- [IConfigSection](#iconfigsection)
- [ConfigSectionExtensions](#configsectionextensions)

---

## IConfigSection
> **File:** `src/api/Gabriel.Core/Configuration/IConfigSection.cs`  
> **Kind:** interface

```csharp
public interface IConfigSection<TSelf> where TSelf : class, IConfigSection<TSelf>
```


Represents a contract for configuration section types bound from appsettings. Any POCO that binds a configuration section should implement `IConfigSection<TSelf>` to expose its SectionName as a static, type-level value. The static abstract SectionName member allows generic code (for example, configuration binders) to obtain the section name without creating an instance of the type, ensuring there is a single source of truth for the section name and reducing drift between the type and the configuration key.

## Remarks
At the architectural level, this interface enables static polymorphism: generic code can read TSelf.SectionName to discover the corresponding appsettings section without instantiating the POCO. This centralizes the binding logic, reduces boilerplate, and minimizes the risk of mismatches between a type and its configuration key. The constraint `TSelf : class, IConfigSection<TSelf>` guarantees that the type participates in the static contract; if a type omits the static SectionName, code that relies on the contract will fail to compile. Leveraging C# 11 static abstract members, this pattern provides type-safe access to metadata directly from the type parameter.

## Example
```csharp
public class DatabaseSettings : IConfigSection<DatabaseSettings>
{
    public static string SectionName => "Database";
    // other properties bound from appsettings...
}

public static class ConfigBinder
{
    // Reads the section name from the type argument without instantiation
    public static TSection Bind<TSection>(IConfiguration config)
        where TSection : class, IConfigSection<TSection>
    {
        var section = config.GetSection(TSection.SectionName);
        return section.Get<TSection>();
    }
}
```

## Notes
- Requires C# 11 and static abstract interface members; your compiler and tooling must support this feature.
- The SectionName value must correspond to a key in your appsettings under which the configuration for that POCO lives.
- Access to SectionName is via the generic type parameter (e.g., TSection.SectionName) rather than an instance property; avoid calling on an instance to preserve the static contract.


---

## ConfigSectionExtensions
> **File:** `src/api/Gabriel.Core/Configuration/IConfigSection.cs`  
> **Kind:** class

```csharp
public static class ConfigSectionExtensions
```


ConfigSectionExtensions.`ConfigureSection<TOptions>` is a compact DI helper that binds a strongly-typed options class to the configuration section named by TOptions.SectionName. It registers the options type with the options system and returns an `OptionsBuilder<TOptions>` so you can chain validators and additional configuration before consuming `IOptions<TOptions>`.

## Remarks
This abstraction centralizes the common pattern of binding an options POCO to a named configuration section, allowing consumers to keep startup code concise. It relies on TOptions exposing the SectionName (via `IConfigSection<TOptions>`) and yields an `OptionsBuilder<TOptions>` for fluent validation and further configuration, aligning with standard IOptions usage.

## Example
```csharp
public interface IMySettings : IConfigSection<IMySettings>
{
    static string SectionName => "MySettings";
    string Endpoint { get; set; }
}

public void ConfigureServices(IServiceCollection services, IConfiguration config)
{
    services.ConfigureSection<IMySettings>(config)
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint),
                      "Endpoint must be configured");
}
```

## Notes
- The binding uses config.GetSection(TOptions.SectionName). If the section is missing, the options object will still be constructed with default values, so you may want to enforce presence via validation.
- The extension returns an `OptionsBuilder<TOptions>`, enabling fluent validation and further options configuration before the container is built.
- SectionName must be provided as a static member on TOptions (via `IConfigSection<TOptions>`); mismatches will surface as compile-time constraints.

---