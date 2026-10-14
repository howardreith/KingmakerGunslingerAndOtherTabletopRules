# Sprint 23 Triceratops and Stegosaurus coverage

Status: **RELEASED / STOP** on October 13, 2026. Product PR
[#41](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/41)
merged normally as `1d41388adb6aaa6e4e102e744abb392104cedbba`.
Stable [v0.0.152](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.152)
was published as Latest at that release point. Its historical result remains
unchanged below; the bounded v0.0.153 follow-up now owns the Latest pointer.
Sprint 24 was not started.

## v0.0.153 bounded Powerful Charge follow-up

Status: **RELEASED / STOP** on October 14, 2026. Product PR
[#43](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/43)
merged normally as `e122013edce9ceb88bd9aa0b40c9762cc717ed8d`.
Stable [v0.0.153](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.153)
is published and Latest. Documentation-only final-records PR
[#44](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/44)
synchronizes this verified endpoint. This arithmetic patch did not start
Sprint 24.

The released v0.0.152 helper documented a positive 2× live-Strength target but
separately truncated its additional half-Strength term. The focused
reproduction therefore produced +13 instead of +14 at modifier +7. Installed
Kingmaker 2.1.7b IL confirmed that the ordinary positive single-natural-attack
term is truncated after 1.5× multiplication, while zero and negative modifiers
are applied once. The local correction now adds `2*m - floor(1.5*m)` for a
positive modifier and adds nothing for zero or a penalty.

| Strength modifier | Ordinary contribution | v0.0.152 charge total | v0.0.153 charge total |
| ---: | ---: | ---: | ---: |
| +7 | +10 | +13 | +14 |
| +8 | +12 | +16 | +16 |
| +9 | +13 | +17 | +18 |
| +10 | +15 | +20 | +20 |

Exact v0.0.153 lineage and artifact identity:

- Tested source: `8f30cf72c916aaee840c18b373be82274729520e`.
- Source fingerprint:
  `5167ff01981bb3a6d4109ebc0dde5646db258902e4c8dea74f2d8414626046ef`.
- Qualification-report commit: `e506840900575f82600e44a02e8beb5091937806`.
- Product merge and peeled tag commit:
  `e122013edce9ceb88bd9aa0b40c9762cc717ed8d`.
- Annotated tag object: `586d42b70b30569f88266c73ea7afc3696e911be`.
- Release ZIP SHA-256:
  `cca7c0d2fe902e79aab090250912e0aa03a135c5680a926ab4e4d8749dfd366b`.
- DLL SHA-256:
  `bab22f7fa8a10c4f9adc2410b4e2fd4ec98b56d3a5291bbd36cf0b47fbf333b0`;
  MVID `dd3797aa-a482-4411-b3a5-f574204a320a`.
- Downloaded release-manifest SHA-256:
  `690e3dbb597d586fc41d9ef8989fb4dc0f443b4b3305d72177c6d727791b0a39`.
- Downloaded SHA256SUMS SHA-256:
  `c9e33dceafd4f125b58604bd62adb19001a71e673d3bce0a9348442d3e2febcd`.

The frozen candidate's one full Sprint gate passed 2,594/2,594 tests plus
strict package validation in 181.8 seconds. Fresh native proof on those bytes
passed 13/13 assertions in each selected profile:

- standalone RTWP runtime
  `20261014T0301019306544Z-7c896449d20e4e3d9335f1e4dde928e9`;
- standalone turn-based runtime
  `20261014T0305412649861Z-3ce64fad9ff94f36a5fde7986fb999a5`;
- highest-risk-combined RTWP runtime
  `20261014T0309454568154Z-c3d6c46905f1401c9d59ce7fb8f7d427`.

Each run used real issued native charge commands. The +7 and +9 rows supplied
the required odd-modifier hit in both combat modes; +8 preserved the printed
ordinary +12 and charged +16 baseline. The observer recorded 2d10 ordinary and
4d10 charged weapon stats, exact Strength and additional-charge contributions,
zero unrelated additive damage, weapon/command/target identity, noncritical
state and action expenditure. Separate native Power Attack-on and critical
checks passed without double application. Ordinary, aborted, later and
opportunity attacks remained ordinary. Rhinoceros +3 and Woolly Rhinoceros +5
fixed-increment charge paths remained unchanged.

The final observer used a bounded request-local faction handoff solely at the
installed `PowerAttackWatcher` attack-roll boundary. It retained the original
summon faction through weapon-stat calculation, restored it before damage and
again in outer cleanup, and changed no production combat component. Earlier
observer/setup failures remain preserved in the machine-readable ledger rather
than being relabeled as product passes.

No identities, routes, assets or serialization state changed. The v0.0.152
twelve-route, art, trample/trip, module lifecycle and persistence evidence is
reused with its original provenance. The surface remains 105 project creatures,
1,075 generated placements, 29 retained native wrappers and 1,104 visible
choices.

The public release contains exactly the ZIP, `SHA256SUMS.txt` and
`release-manifest.json`. GitHub's digests match the downloaded files; the ZIP
passed strict validation and contained the exact qualified DLL/MVID. The release
is stable, non-draft, non-prerelease and Latest. Verification receipt SHA-256 is
`59bf964616e60eec294e34b087b0f23a9653e271ecf657c220d2f173d4deca5b`.

The post-release audit restored the actual intake exactly: 968 Mods entries,
installed KMG 0.0.117 with 136 files, 83 game settings and 118 save files. The
protected baseline and working-save SHA-256 values remain
`cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07`
and `795d59ff448173c6631518676b655d3bd03b31dc31e418f578f9e9488cb10ada`.
Installed/private references and the sound bank match, and no game process,
compatibility lock, game staging, disposable publisher checkout or owned
diagnostic temp remains. Audit SHA-256 is
`02817e0ea81e9c874f3e3fe930c2f60f3035c353f3807f3abe7c0f6a7e5b364c`.

The detailed qualification and failure provenance is in
[`planning/SPRINT23-TRICERATOPS-POWERFUL-CHARGE-ROUNDING-LEDGER.json`](../planning/SPRINT23-TRICERATOPS-POWERFUL-CHARGE-ROUNDING-LEDGER.json).
The endpoint is achieved. Stop; do not start Sprint 24.

## Exact artifact lineage

- Public candidate source: `e0d7cb863b23925cfbf1721e3965c05e811a0bd7`.
- Candidate source-state SHA-256:
  `04ef0ced7549e7f1bcc9bceb91ae04380a1e2c8bcebb9d1b34edd95404f48c7b`.
- Qualification-report commit: `9b117860632ff56e9709cb1650adee991f78e5a5`.
- Product merge and peeled tag commit:
  `1d41388adb6aaa6e4e102e744abb392104cedbba`.
- Annotated tag object: `a64b95587b3be774e73b0d6a7ec0054ae21604b2`.
- Release ZIP SHA-256:
  `e417ad95e796bac7c312ed60e859b888b1dd2f83818ef211027e35d2c1c663db`.
- DLL SHA-256:
  `594113de52c514702b6999fd4e3d3abfd9823e12d003e63d7953f622feaa6c05`;
  MVID `5afdfc38-4657-48e2-a0fa-942098189a4b`.
- Build manifest SHA-256:
  `1f338de9c4f03ac774a38ba3f998bbd19bdc9606f7c7ea73afc373ecff114108`.
- Asset bundle SHA-256:
  `b3cffb49ba32af10db12470401a58f6dff0ead9f219f87e41d9ec138d62fbaeb`.

The ZIP was promoted from the tested candidate without rebuilding for the
reporting commit, product merge, tag or this final documentation. The frozen
candidate's Sprint gate passed 2,594/2,594 tests plus strict package validation
in 178.6 seconds.

## Published roots and totals

| Family | Creature | Tier | Quantity | Root GUID |
| --- | --- | ---: | --- | --- |
| Summon Monster | Triceratops | VI | one | `2c7a44ebb6d645ddaa30e8d241c91418` |
| Summon Monster | Triceratops | VII | 1d3 | `088b7d9f07bd4631ad16520cc15c842e` |
| Summon Monster | Triceratops | VIII | 1d4+1 | `6fbc68c391f84b45ad6f18c4ac6b103e` |
| Summon Monster | Triceratops | IX | 1d4+1 | `8cc56e3b180f4232b463721268b65f62` |
| Summon Nature's Ally | Triceratops | VI | one | `774fe43dbf164e82a024af00a383f077` |
| Summon Nature's Ally | Triceratops | VII | 1d3 | `290584eb8a644df98fa013dfb6ac8931` |
| Summon Nature's Ally | Triceratops | VIII | 1d4+1 | `104ef9483c2843fd8573f927dcb44760` |
| Summon Nature's Ally | Triceratops | IX | 1d4+1 | `134873f5a4234e959a90ebb2a6bf48be` |
| Summon Nature's Ally | Stegosaurus | VI | one | `bb1b546fe26943c8a4dc396e66e17b04` |
| Summon Nature's Ally | Stegosaurus | VII | 1d3 | `d522a44c27fe41feafa3b16243fcdff2` |
| Summon Nature's Ally | Stegosaurus | VIII | 1d4+1 | `6f8a6c3c6b614f21a1b084559b24c326` |
| Summon Nature's Ally | Stegosaurus | IX | 1d4+1 | `ada164beea7d45edab2aa33bc8b8718a` |

Only the four Triceratops Monster roots have the established celestial and
fiendish execution children. Stegosaurus has no Monster entry. The final surface
is 105 project creatures, 93 Monster creatures / 534 placements, 93 Nature's
Ally creatures / 541 placements, 1,075 generated placements, 29 retained native
wrappers and 1,104 visible choices. Native spell-slot spending and summon
duration behavior passed on the public routes.

## Gameplay and presentation results

Triceratops qualified its complete printed profile, one 2d10+12 gore, inferred
Improved Critical (gore), native Power Attack at BAB +10 (-3 attack / +9
damage), a real-charge-only 4d10+16 powerful charge and 1d8+12 trample with
live Strength/HD DC 25.
Trample used native movement contacts and the released enemy-aware response
policy: one executable native opportunity attack at the established penalty,
otherwise Reflex for half, never both. Size legality, interruption/death,
same-round duplicates, next-round reuse, independent tramplers and ordinary
attack/resource controls passed.

Stegosaurus qualified its complete printed profile, one 4d6+12 spiked tail,
native Power Attack at BAB +9 (-3 attack / +9 damage) and one exact-target
native Trip after a valid tail hit. Hit/miss, maneuver success/failure,
immunity, oversized and already
prone targets, duplicate callbacks, separate strikes, unrelated weapons and
source/target destruction passed. A failed trip did not trip the Stegosaurus;
no daze, stun, automatic knockdown, injury-poison gate or Ankylosaurus rider was
present. Native Cleave remained an active action.

Released rhinoceros charge, d6 trample and Ankylosaurus ordinary/critical rider
controls passed. Both new creatures used useful ordinary-attack fallback and
native action spending in RTWP and turn-based cases.

The release contains distinct deterministic project-owned bodies, albedos,
paintings and icons: a horn-led three-horned Triceratops on the Mastodon rig and
a small-headed, high-backed, plated Stegosaurus with a long four-spiked tail on
the Hodag rig. Private mesh/material/texture ownership, immutable donor assets,
Huge selection/collision/navigation scale, measured 15-foot reach, contact poses,
walk/turn extremes, crowding, death/fade/despawn, failed-attach rollback and
module OFF/ON lifecycle all passed. Owner aesthetic approval remains
`NOT_PERFORMED_NONBLOCKING`.

## Fresh, reused and failure evidence

Fresh Sprint 23 evidence covers both identities, profiles, mechanics, original
art owners, RTWP/turn-based behavior, standalone and highest-risk combined
compatibility, two complete targeted persistence cycles, module refusal and
re-enabled attach-once behavior. The final public high-risk inventory run
`20261013T2015478087914Z-8346e9b4bedb4d50a019508e64c73cf9` passed 50/50;
proof SHA-256 is
`cc66691d94b1f7fdb66d976d8bb3dcc7bd575c66c6187f6ea30225ca4fca22ea`.
The public RTWP route run
`20261013T2022330281385Z-652aaac6028d40ce818d710fd1310289` passed 45/45,
including all twelve roots; proof SHA-256 is
`dfa5aa58a33fff8665c86e88cd4911310b38f5b8d636a697c17c1aabfa632c46`.

The first public inventory observation correctly failed two stale census
assertions. It exposed missing exact owner/fact rows and a comparer that removed
the profile-declared Ankylosaurus Stun fact. The observer was narrowly repaired,
its affected tests and candidate gate passed, and both affected public native
cases were rerun on final bytes. Earlier engineering and fixture failures remain
in the machine-readable ledger with exact evidence and restoration receipts;
none was discarded as a pass.

Evidence reused from v0.0.151 is limited to unchanged old roots, broad module
combinations, other creature reviews and the whole-roster persistence baseline.
The touched shared seams have fresh rhinoceros, d6-trampler and Ankylosaurus
controls. Reused evidence is not described as fresh execution.

Accepted limitations are:

- `PASSIVE_CREATURE_SENSES_UNMODELED`.
- `IMPROVED_OVERRUN_NATIVE_CONSUMER_UNAVAILABLE`.
- `RUN_NATIVE_SUMMON_AI_DISPOSITION`.
- `HODAG_DONOR_NO_NATIVE_HIT_ANIMATION`.

## Download verification and restoration

The public release contains exactly the ZIP, `release-manifest.json` and
`SHA256SUMS.txt`. Their downloaded SHA-256 values are respectively
`e417ad95e796bac7c312ed60e859b888b1dd2f83818ef211027e35d2c1c663db`,
`e2813ea7a4c21ed2c829655aab7e0291c3ddb5b9964b1aff8ae13465c730a0cc`
and `a845854a8ef98aed16b08cd8a3312c17369362498f36761902e0a37c9080e409`.
GitHub's asset digests match. The downloaded ZIP passed strict validation,
contained the exact DLL/MVID, reported the 1,075/1,104 roster and disclosed all
four limitations. Release-verification receipt SHA-256 is
`21d806f20a7e3c0ffae7c36e194a21b2700c6896cbea7868afa6d097129ad9b0`.

The final post-release audit passed: 968 Mods entries and 83 guarded settings
match intake; the installed mod remains 0.0.117; the protected baseline save is
`cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07`;
the authorized working save is
`795d59ff448173c6631518676b655d3bd03b31dc31e418f578f9e9488cb10ada`;
the firearm sound bank and installed/private Kingmaker reference bytes match;
and no Kingmaker process, runtime lease, compatibility lock or game staging
remains. Post-release audit SHA-256 is
`450ff0d58bd829129744ae1c9bcbddf47a1bec4421b808d676b3a66512a031c4`.

The intake ledger originally omitted two hexadecimal characters from the
Assembly-CSharp digest. Direct hashing of both 7,262,208-byte files corrected
the record to
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`;
this was a record transcription correction, not a file change.

The machine-readable source for transaction IDs, failure history and exact
evidence identities is
[`planning/SPRINT23-TRICERATOPS-STEGOSAURUS-LEDGER.json`](../planning/SPRINT23-TRICERATOPS-STEGOSAURUS-LEDGER.json).
The endpoint is achieved. Stop; do not start Sprint 24.
