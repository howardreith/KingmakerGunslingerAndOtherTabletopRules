# Z Mission — Repair Invisible Character Bodies in Kingmaker Gunslinger

## Objective

Investigate, reproduce, and repair the character-appearance regression in:

`howardreith/KingmakerGunslingerAndOtherTabletopRules`

Howie reports that new characters have invisible bodies while clothes and weapons remain visible. This occurs during both new-game character creation and mercenary recruitment, and the character remains invisible after creation.

Deliver a narrowly scoped, regression-tested repair, a qualified installable package, and a backed-up local installation for Howie's playtest. This is an implementation and verification mission, not merely a request for analysis or a plan.

Do not assume which races, classes, sexes, or installed build are affected. The report does not yet establish that the problem survives a complete game restart or identify its exact trigger.

## 1. Authority and boundaries

You may inspect this repository and its history, preserve and analyze relevant Kingmaker/UMM logs, change project-owned source and tests, build and package the mod, and conduct guarded runtime qualification with verified disposable fixtures. Repository-scoped network access for fetching source and approved non-force checkpoint publication is authorized. Use existing tools; do not install unrelated software.

You may make narrow, source-controlled additions to the existing guarded test harness when necessary to exercise the actual creation paths. Preserve all activation, identity, ownership, timeout, and cleanup safeguards. Test helpers must remain inactive in ordinary gameplay.

You may deploy diagnostic candidates through the established backup-first runtime workflow. After qualification, you may install the exact final candidate into the existing local UMM installation for Howie's playtest, provided its identity, backup, rollback, and preservation checks pass. Do not leave installation as manual homework merely because it changes the local mod folder.

This does NOT authorize merging, public releases, tags, opening a PR, changing permissions, bypassing external policy, editing unrelated mods, or modifying personal campaign saves. Return the pushed branch and review summary for subsequent review.

Read `AGENTS.md` and the current build, runtime, deployment, and save-safety documentation before acting. This mission assigns this visibility repair only; it does not activate unrelated backlog work. Where an external guard rejects an operation, do not weaken or bypass it. Complete other safe work and record the exact blocker.

## 2. Establish and preserve the real baseline

Expected lab: `C:\Dev\KingmakerGunslingerLab` on DATA/Windows 10. Verify the host, repository root, remote, branch, worktree status, installed game location, and build configuration rather than assuming them.

The earlier review examined commit `ab9eb762823fe706abb68df05c9f42d93aea8eb1`, associated with version 0.0.121. This is a historical reference, not proof of the installed or current source version. Fetch and inspect the current repository.

Before any new game launch, preserve the relevant existing logs so the original failure evidence is not overwritten. Record source SHA, installed DLL hash/MVID and version, game/UMM identity, enabled mod versions, relevant settings, and candidate/package identity. Keep machine-local artifacts private and out of Git.

Use a dedicated branch, preferably `codex/z-character-visibility-repair` if the approved publication policy supports it. Resume an existing branch only when its ancestry and mission records establish that it is this task. Preserve unrelated worktrees and changes; use an isolated worktree rather than resetting, cleaning, stashing, or checking out over someone else's work.

Choose and document a source baseline that preserves accepted work. Compare current master, the active development state, and the installed artifact. Do not install an older-baseline package over a newer installation and silently remove unrelated accepted fixes. Do not merge or cherry-pick unrelated branches without authorization. An unresolved installation-baseline conflict blocks deployment, not all source investigation.

## 3. Investigative leads — not an assumed diagnosis

Start with these project files:

```text
src/KingmakerGunslinger/ElementalRaces/Visuals/
  ElementalCharGenVisualRetentionPatch.cs
  ElementalRaceVisualResourceRegistry.cs
  ElementalRaceVisualFactory.cs
  ElementalRaceVisualBlueprintSet.cs
  ElementalVisualResourceRetentionPolicy.cs
  ElementalVisualResourceRollbackPolicy.cs

src/KingmakerGunslinger/ElementalRaces/ElementalRaceBlueprintFactory.cs
src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs
src/KingmakerGunslinger/Presentation/GunslingerClassAppearance.cs

ELEMENTAL-RACES-CHARACTER-CREATION-STABILIZATION.md
ELEMENTAL-RACES-COMPLETION.md
```

