"""Compact, hash-bound projection of Turn Undead loader-compat runs.

Reads the lab's retained machine-generated records (session-summary.json, each launch-summary.json and
harness-result.json) and writes reviewable JSON under docs/evidence/turn-undead/. Only an explicit selection of the
fields the mission's contract needs is copied (identities, resources and costs, DC classes and statistics, targeting,
saves with their conditional DC adjustments, modifier memberships, counts and hashes, outcomes, restoration). Native
component dumps, raw logs, saves and profiles stay in the lab; each run names the SHA-256 of every source record it was
derived from, so a reviewer with lab access can verify it. Absolute machine paths are never copied. Failed attempts are
retained with their original verdicts.

Usage (run from the repository root):
  python -I -B docs/evidence/turn-undead/extract_native_evidence.py <evidence-root> <fixture-dir>
"""
import hashlib
import json
import os
import sys

RUNS = [
    "20261128T094042Z-umm-0.31.1-turn-undead-first",
    "20261128T095145Z-umm-0.31.1-turn-undead-first",
    "20261128T095946Z-umm-0.31.1-turn-undead-first",
    "20261128T100756Z-umm-0.31.1-turn-undead-first",
    "20261128T104421Z-umm-0.31.1-turn-undead-first",
    "20261128T105226Z-umm-0.31.1-turn-undead-first",
    "20261128T105459Z-umm-0.31.1-turn-undead-cotw-contracts",
    "20261128T112755Z-umm-0.31.1-turn-undead-first",
    "20261128T113029Z-umm-0.31.1-turn-undead-cotw-contracts",
    "20261128T113902Z-umm-0.31.1-turn-undead-first",
    "20261128T114135Z-umm-0.31.1-turn-undead-cotw-contracts",
    "20261128T115155Z-umm-0.31.1-turn-undead-first",
    "20261128T115425Z-umm-0.31.1-turn-undead-cotw-contracts",
    "20261128T115645Z-umm-0.31.1-turn-undead-first",
    "20261128T135648Z-umm-0.31.1-turn-undead-first",
    "20261128T135954Z-umm-0.31.1-turn-undead-cotw-contracts",
    "20261128T140857Z-umm-0.31.1-turn-undead-first",
    "20261128T141137Z-umm-0.31.1-turn-undead-cotw-contracts",
]
# The runs whose contracts step is published as the profile contract extract.
CONTRACT_RUNS = [
    ("20261128T140857Z-umm-0.31.1-turn-undead-first", "contracts-vanilla.json"),
    ("20261128T141137Z-umm-0.31.1-turn-undead-cotw-contracts", "contracts-cotw.json"),
]
SESSION_FIELDS = [
    "sessionId", "session", "loaderProfile", "candidateZipSha256", "candidateDllSha256", "admission", "git",
    "status", "runtimeLease", "transactionId", "ownership", "registryRestore", "registryRestored",
    "ownerIntegrity", "ownerSavesUnchanged", "ownerInstallUnchanged", "ownerSaveFiles", "scenarioPassed",
    "transactionSucceeded", "candidateVersion", "hostConflict",
]
LAUNCH_FIELDS = ["label", "verdict", "problems", "seconds", "identity", "log", "harness"]
SAVE_FIELDS = ["unit", "type", "dc", "natural", "total", "passed", "sourceAbility", "reason", "gameSeconds"]
PROBE_COMMON = ["mode", "problems", "moduleStatus", "featPublished", "cotw", "area", "silencedAreaUnits",
                "interruptedCommands", "cleanupScope", "resistanceEvidence"]
DUMP_NOTE = "native component dump retained in the lab (see sourceRecords); not reproduced here"


def sha256(path):
    with open(path, "rb") as handle:
        return hashlib.sha256(handle.read()).hexdigest()


def load(path):
    with open(path, encoding="utf-8-sig") as handle:
        return json.load(handle)


def pick(source, keys):
    return {key: source.get(key) for key in keys if isinstance(source, dict) and key in source}


def saves(events):
    return [pick(event, SAVE_FIELDS) for event in events or []]


def unit_state(unit):
    if not isinstance(unit, dict):
        return None
    state = unit.get("fleeing")
    return {"distance": unit.get("distance"), "frightened": unit.get("frightened"), "panicked": unit.get("panicked"),
            "damage": unit.get("damage"),
            "fleeing": None if state is None else pick(state, ["initialized", "mindless", "ownerReview", "dc", "rule",
                                                                "sourceAbility", "timeLeftSeconds", "contextDc"])}


