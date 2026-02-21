# IMPLEMENTATION LOG

Current stage: Stage 1/5: Foundation architecture slice wired

## Completed Tasks
- [2026-02-21 | T-001] Created baseline project script folders under `Assets/_Project/Scripts/*`.
- [2026-02-21 | T-002] Added event-driven core bus: `SignalBusSO`.
- [2026-02-21 | T-003] Added core state enum: `GameState`.
- [2026-02-21 | T-004] Added data configs: `InputConfigSO`, `CoreLoopConfigSO`, `SaveConfigSO`.
- [2026-02-21 | T-005] Added `InputAdapter` with unified tap/swipe/hold emission from Input System actions.
- [2026-02-21 | T-006] Added `CoreLoopController` with mass progression, checkpoints (33/66), collapse threshold, win check.
- [2026-02-21 | T-007] Added `PauseService` with ad open/close/reward events and safe `timeScale/audio` restore.
- [2026-02-21 | T-008] Added versioned save layer: `SaveDataModel` + `SaveService` (JSON load/save, migration hook, cloud payload helpers).
- [2026-02-21 | T-009] Added pooling baseline: `ObjectPool` + `IPoolable`.
- [2026-02-21 | T-010] Added composition wiring entrypoint: `CompositionRoot`.
- [2026-02-21 | T-011] Created and populated docs: `PROJECT_CONTEXT.md` and this log.
- [2026-02-21 | T-012] Applied local compatibility patch for UnitySkills physics material API mismatch in `Library/PackageCache/com.besty.unity-skills@86680cd7a616/Editor/Skills/PhysicsSkills.cs` (Unity 6 compile blocker).

## Next Tasks
- Stage 2: Implement F-01 SO domain models (`ElementSO`, `RunBalanceSO`, `AntiTypeSO`, `CardSO`, `MetaSO`).
- Stage 2: Add runtime `GameContext` model and explicit state transitions (`Playing/Paused/CardPick/Collapse/Win/Lose` flow).
- Stage 2: Build proton spawn/orbit/launch pipeline and connect `+M` events.
- Stage 3: Build anti entities, hit-stun, targeting, eat/corrupt logic with caps.
- Stage 3: Integrate collapse spawn-off rule and assault mass drain loop.
- Stage 4: Add HUD feedback (+M/-M floating aggregation), card-pick UI, end-screen stats.
- Stage 5: Integrate Yandex SDK adapters (rewarded/interstitial, cloud save sync, leaderboard stub) and performance tuning.

## Known Issues / Tech Debt
- Current input uses `UI/Point` and `UI/Click`; dedicated gameplay map still missing.
- `CoreLoopController` uses external anti count setter stub until anti-system is implemented.
- `CardPickRequested` currently emits a signal only; pause/select/apply flow not implemented yet.
- No automated tests yet for save migration and gesture classification thresholds.
- UnitySkills REST server currently not listening on `localhost:8090`; likely requires manual restart from Unity menu after domain reload.

## Performance Notes
- No LINQ used in runtime hot paths.
- No per-frame allocations in the introduced scripts (no `Update` loops in current slice).
- Pooling baseline ready; spawn-heavy systems must use pool instead of `Instantiate/Destroy`.
- Save operations are event-driven, not frame-driven.
- WebGL constraints respected: no threading APIs used.

## Yandex SDK Integration Notes
- Ads lifecycle contract is in place via `PauseService` + `SignalBusSO` events:
  - `OnAdOpened`: set `Time.timeScale = 0`, `AudioListener.pause = true`.
  - `OnAdClosed`: restore previous values exactly.
  - `OnRewardGranted`: separate signal for reward flow.
- Cloud save payload helpers exist in `SaveService`:
  - `ExportCloudJson()`
  - `TryLoadCloudJson(string cloudJson)`
- Direct Yandex SDK bridge component is pending (planned Stage 5).
- UnitySkills package in this environment required a local patch to compile under Unity 6; this change is in `Library/PackageCache` and may need a durable package override later.
