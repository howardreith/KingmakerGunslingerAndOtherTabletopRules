<!-- agent-review:v1 enrollment=kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 key=r02-review part=1/1 sha256=618d0193e64e11732050127e14f60536a4f59a2de778ea56d2c4ae1a7ae13603 -->
### Codex GPT-6.2 Astra — review — round 2 — candidate `7328aaa2da`

**Decision:** `changes_required` — The controller confirmed a pinned production build and a passing loader test on the candidate. The same test now breaks complete-suite builds. A matching receipt can also identify the wrong compiler output, and the loader harness can stage different bytes from those admitted at preflight.

## Continuation for Claude

Keep the correction within TRE’s build, package and loader-harness paths. Preserve the focused AssemblyRef check while restoring unfiltered test entry points. Bind the finished ZIP to the exact compiler output and embedded Git commit, cover the inputs that actually produce the package, and carry the admitted identity through staging. Give historical controls a verified release identity. Correct the archive, manifest, preflight, regression-test and documentation issues identified below.

Use disposable offline fixtures for the affected failure paths, including the existing loader safety suite. Run the named controller gate on the corrected commit and retain one clean development package for the checks that need its bytes. Preserve the historical records, runtime guards and per-loader native limitations. The four-profile static result belongs to the recorded DLL.

#### Findings

**R2-F1** · P1 · defect · blocks readiness — The gate-only loader test breaks complete-suite builds
- Location: `tests/KingmakerGunslinger.DomainTests/LoaderReferenceContractTests.cs:22`; acceptance: —
- Trigger: An unfiltered test-domain run or Build-Local -DevelopmentCompleteSuite selects every registered case.
- Expected: The focused gate checks the DLL it compiled, and the ordinary domain suite remains runnable through its existing entry points.
- Actual: Program.cs registers the case unconditionally. It throws without KMG_PRODUCTION_DLL and KMG_PRODUCTION_DLL_SHA256, which only the focused gate sets. Build-Local runs its complete suite before the exact compile. The reported development build avoided the failure by selecting an unrelated test.
- Why it blocks / does not: The new case makes normal unfiltered testing, development complete-suite builds and the release candidate pipeline fail when they reach the domain runner.
- Requested correction: Place the production-artifact assertion at a stage with a verified compiler output, or provide an appropriate bound artifact wherever the case is selected. Preserve a meaningful assertion in the focused gate and a runnable complete suite.
- Requested evidence: Show the named controller gate passing and an offline check of the ordinary unfiltered path without inherited gate variables. Retain a refusal for a mismatched bound DLL.

**R1-F3** · P1 · defect · blocks readiness — Bind the packaged DLL to the exact compiler output
- Location: `scripts/Build-Local.ps1:491`; acceptance: —
- Trigger: The shared Release output is replaced between the exact compile and package assembly.
- Expected: The receipt binds the immutable exact compiler output, its source stamp and the DLL in the finished ZIP.
- Actual: Build-Local still takes dllSha256 and dllMvid from artifacts/bin after packaging. Admission compares those values with the ZIP. Neither step compares the ZIP with the exact compiler’s modDllSha256 or reads its embedded AssemblyMetadata("GitCommit"). A different baseline-compatible DLL copied into the shared output before packaging can satisfy both comparisons.
- Why it blocks / does not: The package and receipt can agree with each other while naming the wrong producing commit.
- Requested correction: Compare the finished package measurement with the exact build output before finalizing either receipt. Verify the packaged GitCommit against the recorded producer at admission for builds that carry that stamp.
- Requested evidence: Record the compiler and packaged identities for a matching build. Show refusal when another baseline-compatible build supplies both the loose and packaged DLL while the receipt names the original producer.

**R1-F4** · P1 · defect · blocks readiness — Include actual packaging inputs in stale-candidate detection
- Location: `scripts/ReferenceProvenance.Common.ps1:302`; acceptance: —
- Trigger: A shipped file or package-producing helper changes while Info.json.Version stays the same.
- Expected: Current-candidate reuse detects changes to the production and packaging inputs that affect the supplied artifact.
- Actual: The material-path list omits scripts/package.ps1, tools/create_deterministic_package.py and shipped files such as INSTALLATION-COMPATIBILITY.md. The last file is copied directly into the ZIP and the validator checks its presence. Changing its contents leaves the older ZIP admissible. The new guard also never compares the receipt’s sourceStateSha256.
- Why it blocks / does not: Direct deployment and loader preflight can identify an earlier package as current after an input that changes its bytes has changed.
- Requested correction: Bind admission to the producing source state or to a verified fingerprint covering the actual compiler and packaging inputs. Preserve reuse of frozen evidence across documentation-only changes outside those inputs.
- Requested evidence: At a fixed mod version, show refusal after changing a shipped input or packaging helper currently omitted from the comparison. Include a matching control and a change confined to unshipped review documentation.

