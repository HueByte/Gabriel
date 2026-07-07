# Repo Map — HueByte/Gabriel

> Deterministic structural map generated for AI agents. Read this file for
> orientation, query [`symbol-graph.json`](symbol-graph.json) for exact
> dependencies, and open the linked per-file docs for behaviour — instead of
> scanning the source tree.

Commit `8238aa07ea35c8d5fd13b396c665ac09fe180f90` · 470 symbols · 250 files · 960 dependency edges

## Subsystems

*Structural clusters detected from the dependency graph — groups of symbols more densely wired to each other than to the rest of the codebase.*

### src/webapp/src/components

52 symbols across 16 files. Key symbols (by connectivity):

- [`MermaidModule`](../Code/src/webapp/src/components/Mermaid.tsx.md) (type) — `src/webapp/src/components/Mermaid.tsx`
- [`loadMermaid`](../Code/src/webapp/src/components/Mermaid.tsx.md) (function) — `src/webapp/src/components/Mermaid.tsx`
- [`PulsePlane`](../Code/src/webapp/src/components/Avatar.tsx.md) (function) — `src/webapp/src/components/Avatar.tsx`
- [`createPulse`](../Code/src/webapp/src/components/Avatar.tsx.md) (function) — `src/webapp/src/components/Avatar.tsx`
- [`advance`](../Code/src/webapp/src/components/Chat.tsx.md) (function) — `src/webapp/src/components/Chat.tsx`
- [`lastIndexWhere`](../Code/src/webapp/src/components/Chat.tsx.md) (function) — `src/webapp/src/components/Chat.tsx`
- [`onKeyDown`](../Code/src/webapp/src/components/Chat.tsx.md) (function) — `src/webapp/src/components/Chat.tsx`
- [`onSubmit`](../Code/src/webapp/src/components/Chat.tsx.md) (function) — `src/webapp/src/components/Chat.tsx`
- *…and 44 more (see symbol-graph.json)*

### src/api/Gabriel.Engine · AgentService

30 symbols across 21 files. Key symbols (by connectivity):

- [`AgentService`](../Code/src/api/Gabriel.Engine/Services/AgentService.cs.md) (class) — `src/api/Gabriel.Engine/Services/AgentService.cs`
- [`GabrielToolBridge`](../Code/src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) (class) — `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`
- [`IChatProvider`](../Code/src/api/Gabriel.Engine/Providers/IChatProvider.cs.md) (interface) — `src/api/Gabriel.Engine/Providers/IChatProvider.cs`
- [`AgentContext`](../Code/src/api/Gabriel.Engine/Services/AgentContext.cs.md) (record) — `src/api/Gabriel.Engine/Services/AgentContext.cs`
- [`GrokChatProvider`](../Code/src/api/Gabriel.Infrastructure/Providers/GrokChatProvider.cs.md) (class) — `src/api/Gabriel.Infrastructure/Providers/GrokChatProvider.cs`
- [`ApplicationUser`](../Code/src/api/Gabriel.Infrastructure/Identity/ApplicationUser.cs.md) (class) — `src/api/Gabriel.Infrastructure/Identity/ApplicationUser.cs`
- [`MockChatProvider`](../Code/src/api/Gabriel.Infrastructure/Providers/MockChatProvider.cs.md) (class) — `src/api/Gabriel.Infrastructure/Providers/MockChatProvider.cs`
- [`LLMModel`](../Code/src/api/Gabriel.Core/Configuration/LLMModel.cs.md) (class) — `src/api/Gabriel.Core/Configuration/LLMModel.cs`
- *…and 22 more (see symbol-graph.json)*

### src/api/Gabriel.Engine · DependencyInjection

28 symbols across 27 files. Key symbols (by connectivity):

