# Call of the Wild conversion semantics (POL-06 / S5 T5)

Status: **accepted and closed for its reviewed prepared-caster development scope (principal review, 2026-11-20).** Not merged, not released. See Acceptance below.

- **Branch:** `codex/pol06-cotw-conversion-semantics`, draft PR #190, stacked on PR #189 (accepted S5 T4 head
  `acd50a042a`).
- **Epic story:** S5 T5, inventory record POL-06: "Metamagic conversion popup includes raw base-level conversion rows".

## Acceptance (principal review, 2026-11-20)

- **Accepted review head:** `c2867a3aa1216c1cd842b7acf644421208e234e9` (PR #190, which stays a draft).
- **Qualified product-build commit:** `c5fdcb1431c25acfac48dfc73b617ef94bc1af51`: package
  `2fd49eaadb1ca6d4a1b873a563b9e3684707d4e2f1cff684cc782c8b24bebb72`, DLL
  `bff80052bfb76d7d29731946a85c1141ea604d014220564431e7e2b429e97645`, MVID `6d4e9a1b-2ae4-441f-b739-924dc535d0bb`,
  harness `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089`. These identities are frozen; the
  records-only heads after it were not rebuilt.
- **Qualified native session:** `20261120T054533Z-owner-0.33.0-pol06-lane`, original in-game verdict **PASS, 77 of
  77** (76 review cells and the profile-restoration check).
- **Scope accepted:** the reviewed prepared-caster Herald Caller and Druid conversion scope: the semantics-preserving
  projection, its raw fallbacks (same-tier rows, unknown and non-summon conversions), the per-conversion observer and
  the native cases listed under Validation. The projection, the raw fallbacks and the duplicate-looking rows are
  retained; no deduplication and no new chooser.
- **Not covered by the acceptance:** every item under Limits stays unrun or open, including the full
  `summon-cleric-alignment` phase with the corrected observer, metamagic beyond Extend and Heighten, a Herald d3 and a
  Druid d4+1 cast, controller presentation, the owner installation and the 15-mod stack. Spontaneous casters stay a
  scope boundary. Exact pinned-slot debit stays qualified to the unique-copy fixture; the native equal-preparation
  spend is preserved. The cursor explanation of the full-phase guard aborts stays an unattributed hypothesis. Every
  earlier failed session is kept below under its own identity.
- **Unchanged by this story:** S5 T2's acceptance and both of its aggregate-FAIL sessions, the unclassified
  console-learning issue, all approved exclusions, and the release-validator obligations including DEF-05's.
- **Presentation claims corrected at acceptance** (Presentation boundary and Evidence verification below): the
  reachability and tooltip claims are stated for the rows actually checked, and the all-row comparison is stated as
  the key it used, not as proof of behavioural equivalence.

## Inputs inspected

| Input | Identity |
|---|---|
| Kingmaker `Assembly-CSharp.dll` (2.1.7b, read-only copy) | SHA-256 `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb` |
| Call of the Wild 1.14.4c-2.1 `CallOfTheWild.dll` (read-only copy of the installed mod) | SHA-256 `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915` |
| Installed Call of the Wild setting `metamagic_for_spontaneous_spell_conversion` | `true` |
| Product before this story | `DruidSummonConversionVariantPatch`, `DruidSummonConversionPopupRowsPatch`, `DruidSummonConversionPolicy` at `acd50a042a` |
| Retained native observation | session `20261111T0045385494739Z` (`summon-cleric-alignment`, combined DATA profile, cleric phase PASS 234/234), cell `cleric-menu-spider-herald-extended-source-tier5` |

Both assemblies were decompiled with ilspycmd into the session scratch area. Nothing in either was changed.

## The conversion contracts

### Ordinary conversion (native, and the first part of Call of the Wild's list)

- **Rows:** `Spellbook.GetSpontaneousConversionSpells(source)`, which reads the conversion arrays at
  `GetSpellLevel(source)`, the source's known level plus its own metamagic cost (`Spellbook.GetSpellLevel(AbilityData)`).
  Each row is `new AbilityData(blueprint, book) { ConvertedFrom = source }`.
- **Metamagic:** the native `ConvertedFrom` setter copies the source's `MetamagicData` only when the source
  `HasVariant` the row. A conversion is not a variant of an unrelated source, so **an ordinary conversion carries no
  metamagic**, even over a source prepared with metamagic.
- **Spell level:** `AbilityData.SpellLevel` follows `ConvertedFrom`, so a converted row has the source's level (known
  level plus the source's metamagic cost).
- **Debit:** `SpendFromSpellbook` and availability walk `ConvertedFrom` to the source. The native
  `Spellbook.Spend(source)` for a prepared book then takes the **last available memorized slot whose spell `Equals`
  the source** (blueprint, caster, fact, book and `ConvertedFrom`). With two equal preparations it may spend the other
  one; it never pins the slot whose popup was opened. The product preserves this native selection and does not
  introduce captured-slot spending.
- **Native evidence:** in session `20261111T0045385494739Z`, the Herald Ogre converted from an unrelated level 4 spell
  prepared with Extend Spell had no Extend, 20 rounds at caster level 20, and spent the level 5 slot
  (`spider-herald-extended-source-caster-level-duration`).

### Call of the Wild's routes

- **`ActionBarGroupSlot.SetToggleAdditionalSpells` prefix:** replaces the native method (returns false). It sets the
  slot's `Conversion` list to `getAdditionalToggleSpells(slot, book, spell)`.
- **`MechanicActionBarSlotMemorizedSpell.GetConvertedAbilityData` and the spontaneous-slot equivalent:** replace the
  native factory with the same list, followed by the source's own variants (`ConvertedFrom = source`).
- **`getAdditionalToggleSpells`** builds, in this order:
  1. the ordinary rows above, built the same way;
  2. with the setting on, the **metamagic conversion rows** below;
  3. for a memorized slot, one ability row per caster ability with `AbilityRestoreSpellSlot`, or with an
     `AbilityConvertSpell` that can be used on the spell (`ParamSpellSlot` set, `ConvertedFrom` null);
  4. for a spontaneous slot, the equivalent restore and convert ability rows.

### Call of the Wild's metamagic conversion rows (the subject of POL-06)

`getMetamagicSpontaneousConversionSpells` takes `L = GetSpellLevel(source)`. For every level `n` from 1 to `L`, and
every conversion spell `b` the book holds at `n`, it uses the **caster's own metamagic feats** (`AddMetamagicFeat`)
that `b.AvailableMetamagic` accepts.

- **Combinations:** it enumerates those feats in combination. At most one from the elemental group (mask
  `0x01E00000`) is allowed, and Heighten is tried at increasing levels.
- **Cost:** `RuleApplyMetamagic` gives the cost, adjusted by the difference between `n` and `b`'s known level. Only a
  combination whose cost equals `L - n` produces a row.
- **The row:** `new AbilityData(b, book) { ConvertedFrom = source, MetamagicData = result }`. The setter runs first and
  copies nothing; the metamagic is then assigned explicitly.

So each such row is:

| Property | Value |
|---|---|
| Creature list | the conversion feature's own quantity spell of a **lower** tier `n` (every metamagic costs at least one level, so `n < L` in practice) |
| Metamagic | built from the **caster's feats** to make up `L - n`; **not the source's metamagic** |
| Spell level | `AbilityData.SpellLevel` is `L`, the source's level, through `ConvertedFrom` (the slot level). The engine's effective level in the cast context (`Params.SpellLevel`) is `L` minus the cost of metamagic that does not raise it: observed 4 for an Extend row, 5 for a Heighten +1 row, both from a level 5 slot |
| Debit | one native debit of an equal preparation of the source, through `ConvertedFrom` (see the ordinary contract) |
| Eligibility | the same conversion feature (Herald Caller or Druid), the caster's metamagic feats and the spell's accepted metamagic; for the Herald, the native umbrella chooses its branch by alignment at cast |

**Native confirmation.** In session `20261111T0045385494739Z`, the level 5 Herald popup over an unrelated level 4
spell prepared with Extend held exactly three such rows, for a caster with only Extend Spell. They were Summon Monster
IV Single, 1d3 and 1d4+1, each with Extend, converted from that source. This is exactly the `n = 4`, cost 1 case.

**Correction to an earlier record.** The 2026-11-11 records (`planning/races-unleashed-compatibility/STATUS.md`,
`KNOWN-ISSUES.md`) described these as conversions "at the source's base level, carrying its metamagic". The rows are
lower-level conversions carrying metamagic built from the caster's feats, cast at the source's level. Their
observations stand. The 2026-11-11 STATUS record is kept as written; `KNOWN-ISSUES.md` now carries the corrected
description.

**Rows that are not summon conversions.**
- Cure and inflict conversions, including any Call of the Wild metamagic versions of them, are not in the Herald or
  Druid quantity arrays.
- Ability rows (`AbilityConvertSpell`, slot restoration) have no `ConvertedFrom`.
- A third party's conversion that is not one of these exact quantity spells is not recognized.

None of these is touched.

### Presentation (native)

- **In the popup:** a converted popup slot (`MechanicActionBarSlotSpontaneusConvertedSpell`) draws only the
  blueprint's icon. Its title and description are the blueprint's name and description.
- **Metamagic:** the native ability tooltip adds a Metamagic block when the row has metamagic
  (`DescriptionTemplatesAbility`).
- **Consequence:** metamagic is visible only in the tooltip. This already applies to Call of the Wild's raw rows.

## What was wrong

The projection handled only the ordinary rows (`GetSpontaneousConversionSpells(source)`). Call of the Wild's metamagic
rows therefore stayed as raw native quantity rows beside the projected creature menu. An example is "Summon Monster
IV (1d4+1 Wolves)" in a level 5 Herald popup, which offers Owlcat's fixed creature instead of the menu's choices at
that tier and quantity.

## Implementation: a semantics-preserving projection

**Where:** `DruidSummonConversionVariantPatch.ProjectRows`, used by both existing postfixes: the factory and the PC
popup's `Conversion` list.

**Recognition.** A row is an *additional* conversion only when all of these hold:
- it is converted from this exact source, in this exact book;
- it carries a nonzero `MetamagicData`;
- its blueprint is one of this conversion feature's own quantity spells, and is not in the source-level list;
- its tier is below the source's level;
- the book's conversion list holds it exactly once at that tier (`DruidSummonConversionPolicy.IsAdditionalConversionLevel`).

**Projection.**
- Each recognized row projects into the published creatures of **its own tier and quantity**, through the same native
  constructor and `ConvertedFrom` path as an ordinary row.
- **Metamagic:** the child inherits exactly that row's `MetamagicData` object (the native setter, through the existing
  scoped `HasVariant` relation).