**R2-F2** · P1 · defect · blocks readiness — Verify the release identity behind HistoricalReleaseControl
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:89`; acceptance: —
- Trigger: A caller selects historical-control mode for a supplied ZIP.
- Expected: An explicitly selected historical control matches a trusted release identity and its applicable package contract. Development candidates retain their purpose and freshness checks.
- Actual: The loader harness’s historical branch runs only the current standalone validator. It accepts an otherwise valid same-version development ZIP without a release receipt or expected digest, yet checks an authentic older release against the current repository version. Deploy-Local also allows HistoricalReleaseControl together with DevelopmentPurpose and disables the freshness check.
- Why it blocks / does not: The exception can admit a stale development candidate as a control and prevent use of a genuine older control under its own identity.
- Requested correction: Make the admission modes mutually exclusive. Verify and record the selected historical release version and expected digest from its release record. Keep the existing deployment boundaries.
- Requested evidence: Exercise historical admission offline with an identified older release, a changed archive, a same-version development package and conflicting mode arguments.

**R2-F3** · P1 · defect · blocks readiness — Carry the admitted identity through staging
- Location: `scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1:194`; acceptance: —
- Trigger: The file at CandidateZip changes after preflight and before extraction.
- Expected: Staging and subsequent in-process identity checks use the bytes admitted against the receipt.
- Actual: Assert-LcSessionInputs discards the receipt and returns a fresh hash of the path. The session later reopens that path, and sets expectedProduct from the DLL it just staged. It never compares the staged ZIP or DLL with the admitted receipt. Deploy-Local likewise reopens the path after validation and compares the deployed DLL only after replacing live files.
- Why it blocks / does not: A replacement between admission and staging can be run as the reviewed candidate, or detected only after deployment has changed the target.
- Requested correction: Stage from a verified immutable copy or held input. Carry the admitted package and DLL identities forward and compare the extracted DLL before target replacement or launch.
- Requested evidence: Use a disposable offline fixture that changes the supplied path after preflight. Show that only the originally admitted bytes can be staged, or that the operation stops before target mutation.

**R2-F4** · P1 · defect · blocks readiness — Pass the development purpose as data to preflight
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:92`; acceptance: —
- Trigger: CandidateDevelopmentPurpose contains characters outside the supported purpose syntax.
- Expected: The development purpose reaches the existing validator as a single data argument.
- Actual: DevelopmentPurpose is interpolated into a powershell.exe -Command string before Assert-KmgDevelopmentPurpose runs. Unlike the adjacent path values, it is inserted without quoting protection. The child parses caller input as PowerShell syntax before the admission or lease guards can reject it.
- Why it blocks / does not: The new preflight lets malformed input change the command being executed outside the guarded transaction.
- Requested correction: Pass the values through parameter binding and apply the existing purpose validation before child execution.
- Requested evidence: Show the normal purpose passing and a benign invalid purpose containing an apostrophe receiving the ordinary purpose-validation error before acquisition.

**R1-F2** · P2 · defect · blocks readiness — Check the complete entry set of the archive actually extracted
- Location: `scripts/validate-package.ps1:41`; acceptance: —
- Trigger: Standalone validation receives a relative path from a changed PowerShell location or a noncanonical ZIP member name.
- Expected: Every file of the supplied archive belongs to the single approved mod tree, and the entry scan and extraction inspect the same ZIP.
- Actual: The new ZIP scan uses a textual mod-directory prefix and opens the caller’s raw path with .NET. Test-Path and Expand-Archive use the PowerShell location. With a relative path these operations can select different files. The prefix alone also does not establish the destination of entries containing parent-directory components. The post-extraction root check still counts directories only.
- Why it blocks / does not: The standalone validator can reject a valid relative path or apply its root-file check to a different archive from the one it extracts.
- Requested correction: Resolve the supplied path once and use it throughout. Check normalized archive entry destinations against the approved tree and account for files at the extraction root.
- Requested evidence: Keep the passing genuine ZIP and both ordinary extra-DLL refusals. Add an offline relative-path case after changing PowerShell location and a harmless noncanonical entry whose destination falls beside the mod directory.

**R2-F5** · P2 · defect · blocks readiness — Restore the loader harness’s missing-file safety check
- Location: `scripts/compatibility/loader-compat/Test-LoaderCompatSafety.ps1:162`; acceptance: —
- Trigger: The existing -SafetyTests entry point exercises missing-harness-refused.
- Expected: The missing-harness fixture reaches the harness-file guard and confirms that the refusal acquires nothing.
- Actual: Test-LoaderCompatSafety.ps1 writes the text 'zip' to candidate.zip and expects 'Harness file missing'. The new candidate admission runs first and throws 'Candidate admission refused' because the fixture has no build manifest. The existing safety suite therefore records a failed assertion.
- Why it blocks / does not: The repository’s offline safety check for the changed loader harness can no longer pass or demonstrate this refusal path.
- Requested correction: Make the missing-harness case reach its intended guard, either by checking the cheap harness prerequisite first or by supplying an admissible fixture candidate. Preserve independent coverage of invalid candidates.
- Requested evidence: Capture the completion output of Invoke-LoaderCompatSession.ps1 -SafetyTests with the missing-harness and invalid-candidate refusals covered offline.

