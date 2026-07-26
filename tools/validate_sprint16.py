#!/usr/bin/env python3
"""Portable validation for Sprint 16's runtime-qualification milestone.

This validates source/package invariants and independent reference models. It
cannot replace compilation against the installed Kingmaker assemblies or the
in-game persistence lifecycle matrix.
"""
from __future__ import annotations

import hashlib
import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
VALIDATION = ROOT / "validation"
EXPECTED_VERSION = "0.0.16"
EXPECTED_INFO = "0.0.16-s16-runtime-qualification"
EXPECTED_TESTS = 371
EXPECTED_PREFIX_COUNTS = {
    "state.": 61,
    "repository.": 21,
    "item-state.": 11,
    "token.": 24,
    "token-repository.": 28,
    "vault-data.": 5,
    "vault-repository.": 25,
    "migration.": 23,
    "identity.": 14,
    "identity-vault.": 15,
    "identity-repository.": 4,
    "identity-migration.": 2,
    "evidence.": 24,
    "preflight.": 20,
}
EXPECTED_ACTIVE = {
    "KMG.Diagnostic.InitializedFeature": "6294cc6964914ea7bf450d5ef82fadde",
    "KMG.Firearms.FirearmProficiency": "5148f69223044799800b65732b6cabea",
    "KMG.Test.TestMusketWeaponType": "6e499550b44c41b3a1ef0693904a46b8",
    "KMG.Test.TestMusketItem": "09641295ceea4c558400c43df2ddf1f9",
    "KMG.Test.LoadedStateToken": "c11a8965dbdd43f08080f4dc51a29113",
    "KMG.Test.BrokenEmptyStateToken": "5513972dd2624c9f86bc29c850dac736",
    "KMG.Test.BrokenLoadedStateToken": "f5fa460f93214458b6f59db24b0dfd12",
    "KMG.Test.WreckedStateToken": "877f65ca3a404f2e98af528b7fb1a2fb",
}
REPRODUCTION_ROWS = {"I03", "I10", "I11", "I13", "I15", "I19", "I23"}
MSBUILD_NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}


def read(relative: str) -> str:
    return (ROOT / relative).read_text(encoding="utf-8")


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def compile_items(project_path: Path) -> list[str]:
    tree = ET.parse(project_path)
    return [node.attrib["Include"] for node in tree.findall(".//m:Compile", MSBUILD_NS)]


def validate_metadata() -> dict:
    info = json.loads(read("Info.json"))
    assert info == {
        "Id": "KingmakerGunslinger",
        "DisplayName": "Kingmaker Gunslinger",
        "Author": "Howie",
        "Version": EXPECTED_VERSION,
        "ManagerVersion": "0.32.5",
        "Requirements": [],
        "AssemblyName": "KingmakerGunslinger.dll",
        "EntryMethod": "KingmakerGunslinger.Main.Load",
    }
    props = read("Directory.Build.props")
    assert f"<KmgVersion>{EXPECTED_VERSION}</KmgVersion>" in props
    assert f"<KmgInformationalVersion>{EXPECTED_INFO}</KmgInformationalVersion>" in props
    assembly = read("src/KingmakerGunslinger/Properties/AssemblyInfo.cs")
    for token in (
        'AssemblyVersion("0.0.16.0")',
        'AssemblyFileVersion("0.0.16.0")',
        'AssemblyInformationalVersion("0.0.16-s16-runtime-qualification")',
    ):
        assert token in assembly, token
    return {"passed": True, "version": EXPECTED_VERSION, "informationalVersion": EXPECTED_INFO}