- **Spell level and debit:** the child's spell level and debit follow the same chain to the source.
- **Admission and receipts:** the Herald alignment filter, the third-party and Spider Summoner admission and the exact
  receipts apply unchanged. A receipt for an additional row re-verifies its tier and metamagic instead of the
  source-level list.

**Left raw, as Call of the Wild made them:**
- a row whose tier is the prepared parent itself. This is a **conservative presentation fallback**: projecting it
  would multiply the prepared parent's creature menu. The row keeps its own metamagic and execution chain. It is
  **not** presumed semantically identical to the source's variants: the fallback compares tiers, not metamagic, so a
  prepared Extended summon and a separately generated Reach row of the same tier both appear, with different
  metamagic. Nothing is hidden and no power changes;
- two rows with the same blueprint and the same metamagic;
- any row whose projection cannot be verified exactly. Such a row never withdraws the ordinary projection.

**Order.** Additional children follow every ordinary child, in Call of the Wild's own row order. Within a row they keep
the published parent's order (`DruidSummonConversionPolicy.ProjectedRank`).

**Scope boundary:** the projection handles prepared slots only, as it did before this story. Call of the Wild's
rows on a spontaneous caster's slot stay as Call of the Wild made them. This is the retained prepared-caster
restriction, not an untested supported route.

