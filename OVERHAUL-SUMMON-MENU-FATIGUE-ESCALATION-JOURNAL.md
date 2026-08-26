# Overhaul, summon-menu, and fatigue escalation journal

## 2026-08-26 intake and source implementation

- Fetched remote refs and selected exact `origin/master`
  `970aeb7972dcb155d5789a636ba156e68d1c0d0a`; local master and the remote ref
  were equal and the named merged commit was the selected baseline itself.
- Created isolated worktree branch
  `codex/overhaul-summon-menu-fatigue-escalation`; the primary baseline
  worktree was clean and no unrelated work was discarded.
- Inspected the relevant reports, journals, runtime procedures, source, tests,
  and installed Kingmaker 2.1.7b UI/condition IL before editing.
- Removed only Overhaul Firearm's game-time polling delivery, retaining the
  exact atomic transaction and combat predicate.
- Added a pure canvas-space menu policy, exact KMG-parent PC action-bar adapter,
  reusable overflow viewport, and supervised read-only rendered observer.
- Added exact post-native-success canonical fatigue coordination, routed
  Acadamae through it, and ordered Cord substitution once after effective
  condition resolution.
- The complete source/domain suite passes 1,278/1,278. Repository validation,
  clean exact-reference Release, output, SoundBank, deterministic rebuild, and
  strict package gates pass. Runtime preflight passes 145 checks. Package/DLL
  SHA-256 are `75BBB30989F871EFD19E937C5B14BFEB6D5BCA6AC99C61750400D8283FF93412`
  and `2A9FDEAFA5679073C783A011495479BC58D600A672B73CBBB472BEBAF328526D`.
  The immutable commit and guarded runtime evidence remain to be recorded.

## Qualification record

This section will be updated with the immutable commit, package and DLL hashes,
guarded run IDs, and any remaining human-only UI boundary after the applicable
gates complete.

## 2026-08-26 exact-reference pre-launch correction

- The first guarded overhaul request stopped before deployment or game launch.
  The runtime harness's exact-reference compiler correctly rejected the new
  direct `Canvas` use because its private compiler list did not yet include the
  installed `UnityEngine.UIModule.dll` forwarding target.
- The project already declared the UI and text-rendering forwarding modules,
  but the private-reference builder, exporter, provenance comparison, and
  environment fingerprint were inconsistent. The exporter also omitted the
  already-required `UnityEngine.AudioModule.dll` from its own source list.
- Aligned those exact installed-engine reference surfaces and added active
  repository validation for the three module names. No proprietary assembly is
  committed or redistributed; the machine-local private bundle contains exact
  hash-matched copies only.
- Re-exported a private local reference bundle and reran repository validation,
  all 1,278 domain/reflection tests, the exact-reference compiler, output,
  SoundBank, and strict package gates successfully. Runtime qualification still
  awaits a clean immutable commit.

## 2026-08-26 fatigue persistence-fixture correction

- Guarded overhaul run `20260826T0507368976150Z-39c16f004d244c00894d7dda93437633`
  passed all 12 assertions against commit `006ecf10da76f5ca4f7a73b8b6c61b490eeaf505`.
- The first guarded `disposable-fatigue-escalation` run
  `20260826T0510130072269Z-e5e82051bd594961952b95fa9c853d77`
  returned structured `ERROR` before assertions. Its qualification fixture had
  attempted to serialize a live `Buff` as a standalone root; Kingmaker's native
  persistence converters require the owning unit graph and rejected that
  unsupported shape in `DictionaryConverter.WriteJson`.
- A second attempt used installed 2.1.7b's
  `UnitSerialization.Serialize(UnitDescriptor)` preview-copy path. Guarded run
  `20260826T0517466281483Z-f2e5cc8e39ea44a68dab9332a310fd2f`
  conclusively showed that this was also not a save/load proof: the native
  `UnitStripConverter` intentionally writes `BuffCollection` as an empty object
  while `UnitSerialization.Active` is true. The production state, duration,
  immunity, same-frame, penalty, and rest assertions all passed; only the
  preview-copy assertion failed.
- Guarded Acadamae run
  `20260826T0521203278363Z-c6ec37b945914a148a36f3c7d7c8f8b6`
  passed all 20 assertions, including first failed save -> permanent Fatigued,
  second failed save -> one permanent Exhausted, stable repetition, independent
  context cleanup, native rest removal, and Cord substitution.
- Removed the invalid preview-copy claim. Added a guarded three-launch fixture
  that uses only `KMG_AUTOMATION_WORKING`: apply and exact-save, fresh-load
  verify plus exact cleanup-save, then fresh-load verify absent with no write.
  Read-only inspection found one pre-existing canonical Fatigued fact on
  `party[0]`, so the fixture preserves that fact and fails closed around the
  fixed clean `party[1]` subject.
- Dirty-source proof runs passed before commit:
  `20260826T0543070886983Z-f465ce8a7d674d9aa848916a9c7f389a`,
  `20260826T0545381889383Z-aa28b65ca90147bca116724ae5c0431c`,
  `20260826T0548077993829Z-525c4edad6594c6690300b8014102182`,
  and focused matrix
  `20260826T0550449542664Z-32f939c9389d4f38af498b90161c81d0`.
  They proved `0/0 -> 0/1`, permanent/rest-removable Exhausted, rooted
  two-context ancestry with no source spell, exact deserialization, exact
  cleanup, zero final save writes, all 11 focused assertions, and no faults.
- Repository validation, all 1,278 tests, exact-reference Release compilation,
  output validation, SoundBank validation, deterministic packaging, strict
  package validation, and the expanded 147-check runtime preflight pass. Every
  applicable guarded surface will be rerun on the final immutable DLL.
