# Aid Another / Helpful optional-mod investigation

## Scope and evidence boundary

Investigation began on `codex/bodyguard-in-harms-way` at
`f3608b12def2bbfe41193abdd197539db7bdbd35`. The supported game remains
Kingmaker 2.1.7b. Its `Assembly-CSharp.dll` SHA-256 is
`3B6450FFEC440E296E586F71C711B195AED144B28D53E1CBB29406D18FEF5AFB` and
its MVID is `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`.

The exact installed/reference Call of the Wild artifact is present. No `ZFavoredClass.dll`,
Favored Class UMM directory, settings file, or
`loaded_blueprints.txt` exists in the compatibility reference root or live Mods
directory as of 2026-08-22. Consequently, the CotW conclusions below are exact
binary contracts, while the Favored Class conclusions are public-source-backed
adapter contracts awaiting exact installed-binary and runtime qualification.
The adapter treats absence normally and fails closed on any future structural
mismatch.

## Call of the Wild artifact

- UMM ID: `CallOfTheWild`
- UMM version: `1.14.4c-2.1`
- entry point: `CallOfTheWild.Main.Load`
- assembly: `CallOfTheWild, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null`
- DLL SHA-256: `4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`
- DLL MVID: `8caab254-aacf-4811-8093-44b9184e6e53`
- `Info.json` SHA-256: `32C0BC48C26EB22787E99FB1EB86D074DF2CD7DCFE4804CE8EB381A3A589D44D`
- `settings.json` SHA-256: `24CC3F80269992A53EBBFD1F5986E5AAB056841D6B2F43D8E22E764CDB73F6E8`
- relevant setting: `balance_fixes=true`; CotW has no trait-enable setting
- supporting source checkout: Holic75/KingmakerRebalance commit
  `1332fb0db844b7863f484ca978bea2349fe49769`

The source checkout is evidence for intent and names only. The installed DLL,
its reflected metadata, decompiled IL, live blueprint ledger, hash, and MVID are
the compatibility authority.

### Canonical configuration

Exact installed type `CallOfTheWild.Rebalance` exposes:

- `createAidAnother()`: static, non-generic, zero-argument, `void`;
- `aid_another_config`: `ContextRankConfig`;
- `aid_another_buffs`: `BlueprintBuff[2]`;
- `aid_another`: `BlueprintAbility`;
- `aid_self_free`: `BlueprintAbility`.

`createAidAnother()` constructs one shared rank configuration with:

- `m_BaseValueType = ContextRankBaseValueType.FeatureList`;
- `m_Progression = ContextRankProgression.BonusValue`;
- `m_StepLevel = 2`;
- `m_FeatureList = BlueprintFeature[]`.

Kingmaker's exact `ContextRankConfig.GetBaseValue(MechanicsContext)`
FeatureList branch reads `context.MaybeCaster.Descriptor.Progression.Features`
and adds one for every feature-list entry the caster owns. Duplicate references
therefore deliberately contribute more than once. The exact `GetValue` path
then applies `BonusValue`, adding `m_StepLevel`; the empty-list result is 2.

The two ordinary consumers contain this exact configuration object by reference:

- attack buff `91c27d7593614e06a22c0d74106377f6`,
  `WarpriestCommunityBlessingAidAnother1Buff`;
- AC buff `fd60ba2291144d9a89890dfb1fec561a`,
  `WarpriestCommunityBlessingAidAnother2Buff`.

Their ordinary ability wrapper is
`ab00871bf2914b3ba492fdb2f1af8875` (`AidAnotherAbilityBase`). Allied Cloak's
self wrapper is `e24a160d13b549e8a36c219e686ac319`
(`AidSelfFreeAbilityBase`), with self options
`ed411b8acad442e9b3ccc3716a607f0b` and
`cee69faa66904e5493c1167203253562`. Allied Cloak therefore reaches the same two
buff/config consumers; it does not need a separate amount formula.

Exact installed static reference inventory identifies current contributors:

- Kindness phantom `Benevolent`, feature
  `4b6bcd49fd20498bbbf278b0d65945bc`, occurs twice and grants +2;
- Community Blessing boost
  `85f156e44c3c4dabb2da66dd7a35e7a3`, occurs twice and grants +2;
- Favored Class may append its Helpful twice when that optional mod is loaded.

KMG does not hard-code those two non-Helpful features as a closed universe. It
counts every owned, non-Helpful entry in the exact compatible canonical list,
preserving order and multiplicity.

### Lifecycle

CotW calls `Rebalance.createAidAnother()` from its
`LibraryScriptableObject.LoadDictionary` postfix. Later CotW creation methods,
including Kindness and Community Blessing, append contributors. A mutation made
only at the return of `createAidAnother()` would therefore observe an incomplete
list. KMG lifecycle postfixes only request reconciliation; the first UMM update,
after every LoadDictionary postfix has completed, performs the transaction.
Repeated callbacks reconcile idempotently.

