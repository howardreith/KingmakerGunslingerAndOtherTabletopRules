# Urban Barbarian human acceptance checklist

The exact immutable `0.0.83` candidate documented below was **REJECTED** in
human review and is preserved only as superseded evidence. Do not describe it
as accepted or release complete. This checklist will be finalized for the
exact `0.0.87` repaired candidate after its new mechanical qualification
passes. Immutable `0.0.84` failed focused runtime qualification; immutable
`0.0.85` failed its packaged CotW observer; immutable `0.0.86` passed focused,
persistence, and CotW profiles but failed its generic module observer. All are
preserved below as superseded evidence.

Human-review rejection findings:

- the level-2 live allocation grid exposed all 31 variants instead of six;
- all 31 variants used the same native Rage icon;
- current selection was not unmistakable in the live grid;
- the automated tier test exercised a patched accessor rather than the actual
  live player-facing enumeration path; and
- Crowd Control was not visibly confirmed in an ordinary two-kobold fight and
  requires real attack-pipeline and combat-log qualification.

## Superseded 0.0.83 candidate identity

- Artifact/source commit: `06cad804651faaace17bdf8432bcd071d50ce9e7`
- Qualification documentation commit:
  `636e4928502cb6a07374279a3a8b35f79f66f4e3`
- Branch: `codex/urban-barbarian`
- Version: `0.0.83`
- Package SHA-256: `b2b4fdd899a1e00955e972d94b45f5624f4d663e88581043cbf969c3d6e3d193`
- DLL and installed-DLL SHA-256:
  `c72eb71bc57b6be79b5cd49c58b262bf0897960eac2c118538d7e6e43cfccaae`