**Unchanged:**
- ordinary rows and their projection;
- cure, inflict, ability and third-party rows;
- the behaviour with the setting off;
- Call of the Wild itself, and every identity.

### Presentation boundary for owner review

The principal review (2026-11-20) retains the projection direction and its raw fallbacks, and the native tooltip as
the distinguishing presentation for this development scope; no nested chooser. The measurements below are for the
owner.

- **Creature icons:** projected children show the creature's icon, like every projected row. Their metamagic is
  visible in the tooltip's Metamagic block, as for Call of the Wild's raw rows.
- **The same creature twice:** a creature can appear once as an ordinary choice (higher tier, no metamagic) and once
  as an additional choice (lower tier, with metamagic). Both are cast at the source's level. Only the tooltip
  distinguishes them, by its metamagic block and by the creature's quantity label.
- **Menu size:** the popup grows by the permitted creatures of each additional row. The count multiplies with the
  caster's metamagic feats that summon spells accept, because Call of the Wild makes one row per combination.
  Measured: Herald 68 ordinary rows, 126 with Extend Spell, **302 with Extend Spell and Heighten Spell** (18 Call of
  the Wild conversions at tiers I to IV, 234 projected creatures). Druid 76 and 140 with Extend Spell.
- **Reachability (two-feat popup, measured for three rows):** the popup contained 302 rows. Reachability was checked
  for three rows only: the selected Extend row (Ant, Giant (Drone), index 186), its second-metamagic counterpart (the
  same creature under the tier IV Heighten +1 conversion, index 213) and the last row (Wolf (1d4+1), index 301). Each
  was fully visible in the popup and on the 1920×1080 screen, in a stationary observation with no scrolling. The other
  299 rows were not individually checked.
