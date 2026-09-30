# Sprint 12 canine and small-quadruped rules and donor audit

Status: intake and hidden foundation complete; implementation remains hidden
until mechanical, visual, quantity, player-path, persistence, compatibility,
and restoration qualification pass.

## Tabletop contract

The [Paizo Dire Rat entry](https://aonprd.com/MonsterDisplay.aspx?ItemName=Dire+Rat)
specifies a Small 1-HD animal with speed 40 feet, Str 10, Dex 17, Con 13,
Int 2, Wis 13, Cha 4, +1 natural armor, a 1d4 bite, Filth Fever at Fortitude
DC 11, and the four-legged +4 CMD defense against trip.

The [Paizo Dog entry](https://aonprd.com/MonsterDisplay.aspx?ItemName=Dog)
specifies a Small 1-HD animal with speed 40 feet, Str 13, Dex 13, Con 15,
Int 2, Wis 12, Cha 6, +1 natural armor, a 1d4 bite, Skill Focus
(Perception), and the four-legged trip defense. It has no trip-on-hit attack.

The [Paizo Hyena entry](https://legacy.aonprd.com/bestiary/hyena.html)
specifies a Medium 2-HD animal with speed 50 feet, Str 14, Dex 15, Con 15,
Int 2, Wis 13, Cha 6, +2 natural armor, a 1d6 bite, Skill Focus
(Perception), four-legged trip defense, and trip on its bite.

The [Paizo Goblin Dog entry](https://legacy.aonprd.com/bestiary/goblinDog.html)
specifies a Medium 1-HD animal with speed 50 feet, Str 15, Dex 14, Con 15,
Int 2, Wis 12, Cha 8, +1 natural armor, Toughness, and a 1d6 bite. It is
immune to disease. A bite that damages a non-goblinoid calls for a Fortitude
DC 12 save; failure applies allergic reaction as a disease for one day,
imposing -2 Dexterity and -2 Charisma. Multiple exposures do not stack, and
magical healing or remove disease removes the reaction.

## Installed-engine evidence

The guarded Steam metadata audit
`20260930T0823186905486Z-observe-expanded-summoning-native-donors` passed all
four assertions on source `9c927612ea4820937ca7014200bbb1f32b36b982`,
version `0.0.140`. Its result SHA-256 is
`7AE42D3E9787E86F1821C0FDBA4D1C3D8A7272953326D4FCF17ABAB35ACB0A6A`;
runtime-evidence manifest
`D1EC6DADF4718DC0D46DBBC1080212A68607D0CE39BB72A4A1F393CE69582DED`;
native-donor audit
`B98A022FA2527348F808315078E6C4212A9E73EBC138E11C4889FCB33F0A82AE`.
The restoration record
`20260930T0825224797467Z-observe-expanded-summoning-native-donors.json`,
SHA-256
`3EC6D4D0D09E76437B34B4AC8141920646C10D4F35C6955AAD3A2C7C1FCB7B31`,
proves the exact pre-run 136-file installation returned to tree SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Repository validation, all 1,954 domain tests, a clean Release build, and
strict standalone package validation preceded the launch. The tested package
SHA-256 was
`2B2840F0E5BB93923BBFD9FB575DF6ACF0A740772FCBD5E0ADACA57943868173`;
DLL SHA-256
`B7EA4474B18FEC3798496BF7DD5DEBAAD496EBC836A115EF46492FEF505D9A52`.

The installed library contains an exact native summoned Dog
(`DogSummoned`, `77f3f2ddf1ec2da45ab956c433e3b557`) and ordinary Dog
(`CR0_DogStandard`, `b6d638547f36b21499324fe5a30a92c6`). Both use prefab
asset `10bce772f3563b046b07514393e8d79e` and carry the correct Small
four-legged dog chassis. Sprint 12 may bind that native rig while retaining
the existing KMG Dog unit and placement identities.

No individual Dire Rat exists in the installed library; only a Rat Swarm is
present. No native Hyena or Goblin Dog unit exists. The current KMG Hyena is a
Wolf-rig projection and the current KMG Goblin Dog is a Worg-rig projection,
so those proxies do not satisfy Sprint 12's distinct-silhouette gate.

Native Filth Fever is the buff
`9545a5550d89feb47a84edaeb4e63d0b`. Its audited graph carries disease
descriptor value `16384`, a Fortitude new-round disease lifecycle, ability
damage, successful-save tracking, and self-removal. It is a safe native
condition payload; it does not provide the Dire Rat's bite delivery or initial
DC 11 save. Sprint 12 will own that exact hit/save delivery and apply the
native payload with the attack context's DC fixed at 11.

The installed library has no blueprint named `DiseaseImmunity`,
`ImmunityToDisease`, `DiseaseImmunityFeature`, `AllergicReaction`,
`GoblinDogAllergicReaction`, `GoblinDogDisease`, `DireRatDisease`, or
`RatDisease`. Sprint 12 therefore owns the Goblin Dog disease-descriptor
immunity and the nonstacking allergic-reaction feature/buff. These components
must be summon-local and must not alter native facts or targets.

Kingmaker classifies 67 audited goblin units with the exact native unit type
`Goblin:d524df24b2f38cf4590525b2e7c4f34e`. No broader native Goblinoid
subtype fact exists. The bounded Kingmaker adaptation treats that exact unit
type as the goblinoid exemption and treats other valid living bite targets as
eligible. This rule is deterministic, uses no name or faction heuristic, and
is disclosed in the player text and fidelity record.

## Implementation boundary

- Preserve every existing Dog, Hyena, and Goblin Dog blueprint and placement
  GUID. Add Dire Rat under newly allocated identities and keep all its choices
  hidden until qualification.
- Bind Dog to the audited native Dog rig. Supply original, deterministic,
  editable silhouettes for Dire Rat, Hyena, and Goblin Dog; do not publish the
  existing Wolf/Worg proxies as completed visuals.
- Keep the compact Small/Medium footprints and native quadruped navigation.
  No creature may replace, modify, or inherit state from an animal companion.
- Dire Rat disease fires only after its exact bite hits and deals damage,
  resolves the printed DC 11 initial save, and uses the native Filth Fever
  payload and cure lifecycle.
- Goblin Dog immunity blocks disease-descriptor buffs on that summon. Its exact
  bite applies one DC 12 save only after damage to an eligible target; failure
  applies the project-owned one-day, nonstacking -2 Dexterity/-2 Charisma
  disease. The exact native Goblin unit type is exempt.
- Direct and 1d4+1 placements must preserve per-unit mechanics, independent
  targets, collision/navigation, dismissal/expiry cleanup, save/load, module
  disable, and optional-mod compatibility without cross-unit state.

## Hidden foundation checkpoint

The 2026-09-30 foundation appends 37 Dire Rat identities after the preserved
2,609-entry ledger: one unit, 18 logical placements, and 18 celestial or
fiendish execution children. The current manifest contains 2,646 identities,
of which 2,644 are active and two remain reserved. The runtime catalog now
registers 900 logical placements and deliberately suppresses 68 placements
belonging only to Dire Rat, Dog, Hyena, and Goblin Dog. The other 832 generated
placements remain published. This is a development boundary, not mechanical
or visual qualification.

The original Dire Rat painting and its deterministic export are recorded in
`assets-source/original-icons/expanded-summoning/PHASE2-SPRINT12-SOURCES.md`.
Its 37 exact family-share consumers are manifest-backed, but the icon has not
yet passed live menu or owner visual review. Dog, Hyena, and Goblin Dog retain
their existing icon records while hidden; their creature visuals still require
the Sprint 12 disposition above.

Repository validation passed, the complete Release domain suite passed all
1,955 tests, the clean Release build passed, and the strict standalone package
validator accepted the 276-file package. The package SHA-256 is
`691ECC757FA3DEB436AA2CAAE2E4EF64410BD76486B0F87CBDF96276B19B3E96`;
the built DLL SHA-256 is
`8A01E5E1E4E4B3FBEE98952F6B5F39C37AFE275D086D2BCC3A37631541C94582`.
No new Kingmaker launch was used to qualify this hidden metadata checkpoint.
The earlier guarded donor audit remains the runtime authority for the selected
native Dog rig, Filth Fever payload, and exact Goblin unit-type adaptation.
