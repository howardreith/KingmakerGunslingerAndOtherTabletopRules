# Sprint 6 disposition: saves, lifecycle cleanup and migrations

Status: **records-only disposition checkpoint, returned for review (2026-11-23).** Nothing is merged or released.

> Sprint 6's supported development implementation is complete for the accepted scopes. Historical incidents and the
> explicitly retained qualification gaps remain unresolved as listed. The evidence does not imply global
> qualification or release.

**Sources:**

- the epic `Gunslinger_Remediation_Epic_2026-11-15.docx` (lab directory, outside the repository), Sprint 6 mission and stories S6 T1–T8;
- the accepted checkpoint records `docs/GUNSLINGER-SPRINT6-*.md`;
- the register `planning/GUNSLINGER-REMEDIATION-STATUS.md`;
- `KNOWN-ISSUES.md`.

**Sprint 6 exit criterion (epic):** "Each incident has evidence-based attribution or remains explicitly blocked; changed persistence and migrations pass bounded native boundary checks." The incidents remain explicitly blocked, and each changed persistence or migration owner passed bounded native boundary checks for its stated scope. The exit is therefore met in that bounded sense. It does **not** mean every record is resolved.

**Categories kept apart throughout:**

- policy acceptance (an owner decision);
- source inference;
- automated checks (focused domain tests, production compilation);
- native execution (named loader-compat sessions);
- merge;
- release.

**Common native environment** unless a row says otherwise:

- installed Kingmaker 2.1.7b (Steam App ID 640820);
- UMM owner profile 0.33.0, in the disposable loader-compat game copy;
- fixture `KMCS_AFTER_NORMAL_012_MODONLY.zks` (`JamandisMansionThroneroom`), real-time with pause (RTWP);
- product-only profile.

**Exception:** A2 (RISK-03 Brown-Fur) staged Call of the Wild 1.14.4c-2.1 as its provider. One loader profile does not qualify other loader versions or profiles.

## 1. Eight-story outcomes