**R2-F6** · P2 · defect · blocks readiness — Require the packaged loader minimum to be a string
- Location: `scripts/ReferenceProvenance.Common.ps1:260`; acceptance: —
- Trigger: The supplied manifest encodes the correct version text in a JSON array.
- Expected: The packaged ManagerVersion is the JSON string "0.31.1".
- Actual: The direct build requires ManagerVersion to be a string. The standalone package check casts the JSON value to string before comparison. A one-element array containing the floor passes that comparison.
- Why it blocks / does not: A supplied ZIP can pass admission with a manifest that violates the shipping loader contract.
- Requested correction: Apply the same string-type and value check to the packaged manifest as the direct build applies to the source manifest.
- Requested evidence: Show the standalone validator accepting the scalar string and refusing an otherwise identical package with an array-valued minimum.

**R1-F8** · P2 · defect · blocks readiness — Handle a Harmony-only mismatch in the MSBuild regression test
- Location: `scripts/Test-PinnedLoaderMsBuild.ps1:81`; acceptance: —
- Trigger: Installed UnityModManager.dll matches the pin while installed 0Harmony12.dll differs.
- Expected: The installed-loader control follows the measured contents of each reference.
- Actual: The test computes whether both installed files match, but every nonmatching set must produce a diagnostic naming UnityModManager.dll. If only 0Harmony12.dll differs, MSBuild correctly names Harmony and the regression test reports failure.
- Why it blocks / does not: The documented regression check still gives a false failure for a correctly rejected reference set.
- Requested correction: Determine the expected mismatching component from the measurements, or exercise the installed-set decision with controlled reference directories.
- Requested evidence: Show exact-match, UMM-only mismatch and Harmony-only mismatch cases producing the expected test verdicts without changing the installation.

**R1-F10** · P2 · required_gap · blocks readiness — Finish correcting the package-route and bootstrap guidance
- Location: `docs/UMM-LOADER-COMPATIBILITY.md:123`; acceptance: —
- Trigger: A developer follows the KMGPIN diagnostic or determines which build route can produce a release ZIP.
- Expected: The route description matches both publisher branches, and a pin failure gives the applicable compile-only bootstrap or mismatched-file recovery.
- Actual: The compatibility guide still says release packages come from the exact-reference compiler. Publish-Release.ps1 uses build.ps1 and package.ps1 when no reference bundle is supplied. The KMGPIN error still recommends bare build.ps1, which runs repository and domain validation before initializing the cache.
- Why it blocks / does not: The guide still misidentifies an affected package route, and the error directs developers to a command that can stop before performing the suggested recovery.
- Requested correction: Describe the MSBuild and exact-reference packaging routes explicitly. Align the diagnostic in Assert-PinnedLoaderReferences.ps1 with the corrected BUILDING.md instructions for absent and mismatched files.
- Requested evidence: Check the wording against both branches of Publish-Release.ps1 and capture the actionable diagnostic from a disposable pin failure.

**R2-F7** · P3 · optional_polish · non-blocking — Close the fixture ZIP before cleanup
- Location: `scripts/Test-CandidateAdmission.ps1:78`; acceptance: —
- Trigger: The Windows admission test finishes while the entry lookup still holds a file handle.
- Expected: The temporary clone and package can be removed deterministically when the admission test finishes.
- Actual: The entry lookup opens a ZipArchive without disposing it. Windows can retain the file handle until collection, while final cleanup suppresses removal errors.
- Why it blocks / does not: This affects temporary workspace cleanup. The recorded admission assertions remain usable.
- Requested correction: Dispose the archive after reading the entry name and surface a failure to remove the owned temporary directory.
- Requested evidence: Confirm the temporary clone is gone after the offline test exits.

**R2-F8** · P3 · optional_polish · non-blocking — Read pin versions independently of JSON layout
- Location: `tests/KingmakerGunslinger.DomainTests/LoaderReferenceContractTests.cs:43`; acceptance: —
- Trigger: A tool reserializes the pins without changing their values.
- Expected: The test reads the version belonging to each reference independently of JSON formatting and property order.
- Actual: PinVersion uses a regular expression requiring relativePath to precede assemblyVersion with a particular colon layout. The production pin reader accepts equivalent JSON with reordered properties.
- Why it blocks / does not: The checked-in pin file has the expected layout. This is a maintenance issue for later pin-file edits.
- Requested correction: Read the pin rows structurally or pass the verified identities from the existing pin reader.
- Requested evidence: A semantically identical pin document with reordered properties yields the same expected AssemblyRefs.

#### Prior finding assessments

