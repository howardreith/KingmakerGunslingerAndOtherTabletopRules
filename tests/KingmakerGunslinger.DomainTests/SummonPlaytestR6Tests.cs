using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SummonPlaytestR6Tests
    {
        private static string Source(string relative)
        {
            return File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "src", "KingmakerGunslinger", relative));
        }

        private static int Count(string text, string value)
        {
            return text.Split(new[] { value }, StringSplitOptions.None).Length - 1;
        }

        public static void AidCastStyleUsesAPlayableClip()
        {
            Assertions.Equal("Omni", SummonPlaytestR6Policy.ChooseCastStyle("Omni", new[] { "Omni", "Point" }), "A playable current style is kept.");
            Assertions.Equal("Omni", SummonPlaytestR6Policy.ChooseCastStyle("Touch", new[] { "Point", "Omni" }), "A style with no clip falls back by preference.");
            Assertions.Equal("Point", SummonPlaytestR6Policy.ChooseCastStyle("Touch", new[] { "Point" }), "The only playable style is used.");
            Assertions.Equal(null, SummonPlaytestR6Policy.ChooseCastStyle("Touch", new string[0]), "No playable clip gives no style.");
            Assertions.Equal(null, SummonPlaytestR6Policy.ChooseCastStyle("Immediate", new[] { "Immediate" }), "Immediate skips the animation, so it is never chosen.");
        }

        // The hook is the risk: a per-target action or precast hook would
        // repeat the cue or sound on a cancelled cast.
        public static void CastCuesFireOncePerExecutionOnExactlyTwoAbilities()
        {
            string cue = Source(Path.Combine("Summoning", "SummonAbilityCastCue.cs"));
            Assertions.True(cue.Contains("IAbilityOnCastLogic"), "The cue uses the native once-per-execution cast hook.");
            Assertions.False(cue.Contains("IAbilityOnSpawnFx") || cue.Contains("ContextAction"), "The cue is not a per-target action.");
            Assertions.True(cue.Contains("Contexts.TryGetValue(context, out seen)"), "A repeated hook for one execution posts nothing.");
            Assertions.True(cue.Contains("caster.View.gameObject"), "The cue is posted on the caster's view, so it ends with that view.");
            Assertions.True(cue.Contains("catch (Exception error)"), "A sound failure cannot interrupt the ability.");
            string vrock = Source(Path.Combine("Blueprints", "ExpandedSummoningSpecialBuilder.Vrock37.cs"));
            string trumpet = Source(Path.Combine("Blueprints", "ExpandedSummoningSpecialBuilder.Trumpet31Blast.cs"));
            Assertions.Equal(1, Count(vrock, "SummonAbilityCastCue"), "Only the Vrock screech carries a cue.");
            Assertions.Equal(1, Count(trumpet, "SummonAbilityCastCue"), "Only the Trumpet blast carries a cue.");
            Assertions.True(vrock.Contains("SummonPlaytestR6Policy.VrockScreechCue") && trumpet.Contains("SummonPlaytestR6Policy.TrumpetBlastCue"), "Each ability names its own cue.");
        }

        public static void LanternDonorChangeIsPerViewAndReversible()
        {
            string orb = Source(Path.Combine("Summoning", "LanternArchonOrbView.cs"));
            Assertions.True(orb.Contains("_donorBodies[index].sharedMesh = hidden ? _emptyMeshes[index] : _donorMeshes[index]"), "The swap is on this view's renderer and can be undone.");
            Assertions.False(orb.Contains(".enabled = false;\n            foreach") && orb.Contains("value.enabled = false"), "Donor renderers are not disabled; their native fading state stays authoritative.");
            int release = orb.IndexOf("internal void Release()", StringComparison.Ordinal);
            Assertions.True(release > 0 && orb.IndexOf("SetDonorBodiesHidden(false)", release, StringComparison.Ordinal) > release, "Release restores the donor mesh.");
            Assertions.True(orb.IndexOf("DestroyImmediate(empty)", release, StringComparison.Ordinal) > release, "Release destroys the private empty meshes.");
        }
    }
}