- **Tooltips (measured for two rows):** two native tooltips were rendered, for the selected Extend row and its
  Heighten counterpart; their Metamagic blocks read "Extended," and "Heightened," respectively. No other row's tooltip
  was rendered.
- **All-row comparison (computed, not rendered):** every one of the 302 rows was compared on its name, its computed
  metamagic text (`UIUtilityTexts.GetMetamagicList` of its mask, the text the native tooltip shows; not read from a
  rendered tooltip), its spawned-unit identities with their count-dice kinds, its metamagic mask and its
  `AbilityData.SpellLevel`. That field is the slot-level property (5 for every row here), not the level captured in a
  cast's execution context; the observer's recorded label for this key ("effective spell level") is inaccurate. No two
  rows that differ under this key share both a name and a metamagic text. The key omits, among other things, the
  heighten level, the conversion and the duration, so this comparison does not prove complete behavioural
  equivalence or difference.
- **Duplicate-looking rows (two-feat popup, measured):** eight labels appear twice with the same name, computed
  metamagic text and comparison key, for example "Beetle, Fire (1d4+1)" with "Heightened,": once from the tier III
  d4+1 conversion under Heighten +2 (key `7b531828b885e8b47a7b06a6f1a34805|16|2|2`) and once from the tier IV d4+1
  conversion under Heighten +1 (`24ed31f7b42eb5f4ab461be90d479066|16|1|1`), both from the level 5 slot. They are
  retained as published and left for owner review; whether every effect of the two is identical was not established.
- **Alternative, if the owner prefers it:** keep Call of the Wild's rows raw and label them with their tier and
  metamagic. Hiding them, or changing their power, would need an explicit product decision; neither was done.

## Validation

### The observer (corrected after the 2026-11-20 principal review)

The first observer checked each projected child's tier, then its membership in the product receipt's own
`ProjectionVariants`, then the combined child set. That could not show that each quantity conversion owns its own
creatures: a projection that crossed the d3 and d4+1 associations would keep the combined set and pass. The corrected
observer (`RuntimeTestRunner.Pol06CotwConversions.cs`, `Pol06ConversionObservationVerdict.cs`) derives every
expectation independently of the projection under test:

- **Conversions:** Call of the Wild's own builder (`getAdditionalToggleSpells`) for the same slot and source, keyed by
  quantity spell, metamagic mask, heighten level and level cost.
- **Each conversion's tier:** the single level at which the book's native conversion lists hold its spell.
- **Each conversion's quantity:** its own spawn expression (no die, d3, d4); a tier I canonical parent, which the native
  arrays hold itself, is Single.
- **The creatures it must own:** the route parent of that tier, filtered to this caster's visible and permitted rows,
  whose quantity at that tier the placement, native-expansion, native-option, third-party and Spider Summoner contracts
  state. Each creature's own spawn expression must agree.
