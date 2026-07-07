# HueByte/Gabriel — Documentation

*Generated from branch `main`*
*Commit: `8238aa07`*

## Languages

- **C#**: 200 files
- **TypeScript (React)**: 31 files
- **TypeScript**: 14 files
- **JavaScript**: 5 files

## Statistics

- **Files processed:** 250
- **Lines of code:** 22,515
- **Symbols documented:** 532
- **Validation retries:** 141

## Documentation Index

### prototype

- [generate](prototype/generate.js.md)
- [palettes](prototype/palettes.js.md)
- [patterns](prototype/patterns.js.md)
- [play](prototype/play.js.md)
- [run](prototype/run.js.md)

### src/api/Gabriel.API

- [Program](src/api/Gabriel.API/Program.cs.md)

### src/api/Gabriel.API/Configuration

- [GlobalRoutePrefixConvention](src/api/Gabriel.API/Configuration/GlobalRoutePrefixConvention.cs.md)
- [InfisicalConfigurationProvider](src/api/Gabriel.API/Configuration/InfisicalConfigurationProvider.cs.md)
- [InfisicalConfigurationSource](src/api/Gabriel.API/Configuration/InfisicalConfigurationSource.cs.md)
- [InfisicalExtensions](src/api/Gabriel.API/Configuration/InfisicalExtensions.cs.md)
- [LogDateEnricher](src/api/Gabriel.API/Configuration/LogDateEnricher.cs.md)

### src/api/Gabriel.API/Contracts/Auth

- [JwtResponse](src/api/Gabriel.API/Contracts/Auth/JwtResponse.cs.md)
- [LoginRequest](src/api/Gabriel.API/Contracts/Auth/LoginRequest.cs.md)
- [MeResponse](src/api/Gabriel.API/Contracts/Auth/MeResponse.cs.md)
- [RefreshTokenRequest](src/api/Gabriel.API/Contracts/Auth/RefreshTokenRequest.cs.md)
- [RegisterRequest](src/api/Gabriel.API/Contracts/Auth/RegisterRequest.cs.md)

### src/api/Gabriel.API/Contracts/Conversations

- [ContextMetricsResponse](src/api/Gabriel.API/Contracts/Conversations/ContextMetricsResponse.cs.md)
- [ConversationResponse](src/api/Gabriel.API/Contracts/Conversations/ConversationResponse.cs.md)
- [CreateConversationRequest](src/api/Gabriel.API/Contracts/Conversations/CreateConversationRequest.cs.md)
- [SetConversationModeRequest](src/api/Gabriel.API/Contracts/Conversations/SetConversationModeRequest.cs.md)
- [UpdateConversationRequest](src/api/Gabriel.API/Contracts/Conversations/UpdateConversationRequest.cs.md)

### src/api/Gabriel.API/Contracts/Diagnostics

- [MetricEntryDto](src/api/Gabriel.API/Contracts/Diagnostics/MetricEntryDto.cs.md)
- [WebSearchDiagnosticsResponse](src/api/Gabriel.API/Contracts/Diagnostics/WebSearchDiagnosticsResponse.cs.md)

### src/api/Gabriel.API/Contracts/Memories

- [MemoryDto](src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs.md)

### src/api/Gabriel.API/Contracts/Messages

- [MessageResponse](src/api/Gabriel.API/Contracts/Messages/MessageResponse.cs.md)
- [SendMessageRequest](src/api/Gabriel.API/Contracts/Messages/SendMessageRequest.cs.md)

### src/api/Gabriel.API/Contracts/Models

- [ModelDto](src/api/Gabriel.API/Contracts/Models/ModelDto.cs.md)

### src/api/Gabriel.API/Contracts/Projects

- [ProjectResponse](src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs.md)

### src/api/Gabriel.API/Contracts/Sequence

- [GabrielSequenceResponse](src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs.md)
- [SequenceCatalogResponse](src/api/Gabriel.API/Contracts/Sequence/SequenceCatalogResponse.cs.md)

### src/api/Gabriel.API/Controllers

