# Repo Map — HueByte/Gabriel

> Deterministic structural map generated for AI agents. Read this file for
> orientation, query [`symbol-graph.json`](symbol-graph.json) for exact
> dependencies, and open the linked per-file docs for behaviour — instead of
> scanning the source tree.

Commit `` · 470 symbols · 250 files · 1159 dependency edges

## Subsystems

*Structural clusters detected from the dependency graph — groups of symbols more densely wired to each other than to the rest of the codebase.*

### src/api/Gabriel.Engine · GabrielToolBridge

30 symbols across 20 files. Key symbols (by connectivity):

- [`GabrielToolBridge`](../Code/src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) (class) — `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`
- [`IChatProvider`](../Code/src/api/Gabriel.Engine/Providers/IChatProvider.cs.md) (interface) — `src/api/Gabriel.Engine/Providers/IChatProvider.cs`
- [`GrokChatProvider`](../Code/src/api/Gabriel.Infrastructure/Providers/GrokChatProvider.cs.md) (class) — `src/api/Gabriel.Infrastructure/Providers/GrokChatProvider.cs`
- [`AgentContext`](../Code/src/api/Gabriel.Engine/Services/AgentContext.cs.md) (record) — `src/api/Gabriel.Engine/Services/AgentContext.cs`
- [`ApplicationUser`](../Code/src/api/Gabriel.Infrastructure/Identity/ApplicationUser.cs.md) (class) — `src/api/Gabriel.Infrastructure/Identity/ApplicationUser.cs`
- [`MockChatProvider`](../Code/src/api/Gabriel.Infrastructure/Providers/MockChatProvider.cs.md) (class) — `src/api/Gabriel.Infrastructure/Providers/MockChatProvider.cs`
- [`LLMModel`](../Code/src/api/Gabriel.Core/Configuration/LLMModel.cs.md) (class) — `src/api/Gabriel.Core/Configuration/LLMModel.cs`
- [`MessageRole`](../Code/src/api/Gabriel.Core/Entities/MessageRole.cs.md) (enum) — `src/api/Gabriel.Core/Entities/MessageRole.cs`
- *…and 22 more (see symbol-graph.json)*

### src/api/Gabriel.Engine · AgentService

26 symbols across 14 files. Key symbols (by connectivity):

- [`AgentService`](../Code/src/api/Gabriel.Engine/Services/AgentService.cs.md) (class) — `src/api/Gabriel.Engine/Services/AgentService.cs`
- [`ConversationsController`](../Code/src/api/Gabriel.API/Controllers/ConversationsController.cs.md) (class) — `src/api/Gabriel.API/Controllers/ConversationsController.cs`
- [`AgentEvent`](../Code/src/api/Gabriel.Engine/Services/AgentEvent.cs.md) (record) — `src/api/Gabriel.Engine/Services/AgentEvent.cs`
- [`AgentEvent`](../Code/src/webapp/src/api/streamChat.ts.md) (type) — `src/webapp/src/api/streamChat.ts`
- [`IAgentService`](../Code/src/api/Gabriel.Engine/Services/IAgentService.cs.md) (interface) — `src/api/Gabriel.Engine/Services/IAgentService.cs`
- [`ContextMetrics`](../Code/src/api/Gabriel.Engine/Services/ContextMetrics.cs.md) (record) — `src/api/Gabriel.Engine/Services/ContextMetrics.cs`
- [`AgentError`](../Code/src/api/Gabriel.Engine/Services/AgentEvent.cs.md) (record) — `src/api/Gabriel.Engine/Services/AgentEvent.cs`
- [`ContextMetricsResponse`](../Code/src/api/Gabriel.API/Contracts/Conversations/ContextMetricsResponse.cs.md) (record) — `src/api/Gabriel.API/Contracts/Conversations/ContextMetricsResponse.cs`
- *…and 18 more (see symbol-graph.json)*

### src/api/Gabriel.Engine · DependencyInjection

26 symbols across 25 files. Key symbols (by connectivity):

