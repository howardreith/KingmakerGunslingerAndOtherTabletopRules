"""Targeted offline read of installed Kingmaker blueprints for the Turn Undead record.

Reads Kingmaker_Data/sharedassets1.assets (Unity 2018.4 serialized file, format 17, no type trees) without the game
and writes docs/evidence/turn-undead/undead-identity-references.json:
  * for each ambiguous undead unit, the installed blueprints that reference it, walked upward through element objects
    (for example a spawn action inside an ability's run action) to the owning blueprint;
  * for each native positive harm channel, every ContextActionSavingThrow element it reaches with its save type and
    the length of its m_ConditionalDCIncrease array.
Only names, GUIDs and counts are written; no blueprint content is copied.

Usage (from the repository root):
  python -I -B docs/evidence/turn-undead/read_installed_blueprints.py "<Kingmaker>/Kingmaker_Data/sharedassets1.assets"
"""
import bisect
import hashlib
import json
import mmap
import os
import re
import struct
import sys

UNITS = {
    "4050be4512d245f40bf9461074b672f4": "SkeletonSummoned",
    "209f78dd464984c40b1eb43fabdab5a0": "SkeletonSummoned_ch5",
    "53e228ba7fe18104c93dc4b7294a1b30": "SummonedSkeletonChampion",
    "976d4937eb2620c4baf465bb6c7a00a7": "CR0.5_SkeletonMage",
    "f68391521a2175647ab311fc64108574": "CR13_BloodyBonesBarbarian",
    "39ace3bd77efd5546ad85b9e9947a311": "LoneWarrior",
    "fe662d20a0272bb4ea66bef675b4b52d": "CR12_DreadZombieCyclopCleric",
    "113ee4680b60b974696313c00b29c4aa": "DLC2Village_DreadZombieCyclopCleric",
    "2e286f14e73b385499551761afa66ce1": "UndeadCyclopsPriest",
    "f0a4efcaa7173bd43ba7fcf1f576e484": "UndeadCyclopsPriest",
}
HARMS = {
    "279447a6bf2d3544d93a0a39c3b8e91d": "ChannelPositiveHarm",
    "4937473d1cfd7774a979b625fb833b47": "ChannelEnergyPaladinHarm",
    "cc17243b2185f814aa909ac6b6599eaa": "ChannelEnergyHospitalerHarm",
    "e1536ee240c5d4141bf9f9485a665128": "ChannelEnergyEmpyrealHarm",
}
SAVE_TYPES = {1: "Fortitude", 2: "Reflex", 3: "Will"}


class Assets:
    def __init__(self, path):
        self.file = open(path, "rb")
        self.mm = mmap.mmap(self.file.fileno(), 0, access=mmap.ACCESS_READ)
        mm = self.mm
        _, _, version, data_offset = struct.unpack(">IIII", mm[0:16])
        if version != 17:
            raise SystemExit("unsupported serialized-file version %d" % version)
        p = mm.find(b"\0", 20) + 1 + 4
        if mm[p] != 0:
            raise SystemExit("type trees present; this reader expects none")
        p += 1
        (count,) = struct.unpack("<i", mm[p:p + 4]); p += 4
        for _ in range(count):
            (cid,) = struct.unpack("<i", mm[p:p + 4]); p += 4 + 1 + 2
            p += 16 if cid == 114 else 0
            p += 16
        (objects,) = struct.unpack("<i", mm[p:p + 4]); p += 4
        self.objects = []
        for _ in range(objects):
            p = (p + 3) & ~3
            pid, start, size, _tid = struct.unpack("<qIIi", mm[p:p + 20]); p += 20
            self.objects.append((pid, data_offset + start, size))
        self.objects.sort(key=lambda o: o[1])
        self.starts = [o[1] for o in self.objects]
        self.by_pid = {o[0]: o for o in self.objects}

    def at(self, offset):
        i = bisect.bisect_right(self.starts, offset) - 1
        o = self.objects[i]
        return o if o[1] <= offset < o[1] + o[2] else None

    def data(self, o):
        return self.mm[o[1]:o[1] + o[2]]

    def name(self, o):
        b = self.data(o)
        (n,) = struct.unpack("<i", b[28:32])  # MonoBehaviour: m_GameObject, m_Enabled, m_Script, then m_Name
        return b[32:32 + n].decode("utf-8", "replace") if 0 < n < 300 else "?"

    def name_end(self, o):
        (n,) = struct.unpack("<i", self.data(o)[28:32])
        return (32 + n + 3) & ~3

    def guid(self, o):
        m = re.search(rb"([0-9a-f]{32})", self.data(o)[:400])
        return m.group(1).decode() if m else None

    def find_guid(self, guid):
        offset = self.mm.find(guid.encode())
        return self.at(offset) if offset >= 0 else None

    def referencers(self, pid):
        pattern = struct.pack("<iq", 0, pid)
        found, pos = [], 0
        while True:
            pos = self.mm.find(pattern, pos)
            if pos < 0:
                return found
            o = self.at(pos)
            if o and o[0] != pid and o not in found:
                found.append(o)
            pos += 1

    def references(self, o):
        b = self.data(o)
        out = []
        for i in range(0, len(b) - 11, 4):
            fid, pid = struct.unpack("<iq", b[i:i + 12])
            if fid == 0 and pid in self.by_pid and pid != o[0]:
                out.append(self.by_pid[pid])
        return out


