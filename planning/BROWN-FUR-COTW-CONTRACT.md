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

The relationship-contract extension of the same guarded observer passed on
commit `efa7f578de31bf7d86bab17fac89296ced0e203e`, run
`20260815T2040197380348Z-9ce88ca5e9fd4abfbcacb64c31fd18c7`. It resolved
native `Player.Party`, `UnitDescriptor.Pet`, `UnitEntityData.Faction`,
`IsDirectlyControllable`, bilateral `IsEnemy` and `CanAttack`, and
`UnitPartSummonedMonster.Summoner` / `IsDirectlyControllable` surfaces. The
installed game did not resolve the probed
`Kingmaker.UnitLogic.Parts.UnitPartPet` name, so the production contract uses
the native owner-side `UnitDescriptor.Pet` reference and never guesses a pet
part type.

Exact immutable evidence identity:

- local-runtime package SHA-256
  `BE86D45FAE6112A2FA1A94E9FC3E7A954597AC03F139783702F4594F3F573A52`;
- built, deployed, installed, and loaded DLL SHA-256
  `D180CCD664ACE4C4DC17066B6A64F1129E5F70CF2829991E9CBA9998BA0BB0F2`;
- DLL MVID `e9462b70-53e6-424a-9bab-686c225af985`;
- structured contract SHA-256
  `497DB90BF6F3E13205679E6D4309E3BF45B0ADC2E8FD2A904A2D1919E619CCEE`;
- runtime-result SHA-256
  `BDDEDF1B93CE0D076CEE5B352F6C3AE1C9BC07A0C1D3566B7C8101889034BED0`;
- runtime-evidence SHA-256
  `529063D01591A05CCCBC161DB6B0B7A605A3140812777A6786C277D8AA8D8E96`;
- orchestration SHA-256
  `39530D44994E49D7C151D12E6F57008D2DBA8F0D463D52BCCEF20605010716BD`;
- deployment `20260815T2040196438588Z`, deployment-manifest SHA-256
  `D00E5366FF3D239D794B2B26FE3870287964F335C9607CCA66A84A074381C092`;
  and
- backup `20260815T2040150546866Z`.

The production relationship policy now grants willingness from positive
self, party, controlled-companion, owner-side animal-companion, or controlled
summon facts. A non-controlled creature qualifies as a friendly ally only
when both faction identities are known and identical and neither side can
attack the other. Enemy contradictions, attackable friends, hostile neutrals,
unknown factions, and non-attackable creatures without positive allied proof
all reject. Runtime fixture coverage of each native relationship category is
still required before selector publication.

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

## Exact reservoir accounting adapter

The real CotW reservoir fixture passed all seven guarded assertions on commit
`1c1fb1e3d4bf45d22437ab0547128c10f73edcf5`. The exact local-runtime package
SHA-256 was
`D03C4358944271EFA83E1C56D7AA291C5992FC8B448A12587BA2B3538994CECD`;
the built, deployed, and loaded DLL SHA-256 was
`D444505407AFF46FB2BAAFECBF6BAAEADCC4FF98477AA880C333F0042243B953`;
the DLL MVID was `be9b3975-347f-464c-91fc-a0fc9dce65b0`; the structured
reservoir artifact SHA-256 was
`ED49E073E31CFE83651B621F5CFBABF6CDC87842DD04E18830468934AF713110`;
and the runtime-result SHA-256 was
`6127CAD544D1B9DFA6654CF30C35D521CBC24D53D635310CF925834BB7EDEF2A`.
The guarded Steam App ID 640820 run used deployment identity
`20260815T1707489383628Z`, backup identity `20260815T1707446061304Z`, and
preserved the feature-module settings whose SHA-256 was
`5B6030AE888F6B127FF23CA03E49578F304DC107ED65E3B2D5F8C8D3D177665E`.

The disposable owner began without the resolved CotW resource
`3b775ee982444493b3de8f7bc31bd872`. Native resource registration initialized
three points. One call to the exact adapter for the combined cost changed the
amount from `3` to `1`; exact restoration returned it to `3`. Reducing the
balance to one caused a two-point request to reject with
`reservoir-insufficient` and leave the balance at one. Removing ownership
caused a one-point request to reject with `reservoir-not-owned`. The fixture
removed both the resource and disposable unit in its bounded cleanup.

This qualifies exact native debit, pre-debit insufficiency and ownership
rejection, restoration, and cleanup against the installed CotW resource. It
did not by itself qualify attachment of that adapter to the cast lifecycle;
the production-boundary checkpoint below supplies that narrower evidence.

## Production cast transaction boundary