- **Per conversion:** its children must equal that set exactly (missing, duplicate and unexpected children fail), under
  one conversion object, each child holding that conversion's metamagic object, spell level, book, caster and fact.
  Expected-raw rows (the prepared parent's own tier, duplicate keys) must be present raw; unexpected raw rows fail;
  non-summon and ability rows must be retained as a multiset. The aggregate completeness check is kept beside it.
- **Domain controls** (`expanded-summoning.pol06-observer-*`): exact associations pass; crossing d3 and d4+1 while
  keeping the combined set fails both crossed conversions while the aggregate passes; missing, duplicate, unexpected,
  split-conversion, foreign-conversion, left-raw, lost-cure and withdrawn cases fail; controlled counts (Single 1, d3
  forced 2 gives 2, d4 forced 2 gives 3) and count-dice mapping.
- **Counts** come from the contract and a controlled roll (the existing `SpiderDieOverride`), never from the observed
  unit count. Superior Summoning, which adds to the dice rows' rank, must be absent for a controlled dice cast.

### The native lane

`summon-pol06-cotw` (`Sprint39CotwConversionPhase`) runs the cleric phase's own setup and cleanup (fixture
prerequisites, census, party, the spider interface casters, native Default ownership and restoration) with only the
POL-06 cells, so unrelated earlier popups cannot stop them. The full `summon-cleric-alignment` phase still runs every
cell. Session file `scripts/compatibility/loader-compat/sessions/pol06-lane.json`.

**Cases** (overlapping, reusing the existing fixtures and observers):

| Case | Caster and source | What it resolves |
|---|---|---|
| Herald popup, Single cast, stale and forbidden refusals, setting off | fixture cleric 20 (chaotic good, Desna, Spider Summoner), Air Walk prepared with Extend in a level 5 slot | per-conversion projection; Extend; the Herald alignment gate; exhausted-source refusal |
| Herald d4+1 cast, d4 forced to 2 | same | the d4+1 path with a controlled roll |
| Herald with Extend Spell and Heighten Spell | same, Heighten Spell feat added | a second metamagic, with its mask, heighten level and cost observed; tiers I to IV; popup size, reachability and tooltips |
| Druid popup, d3 cast with d3 forced to 2, stale refusal | Druid 13 (true neutral), Freedom of Movement prepared with Extend in a level 5 slot | the d3 path; the second route's exhausted-source refusal |
| Natural Herald, plain source | cleric 9/wizard 1 Herald Caller, Burst of Glory with **no metamagic** in a Common level 5 slot, Extend Spell granted, one wizard spell prepared | row metamagic independent of source metamagic; correct book and one debit with another spellbook populated |

There is no Druid forbidden-alignment case: the Druid route has no alignment gate.

## Checkpoint record

### Focused checks

| Check | Result |
|---|---|
| `Invoke-KmgGate -Filter 'expanded-summoning.pol06'` | 8 of 8 (3 projection controls, 5 observer controls) |
| `-Filter 'expanded-summoning.pol06;expanded-summoning.sprint32-native-admission;cleric-alignment;spider'` | 41 of 41, production compiled |
| `-Filter` `cleanup2-druid`, `cleanup4-druid`, `cleric`, `spider`, `sprint32-native-admission` | 3, 1, 19, 13, 1, all PASS |
| Each candidate's development filter (the above plus `native-profile` and `loader-compat`) | 59 of 59 (3,143 registered); strict standalone package validation PASS |
| `Test-LoaderCompatSafety.ps1` | 13 of 13 |
| `Test-KmgSprint32ReviewPhaseScope` for `summon-pol06-cotw` and `summon-cleric-alignment` | both admitted |
| `git diff --check` | clean |

`validate_expanded_summoning_cleanup2_release164.py` and `..._druid_giant_sizes_release166.py` report "A released
identity row changed" identically on the unchanged base (`acd50a042a`); they are historical release validators.

### Candidates and native sessions

All sessions:
- **Environment:** the disposable game copy (`game-copies/loader-compat`, UMM 0.33.0 owner profile), not the owner's
  installation.
- **Mods:** the candidate, Call of the Wild 1.14.4c-2.1 and Races Unleashed 1.0.11.
- **Harness:** `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089`, unchanged since its build at
  `c638de2385`.
- **Save-free;** each candidate's frozen package was staged unchanged.
- **Gameplay source** (`src/KingmakerGunslinger/Summoning`, `Blueprints`) is identical in every candidate below from
  `0707d39c48e5` on. Only runtime-test files, the phase declaration and the session files differ. The compiled DLLs
  differ (hashes and MVIDs below), so no candidate's DLL identity stands in for another's.

**First observer, full `summon-cleric-alignment` phase** (`sessions/pol06-cleric.json`; from the second session the
pointer was parked at (300, 200) and sampled every 5 s):