- **R1-F1** — `fix_verified` — The direct build checks the source floor, and standalone validation reads the packaged assembly’s full loader references. The recorded controls include a passing baseline package, refusal of the v0.0.187 DLL referencing UMM 0.33.0.0 and refusal of a mismatched source floor. R2-F6 covers a separate packaged JSON type defect.
- **R1-F2** — `fix_rejected` — Both ordinary extra-DLL controls passed. The raw path and prefix checks still leave gaps in establishing that every extracted file belongs to the approved tree of the supplied archive.
- **R1-F3** — `fix_rejected` — Admission now rejects a changed receipt hash or MVID. The producer still takes both from a shared output, and admission ignores the available compiler hash and embedded source commit.
- **R1-F4** — `fix_rejected` — Project and reference-helper changes are covered by the recorded refusals. The new path list omits actual package inputs, and direct admission does not verify the recorded source-state fingerprint.
- **R1-F5** — `fix_verified` — The clean development receipt identifies producer 04b9384ed1, both pinned references and ZIP 7275c768cfd9755ff7d8ba210549b1eb80acdabf64fec4e7f62dc32e2d4f6171. Standalone measurement agrees on the packaged DLL and MVID. Subsequent commits contain documentation and evidence only.
- **R1-F6** — `fix_verified` — The audit identifies the frozen DLL and records resolved signatures and component hashes for 0.31.1, 0.32.4, measured owner 0.33.0 and 0.33.4. The profile record distinguishes the owner copy from assembled profiles and dates the author’s stable release selection.
- **R1-F7** — `fix_verified` — Each native row has an explicit NOT RUN disposition. The record names the competing enrollment using the disposable copy for three rows and the missing reviewed harness profile for 0.33.4. It also states that no combined-stack check ran.
- **R1-F8** — `fix_rejected` — The historical control is pinned and both checkouts are verified. The remaining installed-set assertion still expects the UMM filename when Harmony alone differs.
- **R1-F9** — `fix_verified` — The controller ran a relevant nonzero AssemblyRef test on H and recorded its completion. The four implementation-text assertions were removed. R2-F1 covers the separate regression caused by registering the gate-dependent case in the complete suite.
- **R1-F10** — `fix_rejected` — BUILDING.md and the scripts guide now give a compile-only bootstrap and development-package command. The compatibility guide retains the blanket release-route statement, and the emitted recovery diagnostic still recommends bare build.ps1.

#### Evidence assessment

- **Inspected:** The enrolled mission, repository instructions and validation policy, prior findings and dispositions, full feature diff, developer report, transcript notice and controller receipt.; Production project targets, exact compiler, Build-Local and publisher routes, package validator, receipt readers, deployment and loader-session staging, domain runner and affected offline fixtures.; The development receipt, standalone package measurement, audit source and profiles, admission and MSBuild results, release records and current build documentation.
- **Executed verification:** The controller receipt for 7328aaa2da records production compilation against UnityModManager 0.31.1.0 and 0Harmony12 1.2.0.1. The named loader test selected 1 of 3189 and completed with zero failures in a 3.9-second gate.; I verified HEAD and the mission digest. Git shows only documentation and evidence changes after package producer 04b9384ed1. The development receipt, standalone package measurement and API audit agree on DLL SHA-256 4cbc662565de3c5608c6cb18dfc075b13fd6f574d98af0058ced2e02395ea446 and MVID bd268aa9-7524-4559-b580-9918f1fded45.; The committed outputs record 13/13 admission cases on a separate temporary-clone package, 9/9 MSBuild cases at 45b9b77097, 35 pin checks and 13 development-gate fixture checks. The audit records 23 types and 67 members resolved on each of four profiles.; Read-only PowerShell checks confirmed that changing location can leave the process working directory elsewhere, and that casting a one-element JSON array to string makes it compare equal to the loader floor.
- **Source inspection only:** The complete-suite failure, missing-harness fixture regression, receipt provenance gap, omitted material inputs, historical-control behavior and preflight-to-staging identity gap were traced through their callers.; Archive containment, child-command construction, the mixed-loader test expectation and publisher branch selection were assessed from source. No package or installation was modified to exercise them.
- **Required gaps:** Working complete-suite and loader-safety entry points alongside the passing focused gate.; An exact-compiler-to-ZIP identity check, complete material-input comparison, verified historical controls and preservation of the admitted bytes through staging.; Consistent archive and manifest admission, safe preflight argument handling, the Harmony-only mismatch control and accurate build-route diagnostics.
- **Limitations:** The snapshot was read-only. Production builds, package extraction, native sessions and external distribution lookups were not rerun. The foreground tool log was withheld. Results outside the controller gate are attributed to the committed outputs and their named producers.; Native coverage for the reviewed DLL is NOT RUN on all four loaders. The developer recorded a competing enrollment using the disposable game copy and no reviewed 0.33.4 harness profile. No combined-stack run was recorded.

#### Summary

- **Delivered behavior:** TRE declares UMM 0.31.1, verifies the selected loader files and resolved MSBuild inputs, and inspects the actual packaged DLL’s loader references. A frozen development package has matching receipt and static-audit measurements across four loader profiles. The remaining defects affect build entry points and candidate admission.
- **Scope reviewed:** Final round-2 review of TRE draft PR #220 at 7328aaa2da6855e4c4554f3828f699d252c0c3da against this repository’s obligations in the enrolled UMM mission.
- **Pending later-phase work:** none
- **Unresolved questions:** none
- **Remaining owner review:** After correction and independent re-review, the owner decides PR disposition and any release or native qualification.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Owner guidance bound to this review: none.

Owner amendments bound to this review: none.