- [`DependencyInjection`](../Code/src/api/Gabriel.Engine/DependencyInjection.cs.md) (class) — `src/api/Gabriel.Engine/DependencyInjection.cs`
- [`Message`](../Code/src/api/Gabriel.Core/Entities/Message.cs.md) (class) — `src/api/Gabriel.Core/Entities/Message.cs`
- [`ITool`](../Code/src/api/Gabriel.Engine/Tools/ITool.cs.md) (interface) — `src/api/Gabriel.Engine/Tools/ITool.cs`
- [`IToolExecutionContext`](../Code/src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs.md) (interface) — `src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs`
- [`MemorySaveTool`](../Code/src/api/Gabriel.Engine/Tools/Memory/MemorySaveTool.cs.md) (class) — `src/api/Gabriel.Engine/Tools/Memory/MemorySaveTool.cs`
- [`ListProjectFilesTool`](../Code/src/api/Gabriel.Engine/Tools/Projects/ListProjectFilesTool.cs.md) (class) — `src/api/Gabriel.Engine/Tools/Projects/ListProjectFilesTool.cs`
- [`IProjectFileService`](../Code/src/api/Gabriel.Core/Services/IProjectFileService.cs.md) (interface) — `src/api/Gabriel.Core/Services/IProjectFileService.cs`
- [`MemoryListTool`](../Code/src/api/Gabriel.Engine/Tools/Memory/MemoryListTool.cs.md) (class) — `src/api/Gabriel.Engine/Tools/Memory/MemoryListTool.cs`
- *…and 18 more (see symbol-graph.json)*

### src/webapp/src/pulse · ContractMappings

26 symbols across 14 files. Key symbols (by connectivity):

- [`ContractMappings`](../Code/src/api/Gabriel.API/Mapping/ContractMappings.cs.md) (class) — `src/api/Gabriel.API/Mapping/ContractMappings.cs`
- [`Palette`](../Code/src/api/Gabriel.Engine/Sequence/Palette.cs.md) (record) — `src/api/Gabriel.Engine/Sequence/Palette.cs`
- [`Palette`](../Code/src/webapp/src/pulse/palettes.ts.md) (interface) — `src/webapp/src/pulse/palettes.ts`
- [`ProjectsController`](../Code/src/api/Gabriel.API/Controllers/ProjectsController.cs.md) (class) — `src/api/Gabriel.API/Controllers/ProjectsController.cs`
- [`Pattern`](../Code/src/webapp/src/pulse/patterns.ts.md) (interface) — `src/webapp/src/pulse/patterns.ts`
- [`GabrielSequenceResponse`](../Code/src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs.md) (record) — `src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs`
- [`buildState`](../Code/src/webapp/src/components/ThinkingPulse.tsx.md) (function) — `src/webapp/src/components/ThinkingPulse.tsx`
- [`rand`](../Code/prototype/generate.js.md) (function) — `prototype/generate.js`
- *…and 18 more (see symbol-graph.json)*

### src/api/Gabriel.Core · Project

25 symbols across 25 files. Key symbols (by connectivity):

- [`Project`](../Code/src/api/Gabriel.Core/Entities/Project.cs.md) (class) — `src/api/Gabriel.Core/Entities/Project.cs`
- [`Conversation`](../Code/src/api/Gabriel.Core/Entities/Conversation.cs.md) (class) — `src/api/Gabriel.Core/Entities/Conversation.cs`
- [`AppDbContext`](../Code/src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md) (class) — `src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs`
- [`Core`](../Code/src/webapp/src/components/CompactingOverlay.tsx.md) (function) — `src/webapp/src/components/CompactingOverlay.tsx`
- [`ICurrentUser`](../Code/src/api/Gabriel.Core/Identity/ICurrentUser.cs.md) (interface) — `src/api/Gabriel.Core/Identity/ICurrentUser.cs`
- [`ChatService`](../Code/src/api/Gabriel.Core/Services/ChatService.cs.md) (class) — `src/api/Gabriel.Core/Services/ChatService.cs`
- [`GabrielSequenceService`](../Code/src/api/Gabriel.Engine/Sequence/GabrielSequenceService.cs.md) (class) — `src/api/Gabriel.Engine/Sequence/GabrielSequenceService.cs`
- [`DiskProjectFileService`](../Code/src/api/Gabriel.Infrastructure/Projects/DiskProjectFileService.cs.md) (class) — `src/api/Gabriel.Infrastructure/Projects/DiskProjectFileService.cs`
- *…and 17 more (see symbol-graph.json)*

