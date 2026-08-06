#!/usr/bin/env python3
from __future__ import annotations
import argparse,sys
from pathlib import Path
sys.dont_write_bytecode=True
import validate_playtest67,validate_mysterious_stranger
VERSION="0.0.70";INFORMATIONAL_VERSION="0.0.70-focused-aim-repair"
def validate(root:Path)->None:
    validate_playtest67.VERSION=VERSION;validate_playtest67.INFORMATIONAL_VERSION=INFORMATIONAL_VERSION
    validate_playtest67.validate(root);validate_mysterious_stranger.validate(root)
    text=(root/"src/KingmakerGunslinger/Development/DevelopmentUi.cs").read_text(encoding="utf-8")
    if "Kingmaker Gunslinger - 0.0.70 FOCUSED-AIM-REPAIR / DODGE-EXPIRATION-R3" not in text:raise AssertionError("0.0.70 build label missing")
def main()->int:
    p=argparse.ArgumentParser();p.add_argument("--root",type=Path,default=Path(__file__).resolve().parents[1]);a=p.parse_args()
    try:validate(a.root.resolve())
    except Exception as e:print(f"Playtest {VERSION} validation failed: {e}",file=sys.stderr);return 1
    print(f"Playtest {VERSION} source validation passed.");return 0
if __name__=="__main__":raise SystemExit(main())