- [`DependencyInjection`](../Code/src/api/Gabriel.Engine/DependencyInjection.cs.md) (class) — `src/api/Gabriel.Engine/DependencyInjection.cs`
- [`Message`](../Code/src/api/Gabriel.Core/Entities/Message.cs.md) (class) — `src/api/Gabriel.Core/Entities/Message.cs`
- [`ITool`](../Code/src/api/Gabriel.Engine/Tools/ITool.cs.md) (interface) — `src/api/Gabriel.Engine/Tools/ITool.cs`
- [`IToolExecutionContext`](../Code/src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs.md) (interface) — `src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs`
- [`ProjectFile`](../Code/src/api/Gabriel.Core/Entities/ProjectFile.cs.md) (class) — `src/api/Gabriel.Core/Entities/ProjectFile.cs`
- [`MemorySaveTool`](../Code/src/api/Gabriel.Engine/Tools/Memory/MemorySaveTool.cs.md) (class) — `src/api/Gabriel.Engine/Tools/Memory/MemorySaveTool.cs`
- [`ListProjectFilesTool`](../Code/src/api/Gabriel.Engine/Tools/Projects/ListProjectFilesTool.cs.md) (class) — `src/api/Gabriel.Engine/Tools/Projects/ListProjectFilesTool.cs`
- [`IProjectFileService`](../Code/src/api/Gabriel.Core/Services/IProjectFileService.cs.md) (interface) — `src/api/Gabriel.Core/Services/IProjectFileService.cs`
- *…and 20 more (see symbol-graph.json)*

### src/webapp/src/api

19 symbols across 7 files. Key symbols (by connectivity):

- [`GabrielSequence`](../Code/src/webapp/src/api/sequence.ts.md) (interface) — `src/webapp/src/api/sequence.ts`
- [`GabrielSequenceMetadata`](../Code/src/webapp/src/api/sequence.ts.md) (interface) — `src/webapp/src/api/sequence.ts`
- [`installAuthInterceptor`](../Code/src/webapp/src/api/authInterceptor.ts.md) (function) — `src/webapp/src/api/authInterceptor.ts`
- [`refreshSession`](../Code/src/webapp/src/api/authRefresh.ts.md) (function) — `src/webapp/src/api/authRefresh.ts`
- [`signalSessionExpired`](../Code/src/webapp/src/api/authRefresh.ts.md) (function) — `src/webapp/src/api/authRefresh.ts`
- [`GabrielMode`](../Code/src/webapp/src/api/conversationMode.ts.md) (type) — `src/webapp/src/api/conversationMode.ts`
- [`setConversationMode`](../Code/src/webapp/src/api/conversationMode.ts.md) (function) — `src/webapp/src/api/conversationMode.ts`
- [`withRefresh`](../Code/src/webapp/src/api/conversationMode.ts.md) (function) — `src/webapp/src/api/conversationMode.ts`
- *…and 11 more (see symbol-graph.json)*

### src/api/Gabriel.Engine · Project

17 symbols across 17 files. Key symbols (by connectivity):

- [`Project`](../Code/src/api/Gabriel.Core/Entities/Project.cs.md) (class) — `src/api/Gabriel.Core/Entities/Project.cs`
- [`Conversation`](../Code/src/api/Gabriel.Core/Entities/Conversation.cs.md) (class) — `src/api/Gabriel.Core/Entities/Conversation.cs`
- [`GabrielSequenceGenerator`](../Code/src/api/Gabriel.Engine/Sequence/GabrielSequenceGenerator.cs.md) (class) — `src/api/Gabriel.Engine/Sequence/GabrielSequenceGenerator.cs`
- [`Core`](../Code/src/webapp/src/components/CompactingOverlay.tsx.md) (function) — `src/webapp/src/components/CompactingOverlay.tsx`
- [`SequenceCatalog`](../Code/src/api/Gabriel.Engine/Sequence/SequenceCatalog.cs.md) (class) — `src/api/Gabriel.Engine/Sequence/SequenceCatalog.cs`
- [`GabrielSequenceService`](../Code/src/api/Gabriel.Engine/Sequence/GabrielSequenceService.cs.md) (class) — `src/api/Gabriel.Engine/Sequence/GabrielSequenceService.cs`
- [`IGabrielSequenceGenerator`](../Code/src/api/Gabriel.Engine/Sequence/IGabrielSequenceGenerator.cs.md) (interface) — `src/api/Gabriel.Engine/Sequence/IGabrielSequenceGenerator.cs`
- [`IConversationRepository`](../Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md) (interface) — `src/api/Gabriel.Core/Repositories/IConversationRepository.cs`
- *…and 9 more (see symbol-graph.json)*

### src/webapp/src/components · ContractMappings

15 symbols across 9 files. Key symbols (by connectivity):