### src/api/Gabriel.Engine · GabrielMode

21 symbols across 18 files. Key symbols (by connectivity):

- [`GabrielMode`](../Code/src/api/Gabriel.Core/Entities/GabrielMode.cs.md) (enum) — `src/api/Gabriel.Core/Entities/GabrielMode.cs`
- [`GabrielSystemPromptBuilder`](../Code/src/api/Gabriel.Engine/Personality/GabrielSystemPromptBuilder.cs.md) (class) — `src/api/Gabriel.Engine/Personality/GabrielSystemPromptBuilder.cs`
- [`ISystemPromptBuilder`](../Code/src/api/Gabriel.Engine/Personality/ISystemPromptBuilder.cs.md) (interface) — `src/api/Gabriel.Engine/Personality/ISystemPromptBuilder.cs`
- [`Fragments`](../Code/src/api/Gabriel.Engine/Personality/Prompts/Fragments.Persona.cs.md) (class) — `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Persona.cs`
- [`Fragments`](../Code/src/api/Gabriel.Engine/Personality/Prompts/Fragments.Formatting.cs.md) (class) — `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Formatting.cs`
- [`PromptKey`](../Code/src/api/Gabriel.Engine/Personality/Prompts/PromptKey.cs.md) (class) — `src/api/Gabriel.Engine/Personality/Prompts/PromptKey.cs`
- [`PromptRegistry`](../Code/src/api/Gabriel.Engine/Personality/Prompts/PromptRegistry.cs.md) (class) — `src/api/Gabriel.Engine/Personality/Prompts/PromptRegistry.cs`
- [`Fragments`](../Code/src/api/Gabriel.Engine/Personality/Prompts/Fragments.Modes.cs.md) (class) — `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Modes.cs`
- *…and 13 more (see symbol-graph.json)*

### src/webapp/src/pulse · GabrielSequenceGenerator

18 symbols across 11 files. Key symbols (by connectivity):

- [`GabrielSequenceGenerator`](../Code/src/api/Gabriel.Engine/Sequence/GabrielSequenceGenerator.cs.md) (class) — `src/api/Gabriel.Engine/Sequence/GabrielSequenceGenerator.cs`
- [`Patterns`](../Code/src/api/Gabriel.Engine/Sequence/Patterns.cs.md) (class) — `src/api/Gabriel.Engine/Sequence/Patterns.cs`
- [`SequenceCatalog`](../Code/src/api/Gabriel.Engine/Sequence/SequenceCatalog.cs.md) (class) — `src/api/Gabriel.Engine/Sequence/SequenceCatalog.cs`
- [`PaletteTemplates`](../Code/src/api/Gabriel.Engine/Sequence/PaletteTemplates.cs.md) (class) — `src/api/Gabriel.Engine/Sequence/PaletteTemplates.cs`
- [`ShimmerParams`](../Code/src/webapp/src/pulse/patterns.ts.md) (interface) — `src/webapp/src/pulse/patterns.ts`
- [`pick`](../Code/prototype/patterns.js.md) (function) — `prototype/patterns.js`
- [`SequenceController`](../Code/src/api/Gabriel.API/Controllers/SequenceController.cs.md) (class) — `src/api/Gabriel.API/Controllers/SequenceController.cs`
- [`PatternKind`](../Code/src/api/Gabriel.Engine/Sequence/PatternKind.cs.md) (enum) — `src/api/Gabriel.Engine/Sequence/PatternKind.cs`
- *…and 10 more (see symbol-graph.json)*

