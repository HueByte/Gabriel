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


Defines a contract that any configuration POCO bound from appsettings must implement so the configuration section name is stored on the type itself. By using a static abstract member SectionName, the implementing type provides the section name without requiring an instance. This enables generic code (for example, a configuration binder or registrar using a TSelf generic parameter) to read the section name directly from the type (TSelf.SectionName) and bind the corresponding configuration section to the POCO in a type-safe way. The pattern keeps a single source of truth for the section name next to the type and simplifies refactoring because renaming the type and its section name happens in one place.

## Remarks
Static abstract members on interfaces are a C# 11 feature. To use this symbol, your project must be compiled with a language version that supports static abstract interface members. The generic constraint `TSelf : class, IConfigSection<TSelf>` ensures you can access SectionName from the type argument without instantiating it, which is ideal for startup-time binding scenarios. This design ties the section name to the type, reducing drift between type names and configuration keys.

## Notes
- Access SectionName on the concrete type in a generic context (e.g., `TSelf.SectionName`); do not rely on an instance.
- The compiler enforces that each implementing type provides a SectionName; a mismatch will fail compilation.
- Requires C# 11 or newer and a compiler that supports static abstract interface members.

---

## ConfigSectionExtensions
> **File:** `src/api/Gabriel.Core/Configuration/IConfigSection.cs`  
> **Kind:** class

```csharp
public static class ConfigSectionExtensions
```


ConfigSectionExtensions.`ConfigureSection<TOptions>` wires an app configuration section to a strongly-typed options class that implements `IConfigSection<TOptions>`, using the static SectionName defined on TOptions to locate the section. It returns an `OptionsBuilder<TOptions>` so you can continue to compose per-options validators (e.g. .Validate(...)) without losing the fluent Options pattern.

## Remarks
This abstraction centralizes the mapping between a configuration section and its options type, ensuring a consistent naming convention across the codebase. By requiring TOptions to declare a SectionName (via `IConfigSection<TOptions>`), the binding remains decoupled from a particular configuration source and remains easy to test and reuse.

## Notes
- This method relies on TOptions implementing `IConfigSection<TOptions>` and providing a static SectionName; otherwise the call will not compile.
- If the configuration does not contain the specified section, binding will yield default values; consider adding validators or fallbacks to enforce required settings.


---