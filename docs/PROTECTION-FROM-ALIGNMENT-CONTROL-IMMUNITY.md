# Protection from Alignment control immunity

## Scope and behavior

This startup-only feature gives Protection from Evil, Good, Law, and Chaos
Wrath parity for new, explicitly registered domination and comparable
mental-control applications. A protected target rejects a new registered
control buff only when the actual originating unit has the alignment component
opposed by that protection. The ability identity and the terminal buff identity
are both checked; the terminal-buff path remains effective for direct and
custom delivery.

This is deliberately not the complete tabletop paragraph. In particular, an
already-active domination remains active when protection is applied. The
feature does not grant a new saving throw or +2 morale bonus, suppress, pause,
remove, replace, or later resume an existing effect, and it does not grant
blanket immunity to Mind-Affecting, Charm, Compulsion, Fear, or Emotion
descriptors. Removing protection never modifies an existing control buff.

The independent Unity Mod Manager setting is
`protection-from-alignment-control-immunity`, displayed as `Protection from
Alignment: control immunity`. It defaults on, uses feature-module settings
schema 9, and takes effect after a complete Kingmaker restart. Turning it off
publishes no immunity components and preserves vanilla Kingmaker application
behavior.

## Kingmaker 2.1.7b API evidence

The local Kingmaker 2.1.7b managed assemblies were inspected with the
repository's normal IL inspection workflow before implementation:

- `BuffCollection.TriggerRuleApplyBuff` creates and triggers `RuleApplyBuff`
  with the receiving creature as the rule initiator. A target-side
  `RuleInitiatorLogicComponent<RuleApplyBuff>` is therefore the normal engine
  seam.
- `RuleApplyBuff` exposes `Blueprint`, `Context`, and mutable `CanApply`.
  Setting `evt.CanApply = false` in `OnEventAboutToTrigger` is the proven veto.
- `evt.Blueprint` is the terminal `BlueprintBuff` being attempted.
- `evt.Context.SourceAbility` is the originating `BlueprintAbility` when the
  delivery retains one.
- `evt.Context.MaybeCaster` is the originating `UnitEntityData` when one is
  available. Its `Descriptor.Alignment.Value` is tested with native
  `Alignment.HasComponent(AlignmentComponent)` semantics.
- Native `BuffDescriptorImmunity`, `SpellImmunityToSpellDescriptor`, and
  related descriptor components are broader than this rule and cannot express
  the explicit catalog plus source-alignment predicate safely.

No global Harmony patch or new dependency is used. Runtime handling is one
small target-side rulebook component on each terminal protection buff.

## Patched protection-buff inventory

All five entries are required base-game assets. Each resolved in the local
Kingmaker blueprint audit. Runtime publication still validates exact GUID,
exact type, and a nonempty internal name before any mutation.

| Blueprint | GUID | Protected against | Local audit resolved |
|---|---|---|---|
| `ProtectionFromEvilBuff` | `4a6911969911ce9499bf27dde9bfcedc` | Evil | Yes |
| `ProtectionFromGoodBuff` | `b19e788487556aa4397080ef3dbb3619` | Good | Yes |
| `ProtectionFromLawBuff` | `744bec63273df53438c6b76aaaa78382` | Law | Yes |
| `ProtectionFromChaosBuff` | `92150879041b1fb48acfbcf7034e8b33` | Chaos | Yes |
| `AuraOfProtectionFromEvilEffectBuff` | `8deb9d5cef3472646ac5199eb9edfb87` | Evil | Yes |

The Paladin aura's source buff (`c8876df41a13f9243b3bfdb15b84b129`),
area effect (`718fa2f04fe085842a4960022d33d7ac`), and feature
(`4ddace64ffabcf24a8268e4d52c23e88`) converge on the patched aura effect buff;
only that target-owned terminal buff is patched.

Publication is deterministic and idempotent. Zero exact components appends
one; one is retained and reported as already patched; duplicates or a component
for the wrong alignment fail this feature closed. A later failure restores the
exact prior component-array reference when safe.

## Protection delivery and wrapper audit

These audited abilities and consumables ultimately apply one of the four core
terminal buffs above. They need no component of their own, so individual,
communal, Paladin, scroll, potion, item, and other shared-buff delivery paths
inherit the rule without duplicates.