### src/api/Gabriel.Infrastructure · DependencyInjection

12 symbols across 11 files. Key symbols (by connectivity):

- [`DependencyInjection`](../Code/src/api/Gabriel.Infrastructure/DependencyInjection.cs.md) (class) — `src/api/Gabriel.Infrastructure/DependencyInjection.cs`
- [`IWebSearch`](../Code/src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md) (interface) — `src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs`
- [`WebSearchResult`](../Code/src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md) (record) — `src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs`
- [`BraveWebSearch`](../Code/src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs.md) (class) — `src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs`
- [`WebSearchTool`](../Code/src/api/Gabriel.Engine/Tools/Web/WebSearchTool.cs.md) (class) — `src/api/Gabriel.Engine/Tools/Web/WebSearchTool.cs`
- [`DuckDuckGoWebSearch`](../Code/src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs.md) (class) — `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`
- [`InstrumentedWebSearch`](../Code/src/api/Gabriel.Infrastructure/Tools/Web/InstrumentedWebSearch.cs.md) (class) — `src/api/Gabriel.Infrastructure/Tools/Web/InstrumentedWebSearch.cs`
- [`TavilyWebSearch`](../Code/src/api/Gabriel.Infrastructure/Tools/Web/TavilyWebSearch.cs.md) (class) — `src/api/Gabriel.Infrastructure/Tools/Web/TavilyWebSearch.cs`
- *…and 4 more (see symbol-graph.json)*

### src/api/Gabriel.API · ModelsController

11 symbols across 5 files. Key symbols (by connectivity):

- [`ModelsController`](../Code/src/api/Gabriel.API/Controllers/ModelsController.cs.md) (class) — `src/api/Gabriel.API/Controllers/ModelsController.cs`
- [`ModelsResponse`](../Code/src/api/Gabriel.API/Contracts/Models/ModelDto.cs.md) (record) — `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`
- [`ModelsResponse`](../Code/src/webapp/src/api/models.ts.md) (interface) — `src/webapp/src/api/models.ts`
- [`AgentOptions`](../Code/src/api/Gabriel.Core/Configuration/AgentOptions.cs.md) (class) — `src/api/Gabriel.Core/Configuration/AgentOptions.cs`
- [`ModelDto`](../Code/src/api/Gabriel.API/Contracts/Models/ModelDto.cs.md) (record) — `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`
- [`ModelDto`](../Code/src/webapp/src/api/models.ts.md) (interface) — `src/webapp/src/api/models.ts`
- [`SelectedModelDto`](../Code/src/api/Gabriel.API/Contracts/Models/ModelDto.cs.md) (record) — `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`
- [`SelectedModelDto`](../Code/src/webapp/src/api/models.ts.md) (interface) — `src/webapp/src/api/models.ts`
- *…and 3 more (see symbol-graph.json)*

### src/api/Gabriel.Engine · GabrielSequence

11 symbols across 7 files. Key symbols (by connectivity):

- [`GabrielSequence`](../Code/src/api/Gabriel.Engine/Sequence/GabrielSequence.cs.md) (record) — `src/api/Gabriel.Engine/Sequence/GabrielSequence.cs`
- [`IGabrielSequenceGenerator`](../Code/src/api/Gabriel.Engine/Sequence/IGabrielSequenceGenerator.cs.md) (interface) — `src/api/Gabriel.Engine/Sequence/IGabrielSequenceGenerator.cs`
- [`GabrielSequence`](../Code/src/webapp/src/api/sequence.ts.md) (interface) — `src/webapp/src/api/sequence.ts`
- [`IGabrielSequenceService`](../Code/src/api/Gabriel.Engine/Sequence/IGabrielSequenceService.cs.md) (interface) — `src/api/Gabriel.Engine/Sequence/IGabrielSequenceService.cs`
- [`generate`](../Code/prototype/generate.js.md) (function) — `prototype/generate.js`
- [`FrameLayers`](../Code/src/api/Gabriel.Engine/Sequence/FrameLayer.cs.md) (class) — `src/api/Gabriel.Engine/Sequence/FrameLayer.cs`
- [`fetchGabrielSequence`](../Code/src/webapp/src/api/sequence.ts.md) (function) — `src/webapp/src/api/sequence.ts`
- [`GabrielSequenceViewProps`](../Code/src/webapp/src/components/GabrielSequenceView.tsx.md) (interface) — `src/webapp/src/components/GabrielSequenceView.tsx`
- *…and 3 more (see symbol-graph.json)*

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