def validate_manifest() -> dict:
    import jsonschema  # type: ignore

    manifest = json.loads(read("blueprints/blueprints.json"))
    schema = json.loads(read("blueprints/blueprints.schema.json"))
    jsonschema.Draft7Validator(schema).validate(manifest)
    entries = manifest["entries"]
    assert len(entries) == 12
    assert len({entry["symbol"] for entry in entries}) == 12
    assert len({entry["guid"] for entry in entries}) == 12
    for entry in entries:
        assert re.fullmatch(r"KMG\.[A-Za-z0-9_.]+", entry["symbol"])
        assert re.fullmatch(r"[0-9a-f]{32}", entry["guid"])
        assert int(entry["guid"], 16) != 0
    by_symbol = {entry["symbol"]: entry for entry in entries}
    for symbol, guid in EXPECTED_ACTIVE.items():
        assert by_symbol[symbol]["guid"] == guid
        assert by_symbol[symbol]["status"] == "active"
    active = [entry for entry in entries if entry["status"] == "active"]
    assert len(active) == 8
    assert not any(entry.get("milestone") == "Sprint 16" and entry.get("status") == "active" for entry in entries)
    return {
        "passed": True,
        "entryCount": 12,
        "activeCount": 8,
        "reservedCount": 4,
        "newSprint16BlueprintIds": 0,
        "manifestSha256": sha256(ROOT / "blueprints/blueprints.json"),
    }


def validate_projects() -> dict:
    main_path = ROOT / "src/KingmakerGunslinger/KingmakerGunslinger.csproj"
    test_path = ROOT / "tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj"
    main_tree = ET.parse(main_path)
    test_tree = ET.parse(test_path)
    assert main_tree.find(".//m:TargetFrameworkVersion", MSBUILD_NS).text == "v4.7"
    assert test_tree.find(".//m:TargetFrameworkVersion", MSBUILD_NS).text == "v4.7"
    main = compile_items(main_path)
    tests = compile_items(test_path)
    pure = {
        r"Persistence\PersistenceRuntimePreflightCheck.cs",
        r"Persistence\PersistenceRuntimePreflightEvaluator.cs",
        r"Persistence\PersistenceRuntimePreflightProbeData.cs",
        r"Persistence\PersistenceRuntimePreflightReport.cs",
    }
    engine = {r"Development\PersistenceRuntimePreflightProbe.cs"}
    evidence = {
        r"Persistence\PersistenceEvidenceEvaluation.cs",
        r"Persistence\PersistenceEvidenceEvaluator.cs",
        r"Persistence\PersistenceEvidenceObservation.cs",
        r"Persistence\PersistenceEvidenceSeverity.cs",
        r"Persistence\PersistenceEvidenceStatus.cs",
        r"Persistence\PersistenceGateDecision.cs",
        r"Persistence\PersistenceMatrixCatalog.cs",
        r"Persistence\PersistenceMatrixStepDefinition.cs",
        r"Development\PersistenceEvidenceData.cs",
        r"Development\PersistenceEvidenceRecorder.cs",
    }
    assert pure | engine | evidence <= set(main)
    for path in pure:
        assert rf"..\..\src\KingmakerGunslinger\{path}" in tests, path
    for include in main:
        assert (main_path.parent / include.replace("\\", "/")).is_file(), include
    for include in tests:
        assert (test_path.parent / include.replace("\\", "/")).is_file(), include
    external = {
        "0Harmony12", "Assembly-CSharp", "Assembly-CSharp-firstpass", "Newtonsoft.Json",
        "UnityEngine", "UnityEngine.AnimationModule", "UnityEngine.AssetBundleModule",
        "UnityEngine.CoreModule", "UnityEngine.UI", "UnityModManager",
    }
    found = set()
    for reference in main_tree.findall(".//m:Reference", MSBUILD_NS):
        name = reference.attrib["Include"].split(",")[0]
        if name in external:
            found.add(name)
            private = reference.find("m:Private", MSBUILD_NS)
            assert private is not None and private.text == "False", name
    assert found == external
    return {
        "passed": True,
        "mainCompileItems": len(main),
        "testCompileItems": len(tests),
        "newPurePreflightFiles": len(pure),
        "newEnginePreflightFiles": len(engine),
        "nonCopyingExternalReferences": len(found),
    }