The guarded `disposable-brown-fur-cast-execution` fixture passed all eight
assertions on commit `4d5f599457a1b5eb3d012dbbcfc1fc89344585f0`.
The exact local-runtime package SHA-256 was
`53CFD9B94040036F0566DFFCC87C646F7D41C1E0C5654F9607CB40F2815ACB81`;
the built, deployed, and loaded DLL SHA-256 was
`6A82FDE7718F1C168A85197AFAB5DEE13C59A5F07789CD6FFA26CBEE10C0A39A`;
the DLL MVID was `520c8253-5184-4511-9263-82759db39167`; the structured
cast artifact SHA-256 was
`2C14A55B7A33AEC5CBD2972F79DF37F397E611E6AABF4A59AC60FC94F0D07882`;
and the runtime-result SHA-256 was
`45A3532B1285F886B6511DBEA65A86C3CB10D96150C477CDF43C07288BF8DA29`.
The guarded Steam App ID 640820 run used deployment identity
`20260815T1738358154697Z`, backup identity `20260815T1738316601697Z`, and
preserved feature-module settings SHA-256
`5B6030AE888F6B127FF23CA03E49578F304DC107ED65E3B2D5F8C8D3D177665E`.
The harness recorded game build `2018.4.10.10503941`.

The live Harmony registry placed the Brown-Fur `RuleCastSpell.OnTrigger`
prefix at priority 400 with `after=CallOfTheWild`. An already validated
combined intent retained one transaction, one two-point reservation, and one
Share and Supremacy scope before construction of the exact rule context. The
installed Personal Transmutation context gained native Extend. Commitment
then changed the real CotW reservoir from `3` to `1` exactly once and retained
one modifier-adjustment scope. The simulated rule-exception path marked the
transaction `Failed`, restored the reservoir exactly to `3`, and released all
owned state.

A second transaction reserved successfully and then encountered a balance
reduced to one before commitment. It rejected with no partial debit, prevented
native rule execution, and armed one exact `AbilityData.Spend` suppression;
the live Harmony prefix consumed that suppression once (`1 -> 0`). A third
combined request at one point was rejected before retaining a cast surface.
Final transaction, reservation, Share, Supremacy, modifier, resource, and unit
cleanup all passed with no internal failure diagnostic.

This qualifies the production command/ability/rule/context boundary, combined
two-point reservation and debit, after-CotW ordering, exception restoration,
post-reservation insufficiency, one-shot pre-slot suppression, and bounded
cleanup. The seam remains deliberately inert until player intent discovery
supplies a validated transaction. It does not yet prove a submitted native
spellbook cast and its real spell-slot delta, an applied shared effect on an
ally, process-driven completion/interruption, a real Brown-Fur owner, or
save/reload persistence. Those remain player-publication gates.

## Native CotW Arcanist spell-slot boundary

The guarded `disposable-brown-fur-arcanist-slot` fixture passed all six
assertions on commit `0fa4e97f89eb16e594e1475caff8b517d6c187e0`. The exact
local-runtime package SHA-256 was
`D40AC5B20432FD40AAE04DFE812079F1151D066D72C12B7AF61921ABD7A36DA8`;
the built, deployed, loaded, and installed DLL SHA-256 was
`4BD0988DD191F77F6D0A4374C71DCD4EF0507C9FD356D263CDBA5E484EE947C4`;
and the DLL MVID was `684c249a-02a8-4da0-afbc-9a4795848305`. The structured
slot artifact SHA-256 was
`E867EEAE96F3C6632C640910C2DB2A12C9D913B54894E7D137A569EACE9E3C3A`
and the runtime-result SHA-256 was
`478C1D94C6019000E95D60C173AF8734C62E97006500CBA8867CF5BC2D9D9A50`.
The guarded Steam App ID 640820 run used deployment identity
`20260815T1813160355952Z`, backup identity `20260815T1813116893707Z`, and
preserved feature-module settings SHA-256
`5B6030AE888F6B127FF23CA03E49578F304DC107ED65E3B2D5F8C8D3D177665E`.
The harness recorded game build `2018.4.10.10503941` and performed no save
interaction.

A native disposable level-five CotW Arcanist acquired both structurally
resolved spellbooks: casting `0c21cfcab6ce4395bd4df330ab3cf715` and
preparation `ab76417567444a6cb87d9d53e9752955`. The cast used the exact
level-three Beast Shape I spell-list wrapper
`61a7ed778dd93f344a5dacdbad324cc9` and its wolf variant
`3481906baed9487e8403e91a2e9d010a`; the resulting `AbilityData` retained the
casting spellbook as its source and the canonical parent remained spendable.

The combined transaction began and committed once. The real CotW reservoir
changed `4 -> 2` and native `AbilityData.Spend` changed available level-three
slots `6 -> 5`. The injected rule-failure path restored the reservoir exactly
to `4`. In the post-reservation shortage case, the one-point balance remained
`1`, commitment was rejected, the one-shot spend suppression changed `1 -> 0`,
and the real level-three slots remained `6 -> 6`. All transaction,
reservation, Share, Supremacy, modifier, suppression, resource, and disposable
unit state cleaned up.

This closes native CotW Arcanist source-spellbook qualification, canonical
wrapper/selected-variant correlation, combined two-point debit with one real
spell-slot spend, and pre-slot rejection against a real spellbook. It does not
yet prove player intent discovery, ordinary process completion/interruption,
ally effect delivery, a real Brown-Fur feature owner, or save/reload
persistence. Brown-Fur remains reserved and unpublished.