| Candidate | Identity | Session | Result |
|---|---|---|---|
| `0707d39c48e5` | package `bd71f5af18f7f89cfbd9134c4ceb91954ace51896a74e8529e791eb7ffd98321`, DLL `607e5d63fdf241585461f8ec55ad55fddafa1e76cd6125654385e53ac81ff591` | `20261120T025710Z-owner-0.33.0-pol06-cleric` | **FAIL**: 20 checks, 6 failed. Popup-teardown guard refusal at the first popup (Summon Monster IX): a tooltip context with no ability and no live trigger. No POL-06 cell ran |
| `0707d39c48e5` (same bytes) | MVID `62b41a56-5051-45b7-bcb8-4a1956522acc` | `20261120T030023Z-owner-0.33.0-pol06-cleric` | **FAIL**: 258 checks, 7 failed (below) |
| `74d01069713a` | package `063c4b332abd6829379b0ef27f4dc794f2c6453abe819efe1af3d0a297f22862`, DLL `2e7e64df6cc6ff672a2756e32fc84d6fa344590e2864654b8bbcdbf4ddae0220`, MVID `9c18590d-3051-49a5-bb5a-3ba658dea275` | `20261120T031613Z-owner-0.33.0-pol06-cleric` | **FAIL in game, 259 of 260.** The one failure is the stale-choice precondition (below) |
| `52ec0e144b6c` | package `00ac7d5da27a1f358bc8dbd93c1dea57a4afc423e03dcf9b98e89c9e238fb55f`, DLL `1dd95d9b1f3a883846944fcebd27b34fa93fa21c774733e347e834694deffd8a`, MVID `f6b9ce92-3eb0-490a-bd81-159b2d45f2c4` | `20261120T033139Z-owner-0.33.0-pol06-cleric` | **FAIL**: 202 checks, 5 failed. The same guard refusal at the pre-existing `spider-extended` popup, before any POL-06 cell |

Failures of `20261120T030023Z`: the guard refusal in a pre-existing Spider Summoner cell (popup-close admission,
`r6-spider-hold-original`, `spider-held-attempt-original-crab`), and a fixture defect corrected in `74d01069`: the
Herald book held two equal extended preparations, and the native spend took the other one (exactly one level 5 slot,
not the pinned one).

**Stale-choice verdict of `20261120T031613Z`, re-read from retained evidence (policy rule 5):** the in-game check
required `IsAvailableForCast` false, which never reads slots. The retained observation (visible, available, native
reason "No suitable spell slots", Fail, nothing spent or summoned) meets the slot-exhausted precondition. In-game
**FAIL** and re-read **PASS** are both kept. The re-read is not an execution of the corrected observer; that
observer (`slotExhausted`) executed natively in the lane sessions below.

**The guard refusals** happened in three different popups that predate this story; the guard was not weakened. They
are recorded as **popup-guard aborts with suspected cursor interference, a hypothesis**, not an established cause.
The 5 s samples of those sessions sat mostly at (640, 360) or (0, 0). In the first lane session, a driver re-parking
every 50 ms read (0, 0) from its first sample, before the game window existed, and did not check whether the read
succeeded, so those readings do not attribute anything. From the second lane session the driver checked every read:
no failed read and no displacement in about 3,000 samples per session. No matched baseline was run, so no attribution
is claimed.

**Corrected observer, `summon-pol06-cotw` lane** (`sessions/pol06-lane.json`). The pointer was owned by a
scratch-area driver that re-parks it at (300, 200) every 50 ms and records every displacement and failed read:

| Candidate | Identity | Session | Result |
|---|---|---|---|
| `2bc96a6ef6f2` | package `f135117fa4361b5e36d0c5bb705578c519c77c7040c12c4428ecbb4f981e5f23`, DLL `8584e31d3a9af3bd71566b5530c138dc835e0e667c94a3cb6ac2ae1026abdf00`, MVID `42d928c5-a5a8-457e-9275-b3b33fb29f13` | `20261120T050759Z-owner-0.33.0-pol06-lane` | **FAIL**: 10 checks, 2 failed. Lane defect: the phase entered native Default with the fixed purpose `summon-cleric-alignment`, which admission refused for the lane. No POL-06 cell ran. Corrected in `43f69cfede` |
| `43f69cfede6b` | package `5ea7f480d406bc5933310bd841beddd4084e7de17f5920f30d2a23d1d9f2b9be`, DLL `1b33889fe6e3f6fa85508a015d56cf0ddd054ee3c0531e4e6b09dfe4d7fc58e5`, MVID `501a8e00-36ef-4d0f-84cf-d8219c25741c` | `20261120T051940Z-owner-0.33.0-pol06-lane` | **FAIL**: 75 checks, 9 failed. Every POL-06 cell ran; every cast, controlled count, debit, refusal, reachability and tooltip check passed. Observer defects: Call of the Wild's builder namespace (the per-conversion verdicts had no expectation); tier I conversions held as the Summon Monster I parent were not classified; the presentation check compared reading only; the natural Herald's source went into its domain slot (which the native conversion and the product rightly leave alone). Corrected in `41a1d3487d` |
| `41a1d3487def` | package `585633439ead7d36140757ba1785ea1ecfb2c72e587418be273077cb001c5364`, DLL `fa21d1c6a009e41dc6e5b1378f2a3923e7f81242fe9e70a3cf42b6835f719110`, MVID `59be4b64-d99e-4417-a95c-0d2c22baf740` | `20261120T053352Z-owner-0.33.0-pol06-lane` | **FAIL**: 74 checks, 1 failed. Every per-conversion verdict passed. The observer refused the natural Herald's Single cast because that naturally levelled caster holds Superior Summoning, which only affects dice rows. Corrected in `c5fdcb1431` |
| `c5fdcb1431c2` | package `2fd49eaadb1ca6d4a1b873a563b9e3684707d4e2f1cff684cc782c8b24bebb72`, DLL `bff80052bfb76d7d29731946a85c1141ea604d014220564431e7e2b429e97645`, MVID `6d4e9a1b-2ae4-441f-b739-924dc535d0bb` | `20261120T054533Z-owner-0.33.0-pol06-lane` | **PASS, 77 of 77 in game.** Every POL-06 cell reached and completed |

### Native observations (session `20261120T054533Z`, candidate `c5fdcb1431c2`)

**Herald, fixture cleric 20, Air Walk prepared with Extend in a level 5 slot (its only equal preparation).**
- **Popup:** 68 ordinary Summon Monster V rows exact; Call of the Wild's three Summon Monster IV Extend conversions
  each owned exactly its own permitted creatures (Single 27, d3 11, d4+1 20), each child with that conversion's
  metamagic object at spell level 5; none raw; aggregate complete.
- **Single cast:** Black Widow, one creature, Extend, 240 s (40 rounds at caster level 20), exactly the source slot.
- **Stale:** a retained choice of the same conversion after the slot was spent: cast count 0, refused, nothing spent
  or summoned. **Forbidden:** a chaotic evil creature under the same conversion: not visible, unavailable, refused,
  nothing spent or summoned.
- **Setting off:** exactly the 68 ordinary rows.
- **d4+1 cast:** Ant, Giant (Worker) (1d4+1), d4 forced to 2 (one d4 roll observed): three creatures, each Extend,
  240 s, exactly the source slot.
- **Extend Spell and Heighten Spell:** 302 rows; 18 conversions at tiers I to IV (Extend; Heighten +1 to +4; Extend
  with Heighten +1 to +3), each owning exactly its own creatures; no indistinguishable rows; eight redundant pairs
  (above). Heighten cast: Ant, Giant (Drone) under the tier IV Heighten +1 conversion: mask Heighten, heighten level
  1, level cost 1, `AbilityData.SpellLevel` 5; the summon rule carried Heighten, context spell level 5, not extended,
  120 s; one creature; exactly the source slot.

**Druid 13, Freedom of Movement prepared with Extend in a level 5 slot.**
- **Popup:** 76 ordinary Summon Nature's Ally V rows exact; three Extend conversions each owned exactly its own
  creatures (Single 32, d3 12, d4+1 20).
- **d3 cast:** Ant, Giant (Soldier) (1d3), d3 forced to 2 (one d3 roll observed): two creatures, each Extend, 156 s
  (26 rounds at caster level 13), exactly the source slot.
- **Stale:** a retained choice of the same conversion after the slot was spent: cast count 0, refused, nothing spent
  or summoned.

**Natural cleric 9/wizard 1 Herald, Burst of Glory with no metamagic in a Common level 5 slot.**
- **Popup:** 68 ordinary rows exact; the cure rows retained exactly; three Summon Monster IV **Extend** conversions,
  although the source has no metamagic, each owning exactly its own creatures.
