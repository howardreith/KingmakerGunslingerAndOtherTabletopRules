"""Build a local-only art review from canonical records; never generate or alter pixels."""
import hashlib
import html
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CATALOG = ROOT / "assets-source/original-icons/icon-catalog.json"

def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))

def main():
    catalog = read(CATALOG)
    concepts = {c["key"]: c for c in catalog["concepts"]}
    cards = []
    for family in ("pilot", "production"):
        manifest = ROOT / f"assets-source/original-icons/icon-overhaul-v2/{family}/{family}-manifest.json"
        for record in read(manifest)["records"]:
            for path_field, hash_field in (("source", "sourceSha256"), ("export", "exportSha256")):
                if hashlib.sha256((ROOT / record[path_field]).read_bytes()).hexdigest() != record[hash_field]:
                    raise ValueError("Changed pixels: " + record["key"])
            concept = concepts[record["key"]]
            brief = read(ROOT / record["brief"]) if record.get("brief") else {}
            cards.append({
                "key": record["key"], "name": concept["name"],
                "group": concept["reviewGroup"], "source": "../../" + record["source"],
                "export": "../../" + record["export"], "hash": record["exportSha256"],
                "exportSize": record["exportSize"][0],
                "brief": "../../" + record["brief"] if record.get("brief") else "PILOT-APPROVAL.md",
                "behavior": brief.get("behavior", "Pilot image and family direction approved; see the exact decision and scope."),
                "approved": concept["visualReview"]["status"] == "approved",
                "confused": brief.get("confusedWith", []),
                "surfaces": brief.get("uiSurfaces", sorted({c["surface"] for c in catalog["consumers"] if c["concept"] == record["key"]}))
            })
    groups = list(dict.fromkeys(c.get("reviewGroup") for c in catalog["concepts"] if c.get("reviewGroup")))
    cards.sort(key=lambda c: (groups.index(c["group"]), c["name"]))
    data = json.dumps(cards, ensure_ascii=False).replace("<", "\\u003c")
    options = "".join('<option value="'+html.escape(g)+'">'+html.escape(g.replace("-", " ").title())+"</option>" for g in groups)
    page = TEMPLATE.replace("__GROUP_OPTIONS__", options).replace("__DATA__", data)
    destination = ROOT / "reports/icon-overhaul/PRODUCTION-REVIEW.html"
    destination.write_text(page, encoding="utf-8", newline="\n")
    print(f"Built {destination.relative_to(ROOT)} with {len(cards)} exact art records.")

