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

That body-level investigation completed on commit
`53306fb3367a94fdf2d60b535c7bb5fd72a678ea`. The guarded observer passed all
12 assertions. The built and installed DLL SHA-256 was
`C5D1304E5ACC30B1084F1F1475EABE86866C0D41E11EA32BADF59C7E8BB03A4C`, its
MVID was `63f88725-e29e-47eb-a8c5-32f8f4ae93ab`, and the structured contract
artifact SHA-256 was
`52A37B0A67E4B3DB22AA04F58804535B38BD65ED22964783BC002B1A689E49B0`.

The exact installed `canShareSpell` body first rejects abilities bearing
`CallOfTheWild.SharedSpells+CannotBeShared`, then requires a genuine spell,
then requires either CotW's `ac_share_spell` or `share_spells_feat`, and
finally requires a non-null spellbook. The exact installed
`isValidShareSpellTarget` body accepts self, accepts only the caster's pet for
`ac_share_spell`, or requires both the caster's `share_spells_feat` and the
target's `bonded_mind_feat`.

Those semantics are intentionally narrower than Brown-Fur's authorized
willing-creature set and are coupled to unrelated CotW facts. Invoking them
would reject party allies, controlled creatures, summons, and friendly allies
that Brown-Fur must support. The helper type and signatures remain required
compatibility evidence, but the helpers are not an authorized implementation
of Share Transmutation. Brown-Fur therefore requires its own owner-scoped,
execution-scoped target/range adapter ordered deterministically after CotW;
it must reuse native cast delivery without granting or consulting CotW's
animal-companion or Bonded Mind features.

The targeting-prefix extension passed all 13 guarded assertions on commit
`3f85642168d47998186860e36ac9ccff8d8de0fe`. The built and installed DLL
SHA-256 was
`868326E4CF6812BD982D4FC4CB45AC8D589B2A86A9120F1BB03DEB0FC44F9C86`, its
MVID was `4facb5c3-ec6c-49e2-bef8-5fcead591f65`, and the structured contract
artifact SHA-256 was
`563B00E7AD007CA9A7ED6E0672C4BA6F99C19F15DBC899827947945F6C2D4352`.

CotW's priority-400 `AbilityData.TargetAnchor` prefix always skips the native
getter. For a Personal-range spell it returns a unit anchor only when
`AlchemistInfusion` or CotW's `canShareSpell` succeeds. CotW's priority-400
`AbilityData.CanTarget` prefix likewise always skips the native method; after
ordinary target checkers and self-only/metamagic gates, a Personal spell with
a unit anchor delegates the final result to CotW's
`isValidShareSpellTarget` helper.

An after-CotW prefix is not a safe composition point because CotW has already
requested the original method be skipped. Brown-Fur must use exact postfixes
that replace only the result for an already validated, exact `AbilityData`,
caster, and target scope. An unmatched query must retain CotW's result. The
native approach-distance and command-delivery bodies still require inspection
before Touch and exact-30-foot behavior can be authorized.

That native-delivery inspection first ran on commit
`f12c33c9e5a13fb57f5505f27dfe6755b8104203`. It captured all 166 requested IL
instructions but correctly reported FAIL because the assertion expected two
inherited getters to be declared on `UnitUseAbility`; the engine declares them
on `UnitCommand`. No mechanical ambiguity or mutation occurred. After fixing
only that evidence expectation, commit
`29071bdbd059d09455b8d507eb8edf06d9ee6019` passed all 14 guarded assertions.
The built and installed DLL SHA-256 was
`3D22B020BE43A1A85998BC8468DD2B5B177BA39BDC5E87C88E1C4C7CAF5A3B92`, its
MVID was `73887b8d-4ec4-48ba-bde1-5035d5992049`, and the structured contract
artifact SHA-256 was
`B9310BC962EF8255A335D41F4A654E8F47E2E896C207E9250868508E116B7B41`.

`AbilityData.GetApproachDistance(UnitEntityData)` calculates the target and
caster corporeal radii first. A Personal spell without the native Alchemist
Infusion exception then falls through to `BlueprintAbility.GetRange(false)`
and adds those radii. Personal range therefore supplies zero spell range plus
the native contact radii: the correct Touch approach boundary. The inherited
`UnitCommand.ShouldUnitApproach` decision remains true until
`IsUnitEnoughClose`, and `ApproachRadius` returns the radius captured by the
command.

The authorized Share adapter can preserve the native Touch result unchanged.
For the level-20 form it may replace only the exact scoped
`GetApproachDistance` result with 30 feet in meters plus the same native
corporeal radii, while the independent target policy enforces the exact
30-foot willingness boundary before commitment. Runtime qualification must
still prove boundary/over-boundary command behavior and effect delivery.

