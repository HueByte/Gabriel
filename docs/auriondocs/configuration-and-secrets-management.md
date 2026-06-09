# Configuration and Secrets Management

> How the application sources and merges secrets/configuration from Infisical and other configuration layers.

This guide explains how the application sources and merges secrets and configuration from a self-hosted Infisical instance into the .NET IConfiguration system. It focuses on the small set of types that bind Infisical connection settings, register an Infisical-backed configuration source, and pull secrets at startup so the rest of the app can consume them via IConfiguration/IOptions. Read this when you need to understand where secrets come from and how to register Infisical as a configuration layer.

## InfisicalConfigurationProvider.cs

Pulls secrets from Infisical at startup and merges them into IConfiguration.

[InfisicalConfigurationProvider.cs](Code/src/api/Gabriel.API/Configuration/InfisicalConfigurationProvider.cs.md) is the runtime piece that actually performs the secret retrieval and merges the results into the application's IConfiguration data store. In the overall flow this provider is created by the configuration source and is responsible for calling Infisical (using the options supplied) and inserting returned key/value pairs into IConfiguration so they can be read through IConfiguration or bound to IOptions. Treat this file as the implementation that bridges the external secret store and the in-process configuration representation.

## InfisicalConfigurationSource.cs

Wraps InfisicalOptions to produce an Infisical-backed configuration provider.

[InfisicalConfigurationSource.cs](Code/src/api/Gabriel.API/Configuration/InfisicalConfigurationSource.cs.md) encapsulates the wiring that turns configured Infisical connection settings into a concrete configuration provider. It holds the bound InfisicalOptions and implements the IConfigurationSource contract so that when the configuration system builds sources it can instantiate an InfisicalConfigurationProvider. This file is the factory-like glue between the options describing how to talk to Infisical and the provider that will fetch and merge secrets.

## InfisicalExtensions.cs

Registers Infisical-backed configuration source into the configuration builder.

[InfisicalExtensions.cs](Code/src/api/Gabriel.API/Configuration/InfisicalExtensions.cs.md) exposes the extension method used at startup to add Infisical as a configuration source on an IConfigurationBuilder. It accepts the standard options-pattern delegate, produces an InfisicalConfigurationSource (bound with the provided InfisicalOptions), and adds it to the builder. Use this extension from Program.cs or host setup code to include Infisical in the application's configuration pipeline.

## InfisicalOptions.cs

Holds the Infisical connection/endpoint configuration values.

[InfisicalOptions.cs](Code/src/api/Gabriel.Core/Configuration/InfisicalOptions.cs.md) defines the shape of the configuration used to connect to the Infisical secret provider. This type is intended to be bound from the application's configuration (the "Infisical" section or environment variables) and then consumed by the source and provider so they know endpoint, credentials, and other connection details. It is the single place to look for which connection settings drive the Infisical-backed configuration integration.

These pieces collaborate in a linear startup flow: the application calls the extension on the configuration builder to register an Infisical-backed source, the extension constructs an InfisicalConfigurationSource with an InfisicalOptions instance, the source builds an InfisicalConfigurationProvider, and the provider pulls secrets from Infisical at startup and merges them into IConfiguration so the rest of the app can consume them via IConfiguration/IOptions.

---
*Synthesised by Aurion on 2026-06-09 03:22:21 UTC*
