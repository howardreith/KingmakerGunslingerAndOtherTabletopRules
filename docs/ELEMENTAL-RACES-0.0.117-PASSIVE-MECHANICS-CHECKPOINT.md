# Elemental alternate traits 0.0.117: first passive-mechanics checkpoint

## Scope and status

INCREMENTAL MECHANICS PASS, NOT RELEASE C PASS.

Eight of the required 21 traits have executable mechanics and focused native
proof. Thirteen mechanics, trait-bearing persistence, direct legacy receiver
migration, and final release-wide gates remain required. The previously
qualified retain-base persistence does not qualify these new trait facts.

This checkpoint follows
`fa7900289286bc326014057c275de97a30b7d1ae` on
`codex/elemental-races-expansion`. The mission starting master remains
`6874dc15a27ded132456dbdd480f47c794543a05`.
The implementation checkpoint is
`530eff1ebe6814fc17a5fc39c1ac50bb215bfbbf`; the tested artifact truthfully
embeds the preceding framework commit and exact dirty-source fingerprint,
not this later documentation/commit identity. The mandatory post-commit push
refused the branch because the external allowlist still lacks it.

## Implemented inventory

| Trait | Replaces | Native behavior proved |
| --- | --- | --- |
| Wildfire Heart | Energy Resistance | +4 Racial initiative, exact owned modifier, removal |
| Brazen Flame | Energy Resistance and Racial SLA | One +1 fire packet on successful manufactured, unarmed, and natural melee attacks; ranged/spell/SLA exclusion; damage-event replay; Scorching interaction |
| Forge-Hardened | Racial SLA | One +2 Racial save modifier for Fatigue/Exhausted and parent variants; actual Acadamae fatigue context; Craft clause omitted |
| Granite Skin | Energy Resistance | +1 NaturalArmor modifier and exact removal |
| Like the Wind | Energy Resistance | +5 feet native speed; Haste, Slow condition, difficult-terrain condition, class movement, armor, encumbrance, removal |
| Secretive | Energy Resistance and Racial SLA | One +2 Racial save modifier for Enchantment/Divination and parent variants; overlap never doubles the trait |
| Thunderous Resilience | Energy Resistance | Native sonic resistance 5; original electricity resistance removed and restored on trait removal |
| Whispering Wind | Racial SLA | +4 Racial Stealth through a native skill check and exact removal |