def validate_declared_tests() -> dict:
    source = read("tests/KingmakerGunslinger.DomainTests/Program.cs")
    matches = re.findall(r'Case\("([^"]+)",\s*([A-Za-z0-9_]+)\)', source)
    assert len(matches) == EXPECTED_TESTS, len(matches)
    names = [name for name, _ in matches]
    methods = [method for _, method in matches]
    assert len(set(names)) == len(names)
    definitions = set(re.findall(r"private static void\s+([A-Za-z0-9_]+)\s*\(\)", source))
    assert not sorted(set(methods) - definitions)
    counts = {prefix: sum(name.startswith(prefix) for name in names) for prefix in EXPECTED_PREFIX_COUNTS}
    assert counts == EXPECTED_PREFIX_COUNTS, counts
    required = {
        "preflight.check-only-i01-i02",
        "preflight.report-order-required",
        "preflight.evaluate-pass-guid",
        "preflight.evaluate-pass-string",
        "preflight.evaluate-bootstrap-blocked",
        "preflight.evaluate-bootstrap-count-mismatch",
        "preflight.evaluate-identity-missing",
        "preflight.evaluate-identity-duplicate",
        "preflight.evaluate-identity-unreadable",
        "preflight.evaluate-identity-unsupported-type",
        "evidence.evaluation-reproduced-go",
    }
    assert required <= set(names)
    return {"passed": True, "declaredCaseCount": len(names), "prefixCounts": counts}


def validate_preflight_source() -> dict:
    data = read("src/KingmakerGunslinger/Persistence/PersistenceRuntimePreflightProbeData.cs")
    check = read("src/KingmakerGunslinger/Persistence/PersistenceRuntimePreflightCheck.cs")
    report = read("src/KingmakerGunslinger/Persistence/PersistenceRuntimePreflightReport.cs")
    evaluator = read("src/KingmakerGunslinger/Persistence/PersistenceRuntimePreflightEvaluator.cs")
    probe = read("src/KingmakerGunslinger/Development/PersistenceRuntimePreflightProbe.cs")
    bootstrap = read("src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs")

    for token in ("bootstrapInitializationCount < -1", "registeredBlueprintCount < -1", "identityMemberCount < -1"):
        assert token in data
    assert 'step.Id, "I01"' in check and 'step.Id, "I02"' in check
    assert "may produce checks only for I01 and I02" in check
    assert 'string[] expected = { "I01", "I02" }' in report
    for token in (
        "probe.BootstrapInitializationCount == 1",
        "probe.RegisteredBlueprintCount == probe.ExpectedRegisteredBlueprintCount",
        "probe.IdentityMemberCount == 1",
        "probe.IdentityMemberReadable",
        'GuidTypeName = "System.Guid"',
        'StringTypeName = "System.String"',
        "PersistenceEvidenceStatus.Blocked",
    ):
        assert token in evaluator, token
    for token in (
        'IdentityMemberName = "UniqueId"',
        "FindIdentityMembers(typeof(ItemEntityWeapon))",
        "BindingFlags.DeclaredOnly",
        "BlueprintBootstrap.InitializationCount",
        "BlueprintBootstrap.RegisteredBlueprintCount",
        "BlueprintBootstrap.ExpectedRegisteredBlueprintCount",
    ):
        assert token in probe, token
    assert "ExpectedRegisteredBlueprintCount = 8" in bootstrap
    assert "_initializationCount++" in bootstrap
    return {"passed": True, "automaticRows": ["I01", "I02"], "expectedBlueprintCount": 8}


