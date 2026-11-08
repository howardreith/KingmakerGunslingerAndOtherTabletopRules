using System;
using System.Collections.Generic;
using System.Reflection;
using Kingmaker.Blueprints;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Repair R3 UI-01: every registered KMG summon unit blueprint gets its own
    /// runtime BlueprintPortrait made from its approved creature art. Only the
    /// KMG unit's private <c>m_Portrait</c> reference changes; the donor's
    /// shared BlueprintPortrait object, its sprites and every native unit are
    /// untouched. Units keep no unit-level portrait (UnitUISettings stays
    /// empty), so nothing new is written into saves: a loaded summon resolves
    /// its portrait from its blueprint again.
    /// </summary>
    internal static class ExpandedSummoningPortraitBuilder
    {
        private static readonly Dictionary<string, BlueprintPortrait> Portraits =
            new Dictionary<string, BlueprintPortrait>(StringComparer.Ordinal);

        internal static IDictionary<string, BlueprintPortrait> Created { get { return Portraits; } }

        internal static void Configure(IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            if (bySymbol == null) throw new ArgumentNullException("bySymbol");
            FieldInfo field = typeof(BlueprintUnit).GetField("m_Portrait",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null || field.FieldType != typeof(BlueprintPortrait))
                throw new MissingFieldException(typeof(BlueprintUnit).FullName, "m_Portrait");
            int[] crop = SummonPortraitPolicy.CropRect(SummonPortraitPolicy.IconSize);
            int assigned = 0;
            foreach (SummonCreatureSpec creature in SummonPortraitPolicy.PortraitCreatures())
            {
                string symbol = ExpandedSummoningIdentityCatalog.UnitSymbol(creature);
                BlueprintScriptableObject value;
                var unit = bySymbol.TryGetValue(symbol, out value) ? value as BlueprintUnit : null;
                if (unit == null)
                    throw new InvalidOperationException("Registered summon unit is missing: " + symbol + ".");
                BlueprintPortrait portrait;
                if (!Portraits.TryGetValue(creature.Key, out portrait) || portrait == null)
                {
                    Sprite art = ExpandedSummoningProjectIcons.Require(SummonPortraitPolicy.ArtKey(creature));
                    if (art.texture == null || art.texture.width != SummonPortraitPolicy.IconSize ||
                        art.texture.height != SummonPortraitPolicy.IconSize)
                        throw new InvalidOperationException("Summon portrait art has an unexpected texture: " + creature.Key + ".");
                    Sprite small = Sprite.Create(art.texture, new Rect(crop[0], crop[1], crop[2], crop[3]),
                        new Vector2(0.5f, 0.5f), 100f);
                    small.name = SummonPortraitPolicy.PortraitName(creature.Key);
                    small.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    portrait = ScriptableObject.CreateInstance<BlueprintPortrait>();
                    portrait.name = SummonPortraitPolicy.PortraitName(creature.Key);
                    portrait.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    portrait.Data = new PortraitData(null, small, small, small);
                    Portraits[creature.Key] = portrait;
                }
                field.SetValue(unit, portrait);
                assigned++;
            }
            if (assigned != ExpandedSummoningIdentityCatalog.UnitCount)
                throw new InvalidOperationException("Summon portraits must cover every registered summon unit: " +
                    assigned + " of " + ExpandedSummoningIdentityCatalog.UnitCount + ".");
        }
    }
}
