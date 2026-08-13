using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Summoning
{
    internal static class ExpandedSummoningAlignmentModeRuntime
    {
        private static BlueprintFeature _feature;
        private static ModContext _context;
        private static float _elapsed;

        internal static void Configure(BlueprintFeature feature)
        {
            _feature = feature ?? throw new ArgumentNullException("feature");
        }

        internal static void Attach(ModContext context)
        {
            _context = context ?? throw new ArgumentNullException("context");
            context.ModEntry.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(
            UnityModManagerNet.UnityModManager.ModEntry ignored, float deltaTime)
        {
            _elapsed += deltaTime;
            if (_elapsed < 0.5f) return;
            _elapsed = 0f;
            try
            {
                if (_feature == null || _context == null ||
                    !_context.FeatureModules.Active.ExpandedSummoning ||
                    Game.Instance == null || Game.Instance.Player == null) return;
                foreach (var unit in Game.Instance.Player.Party.Where(value =>
                    value != null && value.Descriptor != null))
                    if (!unit.Descriptor.HasFact(_feature))
                        unit.Descriptor.AddFact(_feature);
            }
            catch (Exception exception)
            {
                _context.Logger.Warning("expanded-summoning",
                    "alignment-mode.grant.failed", exception.GetType().Name +
                    ": " + exception.Message);
            }
        }
    }
}