| Blueprint | GUID | Kind | Local audit result |
|---|---|---|---|
| Protection from Evil | `eee384c813b6d74498d1b9cc720d61f4` | Ability | Core Evil buff |
| Protection from Good | `2ac7637daeb2aa143a3bae860095b63e` | Ability | Core Good buff |
| Protection from Law | `c3aafbbb6e8fc754fb8c82ede3280051` | Ability | Core Law buff |
| Protection from Chaos | `1eaf1020e82028d4db55e6e464269e00` | Ability | Core Chaos buff |
| Protection from Evil, Communal | `93f391b0c5a99e04e83bbfbe3bb6db64` | Ability | Core Evil buff |
| Protection from Good, Communal | `5bfd4cce1557d5744914f8f6d85959a4` | Ability | Core Good buff |
| Protection from Law, Communal | `8b8ccc9763e3cc74bbf5acc9c98557b9` | Ability | Core Law buff |
| Protection from Chaos, Communal | `0ec75ec95d9e39d47a23610123ba1bad` | Ability | Core Chaos buff |
| ProtectionFromAlignment | `433b1faf4d02cc34abb0ade5ceda47c4` | Generic ability | Selects a core buff |
| ProtectionFromAlignmentCommunal | `2cadf6c6350e4684baa109d067277a45` | Generic communal ability | Selects a core buff |
| ProtectionFromEvilChaosEvil | `07dccc8e4c4489c4d9de721dddaf12cc` | Special ability | Core Evil buff |
| ProtectionFromChaosChaosEvil | `b70104f09b3da794da923fbf248befc5` | Special ability | Core Chaos buff |
| ProtectionFromChaosEvil | `c28f7234f5fb8c943a77621ad96ad8f9` | Special ability | Core Chaos buff |
| ProtectionFromEvilCommunalChaosEvil | `224f03e74d1dd4648a81242c01e65f41` | Special communal ability | Core Evil buff |
| ProtectionFromChaosCommunalChaosEvil | `b6da529f710491b4fa789a5838c1ae8f` | Special communal ability | Core Chaos buff |
| ProtectionFromChaosEvilCommunal | `3026de673d4d8fe45baf40e0b5edd718` | Special communal ability | Core Chaos buff |
| ProtectionFromEvilCutscene | `1871a2eb5a1ed024bbd86a04bd9b0ca5` | Cutscene ability | Core Evil buff |
| Potion of Protection from Chaos | `ec487c0ecc801e048aed50851d937fd8` | Potion ability | Core Chaos buff |
| Potion of Protection from Evil | `de000ebb9b86c8f48b77576965303183` | Potion ability | Core Evil buff |
| Potion of Protection from Good | `e5e2567210888184cb3c552c02e86b89` | Potion ability | Core Good buff |
| Potion of Protection from Law | `31a74f20fcba2c9419738a94f6727dd6` | Potion ability | Core Law buff |
| Scroll of Protection from Chaos | `59110d30bb15dcd4d89f762b6aa9db9b` | Scroll ability | Core Chaos buff |
| Scroll of Protection from Evil | `96eb7a498b4db2c4a9fcfb632064b948` | Scroll ability | Core Evil buff |
| Scroll of Protection from Good | `c75c69797fd6ee24d84b12796c0c3d45` | Scroll ability | Core Good buff |
| Scroll of Protection from Law | `6dad6628ecc36c7428f6e877975a1041` | Scroll ability | Core Law buff |
| Scroll of Protection from Chaos, Communal | `a2af3233183a22a4693e3de034068d29` | Scroll ability | Core Chaos buff |
| Scroll of Protection from Chaos/Evil, Communal | `7a7cb3118fdb3274a90fc34dd21457f6` | Scroll ability | Core Chaos buff |
| Scroll of Protection from Evil, Communal | `eb776c7c1a2ffc3498adab069588b70c` | Scroll ability | Core Evil buff |
| Scroll of Protection from Good, Communal | `0afbc5cbd6165a64ea79b0a87058f6c1` | Scroll ability | Core Good buff |
| Scroll of Protection from Law, Communal | `915d6ff0a30fe974ca843dde14b1619a` | Scroll ability | Core Law buff |

The base-game item enchantment named `ProtectionFromEvil`
(`b5c80d332c8b8ab4e982adf5a276f9fb`) is an enchantment, not a distinct
`BlueprintBuff`, and is therefore not patched as a protection terminal. No
other distinct item-only protection terminal buff was found. Any item that
applies one of the five patched buffs inherits the behavior automatically.

## Explicit mental-control catalog

An entry qualifies because it meaningfully transfers control, possession, or
faction allegiance to the originating creature. `Required` means that a
missing or wrong-type asset disables only this feature's enabled publication;
optional misses are diagnostic only. All vanilla and Gunslinger entries below
resolved in the local audit. The Call of the Wild 1.14.4c source and installed
assets were locally available and yielded the listed stable identities, but
they remain optional at runtime and do not create a dependency.