## Native combined cast and ally-effect boundary

The guarded `disposable-brown-fur-native-cast` working-save fixture passed all
six assertions on commit `442963a26108fa41a89accabcc7c3209373aceae`.
The exact local-runtime package SHA-256 was
`101AED6D623283B3706738CF1B06B1D8ABE40A1E368E19C7952A4214BDBA3168`;
the built, deployed, loaded, and installed DLL SHA-256 was
`D43DB5DEA7612D6E840694B100730D2181CABBE68EEBE90A6EC1853C4F1A7FD0`;
and the DLL MVID was `c2463116-f5ef-4ed0-836c-977ac1e81b20`. The structured
native-cast artifact SHA-256 was
`DFC3A0BA42B7323D976743A813A2E0FECD27D7E2AFB5F234943432495D3A4516`,
the runtime-result SHA-256 was
`0374EC89FF3BEA2A2770C083B34C755152AC9EF867D3AF9B1E3CB70FB5469AE3`,
and the curated runtime-evidence SHA-256 was
`315D0B6F71D3C0FD3BFEEF270233F673DD188401699C878015067222A05FD7CA`.
The guarded Steam App ID 640820 run used deployment identity
`20260815T1838157414656Z`, backup identity `20260815T1838112725502Z`, and
preserved feature-module settings SHA-256
`5B6030AE888F6B127FF23CA03E49578F304DC107ED65E3B2D5F8C8D3D177665E`.
The harness recorded game build `2018.4.10.10503941`, loaded only the named
`KMG_AUTOMATION_WORKING` save, and observed no save-writing API.

A disposable native level-five CotW Arcanist and disposable ally were spawned
in that exact loaded area. The caster owned the resolved casting and
preparation spellbooks, and the real spellbook-backed `AbilityData` retained
casting book `0c21cfcab6ce4395bd4df330ab3cf715`, Beast Shape I wrapper
`61a7ed778dd93f344a5dacdbad324cc9`, and wolf variant
`3481906baed9487e8403e91a2e9d010a`. The production Brown-Fur transaction
retained combined Powerful Change and Share Transmutation scopes, native
`UnitUseAbility` entered its running state, returned `Success`, constructed an
execution process, reached terminal completion, and marked the transaction
`Completed`.

The real CotW reservoir changed exactly `4 -> 2`, while native spell spending
changed available level-three Arcanist slots exactly `6 -> 5`. The installed
wolf polymorph buff appeared exactly once on the ally and zero times on the
caster. Its one Strength modifier was `+4 Polymorph`, preserving the original
descriptor while applying Powerful Change exactly once. The buff mechanics
context retained the exact caster, ally target, and selected source spell and
had a positive 1,200-second duration. All transaction, reservation, Share,
Supremacy, modifier, suppression, buff, and disposable-unit state cleaned up.

This closes real combined-cast completion, exact two-point/one-slot accounting,
execution-scoped Personal-to-ally delivery, original-descriptor enhancement,
effect provenance, and terminal cleanup for an already validated intent.
Player intent discovery, real Brown-Fur feature ownership, process
interruption after native submission, native dispel, and save/reload
persistence remain publication gates. Brown-Fur remains reserved and
unpublished.

The same working-save fixture was extended with an automatically derived,
pre-commit interruption and passed all seven assertions on commit
`f39c2c0acff97d84b2e2dc7484782f870d90bd10`, run
`20260815T2136354900692Z-4f61a30351c24faca4e7603f096453a3`. It used only
the named `KMG_AUTOMATION_WORKING` save, spawned disposable units, and made no
save write. Evidence-directory identity was
`20260815T2136354654086Z-disposable-brown-fur-native-cast`; deployment
identity was `20260815T2136353877384Z`; backup identity was
`20260815T2136309061847Z`. Exact immutable identities were:

- local-runtime package SHA-256
  `C1C71B81734C2A19ADC36222D56272666231C3989E8F6A4EBA88F791262579EF`;
- built, deployed, installed, and loaded DLL SHA-256
  `16645D25BCFD6AF9FF28624B029D04930BAFED23E035C1D69CAE46D63DAE1170`;
- DLL MVID `06eba2d2-78cc-48d7-896f-eda65bedca7f`;
- structured native-cast artifact SHA-256
  `3D5AE994BAD79CE022FECE4CD85E761B5A2DB5EB2217A7E9033DD70424A0BCC0`;
- runtime-result SHA-256
  `5F0FEBC50EE72505B55387EFF88E2FDE46FFF5157ED639FE25E71A0A376CAC86`;
- runtime-evidence SHA-256
  `B9963CDF1ADD4E6D48BE628208D814D791F73F90F263CEFA6BB43A2EA1E0A2CC`;
- orchestration SHA-256
  `B0472832F5E94AEFF13404C7A96A1E11AA0EC3E3EC60EB4B572237F667A76D82`;