- **Cast:** Ant, Giant (Drone), one creature, Extend, 108 s (18 rounds at caster level 9), from the cleric book;
  exactly the cleric source slot spent; every wizard slot, preparation and availability unchanged (one wizard spell
  prepared).

### Evidence verification after acceptance (immutable session records, read 2026-11-20)

This is a reading of the retained `20261120T054533Z` records (`01-lane/product/sprint32-review.json` and
`runtime-summary.txt`), not part of the original in-game verdict, which stays **PASS, 77 of 77** as executed. The
frozen observer was not changed.

- **Dice events.** The controlled-roll handler (`SpiderDieOverride`) records each of the caster's `RuleRollDice`
  events of the forced die as its formula (`<rolls>x<die>`), not its result, and overrides the first such event to 2.
  - Herald d4+1 cast (`pol06-herald-d4-count-duration-debit`): `rolls` = `["1xD4"]`, `forcedDie` 2, `expectedCount` 3,
    `summoned` 3, three summon rules.
  - Druid d3 cast (`pol06-druid-d3-count-duration-debit`): `rolls` = `["1xD3"]`, `forcedDie` 2, `expectedCount` 2,
    `summoned` 2, two summon rules.
  - Single casts (Herald Black Widow, Heighten, natural Herald): `rolls` = `null` (no override installed).
  - The rolled result after the override is not retained; that it was 2 rests on the override and the matching
    counts (2 + 1 and 2). The in-game check required at least one recorded event (`Count >= 1`); exactly one was
    recorded in each dice cast.
- **Heighten context level.** In `pol06-herald-second-metamagic-cast` the summon rule recorded from the cast context
  (`rules[0]`) carried `metamagic` Heighten, `spellLevel` **5** (`Params.SpellLevel`), `extended` false and 120 s. The
  row's `childSpellLevel` 5 is `AbilityData.SpellLevel`, the slot-level property, recorded separately; `heightenLevel`
  1 and `spellLevelCost` 1. For comparison, every Extend cast's rule recorded `spellLevel` 4 with `childSpellLevel` 5.
- **Presentation records:** `pol06-two-feats-rows-reachable` lists exactly the three target rows above;
  `pol06-two-feats-extend-tooltip` and `pol06-two-feats-second-tooltip` are the two rendered tooltips;
  `pol06-two-feats-presentation-distinguishable` records `behaviourKey` "spawned unit and count dice, metamagic mask,
  effective spell level" while its code reads `AbilityData.SpellLevel` (the discrepancy noted above).
- **Verdict count:** `runtime-summary.txt` has 77 PASS lines and no FAIL line: the 76 review cells and
  `expanded-summoning-native-profile-restored`.
- No retained value contradicts the reported casts, counts, durations or debits.

### The debit contract, as established

- **Established:** with exactly one equal prepared copy, a projected Call of the Wild choice spends exactly that source
  slot, once, in the correct book, with every other slot, preparation, known spell, custom spell and resource
  unchanged, including a second populated spellbook.
- **Not claimed:** that a conversion always spends the slot whose popup was opened. With two equal preparations the
  native `Spellbook.Spend` may spend the other (observed in `20261120T030023Z`). The product preserves this native
  selection: one native debit of an equal preparation, within the correct book, spell, metamagic and level, and no
  unrelated spend. It does not override native slot selection.

### Limits

- **Not executed natively:**
  - the full `summon-cleric-alignment` phase with the corrected observer (its non-POL-06 cells are unchanged code; the
    lane runs the same POL-06 cells);
  - Reach, Empower, Quicken and other metamagic beyond Extend and Heighten (each a different conversion key handled by
    the same per-conversion verdict);
  - a projected d3 Herald cast and a projected d4+1 Druid cast (one of each quantity path per route family was cast);
  - controller presentation;
  - the owner installation and the full 15-mod stack.
- **Scope boundaries, not untested routes:** spontaneous casters (prepared slots only, as before); a Druid alignment gate
  (none exists).
- **Same-tier fallback:** Call of the Wild rows of the prepared parent's own tier stay raw as a conservative
  presentation fallback (see Implementation); the extended Summon Monster V popup in the full phase exercises it.
- **Presentation:** the owner's review of the measurements above remains open. Reachability was checked for three
  rows and tooltips rendered for two; the all-row comparison is a computed key, not proof of behavioural equivalence.
