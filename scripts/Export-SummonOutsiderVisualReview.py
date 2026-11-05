"""Render project-owned exported meshes and write a self-contained art review.

Run with Blender's bundled Python (numpy is used by the existing audit). The
before reference must be explicit. Outputs stay under ignored artifacts; no
native mesh, texture, save, screenshot or build package is copied into Git.
"""
import argparse
import base64
import hashlib
import html
import importlib.util
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
MODELS = [
    ('nalfeshnee', 'Nalfeshnee', 'sprint39-nalfeshnee', 'Heavy continuous trunk, boar face and tusks, broader hands and feet, closed wing membranes.', '2.10'),
    ('glabrezu', 'Glabrezu', 'sprint38-glabrezu', 'Supporting thorax, larger pincer jaws, forward separate smaller arms, connected skin.', '2.20'),
    ('vrock', 'Vrock', 'sprint37-vrock', 'Overlapping flattened feather vanes and covert row, connected thorax, clearer skin and beak separation.', '1.45'),
    ('babau', 'Babau', 'sprint36-demons', 'Lean continuous torso and limbs, rib and iliac relief, cheek planes and inset eyes.', '1.00'),
    ('hezrou', 'Hezrou', 'sprint36-demons', 'Broader high thorax and mandible, smaller eye forms, shallow mouth and teeth, varied olive hide.', '1.45'),
    ('barbed-devil', 'Barbed Devil', 'sprint35-infernal-reach', 'Connected anatomy with rooted shoulder, back and limb thorns and a visual tail.', '1.00'),
    ('bone-devil', 'Bone Devil', 'sprint35-infernal-reach', 'Ivory skull, ribcage and pelvis, bridged neck, thin wing plates and rooted stinger tail.', '1.45'),
    ('bearded-devil', 'Bearded Devil', 'sprint34-infernal-outsiders', 'Connected soldier anatomy, defined face and shorter jaw-rooted beard; native glaive retained.', '1.00'),
    ('kyton-chain-devil', 'Chain Devil', 'sprint34-infernal-outsiders', 'Fitted chain coverage, leather and wrist cuffs; four resting chains begin inside their cuffs.', '1.00'),
    ('lillend', 'Lillend Azata', 'sprint32-support', 'Fitted closed bodice and peplum, natural shoulder/arm/neck/face contours, blended fabric weights.', '1.00'),
]


