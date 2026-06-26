# IConfigSection.cs

> **Source:** `src/api/Gabriel.Core/Configuration/IConfigSection.cs`

## Contents

- [IConfigSection](#iconfigsection)
- [ConfigSectionExtensions](#configsectionextensions)

---

## IConfigSection

> **File:** `src/api/Gabriel.Core/Configuration/IConfigSection.cs`  
> **Kind:** interface

A marker interface for configuration POCOs that requires each implementing type to expose a compile-time SectionName. Use this on types that are bound from appsettings (or any IConfiguration) so the section name lives next to the type and generic helpers can locate the correct configuration section without hard-coded strings.

## Remarks
This interface encodes a single source-of-truth for a configuration section name by requiring a static abstract SectionName on the implementing type. The static-abstract member (C# 11) allows generic methods to access the section name via the type parameter rather than via an instance, enabling patterns like generic binding helpers or DI registration extensions that can discover the section name at compile time.

## Example
```csharp
// POCO that represents settings under the "MyFeature" section
public class MyFeatureOptions : IConfigSection<MyFeatureOptions>
{
    public static string SectionName => "MyFeature";

    public string? Url { get; set; }
    public int TimeoutSeconds { get; set; }
}

// A simple generic binder that reads the static SectionName and binds the section
public static class ConfigurationExtensions
{
    public static TSelf BindSection<TSelf>(this IConfiguration configuration)
        where TSelf : class, IConfigSection<TSelf>, new()
    {
        var section = configuration.GetSection(TSelf.SectionName);
        var instance = new TSelf();
        section.Bind(instance);
        return instance;
    }
}

// Usage
// var options = configuration.BindSection<MyFeatureOptions>();
```

## Notes
- Static abstract interface members require C# 11 (language support) — the compiler must support static interface members.
- SectionName is a static member on the concrete type; it cannot be accessed from an interface instance. Generic helpers must be constrained on the type parameter (e.g. where T : `IConfigSection<T>`) to read it.
- If your binder needs to create an instance, add a new() constraint (as shown) or use Activator.CreateInstance; otherwise you cannot call instance members for binding without an object to bind into.

---

## ConfigSectionExtensions

> **File:** `src/api/Gabriel.Core/Configuration/IConfigSection.cs`  
> **Kind:** class

Binds a configuration section to a strongly-typed options POCO by using the type's declared SectionName (from `IConfigSection<TOptions>`). Use this extension in your composition root (Startup/Program) when you want to register options and immediately continue configuring validators or other option-related behaviors without repeating the section name string.

## Remarks
This convenience extension centralizes the common pattern of reading a named section from IConfiguration and binding it to an options type. By constraining TOptions to `IConfigSection<TOptions>` the method avoids stringly-typed section names and keeps section metadata colocated with the options type. The method returns the `OptionsBuilder<TOptions>`, so callers can fluently add validators (e.g. .Validate(...)), data annotation validation, or further configuration.

## Example
```csharp
// In Program.cs / Startup.cs
services.ConfigureSection<MyOptions>(Configuration)
        .Validate(o => o.RequiredSetting != null, "RequiredSetting is missing");

// MyOptions must implement IConfigSection<MyOptions> and declare SectionName
```

## Notes
- TOptions must implement `IConfigSection<TOptions>`; the binding uses TOptions.SectionName to locate the configuration subsection.
- If the named section does not exist in the provided IConfiguration, Bind will produce an instance with default values (no exception is thrown) — ensure the configuration contains the expected section.
- The returned `OptionsBuilder<TOptions>` is intended to be used to register validation or additional configuration for the options; this method only performs the initial binding.

---