# Turn Undead mission intake

The files in this directory, apart from this note and `.gitattributes`, are the
owner-supplied mission packet copied byte for byte. The owner asked Claude on
2026-11-27 to open the packet and execute its instructions.

| Field | Value |
| --- | --- |
| Packet archive | `Turn_Undead_Claude_Mission_2026-11-27.zip` (owner lab root), 3,544,723 bytes |
| Archive SHA-256 | `ebcfff9cb676eb7359f147b185c87de70574c79f791ca8fd92304c6191998824` |
| Integrity check | `sha256sum -c SHA256SUMS.txt` in this directory: all 17 entries OK at intake |
| Entry point | [START_HERE.txt](START_HERE.txt), then [MISSION.md](MISSION.md) |
| Target base | `master` at `52755b617c79328a972ae5b74bc05784422e332d`, the packet's research baseline |
| Branch | `codex/turn-undead` |

`.gitattributes` keeps the two `.txt` files LF in every checkout, so the
recorded checksums hold. The packet's runtime icons and masters live here as
mission inputs. Production copies go in the established icon source and export
trees. The mod package does not ship them from this directory.

The implementation record that replaces the packet's template is kept in
[IMPLEMENTATION-RECORD.md](IMPLEMENTATION-RECORD.md) once work begins.