Each registered delivery ability was also checked at its action boundary: its
target buff application is the qualifying control effect. Delivery-area and
bookkeeping identities that are not themselves control terminals are not
registered, so sharing a domination implementation does not make them broadly
immune.

| Blueprint | GUID | Kind | Source | Inclusion reason | Required | Local audit resolved |
|---|---|---|---|---|---|---|
| `DominatePerson` | `d7cbd2004ce66a042aeab2e95a3c5c61` | Ability | Vanilla | Changes victim to creator faction | Yes | Yes |
| `DominateMonster` | `3c17035ec4717674cae2e841a190e757` | Ability | Vanilla | Creature-wide native domination | Yes | Yes |
| `DominateAnimal` | `754c478a2aa9bb54d809e648c3f7ac0e` | Ability | Vanilla | Native animal domination | Yes | Yes |
| `C61_NyrissaDominateMonster` | `e349d48d79783d24aba78006f3e84b8c` | Ability | Vanilla | Alternate encounter domination | Yes | Yes |
| `EnchantmentDominateAbility` | `e2754ae5185031e45b853da434ee9c6f` | Ability | Vanilla | Alternate faction-changing domination | Yes | Yes |
| `EnchantmentDominateSpell` | `0f368511a1f73ba4b8b3fd204e751572` | Ability | Vanilla | Alternate domination spell delivery | Yes | Yes |
| `CharmAnimal` | `08df458bd00ba704dab32dd493c61518` | Ability | Vanilla | Faction-converts an animal | Yes | Yes |
| `CharmPerson` | `1af9d5995090e5a4185a30decf0959ad` | Ability | Vanilla | Faction-converts a humanoid | Yes | Yes |
| `BloodlineSerpentineScaledSoulCharmingGazeAbility` | `a5d4f66181c8085429640339f417eae8` | Ability | Vanilla | Alternate native faction-changing charm | Yes | Yes |
| `DominatePersonBuff` | `c0f4e1c24c9cd334ca988ed1bd9d201f` | Buff | Vanilla | Authoritative domination terminal | Yes | Yes |
| `DominatePersonUniqueBuff` | `d6f8f810781b5394392d99204c6a02c2` | Buff | Vanilla | Alternate encounter domination terminal | Yes | Yes |
| `EnchantmentDominatePersonBuff` | `cb7e4dd25ad20f345b6351fdd4c621f3` | Buff | Vanilla | Alternate domination terminal | Yes | Yes |
| `Charm` | `9dc29118addce3d48ae9b92be953b5b4` | Buff | Vanilla | Authoritative faction-changing charm terminal | Yes | Yes |
| `KMG.Summoning.Special.Succubus.Dominate` | `1662d63944d94cdeaa62562dc9ac9349` | Ability | Gunslinger | Expanded Summoning Succubus domination | Yes | Yes |
| `KMG.Summoning.Special.Succubus.Domination` | `6e1f6eb3e773451dbda9e0ecd07486d9` | Buff | Gunslinger | Direct faction-changing terminal | Yes | Yes |
| `ControlUndeadAbility` | `998469fa09314fd687b4ffa051a95c59` | Ability | Call of the Wild | Creator controls undead target | No | Yes locally; optional runtime |
| `ControlUndeadBuff` | `21d20a30b93e4ae281a6d70d9ae1a64d` | Buff | Call of the Wild | Faction-changing terminal | No | Yes locally; optional runtime |
| `ControlConstructAbility` | `efec86b954ff42e99893d55f99e51a5e` | Ability | Call of the Wild | Creator controls construct target | No | Yes locally; optional runtime |
| `ControlConstructBuff` | `fe97da5e44014fd8a643a54fe791e7ae` | Buff | Call of the Wild | Faction-changing terminal | No | Yes locally; optional runtime |
| `WitchAnimalServantHexAbility` | `583e661fe4244a319672bc6ccdc51294` | Ability | Call of the Wild | Removes free will and changes faction | No | Yes locally; optional runtime |
| `WitchAnimalServantHexBuff` | `32b4b11964724f59a9034e61014dbb3c` | Buff | Call of the Wild | Animal Servant terminal | No | Yes locally; optional runtime |
| `SwayingWordAbility` | `e5096e16c9cb46cf9460a9c84dea699b` | Ability | Call of the Wild | Applies bounded native domination | No | Yes locally; optional runtime |

No trusted source-alignment override is assigned to a production entry. A null
or unclassifiable source therefore fails open today. The registration API does
support exact trusted metadata for a genuinely source-less audited effect. The
unresolved `ability|buff` pair is logged once at debug level with the outcome
and policy reason.

No separate native Kingmaker Succubus domination blueprint was present in the
local 2.1.7b blueprint data under a verifiable identity. No GUID was fabricated;
the native domination abilities and terminal buffs above remain covered, and
this unresolved named-monster inventory item requires an in-game encounter
check.