### src/webapp/src/api · MemoriesController

9 symbols across 4 files. Key symbols (by connectivity):

- [`MemoriesController`](../Code/src/api/Gabriel.API/Controllers/MemoriesController.cs.md) (class) — `src/api/Gabriel.API/Controllers/MemoriesController.cs`
- [`MemoryDto`](../Code/src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs.md) (record) — `src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs`
- [`MemoryDto`](../Code/src/webapp/src/api/memories.ts.md) (interface) — `src/webapp/src/api/memories.ts`
- [`saveMemory`](../Code/src/webapp/src/api/memories.ts.md) (function) — `src/webapp/src/api/memories.ts`
- [`SaveMemoryRequest`](../Code/src/webapp/src/api/memories.ts.md) (interface) — `src/webapp/src/api/memories.ts`
- [`listMemories`](../Code/src/webapp/src/api/memories.ts.md) (function) — `src/webapp/src/api/memories.ts`
- [`SaveMemoryRequest`](../Code/src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs.md) (record) — `src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs`
- [`MemoryType`](../Code/src/webapp/src/api/memories.ts.md) (type) — `src/webapp/src/api/memories.ts`
- *…and 1 more (see symbol-graph.json)*

*…and 123 smaller subsystems (see symbol-graph.json).*

## Most connected symbols

The load-bearing symbols — changes here have the widest blast radius.

| Symbol | Kind | Used by | Uses | File |
|---|---|---|---|---|
| [`AgentService`](../Code/src/api/Gabriel.Engine/Services/AgentService.cs.md) | class | 14 | 50 | `src/api/Gabriel.Engine/Services/AgentService.cs` |
| [`DependencyInjection`](../Code/src/api/Gabriel.Engine/DependencyInjection.cs.md) | class | 3 | 53 | `src/api/Gabriel.Engine/DependencyInjection.cs` |
| [`Message`](../Code/src/api/Gabriel.Core/Entities/Message.cs.md) | class | 46 | 2 | `src/api/Gabriel.Core/Entities/Message.cs` |
| [`DependencyInjection`](../Code/src/api/Gabriel.Infrastructure/DependencyInjection.cs.md) | class | 3 | 44 | `src/api/Gabriel.Infrastructure/DependencyInjection.cs` |
| [`Project`](../Code/src/api/Gabriel.Core/Entities/Project.cs.md) | class | 36 | 4 | `src/api/Gabriel.Core/Entities/Project.cs` |
| [`ConversationsController`](../Code/src/api/Gabriel.API/Controllers/ConversationsController.cs.md) | class | 2 | 30 | `src/api/Gabriel.API/Controllers/ConversationsController.cs` |
| [`Conversation`](../Code/src/api/Gabriel.Core/Entities/Conversation.cs.md) | class | 23 | 9 | `src/api/Gabriel.Core/Entities/Conversation.cs` |
| [`ITool`](../Code/src/api/Gabriel.Engine/Tools/ITool.cs.md) | interface | 27 | 1 | `src/api/Gabriel.Engine/Tools/ITool.cs` |
| [`GabrielToolBridge`](../Code/src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | class | 6 | 21 | `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs` |
| [`AgentEvent`](../Code/src/api/Gabriel.Engine/Services/AgentEvent.cs.md) | record | 14 | 11 | `src/api/Gabriel.Engine/Services/AgentEvent.cs` |

*Regenerated on every full documentation run; see [Agent/README.md](README.md) for how to use this pack.*