def first_turn(probe):
    cast = probe.get("cast") or {}
    rounds = probe.get("rounds") or {}
    recovery = probe.get("recovery") or {}
    expiry = probe.get("expiry") or {}
    blueprints = probe.get("blueprints") or {}
    out = {
        "blueprints": {"feat": pick(blueprints.get("feat") or {}, ["guid", "name", "icon", "groups"]),
                       "clericAction": pick(blueprints.get("clericAction") or {},
                                            ["guid", "name", "icon", "type", "actionType", "metamagic",
                                             "savingThrowText", "durationText"])},
        "setup": probe.get("setup"),
        "negativeControl": probe.get("negativeControl"),
        "cast": pick(cast, ["available", "previewDc", "previewDcNote", "expectedDc", "nativeCharacterLevelDcWouldBe",
                            "poolBefore", "poolAfter", "commandResult", "gameSecondsToFinish",
                            "farSkeletonDistanceAtCast"]),
        "initialOutcomes": probe.get("initialOutcomes"),
    }
    if rounds:
        out["rounds"] = dict(pick(rounds, ["gameSeconds", "otherSavesDuringRounds", "skeletonDistance", "lordDistance",
                                           "lordRecoveredEarly"]),
                             lordRepeatSaves=saves(rounds.get("lordRepeatSaves")),
                             skeletonRepeatSaves=saves(rounds.get("skeletonRepeatSaves")),
                             lordFleeingAtEnd=(rounds.get("lord") or {}).get("fleeing") is not None,
                             skeletonFleeingAtEnd=(rounds.get("skeleton") or {}).get("fleeing") is not None,
                             lordStateAtEnd=unit_state(rounds.get("lord")),
                             skeletonStateAtEnd=unit_state(rounds.get("skeleton")))
    if recovery:
        out["recovery"] = dict(pick(recovery, ["gameSeconds", "observedDuringRounds", "skeletonStillFleeing"]),
                               saves=saves(recovery.get("saves")), lordStateAfter=unit_state(recovery.get("lord")))
    if expiry:
        out["expiry"] = dict(pick(expiry, ["predictedSecondsLeft", "endedAfterCastSeconds", "skeletonRepeatSavesWholeEffect"]),
                             skeletonStateAfter=unit_state(expiry.get("skeleton")))
    return out


def with_saves(row, keys):
    out = dict(row)
    for key in keys:
        if key in out:
            out[key] = saves(out[key])
    return out


def contracts(probe):
    cotw = probe.get("callOfTheWild") or {}
    if not isinstance(cotw, dict):
        cotw = {"status": cotw}
    out_cotw = pick(cotw, ["status", "assemblyVersion", "sha256", "registryStatus", "registryUnreadableControl",
                           "channelEntries", "channelEntriesByType", "channelEntriesThatAreTurn", "positiveHarmEntries",
                           "turnIdentities", "turnMenu", "turnMembership", "membershipMutationControl"])
    holders = cotw.get("dcHolders")
    if isinstance(holders, dict):
        # Current schema: shallow fields with long lists as count + hash. Earlier runs held recursive dumps.
        out_cotw["dcHolders"] = {field: value if (value == "absent" or isinstance(value, dict) and "membershipProblems" in value)
                                 else DUMP_NOTE for field, value in holders.items()}
    resistances = cotw.get("channelResistances")
    if isinstance(resistances, list):
        out_cotw["channelResistances"] = resistances if all(isinstance(r, dict) and "membershipProblems" in r for r in resistances) else DUMP_NOTE
    return {
        "engine": probe.get("engine"), "methods": probe.get("methods"), "sources": probe.get("sources"),
        "feat": probe.get("feat"), "flight": probe.get("flight"), "channelResistance4": probe.get("channelResistance4"),
        "nativeContract": probe.get("nativeContract") if "nativeContract" in probe else DUMP_NOTE,
        "callOfTheWild": out_cotw,
    }


def project_probe(name, probe):
    out = pick(probe, PROBE_COMMON)
    trace = probe.get("trace", [])
    out["turnSaves"] = saves([e for e in trace if e.get("what") == "save"])
    out["damageEvents"] = [pick(e, ["initiator", "target", "damage", "value", "what", "gameSeconds"])
                           for e in trace if e.get("what") in ("damage", "heal")]
    mode = probe.get("mode", name)
    if mode == "first-turn":
        out.update(first_turn(probe))
    elif mode == "contracts":
        out.update(contracts(probe))
    elif mode == "undead-caster":
        out["undeadCaster"] = with_saves(probe.get("undeadCaster") or {}, ["clericSaves", "skeletonSaves", "livingControlSaves"])
    elif mode == "selective":
        out["selective"] = [with_saves(row, ["enemySaves", "allySaves"]) for row in probe.get("selective") or []]
    elif mode == "abandon":
        out["abandon"] = probe.get("abandon")
    elif mode == "abandon-interrupt":
        out["abandonInterrupt"] = probe.get("abandonInterrupt")
    elif mode == "cotw-rollback":
        out["cotwRollback"] = probe.get("cotwRollback")
    elif mode == "cotw-rollback-interrupt":
        out["cotwRollbackInterrupt"] = probe.get("cotwRollbackInterrupt")
    elif mode == "cotw-gameplay":
        out["dcProgression"] = [with_saves(row, ["saves"]) for row in probe.get("dcProgression") or []]
        payment = dict(probe.get("paladinPayment") or {})
        for key in ("extraRoute", "layOnHandsRoute"):
            if isinstance(payment.get(key), dict):
                payment[key] = with_saves(payment[key], ["saves"])
        out["paladinPayment"] = payment
    return out


