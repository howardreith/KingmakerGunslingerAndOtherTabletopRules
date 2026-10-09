#!/usr/bin/env python3
"""Retain the 0.0.143 findability contracts for the 0.0.145 Heirloom Nodachi icon release."""
import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_weapon_findability143 as qualified

VERSION = '0.0.145'
SUFFIX = 'heirloom-nodachi-icon'


def validate(root):
    inherited = qualified.baseline.validate

    def release(value):
        # The qualified 0.0.143 gate pins its own suffix; this release only
        # changes the informational/package name.
        qualified.baseline.INFORMATIONAL_VERSION = VERSION + '-' + SUFFIX
        qualified.baseline.PACKAGE_SUFFIX = SUFFIX
        inherited(value)

    qualified.baseline.validate = release
    qualified.VERSION = VERSION
    try:
        qualified.validate(root)
    finally:
        qualified.baseline.validate = inherited


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[1])
    validate(parser.parse_args().root.resolve())