def run(command):
    result = subprocess.run(command, cwd=ROOT, capture_output=True)
    if result.returncode:
        raise RuntimeError(result.stdout.decode(errors='replace') + result.stderr.decode(errors='replace'))
    return result.stdout


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--blender', required=True, type=Path)
    parser.add_argument('--before-ref', required=True)
    parser.add_argument('--output', type=Path, default=ROOT/'artifacts/summon-overhaul/review')
    parser.add_argument('--skip-render', action='store_true')
    args = parser.parse_args()
    args.output = args.output.resolve()
    if not args.output.is_relative_to(ROOT/'artifacts'):
        parser.error('Review output must stay below the ignored artifacts directory.')
    args.output.mkdir(parents=True, exist_ok=True)
    before_ref = run(['git', 'rev-parse', args.before_ref]).decode().strip()
    after_ref = run(['git', 'rev-parse', 'HEAD']).decode().strip()
    source_dirty = bool(run(['git', 'status', '--porcelain']).strip())
    spec = importlib.util.spec_from_file_location('mesh_audit', ROOT/'assets-source/original-models/continuous-bodies/audit_mesh_orientation.py')
    audit = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(audit)
    receipts, cards = [], []

    def picture(path, caption):
        data = base64.b64encode(path.read_bytes()).decode()
        return f'<figure><img alt="{html.escape(caption)}" src="data:image/png;base64,{data}"><figcaption>{html.escape(caption)}</figcaption></figure>'

    for key, title, family, change, scale in MODELS:
        images, receipt = [], {'key': key, 'change': change, 'viewMultiplier': scale}
        for state in ('before', 'after'):
            directory = args.output/state/key
            directory.mkdir(parents=True, exist_ok=True)
            for suffix in ('mesh.json', 'albedo.png'):
                relative = f'assets/{family}/{key}-{suffix}'
                destination = directory/f'{key}-{suffix}'
                destination.write_bytes(run(['git', 'show', f'{before_ref}:{relative}']) if state == 'before' else (ROOT/relative).read_bytes())
            mesh = directory/f'{key}-mesh.json'
            payload, components = audit.audit(mesh)
            receipt[state] = {'sha256': hashlib.sha256(mesh.read_bytes()).hexdigest(),
                'albedoSha256': payload['albedo']['sha256'], 'vertices': payload['vertexCount'],
                'triangles': payload['triangleCount'], 'components': len(components),
                'closedOutwardConsistent': all(c['signedVolume'] > 0 and c['boundaryEdges'] == 0 and c['sameDirectionEdges'] == 0 for c in components),
                'normalOpposesWinding': sum(c['normalOpposesWinding'] for c in components)}
            if not args.skip_render:
                run([str(args.blender.resolve()), '-b', '--factory-startup', '--python-exit-code', '1', '--python',
                     str(ROOT/'assets-source/original-models/continuous-bodies/render_mesh_data.py'), '--',
                     '--mesh', str(mesh), '--out-dir', str(directory), '--name', key,
                     '--up', 'y' if key == 'lillend' else 'z',
                     '--forward=+z' if key == 'lillend' else '--forward=+y',
                     '--views', 'front,party,side', '--size', '720'])
            if state == 'before':
                images.append(picture(directory/f'{key}-shipped-front.png', 'Before: v0.0.177 exported body'))
            else:
                images.append(picture(directory/f'{key}-shipped-front.png', 'Revised exported body: front'))
                images.append(picture(directory/f'{key}-shipped-party.png', 'Revised exported body: party-camera angle'))
        a,b=receipt['before'],receipt['after']
        cards.append(f'''<section class="creature"><p class="eyebrow">MODEL REVIEW</p><h2>{title}</h2><p>{html.escape(change)}</p>
            <div class="images">{''.join(images)}</div><p class="caption">Offline albedo previews under identical studio lighting, independently framed to each body. These images show shape and painting; they do not compare world-space size or reproduce Kingmaker materials.</p>
            <p>Triangles: {a['triangles']:,} → {b['triangles']:,}. View multiplier: {scale}. Native lighting, motion and owner approval: see the current checkpoint record.</p>
            <p class="identity">Revised mesh SHA-256: {b['sha256']}</p></section>''')
        receipts.append(receipt)
        print(f"{key}: {b['triangles']} triangles; outward={b['closedOutwardConsistent']}", flush=True)
    (args.output/'review-manifest.json').write_text(json.dumps({'beforeCommit':before_ref,'sourceHead':after_ref,'sourceDirty':source_dirty,'models':receipts},indent=2)+'\n')
    document=f'''<!doctype html><html lang="en"><meta charset="utf-8"><title>Summoned Creature Visual Overhaul</title><style>
        *{{box-sizing:border-box}} body{{font:16px/1.45 system-ui,sans-serif;margin:0;color:#17201f;background:#e9eeeb}} main{{max-width:1300px;margin:auto}}
        section{{background:white;padding:32px 40px;margin:20px 0;border-radius:12px}}h1{{font-size:42px;line-height:1.1}}h2{{font-size:30px;margin:4px 0 8px}}
        .eyebrow{{font-size:12px;letter-spacing:.15em;color:#45675d;font-weight:700}}.images{{display:grid;grid-template-columns:repeat(3,1fr);gap:12px}}figure{{margin:0}}img{{width:100%;height:auto;display:block;border-radius:6px}}figcaption,.caption{{font-size:12px;color:#52605c}}.identity{{font:11px monospace;overflow-wrap:anywhere}}
        .status{{padding:14px 18px;background:#fff1d6;border-left:4px solid #a56a12}}a{{color:#27624f}}@page{{size:A4 landscape;margin:10mm}}@media print{{body{{background:white;font-size:11px}}section{{margin:0;padding:10px 12px;border-radius:0;break-after:page}}h1{{font-size:30px}}h2{{font-size:22px}}.images img{{max-height:135mm;object-fit:contain}}.identity{{font-size:9px}}.caption{{font-size:10px}}}}</style><main>
        <section><p class="eyebrow">TABLETOP ADDED RULES · DEVELOPMENT REVIEW</p><h1>Summoned Creature<br>Visual Overhaul</h1><p>Ten revised original models for Howie's review.</p>
        <p class="status">Development review material. Owner aesthetic approval has not been granted. Wall of Ice's reported functional failure remains open until native formation, obstruction and cleanup are demonstrated.</p>
        <p>The following pages compare the genuine v0.0.177 exported assets with the revised assets. No concept illustration is presented as an in-game model. Exact source, validation, native results and remaining checks are recorded in planning/SUMMON-OUTSIDER-VISUAL-OVERHAUL-STATE.md.</p>
        <p>Invisible Stalker and Shadow Demon retain their exported bytes after offline inspection; their native invisible/revealed and shadow-material states remain a required review. Satyr's accepted source and runtime appearance paths are preserved.</p>
        <p>Four-arm and wing limits remain explicit: Glabrezu's smaller arms and Vrock/Nalfeshnee wings follow the chest; the native carrier has two arm chains. Visual appendages create no attacks.</p>
        <p class="identity">Before: {before_ref}<br>Review generation HEAD: {after_ref}<br>Uncommitted source changes: {source_dirty}</p>
        <p>Shape references: <a href="https://kurbanovarts.artstation.com/projects/Vyoa5n">Roman Kurbanov's WotR Vrock</a> (layered feather masses and bare head/neck separation); <a href="https://kurbanovarts.artstation.com/projects/qQLvxy">his WotR Babau production art</a> (lean silhouette and pronounced anatomical planes). The project meshes and pixels are original procedural work; third-party artwork is not embedded.</p></section>
        {''.join(cards)}<section><h2>Owner in-game checklist</h2><ol><li>Compare the revised group with a normal humanoid outdoors and indoors at ordinary camera distance and brightness.</li><li>Walk, turn and attack with Glabrezu, Vrock, Bone Devil, Chain Devil, Bearded Devil and Lillend. Check roots, grip, seams and contact.</li><li>Summon two Chain Devils, dismiss one and check that the other keeps its own chains. Rebuild a representative view and check size and materials.</li><li>Check Stalker's invisible, selected and legitimately revealed states; Shadow Demon's deliberate darkness; Satyr as the unchanged control.</li><li>Cast Wall of Ice into a legal empty lane. Check visible grounded wall, actual obstruction, 36 HP, 12-minute game-time duration and obstruction removal. Rejection feedback alone does not prove this defect fixed.</li></ol><p>Release, merge, further roster additions and owner aesthetic approval remain outside this development checkpoint.</p></section></main></html>'''
    (args.output/'Summoned-Creature-Visual-Review.html').write_text(document,encoding='utf-8')


if __name__ == '__main__':
    main()
