# MENU-02: standalone elemental rules proposal

**For owner review; implementation is not approved.** Owner direction: “Prepare
a separate rules-change proposal for review before hiding them.” No entries
are hidden by this proposal. Preserve current entries and original invocation
objects while the decisions below remain open. This document changes no rules,
production code, test, package or save.

## 1. Evidence and limits

Basis: the [installed-engine contract audit](CLEANUP2-ELEMENTAL-CONTRACT-AUDIT.md),
[consumer/seam inventory](CLEANUP2-ELEMENTAL-SEAM-NOTES.md), and immutable review 4:

| Identity | Value |
| --- | --- |
| Native run | `20261022T0015492013641Z-548aebf9d0524b818f2e1c3ca6260101` |
| Evidence directory | `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261022T0015491813663Z-disposable-expanded-summoning-cleanup2-native-review` |
| Candidate source commit | `4c2ad09d6e1c174078c31dd5e99ff4f4d3553ab1` |
| Loaded DLL SHA-256 / MVID | `74801e0d7931ce538fbad370ae5477408df6cc1c0a0ebd9e92c6a8d7ab62dca9` / `5217dedf-0762-4682-b07c-7548bd32e94a` |
| Observation JSON SHA-256 | `8eecf73fdaf96788288ad474f424bef33f85867d0d2c2033f69921f691108f28` |
| Result JSON SHA-256 | `c3d8dcc2293a4455f018e70929e1e5196f6dcdf133229dc8f749e75a3f90b1c1` |

The run remains **FAIL: 173 PASS, 1 FAIL, 78.760 seconds**. Its final elemental
iterator cleanup raised `NullReferenceException`; useful earlier observations
do not repair that result. The diagnostic census completed 12 parent and 48
child comparisons with **zero equivalent pairs**. Both exact PC/console learning
hooks were observed. All 52 actual book/list helper probes retained their source
entry; another eight cases were explicit exceptions: Bard VII/VIII source absent,
and six Wizard Conjuration cases with no actual matching book. These are not
sixty successful suppression probes.

Selected and known legacy rows/references were retained in those probes. One
Small Air legacy cast and one canonical SM II cast each produced one observed
native cast/spawn and debited its original preparation from 7 to 6. This proves
neither equivalence across 24 children nor natural learning UI, old-save reload,
domain/item/NPC execution or completed final cleanup. Raw graphs/private native
bytes remain outside Git.

## 2. Exact rank identities and alternative routes

Each standalone parent offers Air, Earth, Fire and Water. The canonical comparison
uses the same rank's **one-creature** choice, not a higher-tier quantity branch.
SM and SNA are separate routes; same rank/tier does not grant access in the same
book or through the same special slot.

| Rank / level | Standalone parent GUID | SM parent GUID | SNA parent GUID |
| --- | --- | --- | --- |
| Small / II | `970c6db48ff0c6f43afc9dbb48780d03` | `1724061e89c667045a6891179ee2e8e7` | `298148133cdc3fd42889b99c82711986` |
| Medium / IV | `e42b1dbff4262c6469a9ff0a6ce730e3` | `7ed74a3ec8c458d4fb50b192fd7be6ef` | `c83db50513abdf74ca103651931fac4b` |
| Large / V | `89404dd71edc1aa42962824b44156fe5` | `630c8b85d9f07a64f917d79cb5905741` | `8f98a22f35ca6684a983363d32e51bfe` |
| Huge / VI | `766ec978fa993034f86a372c8eb1fc10` | `e740afbab0147944dab35d83faa0ae1c` | `55bbce9b3e76d4a4a8c8e0698d29002c` |
| Greater / VII | `8eb769e3b583f594faabe1cfdb0bb696` | `ab167fd8203c1314bac6568932f1752f` | `051b979e7d7f8ec41b9fa35d04746b33` |
| Elder / VIII | `8a7f8c1223bda1541b42fd0320cdbe2b` | `d3ac756a229830243a72e84f3ab050d0` | `ea78c04f0bd13d049a1cce5daf8d83e0` |

### Differences requiring decisions

**D — selected-child descriptors.** Every standalone child has Summoning plus
its elemental descriptor: Air = Electricity, Earth = Acid, Fire = Fire, Water =
Cold. Both observed canonical routes have Summoning only. Descriptor-sensitive
rules and metamagic can distinguish them. Removing the elemental descriptor, or
adding it to a canonical child, is an explicit rules decision.

**N — SNA alignment and spawn buff.** Every observed SNA child adds an executed
`ContextActionSetSummonAlignment(Caster)` absent from the standalone child. That
can change the summoned actor for another caster alignment. Each also changes
the executed native spawn-buff reference:

| Rank | Standalone buff | SNA buff |
| --- | --- | --- |
| Small | `6fcdf014694b2b542a867763b4369cb3` | `2c3b98e56c1b4e9479f692d7ef95b2d2` |
| Medium | `6fcdf014694b2b542a867763b4369cb3` | `25629d7e78016a340b0e50818b6d8bb5` |
| Large, Huge | `50d51854cf6a3434d96a87d050e1d09a` | `25629d7e78016a340b0e50818b6d8bb5` |
| Greater, Elder | `0dff842f06edace43baf8a2f44207045` | `932d27490e1701548a48b4cbc2f2caac` |

