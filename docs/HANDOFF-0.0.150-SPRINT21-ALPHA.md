# Handoff — Expanded Summoning Sprint 21 (v0.0.150), alpha

**Branch** `codex/expanded-summoning-sprint21-bebelith-giant-crab`
**Head** `20efcd71` · **Alpha tag** `v0.0.150-alpha.1` at `adddef43`
**PR** [#36](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/36) — still a draft, deliberately
**Master base** `d0ceafae` (the v0.0.149 merge) · 27 commits ahead

This stops mid-closure at the owner's instruction, with no further test runs.
Read §1 and §5 first; everything else is detail.

---

## 1. Where this actually stands

Sprint 21 is **functionally complete and published**. Both creatures are
implemented, both have original bodies, the publication is applied, and the
guarded reviews pass. What is **not** finished is the final gate of the whole
Expanded Summoning series: the five-profile compatibility matrix has never
completed a clean pass.

| Gate | State |
| --- | --- |
| Registration, rules, icons, bodies, publication | **done** |
| Deterministic tests / repo validation / package | **2558/2558, PASS, 354 members** at `adddef43` |
| Sprint 21 guarded review | **44/44**, both combat modes, both bodies |
| Publication gate | **44/44** against the published surface |
| Exhaustive player-path census | **10/10** — 1063/1063 paths, 29/29 wrappers |
| Sprint 18 / 19 / 20 regressions | **29/29 · 39/39 · 22/22** |
| Whole-roster persistence | **125/125** |
| Module-disabled | **11/11** |
| **Five-profile compatibility matrix** | **not passing — this is the remaining work** |
| Release (merge, tag v0.0.150, publish) | not started |

`20efcd71`, the branch head, is **compiled but not tested** — see §3.1. The
alpha ZIP is built from `adddef43`, the last fully verified commit.

The Steam installation is at the owner's baseline: **0.0.117, 136 files, DLL
`FD2FC61C250B13857D81ACC197A896450F5B242EE392FA7192B01201E908F35F`**, no open
compatibility transaction, lock released, no Kingmaker process. Every guarded
run restored the live mod tree to
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.

---

## 2. What the sprint shipped

**Giant Crab** — new. N Medium vermin (aquatic), 3d8+6 for 19 hp, AC 16/11/15,
Fort +5 Ref +2 Will +1, speed 30 ft. and swim 20 ft., two claws +4 (1d4+2 plus
grab), Space/Reach 5 ft., constrict 1d4+2, CMB +4 (+8 grapple), CMD 15 (27 vs.
trip), Perception +4, immune to mind-affecting. Seven roots, **Summon Nature's
Ally III only**. It is the first creature in the roster with a Nature's Ally
tier and no Summon Monster one, so it owns **no execution children**.

**Bebelith** — a full overhaul of a creature released in v0.0.147, with its
identity deliberately preserved: the unit and all three Summon Monster roots
keep the exact GUIDs they shipped with (`5dee1a17` tier 7 single, `1626468c`
tier 8 1d3, `e08d19b1` tier 9 1d4+1), and the released key `bebelith` is
**not** renamed even though sources print "Bebilith" — a rename would retire a
GUID and migrate every save holding one.

What changed behind those three roots:

- **Chassis correction.** The live creature read Fortitude +11 against a
  printed +15, Will +11 against +7 and 162 hp against 150. The native outsider
  class swaps the good and poor saves and is one hit point per die tougher.
  One creature-owned carrier of the *differences*; the shared class untouched.
- **Penetrating Strike**, replacing an invented +2 vs. chaotic evil outsiders:
  chaotic and magic on every natural attack, cold iron and good vs. demons only.
- **Rot** — bite-only injury effect, Fortitude DC 23 derived live from
  Constitution, 2 Con a failed save, five exposures, two consecutive successes
  to cure. Never from a claw, the web, or armour dismantling.
- **Web** — now through one parameterized seam the Sprint 6 Giant Spider web
  also uses, with that creature's five released values proved to arrive by name.
- **Dismantle Armor** — the engine's own sunder manoeuvre plus a Reflex save at
  the printed DC, where the released build had neither and used an invented 25.
- **An original body** — the first in the series authored for a creature that
  already ships wearing something (the native Doomspider).

**Surface.** Registered 1056 → 1063, ledger 3039 → 3062 (23 appended
identities), visible **1085 → 1092** — up by the crab's seven, not ten, because
the Bebelith's three roots were already on the menu. Publication was the
removal of one suppression key; no GUID moved.

---

## 3. Exactly what remains

### 3.1 Verify the branch head (5 minutes)

`20efcd71` has a clean Release compile and nothing else. Run the normal gates:

```
python tools/validate_repository.py
dotnet build tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj -c Release
./artifacts/tests/Release/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.exe
```

**Watch for a stale test binary.** `dotnet build` of the test csproj can report
success in under a second and still leave the old exe if it was locked; I
reported a false 2558/2558 that way once in this session. Check the exe's
mtime is after your edit before trusting a pass.

### 3.2 Finish the five-profile compatibility matrix — the real remaining work

```
powershell -File scripts/Invoke-ExpandedSummoningCompatibilityMatrix.ps1
```

Seven steps, ~90 minutes, seven game launches. **Do not touch the git tree
while it runs** — every step after the first checks for a clean tree, and an
edit mid-run makes the rest fail with *"Runtime execution requires a clean Git
state."* **Do not kill it mid-step either**: it leaves an open compatibility
transaction and the dev build deployed. I did both, and the recovery is:

```
powershell -File scripts/compatibility/Restore-KingmakerCompatibilityProfile.ps1 `
    -RunId '<compat-...-id>' -Confirm:$false
```

The run id is the newest directory in `C:\Dev\KingmakerGunslingerLab\compatibility-state`.
Afterwards verify 0.0.117 / 136 files / DLL `FD2FC61C…F35F`, no
`Mods.kmg-compat-*` directory under the Kingmaker install, and
`compatibility-state\compatibility.lock` absent.

**State at the last run (`matrix4`, head `adddef43`):**

| Step | Result |
| --- | --- |
| boundary-summoning-on | 40/47 — 7 failures, none Expanded Summoning (§4.1) |
| boundary-summoning-off | 40/47 — the identical 7 |
| profile-gunslinger-only | mod-load-smoke PASS, optional-mod-compat PASS, inventory **49/50** |
| profile-call-of-the-wild | not reached (I stopped the run) |
| profile-arms-armor | not reached |
| profile-toggle-custom-soundpacks | not reached |
| profile-high-risk-combined | not reached |

The one remaining inventory failure was `expanded-summoning-bebelith`, reported
as `claws2d4=MISMATCH;bite2d6=MISMATCH` with every other part ok. **That is
what `20efcd71` fixes** — so the next matrix run should clear it. Expect the
inventory observer to reach 50/50 and the two profiles that schedule
`disposable-expanded-summoning` (gunslinger-only and high-risk-combined) to run
it for the first time; no earlier attempt got that far, because the launcher
stops a profile's list at its first non-PASS.

### 3.3 Decide the boundary-step question (§4.1) — needs a judgement call

The two boundary steps fail seven gates that have nothing to do with Expanded
Summoning. I believe they should not block this release, and §4.1 sets out the
evidence. **That is a call the owner may want to make**, not something to
assume.

### 3.4 Re-take the closure on one candidate

The mission wants the series closure on *one immutable release candidate*. It
currently spans three: regressions on `7b37df94`, persistence on `cf4bbc4a`,
module-disabled on `6b1ef853`. Once the matrix is clean and no further source
change is needed, re-take on the final head, under one snapshot each:

```
# Scenario batch 1 (one snapshot, one restore)
scripts/Invoke-ExpandedSummoningRuntimeScenario.ps1 `
  -Scenario @('disposable-expanded-summoning-player-path',
              'disposable-expanded-summoning-sprint18-review',
              'disposable-expanded-summoning-sprint19-review',
              'disposable-expanded-summoning-sprint20-review',
              'disposable-expanded-summoning-sprint21-review') `
  -ExpectedVersion '0.0.150' -SaveName 'KMG_AUTOMATION_WORKING' `
  -CompletionTimeoutSeconds 1800 -TimeoutSeconds 1500 -Confirm:$false

# Whole-roster persistence
#   -ScenarioParameterMap @{ '<each scenario>' = @{ persistenceScope = 'whole-roster' } }
#   on working-save-expanded-summoning-prepare + ...-verify-cleanup

# Module-disabled, which MUST be a single-scenario run
#   -Scenario 'working-save-expanded-summoning-verify-cleanup'
#   -ExpandedSummoningDisabled -ScenarioParameters @{ persistenceScope = 'whole-roster' }
```

Ready-made runners are in the session scratchpad; the parameter shapes above
are the ones that actually work. **The `-SaveName` trap:** pass the save with
the typed `-SaveName` parameter; anything else goes in `-ScenarioParameterMap`.
`working-save-expanded-summoning-creature-review` additionally requires
`creatures` and optionally `quantity`, which is why it errored out of my first
closure batch — it is the party-camera art review and is *not* part of the
closure requirement.

**No `playerPathScope` parameter on the player-path scenario** — omitting it is
what makes the census exhaustive over all 1063 roots.

### 3.5 Then release

1. Update `validation/static-validation.json` → `expandedSummoningSprint21150`:
   add the closure evidence ids to `runtimeEvidence`, bump
   `guardedRuntimeRunsSpent` (17 at the last count), set
   `compatibilityRuntimeQualificationPending: false` once the matrix passes,
   and append a `seriesClosure` note.
2. Update PR #36 — the body is already written (§6); mark it ready for review.
3. Merge to master. **No force-push, no history rewrite, no direct push** — the
   only sanctioned push is
   `codex-policy/Push-KingmakerGunslinger.ps1`, run from inside the repository
   working directory (it resolves the toplevel from the cwd, so running it from
   the lab root fails).
4. Tag and publish **v0.0.150** with the ZIP, `SHA256SUMS.txt` and
   `release-manifest.json`; mark it Latest; verify the download; restore the
   installation; record the ledger.
5. **Stop. There is no Sprint 22.**

---

## 4. The things that will confuse you

### 4.1 Seven boundary-step failures that are not this sprint's

`observe-feature-module-settings` fails the same seven gates in both boundary
steps — with Expanded Summoning **on** and **off**, identically. That symmetry
is the point: none of them can be caused by Expanded Summoning.

- **Four are pre-existing by ten releases.** The same scenario at **v0.0.140**
  (evidence `20260927T1030110774186Z-observe-feature-module-settings`) already
  failed `feature-module-brown-fur-publication-gate` and all three
  `teleportation-scroll-template-*`.
- **Three are vendor-row counts** — `capitalGunslinger` 10→7,
  `easternVendors` 66→54, `spearVendors` 34→28 — in the gunslinger,
  eastern-weapon and elven-spear gates, whose own expectations read *"plus six
  rows per installed BTSL table"*. The `gunslinger-only` profile stages no
  optional mods, so those rows are legitimately absent.

Sprint 21's whole diff against master touches no vendor, loot, weapon or
teleportation data. The five files matching those names are version strings and
one ledger count (`git diff d0ceafae -- tools/validate_spear79.py` etc.).

**Unresolved:** whether the three vendor gates are profile-sensitive (my
reading) or regressed in a released version between 0.0.140 and 0.0.149. One
guarded run of `observe-feature-module-settings` *outside* a compatibility
profile would settle it: counts of 10/66/34 mean profile sensitivity, 7/54/28
mean a released-version change. Either way it is not Sprint 21.

### 4.2 Why so many observer defects surfaced at once

Two of the closure gates — the whole-roster persistence matrix and the
compatibility matrix — are **tranche-closure** gates that had not run since
**v0.0.141**. Sprints 18, 19, 20 and 21 all landed in between. So this closure
is the first thing to look at several contracts, and it found them all at once.
Everything in §5 under "observer" is that, not a creature defect. **No creature
finding was made in the entire closure.**

### 4.3 Dismantle Armor's engine gap

A reflection probe found `CombatManeuver.SunderArmor` present and **no**
item-durability component reachable by a mod — no broken condition, no
durability value, no item-damage dice (`SunderArmorBuff`, `EquippedItemDamage`,
`DamageDurability`, `ItemDamagePhysicalDice` are all absent). So the manoeuvre
is the engine's own and the effect is a wearer-side state cancelling the
piece's whole AC contribution. The object survives untouched, which satisfies
every mandatory safety clause by construction. Recorded as
`ITEM_DESTRUCTION_UNMODELED`; **do not invent a global item-durability system.**

Also measured: **Kingmaker refuses to unequip armour while a unit is in
combat** — `ArmorSlot.CanRemoveItem()` returned false in both combat modes and
after the summon was dismissed. The review asserts the disposition the engine
permits.

### 4.4 Kingmaker stacks poison

Rot's per-round saves read 27, 31, 33 against a printed 23. That is the engine's
own poison stacking (~+4 per extra dose) because the creature lands a bite every
round. An instrumented pin reported `contextDcBefore=23; wrote=True;
contextDcAfter=23` — the context already held the printed number. The review
asserts the printed DC on rot's **application** save and records the stacking.
My first diagnosis of this was wrong and the correction is in
`BebelithRot.cs`'s documentation as well as the code.

---

## 5. Every defect this closure found, and what it was

All observer or driver defects. None was a creature.

| # | Defect | Fix commit |
| --- | --- | --- |
| 1 | Sprints 18/19/20 each checked "nothing withheld" by **pinning the published count to their own release's number** (1034/1044/1056). Every later sprint that publishes correctly moves it. Now the invariant, with the old literal as a non-regression floor. | `678a92b5` |
| 2 | A **natural 20 on an excluded limb** was not a spoiled trial. The rend fixtures retried when a *chosen* limb missed but not when a limb told to miss at −94 rolled a 20 and hit; rend then correctly fired on a sequence never arranged. Predicate made symmetric. | `678a92b5` |
| 3 | Same shape on a save: a **natural 1 failing a chosen-made save**. Sprint 20's local predicate covered only the natural-20 direction. Both now shared. | `678a92b5` |
| 4 | Sprint 18 **retried a single sequence**; the rend tracker is scoped to the attack *command*, so abandoned attempts still counted. Adopted Sprint 19's atomic-trial shape. | `7655c06b` |
| 5 | The split-target cases' **second target was never reset** between trials — it accumulated damage, died, and the engine threw `NullReferenceException` from `UnitAttack.GetApproachRadius`. | `7655c06b` |
| 6 | `Sprint18AttemptRolls` took **one cursor over a concatenation of two growing lists**; the first grows in the middle, so the window returned stale rolls from abandoned trials. A cursor each. | `7b37df94` |
| 7 | A repeated trial **began with a live attack command**; the engine merges into it and throws when its target is gone. Cleared in the trial preamble. | `7b37df94` |
| 8 | The resource-receipts observer **never asked `PrimateVisualAttachment` what it owns**, so 16 resources of the apes, Girallon and Xill came back `UNRESOLVED` and the gate failed closed. The resources were always correctly released. | `cf4bbc4a` |
| 9 | `all-identities-context-duration` demanded all 114 creatures be live **whatever the module said**, while recording `moduleEnabled` and never reading it. Now: module on → all live; module off → **none** may instantiate. | `6b1ef853` |
| 10 | The compatibility matrix wrote its report **into `scripts/`, inside the step loop**, so step 1 dirtied the tree and steps 2–7 refused to run. Report moved to the evidence tree. | `f746ef42` |
| 11 | The inventory observer's **qualified-reference table stopped at Sprint 17** — 24 project facts on project units unlisted (Sprint 18's 3, 19's 4, 20's 6, 21's 11). | `16977f32` |
| 12 | The inventory observer's **Bebelith census described the pre-overhaul creature**: it required a flat `AddStatBonus` of −2 that this sprint removed, and the released limb order. Also made to report its parts instead of one word. | `16977f32`, `20efcd71` |

Two product corrections worth noting:

- The released **v0.0.149 record's `packageMemberCount` had been changed from
  349 to 354** — a number belonging to *this* release written onto the previous
  one. Restored, and `expandedSummoningSprint20149` is now inside the gate's
  frozen-record boundary. (`4479ae78`)
- The rot graph's two **native asset ids were swapped**, which rolled back 2432
  registrations on a type mismatch. The fix is an equality between two files
  rather than a third transcription. (`d0026500`)

---

## 6. PR #36

Still a **draft**. A full body is drafted at
`<session scratchpad>/pr36_body.md`; if that is gone, the material is §2, §4
and §5 above. It ends with the required
`🤖 Generated with [Claude Code](https://claude.com/claude-code)` line.

Commit messages must end with
`Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>`.

---

## 7. Standing constraints

- Every Kingmaker launch goes through the repository's guarded **Steam App ID
  640820** mechanism via
  `scripts/Invoke-ExpandedSummoningRuntimeScenario.ps1` (or the compatibility
  launcher). Never launch `Kingmaker.exe` directly; never load or write
  `KMG_AUTOMATION_BASELINE`; never edit, parse, copy or replace saves by hand;
  never leave a development build deployed.
- Only the authorized disposable/working saves. Restore installation, settings,
  saves, process, lease, lock and staging state after every transaction.
- No force-push, history rewrite, destructive reset, or direct push bypass. The
  only sanctioned push is the policy wrapper, whose `AllowedBranches` already
  contains this branch.
- `UnityEngine.PhysicsModule.dll` in the private reference bundle is a
  machine-local build reference only: verify its SHA-256 against the installed
  Steam runtime at intake, record source path and hash in local build
  provenance, refresh read-only if absent or mismatched. Never commit, package,
  upload or expose it; never alter the installed Steam copy.
- No proprietary game mesh, texture or animation clip may be redistributed.
- Observer and fixture corrections are ordinary engineering work and need no
  further owner permission when they are request-local, do not weaken the
  requirement, do not change gameplay, preserve failed evidence, and are
  removed or isolated safely. **A stale expectation, serialization typo, bad
  timing window or observer bug is not an owner decision.**
- Stop and ask only for: a real owner-level rules or balance decision; an
  inability to implement mandatory Dismantle Armor safety without a global item
  subsystem; an unrecoverable restoration or safety failure; a
  proprietary-asset or legal boundary; or no remaining safe evidence-supported
  approach.

---

## 8. Useful paths

| What | Where |
| --- | --- |
| Frozen sprint contract | `planning/EXPANDED-SUMMONING-SPRINT21-CONTRACT.json` |
| Version gate | `tools/validate_expanded_summoning_sprint21150.py` (+ its `test_` twin) |
| Release record | `validation/static-validation.json` → `expandedSummoningSprint21150` |
| Release notes | `docs/RELEASE-NOTES-0.0.150.md` |
| Guarded runtime evidence | `C:\Dev\KingmakerGunslingerLab\runtime-evidence\` |
| Matrix reports | `…\runtime-evidence\expanded-summoning-compatibility\` |
| Compatibility transactions | `C:\Dev\KingmakerGunslingerLab\compatibility-state\` |
| Restoration records | `…\runtime-evidence\expanded-summoning-restoration\` |

Key evidence ids:

- Sprint 21 review (44/44) `20261011T0519575295937Z-disposable-expanded-summoning-sprint21-review`
- Publication gate (44/44) `20261011T0607210899913Z-disposable-expanded-summoning-sprint21-review`
- Donor census (9/9) `20261011T0100003563905Z-observe-expanded-summoning-sprint21-census`
- Closure batch on `7b37df94` `20261011T0832337615067Z…player-path`, `…0850160737552Z…sprint18-review`, `…0857438162151Z…sprint19-review`, `…0907487777628Z…sprint20-review`, `…0912175221409Z…sprint21-review`
- Persistence (125/125) `20261011T0945163763144Z-working-save-expanded-summoning-verify-cleanup`
- Module-disabled (11/11) `20261011T1005587823919Z-working-save-expanded-summoning-verify-cleanup`
