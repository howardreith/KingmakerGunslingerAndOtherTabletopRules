"""Focused original Lillend export checks; no native claims."""
import argparse, json, math, sys, unittest
from pathlib import Path
import bmesh
sys.dont_write_bytecode=True
sys.path.insert(0,str(Path(__file__).resolve().parent))
import generate_lillend as model
CAPTURE=None
class LillendExportTests(unittest.TestCase):
    def test_geometry_weights_and_uvs(self):
        _,rig=model.decode_rig(json.loads(Path(CAPTURE).read_text(encoding="utf-8")))
        bm=bmesh.new();weights={};uvs={}
        try:
            model.build_body(bm,weights,uvs,rig,"lillend-azata")
            self.assertGreater(len(bm.faces),0)
            self.assertTrue(all(face.calc_area()>1e-10 for face in bm.faces))
            self.assertEqual(set(bm.verts),set(weights))
            self.assertEqual(set(bm.verts),set(uvs))
            for row in weights.values():
                self.assertTrue(0<len(row)<=4)
                self.assertAlmostEqual(sum(w for _,w in row),1,places=6)
                self.assertTrue(all(n in model.BONES and 0<w<=1 for n,w in row))
            self.assertEqual(set(model.BONES),{n for row in weights.values() for n,_ in row})
            self.assertTrue(all(math.isfinite(x) for v in bm.verts for x in v.co))
            self.assertTrue(all(0<=x<=1 for uv in uvs.values() for x in uv))
            self.assertGreater(min(v.co.y for v in bm.verts),0)
        finally: bm.free()
    def test_tail_motion_has_fixed_length_and_restores(self):
        for step in range(141):
            points=model.tail_pose(step*model.DURATION/140)
            self.assertTrue(all(math.isfinite(x) for p in points for x in p))
            for i in range(len(points)-1):
                expected=(model.Vector(model.REST[i+1])-model.Vector(model.REST[i])).length
                self.assertAlmostEqual((points[i+1]-points[i]).length,expected,places=5)
        for t in (0,model.DURATION):
            self.assertTrue(all((p-model.Vector(rest)).length<1e-5 for p,rest in zip(model.tail_pose(t),model.REST)))
    def test_foreign_body_rejected(self):
        _,rig=model.decode_rig(json.loads(Path(CAPTURE).read_text(encoding="utf-8")))
        bm=bmesh.new()
        try:
            with self.assertRaises(ValueError):model.build_body(bm,{}, {},rig,"salamander")
            self.assertEqual(len(bm.verts),0)
        finally:bm.free()
if __name__=="__main__":
    parser=argparse.ArgumentParser();parser.add_argument("--capture",required=True)
    args=parser.parse_args(sys.argv[sys.argv.index("--")+1:]);CAPTURE=args.capture
    result=unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(LillendExportTests))
    if not result.wasSuccessful() or result.testsRun==0:raise RuntimeError("Lillend export checks failed or empty")
