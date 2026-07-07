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


Defines a contract that every POCO bound from appsettings must implement so the configuration section name lives next to the type—the single source of truth for the section's identity. The static abstract SectionName member enables generic code to read the name without instantiating TSelf, leveraging C# 11's static interface members to support type-driven configuration binding.

## Remarks
Co-locating the section name with the POCO prevents drift between the type and its configuration identity, simplifying refactoring and discovery. The self-referential generic constraint (TSelf : class, `IConfigSection<TSelf>`) and the static abstract member pattern empower strongly-typed, compile-time checks for the correct section name in generic configuration helpers. This approach requires a C# 11+ toolchain and compatible runtimes; ensure your project targets support for static interface members.

## Example
```csharp
public sealed class DatabaseSettings : IConfigSection<DatabaseSettings>
{
    public static string SectionName => "Database";
}
```

## Notes
- Implementations must be reference types because of the 'class' constraint; attempting to implement with a struct will fail to compile.
- SectionName is accessed statically as TSelf.SectionName; reading it from an instance is not applicable.

---

## ConfigSectionExtensions
> **File:** `src/api/Gabriel.Core/Configuration/IConfigSection.cs`  
> **Kind:** class

```csharp
public static class ConfigSectionExtensions
```


Extends IServiceCollection with a strongly-typed helper to bind a configuration section directly to an options class. By using TOptions.SectionName to locate the section, it hides the repetitive GetSection(...).Bind(...) boilerplate and returns an `OptionsBuilder<TOptions>` so you can continue configuring the options (for example, adding validators) in a fluent chain.

## Remarks

By encapsulating the standard pattern into an extension, this symbol promotes consistent, centralized configuration of options across the application. It relies on TOptions implementing `IConfigSection<TOptions>` to supply the SectionName, ensuring the binding targets the intended configuration subtree and enabling per-options validators to be attached via the returned builder.

## Notes

- If SectionName is incorrect or the configuration section is absent, the `Options<TOptions>` will bind to default values; pair with validators to surface misconfigurations.
- The generic constraint enforces that only options types providing a SectionName participate, preventing accidental misbindings.
- This method is a convenience wrapper; it does not introduce new binding semantics beyond what .`AddOptions<TOptions>`().Bind(section) provides.

---