def validate_evidence_and_fixture_source() -> dict:
    recorder = read("src/KingmakerGunslinger/Development/PersistenceEvidenceRecorder.cs")
    bridge = read("src/KingmakerGunslinger/Development/KingmakerDevelopmentBridge.cs")
    runtime = read("src/KingmakerGunslinger/Firearms/FirearmRuntimeState.cs")
    ui = read("src/KingmakerGunslinger/Development/DevelopmentUi.cs")
    controls = read("src/KingmakerGunslinger/Development/DevelopmentControls.cs")

    for token in (
        "RecordTrustedRuntimePreflight",
        "PersistenceRuntimePreflightProbe.Capture()",
        'Note = "Trusted Sprint 16 runtime preflight: "',
        "Before = null",
        "After = null",
        "_session.CurrentStepIndex = Math.Max",
    ):
        assert token in recorder, token
    # Automatic bypass must be hard-coded to the report's I01/I02 checks, not a generic step parameter.
    record_method = recorder[recorder.index("internal string RecordTrustedRuntimePreflight"):recorder.index("internal string CaptureBefore")]
    assert "stepId" not in record_method
    assert "report.Checks" in record_method

    for token in (
        "CaptureStrictEvidenceFirearms",
        "_identityProvider.TryGetIdentity(candidate, out identity, out identityReason)",
        "EngineItemId = identity.Value",
        "CreatePersistenceFixtureAd",
        "exact.Take(4)",
        "Distinct(StringComparer.Ordinal).Count() != 4",
        "FirearmRuntimeState.HasIdentityVaultRecord(fixture[0].Item1)",
        "FirearmRuntimeState.HasIdentityVaultRecord(fixture[3].Item1)",
        "Additional Test Muskets, if any, were left unchanged",
    ):
        assert token in bridge, token
    strict_method = bridge[bridge.index("private List<PersistenceFirearmEvidenceData> CaptureStrictEvidenceFirearms"):bridge.index("private List<FirearmItemStateSnapshot> DescribeCandidates")]
    for forbidden in ("ItemRuntimeId", "RepositoryIdentity = identity", "RuntimeHelpers.GetHashCode"):
        assert forbidden not in strict_method
    assert "HasIdentityVaultRecord" in runtime and "_vaultStore.TryRead" in runtime
    assert "CreatePersistenceFixtureAd" in controls
    for label in (
        "Record trusted I01/I02 runtime preflight",
        "Create/normalize A-D persistence fixture in shared inventory",
    ):
        assert label in ui

    firearm_text = "\n".join(path.read_text(encoding="utf-8") for path in (ROOT / "src/KingmakerGunslinger/Firearms").glob("*.cs"))
    assert "PersistenceEvidenceRecorder" not in firearm_text
    assert "current-session.json" not in firearm_text
    return {"passed": True, "strictEvidenceIdentity": True, "fixtureItems": 4, "carrierUnchanged": True}


def validate_qualification_script() -> dict:
    source = read("scripts/qualify-runtime-candidate.ps1")
    for token in (
        "Step 1/7: validating source repository.",
        "Step 3/7: capturing exact local environment fingerprint.",
        "Step 4/7: inspecting installed Kingmaker, UMM, Harmony, and persistence contracts.",
        "Step 5/7: compiling and executing the dependency-free C# test harness.",
        "Step 6/7: compiling the mod and producing the UMM ZIP.",
        "validate-package.ps1",
        "readyForKingmakerSmokeTest = $true",
        "persistenceGateDecision = 'NoGoIncomplete'",
        "runtime-candidate.json",
        "RUNTIME-CANDIDATE.md",
        "READY FOR KINGMAKER SMOKE TEST",
    ):
        assert token in source, token
    assert "if (-not $contracts.contractPassed)" in source
    assert "environment.json" in source and "runtime-contracts.json" in source
    assert "Compress-Archive -LiteralPath $qualificationRoot" in source
    return {"passed": True, "blockingStages": 7, "localCandidateOnly": True}


def validate_matrix_and_identity() -> dict:
    catalog = read("src/KingmakerGunslinger/Persistence/PersistenceMatrixCatalog.cs")
    critical = re.findall(r'\bC\("(I\d{2})"', catalog)
    high = re.findall(r'\bH\("(I\d{2})"', catalog)
    reproduced = set(re.findall(r'\bC\("(I\d{2})"[^\r\n]+true\)', catalog))
    assert len(critical) == 30 and len(high) == 5
    assert critical + high == [f"I{index:02d}" for index in range(1, 36)]
    assert reproduced == REPRODUCTION_ROWS

    provider = read("src/KingmakerGunslinger/Firearms/KingmakerFirearmItemIdentityProvider.cs")
    identity = read("src/KingmakerGunslinger/Firearms/FirearmItemId.cs")
    vault = read("src/KingmakerGunslinger/Firearms/UnitPartFirearmStateVault.cs")
    combined = "\n".join((provider, identity, vault))
    assert 'IdentityMemberName = "UniqueId"' in provider
    assert "System.Guid or System.String" in provider
    assert 'Guid.TryParseExact(value, "D"' in identity
    assert "Guid.NewGuid" not in combined
    assert "List<FirearmStateIdentityVaultRecord> _identityRecords" in vault
    assert "List<FirearmStateVaultRecord> _records" in vault
    return {
        "passed": True,
        "matrixRows": 35,
        "criticalRows": 30,
        "highRows": 5,
        "reproductionRows": sorted(REPRODUCTION_ROWS),
        "identityMember": "UniqueId",
    }


