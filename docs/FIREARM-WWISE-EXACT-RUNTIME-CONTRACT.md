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

## Completed IL inspection

`AkSoundEngine.PostEvent(string, UnityEngine.GameObject)` returns `uint`. The
installed integration also exposes the event-ID overload and callback/flag/
external-source variants; the two-argument string/GameObject overload is the
narrow gameplay contract. `AkSoundEngine.AK_INVALID_PLAYING_ID` is the literal
`uint 0`. The wrapper constructs an `AkAutoObject` around the supplied
GameObject before its native call, so the integration performs the required
temporary registration handling; the mod must not attach duplicate `AkGameObj`
components to every unit.

`Kingmaker.Sound.SoundBanksManager` maintains a private
`Dictionary<string,int> s_LoadCount`, uses `AkBankManager.LoadBankAsync`, and
calls manager unload when its count reaches the relevant boundary. This is
curated corroboration, not a stable public success API. The selected custom
path therefore loads through `AkBankManager` once and treats only a nonzero
PostEvent playing ID as event acceptance.

`WeaponVisualParameters` stores private `m_SoundType`, `m_WhooshSound`, and
`m_MissSoundType`. Their getters fall back through `Prototype` when the local
enum is zero or the local whoosh string is empty. Clearing only those locals
would therefore not suppress inherited combat audio while Prototype remains.
Equip/unequip/inventory sounds are separate fields. Because preserving
Human testing subsequently proved an inherited crossbow release on a
Blunderbuss misfire. The firearm presentation now materializes the resolved
model, belt, sheath, animation, attachment, sound-size/type, miss, equip,
unequip, and inventory values into the firearm-owned instance before severing
Prototype. Only `m_WhooshSound` is emptied. This is the narrow testable
intervention for the release/twang hypothesis; human listening remains the
authority for whether the overlap is gone.

## Integrated regression evidence, 2026-08-20

The staging boundary additionally requires a structurally valid BKHD/DIDX/DATA/HIRC bank with exactly five unique, nonempty, in-bounds embedded media entries. The guarded global preview accepts an exact `FirearmKind`, permitting one fresh process to validate the complete Pistol, Musket, Blunderbuss, Revolver, and Rifle Event-name mapping.

Run `20260820T0635323959656Z-88cfa04a0deb4595bfbc2ee8d4284e31` observed one load and nonzero playing IDs for all five global Events plus the live-unit/committed Blunderbuss controls, with zero normal-post increment for forced misfire. This is the strongest safe automated event-acceptance boundary. It does not supersede the fresh human report of silence and cannot establish audibility, mix, device output, or absence of an inherited crossbow layer.