Explicitly excluded after mechanical review: Confusion, fear, sleep, daze,
stun, Hold Person, paralysis, fascination without control, ordinary morale
effects, beneficial mind-affecting buffs, Murderous Command and similar
one-command hostile compulsions, and other effects that do not let the creator
direct, possess, or faction-convert the victim. Call of the Wild's Infectious
Charms propagation markers and Dominate Arcane Eye infrastructure are also not
control terminals.

## Initialization diagnostics

One structured summary records resolved, newly patched, and already-patched
protection buffs; registered ability and buff counts; missing required and
optional assets; and whether all optional Call of the Wild identities were
available at that initialization point. Required failures are logged and
rolled back inside the feature boundary so Shield Other, Expanded Summoning,
and other modules continue. Optional misses neither throw nor disable base-game
coverage.

## Automated coverage boundary

The deterministic harness exercises the qualification/alignment policy,
ability and terminal-buff paths, the mod Succubus identities, descriptor
regressions, trusted and fail-open null-source behavior, catalog and component
idempotence, settings independence, optional-content isolation, wrapper
inventory, and source contracts for the exact Kingmaker event adapter. The
normal test harness cannot instantiate a complete live `RuleApplyBuff`
rulebook graph, so actual event dispatch and gameplay behavior remain subject
to the manual in-game validation below. Automated policy success must not be
reported as in-game qualification.

## Manual in-game validation

Use a disposable test fixture/save and the repository's guarded runtime rules.
Do not overwrite `KMG_AUTOMATION_BASELINE`. Record the exact loaded mod version,
source unit blueprint and alignment, delivery ability, target buffs before and
after, save result, faction/controllability state, and relevant log summary.

### A. Matching alignment

1. On a clean target, apply Protection from Evil and verify its ordinary buff.
2. With a positively identified evil caster, attempt each resolved vanilla
   domination/charm delivery and the Expanded Summoning Succubus delivery.
3. Satisfy or bypass only the effect's ordinary targeting prerequisites; do not
   alter the source alignment during the attempt.
4. Confirm the registered terminal control buff never appears, the target does
   not change faction or become controllable, and the protection buff remains.
5. Repeat representative lawful, good, and chaotic sources against Protection
   from Law, Good, and Chaos. Verify a lawful evil source is blocked by either
   Protection from Law or Protection from Evil.

### B. Mismatched alignment

1. Repeat the same registered delivery from a positively identified non-evil
   source against Protection from Evil.
2. Arrange the ordinary saving throw result consistently with the fixture.
3. Confirm the control effect resolves normally when its ordinary rules permit;
   the protection must not veto it merely because the effect is registered.

### C. Existing domination

1. Dominate the target first and record the active terminal buff and faction.
2. Apply Protection from Evil afterward.
3. Confirm the already-active domination remains active, faction/control does
   not revert, and removing protection changes neither existing effect. This is
   the intentional Wrath parity limitation.

### D. Descriptor regression

1. While Protection from Evil is active, apply a fear effect, Confusion, a
   sleep effect, and a beneficial Mind-Affecting buff.
2. Satisfy their ordinary saves and immunities as needed for observation.
3. Confirm each can apply normally and no blanket descriptor immunity exists.

### E. Delivery-path regression

1. Exercise an encounter/source that uses the native domination terminal and
   record whether its delivery preserves an ability context.
2. Exercise the repository's Expanded Summoning Succubus, whose custom action
   directly applies `6e1f6eb3e773451dbda9e0ecd07486d9`.
3. If an authorized development tool can apply a terminal buff directly, apply
   `c0f4e1c24c9cd334ca988ed1bd9d201f` without an ordinary spell cast and verify
   that the target-side buff identity still blocks an evil source. This
   repository currently has no dedicated guarded scenario for that injection,
   so do not claim this row without a witnessed authorized fixture.

### F. Persistence and idempotence

1. Save a disposable working state while a protection buff is active, then
   reload it through an authorized save procedure.
2. Confirm the ordinary protection buff persists and a new matching-alignment
   registered control attempt remains blocked.
3. Confirm an existing pre-protection domination still persists.
4. Inspect the initialization summary and live protection blueprints: each of
   the five terminal buffs must contain exactly one matching component after
   cache initialization, with no custom serialized relationship or runtime
   entity/context state.
5. Repeat with individual and communal spell delivery and representative
   scroll, potion, Paladin aura, and item delivery that shares a patched buff.
6. Apply multiple applicable protections together, remove one, and confirm the
   remaining protection continues to block its own matching alignment.
