# Save-backed runtime automation blocker

## Decision

`working-save-smoke` is not implemented or production-allowlisted. Reflection-only
inspection of the installed Kingmaker `Assembly-CSharp.dll` established candidate
metadata, but did not establish a safe exact-save contract. Guessing the remaining
semantics would violate the runtime-automation safety requirements.

This investigation did not launch Kingmaker, Steam, or Unity Mod Manager, did not
open or modify a save, and did not decompile the Gunslinger DLL.

## Installed API metadata observed

The installed assembly exposes these public instance methods:

- `Kingmaker.Game.LoadGameFromMainMenu(SaveInfo)`
- `Kingmaker.Game.LoadGameForSmokeTest(SaveInfo)`
- `Kingmaker.Game.LoadGame(SaveInfo)`
- `Kingmaker.MainMenu.LoadGame(SaveInfo)`
- `Kingmaker.EntitySystem.Persistence.SaveManager.LoadRoutine(SaveInfo, Boolean)`
- `Kingmaker.EntitySystem.Persistence.SaveManager.AddCallbackAfterLoad(Action)`
- `Kingmaker.EntitySystem.Persistence.SaveManager.UpdateSaveListAsync()`
- `Kingmaker.EntitySystem.Persistence.SaveManager.UpdateSaveListIfNeeded(Boolean)`
- `Kingmaker.EntitySystem.Persistence.SaveManager.GetSaveByFile(String)`

`SaveInfo` publicly exposes `Name`, `FolderName`, `FileName`, `GameName`,
`GameId`, `Area`, `AreaNameOverride`, `PartyPortraits`, `GameSaveTime`, and
`GameTotalTime`. `Game` publicly exposes `Instance`, `Player`,
`CurrentlyLoadedArea`, `CurrentScene`, and `SaveManager`. `Player` exposes
party, main-character, inventory, game-time, and game-ID state suitable for a
post-load fingerprint if loading could first be proven safe.

The runtime-test callback already runs from Unity Mod Manager `OnUpdate`, so an
invocation made there would be on the established Unity/game thread. The request
parser and allowlist also provide explicit opt-in and normal-launch isolation.

## Unproven safety requirements

The installed metadata does not document or prove:

1. how to enumerate the completed save list without reading the private
   `SaveManager.m_SavedGames` field;
2. the meaning and side effects of the Boolean parameter to `LoadRoutine`;
3. whether `LoadGameForSmokeTest` is safe for a real named user save;
4. whether any candidate path increments `LoadedTimes`, migrates, rewrites,
   autosaves, or otherwise changes either save;
5. whether `AddCallbackAfterLoad` alone means the area, player, party,
   inventory, and mod-owned item state are all fully settled;
6. a supported positive link from the loaded game state back to the exact
   requested `SaveInfo`, rather than only matching mutable campaign metadata;
7. failure behavior for every candidate entry point and whether callback
   absence is the only observable failure signal.

Calling one of these methods based only on its metadata signature would guess
undocumented semantics. Accessing the private list by reflection would add a
version-fragile, unsupported dependency and still would not resolve write and
completion semantics.

## Safety-model status

The request layer could deny missing, unknown, and baseline save names, enforce
the sole allowlisted `KMG_AUTOMATION_WORKING` name, preserve the version/run-ID/
evidence guards, enforce a monotonic timeout, and keep non-save scenarios away
from a loader. Those checks are necessary but insufficient: they cannot make an
unproven game load API non-mutating.

Accordingly, no save-name request contract, save-loading adapter, fingerprint,
or `sprint30-save-backed-smoke` implementation has been added. Existing normal
launch behavior remains unchanged, and `mod-load-smoke` remains the only
production-allowlisted scenario.

## Evidence needed to unblock

Human-provided authoritative API documentation or curated, legally permissible
game-code evidence must establish exact save discovery, load-mode side effects,
completion/failure semantics, and absence of save writes or migration. Only
then should a source change add a minimal adapter with explicit write-API
exclusion tests and a supervised run against `KMG_AUTOMATION_WORKING`.
