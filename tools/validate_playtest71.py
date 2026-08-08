#!/usr/bin/env python3
from __future__ import annotations
import argparse,sys
from pathlib import Path
sys.dont_write_bytecode=True
import validate_playtest67,validate_mysterious_stranger
VERSION="0.0.71";INFORMATIONAL_VERSION="0.0.71-firearm-native-weapon-rigs"
def validate(root:Path)->None:
    validate_playtest67.VERSION=VERSION;validate_playtest67.INFORMATIONAL_VERSION=INFORMATIONAL_VERSION
    validate_playtest67.validate(root,892);validate_mysterious_stranger.validate(root)
    ui=(root/"src/KingmakerGunslinger/Development/DevelopmentUi.cs").read_text(encoding="utf-8")
    if f"Kingmaker Gunslinger - {VERSION} " not in ui or "FIREARM-NATIVE-WEAPON-RIGS" not in ui:raise AssertionError(f"{VERSION} build label missing")
    profile=(root/"src/KingmakerGunslinger/Assets/FirearmPresentationProfile.cs").read_text(encoding="utf-8")
    if profile.count("FirearmPresentationReadiness.AutonomousCandidate") < 5:raise AssertionError("all five candidates not enabled")
    if "FirearmPresentationReadiness.HumanAccepted," in profile:raise AssertionError("weapon falsely marked HumanAccepted")
    if "ThrownStraight" in profile:raise AssertionError("ThrownStraight candidate prohibited")
    if not (root/"src/KingmakerGunslinger/Development/FirearmVisualCalibration.cs").exists():raise AssertionError("calibration lab missing")
    if (root/"src/KingmakerGunslinger/Assets/FirearmVisualEquipmentHandler.cs").exists():raise AssertionError("obsolete renderer scan returned")
    builder=(root/"tools/unity/BuildFirearmBundles.cs").read_text(encoding="utf-8")
    for token in ("RetainHighestDetailRenderers", "KMG_RIG_RENDERER",
                  "ValidateVisibleScales", "MakeHeldLongGunMeshesTwoSided",
                  "policy=opaque-standard-with-reversed-backfaces",
                  "KMG_RIG_BINDING", "KMG_RIG_TRANSFORM", "KMG_RIG_BOUNDS",
                  "RemoveDuplicatePreviewGeometry", "model.dae",
                  "Final2 Sketchfab.fbx",
                  "SourceGripPoint", "SourceSupportPoint", "SourceButtPoint",
                  "SourceMuzzlePoint", "AnchorRelativeToGrip", "KMG_RIG_ANCHORS",
                  "new Vector3(0f, 180f, 180f), 0.24f"):
        if token not in builder:raise AssertionError(f"visibility repair missing: {token}")
    if (root/"tools/unity/KmgDoubleSidedDiffuse.shader").exists():raise AssertionError("runtime-invisible custom shader returned")
def main()->int:
    p=argparse.ArgumentParser();p.add_argument("--root",type=Path,default=Path(__file__).resolve().parents[1]);a=p.parse_args()
    try:validate(a.root.resolve())
    except Exception as e:print(f"Playtest {VERSION} validation failed: {e}",file=sys.stderr);return 1
    print(f"Playtest {VERSION} source validation passed.");return 0
if __name__=="__main__":raise SystemExit(main())