Also inspect the existing creator, visual-unload, rendering, persistence, respec, and compatibility probes. Reuse them where they exercise the relevant behavior faithfully.

The previous review identified a plausible chain:

1. A custom appearance proxy or shared native donor/inner asset becomes missing, destroyed, or replaced.
2. `RetainCharacterCreatorResources` rejects that state while checking its registered resource set.
3. The exception escapes the prefix on `CharGenDollRoom.DollStateUpdated`.
4. The native model update is interrupted, potentially including updates for an unrelated race.

Verify that this code path still exists at your baseline. The repository records earlier shared-resource destruction during creator cleanup and subsequent retention repairs. Those protections may already be present. Do not reimplement an old fix and assume the current failure is solved.

The module's OFF state must not be treated as equivalent to removing its runtime code. Determine which registrations and patches remain active for save compatibility.

Search preserved logs for these messages and their first causal stack:

```text
Owned character creator visual resource was lost:
Owned character creator inner asset was destroyed:
Native visual donor was unloaded:
Native visual dependency inner asset was destroyed:
```

Also investigate relevant missing-reference, material, mesh, skeleton, body-part visibility, appearance serialization, and native creator exceptions. Absence of these exact messages does not rule out another cause. Follow evidence rather than forcing the report to fit the hypothesis.

## 4. Reproduce and identify the first failure

First reproduce the owner's symptoms with the installed configuration, then compare narrowly isolated controls. Preserve the actual race/class/sex, creator entry point, prior creator activity, load sequence, and mod profile for every run.

Use real native new-main-character and mercenary creation paths. A directly constructed test unit is useful for isolation but cannot qualify either reported path. Exercise the transition from preview to committed playable character, not just final review followed by cancellation.

Prefer a fresh process for causal comparisons. Distinguish first creation after launch from later creation after race switching, cancellation, campaign load, or an earlier recruit.

Add request-scoped observations at the relevant lifecycle boundaries. Establish:

- The first invalid resource or first failed body assembly operation, including asset ID and provenance.
- When its state changes, the responsible call stack, and the native cleanup/update boundary involved.
- Which custom or native appearances share that resource.
- Whether the normal doll update completes and how the resulting appearance reaches the committed unit.

Inspect the installed native contracts where needed, including `CreateDolls`, `DollStateUpdated`, `UpdateDollCoroutine`, inner-asset unloading, resource-cache unloading, and transition/save-load paths. Do not commit proprietary assemblies or decompiled game sources.

Keep causal probes observational: they must not reload donors, rebuild proxies, pin extra resources, or otherwise repair the scene before measuring it. In particular, a test-only prewarm or retained object must not make the faulty build appear healthy.

Establish an unfixed-versus-fixed reproduction. Where instrumentation is necessary, run equivalent observation on both builds. A fault-injection test can prove failure isolation but cannot substitute for reproducing the naturally occurring regression. If the exact owner occurrence remains unreproduced, distinguish any independently reproduced defect and retain that uncertainty in the result.

## 5. Repair requirements

Implement the smallest production change supported by the evidence. Keep lifecycle/resource policy separate from thin Harmony/native adapters where practical, following existing project conventions. Do not add a general graphics framework or new dependency for this repair.

### Resource lifetime and recovery

Protect or reconstruct the exact resources that must survive the proven boundary. Distinguish project-owned proxies from native donors and foreign-mod objects. Cover required body/head meshes, materials, and textures—not just palette entries or non-null wrapper objects.

Handle Unity-destroyed references separately from ordinary CLR nulls. If native resources can legitimately reload as new instances, validate their identity, type, and provenance before rebinding; neither blindly deleting reference checks nor rejecting every legitimate reload is an adequate design.

