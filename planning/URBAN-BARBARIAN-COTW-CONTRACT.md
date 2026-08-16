# Urban Barbarian Call of the Wild contract

Status: **SUPPORTED GRAPH QUALIFIED; NO RUNTIME ADAPTER REQUIRED**.

## Independence rule

Urban Barbarian is a native Kingmaker Barbarian archetype. It has no compile-
time Call of the Wild reference and no new external library. Urban blueprint
registration and native Barbarian publication must succeed when CotW is absent.
An absent, changed, unknown, or ambiguous CotW compatibility surface may disable
or mark only optional interoperability; it must never disable Urban core or any
other package module.

CotW's Urban Bloodrager is not an implementation donor. Its whole-stat-only
choice set is incomplete for Urban Barbarian; its chained class-skill mutation
is unsafe; and its magic/proficiency replacements belong to Bloodrager.

## Evidence required before an adapter decision

The guarded architecture inventory must record both final graphs:

- loaded CotW mod-entry ID/version and assembly identity;
- CotW DLL SHA-256 and MVID plus exact settings hash;
- native Rage feature, activatable, resource, buff, Greater/Tireless/Mighty
  facts, and representative native rage powers;
- CotW Rage marker identity, component type/assembly, and graph attachment;
- representative CotW passive and activated rage powers and how each recognizes
  or modifies Rage;
- any component ordering with semantic significance; and
- whether the selected Urban architecture already inherits/produces the exact
  marker and action behavior without reflection.

No adapter will be added merely because CotW is installed. If the finalized
Urban Rage already satisfies the marker and action contract, this document will
conclude that no runtime adapter is required.

## Conditional structural adapter rules

Only demonstrated missing interoperability may authorize an adapter. It must:

- use a structural contract and fingerprint, not only a private name;
- require exact mod entry, assembly, lifecycle, graph, and settings evidence;
- reconcile at a deterministic point after CotW Rage construction;
- preserve component ordering where semantics require it;
- append only missing exact behavior and reject duplicates;
- never change CotW Urban Bloodrager, native Rage owners, or unrelated owners;
- expose a precise failed-check diagnostic; and
- leave Urban publication and unrelated modules active when it cannot qualify.

## Required state model

| CotW state | Urban core | Optional interoperability | Diagnostic |
| --- | --- | --- | --- |
| Absent | Available | Not applicable | CotW not loaded |
| Supported normal | Available | Qualified if exact marker/action tests pass | Exact fingerprint |
| Supported balance fixes | Available | Qualified if exact marker/action tests pass | Exact fingerprint/settings |
| Unknown | Available | Disabled or unqualified | Exact failed structural check |
| Ambiguous | Available | Disabled or unqualified | Exact conflicting candidates/check |

## Adapter decision and evidence

The supported profile is CotW mod ID `CallOfTheWild`, version `1.14.4c-2.1`,
`balance_fixes=true`, DLL SHA-256
`4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`,
MVID `8caab254-aacf-4811-8093-44b9184e6e53`, settings SHA-256
`24CC3F80269992A53EBBFD1F5986E5AAB056841D6B2F43D8E22E764CDB73F6E8`.
The guarded final graph has 50 Rage Power selection entries versus 17 without
CotW, for 33 exact CotW additions. Representative passive/activated surfaces
include Superstition `f5b971182d6445848ab8fd55c47c14f1`, Clear Mind
`cf9b15c016a64812a088b6a25b703e81`, and Terrifying Howl
`57c2d49f8b4d45d0b6fcb111bdf16651`.

CotW adds four `FeatureReplacement` components to the native Rage buff. Each
routes only to CotW Bloodrage or CotW Urban Bloodrager whole-stat buffs. Those
components are not a generic Rage marker and must not be copied to Urban
Barbarian. The native retained `AddFactContextActions` action graph supplies
`UnitCondition.BarbarianRage`; the retained `SpellDescriptorComponent` supplies
the Rage descriptor; and the exact native Rage feature/activatable/resource
identities remain owned. The Urban clone therefore already exposes the marker,
action, descriptor, resource, and prerequisite surfaces used by native and CotW
rage powers. No reflection adapter is required for this supported fingerprint.

