"""Read-only icon authoring gate. No image generation and no runtime dependency."""
from __future__ import annotations
import argparse
import hashlib
import json
import re
import struct
import sys
import zlib
from pathlib import Path

CATALOG = "assets-source/original-icons/icon-catalog.json"
PILOT = "assets-source/original-icons/icon-overhaul-v2/pilot/pilot-manifest.json"
REFERENCES = "docs/art/icon-reference-records.json"
DISPOSITIONS = {
    "original-required", "intentional-family-share", "native-semantic-reuse",
    "protected-existing", "native-monogram", "hidden-internal", "review-out-of-scope"
}
def read_json(root, path):
    return json.loads((root / path).read_text(encoding="utf-8-sig"))

def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def presentation_delta_matches(text, record):
    text = text.replace("\r\n", "\n")
    if text.count(record["after"]) != 1:
        return False
    restored = text.replace(record["after"], record["before"])
    return hashlib.sha256(restored.encode("utf-8")).hexdigest() == record["baselineSha256"]

def inside(root, value):
    if not isinstance(value, str) or not value or "\\" in value:
        raise ValueError("Expected nonempty repository-relative forward-slash path")
    path = (root / value).resolve()
    if Path(value).is_absolute() or not path.is_relative_to(root.resolve()):
        raise ValueError("Path escapes repository: " + value)
    # Exact spelling is part of the package contract even on case-insensitive Windows.
    cursor = root.resolve()
    for part in Path(value).parts:
        if part not in {p.name for p in cursor.iterdir()}:
            raise ValueError("Missing path or incorrect case: " + value)
        cursor = cursor / part
    return path

