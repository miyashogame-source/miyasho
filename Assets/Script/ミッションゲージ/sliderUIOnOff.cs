using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GaugeManager : MonoBehaviour
{
    [Header("Gauge Root")]
    public RectTransform gaugeRoot;

    readonly List<PoseGaugeView> views = new();

    public void Register(PoseGaugeView view)
    {
        if (!views.Contains(view))
            views.Add(view);
    }

    public void Unregister(PoseGaugeView view)
    {
        views.Remove(view);
    }

    void LateUpdate()
    {
        Reorder();
    }

    void Reorder()
    {
        // ‡@ •\¦’†‚ÌƒQ[ƒW‚¾‚¯’Šo
        List<PoseGaugeView> visible = new();

        foreach (var v in views)
        {
            if (v == null) continue;
            if (v.IsVisible)
                visible.Add(v);
        }

        // ‡A •À‚Ñ‘Ö‚¦ƒ‹[ƒ‹
        // •Ï“®’† ¨ Å—Dæ
        // Ÿ‚ÉŒ»İ’l‚ª‚‚¢‡
        visible.Sort((a, b) =>
        {
            // IsChanging ‚ğÅ—Dæ
            int changingCompare =
                b.Model.IsChanging.CompareTo(a.Model.IsChanging);

            if (changingCompare != 0)
                return changingCompare;

            // Ÿ‚ÉŒ»İ’l
            return b.Model.CurrentValue.CompareTo(a.Model.CurrentValue);
        });

        // ‡B SiblingIndex ‚ğ”½‰f
        for (int i = 0; i < visible.Count; i++)
        {
            visible[i].transform.SetSiblingIndex(i);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(gaugeRoot);
    }
}
