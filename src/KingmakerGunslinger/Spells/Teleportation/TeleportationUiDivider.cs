using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Modest, native-looking hairline rules owned by mod UI instances. Every
    // divider is created beneath an owning transform and disappears with it;
    // no shared prefab, localization asset, or native control is mutated.
    internal static class TeleportationUiDivider
    {
        private const float Height = 2f;
        private const float Alpha = 0.45f;
        // Restrained inset from both edges so the rule reads as parchment
        // decoration rather than a border.
        private const float SideMarginFraction = 0.12f;

        internal static GameObject CreateRule(RectTransform parent, string name, float localY, Color tone)
        {
            var rule = new GameObject(name, typeof(RectTransform));
            rule.transform.SetParent(parent, false);
            var rect = (RectTransform)rule.transform;
            float margin = Mathf.Max(4f, parent.rect.width * SideMarginFraction);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, localY);
            rect.offsetMin = new Vector2(margin, rect.offsetMin.y);
            rect.offsetMax = new Vector2(-margin, rect.offsetMax.y);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Height);
            var image = rule.AddComponent<Image>();
            image.color = new Color(tone.r, tone.g, tone.b, Alpha);
            image.raycastTarget = false;
            return rule;
        }

        // A separator that participates in a layout group as a slim row with a
        // centered hairline, used between destination-action groups.
        internal static GameObject CreateRowSeparator(Transform parent, string name, Color tone)
        {
            var separator = new GameObject(name, typeof(RectTransform));
            separator.transform.SetParent(parent, false);
            var rect = (RectTransform)separator.transform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            var element = separator.AddComponent<LayoutElement>();
            element.minHeight = 7f;
            element.preferredHeight = 7f;
            element.flexibleHeight = 0f;
            var rule = CreateRule(rect, name + ".Rule", 0f, tone);
            var ruleRect = (RectTransform)rule.transform;
            ruleRect.anchorMin = new Vector2(0f, 0.5f);
            ruleRect.anchorMax = new Vector2(1f, 0.5f);
            ((RectTransform)rule.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Height);
            return separator;
        }
    }
}
