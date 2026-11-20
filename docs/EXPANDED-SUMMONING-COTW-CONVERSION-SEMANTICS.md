# Call of the Wild conversion semantics (POL-06 / S5 T5)

Status: **implemented on a development branch; native qualification recorded below.** Not merged, not released.

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

Results: see the checkpoint record below.