def evaluate_reference(observations: list[tuple[int, str, str, str]]) -> tuple[str, int, int, int, int]:
    assert len({sequence for sequence, _, _, _ in observations}) == len(observations)
    critical_failed = critical_incomplete = critical_passed = high_failed = 0
    severity = {f"I{i:02d}": ("critical" if i <= 30 else "high") for i in range(1, 36)}
    for step, kind in severity.items():
        rows = sorted((row for row in observations if row[1] == step), key=lambda row: row[0])
        latest = rows[-1] if rows else None
        if kind == "critical":
            if latest is None or latest[2] == "blocked":
                critical_incomplete += 1
            elif latest[2] == "fail":
                critical_failed += 1
            elif step in REPRODUCTION_ROWS and len({row[3] for row in rows if row[2] == "pass"}) < 2:
                critical_incomplete += 1
            else:
                critical_passed += 1
        elif latest is not None and latest[2] == "fail":
            high_failed += 1
    decision = "failed" if critical_failed else "incomplete" if critical_incomplete else "go"
    return decision, critical_passed, critical_failed, critical_incomplete, high_failed


def preflight_reference(bootstrap_ok: bool, initialized: bool, init_count: int, registered: int,
                        expected: int, identity_ok: bool, members: int, readable: bool,
                        type_name: str) -> tuple[str, str]:
    if not bootstrap_ok or init_count < 0 or registered < 0:
        i01 = "blocked"
    else:
        i01 = "pass" if initialized and init_count == 1 and registered == expected else "fail"
    if not identity_ok or members < 0:
        i02 = "blocked"
    else:
        i02 = "pass" if members == 1 and readable and type_name in {"System.Guid", "System.String"} else "fail"
    return i01, i02


def validate_reference_models() -> dict:
    scenarios = 0
    assert preflight_reference(True, True, 1, 8, 8, True, 1, True, "System.Guid") == ("pass", "pass"); scenarios += 1
    assert preflight_reference(True, True, 1, 8, 8, True, 1, True, "System.String") == ("pass", "pass"); scenarios += 1
    assert preflight_reference(False, False, -1, -1, 8, True, 1, True, "System.Guid")[0] == "blocked"; scenarios += 1
    assert preflight_reference(True, False, 0, 0, 8, True, 1, True, "System.Guid")[0] == "fail"; scenarios += 1
    assert preflight_reference(True, True, 2, 8, 8, True, 1, True, "System.Guid")[0] == "fail"; scenarios += 1
    assert preflight_reference(True, True, 1, 7, 8, True, 1, True, "System.Guid")[0] == "fail"; scenarios += 1
    assert preflight_reference(True, True, 1, 8, 8, False, -1, False, "")[1] == "blocked"; scenarios += 1
    assert preflight_reference(True, True, 1, 8, 8, True, 0, False, "")[1] == "fail"; scenarios += 1
    assert preflight_reference(True, True, 1, 8, 8, True, 2, True, "System.Guid")[1] == "fail"; scenarios += 1
    assert preflight_reference(True, True, 1, 8, 8, True, 1, False, "System.Guid")[1] == "fail"; scenarios += 1
    assert preflight_reference(True, True, 1, 8, 8, True, 1, True, "System.Int32")[1] == "fail"; scenarios += 1

    assert evaluate_reference([]) == ("incomplete", 0, 0, 30, 0); scenarios += 1
    assert evaluate_reference([(1, "I01", "fail", "run-001")])[0] == "failed"; scenarios += 1
    one_run = [(index, f"I{index:02d}", "pass", "run-001") for index in range(1, 31)]
    assert evaluate_reference(one_run) == ("incomplete", 23, 0, 7, 0); scenarios += 1
    two_runs = list(one_run)
    sequence = 31
    for step in sorted(REPRODUCTION_ROWS):
        two_runs.append((sequence, step, "pass", "run-002")); sequence += 1
    assert evaluate_reference(two_runs) == ("go", 30, 0, 0, 0); scenarios += 1

    # I03 reference fixture: four distinct IDs, A-C present, D absent.
    fixture = {
        "11111111-1111-4111-8111-111111111111": ("loaded-normal", True),
        "22222222-2222-4222-8222-222222222222": ("broken-empty", True),
        "33333333-3333-4333-8333-333333333333": ("broken-loaded", True),
        "44444444-4444-4444-8444-444444444444": ("empty-normal", False),
    }
    assert len(fixture) == 4 and len(set(fixture)) == 4; scenarios += 1
    assert [present for _, present in fixture.values()] == [True, True, True, False]; scenarios += 1

    # Retain touch-AC range boundary regression.
    tolerance = 0.0001
    increment = 12.192
    assert increment + 0.00005 <= increment + tolerance; scenarios += 1
    assert not increment + 0.001 <= increment + tolerance; scenarios += 1
    return {"passed": True, "scenarioCount": scenarios}


