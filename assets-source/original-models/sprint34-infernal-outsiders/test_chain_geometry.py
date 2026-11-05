"""Focused regression for the actual ellipse construction and exported links."""
import argparse
import base64
import json
import math
from pathlib import Path
import struct
import unittest

from chain_geometry import perpendicular_axes


class LinkFrameTests(unittest.TestCase):
    def test_vertical_and_near_vertical_openings(self):
        for direction in ((0,0,-1),(.02,.06,-.998),(.4,.1,-.91),(0,1,0),(1,0,0)):
            for minor in ((0,0,1),(1,0,0),(0,1,0)):
                a,b=perpendicular_axes(direction,minor)
                self.assertAlmostEqual(sum(x*y for x,y in zip(a,b)),0,places=12)
                self.assertAlmostEqual(sum(x*x for x in a),1,places=12)
                self.assertAlmostEqual(sum(x*x for x in b),1,places=12)
                # The exact centreline ellipse used by the authoring function
                # must retain .027 minor radius, leaving a real .017 opening.
                points=[tuple(a[k]*.045*math.cos(t)+b[k]*.027*math.sin(t) for k in range(3))
                        for t in (i*math.tau/12 for i in range(12))]
                half=(max(sum(p[k]*b[k] for k in range(3)) for p in points)-
                      min(sum(p[k]*b[k] for k in range(3)) for p in points))/2
                self.assertAlmostEqual(half,.027,places=12)
                self.assertGreater(half-.010,.010)

    def test_bad_tangent_refuses(self):
        for direction in ((0,0,0),(float('nan'),0,1),(float('inf'),0,1)):
            with self.assertRaises(ValueError):
                perpendicular_axes(direction,(0,0,1))

    def test_exported_hanging_link_openings_and_owner_regions(self):
        p=Path(__file__).resolve().parents[3]/'assets/sprint34-infernal-outsiders/kyton-chain-devil-mesh.json'
        payload=json.loads(p.read_text())
        blob=base64.b64decode(payload['data'])
        positions=[struct.unpack_from('<3f',blob,12*i) for i in range(payload['vertexCount'])]
        owners=set()
        self.assertEqual(len(payload['chains']),4)
        for chain in payload['chains']:
            region=set(chain['vertices'])
            self.assertFalse(region & owners)
            owners |= region
            for link in chain['links']:
                ids=link['vertices']
                self.assertTrue(set(ids) <= region)
                a,b=link['basisA'],link['basisB']
                self.assertAlmostEqual(sum(x*y for x,y in zip(a,b)),0,places=6)
                # Actual authored tube vertices, not metadata feature counts:
                # the two ellipse ends must enclose the intended tube and hole.
                center=link['center']
                projected=[sum((positions[i][k]-center[k])*b[k] for k in range(3)) for i in ids]
                outer=(max(projected)-min(projected))/2
                self.assertGreater(outer, .027)
                self.assertLess(outer, .039)
                self.assertGreater(link['minorRadius']-link['tubeRadius'],link['tubeRadius'])


if __name__ == '__main__':
    unittest.main()
