# Brown-Fur / Call of the Wild Contract

Status: structural contract reconciliation qualified for the installed CotW
subject; archetype publication and mechanical adapters remain disabled.

## Engineering authority

- Verified engineering base: `a8b19fe39285da44ac443b7bcbd217870ec6ffb6`
- Cleanup human acceptance: **PENDING / intentionally deferred**
- Brown-Fur authorization: explicit user override permits development from the
  pre-human cleanup candidate.
- This authority does not accept or complete the deferred cleanup sprint.

## Inspected CotW fingerprint

The installed investigation subject was inspected without adding a compile-time
or package dependency.

| Evidence | Value |
| --- | --- |
| CotW mod ID | `CallOfTheWild` |
| CotW mod version | `1.14.4c-2.1` |
| Assembly full name | `CallOfTheWild, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null` |
| DLL SHA-256 | `4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915` |
| DLL MVID | `8caab254-aacf-4811-8093-44b9184e6e53` |
| Settings SHA-256 | `24CC3F80269992A53EBBFD1F5986E5AAB056841D6B2F43D8E22E764CDB73F6E8` |
| Inspected balance mode | `balance_fixes=true` |
| Blueprint catalog SHA-256 | `F227B1C302DC8DB9773DE483369407ECC4A154B4082D83C97FCFE0C65912A4F4` |

These binary hashes are evidence, not the sole compatibility gate. Runtime
publication requires every structural check below to agree.

The guarded, save-free `observe-brown-fur-cotw-contract` scenario passed on
source commit `e1424cf139214e71c740b69049097f031e9571fd`. That result proves the
installed dependency identity, reflected public/static surfaces, blueprint
cross-validation, active balance progression, Shared Spells signatures, and
isolated coordinator outcome. It does not claim that player-facing Brown-Fur
mechanics are implemented.

## Required blueprint identities

| Surface | Canonical GUID |
| --- | --- |
| Arcanist class | `19c3cf3d51cf4cbf9a136a600c26585a` |
| Arcanist progression | `2d28526efc2e4a9cb6a84c85267fb344` |
| Arcanist casting spellbook | `0c21cfcab6ce4395bd4df330ab3cf715` |
| Arcanist memorization spellbook | `ab76417567444a6cb87d9d53e9752955` |
| Full arcane reservoir resource | `3b775ee982444493b3de8f7bc31bd872` |
| Arcane reservoir feature | `06427aa76d584db6915900623575439e` |
| Magical Supremacy feature | `2d86a417ab1542f98a8444b2b97d4951` |

The resolver must obtain these objects from CotW's live public/static Arcanist
fields, cross-check their GUIDs and graph relationships, and independently
inspect the resolved progression. A familiar GUID alone is insufficient.

## Reflection contract

The inspected `CallOfTheWild.Arcanist` type exposes the required static fields:

`arcanist_class`, `arcanist_progression`, `arcanist_spellbook`,
`memorization_spellbook`, `arcane_reservoir_resource`, `arcane_reservoir`,
`arcane_exploits`, and `magical_supremacy`.

The safe lifecycle seam is the static zero-argument `createArcanistClass()`
method. Its decoded call graph creates the class, spellbooks, progression,
registers the class, creates all CotW Arcanist archetypes, assigns the final
archetype array, and completes related registrations before returning. Brown-Fur
therefore uses a reflection-resolved postfix, plus one bounded first-update
fallback, and never polls every frame.

The inspected `CallOfTheWild.SharedSpells` bridge exposes exactly:

- `static bool canShareSpell(Kingmaker.UnitLogic.Abilities.AbilityData)`
- `static bool isValidShareSpellTarget(Kingmaker.EntitySystem.Entities.UnitEntityData, Kingmaker.UnitLogic.UnitDescriptor)`

Changed, missing, overloaded, or otherwise ambiguous signatures block Brown-Fur
publication. Any reuse remains cast-scoped and Brown-Fur-owner-scoped; it does
not install a global replacement for Kingmaker targeting.

## Cast-engine and Harmony contract

The guarded, save-free `observe-brown-fur-cast-engine-contract` scenario passed
on commit `91bcdb07d611419d3e1cc93fb5769782340c4ec4`. Its structured contract
artifact has SHA-256
`846C8E9A4C953D75E0F391B54E6272E427A5B44AD5D85F6CD40C29003B28498D`.
The package SHA-256 was
`A97B8BFCACB3F5E279E999D4B06E8BFB43A28107EE74667165F817E975CAEBC6`;
the DLL SHA-256 was
`A7DC5BB37E6ED335AFF53EDDD4A35B1DCF23E6C433004457AAB9181A927D40C3`;
the DLL MVID was `5318a1c4-83a7-4a9e-a801-4970fe505392`, and the installed
DLL hash matched.

The exact native surfaces include:

- `UnitUseAbility(AbilityData, TargetWrapper)` and the command-type overload,
  followed by `OnAction()` and `OnEnded(bool)`;