Rules authority: [Ifrit alternate traits](https://www.aonprd.com/RacesDisplay.aspx?ItemName=Ifrit),
[Oread alternate traits](https://www.aonprd.com/RacesDisplay.aspx?ItemName=Oread),
and [Sylph alternate traits](https://www.aonprd.com/RacesDisplay.aspx?ItemName=Sylph).
No Craft replacement skill, extra bonus, custom visual, optional-mod dependency,
module toggle, settings schema, or new manifest identity was added.

Native stat/resistance components and feature-local save/damage events implement
the slice. The new scenario remains feature-specific; no trait mechanics were
added to the central runtime runner.

## Engineering findings and exact limitations

- A failing native regression showed that a later-acquired Brazen Flame could
  add fire after Scorching Weapons had checked the damage bundle. The owned
  enchantment now rechecks after all preparation callbacks and removes only
  its exact contributed packet when another fire packet is present.
  Installed `RuleDealDamage.OnTrigger` IL constructs `RuleCalculateDamage`
  only after `RulePrepareDamage` finishes. Both acquisition orders pass with
  and without Inner Flame. This preserves the existing Scorching policy that
  another weapon-fire effect suppresses its contribution; it never removes a
  foreign packet. See the [printed nonstacking clause](https://www.aonprd.com/FeatDisplay.aspx?ItemName=Scorching%20Weapons).
- The first replay fixture removed only a buff marker. That is not complete
  cleanup of an unexpired save-hydratable Scorching snapshot. The fixture now
  explicitly ends the owned activation before replay and reports every
  command/process/enchantment conjunct. Production hydration is unchanged.
- Kingmaker's installed `ModifiableValue.DefaultStackingDescriptors`
  explicitly includes `Racial` and `NaturalArmor`. The slice uses those
  requested native descriptors. Independent Racial modifiers therefore stack
  in the engine; each trait itself contributes once. An unrelated +4 remains
  untouched while Forge-Hardened or Secretive adds exactly one +2. This is
  native engine behavior, not a claim of descriptor-wide tabletop nonstacking
  or authorization for a global stacking rewrite.
- Acadamae's self-fatigue save previously carried no reason. Its local factory
  now supplies the exact existing native fatigue context, so descriptor-based
  defenses can recognize it. Its DC, native saving throw, invocation ledger,
  cancellation, slot use, and fatigue application remain on the existing path.
  Both real command regressions pass without an Elemental-module dependency.
- Viewless fixtures prove native movement-stat deltas and live Slow/difficult
  terrain conditions. They do not claim a measured in-area movement multiplier
  or new save-backed movement proof.
- Initial guarded failure result hashes:
  `917495177151897e655b1cca9b1cf7e0abbb618877708bf634f3f0af964e1cbb`
  (ordering defect and incorrect independent-Racial test expectation), and
  `aebfbcb16d912fcd5625fdadf2176d74930c9ba3c13b6429703c89b2c7b08738`
  (correct damage, incomplete replay-fixture cleanup).
  These remain failures; the later exact candidate is separately qualified.
- One preflight invocation reported an unexplained artifact-tree mismatch
  despite no active build/game or recently written artifact file. An unchanged
  diagnostic rerun passed 202 checks with identical 421,536-character
  before/after snapshots. The standard confirmation result is recorded in the
  mission journal. No preflight assertion was relaxed.

## Build and immutable artifact

- Package version: `0.0.117-elemental-traits`.
- Complete repository validation and all 1,415 domain/reflection cases: PASS.
  The two new pure cases evaluate 336 trait/save-predicate combinations and
  16 melee-hit boundary combinations.
- Clean exact-reference Release build and strict standalone package: PASS.
- Package: 23,176,082 bytes, 135 entries.
- Package SHA-256:
  `97c58a4cabe86a9f7e1cb8f97a90a20c6b13930f670d2754440cb7d135ed7032`.
- DLL: 6,043,136 bytes.
- DLL SHA-256:
  `001c615d39b42b36fd5f3d3482ee34cb626de92b5f46021e8e3a3fbff164d7b2`.
- DLL MVID: `353c4cb1-12a3-453f-a718-dd0ef0efa4c8`.
- Source-state SHA-256:
  `1caa8ad1f0a34cebd653ee1fb24fabe132842b5a7bbc013220089162a0185ed8`.
- Deployment ID: `20260905T2355597364036Z`.
- New manifest identities in this slice: zero. Total remains 1,846
  (1,844 active, two reserved); Elemental remains 209
  (208 active, one reserved). The 62 fixed C-framework identities are unchanged.
- ZIPs and raw evidence remain local and uncommitted. Curated checkpoint
  documentation is later than the tested artifact and does not retroactively
  alter its identity.

## Guarded runtime ledger

All eight processes used Windows 10, the documented guarded request, and
Steam App ID 640820. All eight PASS with 13,397/13,397 assertions and zero
runtime-result warnings. Each trait process includes the existing 4,333
framework assertions plus 107 new native passive/interaction assertions.
Nine new request-local trait units are exactly removed per trait process.
No save is opened, selected, or written; no baseline or personal save is used.

| # | Profile / scenario | Run ID | Assertions | runtime-result.json SHA-256 |
| --- | --- | --- | --- | --- |
| 1 | KMG-only ON: traits | `20260905T2356417786081Z-a77aceb8496e410b86e3760fc2e947b3` | 4440 | `8a32560ea14b969048dfad65f90cca22aa3eb02ebeb2988dd8a197d65e9e5232` |
| 2 | KMG-only ON: Ifrit feats | `20260905T2357545199101Z-d5db95204622489bb042fe6b99ccb509` | 12 | `564e0661897fcf606ce1ddddf1f7de54c8147439bd5f29db430136215437444c` |
| 3 | KMG-only ON: Acadamae | `20260905T2359030056467Z-4c8103dd074341a4862e2938c17c9665` | 20 | `f5bb131ede170fef898286d700ce923a8af0517684ab8000ef40a045a81dc66f` |
| 4 | Combined ON: traits | `20260906T0001281249466Z-201568dc8e9a41e9baf7c832bbed7ff5` | 4440 | `8e0b1685b364db15fed74f895761587b538f5f5ba678e3ba0ce99c9710a9f012` |
| 5 | Combined ON: Ifrit feats | `20260906T0003195567160Z-958d4a0b083141a5a262b1d47aedb430` | 12 | `f614b4d6891e526e74a45c30021e500967e9c68db888d59408b22e166993e66d` |
| 6 | Combined ON: Acadamae | `20260906T0005040916193Z-6361d7e3bff041959006b15455704f8b` | 20 | `2acc496268c87fdda251b6cee4d8fb4b0ec2d567e176dd373f953dcfa0c4f292` |
| 7 | Combined OFF: publication | `20260906T0008039900868Z-d5a1620a2a7545ddb5df12d0ffbc00d4` | 13 | `762617f0db7e6c678fab5e67d9b321c8cf5f48c8b759b11af6ef9d03f112e6a1` |
| 8 | Combined OFF: traits | `20260906T0009490491115Z-54726eae2446491ea82ab5a60c98d7c7` | 4440 | `6a38ce2be791e3e89fcb2eddba226fbd3a0b508b7eef4526fe70a6d27e410a12` |

| # | Additional evidence file | Additional evidence SHA-256 | runtime-evidence.json SHA-256 |
| --- | --- | --- | --- |
| 1 | `elemental-alternate-trait-passives.json` | `825ac6d48c3be78e70d18ecadf2a9de189e3d8087f75c71c8c522f571bcad34c` | `b21b9e3b4ec5d3cc4fa5c73f1fdd3c749c5e5eaad5bb251a3ccb5656e31b811f` |
| 2 | `elemental-ifrit-feats.json` | `c7f155779085ec1c28a32921839159efec3ebc0a5e1c64d5543f0aedb40141ec` | `904b61d04ebcd3616c3040380e8d01c25c4714811099a16280749309eaac99a0` |
| 3 | `runtime-events.json` | `e3566b3a06430868d71e9287dfd6c6c520a3da027aabea01951d407ee131dc2f` | `8768532776f1eb2936a187e9bce922ef1463d4cfa08c42fe6aeefe2fec3a7c4d` |
| 4 | `elemental-alternate-trait-passives.json` | `825ac6d48c3be78e70d18ecadf2a9de189e3d8087f75c71c8c522f571bcad34c` | `48e130e3cdfd05d8d35b127bf00b929656595681717dd74c5ce09c1cef2943a0` |
| 5 | `elemental-ifrit-feats.json` | `a8dcaa427a7630cc07f34a716a71caa0d87322c2c53ba65f7d2b41b6233e24fc` | `0da6b8ff4c90194e0a795726263368762a6b7c323a5fadb604b6b8aa55cc4e8a` |
| 6 | `runtime-events.json` | `e3566b3a06430868d71e9287dfd6c6c520a3da027aabea01951d407ee131dc2f` | `2790ecf1af96b3202560735636d783889a0b1655776babeaf0119403395711fc` |
| 7 | `runtime-events.json` | `e3566b3a06430868d71e9287dfd6c6c520a3da027aabea01951d407ee131dc2f` | `b8b08d68d143eca086777740cd58050eba462a6b3499cc8a4432c5a651a272c1` |
| 8 | `elemental-alternate-trait-passives.json` | `825ac6d48c3be78e70d18ecadf2a9de189e3d8087f75c71c8c522f571bcad34c` | `162aef958f837a897d263e01e2f56dcb7c6e65754586347d7b44815ae05a1410` |

`runtime-events.json` is the auxiliary event log, not a substitute for the
actual assertions in `runtime-result.json`. Identical auxiliary hashes
represent identical content, not independent mechanical proof.

## Exact compatibility restoration

Combined means the established
`gunslinger-high-risk-combined-favored-class` profile:
Call of the Wild, Races Unleashed, Tweak or Treat, and Favored Class alongside
KMG under the profile's pinned identities. Favored Class is observed only;
no FCB content or publication behavior was added.

All three transactions returned `Restored` with
`restorationVerified=true` and identical 968-entry original manifests.

| Profile / state | Transaction ID | Transaction JSON SHA-256 |
| --- | --- | --- |
| KMG-only ON | `compat-20260905T235621Z-2e8da2dcb7d2` | `ef3964eb757274314c3367da7e688440e817107daba9800eb8643ca94469cdc4` |
| Combined ON | `compat-20260906T000050Z-b45d8402c83c` | `8a9f1bd07728e6d318487e442da7ee73715e9572e532755b0b861446d055a813` |
| Combined OFF | `compat-20260906T000733Z-45ba133a4c32` | `784cf8593de7ca6c67a0782f6e098edd2b0762c06a4d6af09a08cf7d698b5749` |

Original/restored manifest digest:
`b3167a2c14b8db9aa8bc3d52adfec465f341d0ff6febdca6cead92805c20988b`,
defined as SHA-256 of UTF-8 PowerShell
`ConvertTo-Json -Depth 6 -Compress` of `originalManifest`.
Original/restored FeatureModules.json:
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
Call of the Wild settings:
`24cc3f80269992a53ebbfd1f5986e5aab056841d6b2f43d8e22e764cdb73f6e8`.
Combined-profile Favored Class staged settings, before/after:
`bdceed77d2bf4a31dd9e4eeb64ef9d55a42ef59d23f46abcb1ddbcc6ef66754b`.

The OFF publication run retains all 16 foreign/native race entries, all
348 universal feat entries, all 175 Fighter feat entries, and their order.
All 25 feat identities and the race/heritage/trait graph register while
project race/feat publication is empty. The following OFF mechanics process
passes the same complete trait matrix. No Kingmaker process remains.

## Remaining gates and next work

Thirteen mechanics remain: Fire/Stone/Storm in the Blood; Fire/Earth/Air
Insight; Efreeti Magic; Crystalline Form; Treacherous Earth; Breeze-Kissed;
Acid Breath; Nereid Fascination; Ooze Breath.

Installed read-only IL establishes native `RuleSummonUnit.BonusDuration`
addition and actual-received `RuleHealDamage.Value` as promising narrow
boundaries for the shared duration/healing helpers. Their typed predicates,
daily state, actual commands/events, and persistence are not yet implemented
or qualified.

Trait-bearing fresh-process saves, active-trait level/rest/respec,
death/resurrection, polymorph/return, direct 0.0.114 receiver migration, full
six-profile compatibility, generic module-boundary qualification, and final
release documentation remain pending. Visual Adjustments is absent and
NOT-RUN. No Release C PASS is claimed. The mandatory branch push remains
blocked by its external allowlist; no bypass, merge, tag, or public release
has occurred.