def owners(assets, pid, depth=0, seen=None):
    """Blueprints reaching an object, through element objects (names starting with '$')."""
    seen = seen if seen is not None else {pid}
    result = []
    if depth > 6:
        return result
    for o in assets.referencers(pid):
        if o[0] in seen:
            continue
        seen.add(o[0])
        name = assets.name(o)
        if name.startswith("$") or name == "?":
            via = name.split("$")[1] if name.startswith("$") else "?"
            for owner in owners(assets, o[0], depth + 1, seen):
                owner["via"] = [via] + owner.get("via", [])
                result.append(owner)
        else:
            result.append({"blueprint": name, "guid": assets.guid(o)})
    return result


def saves_in(assets, root):
    seen, stack, saves = set(), [(root, 0)], []
    while stack:
        o, depth = stack.pop()
        if o[0] in seen or depth > 8:
            continue
        seen.add(o[0])
        name = assets.name(o)
        if name.startswith("$ContextActionSavingThrow"):
            b, p = assets.data(o), assets.name_end(o)
            save_type, increases = struct.unpack("<ii", b[p:p + 8])
            saves.append({"element": name, "type": SAVE_TYPES.get(save_type, save_type), "conditionalDcIncreaseCount": increases})
        if o is root or name.startswith("$"):
            stack.extend((ref, depth + 1) for ref in assets.references(o))
    return sorted(saves, key=lambda s: s["element"])


def main():
    if len(sys.argv) != 2:
        raise SystemExit(__doc__)
    path = sys.argv[1]
    assets = Assets(path)
    units = []
    for guid, expected in UNITS.items():
        o = assets.find_guid(guid)
        if o is None:
            units.append({"guid": guid, "expectedName": expected, "found": False})
            continue
        refs = [r for r in owners(assets, o[0]) if r["blueprint"] not in ("BlueprintLibrary",)]
        units.append({"guid": guid, "name": assets.name(o), "referencedBy": refs})
    harms = []
    for guid, expected in HARMS.items():
        o = assets.find_guid(guid)
        harms.append({"guid": guid, "name": assets.name(o) if o else expected, "saves": saves_in(assets, o) if o else None})
    with open(path, "rb") as handle:
        digest = hashlib.sha256()
        for chunk in iter(lambda: handle.read(1 << 24), b""):
            digest.update(chunk)
    document = {
        "schema": "kmg.turn-undead.installed-references.v1",
        "source": {"file": "Kingmaker_Data/sharedassets1.assets", "sha256": digest.hexdigest(),
                   "reader": "docs/evidence/turn-undead/read_installed_blueprints.py"},
        "note": "Installed 2.1.7b blueprint references (names, GUIDs, counts only). 'via' lists the element types between "
                "the unit and the owning blueprint, e.g. a spawn action inside an ability's run action. BlueprintRoot "
                "and BlueprintLibrary registrations are omitted.",
        "units": units,
        "nativeHarmSaves": harms,
    }
    out = os.path.join(os.path.dirname(os.path.abspath(__file__)), "undead-identity-references.json")
    with open(out, "w", encoding="utf-8", newline="\n") as handle:
        json.dump(document, handle, indent=1)
        handle.write("\n")
    print("wrote", out)


if __name__ == "__main__":
    main()