def png_info(path):
    data = path.read_bytes()
    if data[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError("Not PNG: " + str(path))
    offset, ihdr, idat, ended = 8, None, [], False
    while offset < len(data):
        if offset + 12 > len(data):
            raise ValueError("Truncated PNG")
        size = struct.unpack_from(">I", data, offset)[0]
        tag = data[offset + 4:offset + 8]
        chunk = data[offset + 8:offset + 8 + size]
        end = offset + 12 + size
        if end > len(data) or zlib.crc32(tag + chunk) != struct.unpack_from(">I", data, end - 4)[0]:
            raise ValueError("Invalid PNG chunk or CRC")
        if tag == b"IHDR":
            if ihdr is not None or size != 13:
                raise ValueError("Invalid PNG IHDR")
            ihdr = struct.unpack(">IIBBBBB", chunk)
        if tag == b"IDAT":
            idat.append(chunk)
        if tag == b"IEND":
            ended = True
            if end != len(data):
                raise ValueError("Trailing PNG data")
            break
        offset = end
    if not ended or not ihdr or not idat:
        raise ValueError("Incomplete PNG")
    w, h, depth, color, compression, filtering, interlace = ihdr
    if not (0 < w <= 16384 and 0 < h <= 16384):
        raise ValueError("Invalid PNG dimensions")
    if depth == 8 and color in (2, 6) and interlace == 0:
        raw = zlib.decompress(b"".join(idat))
        if len(raw) != h * (1 + w * (3 if color == 2 else 4)):
            raise ValueError("PNG scanline size mismatch")
    return {"size": [w, h], "depth": depth, "color": color, "interlace": interlace}

def validate(root, catalog=None, pilot=None, references=None, registry=None):
    """Optional in-memory documents support corruption tests without mutating files."""
    root = Path(root).resolve()
    catalog = read_json(root, CATALOG) if catalog is None else catalog
    pilot = read_json(root, PILOT) if pilot is None else pilot
    references = read_json(root, REFERENCES) if references is None else references
    registry = read_json(root, "blueprints/blueprints.json") if registry is None else registry
    errors = []
    def require(ok, message):
        if not ok:
            errors.append(message)
    def file_check(path, expected=None, size=None):
        try:
            target = inside(root, path)
            if expected is not None:
                require(bool(re.fullmatch("[0-9a-f]{64}", expected)), "Invalid hash: " + path)
                require(sha256(target) == expected, "Hash mismatch: " + path)
            if size is not None:
                require(png_info(target)["size"] == size, "PNG dimensions: " + path)
            return target
        except (ValueError, OSError, zlib.error, struct.error) as exc:
            errors.append(str(exc))
            return None

    require(catalog.get("schemaVersion") == 1, "Unsupported catalog schema")
    for path in [catalog["guide"], catalog["referenceIndex"], REFERENCES,
                 "reports/icon-overhaul/PILOT-REVIEW.html"]:
        file_check(path)
    for path in ["AGENTS.md", "README.md"]:
        text = (root/path).read_text(encoding="utf-8-sig")
        require("docs/ICON-ART-GUIDE.md" in text, "Guide discovery missing: " + path)
    for group in ["delegatedManifests", "protectedFiles", "protectedAssignmentFiles"]:
        require(bool(catalog[group]), "Empty " + group)
        seen = set()
        for record in catalog[group]:
            require(record["path"] not in seen, "Duplicate " + group + ": " + record["path"])
            seen.add(record["path"])
            file_check(record["path"], record["sha256"])
    file_check(catalog["baseline"]["registryPath"], catalog["baseline"]["registrySha256"])
    for delta in catalog["authorizedPresentationDeltas"]:
        target = file_check(delta["path"])
        if target:
            require(presentation_delta_matches(target.read_text(encoding="utf-8"), delta),
                    "Change outside authorized presentation delta: " + delta["path"])
    entries = {e["symbol"]: e for e in registry["entries"]}
    required = {s for s in entries if any(s.startswith(p) for p in catalog["coveragePrefixes"])}
    required.update(catalog["additionalRequiredSymbols"])
    consumers = catalog["consumers"]
    actual = {c["symbol"] for c in consumers}
    require(len(actual) == len(consumers), "Duplicate consumer")
    require(not required - actual, "Missing consumer: " + ", ".join(sorted(required - actual)))
    concepts = {c["key"]: c for c in catalog["concepts"]}
    require(len(concepts) == len(catalog["concepts"]), "Duplicate concept")
    for consumer in consumers:
        symbol = consumer["symbol"]
        original = entries.get(symbol)
        require(original is not None, "Unknown consumer: " + symbol)
        if original:
            require(consumer["guid"] == original["guid"] and consumer["type"] == original["plannedType"],
                    "Identity mismatch: " + symbol)
        require(consumer["disposition"] in DISPOSITIONS, "Invalid disposition: " + symbol)
        require(consumer["concept"] in concepts, "Unknown concept: " + symbol)
        require(consumer["currentArt"] in catalog["currentArtSources"], "Unknown art source: " + symbol)
        for field in ["reason", "publication", "surface", "name", "module"]:
            require(bool(consumer.get(field)), "Missing consumer " + field + ": " + symbol)
    for record in catalog["currentArtSources"].values():
        file_check(record["source"])
    for record in references["projectReferences"]:
        file_check(record["path"], record["sha256"], record["size"])
        require(bool(record["approvalScope"]), "Reference lacks approval scope: " + record["id"])
    for record in references["nativeReferences"]:
        require(record["publication"] == "local-reference-only", "Native reference publication is forbidden")
        require(bool(re.fullmatch("[0-9a-f]{64}", record["sha256"])), "Invalid local reference hash")
    require(references["candidateReferenceAuthority"] == PILOT, "Competing candidate reference authority")

    records = {r["key"]: r for r in pilot["records"]}
    require(len(records) == len(pilot["records"]), "Duplicate pilot key")
    exported_hashes = {}
    for key, record in records.items():
        file_check(record["source"], record["sourceSha256"], record["sourceSize"])
        path = file_check(record["export"], record["exportSha256"], record["exportSize"])
        require(record["source"] != record["export"], "Export overwrites original: " + key)
        if path:
            try:
                info = png_info(path)
                require(info["depth"] == 8 and info["color"] == 6 and info["interlace"] == 0,
                        "Runtime export must be 8-bit noninterlaced RGBA: " + key)
            except (ValueError, zlib.error, struct.error) as exc:
                errors.append(str(exc))
        require(record["exportSize"] == ([64, 64] if key == "rapid-reload" else [128, 128]),
                "Export profile mismatch: " + key)
        require(record["export"].startswith("assets-source/original-icons/icon-overhaul-v2/pilot/exports/"),
                "Pilot silently promoted to runtime: " + key)
        same = exported_hashes.setdefault(record["exportSha256"], key)
        require(same == key, "Distinct concepts duplicate artwork: " + same + " / " + key)
        concept = concepts.get(key)
        require(concept is not None, "Uncataloged pilot: " + key)
        if concept:
            require(concept["assetAuthority"] == {"manifest": PILOT, "key": key},
                    "Competing asset authority: " + key)
            review = concept["visualReview"]
            if review["status"] == "approved":
                require(review["reviewedExportSha256"] == record["exportSha256"],
                        "Stale visual approval hash: " + key)
                require(bool(review.get("evidence")), "Approval lacks owner evidence: " + key)
                if review.get("evidence"):
                    file_check(review["evidence"])
            else:
                require(review["reviewedExportSha256"] is None,
                        "Unapproved image has an approval hash: " + key)
        if record["visualStatus"] != "approved":
            require(record["approvedHash"] is None, "Candidate auto-approved: " + key)
        else:
            require(concept and concept["visualReview"]["status"] == "approved",
                    "Pilot approval not recorded in catalog: " + key)
    for key, concept in concepts.items():
        require(bool(concept["sharingReason"]), "Missing sharing reason: " + key)
        authority = concept.get("assetAuthority")
        if authority:
            require(authority["manifest"] == PILOT and authority["key"] in records,
                    "Unresolved asset authority: " + key)
        require(bool(concept["technicalStatus"]) and bool(concept["visualReview"]["status"]),
                "Technical/visual status missing: " + key)

    ui_ids = set()
    for entry in catalog["uiEntries"]:
        require(entry["id"] not in ui_ids, "Duplicate UI entry: " + entry["id"])
        ui_ids.add(entry["id"])
        original = entries.get(entry["parameterSymbol"])
        require(original is not None and original["guid"] == entry["parameterGuid"],
                "UI parameter identity mismatch: " + entry["id"])
        require(entry["nativeAcronym"] == entry["name"][0] and entry["name"] in {"Blunderbuss", "Musket", "Pistol"},
                "Unexpected firearm publication: " + entry["id"])
        file_check(entry["source"])
    require(len(ui_ids) == 15, "Expected 5 parametrized families x 3 native firearm entries")

    # Delegate summoning placement ownership rather than copying its 693-entry mapping.
    summoning = read_json(root, "assets-source/original-icons/expanded-summoning/icon-manifest.json")
    for icon in summoning["icons"]:
        for symbol in icon["blueprintSymbols"]:
            require(symbol in entries, "Stale delegated summon consumer: " + symbol)
    prompts = read_json(root, "assets-source/original-icons/icon-overhaul-v2/pilot/production-prompts.json")
    for revision in prompts["revisionInputs"]:
        file_check(revision["path"], revision["sha256"])
    return errors

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        errors = validate(args.root)
    except (KeyError, TypeError, ValueError, OSError) as exc:
        errors = ["Invalid icon catalog: " + str(exc)]
    if errors:
        for error in errors:
            print("FAIL:", error)
        return 1
    print("PASS: icon catalog coverage, identities, references, candidate PNGs, delegated manifests and protected bytes/assignment source.")
    print("Visual approval and native UI/runtime qualification remain separate gates.")
    return 0

if __name__ == "__main__":
    sys.exit(main())