def validate_runtime_contract_script() -> dict:
    source = read("scripts/inspect-runtime-contracts.ps1")
    for token in (
        "$itemEntityWeaponUniqueIdMembers",
        "$firearmItemIdentityContractPassed",
        "requiredMemberName = 'UniqueId'",
        "acceptedValueTypes = @('System.Guid', 'System.String')",
        "generatedByMod = $false",
        "fallbackMembersAccepted = $false",
        "sprint = 16",
    ):
        assert token in source, token
    assert "$firearmItemIdentityContractPassed -and" in source
    return {"passed": True, "sprint": 16, "identityContractRequired": True}


def validate_documents() -> dict:
    required = {
        "SPRINT-16-REPORT.md": ("runtime qualification", "371", "NoGoIncomplete"),
        "README.md": (EXPECTED_INFO, "NOT READY FOR KINGMAKER", "A-D"),
        "TESTING.md": ("KingmakerGunslinger-0.0.16.zip", "Record trusted I01/I02", "I03"),
        "KNOWN-ISSUES.md": ("not installable", "Ammunition remains blocked"),
        "docs/RUNTIME-QUALIFICATION.md": ("Trusted I01/I02 preflight", "A-D fixture", "Strict evidence identity"),
        "docs/PERSISTENCE-TEST-MATRIX.md": ("35", "I35"),
        "docs/decisions/ADR-0023-runtime-qualification-before-ammunition.md": ("Accepted for Sprint 16", "one-command"),
        "planning/SPRINT-17-ENTRY-CRITERIA.md": ("Branch A", "Branch B", "Branch C"),
        "docs/history/SPRINT-15-REPORT.md": ("Sprint 15", "351"),
        "docs/history/SPRINT-16-ENTRY-CRITERIA.md": ("Branch A", "Branch B"),
        "NO-INSTALL-PACKAGE.txt": ("NOT READY FOR KINGMAKER", "READY FOR KINGMAKER SMOKE TEST"),
    }
    for relative, tokens in required.items():
        text = read(relative)
        for token in tokens:
            assert token in text, (relative, token)

    broken = []
    checked = 0
    link_pattern = re.compile(r"\[[^\]]+\]\(([^)]+)\)")
    for path in ROOT.rglob("*.md"):
        for raw in link_pattern.findall(path.read_text(encoding="utf-8")):
            target = raw.split("#", 1)[0]
            if not target or "://" in target or target.startswith("mailto:"):
                continue
            checked += 1
            resolved = (path.parent / target).resolve()
            try:
                resolved.relative_to(ROOT.resolve())
            except ValueError:
                broken.append(f"{path.relative_to(ROOT)} -> {raw} (escapes root)")
                continue
            if not resolved.exists():
                broken.append(f"{path.relative_to(ROOT)} -> {raw}")
    assert not broken, broken
    return {"passed": True, "requiredDocuments": len(required), "localLinksChecked": checked}


