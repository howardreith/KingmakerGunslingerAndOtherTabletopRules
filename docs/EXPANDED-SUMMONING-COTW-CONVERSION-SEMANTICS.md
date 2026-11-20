# Call of the Wild conversion semantics (POL-06 / S5 T5)

Status: **implemented on a development branch; native observations recorded below, with an environmental limit on the final observer.** Not merged, not released.

- **Branch:** `codex/pol06-cotw-conversion-semantics`, draft PR #190, stacked on PR #189 (accepted S5 T4 head
  `acd50a042a`).
- **Epic story:** S5 T5, inventory record POL-06: "Metamagic conversion popup includes raw base-level conversion rows".

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
- **Debit:** `SpendFromSpellbook` and availability walk `ConvertedFrom` to the source's slot.
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
| Spell level | `L`, the source's level, through `ConvertedFrom` |
| Debit | the source's slot, through `ConvertedFrom` |
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
- a row whose tier is the prepared parent itself: its children would repeat the source's own listed variants, with
  the same metamagic, level and debit;
- two rows with the same blueprint and the same metamagic;
- any row whose projection cannot be verified exactly. Such a row never withdraws the ordinary projection.

**Order.** Additional children follow every ordinary child, in Call of the Wild's own row order. Within a row they keep
the published parent's order (`DruidSummonConversionPolicy.ProjectedRank`).

**Unchanged:**
- ordinary rows and their projection;
- cure, inflict, ability and third-party rows;
- the behaviour with the setting off;
- Call of the Wild itself, and every identity.

### Presentation boundary for owner review

- **Creature icons:** projected children show the creature's icon, like every projected row. Their metamagic is
  visible in the tooltip's Metamagic block, as for Call of the Wild's raw rows.
- **The same creature twice:** a creature can appear once as an ordinary choice (higher tier, no metamagic) and once
  as an additional choice (lower tier, with metamagic). Both are cast at the source's level. Only the tooltip
  distinguishes them, by its metamagic block and by the creature's quantity label.
- **Menu size:** the popup grows by the permitted creatures of each additional row. The count multiplies with the
  caster's metamagic feats that summon spells accept (for example Extend, Reach, Quicken, and Heighten at every lower
  level), because Call of the Wild makes one row per combination.
- **Alternative, if the owner prefers it:** keep Call of the Wild's rows raw and label them with their tier and
  metamagic. Hiding them, or changing their power, would need an explicit product decision; neither was done.

## Validation

**Focused development checks:**
- production compilation;
- domain controls `expanded-summoning.pol06-*` (additional-level recognition, ordering, in-place projection beside
  unprojected rows) and the existing conversion, cleric-alignment and spider selections.

**Native qualification:** the `summon-cleric-alignment` phase on the combined DATA profile, extended with POL-06 cells
(`RuntimeTestRunner.Pol06CotwConversions.cs`):
- **Herald popup** over an unrelated level 4 spell prepared with Extend:
  - ordinary Summon Monster V rows exact;
  - Call of the Wild's Summon Monster IV rows projected into exactly the caster's permitted Summon Monster IV
    creatures, each with the row's Extend at spell level 5;
  - none raw.
- **Projected Herald cast** (the Black Widow where offered):
  - Extend's doubled duration from the engine's summon rule;
  - the quantity row's count;
  - the level 5 slot spent exactly once.
- **Refusals with nothing spent:**
  - a retained projected choice after its slot was spent;
  - a forbidden-alignment creature under the same row.