- [AuthController](src/api/Gabriel.API/Controllers/AuthController.cs.md)
- [ConversationsController](src/api/Gabriel.API/Controllers/ConversationsController.cs.md)
- [DiagnosticsController](src/api/Gabriel.API/Controllers/DiagnosticsController.cs.md)
- [MemoriesController](src/api/Gabriel.API/Controllers/MemoriesController.cs.md)
- [ModelsController](src/api/Gabriel.API/Controllers/ModelsController.cs.md)
- [ProjectFilesController](src/api/Gabriel.API/Controllers/ProjectFilesController.cs.md)
- [ProjectsController](src/api/Gabriel.API/Controllers/ProjectsController.cs.md)
- [SequenceController](src/api/Gabriel.API/Controllers/SequenceController.cs.md)

### src/api/Gabriel.API/Identity

- [AuthCookies](src/api/Gabriel.API/Identity/AuthCookies.cs.md)
- [HttpContextCurrentUser](src/api/Gabriel.API/Identity/HttpContextCurrentUser.cs.md)
- [IdentitySeeder](src/api/Gabriel.API/Identity/IdentitySeeder.cs.md)

### src/api/Gabriel.API/Mapping

- [ContractMappings](src/api/Gabriel.API/Mapping/ContractMappings.cs.md)

### src/api/Gabriel.API/Middleware

- [GlobalExceptionHandler](src/api/Gabriel.API/Middleware/GlobalExceptionHandler.cs.md)
- [RequireNonNullablePropertiesSchemaFilter](src/api/Gabriel.API/Middleware/RequireNonNullablePropertiesSchemaFilter.cs.md)

### src/api/Gabriel.Core

- [DependencyInjection](src/api/Gabriel.Core/DependencyInjection.cs.md)

### src/api/Gabriel.Core/Configuration

- [AgentOptions](src/api/Gabriel.Core/Configuration/AgentOptions.cs.md)
- [AgentToolsOptions](src/api/Gabriel.Core/Configuration/AgentToolsOptions.cs.md)
- [AuthOptions](src/api/Gabriel.Core/Configuration/AuthOptions.cs.md)
- [BraveSearchOptions](src/api/Gabriel.Core/Configuration/BraveSearchOptions.cs.md)
- [GitHubDocsOptions](src/api/Gabriel.Core/Configuration/GitHubDocsOptions.cs.md)
- [GrokOptions](src/api/Gabriel.Core/Configuration/GrokOptions.cs.md)
- [IConfigSection](src/api/Gabriel.Core/Configuration/IConfigSection.cs.md)
- [InfisicalOptions](src/api/Gabriel.Core/Configuration/InfisicalOptions.cs.md)
- [JwtOptions](src/api/Gabriel.Core/Configuration/JwtOptions.cs.md)
- [LLMModel](src/api/Gabriel.Core/Configuration/LLMModel.cs.md)
- [LLMProviderOptions](src/api/Gabriel.Core/Configuration/LLMProviderOptions.cs.md)
- [LocalDocsOptions](src/api/Gabriel.Core/Configuration/LocalDocsOptions.cs.md)
- [ModelSelection](src/api/Gabriel.Core/Configuration/ModelSelection.cs.md)
- [PersonalityOptions](src/api/Gabriel.Core/Configuration/PersonalityOptions.cs.md)
- [ProjectFilesOptions](src/api/Gabriel.Core/Configuration/ProjectFilesOptions.cs.md)
- [TavilySearchOptions](src/api/Gabriel.Core/Configuration/TavilySearchOptions.cs.md)
- [ToolMode](src/api/Gabriel.Core/Configuration/ToolMode.cs.md)

### src/api/Gabriel.Core/Entities

