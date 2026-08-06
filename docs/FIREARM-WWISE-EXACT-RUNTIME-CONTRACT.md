# Exact Kingmaker Wwise runtime contract

## Proven local baseline

Source: the installed Steam Kingmaker 2.1.7b managed assemblies used by the
repository's exact-reference build. Inspection is curated; no proprietary
assembly or raw decompilation output is committed.

| Contract | Exact observed signature |
|---|---|
| Engine readiness | `bool AkSoundEngine.IsInitialized()` |
| Manager load | `void AkBankManager.LoadBank(string, bool, bool)` |
| Manager unload | `void AkBankManager.UnloadBank(string)` |
| Manager bulk unload | `void AkBankManager.DoUnloadBanks()` |
| Base path | `string AkBasePathGetter.GetPlatformBasePath()` |
| Valid base path | `string AkBasePathGetter.GetValidBasePath()` |
| Bank handle identity | `string AkBankHandle.bankName` |
| Bank handle relative path | `string AkBankHandle.relativeBasePath` |

Direct `AkSoundEngine.LoadBank` overloads return `AKRESULT` and include
string-name variants with an out `uint` bank ID. They are comparison evidence,
not the selected release path.

## Selected loading strategy

Stage only `KMG_Firearms.bnk` into
`Kingmaker_Data/StreamingAssets/Audio/GeneratedSoundBanks/Windows`, wait for
`AkSoundEngine.IsInitialized()`, then request exactly one process-lifetime
`AkBankManager.LoadBank("KMG_Firearms", false, false)`. This matches the
Kingmaker-era manager precedent and avoids changing Wwise base paths.

Manager load request is not proof of successful playback. A non-invalid
playing ID from the exact `PostEvent` overload will be required for automated
event-acceptance evidence; audibility remains a human judgment.

## Discharge-to-audio matrix

| Route | Commit boundary | Normal event rule |
|---|---|---|
| Ordinary marked attack | natural roll classified after round commit | one for non-misfire, including miss; zero for misfire/rejection |
| Scatter Shot | complete custom volley transaction commits | one Blunderbuss event; zero for all-roll misfire/rollback |
| Dead Shot | custom deed transaction commits after probes/delivery | one event total; never for probes; zero on misfire/rollback |
| Startling Shot | state, grit, and effect transaction commits | one event; zero on rollback |
| Menacing Shot | enumerated delivery reaches committed completion | one event total, never per target; zero on cancellation/fault |
| Stop Bleeding | complete deed transaction commits | one event; zero on rollback |

## Still under exact inspection

- Every `AkSoundEngine.PostEvent` overload and the exact invalid playing-ID
  constant/value.
- `AkGameObj` registration behavior for `PostEvent(GameObject)`.
- `SoundBanksManager` observable load tracking.
- `Kingmaker.Sound.AkAudioService` initialization lifecycle.
- Native `WeaponVisualParameters` combat-sound getter/call sites. Current
  production presentation already reports inherited crossbow sound fallback as
  cleared; this mission will verify rather than broaden that mutation.