- [`ContractMappings`](../Code/src/api/Gabriel.API/Mapping/ContractMappings.cs.md) (class) — `src/api/Gabriel.API/Mapping/ContractMappings.cs`
- [`ConversationResponse`](../Code/src/api/Gabriel.API/Contracts/Conversations/ConversationResponse.cs.md) (record) — `src/api/Gabriel.API/Contracts/Conversations/ConversationResponse.cs`
- [`ChatEntry`](../Code/src/webapp/src/components/Chat.tsx.md) (type) — `src/webapp/src/components/Chat.tsx`
- [`GabrielSequenceResponse`](../Code/src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs.md) (record) — `src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs`
- [`RgbColor`](../Code/src/api/Gabriel.Engine/Sequence/RgbColor.cs.md) (record) — `src/api/Gabriel.Engine/Sequence/RgbColor.cs`
- [`historyToEntries`](../Code/src/webapp/src/components/Chat.tsx.md) (function) — `src/webapp/src/components/Chat.tsx`
- [`MessageToolCall`](../Code/src/api/Gabriel.API/Contracts/Messages/MessageResponse.cs.md) (record) — `src/api/Gabriel.API/Contracts/Messages/MessageResponse.cs`
- [`applyAgentEvent`](../Code/src/webapp/src/components/Chat.tsx.md) (function) — `src/webapp/src/components/Chat.tsx`
- *…and 7 more (see symbol-graph.json)*

### src/api/Gabriel.Infrastructure · DependencyInjection

12 symbols across 11 files. Key symbols (by connectivity):

- [`DependencyInjection`](../Code/src/api/Gabriel.Infrastructure/DependencyInjection.cs.md) (class) — `src/api/Gabriel.Infrastructure/DependencyInjection.cs`
- [`IWebSearch`](../Code/src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md) (interface) — `src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs`
- [`WebSearchResult`](../Code/src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md) (record) — `src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs`
- [`WebSearchTool`](../Code/src/api/Gabriel.Engine/Tools/Web/WebSearchTool.cs.md) (class) — `src/api/Gabriel.Engine/Tools/Web/WebSearchTool.cs`
- [`InstrumentedWebSearch`](../Code/src/api/Gabriel.Infrastructure/Tools/Web/InstrumentedWebSearch.cs.md) (class) — `src/api/Gabriel.Infrastructure/Tools/Web/InstrumentedWebSearch.cs`
- [`BraveWebSearch`](../Code/src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs.md) (class) — `src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs`
- [`TavilyWebSearch`](../Code/src/api/Gabriel.Infrastructure/Tools/Web/TavilyWebSearch.cs.md) (class) — `src/api/Gabriel.Infrastructure/Tools/Web/TavilyWebSearch.cs`
- [`BraveSearchOptions`](../Code/src/api/Gabriel.Core/Configuration/BraveSearchOptions.cs.md) (class) — `src/api/Gabriel.Core/Configuration/BraveSearchOptions.cs`
- *…and 4 more (see symbol-graph.json)*

### src/webapp/src/pulse

12 symbols across 3 files. Key symbols (by connectivity):

- [`paletteForSeed`](../Code/src/webapp/src/pulse/palettes.ts.md) (function) — `src/webapp/src/pulse/palettes.ts`
- [`paletteGradientCss`](../Code/src/webapp/src/pulse/palettes.ts.md) (function) — `src/webapp/src/pulse/palettes.ts`
- [`pickPalette`](../Code/src/webapp/src/pulse/palettes.ts.md) (function) — `src/webapp/src/pulse/palettes.ts`
- [`FlowParams`](../Code/src/webapp/src/pulse/patterns.ts.md) (interface) — `src/webapp/src/pulse/patterns.ts`
- [`NoiseParams`](../Code/src/webapp/src/pulse/patterns.ts.md) (interface) — `src/webapp/src/pulse/patterns.ts`
- [`fbm`](../Code/src/webapp/src/pulse/patterns.ts.md) (function) — `src/webapp/src/pulse/patterns.ts`
- [`hash2`](../Code/src/webapp/src/pulse/patterns.ts.md) (function) — `src/webapp/src/pulse/patterns.ts`
- [`smooth`](../Code/src/webapp/src/pulse/patterns.ts.md) (function) — `src/webapp/src/pulse/patterns.ts`
- *…and 4 more (see symbol-graph.json)*