- [Conversation](src/api/Gabriel.Core/Entities/Conversation.cs.md)
- [GabrielMode](src/api/Gabriel.Core/Entities/GabrielMode.cs.md)
- [MemoryEntry](src/api/Gabriel.Core/Entities/MemoryEntry.cs.md)
- [MemoryEntryType](src/api/Gabriel.Core/Entities/MemoryEntryType.cs.md)
- [Message](src/api/Gabriel.Core/Entities/Message.cs.md)
- [MessageRole](src/api/Gabriel.Core/Entities/MessageRole.cs.md)
- [MetricEntry](src/api/Gabriel.Core/Entities/MetricEntry.cs.md)
- [Project](src/api/Gabriel.Core/Entities/Project.cs.md)
- [ProjectFile](src/api/Gabriel.Core/Entities/ProjectFile.cs.md)

### src/api/Gabriel.Core/Exceptions

- [DomainException](src/api/Gabriel.Core/Exceptions/DomainException.cs.md)
- [NotFoundException](src/api/Gabriel.Core/Exceptions/NotFoundException.cs.md)

### src/api/Gabriel.Core/Identity

- [ICurrentUser](src/api/Gabriel.Core/Identity/ICurrentUser.cs.md)
- [IJwtTokenService](src/api/Gabriel.Core/Identity/IJwtTokenService.cs.md)
- [IRefreshTokenStore](src/api/Gabriel.Core/Identity/IRefreshTokenStore.cs.md)
- [IUserPreferences](src/api/Gabriel.Core/Identity/IUserPreferences.cs.md)
- [RefreshToken](src/api/Gabriel.Core/Identity/RefreshToken.cs.md)

### src/api/Gabriel.Core/Personality

- [ConversationState](src/api/Gabriel.Core/Personality/ConversationState.cs.md)
- [Mood](src/api/Gabriel.Core/Personality/Mood.cs.md)

### src/api/Gabriel.Core/Repositories

- [IConversationRepository](src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md)
- [IMemoryRepository](src/api/Gabriel.Core/Repositories/IMemoryRepository.cs.md)
- [IMetricRepository](src/api/Gabriel.Core/Repositories/IMetricRepository.cs.md)
- [IProjectRepository](src/api/Gabriel.Core/Repositories/IProjectRepository.cs.md)
- [IUnitOfWork](src/api/Gabriel.Core/Repositories/IUnitOfWork.cs.md)

### src/api/Gabriel.Core/Services

- [ChatService](src/api/Gabriel.Core/Services/ChatService.cs.md)
- [IChatService](src/api/Gabriel.Core/Services/IChatService.cs.md)
- [IMemoryService](src/api/Gabriel.Core/Services/IMemoryService.cs.md)
- [IProjectFileService](src/api/Gabriel.Core/Services/IProjectFileService.cs.md)
- [IProjectService](src/api/Gabriel.Core/Services/IProjectService.cs.md)
- [MemoryService](src/api/Gabriel.Core/Services/MemoryService.cs.md)
- [ProjectService](src/api/Gabriel.Core/Services/ProjectService.cs.md)

### src/api/Gabriel.Engine

- [DependencyInjection](src/api/Gabriel.Engine/DependencyInjection.cs.md)

### src/api/Gabriel.Engine/Personality

- [GabrielSystemPromptBuilder](src/api/Gabriel.Engine/Personality/GabrielSystemPromptBuilder.cs.md)
- [HeuristicConversationStateUpdater](src/api/Gabriel.Engine/Personality/HeuristicConversationStateUpdater.cs.md)
- [IConversationStateUpdater](src/api/Gabriel.Engine/Personality/IConversationStateUpdater.cs.md)
- [IResponsePostProcessor](src/api/Gabriel.Engine/Personality/IResponsePostProcessor.cs.md)
- [ISystemPromptBuilder](src/api/Gabriel.Engine/Personality/ISystemPromptBuilder.cs.md)
- [ResponsePostProcessor](src/api/Gabriel.Engine/Personality/ResponsePostProcessor.cs.md)

### src/api/Gabriel.Engine/Personality/Prompts