The first scoped targeting fixture passed all five guarded assertions on
commit `8f649db44b6a4c4e5cc980df08375f19085a2f0b`. The built and installed DLL
SHA-256 was
`5FD961F69B6BBC71EBB12DE3CC5A3B5198A417F287341C8F16C9A08DAC9D7396`, its
MVID was `bdb039a8-9898-4f66-85c0-34bbc150f446`, and the structured Share
artifact SHA-256 was
`AC729B058D363381F83495A2824AB0C5D913C6D7E2E4C8B521F21417D0AA23CB`.

With installed Personal Transmutation
`3481906baed9487e8403e91a2e9d010a`, CotW's unmatched baseline was
`Owner`, `CanTarget=false`, and a 1.0-meter native contact radius. An exact
Touch scope changed only the computed anchor/result to `Unit/true`, explicitly
rejected a different target, and retained the 1.0-meter native radius. Release
restored `Owner/false`. An exact capstone scope retained `Unit/true` and
increased the approach distance from 1.0 to 10.144 meters, an exact 9.144-meter
(30-foot) delta.

The spell remained `Personal` with the same self/friend/enemy/point flags,
active scopes returned to zero, and all three disposable units were removed.
This qualifies CotW patch composition, exact identity isolation, Touch range,
the capstone approach-distance delta, and cleanup. It does not yet prove a
completed spell effect on the selected ally, real unwilling/faction
classification, command movement at the boundary, reservoir/slot commitment,
or interruption cleanup.

## Native cast commitment order

The refined save-free cast observer passed all 15 assertions on commit
`38779fb6c5671d5bac7af5536b6f3a80c9d8a2a7`. The exact local-runtime package
SHA-256 was
`223412C6BEA4827B66F11E29B7D5DAFDA8A09E2E949FA1F892C6FF1239CEE150`;
the built, deployed, and loaded DLL SHA-256 was
`3888DF6D125F3E5407F974630C78A8BEA850A9AE37DD1E06E702BCB3346D26B2`;
the DLL MVID was `2278bf2f-1a86-4201-a2af-33f66b820fce`; and the structured
contract artifact SHA-256 was
`12BA587CB4CF514E737FAA1B3BE39DD3E5BD2E9718121E059D5FEAA3D1341F06`.

The decoded native order is exact:

1. `UnitUseAbility.OnAction()` rejects unavailable, dead, out-of-contract,
   failed-concentration, and empty charged-item cases before constructing the
   cast rule.
2. `RuleCastSpell(AbilityData, TargetWrapper)` stores the exact spell and
   target and immediately calls `AbilityData.CreateExecutionContext(target)`.
3. `RuleCastSpell.OnTrigger(...)` performs UMD, ordinary spell-failure, and
   arcane-spell-failure checks. Only its successful branch sets `Success=true`,
   calls `AbilityData.Cast(Context)`, and retains the returned execution
   process.
4. After the rule returns, `UnitUseAbility.OnAction()` retains that process and
   calls `AbilityData.Spend()` unless UMD failed. This native spend therefore
   also occurs for ordinary spell failure and arcane spell failure.
5. `AbilityData.Spend()` consumes the material component, item charge,
   spellbook slot, and any ability resource in that order.
6. Only after `Spend()` returns does the command inspect `RuleCastSpell.Success`
   and return failure or continue the cast presentation.

`AbilityData.Cast(Context)` only rejects a variant-wrapper blueprint and then
submits the already-created exact context to the native ability execution
controller. `AbilityExecutionContext` clones the supplied `AbilityParams`, so
Transmutation Supremacy must enter the exact execution-context construction
path before that clone; mutating a later shared `AbilityData` or blueprint is
neither necessary nor safe.

These bodies rule out debiting the reservoir when a toggle is merely armed and
rule out treating the `AbilityData.Spend()` prefix as a sufficient rejection
point: successful execution has already been submitted by then. The authorized
transaction design must validate and reserve the complete requested cost at
the concrete `RuleCastSpell`/execution-context boundary, suppress both rule
execution and the immediately following native spend for a rejected
transaction, and commit the reservation exactly once in the post-rule,
pre-`AbilityData.Spend()` window that the native command exposes. A retained
scope must then follow the exact execution process through completion or
interruption. The remaining investigation is the exact
`CreateExecutionContext` parameter path and execution-process cleanup
callbacks; player-facing wiring remains disabled until those are qualified.