**M — SM template-related execution.** Small/Medium canonical children retain
an Evil-caster conditional in AfterSpawn; Large/Huge/Elder retain an Evil-caster
conditional around spawn branches. Greater has a direct spawn; its larger raw
comparison count alone does not prove changed spawn behavior. Medium also
changes the initial spawn buff from the low native band to the middle band.
These inherited native SM payloads execute;
they cannot be discarded as inactive parent data. Their complete buff contents,
all branch outcomes and applicability to elemental units need a bounded native
audit before choosing equivalence or a deliberate change. This proposal does
not assert that every buff-ID difference changes stats, or authorize adding a
celestial/fiendish template merely because SM has a conditional.

**U — unit identity.** Standalone children summon native units; canonical choices
summon their project units. The comparator's species alias is not a full
stat/fact/AI equivalence proof. Observed signatures agree on one creature,
CL-based extendable rounds, linked lifetime, control flag and summon pool;
that agreement does not settle D/N/M/U or actor-specific effects.

### Individual child dispositions

All 24 remain **preserved pending owner decision**, with D/M/N/U unresolved.
Counts are raw SM/SNA diagnostic differences, not counts of distinct rules.
The exact observed disposition is `preserve-contract-difference:<first path>`;
no child received an equivalence waiver.

| Child | Exact native child GUID | SM / SNA differences | Current disposition |
| --- | --- | --- | --- |
| Small Air | `9cc6b61eba880b944a8f489c44640b5c` | 78 / 5 | Preserve Electricity; D/M/N/U |
| Small Earth | `69b36426bb910e341a943f101daed594` | 78 / 5 | Preserve Acid; D/M/N/U |
| Small Fire | `d8f88028204bc2041be9d9d51f58e6a5` | 78 / 5 | Preserve Fire; D/M/N/U |
| Small Water | `107788f47c4481f4db6da06498b28270` | 78 / 5 | Preserve Cold; D/M/N/U |
| Medium Air | `cce8eca4409e1be4794ce94597034f38` | 79 / 5 | Preserve Electricity; D/M/N/U |
| Medium Earth | `4ed414d36753459499578c9c3eb38b17` | 79 / 5 | Preserve Acid; D/M/N/U |
| Medium Fire | `7b30211b83d55194db872b6c9c0d9cc1` | 79 / 5 | Preserve Fire; D/M/N/U |
| Medium Water | `2c86e646a17f6064994ba7e8da4dd77c` | 79 / 5 | Preserve Cold; D/M/N/U |
| Large Air | `16fff43f034133a4a86e914a523e021f` | 246 / 5 | Preserve Electricity; D/M/N/U |
| Large Earth | `202a024bc7c224d429a9cfb14fa9c3d8` | 246 / 5 | Preserve Acid; D/M/N/U |
| Large Fire | `b3a203742191449458d2544b3f442194` | 246 / 5 | Preserve Fire; D/M/N/U |
| Large Water | `9374c81817be79f4e92b48a4aa5ded6e` | 246 / 5 | Preserve Cold; D/M/N/U |
| Huge Air | `2c26e78ca33e83843b1aa3f6783877bd` | 246 / 5 | Preserve Electricity; D/M/N/U |
| Huge Earth | `989d3ed13d27d054ea2d26ab4956d075` | 246 / 5 | Preserve Acid; D/M/N/U |
| Huge Fire | `4814f8645d1d77447a70479c0be51c72` | 246 / 5 | Preserve Fire; D/M/N/U |
| Huge Water | `4695c4674e62eb3479fa03f0b47cc6a8` | 246 / 5 | Preserve Cold; D/M/N/U |
| Greater Air | `a4849d6bd536ade48a3839a4ad960a8b` | 196 / 5 | Preserve Electricity; D/M/N/U |
| Greater Earth | `3ecd589cf1a55df42a3b66940ee93ea4` | 196 / 5 | Preserve Acid; D/M/N/U |
| Greater Fire | `35c6b4497a5bfd24aabe0cf28507abef` | 196 / 5 | Preserve Fire; D/M/N/U |
| Greater Water | `cfa3ff71a42ab8a4c8a8a4b5db3f04d4` | 165 / 199 | Preserve Cold; D/M/N/U; order audit |
| Elder Air | `333efbf776ab61c4da53e9622751d95f` | 285 / 5 | Preserve Electricity; D/M/N/U |
| Elder Earth | `65254c7a2cf18944287207e1de3e44e8` | 285 / 5 | Preserve Acid; D/M/N/U |
| Elder Fire | `e4926aa766a1cc048835237b3a97597d` | 285 / 5 | Preserve Fire; D/M/N/U |
| Elder Water | `1e54cc4ef5b261248adb30916ba49e72` | 285 / 5 | Preserve Cold; D/M/N/U |

### Comparison noise to separate before judging rules