### src/webapp/src/lib

11 symbols across 2 files. Key symbols (by connectivity):

- [`formatError`](../Code/src/webapp/src/lib/notify.ts.md) (function) — `src/webapp/src/lib/notify.ts`
- [`notifyError`](../Code/src/webapp/src/lib/notify.ts.md) (function) — `src/webapp/src/lib/notify.ts`
- [`notifyInfo`](../Code/src/webapp/src/lib/notify.ts.md) (function) — `src/webapp/src/lib/notify.ts`
- [`notifySuccess`](../Code/src/webapp/src/lib/notify.ts.md) (function) — `src/webapp/src/lib/notify.ts`
- [`readBool`](../Code/src/webapp/src/lib/userPrefs.ts.md) (function) — `src/webapp/src/lib/userPrefs.ts`
- [`readLegacyHideReactDetails`](../Code/src/webapp/src/lib/userPrefs.ts.md) (function) — `src/webapp/src/lib/userPrefs.ts`
- [`useBoolPref`](../Code/src/webapp/src/lib/userPrefs.ts.md) (function) — `src/webapp/src/lib/userPrefs.ts`
- [`useHideThinking`](../Code/src/webapp/src/lib/userPrefs.ts.md) (function) — `src/webapp/src/lib/userPrefs.ts`
- *…and 3 more (see symbol-graph.json)*

### src/api/Gabriel.Core · MemoryEntry

10 symbols across 9 files. Key symbols (by connectivity):

- [`MemoryEntry`](../Code/src/api/Gabriel.Core/Entities/MemoryEntry.cs.md) (class) — `src/api/Gabriel.Core/Entities/MemoryEntry.cs`
- [`IMemoryService`](../Code/src/api/Gabriel.Core/Services/IMemoryService.cs.md) (interface) — `src/api/Gabriel.Core/Services/IMemoryService.cs`
- [`MemoryService`](../Code/src/api/Gabriel.Core/Services/MemoryService.cs.md) (class) — `src/api/Gabriel.Core/Services/MemoryService.cs`
- [`MemoryEntryType`](../Code/src/api/Gabriel.Core/Entities/MemoryEntryType.cs.md) (enum) — `src/api/Gabriel.Core/Entities/MemoryEntryType.cs`
- [`IMemoryRepository`](../Code/src/api/Gabriel.Core/Repositories/IMemoryRepository.cs.md) (interface) — `src/api/Gabriel.Core/Repositories/IMemoryRepository.cs`
- [`MemoryEntrySpec`](../Code/src/api/Gabriel.Core/Services/IMemoryService.cs.md) (record) — `src/api/Gabriel.Core/Services/IMemoryService.cs`
- [`MemoriesController`](../Code/src/api/Gabriel.API/Controllers/MemoriesController.cs.md) (class) — `src/api/Gabriel.API/Controllers/MemoriesController.cs`
- [`MemoryRepository`](../Code/src/api/Gabriel.Infrastructure/Persistence/Repositories/MemoryRepository.cs.md) (class) — `src/api/Gabriel.Infrastructure/Persistence/Repositories/MemoryRepository.cs`
- *…and 2 more (see symbol-graph.json)*

### src/api/Gabriel.Engine · DomainException

10 symbols across 8 files. Key symbols (by connectivity):

- [`DomainException`](../Code/src/api/Gabriel.Core/Exceptions/DomainException.cs.md) (class) — `src/api/Gabriel.Core/Exceptions/DomainException.cs`
- [`AgentPathResolver`](../Code/src/api/Gabriel.Engine/Tools/Files/AgentPathResolver.cs.md) (class) — `src/api/Gabriel.Engine/Tools/Files/AgentPathResolver.cs`
- [`FileInfoTool`](../Code/src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md) (class) — `src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs`
- [`FindTool`](../Code/src/api/Gabriel.Engine/Tools/Files/FindTool.cs.md) (class) — `src/api/Gabriel.Engine/Tools/Files/FindTool.cs`
- [`GrepTool`](../Code/src/api/Gabriel.Engine/Tools/Files/GrepTool.cs.md) (class) — `src/api/Gabriel.Engine/Tools/Files/GrepTool.cs`
- [`ListDirTool`](../Code/src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs.md) (class) — `src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs`
- [`IAgentPathResolver`](../Code/src/api/Gabriel.Engine/Tools/Files/IAgentPathResolver.cs.md) (interface) — `src/api/Gabriel.Engine/Tools/Files/IAgentPathResolver.cs`
- [`PathRootMode`](../Code/src/api/Gabriel.Engine/Tools/Files/IAgentPathResolver.cs.md) (enum) — `src/api/Gabriel.Engine/Tools/Files/IAgentPathResolver.cs`
- *…and 2 more (see symbol-graph.json)*