That remaining lifecycle inspection passed all 16 guarded assertions on the
refined commit `475394e2216af64b547ebe1f79ed75e40abb61b4`. The local-runtime
package SHA-256 was
`227E8A7D63166D2DD2572BB36D0369520B70F40C656C0E236E087C0CA5E39749`;
the built, deployed, and loaded DLL SHA-256 was
`2855B8C7DC498993F5A4D2796B77F1D17EF60EC417FB809447F550CFCFE8394C`;
the MVID was `7d03eee7-c922-43e3-8d47-0a7219f3b1a9`; and the structured
contract artifact SHA-256 was
`E0567C932A582220905C0C043D49213B469D76F2C4C879F9C133B330AEA12813`.

`AbilityData.CalculateParams()` first handles item and fact overrides, then
otherwise triggers the native `RuleCalculateAbilityParams` and returns its
result. `CreateExecutionContext(target)` passes that result directly to the
context constructor, which clones it. An exact postfix on context creation can
therefore add Extend to only the new context's cloned `AbilityParams`, after
native prepared/metamixing calculation and command action-cost selection but
before any duration calculation or effect execution. It need not and must not
alter the shared `AbilityData` or `BlueprintAbility`.

`AbilityExecutionController.Execute(context)` creates one
`AbilityExecutionProcess` holding that exact context and adds it to the
controller. Each process owns its own iterator. `Tick()` sets `IsEnded=true`
when the iterator completes and also sets it on a caught execution exception;
on either terminal path it invokes `AbilityCustomLogic.Cleanup(context)` for
the ability components. `Detach()` removes only that exact process through the
controller. Separately, `UnitUseAbility.OnEnded(bool)` always reaches the
command-end postfix and exposes interruption/cancellation even when no process
was created.

The runtime transaction may consequently use reference-identity maps rather
than a global current-cast variable: command/rule/context while validation and
commit are pending, then the exact execution process while effects run. It can
release uncommitted reservations from command end, retain committed effect
scopes until the process becomes terminal, and run one idempotent release from
the `Tick` terminal postfix. Load/combat-transition clearing remains an
independent bounded safety net. This evidence closes the event-order
investigation; actual resource-backed transaction wiring and mechanical
qualification remain pending, so Brown-Fur stays unpublished.

## Scoped Transmutation Supremacy context

The context-local Transmutation Supremacy fixture passed all five guarded
assertions on commit `49d785c43b2c389d63e3a5abebd49c9288191943`.
The exact local-runtime package SHA-256 was
`E091734419E34B761312BACF076A04D1264639DBF6F4E7D11EE77DC2CA7504DD`;
the built, deployed, and loaded DLL SHA-256 was
`2A5DE4EFA1661156B89C98B030E9F7BC5C73B1D00E68F8D553AE8070A041ABAF`;
the DLL MVID was `beb5921b-84f9-4d60-a787-ce93a80f2a36`; and the
structured result SHA-256 was
`E4DC08A771B656DEB1A805AFD1684282421D8C9F9BC9AC4A101A19BD61FEEE0A`.

The fixture used the installed Personal-range Transmutation spell
`3481906baed9487e8403e91a2e9d010a`. Its ordinary execution context contained
no metamagic. One exact retained scope added native `Extend` to one newly
created context, rejected duplicate retention, and left a different subsequent
context ordinary after release. An already-Extended context remained Extended
without recording another modification. The shared blueprint range,
metamagic-support mask, and fixture spell level were unchanged; the tracker
returned to zero active scopes and the disposable caster was removed.

This qualifies reference-isolated, non-stacking context mutation and proves
that the runtime patch does not mutate shared spell data. It does not yet prove
timed-duration doubling, prepared-spell slot preservation, casting-time
preservation, instantaneous or permanent duration handling, or integration
with a real Brown-Fur owner and reservoir transaction. Those remain
player-publication gates.

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

The guarded `disposable-brown-fur-bonus-carriers` fixture then passed all seven
assertions on commit
`7008c0e1067f47c713612c3d751f7519a9a0c62d`. The loaded DLL SHA-256 was
`846BAE2B12A6A536B55273BD2E7B1981993A0E5C4C009815804A9CC679F00925`
and its MVID was `5e81a5db-70ae-4ed8-9436-5681772bd71d`. The structured
carrier artifact SHA-256 was
`C0CFF16F1E1C82857C28CA964F5F574B22B8ED8517A1A0660AE1923A3339D254`.

The save-free fixture applied five real installed spell buffs to disposable
engine units and covered all six carrier families (the Enlarge Person case
combines `AddGenericStatBonus` and `ChangeUnitSize`). It observed:

- `AddStatBonus`: Strength `+4 Enhancement`;
- `AddContextStatBonus`: Constitution `+2 Enhancement`;
- `AddStatBonusAbilityValue`: Dexterity `+6 Enhancement`;
- `Polymorph`: Strength `+2 Polymorph`; and
- `AddGenericStatBonus` with `ChangeUnitSize`: Strength `+2 Size`.

Every case produced exactly one modifier owned by the applied buff, retained
the expected destination stat and nonempty native source-component identity,
and carried the exact caster, source spell, target, and caster level through
the buff's child `MechanicsContext`. Removing each buff removed its modifier
and restored the original stat value; both disposable units were removed from
the live registry and disposed.

Kingmaker does not retain the caller-created `MechanicsContext` by reference:
the applied buff receives a child context whose `ParentContext` is the caller
context. `Buff.IsFromSpell` was true for the three direct carriers but false for
the polymorph and size cases even though their child contexts retained the
exact source spell. Powerful Change eligibility must therefore be decided at
the genuine cast boundary and correlated through context/caster/spell/source
identity; `Buff.IsFromSpell` alone is not an authorized eligibility gate.

This checkpoint proves native carrier registration, descriptor provenance,
and cleanup. It still does not qualify the Brown-Fur value adjustment itself,
descriptor competition, recast, dispel, or save/reload persistence.

The execution-scoped adjustment checkpoint passed all seven guarded assertions
on commit `2a44f651d1fe6ca94bcae9cb7a9b945598bdbe04`. The loaded DLL SHA-256 was
`C151FB1B7D91DAE611A3DB8A898AAC40D62B5E1CAC850C8695212C26C0BA108B`,
its MVID was `c86e3514-d70f-42cd-bd79-59644251e727`, and the structured carrier
artifact SHA-256 was
`FE4CC3DC499BAC2942B91442A34A68330E553CD24A22C2B24F1CB68861227165`.

The exact native modifier transitions were:

- static Strength `4 -> 6 Enhancement`;
- context Constitution `2 -> 4 Enhancement`;
- ability-value Dexterity `6 -> 8 Enhancement`;
- polymorph Strength `2 -> 4 Polymorph`; and
- size Strength `2 -> 4 Size`.

Each immutable transaction scope was correlated through the root/child
mechanics-context chain, exact caster, exact source spell, exact applied-buff
GUID, selected stat, and carrier family. Each case adjusted exactly one
modifier. A second scope deliberately selected Charisma for the non-Charisma
carrier and made zero adjustments. After release, the same buff registered its
ordinary value again, proving no scope leakage. Descriptor, source fact,
source component, destination stat, removal, and baseline restoration remained
native throughout.

The patch is inert when no committed scope exists and is ordered after CotW at
`ModifiableValue.AddModifier(Modifier)`. It catches its own exceptions so the
optional extension cannot prevent native modifier registration. Remaining
Powerful Change runtime gates include descriptor competition and suppression,
ordinary/enhanced recast in both directions, dispel, level-20 `+4`, full
spell-slot/reservoir commit integration, cancellation/interruption, and
save/reload persistence.

The advanced stacking/recast/capstone extension passed all eight assertions on
commit `bc8f30c439c53d0ff4e00ae5bb39c1fef7608c71`. The loaded DLL SHA-256 was
`A7CCD03A852057CD91F943D5C90226880D742F9D8857CD1BEF53FCF3B62E8141`,
its MVID was `80645298-bfcc-47f9-b7bc-8488df7b9bd0`, and the structured
carrier artifact SHA-256 was
`6E8FC8618D177BDDD4CC294F492BB1013B4F47446E3F17122B0AA912A6F3DBEA`.

Against a baseline Strength of 10, weaker, equal, and stronger Enhancement
competitors resolved to 16, 16, and 20 respectively while the Brown-Fur source
modifier remained exactly `+6 Enhancement` in all three cases. Thus the
engine's native descriptor suppression, rather than an extra untyped bonus,
decided the effective stat.

Ordinary-to-enhanced recast left exactly one Bull's Strength buff at Strength
16. Releasing the transaction while that buff remained active retained the
enhanced value. Enhanced-to-ordinary recast then left exactly one ordinary
buff at Strength 14. The level-20 form registered exactly `+8 Enhancement`.
All buffs, disposable competitor facts, scopes, and modifiers cleaned up and
returned Strength to baseline.

Descriptor competition, both recast directions, post-transaction enhanced-buff
retention, and the `+4` capstone are therefore qualified for this installed
carrier contract. Remaining Powerful Change gates are native dispel,
save/reload persistence, full spell-slot/reservoir commit integration, and
cancellation/interruption at the real cast boundary.

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