The audit proves a selected execution child uses its own effects/ranks;
`ConvertedFrom` does not execute the menu parent's own RunAction/ContextRankConfig.
Retained parent action/rank payloads therefore are not extra child effects.
Indexed component-order differences can also inflate counts, notably Greater
Water. Review component types and active fields before interpreting a missing
indexed path. Animation differences concern presentation; parent metamagic,
descriptor/type, access and exact identity still require their own decisions.
No blanket ignore rule for parent components, unknown interfaces or child
graphs is proposed. In particular, SNA's selected-child descriptor, spawn-buff
and alignment differences remain after discarding inactive parent comparisons.

## 3. References and consumers that must retain their meaning

| Surface | Concrete preservation requirement |
| --- | --- |
| Saved known/prepared/pinned spells | Keep all original GUIDs, native definitions, AbilityData, SpellSlot, known-level mappings and usable old buttons/preparations. `ConvertedFrom` preserves original book debit; the selected child still owns execution and costs. No rewrite or migration is approved. |
| Feats and actor features | Spell Specialization matches selected child or exact Parent and adds +2 CL. AutoMetamagic can inspect source/parent descriptors and exact lists; rods and IgnoreSpellResistanceForSpells can whitelist child/Parent. Do not transfer those benefits by name. KMG Elemental Insight recognizes canonical SM/SNA ancestry and can add +2 rounds where its conditions hold; standalone roots are outside that set. Acadamae reads the actual source chain/preparation and selected action/descriptor. |
| Scrolls, wands and weapons | Preserve 24 elemental scroll Ability/learned-parent links, four Medium wands and exact item charges/Fact.SourceItem. Fireborn Warhammer `854e566488509a04994101344c3c4e2a`, Lava-Forged Axe `c93fc085584f26d49a0a0d0f5f876a0e`, Whirlwind bow `1f546ab76bb0e77478ad08248795f7d7`, Eye of the Tornado `b6c71cc97303b0f4f9f0a28e14bbb66e` remain separate grants. |
| Domains and class grants | Preserve Fire list `d8f30625d1b1f9d41a24446cbf7ac52e` Huge/Elder Fire, Travel `ab90308db82342f47bf0d636fe941434` Greater Air, Weather `eba577470b8ee8443bb4552433451990` Small parent. Review 4 confirms these raw memberships, not their casts. Preserve Flamewarden's Small/Medium/Large Fire, Enemy of the Fey, Tristian and parametrized-feature access; ordinary SM/SNA access cannot replace a domain/special slot by assumption. |
| NPCs and unique action | Preserve Nereid Huge Water/AI/metamagic routes and campaign/prebuilt facts. Wriggling Man ability `03b02f96bf14a7d4290eb2605bd2c15e` summons Infested Elder Air `96aa2542f4fd17d4f901f2379e723fd4`; it is an explicit unique exception, never an ordinary duplicate. |

The initial inventory has 29 incomplete owners. They remain unresolved, including
player/pregen graphs; neither the successful lower-seam probes nor this proposal
converts that into complete consumer coverage.

## 4. Bounded alternatives and requested decisions

| Option | Proposed behavior | Owner decision / limit |
| --- | --- | --- |
| A. Preserve exceptions | Retain standalone access and execution; clarify labels or group entries while keeping every distinct invocation reachable. | Accept residual visible entries where D/M/N/U or actor access differs. Lowest rules impact; MENU-02 hiding remains incomplete. |
| B. Canonical presentation with source-compatible invocation | Show a common elemental family presentation while dispatching the original source child and original slot/book/item context. A source-owned adapter is considered only if the actual native lineage permits it. | Approve grouping scope and how distinct slot/resource choices remain visible. Do not assign arbitrary `ConvertedFrom`, reparent shared children or assume a canonical child inherits source descriptors/buffs/resources/feat identity. If a UI cannot preserve that context, retain the exception. This is not proven feasible on every surface. |
| C. Deliberate canonical rules change | Choose SM or SNA as the new ordinary-learning route at each actual book/level; explicitly reconcile descriptors, alignment, spawn buffs/templates, unit rules and source-specific benefits. | Decide each rule axis and affected consumers first, including whether new learning changes while old invocations retain their original rules. Choose treatment of characters who know only the standalone root, specialization/metamagic facts and selection budgets. Any migration is a separate approval. No balance choice is supplied here. |

**Recommended review sequence:** retain A during review; first classify narrow
parent/order comparison noise, then audit D/M/N/U and actual actor benefits per
child/family. Prefer B for genuinely distinct saved/special invocations if its
native context can be preserved. Select C only after the owner explicitly
chooses the changed semantics and affected access, independently of the desire
to reduce menu rows. Do not choose SM for a Druid/domain route or SNA for a
Wizard route solely because the creature name matches.

After a decision, prepare a separate implementation plan and focused checks:
exact PC/console learning and budget, known/prepared/pinned invocation/debit,
source-specific feat/metamagic, item/domain/NPC exceptions, module OFF, and
authorized fresh save/reload. Repair and rerun the affected cleanup observer
before claiming a complete native phase. No hiding or comparator relaxation is
authorized by this document.