- **Setting off** (Call of the Wild's in-memory value, restored; settings file untouched): ordinary rows only.
- **Druid popup and cast:** the same projection for Summon Nature's Ally IV.
- **Unchanged cells, still in the phase:**
  - the Herald Ogre from the extended source: ordinary conversion, no Extend;
  - the extended Summon Monster V popup, whose same-tier Call of the Wild rows stay raw;
  - the natural cleric/wizard Herald with its cure rows and unchanged wizard book;
  - the Druid spider popups.

## Checkpoint record

### Focused checks

| Check | Result | Time |
|---|---|---|
| `Invoke-KmgGate -Filter 'expanded-summoning.cleanup2-druid;expanded-summoning.cleanup4-druid;expanded-summoning.pol06;cleric-alignment;spider;native-profile'` | 48 of 48 (3,138 registered), production compiled | 15 s |
| `-Filter 'expanded-summoning.pol06'` | 3 of 3 | — |
| Each candidate's development filter (above plus `loader-compat`) | 53 of 53; strict standalone package validation PASS | 400–418 s per build |
| `git diff --check`; icon catalog | clean; PASS | — |

`validate_expanded_summoning_cleanup2_release164.py` and `..._druid_giant_sizes_release166.py` list the two
conversion files. Both report "A released identity row changed" identically on the unchanged base (`acd50a042a`), so
they are historical release validators and say nothing about this change.

### Candidates and native sessions

All sessions:
- **Environment:** the disposable game copy (`game-copies/loader-compat`, UMM 0.33.0 owner profile), not the owner's
  installation.
- **Mods:** the candidate, Call of the Wild 1.14.4c-2.1 and Races Unleashed 1.0.11.
- **Harness:** `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089`, unchanged since its build at
  `c638de2385`.
- **Session file:** `scripts/compatibility/loader-compat/sessions/pol06-cleric.json`, phase `summon-cleric-alignment`,
  save-free.
- **Input:** from the second session on, the pointer was parked at (300, 200) and sampled every 5 s.

| Candidate | Identity | Session | Result |
|---|---|---|---|
| `0707d39c48e5` | package `bd71f5af18f7f89cfbd9134c4ceb91954ace51896a74e8529e791eb7ffd98321`, DLL `607e5d63fdf241585461f8ec55ad55fddafa1e76cd6125654385e53ac81ff591` (in game) | `20261120T025710Z-owner-0.33.0-pol06-cleric` | **FAIL**, environment: 20 checks, 6 failed. The popup-teardown guard refused at the first popup (Summon Monster IX), because of a tooltip context with no ability and no live trigger. Pointer not controlled. No POL-06 cell ran |
| `0707d39c48e5` (same bytes) | as above, MVID `62b41a56-5051-45b7-bcb8-4a1956522acc` (in game) | `20261120T030023Z-owner-0.33.0-pol06-cleric` | **FAIL**: 258 checks, 7 failed. See the table below |
| `74d01069713a` (observer fix 1) | package `063c4b332abd6829379b0ef27f4dc794f2c6453abe819efe1af3d0a297f22862`, DLL `2e7e64df6cc6ff672a2756e32fc84d6fa344590e2864654b8bbcdbf4ddae0220`, MVID `9c18590d-3051-49a5-bb5a-3ba658dea275` (in game) | `20261120T031613Z-owner-0.33.0-pol06-cleric` | **FAIL in game, 259 of 260**. The one failure is the stale-choice check's wrong precondition (below). Every other check passes, including every POL-06 behaviour and every pre-existing cleric, Herald, Spider Summoner and Druid cell |
| `52ec0e144b6c` (observer fix 2) | package `00ac7d5da27a1f358bc8dbd93c1dea57a4afc423e03dcf9b98e89c9e238fb55f`, DLL `1dd95d9b1f3a883846944fcebd27b34fa93fa21c774733e347e834694deffd8a`, MVID `f6b9ce92-3eb0-490a-bd81-159b2d45f2c4` (in game) | `20261120T033139Z-owner-0.33.0-pol06-cleric` | **FAIL**, environment: 202 checks, 5 failed. The same teardown-guard refusal, at the pre-existing `spider-extended` popup, aborted the spider interface cells before any POL-06 cell ran |

**Failures of the second session (`20261120T030023Z`):**

| Failure | Cause |
|---|---|
| Popup-close admission, `r6-spider-hold-original`, `spider-held-attempt-original-crab` | the same teardown-guard refusal, in a pre-existing Spider Summoner cell |
| `...-pol06-herald-additional-cast-debit`, `r6-pol06-herald-additional-cast`, `pol06-herald-additional-extend-duration-debit` (debit part only), `pol06-herald-stale-choice-refused` | fixture defect, corrected in `74d01069` |

The fixture defect, in detail:
- The Herald book held two identical extended Freedom of Movement copies.
- The native `Spellbook.Spend` spends the last available memorized slot whose spell equals the source. It spent the
  other copy: exactly one level 5 slot, but not the pinned one.
- The retained choice then legitimately spent the pinned slot.
- The cast itself was correct: Extend, 240 s, the exact unit.

**The guard refusals.** All three are in popups that predate this story, at three different places, and the guard
was not weakened. In every controlled session, most pointer samples sit at the game window's centre (640, 360) or
at (0, 0), not where it was parked. The game repositions the cursor on its 1280×720 screen, where the popups render.
This is recorded as an environmental cause, not a proven one. The earlier cleric phase passed 234/234 on the live
combined installation (`20261111T0045385494739Z`).

**Stale-choice verdict, re-read from retained evidence (policy rule 5):**
- **In game (session `20261120T031613Z`):** the check required native `IsAvailableForCast` to be false.
- **Why that was wrong:** that getter never reads spell slots. Slot exhaustion shows only in
  `GetAvailableForCastCount`, `GetUnavailableReason` and execution.
- **The retained observation:** visible, `IsAvailableForCast` true, native reason "No suitable spell slots", command
  result Fail, nothing spent, no spawn action, nothing summoned. That reason comes from `GetUnavailableReason` through
  the conversion chain, which reports it only when `Spellbook.CanSpend(source)` is false; that is the slot-exhausted
  precondition.
- **Verdicts:** in-game FAIL; re-read PASS. Both are kept.
- **The corrected observer** (`52ec0e144b`, `slotExhausted`) has not executed natively: its only session aborted on
  the guard first.
- **Production code is identical** in `74d01069713a` and `52ec0e144b6c`; only two runtime-test files differ.

### Native observations (session `20261120T031613Z`, candidate `74d01069713a`)

**Herald, chaotic good under Desna, caster level 20, holding only Extend Spell.** The source is Air Walk (level 4)
prepared with Extend in a level 5 slot, its only equal prepared copy.
- **Popup:**
  - the 68 ordinary Summon Monster V rows, exactly this caster's permitted list, in published order;
  - then 58 projected children from Call of the Wild's three Summon Monster IV rows (Single, 1d3, 1d4+1), exactly the
    permitted Summon Monster IV creatures, each once;
  - every child carries that row's own Extend object at spell level 5, with a current receipt for additional tier IV;
  - no raw row remained (`additional=3;additionalChildren=58;additionalRaw=0`);
  - the five forbidden Summon Monster IV creatures were not offered.
- **Projected cast**, Summon Monster IV Black Widow (Spider Summoner), through the native row click, target and
  command:
  - one Giant Black Widow;
  - the engine's summon rule shows Extend, 240 s, which is 40 rounds at caster level 20;
  - exactly the source's level 5 slot was spent.
- **Stale choice:** a retained choice from the same row after that slot was spent was refused (above).
- **Forbidden choice:** a Dretch (1d3), chaotic evil, under the same Extend row was refused before any spend (not
  visible, unavailable, Fail, nothing spent or summoned).
- **Setting off** (Call of the Wild's in-memory value; its settings file untouched; restored afterwards): the same
  popup listed exactly the 68 ordinary rows, with no additional row (`additional=0`).
- **The ordinary conversion is unchanged:** the Ogre converted from the extended Freedom of Movement source had no
  Extend, 120 s, at caster level 20, as before.
- **Same-tier rows stay raw:** in the extended Summon Monster V popup (level 6 slot), Call of the Wild's same-tier
  Summon Monster V rows stayed raw and were set aside. The natural cleric 9 / wizard 1 Herald still cast from its
  cleric book with its wizard book unchanged.

**Druid, true neutral, caster level 13, holding the feat and Extend Spell.** The source is Freedom of Movement
prepared with Extend in a level 5 slot, its only equal copy.
- **Popup:** the 76 ordinary Summon Nature's Ally V rows exact, then 64 projected Summon Nature's Ally IV children from
  the three rows, each with Extend at spell level 5; none raw.
- **Projected cast:** Giant Ant (Drone), Summon Nature's Ally IV Single. Extend, 156 s (26 rounds at caster level 13),
  exactly the source slot spent.

### Limits

**Not executed natively:**
- the final observer (`52ec0e144b`);
- the Druid stale and forbidden cases (the Druid has no alignment gate; only the Herald stale case ran);
- quantities above Single in a projected cast (1d3 and 1d4+1 children were listed and verified, not cast);
- metamagic other than Extend, and combinations, including Heighten;
- spontaneous casters' Call of the Wild rows (the projection handles prepared slots only, as before);
- controller presentation;
- the live owner installation.

**Not in this story:** the full stack of 15 mods, and the other phases that open conversion popups over metamagic
sources (alphabetical menus, menu order).

**Presentation:** the presentation boundary above is for owner review. In these sessions the Herald popup grew from 68
to 126 rows and the Druid popup from 76 to 140.