- [Fragments.FewShot](src/api/Gabriel.Engine/Personality/Prompts/Fragments.FewShot.cs.md)
- [Fragments.Formatting](src/api/Gabriel.Engine/Personality/Prompts/Fragments.Formatting.cs.md)
- [Fragments.Memory](src/api/Gabriel.Engine/Personality/Prompts/Fragments.Memory.cs.md)
- [Fragments.Modes](src/api/Gabriel.Engine/Personality/Prompts/Fragments.Modes.cs.md)
- [Fragments.Persona](src/api/Gabriel.Engine/Personality/Prompts/Fragments.Persona.cs.md)
- [IPromptRegistry](src/api/Gabriel.Engine/Personality/Prompts/IPromptRegistry.cs.md)
- [PromptKey](src/api/Gabriel.Engine/Personality/Prompts/PromptKey.cs.md)
- [PromptRegistry](src/api/Gabriel.Engine/Personality/Prompts/PromptRegistry.cs.md)

### src/api/Gabriel.Engine/Providers

- [AvailableModel](src/api/Gabriel.Engine/Providers/AvailableModel.cs.md)
- [ChatProviderEvent](src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs.md)
- [ChatProviderMessage](src/api/Gabriel.Engine/Providers/ChatProviderMessage.cs.md)
- [ChatProviderToolCall](src/api/Gabriel.Engine/Providers/ChatProviderToolCall.cs.md)
- [IChatProvider](src/api/Gabriel.Engine/Providers/IChatProvider.cs.md)
- [IChatProviderRegistry](src/api/Gabriel.Engine/Providers/IChatProviderRegistry.cs.md)
- [IModelCatalog](src/api/Gabriel.Engine/Providers/IModelCatalog.cs.md)
- [ModelCatalog](src/api/Gabriel.Engine/Providers/ModelCatalog.cs.md)
- [ToolDescriptor](src/api/Gabriel.Engine/Providers/ToolDescriptor.cs.md)

### src/api/Gabriel.Engine/Providers/ToolBridge

- [GabrielToolBridge](src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md)
- [ToolCallBlockParser](src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallBlockParser.cs.md)
- [ToolCallStreamSplitter](src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallStreamSplitter.cs.md)

### src/api/Gabriel.Engine/Sequence

- [Frame](src/api/Gabriel.Engine/Sequence/Frame.cs.md)
- [FrameLayer](src/api/Gabriel.Engine/Sequence/FrameLayer.cs.md)
- [GabrielSequence](src/api/Gabriel.Engine/Sequence/GabrielSequence.cs.md)
- [GabrielSequenceGenerator](src/api/Gabriel.Engine/Sequence/GabrielSequenceGenerator.cs.md)
- [GabrielSequenceService](src/api/Gabriel.Engine/Sequence/GabrielSequenceService.cs.md)
- [IGabrielSequenceGenerator](src/api/Gabriel.Engine/Sequence/IGabrielSequenceGenerator.cs.md)
- [IGabrielSequenceService](src/api/Gabriel.Engine/Sequence/IGabrielSequenceService.cs.md)
- [Noise](src/api/Gabriel.Engine/Sequence/Noise.cs.md)
- [Palette](src/api/Gabriel.Engine/Sequence/Palette.cs.md)
- [PaletteTemplates](src/api/Gabriel.Engine/Sequence/PaletteTemplates.cs.md)
- [PatternKind](src/api/Gabriel.Engine/Sequence/PatternKind.cs.md)
- [Patterns](src/api/Gabriel.Engine/Sequence/Patterns.cs.md)
- [RgbColor](src/api/Gabriel.Engine/Sequence/RgbColor.cs.md)
- [SequenceCatalog](src/api/Gabriel.Engine/Sequence/SequenceCatalog.cs.md)

### src/api/Gabriel.Engine/Services

- [AgentContext](src/api/Gabriel.Engine/Services/AgentContext.cs.md)
- [AgentEvent](src/api/Gabriel.Engine/Services/AgentEvent.cs.md)
- [AgentService](src/api/Gabriel.Engine/Services/AgentService.cs.md)
- [ContextMetrics](src/api/Gabriel.Engine/Services/ContextMetrics.cs.md)
- [IAgentService](src/api/Gabriel.Engine/Services/IAgentService.cs.md)
- [IMetricRecorder](src/api/Gabriel.Engine/Services/IMetricRecorder.cs.md)
- [ITokenEstimator](src/api/Gabriel.Engine/Services/ITokenEstimator.cs.md)
- [MetricRecorder](src/api/Gabriel.Engine/Services/MetricRecorder.cs.md)
- [NaiveTokenEstimator](src/api/Gabriel.Engine/Services/NaiveTokenEstimator.cs.md)