- `RuleCastSpell.OnTrigger(RulebookEventContext)`, with exact `Spell`,
  `SpellTarget`, `Context`, `ExecutionProcess`, and `Success` state;
- `AbilityData.Blueprint`, `Spellbook`, `ConvertedFrom`, `SpellLevel`,
  item-source properties, and `SpendFromSpellbook()`;
- `Spellbook.CanSpend(AbilityData, bool)`, `Spend(AbilityData, bool)`, and
  `SpendInternal(BlueprintAbility, AbilityData, bool, bool)`;
- `AbilityExecutionContext.Ability`, `Params`, and `ParentContext`; and
- mutable per-execution `AbilityParams.Metamagic`, `CasterLevel`, and
  `SpellLevel` state.

Descriptor-preserving Powerful Change can compose at the exact
`ModifiableValue.AddModifier(Modifier)` surface. CotW already owns a priority
400 prefix there through
`SpellManipulationMechanics.ModifiableValue_AddModifier_Patch`. Brown-Fur must
declare deterministic ordering relative to CotW and modify the existing
`Modifier` value without replacing its source fact or descriptor.

CotW also owns priority 400 patches at these relevant seams:

- `AbilityData.CanTarget(TargetWrapper)` prefix;
- `AbilityData.TargetAnchor` prefix;
- `AbilityData.ActionType` postfix;
- `ContextDurationValue.Calculate(MechanicsContext)` postfix;
- `AbilityEffectRunAction.Apply(AbilityExecutionContext, TargetWrapper)`
  prefix and postfix; and
- `RuleCastSpell.OnTrigger(RulebookEventContext)` postfix.

No installed Harmony patch has a declaring type named `SharedSpells`.
`SharedSpells` exposes ordinary static helpers and blueprint construction/fixup
methods instead. The live registry contained 41 relevant CotW cast,
metamagic, modifier, targeting, and duration patches. Consequently Brown-Fur
cannot assume that invoking a Shared Spells helper installs an isolated cast
path; the two helper bodies and their blueprint fixups must be decoded before
reuse is authorized.

The modifier-provenance extension of this scenario passed all 11 assertions on
commit `2c18c84d44be6907d3d30dbdd5a42f7d8a1bcef1`. The exact local-runtime
package SHA-256 was
`760920EA11FB67AA7BBCFF5215738D4C76042C3B590E669F0C9F4075A686E1AE`;
the built and installed DLL SHA-256 was
`E34230DC29C6C68E1A4268635D10CD5DBFEC930A012C9C3AE2121922C7282805`;
the DLL MVID was `d4158a13-01bd-43ff-97c5-a14048eba42b`; and the extended
contract artifact SHA-256 was
`E7DFB8D1CD4078AD199BF4E64929DB6F990BADC5DD577552DD6328AFEB11DD7B`.

That live extension proves `ModifiableValue.Modifier` exposes independent
`ModValue`, `ModDescriptor`, `AppliedTo`, `Source`, and `SourceComponent`
fields; the destination `ModifiableValue` exposes its `Type` and `Owner`; the
source `Fact` exposes its blueprint and `MaybeContext`; and a source `Buff`
exposes its blueprint, spell provenance, and mechanics context. It also resolves
all six carrier families present in the installed inventory:

- `AddStatBonus`;
- `AddContextStatBonus`;
- `AddGenericStatBonus`;
- `AddStatBonusAbilityValue`;
- `Polymorph`; and
- `ChangeUnitSize`.

The authorized generic Powerful Change strategy may therefore change only the
positive `ModValue` during the matching execution while preserving its original
descriptor and source identity. It must cross-check the destination ability
stat and source buff/context against the immutable cast intent. This structural
result does not by itself qualify stacking, recast, dispel, persistence, or the
polymorph/size adapter behavior; those remain mechanical runtime gates.

## Progression contract

The actual exploit-bearing `LevelEntry` objects are authoritative. The settings
file is fingerprint evidence only.

| Shape | Resolved exploit levels | Replaced opportunities |
| --- | --- | --- |
| Normal | `1,3,5,7,9,11,13,15,17,19` | `3,9` |
| Balance fixes | `1,4,7,10,13,16,19` | `4,10` |

The pure progression policy rejects null, empty, duplicate, unordered,
out-of-range, partial, and unknown schedules. An unknown future CotW schedule
blocks Brown-Fur only; unrelated package modules remain available.

## Fail-closed checks

Publication requires one active CotW mod entry and assembly, resolved Arcanist
class and progression, both spellbooks, reservoir, exploit selection, Magical
Supremacy, exact Shared Spells signatures, a readable current archetype array,
a recognized exploit schedule, and a nonempty structurally inventoried
Transmutation spell set.

CotW absence produces `Unavailable` rather than a package bootstrap failure.
Any installed-but-failed check produces `Blocked` with the exact failed check.
The reflection-only runtime resolver and isolated lifecycle coordinator are now
implemented and qualified against the installed fingerprint above. They do not
register or publish Brown-Fur blueprints yet. The publication transaction and
spell mechanics remain pending; this document does not claim player-facing
Brown-Fur compatibility.