- deployment-manifest SHA-256
  `EC445016A998B378A95102902A0D3EE785B7DD4C30B399727748257E472ED0E7`;
  and
- preserved feature-settings SHA-256
  `5B6030AE888F6B127FF23CA03E49578F304DC107ED65E3B2D5F8C8D3D177665E`.

After re-establishing the request-local known spell and reservoir, the real
three Brown-Fur owner facts plus Strength and Share one-shot markers armed one
automatic combined command. It was targetable, available, entered the native
running state, and cleared both markers. Native `InterruptAll(true)` then
ended it with result `Interrupt` before rule commitment. Reservoir remained
`4 -> 4`, level-three slots remained `6 -> 6`, and transaction, reservation,
Share, Supremacy, and modifier counts all returned to zero. The successful
cast portion of the same run continued to prove `4 -> 2`, `6 -> 5`, ally-only
effect delivery, `+4 Polymorph` Strength, and terminal process cleanup.

This closes cancellation after native command submission but before cast
commitment: no partial or duplicate debit, no slot expenditure, one-shot
cleanup, and bounded release are proven. Native dispel/expiration,
save/reload persistence, module-OFF existing-owner behavior, and selector
publication remain gates. Brown-Fur remains registered but unpublished.

The native lifecycle fixture then passed all nine guarded assertions on exact
commit `334e526fec23c27e6c099fc44b1f797d4e99dece`, run
`20260815T2202189689958Z-d92ec9a042cc477aacd22cf4e4c20b25`.
It was a save-free Steam App ID 640820 launch with evidence-directory identity
`20260815T2202189516902Z-disposable-brown-fur-bonus-carriers`, deployment
identity `20260815T2202188770345Z`, and backup identity
`20260815T2202145823035Z`. Exact immutable identities were:

- local-runtime package SHA-256
  `1F395F19E4C2EDA73087605258BE3935B52EFD945889DAFCF98E829B4724D9B0`;
- built, deployed, installed, and loaded DLL SHA-256
  `FEBBC311E50235E7677E8BABA062DFBE50198304EC86F6918DE98B5DCD9B99EA`;
- DLL MVID `51d34187-ecc7-4ccc-b5c9-7fab29775eca`;
- structured bonus-carrier artifact SHA-256
  `082088E3D518ADB023795A4BE9B93D977F813CD9BF8A19D98386070AE73262F6`;
- runtime-result SHA-256
  `0A0B48C165877203122484589AD6C5857CE70EB5C7C06185EA1FBA3066BA3700`;
- runtime-evidence SHA-256
  `E65F40943E24991C8EECE85F350EB41C18C927697DB1F12CAFEA4CB0B2F157A8`;
- orchestration SHA-256
  `FEEDBE593AB4E5B08902A78F433A209D02D69E0E5FB4BCAE57972E2D6B225471`;
- deployment-manifest SHA-256
  `187EF7D36B3AA61704EFEB3AEC6944B2922F436EF55F804B451D1F895429FE6C`;
- preserved feature-settings SHA-256
  `5B6030AE888F6B127FF23CA03E49578F304DC107ED65E3B2D5F8C8D3D177665E`;
  and
- game build collection `2018.4.10.10503941`.

After the execution-local modifier scope had been released, the fixture
triggered a real targeted `RuleDispelMagic`. Its deterministic caster-level
check recorded roll `17`, caster level `0`, DC `31`, and explicit test bonus
`1000`; the rule returned success, removed the enhanced installed Bull's
Strength buff, and restored the target's Strength to baseline. A separately
enhanced buff was assigned an elapsed absolute `m_EndTime`; the native
`BuffCollection.UpdateNextEvent()` and `Tick()` path observed nonpositive
time remaining, removed the buff, and restored Strength. This proves the
execution-scoped value adjustment does not replace the buff or damage native
dispel and expiration semantics. Final carrier buff count and all modifier
scope state were zero.

This closes the native dispel/expiration gate. Save/reload persistence,
module-OFF existing-owner behavior, dependency profiles, boundary matrix, and
selector publication remain gates. Brown-Fur remains registered but
unpublished.

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

The timed-duration extension passed all seven guarded assertions on corrected
commit `2d91e4898761feaf46731a15c92900c84f40aef6`. Its exact local-runtime
package SHA-256 was
`5594682655E6C24BA076FD5961DDA534930EE98D8B6ED480550FE16BBA15DFF7`;
the built, deployed, and loaded DLL SHA-256 was
`6180E8852FA8684C181D197A433E5D4519A60FB7EB53FF80CFE674EE113086DA`;
the DLL MVID was `7af4aca2-69fe-41b3-a5a0-829aec81c5a8`; and the
structured Supremacy artifact SHA-256 was
`A3FFE59459A8F2A42CD96A48CD88C822224705857F279CD95019C2751AB15D25`.