### src/api/Gabriel.Engine/Tools

- [GetCurrentTimeTool](src/api/Gabriel.Engine/Tools/GetCurrentTimeTool.cs.md)
- [ITool](src/api/Gabriel.Engine/Tools/ITool.cs.md)
- [IToolExecutionContext](src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs.md)
- [IToolRegistry](src/api/Gabriel.Engine/Tools/IToolRegistry.cs.md)
- [ToolRegistry](src/api/Gabriel.Engine/Tools/ToolRegistry.cs.md)

### src/api/Gabriel.Engine/Tools/Calc

- [CalculateTool](src/api/Gabriel.Engine/Tools/Calc/CalculateTool.cs.md)

### src/api/Gabriel.Engine/Tools/Codecs

- [Base64Tool](src/api/Gabriel.Engine/Tools/Codecs/Base64Tool.cs.md)
- [HashTool](src/api/Gabriel.Engine/Tools/Codecs/HashTool.cs.md)

### src/api/Gabriel.Engine/Tools/Colors

- [ColorConvertTool](src/api/Gabriel.Engine/Tools/Colors/ColorConvertTool.cs.md)

### src/api/Gabriel.Engine/Tools/Data

- [JsonFormatTool](src/api/Gabriel.Engine/Tools/Data/JsonFormatTool.cs.md)

### src/api/Gabriel.Engine/Tools/Docs

- [DocsListTool](src/api/Gabriel.Engine/Tools/Docs/DocsListTool.cs.md)
- [DocsReadTool](src/api/Gabriel.Engine/Tools/Docs/DocsReadTool.cs.md)
- [IDocsLookup](src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs.md)

### src/api/Gabriel.Engine/Tools/Files

- [AgentPathResolver](src/api/Gabriel.Engine/Tools/Files/AgentPathResolver.cs.md)
- [FileInfoTool](src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md)
- [FindTool](src/api/Gabriel.Engine/Tools/Files/FindTool.cs.md)
- [GrepTool](src/api/Gabriel.Engine/Tools/Files/GrepTool.cs.md)
- [IAgentPathResolver](src/api/Gabriel.Engine/Tools/Files/IAgentPathResolver.cs.md)
- [ListDirTool](src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs.md)

### src/api/Gabriel.Engine/Tools/Memory

- [MemoryListTool](src/api/Gabriel.Engine/Tools/Memory/MemoryListTool.cs.md)
- [MemoryRemoveTool](src/api/Gabriel.Engine/Tools/Memory/MemoryRemoveTool.cs.md)
- [MemorySaveTool](src/api/Gabriel.Engine/Tools/Memory/MemorySaveTool.cs.md)

### src/api/Gabriel.Engine/Tools/Numbers

- [BaseConvertTool](src/api/Gabriel.Engine/Tools/Numbers/BaseConvertTool.cs.md)

### src/api/Gabriel.Engine/Tools/Projects

- [ListProjectFilesTool](src/api/Gabriel.Engine/Tools/Projects/ListProjectFilesTool.cs.md)
- [ReadProjectFileTool](src/api/Gabriel.Engine/Tools/Projects/ReadProjectFileTool.cs.md)

### src/api/Gabriel.Engine/Tools/Strings

- [TextStatsTool](src/api/Gabriel.Engine/Tools/Strings/TextStatsTool.cs.md)
- [TextTransformTool](src/api/Gabriel.Engine/Tools/Strings/TextTransformTool.cs.md)

### src/api/Gabriel.Engine/Tools/Web

- [IUrlFetcher](src/api/Gabriel.Engine/Tools/Web/IUrlFetcher.cs.md)
- [IWebSearch](src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md)
- [WebFetchTool](src/api/Gabriel.Engine/Tools/Web/WebFetchTool.cs.md)
- [WebSearchTool](src/api/Gabriel.Engine/Tools/Web/WebSearchTool.cs.md)

