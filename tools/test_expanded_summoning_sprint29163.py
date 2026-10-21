"""Small refusal checks for the Sprint 29 public source boundary."""
import unittest

from validate_expanded_summoning_sprint29163 import check_scope


class Sprint29PublicBoundaryTests(unittest.TestCase):
    def setUp(self):
        self.released = [("released-giant", None, False, 6)]
        self.current = self.released + [
            ("ettin", None, False, 5),
            ("cloud-giant", None, False, 8),
            ("storm-giant", None, False, 9),
        ]

    def test_exact_public_append_is_accepted(self):
        check_scope(self.released, self.current, set())

    def test_any_new_suppression_is_refused(self):
        with self.assertRaisesRegex(AssertionError, "withholds"):
            check_scope(self.released, self.current, {"storm-giant"})

    def test_missing_or_reinterpreted_root_is_refused(self):
        for changed in (self.current[:-1],
                        self.current[:-1] + [("storm-giant", 9, False, 9)],
                        self.current[:-1] + [("storm-giant", None, True, 9)]):
            with self.assertRaises(AssertionError):
                check_scope(self.released, changed, set())

    def test_released_row_reordering_is_refused(self):
        with self.assertRaisesRegex(AssertionError, "Released"):
            check_scope(self.released, [("changed", None, False, 6)] +
                        self.current[1:], set())


if __name__ == "__main__":
    unittest.main()