Against an explicitly initialized native five-round extendable duration, the
ordinary, scoped, already-Extended, and post-release contexts calculated
exactly `5`, `10`, `10`, and `5` rounds. The installed spell's action type
remained `Standard`, so the context-local addition did not alter casting time.
The first version of this fixture failed closed before scope retention because
its synthetic `ContextDurationValue` omitted the required zero dice carrier;
the corrected fixture supplies the complete native duration structure rather
than weakening the assertion.

This closes native timed-duration doubling and action-type preservation for
the installed context contract. Prepared-spell slot preservation, actual
Metamixing input, instantaneous and permanent spells, variants and converted
spells, concurrency at the live context boundary, and real Brown-Fur owner and
transaction integration remain pending.

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

The installed-inventory exceptional-duration investigation first failed closed
on commit `4df227c4eeeecd06f42d2000bf44962cc2a6bc38`: native/CotW duration
calculation left Resonating Word at `3` rounds and Obsidian Flow at `600`
rounds even though their scoped contexts carried `Extend`. Both installed
spells intentionally omit ordinary Extend from their metamagic masks. No
exception occurred, the other seven assertions passed, and all scopes cleaned
up; the failure established that the generic context flag alone was not a
faithful implementation for these two spells.

A post-CotW named adapter for only those two permanent spell GUIDs and their
exact root duration rates then passed all nine assertions on commit
`f6048539d0e609ed4fd0787b3e18e73e478f5746`. Resonating Word
`df7d13c967bce6a40bec3ba7c9f0e64c` calculated `3 -> 6` rounds and
Obsidian Flow `e48638596c955a74c8a32dbc90b518c1` calculated `600 -> 1200`
rounds. The adapter required the exact modified execution-context reference;
the installed Personal-spell blueprint still reported unchanged range,
metamagic support, and spell level, and the tracker returned to zero scopes.
The ordinary five-round, already-Extended, restored, and action-type assertions
also remained `5 -> 10`, `10`, `5`, and `Standard`.

Exact passing evidence identity:

- run `20260815T1900551508660Z-47c66e45d9f047ffb22d05221a14278f`;
- local-runtime package SHA-256
  `4D4E5FCCCCD04F503694FECC3F5B6400D59D903EB7EAC0208F0FAF7302F90FA19`;
- built, deployed, installed, and loaded DLL SHA-256
  `F4FFDDB7101542A98970276C4A4B2CCDF452B8994BB16EA43DCC5F752E8AA11A`;
- DLL MVID `8def1fcf-2c20-44f8-b41f-57511b5cfc91`;
- structured Supremacy artifact SHA-256
  `F1EA4BCC3F3B20FAE0C49F4C64A64F563270FB81E3A308811A38CA139B8F5524`;
- runtime-result SHA-256
  `766E2FEB65631B1F4DAE4E01305FBB3E64231A7975C37757CD5ECE119A2D7119`;
- runtime-evidence SHA-256
  `D11F39871B940A510E9B025395AFB8BC06DDF7BCFA4CA924433BD5B21F838A6B`;
- orchestration SHA-256
  `52E94C550D699FF0177D750E701348FB0604B5C5484D92BAA6C996DFEF2381D0`;
- deployment `20260815T1900550585177Z`, backup
  `20260815T1900507270448Z`, preserved feature-settings SHA-256
  `5B6030AE888F6B127FF23CA03E49578F304DC107ED65E3B2D5F8C8D3D177665E`,
  and game build collection `2018.4.10.10503941`.

The guarded Steam App ID 640820 launch was save-free and recorded no save
interaction. This closes the two exceptional timed carriers in the installed
inventory. Instantaneous/permanent no-op behavior and final inventory
classification remain publication gates.

The blank-duration Earth Tremor wrapper required a second inventory pass. Its
three selected variants each contain an exact one-hour area duration despite
the blank localized spell duration and lack of ordinary Extend support. The
first guarded attempt on `d13099e128dad56c5fd575da54c8b1abb1dafbd5`
failed closed because spread and cone already honored the scoped Extend through
CotW (`600 -> 1200`) and the provisional family adapter doubled them again to
`2400`; line remained `600 -> 1200`. The adapter was therefore narrowed to the
line GUID only.

The corrected fixture passed all ten assertions on
`7f282f91b2fd8859274711e164b5de683ab261fd`. Spread
`c7b52e9a09ef442f9308d9119f5877d2`, cone
`3e4a0790fc2749bbacb1b3b1d2401148`, and line
`91266b6d2a4c4fd6b8e1549bc2381d12` each calculated exactly
`600 -> 1200`; only line used the named post-CotW adapter. Resonating Word,
Obsidian Flow, ordinary timed Extend, already-Extended non-stacking, action
type, blueprint isolation, and zero-scope cleanup remained passing.

