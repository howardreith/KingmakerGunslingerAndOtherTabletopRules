#!/usr/bin/env python3
"""Retain the 0.0.143 findability contracts for the 0.0.144 release metadata."""
import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_weapon_findability143 as qualified


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[1])
    qualified.VERSION = '0.0.144'
    qualified.validate(parser.parse_args().root.resolve())
