# Hazina Integration Plan

## Doelstelling
Ombouwen van LearningTool naar gebruik van Hazina framework services waar mogelijk, voor betere herbruikbaarheid en minder duplicate code.

## Huidige Architectuur

### ChatService (handmatige OpenAI integratie)
- Directe `OpenAIClient` en `ChatClient` gebruik
- Tools handmatig gedefinieerd via `ChatTool.CreateFunctionTool`
- Tool execution via switch statement in ChatController
- System prompt hardcoded in ChatService

### KnowledgeService (custom repositories)
- `ISkillRepository`, `ITopicRepository`, `ICourseRepository`
- SQLite via Entity Framework Core
- Zoekfunctionaliteit in repositories

## Hazina Services Beschikbaar

### 1. Hazina.LLMs.OpenAI
**Classes:**
- `SimpleOpenAIClientChatInteraction` - wrapper voor OpenAI chat met tool support
- `OpenAIClientWrapper` - client initialisatie

**Voordelen:**
- Automatische tool handling via IToolsContext
- Logging ingebouwd
- Token tracking via callbacks
- Retry logic en error handling

### 2. Hazina Tools Framework
**Interfaces:**
- `IToolsContext` - container voor tools + callbacks
- `HazinaChatTool` - tool definitie met execute functie

**Voordelen:**
- Declaratieve tool definitie
- Automatische parameter parsing
- Tool execution lifecycle hooks
- Max iterations protection

### 3. Hazina.Store.Sqlite (optioneel)
- Document store interface
- Kan gebruikt worden voor Skills/Topics/Courses storage

## Integratie Plan

### Fase 1: OpenAI Client Wrapper (Priority: HIGH)
**Wat:**
- Vervang directe OpenAI client door `SimpleOpenAIClientChatInteraction`
- Implementeer `IToolsContext` voor tool management

**Waarom:**
- Elimineer duplicate OpenAI setup code
- Krijg ingebouwde logging, retry logic, token tracking
- Consistente error handling

**Files te wijzigen:**
- `ChatService.cs` - vervang OpenAI client initialisatie
- Add `ToolsContextFactory.cs` - create IToolsContext instances
- `Program.cs` - register Hazina services

**Schatting:** 2-3 uur

### Fase 2: Tools Framework (Priority: HIGH)
**Wat:**
- Converteer handmatige tool definitiestowards HazinaChatTool
- Verplaats tool execution logic naar tool definitions
- Gebruik IToolsContext.Add() in plaats van switch statements

**Waarom:**
- Tools zijn self-contained (definitie + execution samen)
- Makkelijker te testen
- Makkelijker nieuwe tools toe te voegen
- Elimineer ChatController switch statements

**Files te wijzigen:**
- Add `Tools/SearchLibraryTool.cs`
- Add `Tools/AddSkillTool.cs`
- Add `Tools/AddTopicTool.cs`
- Add `Tools/AddCourseTool.cs`
- Add `Tools/RemoveSkillTool.cs`
- Add `Tools/GetUserSkillsTool.cs`
- Modify `ChatController.cs` - remove ExecuteToolAsync switch
- Modify `ChatService.cs` - add tools to IToolsContext

**Schatting:** 3-4 uur

### Fase 3: Prompt Management (Priority: MEDIUM)
**Wat:**
- Externalize system prompts to configuration
- Use Hazina.AI.PromptManagement voor prompt templates

**Waarom:**
- Prompts aanpasbaar zonder recompile
- Versioning van prompts
- A/B testing mogelijkheid

**Files te wijzigen:**
- Add `prompts/general-chat.txt`
- Add `prompts/course-teaching.txt`
- Modify `ChatService.cs` - load prompts from files
- Add prompt configuration to appsettings.json

**Schatting:** 1-2 uur

### Fase 4: Storage Abstraction (Priority: LOW)
**Wat:**
- Optioneel: wrap repositories met Hazina.Store.Sqlite interfaces
- Zorgt voor consistentie met andere Hazina apps

**Waarom:**
- Makkelijker te migreren naar andere storage backends later
- Consistent met Hazina ecosystem

**Schatting:** 2-3 uur

## Dependencies Toevoegen

```xml
<!-- In src/LearningTool.Application/LearningTool.Application.csproj -->
<ItemGroup>
  <PackageReference Include="Hazina.LLMs.Client" Version="1.0.0" />
  <PackageReference Include="Hazina.LLMs.OpenAI" Version="1.0.0" />
  <PackageReference Include="Hazina.LLMs.Classes" Version="1.0.0" />
</ItemGroup>
```

## Implementatie Volgorde

1. ✅ Create feature/hazina-integration branch
2. ⏳ Fase 1: OpenAI Client Wrapper (start here)
3. ⏳ Fase 2: Tools Framework
4. ⏳ Fase 3: Prompt Management
5. ⏳ Fase 4: Storage Abstraction (optional)

## Risico's & Mitigatie

**Risico:** Hazina packages niet gepubliceerd op NuGet
**Mitigatie:** Gebruik local package references naar C:/Projects/hazina/nupkgs

**Risico:** Breaking changes tijdens development
**Mitigatie:** Pin exact versions in csproj

**Risico:** Extra complexity
**Mitigatie:** Alleen integreren waar duidelijk voordeel is (Fase 1+2), rest optioneel

## Success Criteria

- [ ] Code compile zonder errors
- [ ] Alle bestaande tests blijven passeren
- [ ] Tool calls werken identiek aan huidige implementatie
- [ ] Minder duplicate code dan voor integratie
- [ ] Chat functionaliteit werkt in productie

## Timeline

- Fase 1: Vandaag (22 feb)
- Fase 2: Vandaag (22 feb)
- Fase 3: Optioneel later
- Fase 4: Optioneel later
- Testing & Deployment: Morgen (23 feb)
