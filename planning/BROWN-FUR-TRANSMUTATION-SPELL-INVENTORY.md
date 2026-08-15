# Brown-Fur Transmutation Spell Inventory

Status: authoritative investigation capture complete; mechanical classification
and adapter qualification remain in progress.

## Authoritative capture

The guarded, save-free inventory scenario passed on source commit
`ba54040eb505ddc20593a3503409b34235d03399` with the supported CotW subject
documented in `BROWN-FUR-COTW-CONTRACT.md`.

| Evidence | Value |
| --- | --- |
| Scenario | `observe-brown-fur-transmutation-inventory` |
| Result | `PASS` |
| Package SHA-256 | `E75F56C5960EA29BA73067DAB73B634FCC12E00C55DE051C973E908200FDFD9F` |
| DLL SHA-256 | `A344F6F0BC9067B4EDD4D2CFF1CB2B2402771BF3D2FCEB79E90BECBD22A9B18C` |
| DLL MVID | `1fd71b54-f18c-4dce-a0e3-5636729da02b` |
| Inventory SHA-256 | `A7A0CAD8336A1EC006571A5A82086E67EC30E6D22F7E5BB137DA21DBC3A46B17` |
| Root Transmutation spells | `86` |
| Root and recursively expanded variant records | `177` |
| Personal-range records | `100` |
| Positive ability-bonus candidates | `112` |
| Explicit hard-coded `ToCaster` records | `0` |

The inventory artifact is intentionally retained as runtime evidence rather
than committed as a raw, machine-local dump. The hashes above bind this report
to the exact immutable source and installed DLL used for the capture.

The authoritative source is the resolved CotW Arcanist casting spell list in the
installed Kingmaker process. The guarded, save-free scenario
`observe-brown-fur-transmutation-inventory` enumerates every genuine
Transmutation root and recursively follows every `AbilityVariants` relationship.
It records spell levels, source spellbook, range and target flags, duration,
metamagic, component/action graphs, applied buffs, bonus carriers, modifier
descriptors, context/static values, polymorph and size components, hard-coded
caster routing, and save/dispel presentation.

The authoritative capture deliberately labels every record `Unexplained`. That
is an investigation state, not a release classification. Brown-Fur blueprint
publication remains absent while any entry is `Unexplained`. Each entry will be
curated into exactly one of:

- Supported by generic contract
- Supported by named adapter
- Intentionally ineligible
- Blocked by an understood engine limitation
- Unexplained

Converted-from relationships live on per-cast `AbilityData`, not solely on the
shared blueprint. The inventory records this runtime boundary; variant and
converted-chain cast fixtures must supplement the static blueprint graph before
final qualification.

## Observed inventory shape

| Property | Counts |
| --- | --- |
| Range | Personal `100`; Touch `46`; Close `21`; Medium `6`; Unlimited `2`; Projectile `2` |
| Duration | one minute/level `139`; one round/level `15`; ten minutes/level `7`; blank or instantaneous `7`; one hour/level `5`; three rounds `1`; one hour `1`; Permanent `1`; Time Stop apparent-time duration `1` |
| Applied buffs per record | zero `23`; one `106`; two `14`; three `20`; eight `14` |

The `112` positive ability-bonus candidates use these exact component-family
combinations:

| Positive carrier family | Records |
| --- | ---: |
| `Polymorph` | 78 |
| `AddStatBonus` | 14 |
| `AddGenericStatBonus` + `ChangeUnitSize` | 10 |
| `AddContextStatBonus` | 4 |
| `AddStatBonus` + `ChangeUnitSize` | 4 |
| `AddStatBonusAbilityValue` | 1 |
| `AddContextStatBonus` + `ChangeUnitSize` | 1 |

The other `65` records have no positive Strength, Dexterity, Constitution,
Intelligence, Wisdom, or Charisma bonus carrier and will be intentionally
ineligible for Powerful Change unless later per-cast evidence proves a carrier
that the static graph cannot see. Baleful Polymorph is correctly excluded from
the positive-bonus candidate set. Enlarge and Reduce Person families are
included through their nested conditional action-applied buffs.

No Personal-range record exposes a static `ToCaster` action flag. This removes
one known static hazard but does not prove Share Transmutation: actual target
redirection, willingness, delivery, and effect ownership remain subject to
per-cast qualification.

## Remaining classification gate

Before publication, the curated inventory must include one row for every one of
the `177` records, including parents, variants, and runtime `ConvertedFrom`
fixtures. Each positive carrier family needs descriptor-preserving stacking,
recast, dispel, expiration, and save/reload proof. Each Personal spell needs a
generic or named Share adapter decision. Numeric metamagic flags captured by the
engine must also be converted into an explicit Extend-eligibility decision.

Current classification totals are therefore:

| Classification | Count |
| --- | ---: |
| Supported by generic contract | 0 |
| Supported by named adapter | 0 |
| Intentionally ineligible | 0 |
| Blocked by an understood engine limitation | 0 |
| Unexplained | 177 |