Keep existing blueprint and appearance resource IDs stable. Preserve skin/head choices, customization data, equipment, race mechanics, and existing saves. Make recovery idempotent and bounded; avoid per-frame reconstruction, retained-resource growth, duplicate proxies, and recursive doll rebuilds.

### Failure isolation

An unrelated optional elemental asset must not abort ordinary native character creation. Contain project-specific failures at the integration boundary, preserve native/foreign state, and report meaningful, rate-limited diagnostics.

Simply wrapping the prefix in `try/catch` is not completion. It must not leave the selected character without valid body/head resources. An exceptional fallback must be complete, compatible, and explicitly reported; normal supported appearances must still render as intended. Do not silently redefine every elemental race as a plain Human/Aasimar to pass visibility checks.

Do not suppress native or third-party exceptions wholesale. Do not globally disable resource unloading, pin every loaded asset, mutate shared native donors, change unrelated shaders, or disable the race module as the permanent fix.

### Creation and persistence

The repair must work in the native preview and on the committed world unit. Cover cancellation, reopening, mercenary entry, scene transitions, and fresh-process reload as required by the causal findings.

Determine whether affected characters retain valid serialized appearance choices or were committed with incomplete data. Where an already-affected disposable character can be safely reconstructed, verify that recovery without changing its chosen race, appearance, equipment, stats, or progression. Do not introduce a speculative save migration or edit a personal save. Document any unrecoverable-data limitation accurately.

## 6. Regression tests and native acceptance

Add focused behavioral tests for the actual defect and recovery/isolation policy. Include repeated callbacks, resource loss or replacement, partially invalid dependency sets, exact foreign-state preservation, and bounded cleanup where applicable. Tests that merely assert source strings, object counts, or absence of exceptions do not establish the rendered outcome.

Use actual Unity/native runtime observations for destroyed assets, bundle cleanup, model assembly, and persistence. A valid body requires the expected live drawable body/head components, compatible mesh/material data, and appropriate renderer/visibility state. Compare against healthy native behavior so legitimate optional or hidden submeshes are not incorrectly classified as failures.

Mandatory coverage:

| Area | Required evidence |
| --- | --- |
| Native races | Human and Aasimar, both sexes; not broken by elemental resource failures. |
| Added races | Ifrit, Oread, Sylph, and Undine, both sexes; intended body/head appearance retained. |
| Entry points | Actual new-game creation and actual mercenary recruitment, each through commitment and world appearance. |
| Customization | Representative head, skin, hair, and body-preset changes; race changes and back-navigation. |
| Repetition | Multiple creator openings, cancellation/reopening, and repeated recruitment in one process. |
| Persistence | Committed disposable characters before and after area transition, save, full exit, fresh launch, and reload. |
| Compatibility | Focused KMG-only and owner's real installed-stack comparisons; module OFF with legacy elemental data where safely supported. |
| Delivery identity | The actual delivered DLL/package passes final focused native regressions. |

Batch cases through a small number of guarded launches where possible; do not create an unnecessary full cross-product of every option and mod combination. Include a non-Gunslinger class control to separate race rendering from Gunslinger clothing.

Observe the actual creator/world presentation with supporting captures where permitted, and provide a short human visual checklist. Rendered evidence must show the real tested surface or clearly state that it is an off-screen facsimile. Do not label a synthetic render as native UI acceptance.

Keep three outcomes separate: diagnostic execution, production behavior, and human visual acceptance. A harness that runs successfully while the character remains invisible is a failure. Human acceptance remains pending until Howie actually reports it; that alone is not a reason to withhold an otherwise qualified local candidate.

## 7. Runtime, save, and installation safety

Use the documented guarded `-kmgRuntimeTestRequest` workflow and Steam App ID `640820` for every automated game launch. Never fall back to direct `Kingmaker.exe` launch, coordinate clicking, OCR-driven operation, or an unguarded automation route.