### src/api/Gabriel.Infrastructure

- [DependencyInjection](src/api/Gabriel.Infrastructure/DependencyInjection.cs.md)

### src/api/Gabriel.Infrastructure/Identity

- [ApplicationUser](src/api/Gabriel.Infrastructure/Identity/ApplicationUser.cs.md)
- [IdentityServiceCollectionExtensions](src/api/Gabriel.Infrastructure/Identity/IdentityServiceCollectionExtensions.cs.md)
- [JwtTokenService](src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs.md)
- [UserPreferencesService](src/api/Gabriel.Infrastructure/Identity/UserPreferencesService.cs.md)

### src/api/Gabriel.Infrastructure/Persistence

- [AppDbContext](src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md)
- [UnitOfWork](src/api/Gabriel.Infrastructure/Persistence/UnitOfWork.cs.md)

### src/api/Gabriel.Infrastructure/Persistence/Configurations

- [ConversationConfiguration](src/api/Gabriel.Infrastructure/Persistence/Configurations/ConversationConfiguration.cs.md)
- [MemoryEntryConfiguration](src/api/Gabriel.Infrastructure/Persistence/Configurations/MemoryEntryConfiguration.cs.md)
- [MessageConfiguration](src/api/Gabriel.Infrastructure/Persistence/Configurations/MessageConfiguration.cs.md)
- [MetricEntryConfiguration](src/api/Gabriel.Infrastructure/Persistence/Configurations/MetricEntryConfiguration.cs.md)
- [ProjectConfiguration](src/api/Gabriel.Infrastructure/Persistence/Configurations/ProjectConfiguration.cs.md)
- [ProjectFileConfiguration](src/api/Gabriel.Infrastructure/Persistence/Configurations/ProjectFileConfiguration.cs.md)
- [RefreshTokenConfiguration](src/api/Gabriel.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs.md)

### src/api/Gabriel.Infrastructure/Persistence/Repositories

- [ConversationRepository](src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs.md)
- [MemoryRepository](src/api/Gabriel.Infrastructure/Persistence/Repositories/MemoryRepository.cs.md)
- [MetricRepository](src/api/Gabriel.Infrastructure/Persistence/Repositories/MetricRepository.cs.md)
- [ProjectRepository](src/api/Gabriel.Infrastructure/Persistence/Repositories/ProjectRepository.cs.md)
- [RefreshTokenStore](src/api/Gabriel.Infrastructure/Persistence/Repositories/RefreshTokenStore.cs.md)

### src/api/Gabriel.Infrastructure/Projects

- [DiskProjectFileService](src/api/Gabriel.Infrastructure/Projects/DiskProjectFileService.cs.md)

### src/api/Gabriel.Infrastructure/Providers

- [GrokAuthHandler](src/api/Gabriel.Infrastructure/Providers/GrokAuthHandler.cs.md)
- [GrokChatProvider](src/api/Gabriel.Infrastructure/Providers/GrokChatProvider.cs.md)
- [MockChatProvider](src/api/Gabriel.Infrastructure/Providers/MockChatProvider.cs.md)

### src/api/Gabriel.Infrastructure/Tools/Docs

- [CompositeDocsLookup](src/api/Gabriel.Infrastructure/Tools/Docs/CompositeDocsLookup.cs.md)
- [GitHubDocsLookup](src/api/Gabriel.Infrastructure/Tools/Docs/GitHubDocsLookup.cs.md)
- [LocalDocsLookup](src/api/Gabriel.Infrastructure/Tools/Docs/LocalDocsLookup.cs.md)

### src/api/Gabriel.Infrastructure/Tools/Web

- [BraveWebSearch](src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs.md)
- [CompositeWebSearch](src/api/Gabriel.Infrastructure/Tools/Web/CompositeWebSearch.cs.md)
- [DuckDuckGoWebSearch](src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs.md)
- [HttpUrlFetcher](src/api/Gabriel.Infrastructure/Tools/Web/HttpUrlFetcher.cs.md)
- [InstrumentedWebSearch](src/api/Gabriel.Infrastructure/Tools/Web/InstrumentedWebSearch.cs.md)
- [TavilyWebSearch](src/api/Gabriel.Infrastructure/Tools/Web/TavilyWebSearch.cs.md)

