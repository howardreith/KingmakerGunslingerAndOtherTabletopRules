#!/usr/bin/env python3
"""Dispatch repository validation from authoritative Info.json version metadata."""
from __future__ import annotations

import argparse
import json
import subprocess
import sys
from pathlib import Path

VALIDATORS = {
    "0.0.29": "validate_sprint29.py",
    "0.0.30": "validate_sprint30.py",
    "0.0.31": "validate_sprint31.py",
    "0.0.32": "validate_sprint32.py",
    "0.0.33": "validate_sprint33.py",
    "0.0.34": "validate_sprint34.py",
    "0.0.35": "validate_sprint35.py",
    "0.0.36": "validate_sprint36.py",
    "0.0.37": "validate_sprint37.py",
    "0.0.38": "validate_sprint38.py",
    "0.0.39": "validate_sprint39.py",
    "0.0.40": "validate_sprint40.py",
    "0.0.41": "validate_sprint41.py",
    "0.0.42": "validate_sprint42.py",
    "0.0.43": "validate_sprint43.py",
    "0.0.44": "validate_sprint44.py",
    "0.0.45": "validate_sprint45.py",
    "0.0.46": "validate_sprint46.py",
    "0.0.47": "validate_sprint47.py",
    "0.0.50": "validate_sprint50.py",
    "0.0.51": "validate_sprint51.py",
    "0.0.52": "validate_sprint52.py",
    "0.0.53": "validate_sprint53.py",
    "0.0.54": "validate_sprint54.py",
    "0.0.55": "validate_sprint55.py",
    "0.0.56": "validate_sprint56.py",
    "0.0.57": "validate_sprint57.py",
    "0.0.58": "validate_sprint58.py",
    "0.0.59": "validate_sprint59.py",
    "0.0.60": "validate_sprint60.py",
    "0.0.61": "validate_playtest61.py",
    "0.0.62": "validate_playtest62.py",
    "0.0.63": "validate_playtest63.py",
    "0.0.64": "validate_playtest64.py",
    "0.0.65": "validate_playtest65.py",
    "0.0.66": "validate_playtest66.py",
    "0.0.67": "validate_playtest67.py",
    "0.0.68": "validate_playtest68.py",
    "0.0.69": "validate_playtest69.py",
    "0.0.70": "validate_playtest70.py",
    "0.0.71": "validate_playtest71.py",
    "0.0.72": "validate_compatibility72.py",
    "0.0.73": "validate_archetypes73.py",
    "0.0.74": "validate_paper74.py",
    "0.0.75": "validate_feature75.py",
    "0.0.76": "validate_acadamae76.py",
    "0.0.77": "validate_shield77.py",
    "0.0.78": "validate_summoning78.py",
    "0.0.79": "validate_spear79.py",
    "0.0.80": "validate_eastern80.py",
    "0.0.81": "validate_brown_fur81.py",
    "0.0.82": "validate_brown_fur82.py",
    "0.0.83": "validate_urban_barbarian83.py",
    "0.0.84": "validate_urban_barbarian84.py",
    "0.0.85": "validate_urban_barbarian85.py",
    "0.0.86": "validate_urban_barbarian86.py",
    "0.0.87": "validate_urban_barbarian87.py",
    "0.0.88": "validate_urban_barbarian88.py",
    "0.0.89": "validate_weapon_presentation89.py",
    "0.0.90": "validate_bodyguard90.py",
    "0.0.91": "validate_bodyguard91.py",
    "0.0.92": "validate_helpful92.py",
    "0.0.93": "validate_eastern_favored93.py",
    "0.0.94": "validate_in_harms_way94.py",
    "0.0.95": "validate_immediate_action95.py",
    "0.0.96": "validate_firearm_audio96.py",
    "0.0.97": "validate_compatibility_attribution97.py",
    "0.0.98": "validate_craft_magic_items98.py",
    "0.0.99": "validate_craft_magic_items99.py",
    "0.0.100": "validate_craft_magic_items100.py",
    "0.0.101": "validate_craft_magic_items101.py",
    "0.0.102": "validate_gunslinger_fixes102.py",
    "0.0.103": "validate_overhaul_summon_fatigue103.py",
    "0.0.104": "validate_summon_same_turn104.py",
    "0.0.105": "validate_player_presentation105.py",
    "0.0.106": "validate_fatigue_authority106.py",
    "0.0.107": "validate_icon_overhaul107.py",
    "0.0.108": "validate_icon_polish108.py",
    "0.0.109": "validate_martial_repair_notifications109.py",
    "0.0.110": "validate_protection_from_alignment110.py",
    "0.0.111": "validate_gunslinger_outfit_kitbash111.py",
    "0.0.112": "validate_ammunition_cmi_copy_notifications112.py",
    "0.0.113": "validate_save_load_hotfix113.py",
    "0.0.114": "validate_elemental_races114.py",
    "0.0.115": "validate_share_transmutation115.py",
    "0.0.116": "validate_elemental_feats116.py",
    "0.0.117": "validate_elemental_traits117.py",
}


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    root = args.root.resolve()
    try:
        info_path = root / "Info.json"
        info = json.loads(info_path.read_text(encoding="utf-8"))
        version = info.get("Version")
        validator_name = VALIDATORS.get(version)
        if validator_name is None:
            raise RuntimeError(f"Unsupported repository version: {version!r}")
        validator = Path(__file__).resolve().parent / validator_name
        command = [sys.executable, str(validator)]
        if version in {"0.0.100", "0.0.101", "0.0.102", "0.0.103",
                "0.0.104", "0.0.105", "0.0.106", "0.0.107", "0.0.108",
                "0.0.109", "0.0.110", "0.0.111", "0.0.112", "0.0.113",
                "0.0.114", "0.0.115", "0.0.116", "0.0.117"}:
            command.extend(["--root", str(root)])
        elif version in {"0.0.30", "0.0.31", "0.0.32", "0.0.33", "0.0.34", "0.0.35", "0.0.36", "0.0.37", "0.0.38", "0.0.39", "0.0.40", "0.0.41", "0.0.42", "0.0.43", "0.0.44", "0.0.45", "0.0.46", "0.0.47", "0.0.50", "0.0.51", "0.0.52", "0.0.53", "0.0.54", "0.0.55", "0.0.56", "0.0.57", "0.0.58", "0.0.59", "0.0.60", "0.0.61", "0.0.62", "0.0.63", "0.0.64", "0.0.65", "0.0.66", "0.0.67", "0.0.68", "0.0.69", "0.0.70", "0.0.71", "0.0.72", "0.0.73", "0.0.74", "0.0.75", "0.0.76", "0.0.77", "0.0.78", "0.0.79", "0.0.80", "0.0.81", "0.0.82", "0.0.83", "0.0.84", "0.0.85", "0.0.86", "0.0.87", "0.0.88", "0.0.89", "0.0.90", "0.0.91", "0.0.92", "0.0.93", "0.0.94", "0.0.95", "0.0.96", "0.0.97", "0.0.98", "0.0.99"}:
            command.extend(["--root", str(root)])
        elif root != Path(__file__).resolve().parents[1]:
            raise RuntimeError("Sprint 29 fixture-root dispatch is not supported by its historical CLI.")
        completed = subprocess.run(command, check=False)
        if completed.returncode:
            return completed.returncode
        print(f"Repository validation dispatched version {version} to {validator_name}.")
        return 0
    except Exception as exception:
        print(f"Repository validation failed: {exception}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
