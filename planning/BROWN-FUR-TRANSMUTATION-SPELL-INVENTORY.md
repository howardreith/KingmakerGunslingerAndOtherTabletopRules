# Brown-Fur Transmutation Spell Inventory

The installed-spell classification remains authoritative for the `0.0.82`
repair. The repair changes pre-command targeting and player intent presentation,
not spell classification. Beast Shape II, Undead Anatomy I, and Resinous Skin
remain supported inventory rows whose repaired ally-target execution requires
fresh guarded runtime qualification.

Status: authoritative installed inventory captured and deterministically
classified; `Unexplained = 0`. Every separate ownership, interruption, dispel,
persistence, and publication gate now passes on the immutable pre-human
candidate. Human acceptance remains pending.

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

## Classified capture

The fail-closed classifier and complete installed graph passed all six guarded
assertions on source commit
`b707fd5c3aa8773fb16ae3a3f968de1ab529b766`. The result contains the same
`86` roots and `177` recursively expanded records as the investigation capture,
with `174` supported by generic contract, `3` supported by a true Brown-Fur
named adapter, and no `Unexplained`, engine-blocked, or wholly ineligible row.
Dimension-level ineligibility remains explicit: `77` non-Personal records are
ineligible for Share Transmutation and `65` records without a positive
ability-score carrier are ineligible for Powerful Change. All genuine
Transmutations still have an explicit Transmutation Supremacy decision.

| Evidence | Value |
| --- | --- |
| Scenario | `observe-brown-fur-transmutation-inventory` |
| Result | `PASS` |
| Run | `20260815T1951517164731Z-f2988c8a3fac4d60bcbabe3e3432bc0c` |
| Package SHA-256 | `60505729A0846A9641FDC1A5D25FA4E4C93B57814223C8AECB546CB3350BA638` |
| DLL / installed DLL SHA-256 | `B4B48DAA9C7AE6BD9A963C142D6435DAA3B1B388F0DA0D0B0E34CF0F9694CF91` |
| DLL MVID | `f041926b-a2c6-4db5-aeae-95ca51ced190` |
| Classified inventory SHA-256 | `82AE0FFA8313DA14B173A61430F47308320849436AC51CB001EFBF5CB3238A7D` |
| Runtime result SHA-256 | `0C2C095001150D6218FB7C8839F59D11D7B2F82F41E3B05041552AD50F0BAAD7` |
| Runtime evidence SHA-256 | `560DE55E6C7D99C6A7228DBF1C616A02E5D3AB92030D9B354CD6CF514792EC9C` |
| Orchestration SHA-256 | `16098FF1DB97499DB12EB1D9616D588B7B2FB293E23C48111392D4F95D2A2C2C` |
| Deployment | `20260815T1951516057686Z` |
| Backup | `20260815T1951471610464Z` |
| Feature settings SHA-256 | `5B6030AE888F6B127FF23CA03E49578F304DC107ED65E3B2D5F8C8D3D177665E` |
| Game build collection | `2018.4.10.10503941` |

The guarded Steam App ID 640820 launch was save-free and recorded no save
interaction. A prior run of the immediately preceding classifier commit
finished PASS three seconds after the default orchestration deadline; because
orchestration recorded ERROR, it was not counted. The final run used an
explicit 180-second guarded timeout and completed normally.

The authoritative source is the resolved CotW Arcanist casting spell list in the
installed Kingmaker process. The guarded, save-free scenario
`observe-brown-fur-transmutation-inventory` enumerates every genuine
Transmutation root and recursively follows every `AbilityVariants` relationship.
It records spell levels, source spellbook, range and target flags, duration,
metamagic, component/action graphs, applied buffs, bonus carriers, modifier
descriptors, context/static values, polymorph and size components, hard-coded
caster routing, and save/dispel presentation.

The original authoritative capture deliberately labeled every record
`Unexplained`. The pure classification policy now places each installed entry
into exactly one of:

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

## Classification policy and remaining gates

The structured artifact contains one complete record for every one of the
`177` roots and recursively expanded variants. Each row records its canonical
GUID, parent and variants, spell levels and spellbook, targeting, duration and
explicit Extend support, action/buff graph, bonus carriers and descriptors,
polymorph/size and caster-routing hazards, compatibility decisions, required
adapter, and final qualification. The artifact is hash-bound above rather than
committed as a raw runtime dump.

Current classification totals are:

| Classification | Count |
| --- | ---: |
| Supported by generic contract | 174 |
| Supported by named adapter | 3 |
| Intentionally ineligible | 0 overall; 77 Share and 65 Powerful dimension decisions |
| Blocked by an understood engine limitation | 0 |
| Unexplained | 0 |

The three named Supremacy adapters are Resonating Word
`df7d13c967bce6a40bec3ba7c9f0e64c`, Obsidian Flow
`e48638596c955a74c8a32dbc90b518c1`, and Earth Tremor line
`91266b6d2a4c4fd6b8e1549bc2381d12`. Earth Tremor cone and spread are
separately recognized exact installed CotW-native paths; they honor the scoped
Extend context without a Brown-Fur duration adapter and therefore remain
generic classifications.

The inventory classification gate is closed. Runtime `ConvertedFrom`
canonicalization, actual player ownership/intent, representative restricted
Personal-spell delivery, post-submission interruption, dispel, and persistence
remain separate player-facing publication gates.