| Story | Original question | Player-visible result / selected policy | Accepted development scope | Evidence class (controlling record) | Remaining gap / retained limitation | Delivery | Next action / reopening condition |
|---|---|---|---|---|---|---|---|
| **S6 T1 / REP-05** | Why did the v0.0.88 Tenebrous affected save and a marker-free same-area control stall before the after-load callback? | None; no attribution | None. **BLOCKED** | Intake only (2026-11-22): no matching save, log or run directory on DATA (`docs/GUNSLINGER-SPRINT6-A1-CONTROLLED-RAGE-PERSISTENCE.md`; `docs/GUNSLINGER-SAVE-LOAD-FORENSICS.md`) | The incident is unattributed; a generic new save cannot close it | Not applicable | Reopen when the owner supplies `Quick_6.zks` (489,715 B, `B4D6D093…`) and `Quick_5.zks` (644,034 B, `980821EB…`), or their `artifacts/save-forensics` copies, plus the 20260820 run directories, mod stack, DLC and loader. Search is not repeated |
| **S6 T2 / REP-06** | What caused the November 11 buff-restoration failure through `Buff.SpawnParticleEffect` / `OnAreaLoaded`? | None; no attribution (null-FX causation withdrawn; null blueprint only a hypothesis) | None. **BLOCKED** | Intake only (2026-11-22): no log with both frames and no 2,680,892-byte log on DATA (`planning/release-audit-0.0.182/FINDINGS.md` GS-AUD-0003/0024) | Unattributed incident | Not applicable | Reopen when the owner supplies the original 2,680,892-byte `output_log.txt` (and `_prev` or crash folder), the save, the package hash and the machine |
| **S6 T3 / RISK-02** | Does Powerful Change keep its adjustment when its caster cannot be resolved after load? | A dismissed Brown-Fur caster's Powerful Change on another character now restores its exact owned adjustment after load (the baseline restored it at +4) | Accepted for the demonstrated dismissed-caster restoration repair | Native: baseline `20261122T184839Z` FAIL as expected on `caad210696`; candidate `4e11e6c25c` session `20261122T190544Z-owner-0.33.0-risk02-bf-restore` PASS 3/3; focused tests (`docs/GUNSLINGER-SPRINT6-B-POWERFUL-CHANGE-RESTORATION.md`) | Unexecuted: second active-effect roundtrip, same-target replacement by another caster, natural expiry, area transition, other spells/carriers, story removals, downgrade. Released-version applicability is source-inferred | Draft PR #203; not merged, not released | None unless a later review reopens it |
| **S6 T4 / RISK-03** | Can malformed owned Controlled Rage / Brown-Fur persistence abort save/load or lose recoverable state? | Malformed owned entries are contained per entry; valid siblings survive; invalid entries grant nothing; recoverable payload is kept where the engine delivers it | Accepted for both owners with the recorded engine limitations | Native: A1 `20261122T143026Z` PASS (product `f9c2440651`); A2 `20261122T173603Z-owner-0.33.0-risk03-bf-persist` PASS 3/3 (product `caad210696`, CotW provider); earlier FAIL/host-error sessions retained (`docs/GUNSLINGER-SPRINT6-A1-…`, `docs/GUNSLINGER-SPRINT6-A2-BROWN-FUR-PERSISTENCE.md` §10) | Engine-level limits:<br>• repeated properties collapse before owner code;<br>• number digits and date text can be lost;<br>• unrepresentable numbers can fail the parts-array load (`165726Z`).<br>Unexecuted: party-member load, downgrade, other spells/carriers, timer expiry.<br>**Not universal malformed-save recovery.** The declined engine-level proposals are not revived | Draft PRs #198, #200; not merged, not released | None unless a later review reopens a limitation |
| **S6 T5 / LIM-01** | Can vendor retirement subtract a player-sold copy that is indistinguishable from authored stock? | Retired publication stays retired; ambiguous materialized copies at Oleg, Dire Narlmarches and Pitax are preserved, not subtracted | Accepted under the policy preserving ambiguous materialized vendor stock while retiring publication | Native on constructed legacy-shaped vendor data: baseline `20261123T172326Z` reproduced the loss on `9e902dfa80`; candidate `634a6efdd3` session `20261123T173520Z` PASS 3/3; 7 model tests (`docs/GUNSLINGER-SPRINT6-D1-LIM01-VENDOR-RETIREMENT.md` §11) | • No organic legacy save and no UI trade.<br>• Old ambiguous stock stays buyable by policy.<br>• Earlier losses are not reconstructed.<br>• Cross-save equality is captured.<br>• Fault fallback is best effort; pin drift leaves native behavior.<br>• Capital Model D stock is out of scope | Draft PR #213; not merged, not released | None unless reopened |
| **S6 T6 / LIM-02** | Can old Earth Elemental preference, old Griffon duplicate Charge and the Bebelith marker be reconciled where ownership is provable? | Old Earth Elementals keep their target preference and old Griffons the duplicate Charge until summon expiry; the Bebelith marker is kept, inert (zero AC) | Retention policies and the current-build marker diagnostic accepted | Source inference for the ownership analysis; native **diagnostic** (current-build marker, not a pre-fix load) `20261123T182444Z` PASS 2/2 (harness build `df6bca90…`) (`docs/GUNSLINGER-SPRINT6-D2-LIM02-LEGACY-SUMMON-STATE.md` §8) | • **Authentic pre-fix-save qualification unexecuted.**<br>• Targeted Earth action substitution unimplemented.<br>• No native positive penalty control.<br>• Cleanup fix compiled only | Draft PR #214; no product change | Reopen if an admitted pre-fix save becomes available (exact saves named in D2 §2) |
| **S6 T7 / LIM-03** | Does the Thornstep retirement remove only never-held copies at the exact former source? | Never-held copies at the former source are removed, at most the recorded count; claimed-and-returned, missing-history and unrelated copies are kept | Accepted for the induced native qualification, as intentional behavior | Native on an **induced** route (dropped-loot bags with the exact former-source reference, constructed inputs): `20261123T185954Z` PASS 3/3 (harness `a3621eb0d0`, build `ea03eed3…`) on product `634a6efdd3` (`docs/GUNSLINGER-SPRINT6-D3-LIM03-THORNSTEP-RETIREMENT.md` §9) | • No natural Riverine Rise container, organic save or transition.<br>• Native-only comparisons are source-inferred.<br>• Cross-save identity relies on test-only markers.<br>• No fault or pin-drift execution.<br>• No census of map objects sharing the loot blueprint | Draft PR #215; no product change | None unless reopened |
| **S6 T8 / VER-02** | Do real hold, grapple, swallow, engulf and attachment terminal paths clean up? | Selected terminals release targets and owned constraints; saves keep ongoing summon holds (C2a R1); swallow release on inside death (C3a); reset on reload remains intentional | Representative subscopes accepted (C1, C2, C2a R1, C3a R1, C3b, C3c/G3, G4, G1/G2, G1-R R1). **VER-02 stays open** | Native per subscope; see the controlling records `docs/GUNSLINGER-SPRINT6-C-…`, `C2-…`, `C2A-…`, `C3A-…`, `C3B-…`, `C3C-…`, `G4-…`, `G1G2-…`, `G1R-…` | • G5 (real party exit) is fixture-blocked.<br>• Turn-based mode is unexecuted.<br>• 3–4 links, successful Dismissal and the stale-callback race are unexecuted.<br>• Xill/Scorpion are inspected-shared plus historical.<br>• Other recorded limits apply.<br>None of these is automatically a new mandatory grapple campaign | Draft PRs #204–#212; product changes in C2a and C3a; not merged, not released | G5 when an admitted transition fixture exists (section 4); otherwise none mandatory |

