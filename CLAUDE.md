# CLAUDE.md

This file provides**New API Methods:**
- `SubscribeToCellButtonDown(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing)`
- `SubscribeToCellButtonUp(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing)`
- `SubscribeToCellButtonState(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing)`
- `SubscribeToCellButtonComplete(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing, float longPressDuration = 1.0f)` - настраиваемое время длительного нажатия

**Key Features:**
- Uses LocalInputHandler for precise cell detection instead of coordinate calculation
- Events trigger only when mouse is over registered cell handlers (handler.IsMouseOver)
- Customizable long press duration per subscription
- Multiple long press events with different durations on same cell
- Automatic cleanup when events trigger or button is released

**Architecture:**
- `LocalInputHandler`: Unity component on cell GameObjects with colliders
- `InputService`: Central coordination of global input events with cell-specific handlers
- Registration: Cells register via `RegisterCellHandler(gridPosition, handler)`
- Detection: Uses Unity's pointer event system for accurate hit detectionance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a 2D incremental plant-growing game built in Unity 2022.3.25f1. Players plant seeds on a grid, water them, and harvest them for rewards. The architecture emphasizes reactive programming patterns and dependency injection.

## Recent Changes

### Input System Refactoring (September 2025)

**Major Change**: Replaced fixed `OnCellClicked` events with flexible button subscription system for cells.

**Before:**
```csharp
_inputService.OnCellClicked.Subscribe(HandleCellClick);
_inputService.OnCellClickedLate.Subscribe(HandleCellClick);
```

**After:**
```csharp
// Subscribe to specific button for specific cell
_inputService.SubscribeToCellButtonDown(new Vector2Int(0, 0), KeyCode.Mouse0, InputTiming.Tick)
    .Subscribe(_ => HandleCellAction(new Vector2Int(0, 0)));

// Subscribe to different buttons for same cell
_inputService.SubscribeToCellButtonDown(position, PlayerInput.WateringAction, InputTiming.LateTick);
_inputService.SubscribeToCellButtonDown(position, PlayerInput.PlantAction, InputTiming.LateTick);
```

**New API Methods:**
- `SubscribeToCellButtonDown(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing)`
- `SubscribeToCellButtonUp(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing)`
- `SubscribeToCellButtonState(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing)`
- `SubscribeToCellButtonComplete(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing)` - длительное нажатие (1 сек)

**Benefits:**
- Can bind any key/mouse button to any cell
- Multiple actions per cell (left-click, right-click, keyboard keys)
- Consistent with existing global button subscription system
- More flexible for different game mechanics

**Example Usage:**
See `CellInputExamples.cs` for comprehensive usage patterns.

## Build and Development Commands

This is a Unity project - open it in Unity 2022.3.25f1 and use Unity Editor for building and testing. No additional build scripts or commands are configured.

**Key Development Workflow:**
- Open project in Unity 2022.3.25f1
- Main scene is configured in EditorBuildSettings
- Use Unity's Play Mode for testing
- Test Framework package is included but no tests are currently implemented

## Core Architecture

### Dependency Injection (VContainer)
- **Entry Point**: `GameLifetimeScope` configures all services and dependencies
- **Registration**: All singletons registered via `builder.RegisterGardenSystems()` extension method
- **Service Pattern**: Core functionality split into services (`IEconomyService`, `ISaveService`, `IGridService`, etc.)
- **Factory Pattern**: `IPlantFactory` creates plant entities with proper dependency injection

### Reactive Programming (UniRx)
- **State Management**: All reactive state uses `ReactiveProperty<T>` and `IReadOnlyReactiveProperty<T>`
- **Events**: Communication through `Subject<T>` and `IObservable<T>` streams
- **Memory Management**: Always use `CompositeDisposable` with `.AddTo(_disposables)` for cleanup
- **Presenters**: Follow MVP pattern - presenters subscribe to services and update views reactively

### Project Structure
```
Assets/_Project/Scripts/
├── Garden/
│   ├── Core/           # Business logic (Services, Entities, Factories, Presenters)
│   ├── Data/           # Data structures and ScriptableObjects
│   ├── View/           # MonoBehaviour components for visual representation  
│   ├── UI/             # UI components and views
│   └── DI/             # Dependency injection configuration
├── Input/              # Input handling system
│   ├── Examples/       # Usage examples for input system
│   └── ...
└── SkillTree/          # Skill system (separate feature)
```

## Key Systems