def extract_run(root, run):
    base = os.path.join(root, run)
    summary_path = os.path.join(base, "session-summary.json")
    summary = load(summary_path)
    record = pick(summary, SESSION_FIELDS)
    record["registryVerification"] = pick(summary.get("registryVerification") or {}, ["status", "sha256", "backupSha256"])
    record["loader"] = pick(summary.get("loaderInstall") or {}, ["profile", "ummVersion", "harmonyVersion", "files"])
    record["stagedMods"] = summary.get("stagedMods")
    record["sourceRecords"] = {"session-summary.json": sha256(summary_path)}
    launches = []
    for launch in summary.get("launches", []):
        label = launch.get("label")
        entry = pick(launch, LAUNCH_FIELDS)
        launch_dir = os.path.join(base, label)
        for name in ("launch-summary.json", os.path.join("harness", "harness-result.json")):
            path = os.path.join(launch_dir, name)
            if os.path.exists(path):
                record["sourceRecords"][label + "/" + name.replace(os.sep, "/")] = sha256(path)
        result_path = os.path.join(launch_dir, "harness", "harness-result.json")
        if os.path.exists(result_path):
            result = load(result_path)
            entry["harnessStatus"] = result.get("status")
            entry["harnessFailure"] = result.get("failure")
            entry["isolationRestored"] = result.get("isolationRestored")
            entry["steps"] = result.get("steps")
            entry["probes"] = {key[len("kmgTurnUndead_"):]: project_probe(key[len("kmgTurnUndead_"):], value)
                               for key, value in result.items()
                               if key.startswith("kmgTurnUndead_") and isinstance(value, dict)}
        launches.append(entry)
    record["launches"] = launches
    return record


def write(path, document):
    with open(path, "w", encoding="utf-8", newline="\n") as handle:
        json.dump(document, handle, indent=1)
        handle.write("\n")


def main():
    if len(sys.argv) != 3:
        raise SystemExit(__doc__)
    root, fixtures = sys.argv[1], sys.argv[2]
    out_dir = os.path.dirname(os.path.abspath(__file__))
    fixture = os.path.join(fixtures, "KMCS_AFTER_NORMAL_012_MODONLY.zks")
    runs = [extract_run(root, run) for run in RUNS]
    write(os.path.join(out_dir, "native-runs.json"), {
        "schema": "kmg.turn-undead.native-runs.v2",
        "note": "Machine-generated by docs/evidence/turn-undead/extract_native_evidence.py from the lab's retained "
                "loader-compat records: an explicit projection of contract fields, outcomes and restoration records. "
                "Failed attempts are retained with their original verdicts. Native component dumps, raw logs, saves and "
                "profiles stay in the lab (hashes in each run's sourceRecords).",
        "referenceFixture": {"save": "KMCS_AFTER_NORMAL_012_MODONLY.zks", "sha256AtExtraction": sha256(fixture),
                             "provenance": "SHA-256 of the reference fixture file in the lab fixture directory, read when this "
                                           "extract was generated; each session staged its own copy of that file into its "
                                           "isolated profile (the staged copies are not hashed here)."},
        "observer": "tools/LoaderCompatHarness (TurnUndead*Probes.cs, TurnUndeadObserverRules.cs) at the run's git commit; "
                    "harness DLL hash in stagedMods.KmgLoaderCompatHarness",
        "runs": runs,
    })
    for run, name in CONTRACT_RUNS:
        record = next(r for r in runs if r["sessionId"] == run)
        launch = record["launches"][0]
        probe = launch["probes"]["contracts"]
        contract = {
            "schema": "kmg.turn-undead.contracts.v2",
            "run": run, "loaderProfile": record["loaderProfile"],
            "candidate": pick(record["admission"], ["producerCommit", "packageSha256", "dllSha256"]),
            "activeMods": (launch.get("identity") or {}).get("activeMods"),
            "sourceRecords": record["sourceRecords"],
        }
        contract.update({key: value for key, value in probe.items() if key not in ("turnSaves", "damageEvents")})
        write(os.path.join(out_dir, name), contract)
    print("wrote native-runs.json, " + ", ".join(name for _, name in CONTRACT_RUNS))


if __name__ == "__main__":
    main()