### src/webapp/src/pulse · Patterns

10 symbols across 5 files. Key symbols (by connectivity):

- [`Patterns`](../Code/src/api/Gabriel.Engine/Sequence/Patterns.cs.md) (class) — `src/api/Gabriel.Engine/Sequence/Patterns.cs`
- [`SequenceController`](../Code/src/api/Gabriel.API/Controllers/SequenceController.cs.md) (class) — `src/api/Gabriel.API/Controllers/SequenceController.cs`
- [`ShimmerParams`](../Code/src/webapp/src/pulse/patterns.ts.md) (interface) — `src/webapp/src/pulse/patterns.ts`
- [`SequenceCatalogResponse`](../Code/src/api/Gabriel.API/Contracts/Sequence/SequenceCatalogResponse.cs.md) (record) — `src/api/Gabriel.API/Contracts/Sequence/SequenceCatalogResponse.cs`
- [`PlasmaParams`](../Code/src/webapp/src/pulse/patterns.ts.md) (interface) — `src/webapp/src/pulse/patterns.ts`
- [`PulseParams`](../Code/src/webapp/src/pulse/patterns.ts.md) (interface) — `src/webapp/src/pulse/patterns.ts`
- [`SpiralParams`](../Code/src/webapp/src/pulse/patterns.ts.md) (interface) — `src/webapp/src/pulse/patterns.ts`
- [`WavesParams`](../Code/src/webapp/src/pulse/patterns.ts.md) (interface) — `src/webapp/src/pulse/patterns.ts`
- *…and 2 more (see symbol-graph.json)*

*…and 56 smaller subsystems (see symbol-graph.json).*

## Most connected symbols

The load-bearing symbols — changes here have the widest blast radius.

| Symbol | Kind | Used by | Uses | File |
|---|---|---|---|---|
| [`AgentService`](../Code/src/api/Gabriel.Engine/Services/AgentService.cs.md) | class | 14 | 48 | `src/api/Gabriel.Engine/Services/AgentService.cs` |
| [`DependencyInjection`](../Code/src/api/Gabriel.Engine/DependencyInjection.cs.md) | class | 0 | 53 | `src/api/Gabriel.Engine/DependencyInjection.cs` |
| [`Message`](../Code/src/api/Gabriel.Core/Entities/Message.cs.md) | class | 46 | 2 | `src/api/Gabriel.Core/Entities/Message.cs` |
| [`DependencyInjection`](../Code/src/api/Gabriel.Infrastructure/DependencyInjection.cs.md) | class | 0 | 44 | `src/api/Gabriel.Infrastructure/DependencyInjection.cs` |
| [`Project`](../Code/src/api/Gabriel.Core/Entities/Project.cs.md) | class | 36 | 4 | `src/api/Gabriel.Core/Entities/Project.cs` |
| [`Conversation`](../Code/src/api/Gabriel.Core/Entities/Conversation.cs.md) | class | 23 | 7 | `src/api/Gabriel.Core/Entities/Conversation.cs` |
| [`ITool`](../Code/src/api/Gabriel.Engine/Tools/ITool.cs.md) | interface | 27 | 1 | `src/api/Gabriel.Engine/Tools/ITool.cs` |
| [`GabrielToolBridge`](../Code/src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | class | 6 | 21 | `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs` |
| [`ConversationsController`](../Code/src/api/Gabriel.API/Controllers/ConversationsController.cs.md) | class | 2 | 24 | `src/api/Gabriel.API/Controllers/ConversationsController.cs` |
| [`ConversationState`](../Code/src/api/Gabriel.Core/Personality/ConversationState.cs.md) | record | 16 | 5 | `src/api/Gabriel.Core/Personality/ConversationState.cs` |

*Regenerated on every full documentation run; see [Agent/README.md](README.md) for how to use this pack.*