## Favored Class source contract (binary absent)

The authorized public source checkout is Holic75/KingmakerFavoredClass commit
`56ec6c5fd34f0da037350f951383ca7f1a0c5e57`. Its UMM metadata declares:

- UMM ID and expected assembly: `ZFavoredClass` / `ZFavoredClass.dll`;
- version: `1.3.2b`;
- entry point: `ZFavoredClass.Main.Load`;
- setting: exact `Main.settings.enable_traits` Boolean;
- lifecycle: exact static `ZFavoredClass.Traits.load(bool)`.

Blueprint identities in that source ledger are:

- Combat Trait selection: `43d763957f364315b5fff85f9e91ca51`;
- Race Trait selection: `331ed3c4a988415785f71a37b826d0f1`;
- first top-level Trait selection: `34e2812e0f8241bb9e1bee5240c9eb2e`;
- second top-level Trait selection: `5253dcee502a49249bdd8bfdfe525e9f`;
- Adopted: `987e573c15e241c285e0fa1d5ac0a0a2`;
- Additional Traits: `6a1f65b204a74c22b0f47e1e2c808441`;
- existing halfling Helpful: `c9bd9f6cc24f41e684a68e6510afc726`;
- native halfling race: `b0c3ef2729c498f47970bb50fa1acd30`.

The source builds `HelpfulTrait`, displayed as `Helpful`, as a rank-one
`FeatureGroup.Trait` with exactly one
`ZFavoredClass.NewMechanics.PrerequisiteRace` targeting the halfling race. It
adds the same feature twice to CotW's canonical list and puts it once in Race
Traits. Ownership—not current race—is the mechanical grant condition.

`Traits.load(false)` still creates the trait/category identities and appends the
halfling Helpful contributor; it omits only top-level/new-selection publication
and companion initialization. `Traits.load(true)` additionally routes both
top-level Trait choices and Additional Traits through the category selections.
KMG never changes companion presets.

Adopted is a clone of Race Traits with empty components and
`IgnorePrerequisites=true`. Reciprocal `PrerequisiteNoFeature` components safely
prevent normal dual selection through Combat/Race categories, both top-level
choices, Additional Traits, and ordinary respec. Adopted intentionally ignores
those prerequisites, so KMG does not break its race-trait behavior. Any dual
owner from Adopted, old saves, respec tools, or editors is handled mechanically
as the better Helpful replacement and diagnosed once; neither trait is removed.

## Selected integration

KMG owns one new save-stable trait, `KMG.Traits.HelpfulCombat`, and always
registers it. It is only published into the exact compatible Favored Class
Combat Trait selection when `enable_traits=true` and `bodyguard-feats` is active.
The foreign halfling Helpful is reused without cloning, renaming, re-GUIDing, or
Race Trait re-publication.

The shared resolver computes:

`2 + max(combat Helpful +1, halfling Helpful +2) + all owned non-Helpful canonical entries`.

The selected KMG adapter appends combat Helpful exactly once to CotW's canonical feature list. An
exact-reference-gated postfix on only that configuration's
`ContextRankConfig.GetValue(MechanicsContext)` applies the shared resolver to
ordinary attack, ordinary AC, and Allied Cloak consumers. Bodyguard will call the
same resolver after its native AoO spend and Aid d20, then uses the resolved
amount as its one attack-scoped contribution. No Aid ability, buff, action, or
attack is synthesized.

All touched arrays are transactional: CotW feature list, Favored Combat Trait
`Features`/`AllFeatures`, and both Helpful component arrays. Partial failure
restores the exact original references, ordering, and multiplicity. Optional
absence or incompatibility never fails KMG bootstrap.

Rejected approaches:

- appending both Helpful encodings without correction: invalid dual owners get
  +5 before unrelated increases;
- capping Aid Another at +4: destroys Benevolent and other independent increases;
- checking only the two known external features: discards future/unknown
  canonical contributors;
- invoking CotW's standard-action ability for Bodyguard: changes timing and
  action economy;
- globally patching every ContextRankConfig: affects unrelated ranks;
- localized-name discovery: unstable and ambiguous because both traits are named
  Helpful;
- copying Favored Class code/assets or compiling against its DLL: violates
  standalone and redistribution requirements.

## Qualification status

The exact CotW artifact supports binary contract and CotW-only runtime work. The
Favored Class adapter, selection publication, traits-disabled profile, and
three-mod profile cannot be called runtime-qualified until an authorized exact
`ZFavoredClass.dll` profile is present. This absence is an optional-extension
qualification blocker, not a KMG package/bootstrap failure.