### Input System
- **Global Input**: `SubscribeToButtonDown/Up/State(KeyCode, InputTiming)` for global key bindings
- **Cell Input**: `SubscribeToCellButtonDown/Up/State(Vector2Int, KeyCode, InputTiming)` for cell-specific actions
- **Timing**: `InputTiming.Tick` (early) vs `InputTiming.LateTick` (late) processing
- **Player Keys**: `PlayerInput.WateringAction` (Mouse0), `PlayerInput.PlantAction` (Mouse1)

### Grid System
- **Grid Configuration**: Size and display settings in `GameSettings` ScriptableObject
- **World Positioning**: `GridService.GridToWorldPosition()` converts grid coords to world space
- **Soil Types**: Each cell has a `SoilType` that affects plant growth via `GetGrowthModifier()`
- **Coordinate System**: Uses Vector2Int for grid positions, Vector2 for world positions

### Plant System  
- **Plant Data**: Configured via `PlantData` ScriptableObjects with growth stages, rarity, rewards
- **Plant Entity**: `PlantEntity` manages growth state, watering requirements, and lifecycle
- **Growth Mechanics**: Plants progress from `PlantState.New` to `FullyGrown`, requiring water to avoid withering
- **Factory Creation**: Plants created via `PlantFactory` which instantiates prefabs and injects dependencies

### Economy System
- **Currency**: Coins managed through `IEconomyService.Coins` ReactiveProperty
- **Collectibles**: Petals collection system with `PetalsCollection` and reactive change events
- **Persistence**: Economy state serialized via `GetSaveData()` methods

### Watering System
- **Watering Mechanics**: Plants require water every ~10 seconds or they wither
- **Long Press UI**: `LocalInputHandler` provides comprehensive input handling including clicks, hover effects, and long press interactions for watering
- **State Tracking**: Plants track `TimeSinceLastWatering` and transition states accordingly

## Key Dependencies

**Package Dependencies** (from `Packages/manifest.json`):
- `com.neuecc.unirx`: Reactive programming framework
- `jp.hadashikick.vcontainer`: Dependency injection container  
- DOTween: Animation library (in `Assets/Plugins/Demigiant/DOTween/`)
- Unity's URP, UGUI, TextMeshPro for rendering and UI

## Configuration and Settings

- **Game Settings**: Central configuration via `GameSettings` ScriptableObject
  - Grid size, growth timings, auto-save intervals
  - Plant rarity chances and available plants array
  - Watering duration and wither chances
- **Auto-save**: Configured interval in `GameSettings.AutoSaveInterval` (30 seconds default)
- **DOTween**: Optimized configuration in `GameInitializer` with capacity limits and recycling

## Common Development Patterns

### Adding New Services
1. Create interface in `Core/Interfaces/` 
2. Implement in `Core/Services/`
3. Register in `GameLifetimeScope` via `builder.Register<IInterface, Implementation>(Lifetime.Singleton)`
4. Inject via constructor with `[Inject]` attribute

### Adding New UI
1. Create view MonoBehaviour in `View/` or `UI/`
2. Create presenter in `Core/Presenters/` implementing `IInitializable`
3. Add presenter to entry points in `GameLifetimeScope`
4. Use reactive subscriptions to bind data to view

### Working with Plant States
- All plant state changes go through `PlantEntity.State` ReactiveProperty
- Visual updates happen automatically via state subscription in plant view
- Growth progress is separate from state - use `GrowthProgress` ReactiveProperty

## Important Implementation Notes

### Save System
- Currently using placeholder `SaveService` - implement JSON serialization to `Application.persistentDataPath`
- Save data structure defined in `SaveData` class but needs full implementation
- Auto-save triggers on coin changes (throttled) and regular intervals

### Plant Growth Service
- `IPlantGrowthService` interface exists but needs implementation
- Should handle time-based growth progression using `Observable.Interval` or similar
- Growth rates affected by soil modifiers via `GridCell.GetGrowthModifier()`

### Event System
- Grid events: `OnCellClicked`, `OnPlantHarvested` with `PlantHarvestedEvent`
- Plant destruction: `PlantDestroyedEvent` for cleanup and effects
- Economy events: Coin and petal collection changes trigger reactive updates

### Memory Management
- Always dispose of `CompositeDisposable` in `IDisposable.Dispose()`  
- Use `.AddTo(_disposables)` for all reactive subscriptions
- Plant entities handle their own cleanup when destroyed