- DLL MVID: `1f53a664-2557-4866-b690-a720cbff840f`
- Deployment manifest:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260816T1831540393986Z\deployment.json`
- Deployment manifest SHA-256:
  `145de454d3e7282783f656a65d005036bd39a3f5f0f4bf43d511e07238817584`
- Kingmaker build: `2018.4.10.10503941`
- Mechanical seal: focused Urban PASS; two-launch existing-owner OFF
  persistence PASS; CotW normal/balance/absent PASS; module boundary 18/18
  PASS; 1,149/1,149 fast tests PASS.

The tracked qualification evidence was finalized in a later documentation-only
commit. That commit does not change the package, DLL, or installed candidate;
the artifact/source identity above remains authoritative for the rejected
candidate's preserved evidence only.

## Superseded 0.0.84 candidate identity

- Artifact/source commit: `034673ab9ae7187a8d1af2ba8f906a286f77b0bc`
- Package SHA-256: `499fc9e34bd628fed356f235ea124fccc72a0c28201799adbe766e33a7a775fd`
- DLL SHA-256: `9b7b5e59a155b25d77724b9f9949d6497ac9ace5bd43c354f9f8f19597739abe`
- DLL MVID: `fe87293c-caa7-4c5f-9d42-06f8a22a8ac7`
- Package-manifest SHA-256: `94766e6db1cebb7ade6da117c5d294ee2b17de33263f116762f53d63abb02220`
- Deployment-manifest SHA-256: `fa1615eb10332742a525b6d92624adc5705f98ee9eecdf4ba4b89022d35ae2a7`
- Failed focused run:
  `20260816T2049371746501Z-disposable-urban-barbarian-focused`

The run proved exact +1 command-path attack and target-AC mechanics at the
two-enemy threshold, along with the repaired selector, icon, selected-state,
Trickery, spell-lock, Rage, and HP assertions. It failed because the AC rule's
combat-log `BonusSources` inventory omitted Crowd Control. It is not human
accepted or release complete.

## Superseded 0.0.85 candidate identity

- Artifact/source commit: `b80e63a0af1fe07d761362ce6de846b838507657`
- Package SHA-256: `4cd2d1516441fbe4e96b975ebd7ad8180ed8d21d656858713737e4281919ad53`
- DLL SHA-256: `b7ad444ef8230636e7db84384faaf8f969762776626ceb3a5bdb6fe0562f35bf`
- DLL MVID: `96d3412b-d115-4bd9-b94a-03f94770c502`
- Package-manifest SHA-256: `5793be17480f6fea16472fa5a5bf431e29b926b841a0d8a5f60b3d77fa77fd8a`
- Deployment-manifest SHA-256: `6eba6583380bac661ab078c8fd1be21bcb13401effc75b01e74b8192f5ad2012`
- Focused PASS:
  `20260816T2101481246450Z-disposable-urban-barbarian-focused`
- Persistence PASS pair:
  `20260816T2104057985757Z-working-save-urban-barbarian-prepare` and
  `20260816T2106360356218Z-working-save-urban-barbarian-off-verify-cleanup`
- CotW-normal ERROR:
  `20260816T2110444459225Z-observe-urban-barbarian-rage-inventory`

The CotW run failed before assertions because the packaged inventory observer
still called `.Single()` on the deliberately inert legacy selector. Its
compatibility transaction restored the original Mods tree exactly. The
artifact is superseded and is not human accepted or release complete.

## Superseded 0.0.86 candidate identity

- Artifact/source commit: `3dbe20ae8e77246df4a711f64e30a07988401ec1`
- Package SHA-256: `c266b3417cba7ab362b9e21fcef990cad3af91c11caffdfb6fe3cdf9cd02e0e5`
- DLL SHA-256: `54793b8a7f3f4b33b03b1c152a37b150ca1237c5db7571e9ab635e9b4e018e84`
- DLL MVID: `21473f81-9e24-4e9b-8cdb-59b04e95ddfe`
- Package-manifest SHA-256: `d746e50d5d0717a3a3522fd6ca77e7d8eb51ee9ea3fe02868aca5ba553ecd9f8`
- Deployment-manifest SHA-256: `aac7a5dc379cd5e3ccba853f1690de8c5c115aceb70b10008379431a5a17f077`
- Focused PASS: `20260816T2120430615092Z-disposable-urban-barbarian-focused`
- Persistence PASS pair:
  `20260816T2122439864672Z-working-save-urban-barbarian-prepare` and
  `20260816T2124570602978Z-working-save-urban-barbarian-off-verify-cleanup`
- CotW normal/balance/absent PASS:
  `20260816T2127485691200Z`, `20260816T2130180421876Z`, and
  `20260816T2132397239363Z-observe-urban-barbarian-rage-inventory`
- All-ON boundary ERROR:
  `20260816T2134169673139Z-observe-feature-module-settings`

The first boundary run failed before assertions because the separate generic
module observer still called `.Single()` on the inert legacy selector and
retained 70/31 presentation assumptions. Feature settings restored exactly.
The artifact is superseded and is not human accepted or release complete.

## Character creation and presentation

- [ ] With Urban Barbarian ON, the native Barbarian archetype list shows
  **Urban Barbarian** exactly once.
- [ ] Name, description, icon, and progression rows are readable and coherent.
- [ ] Level 1 shows Urban Barbarian Proficiencies, Crowd Control, and Controlled
  Rage, and does not show Fast Movement.
- [ ] The displayed class skills are Athletics, Mobility, Knowledge (World),
  Perception, and Persuasion.
- [ ] Simple and martial weapons, light armor, and non-tower shields remain;
  medium armor proficiency is absent and no unrelated proficiency changed.
- [ ] Native Barbarian and any CotW Barbarian archetypes remain present,
  ordered, and unduplicated.

## Crowd Control

- [ ] The tooltip states the two-active-hostile threshold, +1 attack, +1 dodge
  AC, edge-to-edge adjacency, and that weapon reach does not extend adjacency.
- [ ] Visible combat values grant neither bonus with zero or one adjacent
  enemy and exactly +1 attack/+1 dodge AC with two or more.
- [ ] The live combat-log breakdown names **Crowd Control** as the +1 attack
  source on an Urban Barbarian attack and as the +1 dodge AC source on an
  incoming attack while two qualifying enemies are adjacent; neither entry is
  present at zero or one qualifying enemy.
- [ ] The visible result updates promptly when an enemy moves in/out or dies.
- [ ] A large adjacent enemy behaves by creature edge, and a reach weapon does
  not enlarge the five-foot adjacency boundary.
- [ ] Melee and ranged attacks receive the same threshold bonus.

## Controlled Rage selector and tiers

- [ ] The action bar/ability panel contains one compact Controlled Rage
  Allocation selector, not 31 top-level buttons.
- [ ] The current selection is unmistakable and the selector remains readable
  at normal action-bar and ability-panel size.
- [ ] Ordinary Rage shows only the six legal +4 allocations: three full-score
  and three +2/+2 choices.
- [ ] Greater Rage shows only ten +6 allocations, including full +6, every
  +4/+2 direction, and +2/+2/+2.
- [ ] Mighty Rage shows only fifteen +8 allocations, including full +8,
  +6/+2, +4/+4, and +4/+2/+2 families.
- [ ] Each newly unlocked tier defaults independently to full Strength; an old
  tier selection is neither active nor offered as usable.
- [ ] Selection costs no Rage rounds, persists until changed, and cannot be
  changed while Rage is active.

## Controlled Rage mechanics

- [ ] Full Strength, Dexterity, and Constitution choices change the actual
  score by exactly +4/+6/+8 at their respective tiers.
- [ ] Representative split allocations change each actual score exactly and
  sum to the current pool.
- [ ] No selected bonus applies while Rage is inactive.
- [ ] Controlled Rage grants no ordinary Rage attack bonus, weapon damage
  bonus, temporary HP, Will bonus, or AC penalty.
- [ ] Intelligence-, Dexterity-, and Charisma-based skills remain usable.
- [ ] Save before a locked chest or detected trap. A suitable ordinary-Rage
  control character is prohibited from performing the Dexterity-based
  Trickery action while raging, while the Urban Barbarian can perform the
  same Trickery check during Controlled Rage.
- [ ] With Controlled Rage still active after the Trickery comparison,
  spellcasting/concentration remains prohibited as under ordinary Rage.
- [ ] Stealth may be checked secondarily; Use Magic Device is not used as the
  primary skill test because Rage can independently restrict concentration-
  dependent magic-item activation.
- [ ] The native Rage resource and live counter remain visible; activation,
  per-round spending, cancellation, fatigue, and Tireless Rage behave normally.

## Constitution and integration

- [ ] Constitution allocation at full health increases maximum/current HP only
  through the real Constitution modifier and removes it exactly when Rage ends.
- [ ] After damage and at low HP, ending Rage restores the same damage deficit
  without healing, duplication, or an immortal negative-HP state.
- [ ] Repeated entry/exit, level transition, and save/load do not create HP or
  duplicate modifiers.
- [ ] A representative passive native rage power, activated native rage power,
  and Rage-required feat or item recognize Controlled Rage.
- [ ] Under the supported CotW profile, a representative CotW-added rage power
  and Rage marker work without duplicates; with CotW absent, the Urban core
  remains fully available.

## Module and compatibility behavior

- [ ] The UMM label is **Urban Barbarian** and does not say CotW is required.
- [ ] CotW status text clearly distinguishes native core availability from
  optional interoperability qualification.
- [ ] With the module OFF, Urban Barbarian is absent from new selection/respec.
- [ ] An existing Urban owner loaded with the module OFF retains progression,
  selection, Rage, Crowd Control, and all owned features.
- [ ] Repeated restart or settings changes produce no duplicate archetype,
  feature, buff, selector, allocation, resource, marker, or action.

Record acceptance or each requested change against the immutable identity in
the handoff. Do not modify or replace the installed candidate during review.