Exact corrected evidence identity: run
`20260815T1915449792466Z-e12a8472d5b842df90510c0e6f8d8777`; package
SHA-256 `65C172EF676248BAABEB30BC4686B5F9F9E6237E9D31D6B8972D60F17EA42057`;
DLL SHA-256
`A160E6EB36E5B4A2EE339C17BC4D25ECEA0CC55C7836A11B96D70244B94D4BBD`;
MVID `cbcc46c0-3179-4b08-a4c1-f6c5fb0cf832`; structured artifact
SHA-256 `31A5B4D75F7BF0AE37048EA981903AAFF4DA232AE13775196C891A2F3C7FD36E`;
runtime-result SHA-256
`9891F0E15180C55441195267DB51B6B09A7756F5E2E0E4A5A8F50DFDA262AF7C`;
runtime-evidence SHA-256
`BF751F10471627FDC453B2A192F0FACBFFA9404AB18D0BCFC082BEFB298969FA`;
orchestration SHA-256
`B89360C45EB6EDD9E9812509B89D6472C460EC6180A9D9377BC019199C14BD8C`;
deployment `20260815T1915448909848Z`; backup
`20260815T1915405293055Z`. The guarded launch was save-free.

The final no-Extend scan first produced a useful failed fixture on
`6151242b4dd684eb1ff04546629b9664bb1a37c6`: Jolt, Baleful Polymorph,
Disintegrate, and Stone to Flesh all preserved their zero or absent duration
carriers, but the fixture incorrectly attempted to execute Earth Tremor's
variational parent and Kingmaker rejected it with `Can't cast variational
ability`. The correction treats the parent as the three-variant selector it
is and executes only selected variants (already qualified above).

The corrected immutable candidate
`92aa73a35f218ef6ae994b7015161cbc88c236e1` passed all eleven Supremacy
assertions. Jolt preserved its single zero-round carrier, Baleful Polymorph
preserved its single zero-round carrier and permanent application flag,
Disintegrate preserved all four zero-round carriers, Stone to Flesh preserved
its absence of duration carriers, and the Earth Tremor parent preserved its
three selected variants and absence of a root duration. No spell supported
ordinary Extend, no instantaneous/permanent/selector path acquired a duration,
and all execution scopes returned to zero.

Exact passing evidence identity:

- run `20260815T1929074013106Z-63ebaadda1474cf582eec5c0f489b78e`;
- local-runtime package SHA-256
  `3E87DA19F357109100F236C492B9E556700F6A585E0BD1EDE6C292C7AC33F83E`;
- built, deployed, installed, and loaded DLL SHA-256
  `99AEDDCE47720610D8897B8E449FE2B2DB4FB67C74D93D02F27F107FF2D83A6D`;
- DLL MVID `d64deafc-05a7-47a0-85bd-6a24fbed7044`;
- structured Supremacy artifact SHA-256
  `4745290BF6D90D63376D16E34132AE05DC75C2178F595370B3667A3407F7692F`;
- runtime-result SHA-256
  `3A6002E0247D8BE8645D0F6EF2246482785FEE480BA631E7C5EE7BEF746C71AB`;
- runtime-evidence SHA-256
  `1D303D4034D4234C5E7909A90CBC7FF3C6AC73800F82D54F99BC22B1EEAA8238`;
- orchestration SHA-256
  `777FBEAB1883854F858F4B930CB1DD75E16FF721D658725198E42A4FE2DE2F63`;
- deployment `20260815T1929073015390Z`, backup
  `20260815T1929029719860Z`, preserved feature-settings SHA-256
  `5B6030AE888F6B127FF23CA03E49578F304DC107ED65E3B2D5F8C8D3D177665E`,
  and game build collection `2018.4.10.10503941`.

The guarded Steam App ID 640820 launch was save-free and recorded no save
interaction. This closes the instantaneous, permanent, and selector no-op
Supremacy carrier gate. Deterministic classification of all installed
inventory rows remains required before publication.

The subsequent pure fail-closed classifier passed the complete installed
inventory on `b707fd5c3aa8773fb16ae3a3f968de1ab529b766`: `86` roots, `177`
root/variant records, `100` Personal records, `112` positive bonus candidates,
and zero hard-coded `ToCaster` records. Final qualification counts are `174`
supported by generic contract, `3` supported by a true Brown-Fur named
adapter, and `0` unexplained. The named adapters are Resonating Word, Obsidian
Flow, and Earth Tremor line. Earth Tremor cone and spread are explicitly
classified as installed CotW-native hidden-duration paths.

Exact classified evidence identity: run
`20260815T1951517164731Z-f2988c8a3fac4d60bcbabe3e3432bc0c`; package
SHA-256 `60505729A0846A9641FDC1A5D25FA4E4C93B57814223C8AECB546CB3350BA638`;
DLL and installed DLL SHA-256
`B4B48DAA9C7AE6BD9A963C142D6435DAA3B1B388F0DA0D0B0E34CF0F9694CF91`;
MVID `f041926b-a2c6-4db5-aeae-95ca51ced190`; classified inventory SHA-256
`82AE0FFA8313DA14B173A61430F47308320849436AC51CB001EFBF5CB3238A7D`;
runtime-result SHA-256
`0C2C095001150D6218FB7C8839F59D11D7B2F82F41E3B05041552AD50F0BAAD7`;
runtime-evidence SHA-256
`560DE55E6C7D99C6A7228DBF1C616A02E5D3AB92030D9B354CD6CF514792EC9C`;
orchestration SHA-256
`16098FF1DB97499DB12EB1D9616D588B7B2FB293E23C48111392D4F95D2A2C2C`;
deployment `20260815T1951516057686Z`; backup
`20260815T1951471610464Z`. The guarded launch was save-free and recorded no
save interaction. This closes the installed static inventory classification
gate; it does not publish Brown-Fur.