**Not "all resolved":**

- REP-05 and REP-06 are blocked.
- LIM-02's authentic legacy qualification is unexecuted.
- VER-02 is open, with G5 blocked.
- LIM-03 is qualified only on an induced route.
- No Sprint 6 work is merged or released.

## 2. Evidence and delivery map

**Observed 2026-11-23T19:31Z, read-only:**

- `origin/master` = `06abcc775e` (2026-11-22, "Merge pull request #202 … release-0.0.194-records").
- Latest release **v0.0.194** (published 2026-11-22T17:58Z); previous v0.0.193 and v0.0.192.

**The stack's relation to master:**

- The Sprint 6 stack forks from `dfc182aee7` (the v0.0.193 records merge).
- Master has **17 later commits**, including v0.0.194 (PR #199 Bag of Tricks item weight, #201 and #202 release).
- Every Sprint 6 candidate stages version 0.0.193.
- **Integration onto current master is an outstanding later obligation**, not done here.

**Live stack, verified:** each head is an ancestor of the next. All 15 PRs are **OPEN, draft, unmerged**. The PR head is the current branch tip, which includes acceptance records. The accepted record head is the head named in the record's acceptance section.

| PR | Base branch (verified parent head) | Live head | Checkpoint | Kind | Qualifying product (package / DLL) | Native session(s) | Accepted record head |
|---|---|---|---|---|---|---|---|
| #198 | `master` (fork `dfc182aee7`) | `cdb01ffe32` | A1 RISK-03 Controlled Rage | Product | `f9c2440651` (package `6bc93163…118e`, DLL `e7afabe4…ad96`); superseded `f4e5de5f92dd` (DLL `f373e027…`) | `143026Z` PASS; `081651Z`, `081904Z`, `082442Z` retained | `f3f91386f1` |
| #200 | #198 `cdb01ffe32` | `e0a41fd237` | A2 RISK-03 Brown-Fur | Product | `caad210696` (`59739a7f…0956` / `b651c69b…87b8`); superseded `bda6aa363c4e` (DLL `2e4933f4…`) | `173603Z` PASS 3/3 (CotW); `154318Z` PASS and FAIL/host-error `153741Z`, `154047Z`, `164332Z`, `165415Z`, `165726Z`, `173115Z` retained | `4d29e88c5f` |
| #203 | #200 `e0a41fd237` | `024db08b98` | B RISK-02 | Product | `4e11e6c25c` (`2608e9f5…374b` / `d813f62c…2cc4`); baseline `caad210696` | baseline `184839Z` FAIL (expected); `190544Z` PASS 3/3; fixture FAIL `183136Z` retained | `d44cc4002f` |
| #204 | #203 `024db08b98` | `350062e1ea` | C1 VER-02 single-link | Harness only | reuses `4e11e6c25c` | `200515Z` PASS (harness `33530ca8…`); `195339Z`, `200003Z` (launch 3 refused) retained | `01c0e2b476` |
| #205 | #204 `350062e1ea` | `fbb773db04` | C2 multi-link | Harness only | reuses `4e11e6c25c` | `205434Z` PASS; G5 probe compiled (`3b9d4498…`), not executed | `a5baf9cd99` |
| #206 | #205 `fbb773db04` | `afb4af0ba0` | C2a R1 save suspension | Product | `044355c2a9` (`47ec71d0…06fe` / `3746ff5a…a6b2`); superseded `21f46b2009` (DLL `c717efac…`) | reproduction `214832Z`, `215826Z`; R1 `231045Z` (plus `231327Z`, `231658Z`) | `4729014dba` |
| #207 | #206 `afb4af0ba0` | `1efff0fa3e` | C3a R1 swallow/engulf | Product | `9e902dfa80` (`704cb0b4…79c9` / `431f13da…5676`); baseline `044355c2a9` | `003442Z`; R1 `012518Z`; diagnostics `001335Z`, `001728Z` retained | `4d7aa6ec1f` |
| #208 | #207 `1efff0fa3e` | `05a8044706` | C3b crocodile/T-rex | Harness only | reuses `9e902dfa80` | `015820Z` PASS | `647af2c1ba` |
| #209 | #208 `05a8044706` | `631a66e5cf` | C3c / G3 | Harness only | reuses `9e902dfa80` | `024057Z` | `43b8a3a914` |
| #210 | #209 `631a66e5cf` | `69da3d490e` | G4 Stirge | Harness only | reuses `9e902dfa80` | `033626Z` PASS; `033015Z` launches 2–3 PASS, launch 1 FAIL (oracle defect) retained | `6cf8934591` |
| #211 | #210 `69da3d490e` | `13f80c22cd` | G1/G2 Barbed Devil / Crab | Harness only | reuses `9e902dfa80` | `144444Z` PASS 4/4; `142611Z`, `143658Z` FAIL retained | `6ec543dffd` |
| #212 | #211 `13f80c22cd` | `e16b46eb76` | G1-R R1 lethal terminals | Harness only | reuses `9e902dfa80` | `162752Z` PASS 5/5 (harness `cf5d90c2…`); characterization `152231Z`, `152834Z`, `153430Z`, `154102Z` and corrections `161325Z`, `161901Z`, `162428Z` FAIL retained | `cb8ef00a34` |
| #213 | #212 `e16b46eb76` | `5fdc422265` | D1 LIM-01 | Product | `634a6efdd3` (`862027f1…0fdc37` / `65a0b60f…5c43ef`); baseline `9e902dfa80` | baseline `172326Z`; `173520Z` PASS 3/3 | `61f18c908c` (acceptance records `5fdc422265`) |
| #214 | #213 `5fdc422265` | `d3659698e5` | D2 LIM-02 | Harness only (diagnostic) | reuses `634a6efdd3` | `182444Z` PASS 2/2 (harness build `df6bca90…`; cleanup fix `0970dc9c…` compiled only) | `189b68a9fc` (acceptance records `d3659698e5`) |
| #215 | #214 `d3659698e5` | `c1a67ab910` | D3 LIM-03 | Harness only | reuses `634a6efdd3` | `185954Z` PASS 3/3 (harness build `ea03eed3…`) | `f6bcd2f638` (acceptance records `c1a67ab910`) |
| this closeout | #215 `c1a67ab910` | see PR | Disposition | Records only | — | — | — |

**Kinds of checkpoint:**

- **Product-changing:** A1, A2, B, C2a (R1), C3a (R1), D1.
- **Harness-only qualification:** C1, C2, C3b, C3c, G4, G1/G2, G1-R, D2, D3.
- **Records-only acceptance updates:** the later commits on each branch after its qualifying commit, and this closeout.

**What each PASS is evidence for:**

- The final D1 package contains every earlier owner's code. **Earlier owners were not requalified on it.** Each PASS above is evidence for the bytes and stated scope it ran on. That the D1 package contains the same source is a separate claim of source equivalence; it was not checked natively.
- Earlier FAIL, host-error, refused and superseded records keep their labels in their own records.

## 3. Residuals

| Class | Item | Missing input or reopening condition | Player-visible implication | Blocks |
|---|---|---|---|---|
| Missing original incident inputs | REP-05 Tenebrous stalls | `Quick_6`/`Quick_5` originals (hashes above), 20260820 run directories, incident mod stack/DLC/loader | Unknown: an old save could still stall; unattributed | Individual qualification of REP-05; epic completion. Not development acceptance of other stories |
| Missing original incident inputs | REP-06 buff-restoration failure | The 2,680,892-byte log, the save, the package hash, the machine | Unknown; unattributed | REP-06; epic completion |
| Missing authentic legacy fixtures | LIM-02 Earth/Griffon/marker on a pre-fix save | An admitted save from v0.0.101–v0.0.181 (Earth), v0.0.161–v0.0.179 (Griffon) or pre-v0.0.182 (marker); production not admitted or attempted | An old summon keeps its old preference or duplicate Charge until expiry; the marker shows but changes no AC | Authentic qualification only; the retention dispositions are accepted |
| Missing authentic legacy fixtures | LIM-01 and LIM-03 organic saves | An organic old save with the retired vendor stock or a generated Riverine Rise container | None expected beyond the accepted policies | Nothing mandatory |
| G5 transition fixture | VER-02 real party exit (`Sweep(true)`, pre-exit autosave) | An admitted, disposable, product-only-loadable save in an area with an unscripted, unrestricted exit to another local area (C2 record §6); the probe is compiled (`3b9d4498…`), not executed | Holds across a real area exit are source-inferred | Closure of VER-02; not the accepted subscopes |
| Accepted policy/engine limitations | RISK-03 engine conversion limits; LIM-01 ambiguous stock kept; LIM-02 until-expiry retentions; LIM-03 count limit; RISK-02 untested lifecycles | None; reopen only on new evidence or an owner decision | As described per story | Nothing |
| Unexecuted representative or compatibility cases | Turn-based mode; other loader versions and profiles (only UMM owner-0.33.0 ran); provider matrices beyond CotW for A2; downgrade; party-member loads; 3–4 hold links; successful Dismissal | A later selected candidate milestone (Sprint 9) | Behavior there is not qualified | Integrated release qualification (Sprint 9), not development acceptance |
| Later integration and release obligations | The stack is unmerged and 17 commits behind master (v0.0.194); candidates stage 0.0.193 | Owner-authorized integration (rebase or merge onto master) and a separately authorized release under the current procedure | Players have none of Sprint 6 yet | Release; not development acceptance |

## 4. Recommended next sprint (from the original epic)

**Sprint 7: "Repair concrete presentation and audio defects."**

- **Dependencies:** mechanical paths used for observation must work. Asset-only work can run beside Sprints 4–6.
- **Exit:** concrete defects are repaired or specifically disproved; provisional audio and engine-template limits have explicit dispositions.
- **Not worked elsewhere:** none of its records appears in the register, and no master commit since the epic addresses them.

| Story | Record | Scope | Needs |
|---|---|---|---|
| S7 T1 | REP-04 | Investigate the premature Ghaele dissolve on a fresh ordinary summon (idle, movement, combat, lifetime; bright and dim scenes) before changing materials | Native capture; a non-reproduction does not close it |
| S7 T2 | VIS-01 | Remove the 48 zero-area Nalfeshnee wing-root triangles at the deterministic source `assets-source/original-models/sprint39-nalfeshnee/generate_sprint39_nalfeshnee.py` | Pinned Blender pipeline; native idle/walk/attack/turn captures; **owner visual acceptance** |
| S7 T3 | VIS-02 | Salamander waist-pleat topology and finger normals (19 of 12,924 corners) | **Named asset prerequisite:** the exact private capture and authoring inputs for `generate_human_salamander.py`; no substitute model |
| S7 T4 | VIS-03 | Trample/Overrun target-window placeholders ("0 feet", school "None") via a scoped presentation override, or an explicit engine limitation | Desktop and relevant controller windows |
| S7 T5 | VIS-04 | Ankylosaurus impact cue at the real impact event | A properly sourced cue; **listening acceptance** |
| S7 T6 | VIS-05 | Owned dust visual for the Dust Mephit breath | A properly sourced dust effect (**asset choice**); visual acceptance |
| S7 T7 | VIS-06 | Trumpet Archon cue: keep or replace | **Owner listening decision** |

**Recommended first bounded checkpoint: S7 T2 (VIS-01), Nalfeshnee wing-root degenerates.**

- **Why it is bounded:** the defect is exactly counted, the source generator and pinned toolchain are in the lab, and acceptance is measurable (no zero-area triangles, holes, flipped faces or isolated vertices). Scale, reach and gameplay are unchanged.
- **Deliverable:**
  - regenerate through the existing pipeline;
  - record source and export hashes;
  - run the mesh validation;
  - capture both wing roots in native idle, walk, attack and turns;
  - return for **owner visual acceptance**, recorded separately from the technical PASS.
- **Not to be started until authorized:** S7 T3 (needs its private inputs), T5–T7 (listening or asset decisions) and T1 (native investigation) each need their own authorization. No Sprint 9 or integrated release qualification is started.

## 5. Validation of this checkpoint

Records only:

- no product build, package or game launch;
- no test suite or compatibility matrix;
- no merge, release, rebase, history rewrite or reviewer enrollment;
- PR states, bases, heads, ancestry, master and release were read once via `gh` and `git` (observation time above);
- record links and identities were checked against the listed records.
