# Configuration and secret management via Infisical

> How the application reads and applies secrets/configuration from Infisical at startup and exposes it to the configuration system. This enables dynamic secrets handling in startup.

Configuration and secret management via Infisical

This guide explains how the application integrates a self-hosted Infisical secrets provider into ASP.NET Core's configuration system so secrets are fetched at startup and participate in normal IConfiguration/IOptions binding. The files below show the small surface: a POCO for options, an IConfigurationSource, a provider that pulls secrets, and an extension helper to wire everything into the IConfigurationBuilder.

## InfisicalConfigurationProvider.cs
Pulls secrets from a self-hosted Infisical instance at application startup and injects them into IConfiguration.

[InfisicalConfigurationProvider.cs](Code/src/api/Gabriel.API/Configuration/InfisicalConfigurationProvider.cs.md) is the runtime component that actually contacts Infisical (using the settings captured in the options type) and adds the returned secret key/value pairs into the configuration key/value collection. Implemented as an IConfigurationProvider, it runs during configuration building so the fetched secrets become regular configuration entries that can be read through IConfiguration or bound into IOptions<T>. The provider is the piece that translates remote secrets into the configuration system's in-memory representation.

## InfisicalConfigurationSource.cs
Provides an IConfigurationSource implementation that captures InfisicalOptions and produces InfisicalConfigurationProvider instances.

[InfisicalConfigurationSource.cs](Code/src/api/Gabriel.API/Configuration/InfisicalConfigurationSource.cs.md) encapsulates the InfisicalOptions and acts as the factory used by the configuration system. When the ConfigurationBuilder builds its providers, this source produces configured instances of the provider described above. It bridges the builder-time registration (where options are supplied) and the provider-time execution (where secrets are fetched), ensuring the provider receives the bound options needed to connect to Infisical.

## InfisicalExtensions.cs
Adds Infisical as a configuration source to an IConfigurationBuilder using the options-pattern.

[InfisicalExtensions.cs](Code/src/api/Gabriel.API/Configuration/InfisicalExtensions.cs.md) contains the extension method(s) used by application startup to register Infisical-backed configuration. Using the familiar options pattern, these extensions let callers supply or bind an InfisicalOptions instance and add the corresponding InfisicalConfigurationSource to the builder. This is the public integration point you call from Program.cs or a host builder to enable Infisical-based secrets in your app configuration.

## InfisicalOptions.cs
Holds options used to connect to an Infisical secrets provider.

[InfisicalOptions.cs](Code/src/api/Gabriel.Core/Configuration/InfisicalOptions.cs.md) is the POCO that carries the settings required to talk to the Infisical instance. It is intended to be bound from appsettings, environment variables, or user-secrets and then passed through the extension method into the configuration source so the provider can use those values at runtime.

How the pieces fit together

At startup you call the extension on IConfigurationBuilder to register Infisical with a configured InfisicalOptions instance; that extension adds an InfisicalConfigurationSource to the builder. When the configuration system builds, the source creates an InfisicalConfigurationProvider which uses the provided options to fetch secrets from the self-hosted Infisical instance and injects them into the IConfiguration key/value collection. The end result is that remote secrets participate in normal configuration access and IOptions binding without special-case code elsewhere in the application.

---
*Synthesised by Aurion on 2026-06-08 22:34:39 UTC*