### src/webapp

- [vite.config](src/webapp/vite.config.ts.md)

### src/webapp/src

- [App](src/webapp/src/App.tsx.md)
- [main](src/webapp/src/main.tsx.md)
- [router](src/webapp/src/router.tsx.md)
- [vite-env.d](src/webapp/src/vite-env.d.ts.md)

### src/webapp/src/api

- [authInterceptor](src/webapp/src/api/authInterceptor.ts.md)
- [authRefresh](src/webapp/src/api/authRefresh.ts.md)
- [conversationMode](src/webapp/src/api/conversationMode.ts.md)
- [memories](src/webapp/src/api/memories.ts.md)
- [models](src/webapp/src/api/models.ts.md)
- [sequence](src/webapp/src/api/sequence.ts.md)
- [streamChat](src/webapp/src/api/streamChat.ts.md)

### src/webapp/src/auth

- [AuthContext](src/webapp/src/auth/AuthContext.tsx.md)

### src/webapp/src/components

- [Avatar](src/webapp/src/components/Avatar.tsx.md)
- [Chat](src/webapp/src/components/Chat.tsx.md)
- [CompactingOverlay](src/webapp/src/components/CompactingOverlay.tsx.md)
- [ContextStats](src/webapp/src/components/ContextStats.tsx.md)
- [GabrielSequenceView](src/webapp/src/components/GabrielSequenceView.tsx.md)
- [Markdown](src/webapp/src/components/Markdown.tsx.md)
- [MemoryList](src/webapp/src/components/MemoryList.tsx.md)
- [MemoryQuickSave](src/webapp/src/components/MemoryQuickSave.tsx.md)
- [Mermaid](src/webapp/src/components/Mermaid.tsx.md)
- [ModeSelector](src/webapp/src/components/ModeSelector.tsx.md)
- [ModelSelector](src/webapp/src/components/ModelSelector.tsx.md)
- [ProjectPicker](src/webapp/src/components/ProjectPicker.tsx.md)
- [Sidebar](src/webapp/src/components/Sidebar.tsx.md)
- [SkinPicker](src/webapp/src/components/SkinPicker.tsx.md)
- [StreamingText](src/webapp/src/components/StreamingText.tsx.md)
- [ThinkingPulse](src/webapp/src/components/ThinkingPulse.tsx.md)

### src/webapp/src/layouts

- [AuthLayout](src/webapp/src/layouts/AuthLayout.tsx.md)
- [MainLayout](src/webapp/src/layouts/MainLayout.tsx.md)

### src/webapp/src/lib

- [notify](src/webapp/src/lib/notify.ts.md)
- [userPrefs](src/webapp/src/lib/userPrefs.ts.md)

### src/webapp/src/pages

- [ChatPage](src/webapp/src/pages/ChatPage.tsx.md)
- [DiagnosticsPage](src/webapp/src/pages/DiagnosticsPage.tsx.md)
- [IndexPage](src/webapp/src/pages/IndexPage.tsx.md)
- [LoginPage](src/webapp/src/pages/LoginPage.tsx.md)
- [ProjectSettingsPage](src/webapp/src/pages/ProjectSettingsPage.tsx.md)
- [RegisterPage](src/webapp/src/pages/RegisterPage.tsx.md)
- [UserSettingsPage](src/webapp/src/pages/UserSettingsPage.tsx.md)

### src/webapp/src/pulse

- [palettes](src/webapp/src/pulse/palettes.ts.md)
- [patterns](src/webapp/src/pulse/patterns.ts.md)
- [rng](src/webapp/src/pulse/rng.ts.md)

### src/webapp/src/routes

- [ProtectedRoute](src/webapp/src/routes/ProtectedRoute.tsx.md)
- [PublicRoute](src/webapp/src/routes/PublicRoute.tsx.md)