Codex CLI `0.170.0`, thread `01a20c62-64e3-7f71-a76c-22263eb70066`, duration 846.4s, response sha256 `f2b12833daaa4e1112d65c5730e2b827259a3353f043cb1aa4526cdad318696e`, request `kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r02-ae7b57`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "7328aaa2da6855e4c4554f3828f699d252c0c3da",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "The controller confirmed a pinned production build and a passing loader test on the candidate. The same test now breaks complete-suite builds. A matching receipt can also identify the wrong compiler output, and the loader harness can stage different bytes from those admitted at preflight.\n\n## Continuation for Claude\n\nKeep the correction within TRE\u2019s build, package and loader-harness paths. Preserve the focused AssemblyRef check while restoring unfiltered test entry points. Bind the finished ZIP to the exact compiler output and embedded Git commit, cover the inputs that actually produce the package, and carry the admitted identity through staging. Give historical controls a verified release identity. Correct the archive, manifest, preflight, regression-test and documentation issues identified below.\n\nUse disposable offline fixtures for the affected failure paths, including the existing loader safety suite. Run the named controller gate on the corrected commit and retain one clean development package for the checks that need its bytes. Preserve the historical records, runtime guards and per-loader native limitations. The four-profile static result belongs to the recorded DLL.",
  "evidence_assessment": {
    "executed_verification": [
      "The controller receipt for 7328aaa2da records production compilation against UnityModManager 0.31.1.0 and 0Harmony12 1.2.0.1. The named loader test selected 1 of 3189 and completed with zero failures in a 3.9-second gate.",
      "I verified HEAD and the mission digest. Git shows only documentation and evidence changes after package producer 04b9384ed1. The development receipt, standalone package measurement and API audit agree on DLL SHA-256 4cbc662565de3c5608c6cb18dfc075b13fd6f574d98af0058ced2e02395ea446 and MVID bd268aa9-7524-4559-b580-9918f1fded45.",
      "The committed outputs record 13/13 admission cases on a separate temporary-clone package, 9/9 MSBuild cases at 45b9b77097, 35 pin checks and 13 development-gate fixture checks. The audit records 23 types and 67 members resolved on each of four profiles.",
      "Read-only PowerShell checks confirmed that changing location can leave the process working directory elsewhere, and that casting a one-element JSON array to string makes it compare equal to the loader floor."
    ],
    "inspected": [
      "The enrolled mission, repository instructions and validation policy, prior findings and dispositions, full feature diff, developer report, transcript notice and controller receipt.",
      "Production project targets, exact compiler, Build-Local and publisher routes, package validator, receipt readers, deployment and loader-session staging, domain runner and affected offline fixtures.",
      "The development receipt, standalone package measurement, audit source and profiles, admission and MSBuild results, release records and current build documentation."
    ],
    "limitations": [
      "The snapshot was read-only. Production builds, package extraction, native sessions and external distribution lookups were not rerun. The foreground tool log was withheld. Results outside the controller gate are attributed to the committed outputs and their named producers.",
      "Native coverage for the reviewed DLL is NOT RUN on all four loaders. The developer recorded a competing enrollment using the disposable game copy and no reviewed 0.33.4 harness profile. No combined-stack run was recorded."
    ],
    "required_gaps": [
      "Working complete-suite and loader-safety entry points alongside the passing focused gate.",
      "An exact-compiler-to-ZIP identity check, complete material-input comparison, verified historical controls and preservation of the admitted bytes through staging.",
      "Consistent archive and manifest admission, safe preflight argument handling, the Harmony-only mismatch control and accurate build-route diagnostics."
    ],
    "source_inspection_only": [
      "The complete-suite failure, missing-harness fixture regression, receipt provenance gap, omitted material inputs, historical-control behavior and preflight-to-staging identity gap were traced through their callers.",
      "Archive containment, child-command construction, the mixed-loader test expectation and publisher branch selection were assessed from source. No package or installation was modified to exercise them."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [],
      "actual": "Program.cs registers the case unconditionally. It throws without KMG_PRODUCTION_DLL and KMG_PRODUCTION_DLL_SHA256, which only the focused gate sets. Build-Local runs its complete suite before the exact compile. The reported development build avoided the failure by selecting an unrelated test.",
      "blocking_rationale": "The new case makes normal unfiltered testing, development complete-suite builds and the release candidate pipeline fail when they reach the domain runner.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The focused gate checks the DLL it compiled, and the ordinary domain suite remains runnable through its existing entry points.",
      "id": "R2-F1",
      "location": {
        "file": "tests/KingmakerGunslinger.DomainTests/LoaderReferenceContractTests.cs",
        "line": 22
      },
      "priority": "P1",
      "requested_correction": "Place the production-artifact assertion at a stage with a verified compiler output, or provide an appropriate bound artifact wherever the case is selected. Preserve a meaningful assertion in the focused gate and a runnable complete suite.",
      "requested_evidence": "Show the named controller gate passing and an offline check of the ordinary unfiltered path without inherited gate variables. Retain a refusal for a mismatched bound DLL.",
      "title": "The gate-only loader test breaks complete-suite builds",
      "trigger": "An unfiltered test-domain run or Build-Local -DevelopmentCompleteSuite selects every registered case."
    },
    {
      "acceptance_ids": [],
      "actual": "Build-Local still takes dllSha256 and dllMvid from artifacts/bin after packaging. Admission compares those values with the ZIP. Neither step compares the ZIP with the exact compiler\u2019s modDllSha256 or reads its embedded AssemblyMetadata(\"GitCommit\"). A different baseline-compatible DLL copied into the shared output before packaging can satisfy both comparisons.",
      "blocking_rationale": "The package and receipt can agree with each other while naming the wrong producing commit.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The receipt binds the immutable exact compiler output, its source stamp and the DLL in the finished ZIP.",
      "id": "R1-F3",
      "location": {
        "file": "scripts/Build-Local.ps1",
        "line": 491
      },
      "priority": "P1",
      "requested_correction": "Compare the finished package measurement with the exact build output before finalizing either receipt. Verify the packaged GitCommit against the recorded producer at admission for builds that carry that stamp.",
      "requested_evidence": "Record the compiler and packaged identities for a matching build. Show refusal when another baseline-compatible build supplies both the loose and packaged DLL while the receipt names the original producer.",
      "title": "Bind the packaged DLL to the exact compiler output",
      "trigger": "The shared Release output is replaced between the exact compile and package assembly."
    },
    {
      "acceptance_ids": [],
      "actual": "The material-path list omits scripts/package.ps1, tools/create_deterministic_package.py and shipped files such as INSTALLATION-COMPATIBILITY.md. The last file is copied directly into the ZIP and the validator checks its presence. Changing its contents leaves the older ZIP admissible. The new guard also never compares the receipt\u2019s sourceStateSha256.",
      "blocking_rationale": "Direct deployment and loader preflight can identify an earlier package as current after an input that changes its bytes has changed.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Current-candidate reuse detects changes to the production and packaging inputs that affect the supplied artifact.",
      "id": "R1-F4",
      "location": {
        "file": "scripts/ReferenceProvenance.Common.ps1",
        "line": 302
      },
      "priority": "P1",
      "requested_correction": "Bind admission to the producing source state or to a verified fingerprint covering the actual compiler and packaging inputs. Preserve reuse of frozen evidence across documentation-only changes outside those inputs.",
      "requested_evidence": "At a fixed mod version, show refusal after changing a shipped input or packaging helper currently omitted from the comparison. Include a matching control and a change confined to unshipped review documentation.",
      "title": "Include actual packaging inputs in stale-candidate detection",
      "trigger": "A shipped file or package-producing helper changes while Info.json.Version stays the same."
    },
    {
      "acceptance_ids": [],
      "actual": "The loader harness\u2019s historical branch runs only the current standalone validator. It accepts an otherwise valid same-version development ZIP without a release receipt or expected digest, yet checks an authentic older release against the current repository version. Deploy-Local also allows HistoricalReleaseControl together with DevelopmentPurpose and disables the freshness check.",
      "blocking_rationale": "The exception can admit a stale development candidate as a control and prevent use of a genuine older control under its own identity.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "An explicitly selected historical control matches a trusted release identity and its applicable package contract. Development candidates retain their purpose and freshness checks.",
      "id": "R2-F2",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 89
      },
      "priority": "P1",
      "requested_correction": "Make the admission modes mutually exclusive. Verify and record the selected historical release version and expected digest from its release record. Keep the existing deployment boundaries.",
      "requested_evidence": "Exercise historical admission offline with an identified older release, a changed archive, a same-version development package and conflicting mode arguments.",
      "title": "Verify the release identity behind HistoricalReleaseControl",
      "trigger": "A caller selects historical-control mode for a supplied ZIP."
    },
    {
      "acceptance_ids": [],
      "actual": "Assert-LcSessionInputs discards the receipt and returns a fresh hash of the path. The session later reopens that path, and sets expectedProduct from the DLL it just staged. It never compares the staged ZIP or DLL with the admitted receipt. Deploy-Local likewise reopens the path after validation and compares the deployed DLL only after replacing live files.",
      "blocking_rationale": "A replacement between admission and staging can be run as the reviewed candidate, or detected only after deployment has changed the target.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Staging and subsequent in-process identity checks use the bytes admitted against the receipt.",
      "id": "R2-F3",
      "location": {
        "file": "scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1",
        "line": 194
      },
      "priority": "P1",
      "requested_correction": "Stage from a verified immutable copy or held input. Carry the admitted package and DLL identities forward and compare the extracted DLL before target replacement or launch.",
      "requested_evidence": "Use a disposable offline fixture that changes the supplied path after preflight. Show that only the originally admitted bytes can be staged, or that the operation stops before target mutation.",
      "title": "Carry the admitted identity through staging",
      "trigger": "The file at CandidateZip changes after preflight and before extraction."
    },
    {
      "acceptance_ids": [],
      "actual": "DevelopmentPurpose is interpolated into a powershell.exe -Command string before Assert-KmgDevelopmentPurpose runs. Unlike the adjacent path values, it is inserted without quoting protection. The child parses caller input as PowerShell syntax before the admission or lease guards can reject it.",
      "blocking_rationale": "The new preflight lets malformed input change the command being executed outside the guarded transaction.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The development purpose reaches the existing validator as a single data argument.",
      "id": "R2-F4",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 92
      },
      "priority": "P1",
      "requested_correction": "Pass the values through parameter binding and apply the existing purpose validation before child execution.",
      "requested_evidence": "Show the normal purpose passing and a benign invalid purpose containing an apostrophe receiving the ordinary purpose-validation error before acquisition.",
      "title": "Pass the development purpose as data to preflight",
      "trigger": "CandidateDevelopmentPurpose contains characters outside the supported purpose syntax."
    },
    {
      "acceptance_ids": [],
      "actual": "The new ZIP scan uses a textual mod-directory prefix and opens the caller\u2019s raw path with .NET. Test-Path and Expand-Archive use the PowerShell location. With a relative path these operations can select different files. The prefix alone also does not establish the destination of entries containing parent-directory components. The post-extraction root check still counts directories only.",
      "blocking_rationale": "The standalone validator can reject a valid relative path or apply its root-file check to a different archive from the one it extracts.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every file of the supplied archive belongs to the single approved mod tree, and the entry scan and extraction inspect the same ZIP.",
      "id": "R1-F2",
      "location": {
        "file": "scripts/validate-package.ps1",
        "line": 41
      },
      "priority": "P2",
      "requested_correction": "Resolve the supplied path once and use it throughout. Check normalized archive entry destinations against the approved tree and account for files at the extraction root.",
      "requested_evidence": "Keep the passing genuine ZIP and both ordinary extra-DLL refusals. Add an offline relative-path case after changing PowerShell location and a harmless noncanonical entry whose destination falls beside the mod directory.",
      "title": "Check the complete entry set of the archive actually extracted",
      "trigger": "Standalone validation receives a relative path from a changed PowerShell location or a noncanonical ZIP member name."
    },
    {
      "acceptance_ids": [],
      "actual": "Test-LoaderCompatSafety.ps1 writes the text 'zip' to candidate.zip and expects 'Harness file missing'. The new candidate admission runs first and throws 'Candidate admission refused' because the fixture has no build manifest. The existing safety suite therefore records a failed assertion.",
      "blocking_rationale": "The repository\u2019s offline safety check for the changed loader harness can no longer pass or demonstrate this refusal path.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The missing-harness fixture reaches the harness-file guard and confirms that the refusal acquires nothing.",
      "id": "R2-F5",
      "location": {
        "file": "scripts/compatibility/loader-compat/Test-LoaderCompatSafety.ps1",
        "line": 162
      },
      "priority": "P2",
      "requested_correction": "Make the missing-harness case reach its intended guard, either by checking the cheap harness prerequisite first or by supplying an admissible fixture candidate. Preserve independent coverage of invalid candidates.",
      "requested_evidence": "Capture the completion output of Invoke-LoaderCompatSession.ps1 -SafetyTests with the missing-harness and invalid-candidate refusals covered offline.",
      "title": "Restore the loader harness\u2019s missing-file safety check",
      "trigger": "The existing -SafetyTests entry point exercises missing-harness-refused."
    },
    {
      "acceptance_ids": [],
      "actual": "The direct build requires ManagerVersion to be a string. The standalone package check casts the JSON value to string before comparison. A one-element array containing the floor passes that comparison.",
      "blocking_rationale": "A supplied ZIP can pass admission with a manifest that violates the shipping loader contract.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The packaged ManagerVersion is the JSON string \"0.31.1\".",
      "id": "R2-F6",
      "location": {
        "file": "scripts/ReferenceProvenance.Common.ps1",
        "line": 260
      },
      "priority": "P2",
      "requested_correction": "Apply the same string-type and value check to the packaged manifest as the direct build applies to the source manifest.",
      "requested_evidence": "Show the standalone validator accepting the scalar string and refusing an otherwise identical package with an array-valued minimum.",
      "title": "Require the packaged loader minimum to be a string",
      "trigger": "The supplied manifest encodes the correct version text in a JSON array."
    },
    {
      "acceptance_ids": [],
      "actual": "The test computes whether both installed files match, but every nonmatching set must produce a diagnostic naming UnityModManager.dll. If only 0Harmony12.dll differs, MSBuild correctly names Harmony and the regression test reports failure.",
      "blocking_rationale": "The documented regression check still gives a false failure for a correctly rejected reference set.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The installed-loader control follows the measured contents of each reference.",
      "id": "R1-F8",
      "location": {
        "file": "scripts/Test-PinnedLoaderMsBuild.ps1",
        "line": 81
      },
      "priority": "P2",
      "requested_correction": "Determine the expected mismatching component from the measurements, or exercise the installed-set decision with controlled reference directories.",
      "requested_evidence": "Show exact-match, UMM-only mismatch and Harmony-only mismatch cases producing the expected test verdicts without changing the installation.",
      "title": "Handle a Harmony-only mismatch in the MSBuild regression test",
      "trigger": "Installed UnityModManager.dll matches the pin while installed 0Harmony12.dll differs."
    },
    {
      "acceptance_ids": [],
      "actual": "The compatibility guide still says release packages come from the exact-reference compiler. Publish-Release.ps1 uses build.ps1 and package.ps1 when no reference bundle is supplied. The KMGPIN error still recommends bare build.ps1, which runs repository and domain validation before initializing the cache.",
      "blocking_rationale": "The guide still misidentifies an affected package route, and the error directs developers to a command that can stop before performing the suggested recovery.",
      "blocks_readiness": true,
      "category": "required_gap",
      "expected": "The route description matches both publisher branches, and a pin failure gives the applicable compile-only bootstrap or mismatched-file recovery.",
      "id": "R1-F10",
      "location": {
        "file": "docs/UMM-LOADER-COMPATIBILITY.md",
        "line": 123
      },
      "priority": "P2",
      "requested_correction": "Describe the MSBuild and exact-reference packaging routes explicitly. Align the diagnostic in Assert-PinnedLoaderReferences.ps1 with the corrected BUILDING.md instructions for absent and mismatched files.",
      "requested_evidence": "Check the wording against both branches of Publish-Release.ps1 and capture the actionable diagnostic from a disposable pin failure.",
      "title": "Finish correcting the package-route and bootstrap guidance",
      "trigger": "A developer follows the KMGPIN diagnostic or determines which build route can produce a release ZIP."
    },
    {
      "acceptance_ids": [],
      "actual": "The entry lookup opens a ZipArchive without disposing it. Windows can retain the file handle until collection, while final cleanup suppresses removal errors.",
      "blocking_rationale": "This affects temporary workspace cleanup. The recorded admission assertions remain usable.",
      "blocks_readiness": false,
      "category": "optional_polish",
      "expected": "The temporary clone and package can be removed deterministically when the admission test finishes.",
      "id": "R2-F7",
      "location": {
        "file": "scripts/Test-CandidateAdmission.ps1",
        "line": 78
      },
      "priority": "P3",
      "requested_correction": "Dispose the archive after reading the entry name and surface a failure to remove the owned temporary directory.",
      "requested_evidence": "Confirm the temporary clone is gone after the offline test exits.",
      "title": "Close the fixture ZIP before cleanup",
      "trigger": "The Windows admission test finishes while the entry lookup still holds a file handle."
    },
    {
      "acceptance_ids": [],
      "actual": "PinVersion uses a regular expression requiring relativePath to precede assemblyVersion with a particular colon layout. The production pin reader accepts equivalent JSON with reordered properties.",
      "blocking_rationale": "The checked-in pin file has the expected layout. This is a maintenance issue for later pin-file edits.",
      "blocks_readiness": false,
      "category": "optional_polish",
      "expected": "The test reads the version belonging to each reference independently of JSON formatting and property order.",
      "id": "R2-F8",
      "location": {
        "file": "tests/KingmakerGunslinger.DomainTests/LoaderReferenceContractTests.cs",
        "line": 43
      },
      "priority": "P3",
      "requested_correction": "Read the pin rows structurally or pass the verified identities from the existing pin reader.",
      "requested_evidence": "A semantically identical pin document with reordered properties yields the same expected AssemblyRefs.",
      "title": "Read pin versions independently of JSON layout",
      "trigger": "A tool reserializes the pins without changing their values."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F1",
      "rationale": "The direct build checks the source floor, and standalone validation reads the packaged assembly\u2019s full loader references. The recorded controls include a passing baseline package, refusal of the v0.0.187 DLL referencing UMM 0.33.0.0 and refusal of a mismatched source floor. R2-F6 covers a separate packaged JSON type defect."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F2",
      "rationale": "Both ordinary extra-DLL controls passed. The raw path and prefix checks still leave gaps in establishing that every extracted file belongs to the approved tree of the supplied archive."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F3",
      "rationale": "Admission now rejects a changed receipt hash or MVID. The producer still takes both from a shared output, and admission ignores the available compiler hash and embedded source commit."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F4",
      "rationale": "Project and reference-helper changes are covered by the recorded refusals. The new path list omits actual package inputs, and direct admission does not verify the recorded source-state fingerprint."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F5",
      "rationale": "The clean development receipt identifies producer 04b9384ed1, both pinned references and ZIP 7275c768cfd9755ff7d8ba210549b1eb80acdabf64fec4e7f62dc32e2d4f6171. Standalone measurement agrees on the packaged DLL and MVID. Subsequent commits contain documentation and evidence only."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F6",
      "rationale": "The audit identifies the frozen DLL and records resolved signatures and component hashes for 0.31.1, 0.32.4, measured owner 0.33.0 and 0.33.4. The profile record distinguishes the owner copy from assembled profiles and dates the author\u2019s stable release selection."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F7",
      "rationale": "Each native row has an explicit NOT RUN disposition. The record names the competing enrollment using the disposable copy for three rows and the missing reviewed harness profile for 0.33.4. It also states that no combined-stack check ran."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F8",
      "rationale": "The historical control is pinned and both checkouts are verified. The remaining installed-set assertion still expects the UMM filename when Harmony alone differs."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F9",
      "rationale": "The controller ran a relevant nonzero AssemblyRef test on H and recorded its completion. The four implementation-text assertions were removed. R2-F1 covers the separate regression caused by registering the gate-dependent case in the complete suite."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F10",
      "rationale": "BUILDING.md and the scripts guide now give a compile-only bootstrap and development-package command. The compatibility guide retains the blanket release-route statement, and the emitted recovery diagnostic still recommends bare build.ps1."
    }
  ],
  "request_id": "kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r02-ae7b57",
  "round": 2,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "TRE declares UMM 0.31.1, verifies the selected loader files and resolved MSBuild inputs, and inspects the actual packaged DLL\u2019s loader references. A frozen development package has matching receipt and static-audit measurements across four loader profiles. The remaining defects affect build entry points and candidate admission.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "After correction and independent re-review, the owner decides PR disposition and any release or native qualification."
    ],
    "scope_reviewed": "Final round-2 review of TRE draft PR #220 at 7328aaa2da6855e4c4554f3828f699d252c0c3da against this repository\u2019s obligations in the enrolled UMM mission.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
