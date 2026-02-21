# PROJECT CONTEXT

## Project
- Name: Elementum
- Platform target: Unity WebGL (Yandex Games), mobile-first afterwards
- Source of truth: `AI/elementum_manifest_mvpplus.md`

## Current Architecture Slice (implemented 2026-02-21)
- Composition-based wiring through `CompositionRoot`, no singletons.
- Cross-system communication through `SignalBusSO` events.
- Input abstraction via `InputAdapter` (high-level `Tap/Swipe/Hold` signals).
- Gameplay stub via `CoreLoopController` (mass progress, thresholds, collapse/win checks).
- Ads pause integration via `PauseService` (`Time.timeScale` + `AudioListener.pause` restore-safe flow).
- Versioned JSON save flow via `SaveService` + `SaveDataModel`.
- Pooling base via `ObjectPool` + `IPoolable`.
- Tunables extracted to ScriptableObjects: `InputConfigSO`, `CoreLoopConfigSO`, `SaveConfigSO`.

## Folder Baseline
- `Assets/_Project/Scripts/Core`
- `Assets/_Project/Scripts/Data`
- `Assets/_Project/Scripts/Input`
- `Assets/_Project/Scripts/Systems`
- `Assets/_Project/Scripts/Pool`
- `Docs/PROJECT_CONTEXT.md`
- `Docs/IMPLEMENTATION_LOG.md`

## Signal Contracts (v1)
- Input:
  - `Tap(Vector2)`
  - `Swipe(Vector2 start, Vector2 end, float duration)`
  - `Hold(Vector2 pos, float duration)`
- Gameplay:
  - `GameStateChanged(GameState)`
  - `MassChanged(int current, int delta, string reason)`
  - `CardPickRequested(int checkpointPercent)`
  - `CollapseStarted()`
  - `WinReached()`
- Ads:
  - `AdOpened()`
  - `AdClosed()`
  - `RewardGranted()`
- Save:
  - `SaveRequested()`

## Input Binding Defaults
- Action asset expected in inspector.
- Default paths:
  - `UI/Point`
  - `UI/Click`
- Gesture thresholds are read from `InputConfigSO`.

## Save Model
- File: JSON in `Application.persistentDataPath`.
- Version field: `SaveVersion` (current `1`).
- Core fields: `LastKnownMass`, `BestMass`, `TotalWins`, `StarDust`, `LastElementId`.
- Cloud integration extension point: `ExportCloudJson` / `TryLoadCloudJson`.

## Open Integration Tasks
- Add dedicated gameplay action map (`Gameplay/Point`, `Gameplay/Press`) instead of using `UI/*`.
- Connect anti-system active count to `CoreLoopController.SetActiveAntiCount`.
- Add UI listeners for HUD mass, card pick windows, collapse state, and result screen.
- Add Yandex SDK bridge component that calls `PauseService.NotifyAdOpened/Closed/RewardGranted`.

## Environment Notes
- During this iteration UnitySkills package showed Unity 6 compile incompatibility in `PhysicsSkills.cs` (`PhysicMaterial` vs `PhysicsMaterial`).
- A local runtime patch was applied in `Library/PackageCache/com.besty.unity-skills@86680cd7a616/Editor/Skills/PhysicsSkills.cs` to unblock compilation.
- Because the patch is in `PackageCache`, it is not a durable source-controlled fix yet.