The first isolated-registration attempt on
`c0fbe1893115fc4fada88f6a14ac07990f1aab43`, run
`20260815T2002264792969Z-a68800ceb56949cf821cbfd3c330ab9d`, failed
closed because `BlueprintManifest.ResolveActive` correctly rejected the first
Brown-Fur identity while its ledger status was still `reserved`. No Brown-Fur
identity was registered, the Arcanist archetype array retained zero Brown-Fur
references, and the ordinary package bootstrap remained ready. This was an
identity-ledger state error, not a CotW structural-contract failure.

After activating the same 19 permanent GUIDs and generalizing the inherited
manifest validators, the exact commit
`dd35e276cac658f3976aaddbcf4f6f61cd7eae26` passed all 13 guarded,
save-free assertions. Run
`20260815T2011411795095Z-fe885f683f564c58994018418f89a2fa` proved:

- the package bootstrap remained ready;
- the installed CotW balance-fixes structure remained compatible at exploit
  levels `1,4,7,10,13,16,19` and replacements `4,10`;
- all 19 Brown-Fur identities were registered by exact permanent GUID;
- Brown-Fur still had zero references in the CotW Arcanist archetype array;
- effective status was Available / Not published; and
- the coordinator reconciled exactly once without save or input access.

Exact passing evidence identity:

- local-runtime package SHA-256
  `675B478D969FE68581EAF6E4EFB50CAE4FF8C2856FD84A5F2BBF72211D8B0F22`;
- built, deployed, installed, and loaded DLL SHA-256
  `CD526A7CEE0D47E6DF999A1FF534AAF1BF114D3D4E9B65FA1261CEBEA0795762`;
- DLL MVID `89f0a045-f1bb-4117-b040-5f4442875119`;
- runtime-result SHA-256
  `AC7EFDB394C2CC8A49C1E1790ED0B2C38AE1450F98706C912D9DF976F78F08D7`;
- runtime-evidence SHA-256
  `8BBD695526245562E6035EFA2A1332C47225AB40659B647641A495BF338494D0`;
- orchestration SHA-256
  `58A1EE922B17B7A855156F361C69134EAAC9D9292B6005950348D16A8CD3BF77`;
- deployment `20260815T2011410833543Z`, deployment-manifest SHA-256
  `EDDA489238581A23D5C5B1EFFF1AF6AA1218D080430FEDFC4C13155623786BD0`;
- backup `20260815T2011363819079Z`, preserved feature-settings SHA-256
  `5B6030AE888F6B127FF23CA03E49578F304DC107ED65E3B2D5F8C8D3D177665E`;
  and
- game build collection `2018.4.10.10503941`.

This closes optional identity registration while deliberately retaining the
mechanics qualification gate before selector publication.

The real player-intent bridge then passed all seven guarded assertions on
commit `37486e6bc100941f6c88fb28a0ad828a99b525c4`. The save-free fixture
granted the three actual registered Brown-Fur features to a disposable unit.
Native `AddFacts` supplied the Powerful Change selection ability and the
off-by-default Share Transmutation activatable. With the real Wisdom marker
and Share activatable enabled, the runtime decision observed Powerful Change,
Wisdom, Share Transmutation, and Transmutation Supremacy together. Clearing
the one-shot intent removed both transient markers and switched Share off
while retaining all three features. A score marker without Powerful Change
ownership failed closed as `powerful-feature-missing` without arming a cast.
All facts and the disposable unit were removed, the Arcanist selector remained
at zero Brown-Fur references, and no save interaction occurred.

Exact passing evidence identity:

- run `20260815T2030084869082Z-f2bbc2103df344479f67ea782e23eff4`;
- evidence directory identity
  `20260815T2030084582275Z-disposable-brown-fur-player-intent`;
- local-runtime package SHA-256
  `397DBF721A897B0D1C592C8CA3083B9147BCA9C7702801A8C7A4FEA53E19230E`;
- built, deployed, installed, and loaded DLL SHA-256
  `6A5FA749BAEE4C09F2032B0F8D506F3655793BA80CE09C4CFB66EC1AEB228CEC`;
- DLL MVID `94665938-eb3b-4e96-8a29-f69689be548e`;
- structured player-intent artifact SHA-256
  `5CD3BE57240C3531031C8E5951E540C1DAB229564BB04C6C50F1299A99871F3C`;
- runtime-result SHA-256
  `D933BC092E2CF02C715332E076B74B374036B8A6DFAE3857F911364B9CBA3699`;
- runtime-evidence SHA-256
  `CD1153231D3B616D301C8A66820F2C894DFCC408B6C9830D48D6F1960660EEE8`;