Never select, load, overwrite, rename, delete, or modify `KMG_AUTOMATION_BASELINE`. Never touch Howie's personal campaign saves. Use `KMG_AUTOMATION_WORKING` only through its established identity-verified contract, or mission-owned disposable saves through a proven guarded lease. A name prefix alone does not establish ownership.

Before save-capable scenarios, prove save ownership, automatic-save handling, and exact cleanup/restoration. Add only narrow guarded fixture support when existing scenarios cannot exercise the real route. If a safe save boundary cannot be established, stop that scenario and continue other safe work; do not invent a substitute PASS.

A KMG-disabled comparison must be save-free or use a compatible disposable fixture. Never load and save a content-dependent campaign with required mods missing.

Do not terminate Howie's running game or another agent's test. Respect active runtime/deployment leases and recheck ownership before each launch or installation. Unexpected Steam credentials, purchases, cloud conflicts, and entitlement failures are stop conditions for runtime activity, not invitations to change saves or account state.

Before final deployment, back up and verify the existing project mod installation and settings. Preserve foreign mods and unrelated files. Install only the exact qualified candidate, verify installed hashes/MVID and loader behavior through the approved process, and record rollback steps. Restore all temporary profile/settings changes. Do not roll back another agent's newer installation using an outdated backup.

## 8. Validation, checkpoints, and continuation

Use the repository's current validation and build entry points: repository validation, complete domain tests, clean Release compilation, and installable-package validation. Do not hardcode historical test totals or reuse a different version's qualification claim.

Keep these durable mission records, reusing equivalent established files when appropriate:

```text
CHARACTER-VISIBILITY-REPAIR-MISSION.md
CHARACTER-VISIBILITY-REPAIR-STATE.md
CHARACTER-VISIBILITY-REPAIR-REPORT.md
```

Track the starting identities, reproduction, hypotheses confirmed/rejected, source changes, tests, runtime results, current deployment/backup state, blockers, and the next exact action. Preserve failed evidence rather than replacing it with later successful summaries. Commit curated findings, not raw logs, saves, packages, local paths containing private data, or proprietary files.

Commit coherent qualified checkpoints and publish through the repository-approved helper. For a permitted `codex/*` branch, the currently documented command is:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1
```

Verify the current policy before use. Do not alter its allowlist or substitute an unauthorized push route. Record publication refusal precisely and continue other work that does not require that permission.

The mission may span usage windows. Before an interruption, preserve work and exact resume/deployment state, and publish qualified checkpoints where permitted. On the next invocation, inspect the real working tree, running processes, installed artifact, and durable state before continuing. Do not assume automatic resumption after a quota reset.

Do not stop merely because you wrote a plan, found a suspicious method, passed unit tests, made a commit, or encountered one failed experiment. Change strategy when evidence warrants it. Continue until delivery or a genuine safety/access/prerequisite blocker; do not ask Howie to make routine engineering decisions.

## 9. Completion and final handoff

Successful completion requires a supported causal explanation, a scoped repair, regression coverage, final-artifact native qualification through both reported creation paths, a reviewable pushed branch, and a verified backup-first local installation.

Report any incomplete gate as FAIL, BLOCKED, or NOT RUN rather than calling the mission solved. A plausible hardening change without a reproduced behavioral improvement is not a confirmed fix for Howie's report.

The final report must identify:

1. The root cause, first failed resource/operation, and evidence connecting it to invisible bodies and any cross-race effect.
2. The exact changes and preservation of native/foreign resources and stable save identities.
3. Unfixed-versus-fixed results, creation/persistence/compatibility coverage, and any remaining uncertainty.
4. Branch and source SHA; package path, DLL/package hashes and MVID; installation status, backup location, and rollback procedure.
5. A brief human checklist for new-game creation, mercenary recruitment, world appearance, and save/restart/reload.

Do not merge, tag, publish a release, or claim Howie's visual acceptance. End with the actual delivered state and any remaining blocker.
