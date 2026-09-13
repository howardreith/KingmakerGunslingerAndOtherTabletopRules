# Icon overhaul v2 state

Status: **NATIVE_FACT_PRESENTATION_QUALIFIED; COMPLETE_UI_REVIEW_IN_PROGRESS**.
Ten exact pilot images and family direction are approved. Eighty production
paintings and final native UI approval remain pending. This is a technical checkpoint.

- Branch: `codex/icon-art-overhaul-v2`. Current master
  `9dc2b6301d97bc83540845240544d34b6fad4b48` (PR #18, version `0.0.128`)
  was integrated at the owner's request by ordinary merge `75017b3e`.
  Remote master was verified unchanged on 2026-09-13. Published history and the
  old `codex/icon-art-overhaul` branch are preserved. No master merge or release.
- The selected-fact change builds on published `32be894c920a44390dd820c04be4f55a5c6877a2`.
  Its [exact qualification](../reports/icon-overhaul/NATIVE-FACT-SLOT-QUALIFICATION.json)
  records **nine guarded Steam runs, 185 assertions and 255 original native PNGs**.
  All five capture manifests passed provenance validation. No save write was observed.
- Actual Total and character-sheet Weapon Focus/Rapid Reload slots now use native
  P/M/B text, background and border/mask handling. All six real mercenary sheets
  were inspected clear at 1280x720. Exact facts, blueprint parameters, fallback
  sprites and non-firearm controls are preserved. Higher dependent roots have
  native data checks; their full rendered coverage is not inferred from Weapon Focus.
- The firearm-containing native Total list had a disabled PreferredSize fitter,
  height 862.03 and preferred height 1,461. The bounded adapter activates its existing
  vertical fitter without driving width or moving individual rows. Each committed
  sheet proves original fitter/backend restoration. All six actors passed ten
  sheet cleanup checks, plus the regression's seven final membership/inventory checks.
- Other current runs: ordinary Ifrit/Gunslinger and Ifrit/Fighter creator cases,
  dependent feats, Gunslinger OFF, Eastern Weapons OFF, native spell learning and
  final working-save smoke. Current Wakizashi and all four learning targets were
  inspected clear. WK clipping remains unreproduced at this scale; no eastern edit.
- Required gates: repository validation, **23 catalog**, **14 screenshot**, six
  paired-evidence and eleven request cases, all **1,629 domain tests**, clean Release
  and strict **224-file package** validation. Documentation curation afterward
  changes neither compiled code nor pixels.
- Exact artifact: source SHA-256
  `601a789472926949042b429a5c0b5d48c34d141e01ceebd94dcfaf9ddaf60c3d`;
  package `1d2700bb0fcf069a05857071579456366aa43a3129643e4dbed0d115a163061b`;
  DLL `1fe901a2d2fae111ba68116db319c0e625a399f29d5dd933536a4f8643c602d5`;
  MVID `2276caa3-7c42-4cb0-98a1-d683706e0540`.
  Immutable copies remain under ignored `native-fact-slot-qualified-128-*`.
- Installation: game closed normally. All 224 package files, three known additions
  and 136 backup files passed the audit. All **136 original 0.0.117 paths/hashes**
  were independently verified restored at `2026-09-13T17:40:52.842003+00:00`. Original settings are restored.
- Earlier [integration](../reports/icon-overhaul/NATIVE-128-QUALIFICATION.json)
  preserves 35,756 protected icon transitions and 255 owned graphs. Its 116 native
  captures cover inherited creator/spellbook surfaces. The separate
  [viewport checkpoint](../reports/icon-overhaul/NATIVE-VIEWPORT-QUALIFICATION.json)
  preserves six runs/86 assertions/138 captures for P/M/B, NO, WK, KA, EB and learning.
  Earlier artifacts are not mislabeled as reruns on the current DLL.
- Art candidates are complete: 89 paintings plus Rapid Reload, individual sources,
  deterministic exports, briefs/prompts and three preserved revisions. Runtime uses
  90 exports and 137 explicit painted assignments through the existing cache.
  The [production review](../reports/icon-overhaul/PRODUCTION-REVIEW.html) shows all 90;
  [pilot approval](../reports/icon-overhaul/PILOT-APPROVAL.md) binds only ten exact hashes.
  Review of the remaining 80 images has been requested while independent work continues.
- Catalog: 284 identities, 107 concepts and fifteen native firearm UI entries.
  Dispositions: 91 original-required, 47 intentional-family-share, 21 native reuse,
  56 protected, six native monograms and 63 hidden. Twelve obsolete wrappers retain
  fallback sprites. All 117 protected-file checks and assignment boundaries remain.
- Permanent policy is discoverable from AGENTS through the
  [guide](../docs/ICON-ART-GUIDE.md), [references](../docs/art/ICON-REFERENCE-INDEX.md)
  and [catalog](../assets-source/original-icons/icon-catalog.json). Intake remains
  staged in the lab; archive SHA-256
  `78a44c77861969fee778a574bdee9c3761cef323553a20951c5e9fbbb7765a8f`.
  Sixteen members/ten supplied references were verified. Raw native material stays local.
- Evidence limits: the ordinary creator doll is empty; generated test names can
  overlap the sheet header. Remote mercenary abilities are deactivated, so these
  sheets do not qualify active action bars. Existing Weapon Focus labels repeat
  the feat name; this separate naming finding is recorded without changing names.
- Remaining: full racial feat menus, racial actions/variants/visible buffs, applicable
  higher dependent feat rendering, scroll inventory/tooltip/merchant views, and
  confirmation of the strategic text-only controls. Then final owner UI review.
  The working archive has no supported P/M/B parameter GUID in its eight JSON members;
  mission section 8 requires separate authority for any new save/write fixture.
  No such write is authorized or performed. Continue independent UI work first.

Next bounded work: use actual native scroll inventory/description consumers within
the existing local spellbook fixture, with exact temporary item and UI restoration.
The strategic destination implementation uses native text buttons without assigning
spell icons; confirm that surface without adding decoration. Full racial/action
coverage still needs its own supported native fixture. See the
[native workflow](../docs/ICON-NATIVE-UI-EVIDENCE.md) and journal.

Intermediate reports are checkpoints, not completion. Continue until acceptance or
an explicit mission gate; technical PASS never substitutes for owner approval.