- orchestration SHA-256
  `62A1EF18311F0FB5343AF601EC415DA623E837F74648B4ADE60C8B5C89E353CA`;
- deployment `20260815T2030083854394Z`, deployment-manifest SHA-256
  `443CB60B73AFC6593126BDE021CEE449B51538C0A57B48D447CF99BB70F045DE`;
- backup `20260815T2030039393280Z`, preserved feature-settings SHA-256
  `5B6030AE888F6B127FF23CA03E49578F304DC107ED65E3B2D5F8C8D3D177665E`;
  and
- game build collection `2018.4.10.10503941`.

This closes real registered feature ownership, native grant presentation at
the fact layer, combined one-shot intent observation, owner-scoped cleanup,
and orphan-marker rejection. It does not yet attach intent derivation to the
native command constructor, prove post-submission interruption, dispel, or
persistence, or authorize selector publication. Brown-Fur remains registered
but unpublished.

The native command bridge subsequently passed all nine guarded
`disposable-brown-fur-arcanist-slot` assertions on commit
`f596b374de8c0b813ae40a397c533b02384f3db3`, run
`20260815T2127190485065Z-01133dcaf3324968876f48017763459b`. The save-free
Steam App ID 640820 fixture used evidence-directory identity
`20260815T2127190292669Z-disposable-brown-fur-arcanist-slot`, deployment
identity `20260815T2127189540525Z`, and backup identity
`20260815T2127146060876Z`. Exact immutable identities were:

- local-runtime package SHA-256
  `F81E853D28A9A385C5295514608B5D652522AA04AE87B5D8B7DC76A12532C596`;
- built, deployed, installed, and loaded DLL SHA-256
  `F0825D68A5FF40AF59EBE3398FAF15A3569251DF275765EF56BB86D08199FFC8`;
- DLL MVID `e8d2a601-d7f5-4df6-86b8-5bd59f36db86`;
- structured slot artifact SHA-256
  `A3B39287E427EB58B79FCEF3AD33DA33206512853D4453BE442440DA88B7BD57`;
- runtime-result SHA-256
  `CD7E1A751F09CD27CD940E88BBA2655604CFF67D9675E37F0BE5A094858A45C5`;
- runtime-evidence SHA-256
  `C59E7DAC581A5A25CEF03F70E52947B95FE6F6C9AE61ACE28613E4DE4DF15512`;
- orchestration SHA-256
  `CA09098B0B51769E54F77C4464717F09EE392925366A9FB33F240B4B8FACB8E4`;
- deployment-manifest SHA-256
  `AD6B9B01FC1072743980F3A29C4B5F3689D193E55A9ACB0D565C1DD02ABAB633`;
- preserved feature-settings SHA-256
  `5B6030AE888F6B127FF23CA03E49578F304DC107ED65E3B2D5F8C8D3D177665E`;
  and
- game build collection `2018.4.10.10503941`.

The actual Powerful Change, Share Transmutation, and Transmutation Supremacy
facts plus Strength and Share one-shot markers caused the real
`UnitUseAbility` constructor postfix to canonicalize the Beast Shape I wrapper
and wolf variant, validate the installed inventory record, and retain exactly
one combined transaction with cost two. The constructor cleared both one-shot
markers while retaining feature ownership. Rule commitment changed the CotW
reservoir `4 -> 2`; native `AbilityData.Spend()` followed the preserved
`ConvertedFrom` root and changed level-three Arcanist slots `6 -> 5`.

The same run proved command-scoped invalid-stat rejection: selecting Charisma
for the Strength-only spell produced native `OnAction()` result `Fail` before
reservoir or slot expenditure. The insufficient-reservoir race likewise left
the one-point reservoir and six slots unchanged and consumed its spend
suppression exactly once. Final transaction, reservation, targeting,
Supremacy, modifier, suppression, resource, one-shot, and disposable-unit
state were all zero or removed.

Two prior diagnostic runs isolated a request-local fixture issue:
`Spellbook.Rest()` removes a spell introduced through temporary
`AddKnown(..., isCopy: true)`. The passing fixture records that false/zero
precondition, restores the disposable known spell, then proves every real
Brown-Fur fact, toggle, and command leaves `CanSpend=true`, available count
six, and `IsKnown=true`. No production workaround or direct slot debit was
added.

This closes native command-constructor intent derivation, canonical source
retention, combined exact accounting, pre-action invalid-intent rejection,
and one-shot cleanup. Post-submission interruption, native dispel,
save/reload persistence, module-OFF existing-owner behavior, and selector
publication remain gates. Brown-Fur remains registered but unpublished.

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
implemented and qualified against the installed fingerprint above. With a
compatible CotW contract they register all 19 stable Brown-Fur identities in a
separate rollback-capable transaction, but do not yet publish the archetype.
Interruption, dispel, persistence, module-OFF owner behavior, and selector
publication remain pending; this document does not claim
player-facing Brown-Fur compatibility.
