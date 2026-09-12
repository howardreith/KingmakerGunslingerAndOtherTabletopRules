# Icon overhaul v2 state

Status: AWAITING_PILOT_APPROVAL. Plan approved; no new artwork approved.

- Branch: `codex/icon-art-overhaul-v2`; starting commit `db1fccf648b25baa53667dd00ac566e2a0d7f6b9`, version `0.0.127`.
- The original requested branch remains at `7e77a970` (historical 0.0.107); no branch was overwritten or merged. Relevant icon source is unchanged from the pack's audit commit `258decb8fc2dad58d7a096772b34ec4202d71bd5`.
- Input ZIP: `C:/Dev/KingmakerGunslingerLab/incoming/Kingmaker_Icon_Overhaul_Codex_Pack_v2.zip`; SHA-256 `78a44c77861969fee778a574bdee9c3761cef323553a20951c5e9fbbb7765a8f`.
- Unpacked input: `C:/Dev/KingmakerGunslingerLab/incoming-assets/icon-overhaul`. All 16 SHA256SUMS entries verified. Only mission/plan copied to `planning/icon-overhaul`; screenshots remain local.
- Active contract: [mission](icon-overhaul/Kingmaker_Icon_Overhaul_Codex_Mission.md), [plan](icon-overhaul/Kingmaker_Icon_Overhaul_Plan.md).
- Built-in image generation executed successfully. Individual originals are preserved under `assets-source/original-icons/icon-overhaul-v2/pilot/sources`. No API fallback, credentials, downloads or software installation.
- Census: 247 registered elemental/strategic records, including non-icon appearance records. Source inventory is not live coverage. Nereid Shake Free/assistance/aura and independent scroll icons are included.
- Native read-only IL inspection confirms explicit `FeatureUIData` null icon is retained; `CharBuildSelectorItem.SetIcon` selects TMP acronym and native background; default selected-feature constructor independently reads the parameter blueprint icon. Raw IL stays ignored under `artifacts/icon-overhaul-v2/native-contracts`.
- At intake, installed Info.json reported `0.0.117`; no game/Steam process was observed. The later temporary 0.0.127 qualification and verified restoration are recorded below.
- Publication restriction: mandated push wrapper allowlists the old branch but not this new branch. Owner notified asynchronously; do not alter/bypass policy. Continue independent local work.
- Completed: ten isolated candidates, deterministic exports/previews, canonical guide/reference index/catalog, 284 blueprint consumers and 15 explicit UI entries, 117 protected files, generator retirement, and native firearm selector prototype.
- Qualification: repository validation; 9 focused catalog tests; all 1,612 domain tests; clean Release build/package; four guarded Steam runs / 66 assertions PASS. See the exact artifact and run IDs in [curated qualification](../reports/icon-overhaul/PILOT-QUALIFICATION.json).
- Restoration: original 0.0.117 installation verified identical at every path/hash (136 files), including original settings. The runtime temporarily normalized settings to schema 11; the original schema-10 bytes were restored. See the report's pre-restoration audit caveat.
- Review: [interactive pilot](../reports/icon-overhaul/PILOT-REVIEW.html), [contact sheet](../reports/icon-overhaul/pilot-contact-sheet.png), [implementation report](../reports/icon-overhaul/IMPLEMENTATION-REPORT.md). All new pixels remain outside the installable package.
- Publication: the exact push wrapper was run and refused this non-allowlisted branch. No bypass. Await owner allowlist update.
- Next: obtain the owner's actual-image pilot decision, arrange bounded supervised native menu captures, complete live elemental/strategic consumer/assignment census, then produce approved families. No mass production before pilot approval; no final completion before separate final visual approval.

Focused prototype runtime: PASS. Native UI/typography/clipping, saved-parameter-specific round trip, complete live census, final raster integration and final visual acceptance: pending. This is not a completed overhaul or release.