TEMPLATE = r"""<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Kingmaker icon collection — production review</title>
<style>
:root { color-scheme:dark; --bg:#151b1f; --panel:#20292f; --text:#f2e6ce; --muted:#b8bab3; --line:#47525b; --size:64px; --tile:#0b1014; }
* { box-sizing:border-box; } body { margin:0; background:var(--bg); color:var(--text); font:16px/1.5 "Segoe UI",sans-serif; }
body.paper { --bg:#e5d5b5; --panel:#f1e5cb; --text:#2b2925; --muted:#615b50; --line:#b9a98c; --tile:#d6c6a6; color-scheme:light; }
header,main { max-width:1500px; margin:auto; padding:24px; }
header { padding-bottom:12px; } h1 { font-size:30px; margin:0 0 10px; font-weight:600; }
h2 { font-size:17px; margin:10px 0 3px; } p { margin:8px 0; } a { color:inherit; text-underline-offset:3px; }
.note { color:var(--muted); max-width:1100px; } nav { display:flex; gap:20px; flex-wrap:wrap; margin-top:16px; }
.controls { position:sticky; top:0; z-index:2; display:flex; align-items:center; flex-wrap:wrap; gap:12px; background:var(--bg); border-block:1px solid var(--line); padding:16px 24px; }
label { display:flex; align-items:center; gap:7px; } input,select,button { font:inherit; }
input[type=search],select,button { border:1px solid var(--line); border-radius:5px; padding:7px 10px; background:var(--panel); color:var(--text); }
button { cursor:pointer; } button:hover,button:focus-visible { outline:2px solid #b99856; outline-offset:2px; }
#search { width:230px; } #count { margin-left:auto; color:var(--muted); }
.grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(255px,1fr)); gap:18px; }
.card { min-width:0; padding:18px; border:1px solid var(--line); border-radius:7px; background:var(--panel); }
.preview { min-height:150px; display:flex; align-items:center; justify-content:center; background:var(--tile); border:0; width:100%; }
.preview img { width:var(--size); height:var(--size); object-fit:contain; image-rendering:auto; }
.gray .preview img,.gray #original { filter:grayscale(1); }
.group,.state { font-size:12px; color:var(--muted); } .state { display:inline-block; margin-top:7px; border:1px solid var(--line); padding:2px 7px; border-radius:12px; }
.links { display:flex; gap:13px; font-size:13px; flex-wrap:wrap; margin-top:12px; } details { margin-top:10px; font-size:13px; }
summary { cursor:pointer; } code { font-size:11px; overflow-wrap:anywhere; } .behavior { min-height:4.5em; font-size:14px; }
dialog { background:var(--panel); color:var(--text); border:1px solid var(--line); border-radius:8px; width:min(1050px,95vw); max-height:94vh; padding:24px; }
dialog::backdrop { background:#000b; } #original { display:block; width:auto; height:auto; max-width:100%; max-height:64vh; margin:15px auto; }
.dialogtop { display:flex; align-items:center; justify-content:space-between; gap:15px; } #dialogtitle { margin:0; font-size:22px; }
.empty { padding:40px; color:var(--muted); } @media(max-width:600px) { header,main { padding:16px; } .controls { position:static; padding:12px 16px; } .grid { grid-template-columns:1fr; } #count { margin-left:0; } }
</style></head><body>
<header>
<h1>Kingmaker icon collection</h1>
<p>89 painted identities and the Rapid Reload emblem, preserved as individual originals and reproducible exports.</p>
<p class="note"><strong>Art inspection only.</strong> The ten pilot images and family direction are approved. The 80 production images await final owner review. Integration and native game-screen qualification are still pending. P/M/B use the native lettering route and are not represented by generated lettering here.</p>
<nav><a href="PILOT-APPROVAL.md">Pilot approval</a><a href="../../docs/ICON-ART-GUIDE.md">Art guide</a><a href="../../assets-source/original-icons/icon-catalog.json">Canonical catalog</a><a href="IMPLEMENTATION-REPORT.md">Evidence and limits</a></nav>
</header>
<div class="controls" aria-label="Review controls">
<input id="search" type="search" placeholder="Search name or behavior" aria-label="Search icons">
<label>Family <select id="group"><option value="">All families</option>__GROUP_OPTIONS__</select></label>
<label>Review <select id="status"><option value="">All images</option><option value="new">Production pending</option><option value="approved">Pilot approved</option></select></label>
<label>Size <input id="size" type="range" min="32" max="128" step="8" value="64"><output id="sizevalue">64 px</output></label>
<label><input id="gray" type="checkbox">Grayscale</label>
<label><input id="paper" type="checkbox">Parchment</label>
<span id="count" role="status"></span>
</div>
<main><div id="grid" class="grid"></div><p id="empty" class="empty" hidden>No matching icons.</p></main>
<dialog id="detail"><div class="dialogtop"><h2 id="dialogtitle"></h2><button id="close">Close</button></div><p id="dialogbehavior"></p><img id="original" alt=""><p class="note">Preserved high-resolution source. Use the gallery's size control to judge the actual exported pixels.</p><p><a id="sourceLink">Open original</a> · <a id="exportLink">Open runtime-size export</a> · <a id="briefLink">Read brief / approval</a></p><code id="dialoghash"></code></dialog>
<script>
const icons=__DATA__;
const $=id=>document.getElementById(id);
function node(tag,text,cls){const n=document.createElement(tag);if(text!==undefined)n.textContent=text;if(cls)n.className=cls;return n;}
function link(text,url){const n=node("a",text);n.href=url;return n;}
function inspect(icon){
 $("dialogtitle").textContent=icon.name;$("dialogbehavior").textContent=icon.behavior;
 $("original").src=icon.source;$("original").alt=icon.name+" original";
 $("sourceLink").href=icon.source;$("exportLink").href=icon.export;$("briefLink").href=icon.brief;
 $("dialoghash").textContent="Export SHA-256: "+icon.hash;$("detail").showModal();
}
function render(){
 const query=$("search").value.trim().toLowerCase(),group=$("group").value,status=$("status").value;
 const selected=icons.filter(i=>(!group||i.group===group)&&(!status||(status==="approved")===i.approved)&&
  (!query||(i.name+" "+i.key+" "+i.behavior).toLowerCase().includes(query)));
 const fragment=document.createDocumentFragment();
 for(const icon of selected){
  const card=node("article",undefined,"card"),button=node("button",undefined,"preview"),img=node("img");
  img.src=icon.export;img.alt=icon.name;img.loading="lazy";button.append(img);button.setAttribute("aria-label","Inspect "+icon.name);button.onclick=()=>inspect(icon);
  card.append(button,node("h2",icon.name),node("div",icon.group.replaceAll("-"," "),"group"),
   node("span",icon.approved?"Pilot image approved":"Awaiting final review","state"),node("p",icon.behavior,"behavior"));
  const links=node("div",undefined,"links");links.append(link("Source",icon.source),link(icon.exportSize+" px export",icon.export),link("Brief",icon.brief));card.append(links);
  const details=node("details");details.append(node("summary","Identity and comparisons"),node("p",icon.key),node("code","SHA-256: "+icon.hash),
   node("p","UI: "+icon.surfaces.join(" · ")),node("p","Compare: "+icon.confused.join(", ")));card.append(details);
  fragment.append(card);
 }
 $("grid").replaceChildren(fragment);$("count").textContent=selected.length+" of "+icons.length+" images";$("empty").hidden=selected.length>0;
}
for(const id of ["search","group","status"])$(id).addEventListener("input",render);
$("size").addEventListener("input",()=>{document.documentElement.style.setProperty("--size",$("size").value+"px");$("sizevalue").textContent=$("size").value+" px";});
$("gray").onchange=()=>document.body.classList.toggle("gray",$("gray").checked);
$("paper").onchange=()=>document.body.classList.toggle("paper",$("paper").checked);
$("close").onclick=()=>$("detail").close();
$("detail").addEventListener("click",e=>{if(e.target===$("detail")){const r=$("detail").getBoundingClientRect();if(e.clientX<r.left||e.clientX>r.right||e.clientY<r.top||e.clientY>r.bottom)$("detail").close();}});
render();
</script></body></html>
"""

if __name__ == "__main__":
    main()
