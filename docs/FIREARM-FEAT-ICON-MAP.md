# Firearm feat icon map

The supported firearm selector set is exactly Pistol, Musket, and
Blunderbuss. Each uses original vector-path lettering on the same opaque,
full-square burgundy/brown gradient and gold-frame system. The same three
sprites are centralized across Rapid Reload, Weapon Focus, Greater Weapon
Focus, Weapon Specialization, Greater Weapon Specialization, Improved
Critical, and Gun Training. Rifle and Revolver retain hidden blueprint
identities only for old-save/Toy Box tolerance and have no selector textures.

Rapid Reload retains the circular reload arrow plus tool on transparency. The
motif is larger, uses muted `#A6533F`, and has no blue corner glyphs or inset
card, matching the silhouette treatment of neighboring vanilla feat icons.

| Firearm/purpose | Sprite key | Symbol | SHA-256 |
|---|---|---|---|
| Pistol | `firearm-monogram-pistol` | P | `5343D062083ADA98BF0AABDFC0EB3D538C0C8B9FD9CFBAAABBAB2C8CC3A0DF0D` |
| Musket | `firearm-monogram-musket` | M | `675D291D8EA7FC7955AB6468D9134A09619B727A1C789357D6FBD4A1485AA848` |
| Blunderbuss | `firearm-monogram-blunderbuss` | B | `08E4E9061CA76B26B778804DA4436446382EC725F43096416FF0EAE3B9BED4A9` |
| Rapid Reload | `rapid-reload` | reload arrow and tool | `EFAB95075AD8AF61FE10425090015A75432B74113FBC34EBC185969E1E82B321` |

Before the overhaul, every firearm choice used a pale inset card and Rapid
Reload used an undersized off-palette emblem with blue corners. The current
source is rendered at 512 px and downsampled once to 64 px. The deterministic
64/32 px inspection map is
`assets-source/original-icons/firearm-feats/firearm-feat-icon-map.png`,
SHA-256 `5C20F7C3092EA3889269C26F2C046C0B3E266600F26BD6B93709093F002F4F4C`.
The map is supporting evidence; guarded in-game screenshots are the visual
acceptance authority.
