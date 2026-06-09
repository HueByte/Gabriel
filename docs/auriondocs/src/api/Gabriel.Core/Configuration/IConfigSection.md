# IConfigSection.cs

> **Source:** `src/api/Gabriel.Core/Configuration/IConfigSection.cs`

## Contents

- [IConfigSection](#iconfigsection)
- [ConfigSectionExtensions](#configsectionextensions)

---

## IConfigSection

> **File:** `src/api/Gabriel.Core/Configuration/IConfigSection.cs`  
> **Kind:** interface

A marker-style interface intended for POCO configuration classes that enforces a compile-time, static SectionName string to live next to the type. Implement this when you want the configuration section name colocated with the options type so generic binding/registration helpers can find the correct IConfiguration section without creating an instance.

## Remarks
This interface uses the C# 11 static-abstract member pattern so generic code (for example, registration or binding extension methods) can read SectionName from TSelf without instantiating it. The generic self-referential constraint (TSelf : class, `IConfigSection<TSelf>`) encourages the common pattern where a type provides its own section name (the curiously recurring generic pattern). Keeping the section name with the type reduces the risk of mismatched names when renaming options classes.

## Example
```csharp
// Options POCO that declares its config section name next to the type
public class MyOptions : IConfigSection<MyOptions>
{
    public static string SectionName => "MyOptions";

    public string ConnectionString { get; set; }
}

// Typical registration in Startup/Program using IConfiguration
// (either an existing generic helper or manual binding)
var section = configuration.GetSection(MyOptions.SectionName);
services.Configure<MyOptions>(section);
// or
var opts = new MyOptions();
section.Bind(opts);
```

## Notes
- Requires C# 11 / .NET 7 (or later) support for static abstract interface members.
- Implementations must supply a public static string SectionName; it is not an instance member.
- The generic constraint requires implementing the interface with the concrete type itself (e.g., MyOptions : `IConfigSection<MyOptions>`).

---

## ConfigSectionExtensions

> **File:** `src/api/Gabriel.Core/Configuration/IConfigSection.cs`  
> **Kind:** class

Binds an options type to the IConfiguration section identified by that type's declared SectionName and returns the resulting OptionsBuilder so callers can continue configuring the options (for example by adding validation). Use this when your options type implements `IConfigSection<TOptions>` and declares its section name centrally to avoid repeating string keys.

## Remarks
This extension centralizes the mapping between an options type and its configuration section by relying on the options type's SectionName. It fits into the Microsoft Options + Configuration pattern: the method calls `AddOptions<TOptions>`() and Bind(...) internally, so it registers the options and applies configuration binding in one call while still returning an `OptionsBuilder<TOptions>` for further chaining (validators, post-configuration, etc.).

## Example
```csharp
// In Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args);
// MyOptions implements IConfigSection<MyOptions> and exposes a SectionName
builder.Services.ConfigureSection<MyOptions>(builder.Configuration)
    .Validate(o => o.SomeRequiredProperty != null, "SomeRequiredProperty is required");
```

## Notes
- TOptions must implement `IConfigSection<TOptions>` and provide a SectionName that matches a key in IConfiguration; if the section is missing, the binder will produce default values for properties.
- The returned `OptionsBuilder<TOptions>` allows you to chain validation (.Validate), post-configuration (.PostConfigure), etc.
- Binding behavior follows the standard ConfigurationBinder rules (nested objects and collections are supported according to the binder's conventions).

---