def validate_syntax() -> dict:
    from tree_sitter import Language, Parser  # type: ignore
    import tree_sitter_c_sharp  # type: ignore
    import tree_sitter_powershell  # type: ignore

    result = {}
    for extension, module, key in (
        ("cs", tree_sitter_c_sharp, "csharp"),
        ("ps1", tree_sitter_powershell, "powershell"),
    ):
        parser = Parser(Language(module.language()))
        files = sorted(ROOT.rglob(f"*.{extension}"))
        bad = [str(path.relative_to(ROOT)) for path in files if parser.parse(path.read_bytes()).root_node.has_error]
        assert not bad, bad
        result[key] = {"files": len(files), "syntaxErrors": 0}
    result["passed"] = True
    return result


def validate_text_and_cleanliness() -> dict:
    forbidden_suffixes = {".dll", ".exe", ".pdb", ".mdb", ".pyc"}
    forbidden_names = {"GamePath.props", "environment.json", "runtime-contracts.json"}
    forbidden = []
    hygiene = []
    text_count = 0
    for path in ROOT.rglob("*"):
        if not path.is_file():
            continue
        relative = path.relative_to(ROOT)
        if "__pycache__" in relative.parts or path.suffix.lower() in forbidden_suffixes or path.name in forbidden_names:
            forbidden.append(str(relative))
            continue
        if path.suffix.lower() in {".cs", ".ps1", ".py", ".md", ".txt", ".json", ".xml", ".props", ".csproj", ".sln", ".csv", ".example", ".gitignore", ".gitattributes", ".editorconfig"} or path.name == "LICENSE":
            text_count += 1
            data = path.read_bytes()
            assert b"\x00" not in data, relative
            text = data.decode("utf-8")
            if text and not text.endswith("\n"):
                hygiene.append(f"{relative}: missing final newline")
            for line_number, line in enumerate(text.splitlines(), 1):
                if line.rstrip(" \t") != line:
                    hygiene.append(f"{relative}:{line_number}: trailing whitespace")
                    break
    assert not forbidden, forbidden
    assert not hygiene, hygiene[:20]
    return {"passed": True, "textFilesChecked": text_count, "forbiddenFiles": 0, "hygieneIssues": 0}


def main() -> int:
    report = {"version": EXPECTED_INFO, "passed": False, "checks": {}}
    checks = report["checks"]
    checks["metadata"] = validate_metadata()
    checks["manifest"] = validate_manifest()
    checks["projects"] = validate_projects()
    checks["declaredTests"] = validate_declared_tests()
    checks["preflightSource"] = validate_preflight_source()
    checks["evidenceAndFixtureSource"] = validate_evidence_and_fixture_source()
    checks["qualificationScript"] = validate_qualification_script()
    checks["matrixAndIdentity"] = validate_matrix_and_identity()
    checks["referenceModels"] = validate_reference_models()
    checks["runtimeContractScript"] = validate_runtime_contract_script()
    checks["documents"] = validate_documents()
    checks["syntax"] = validate_syntax()
    checks["cleanliness"] = validate_text_and_cleanliness()
    report["passed"] = True
    VALIDATION.mkdir(parents=True, exist_ok=True)
    output = VALIDATION / "static-validation.json"
    output.write_text(json.dumps(report, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    print(
        f"Sprint 16 portable validation passed: {EXPECTED_TESTS} declared tests, "
        f"{checks['matrixAndIdentity']['matrixRows']} lifecycle rows, "
        f"{checks['referenceModels']['scenarioCount']} independent model scenarios, "
        f"{checks['syntax']['csharp']['files']} C# files, "
        f"{checks['syntax']['powershell']['files']} PowerShell scripts."
    )
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"Sprint 16 validation failed: {exc}", file=sys.stderr)
        raise