CotW absent run `20260816T1331159196672Z...` passed with the native 12-component
Rage graph. CotW-present run `20260816T1328091838409Z...` passed with the same
12 native components followed by the four Bloodrager routing components. The
reversible absent transaction restored CotW, CotW settings SHA-256
`24CC3F...F6E8`, and feature settings SHA-256 `5B6030...665E` exactly.

Unknown or ambiguous CotW versions remain an explicit unqualified optional
status only. Urban core publication and every unrelated module remain active;
no compatibility mutation is attempted.

The player-facing UMM panel consumes an Urban-only status registry. It always
labels the native core as available and CotW as optional. The supported status
requires the exact DLL SHA-256/MVID above, one loaded assembly, the finalized
four-component CotW `FeatureReplacement` tail on native Rage, exactly one
retained native action marker, exactly one retained Rage descriptor, and no
CotW component copied to the Urban buff. Assembly-count, identity, finalized-
graph, marker, or duplicate-behavior failures name the exact structural check,
mark only optional interoperability unqualified, and leave Urban publication
untouched. CotW absence is displayed as not applicable.

## Final-candidate compatibility seal

The immutable `0.0.87` repair candidate at source commit
`b2e6f2b3062cc4146807d44bb1314d3e7bccf4ac` repeated the complete observer:

- normal: `20260816T2148345402027Z...`, `balance_fixes=false`, settings
  SHA-256 `E99445DA6D9E0A73F0D9F3770D6ED5974E0167F0AB8441E969C5EE46E4184885`, PASS;
- balance fixes: `20260816T2151049178079Z...`, `balance_fixes=true`, settings
  SHA-256 `24CC3F80269992A53EBBFD1F5986E5AAB056841D6B2F43D8E22E764CDB73F6E8`, PASS;
- absent: `20260816T2153236927353Z...`, Urban core available and optional
  interoperability not applicable, PASS.

Both present profiles again resolved CotW `1.14.4c-2.1`, DLL SHA-256
`4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`,
and MVID `8caab254-aacf-4811-8093-44b9184e6e53`. Transactions
`urban87-cotw-normal-20260816`, `urban87-cotw-balance-20260816`, and
`urban87-cotw-absent-20260816` are all `Restored` with
`restorationVerified=true`. No adapter is required for the supported
fingerprint; unknown/ambiguous behavior remains isolated and cannot disable
Urban core.

The following `0.0.83` seal is preserved as superseded pre-rejection evidence
only and does not establish human acceptance.

The immutable `0.0.83` candidate at artifact commit
`06cad804651faaace17bdf8432bcd071d50ce9e7` repeated the graph observer under
all required focused profiles:

- normal: `20260816T1842028623376Z...`, `balance_fixes=false`, settings
  SHA-256 `E99445DA6D9E0A73F0D9F3770D6ED5974E0167F0AB8441E969C5EE46E4184885`, PASS;
- balance fixes: `20260816T1844364358461Z...`, `balance_fixes=true`, settings
  SHA-256 `24CC3F80269992A53EBBFD1F5986E5AAB056841D6B2F43D8E22E764CDB73F6E8`, PASS;
- absent: `20260816T1847344493320Z...`, no CotW directory or assembly, Urban
  core available and optional interoperability not applicable, PASS.

Both present profiles resolved CotW `1.14.4c-2.1`, DLL SHA-256
`4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`,
and MVID `8caab254-aacf-4811-8093-44b9184e6e53`. Each named profile transaction
restored the complete pre-profile Mods tree exactly. These results seal the
no-adapter decision for the supported fingerprint; unknown and ambiguous
fingerprints remain isolated fast-test outcomes that cannot disable Urban core.
