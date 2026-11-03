# Sprint 37 status — Vrock, released as v0.0.175

**RELEASED.** Stable, not a draft and not a prerelease, published from the exact
tested bytes with no release rebuild.

## The released artifact

| identity | value |
|---|---|
| tested source | `51aacea46384746197400f02d2e75fe02ae23d2f` |
| source fingerprint | `3c24a56fa5d24195003d70234b5857961da174885ee24a9f3e9e465ecc2170d3` |
| package SHA-256 | `1ef41a6c67490ed37b53a7b17ab9fd32d211e51e18ac720fa1ca27b2db592c09` |
| DLL SHA-256 | `248537327b4b1a590d81d63ac9db317d5b0877b3d415e98b922a86095320bba1` |
| DLL MVID | `25a5b6b7-b894-4832-888b-d91b95aaa9f4` |
| product merge | `68198da466354c47212df9741fbd18d37b920855` (PR #90) |
| base at merge | `b1cf3a3dd97a4df7d4838961bdea97a254a576d8` |
| tag | `v0.0.175`, object `cceff88b7de6c67ab5b4b61c5bbab8eb169e8bef`, targeting the merge |

The published asset was downloaded back and hashed: the public ZIP, DLL and
module version id are identical to the tested ones, `SHA256SUMS.txt` agrees, and
the packaged `Info.json` reads 0.0.175. The rebuild from the same source is
byte-identical in package, DLL and MVID.

## Evidence, all fresh against that one artifact

No row is reused; the publisher contract refuses a reused row for this sprint
outright. Seven guarded processes, every one of them on this package, each
bound to the bytes the game actually loaded through the engine's own
loaded-build receipt:

| row | source |
|---|---|
| `core-profile-and-self-buffs` | combined mechanics group |
| `spore-protection-and-geometry` | combined mechanics group |
| `screech-identity-and-recharge` | combined mechanics group, real time |
| `both-modes-screech` | combined mechanics group, genuine turn-based combat |
| `observed-ai-and-babau-dispel` | combined mechanics group |
| `public-routes-presentation` | combined mechanics group, all three routes |
| `mixed-on-persistence` | three fresh processes |
| `mixed-off-persistence` | three further fresh processes, module switched off |
| `highest-risk-integration` | the ON prepare process on the installed stack |
| `complete-source-build-package-gate` | the build itself, complete unfiltered suite |
| `exact-reproducibility` | the rebuild |

Six distinct process identities and six distinct run identifiers across the two
envelopes. The complete domain suite is 2796 registered, 2796 executed, zero
failures, unfiltered.

## Owner decisions, as shipped

**Spores ships with one explicit counterplay omission.** Bless destroys active
growth, a supported disease-removing effect strips it, and disease immunity
prevents exposure including the opening damage packet or halts later growth if
acquired afterwards — each observed on live units. No faithful creature-directed
holy-water application was identified in the audited installed paths, so that
one printed counter is unavailable here. That is a narrow accepted omission and
not a claim that all printed Spores counterplay is implemented. Nothing stands
in for it: no consumable, no substitute action, no repurposed enchantment.

**Automatic Stunning Screech stays conservative.** It withholds whenever a
non-exempt ally or bystander is in the radius and remains useful by attacking or
using another ability; manual use is reachable from the real action bar under
the ordinary standard-action and once-per-hour rules, including its collateral.
This is an AI policy, not an eligibility change: the effect is not enemy-only
and the party has no immunity. The AI choice, the useful fallback and manual
availability are asserted separately.

**Disclosed omissions.** Telekinesis, Dance of Ruin and greater teleport are not
represented; the printed 30-foot land speed is omitted rather than summed with
the 50-foot fly speed; the demon exemption covers this project's registered
demons only.

## Restoration

Every transaction restored the live mod tree to `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`
and the native game settings exactly. After publication: 968-entry Mods manifest
equal to the recorded intake, the owner's own installed DLL
`fd2fc61c250b13857d81acc197a896450f5b242ee392fa7192b01201e908f35f` back in
place, 118 saves, the protected Working fixture at its original
`650bc9768cf46d419e0d9e6a1793c46493d82dadf1d3498773a8496c1f975850` with its
original timestamps, `KMG_AUTOMATION_BASELINE` untouched, no game process, no
runtime lease or lock, and no staging directory left behind. The Working fixture
was restored at each envelope boundary, each restoration verified by hash.

## Failures retained, not relabelled

The qualification found defects in its own evidence machinery before it found
any in the creature, and each is recorded in the commit that fixed it rather
than edited out:

* a bounded natural-twenty retry that gated itself on a roll-dependent
  predicate, so the one case it existed for was reported as a hard failure;
* required assertion names that omitted the scenario prefix the runner emits
  and so could never have matched their own receipts;
* a fresh-execution check that accepted no shape this sprint produces, and was
  stubbed out of its own publisher suite;
* a turn-based phase that differed from the real-time one only by a suffix on
  its assertion names and never entered turn-based mode;
* four separate hand-maintained scope lists this sprint was missing from — one
  let a persistence stage report PASS while observing nothing at all, one made
  it summon a creature the scope does not own, one measured it against the
  broad fixture's module-off expectation, and two more silently skipped the
  body-visual checks every sibling sprint performs.

The scope lists are now derived from one source with a reflection test over the
declared scope constants; either of the first two bugs would fail it. Two item
and attribution assertions of mine were also wrong about the engine rather than
about the product — a natural attack is a weapon item in the unit's inventory,
and a save does not serialize the live caster reference — and were corrected to
match the released Sprint 36 shapes.

The earlier development-candidate receipts remain history. They qualified
nothing.

## Sprint 38

**NOT_STARTED.** Glabrezu is not begun, and no Sprint 38 work is present.
