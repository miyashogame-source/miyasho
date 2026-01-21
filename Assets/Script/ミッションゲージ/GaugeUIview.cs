using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
public class PoseGaugeView : MonoBehaviour
{
    [Header("Fade Setting")]
    public float fadeOutDelay = 1.5f;
    public float fadeOutSpeed = 3f;

    [Header("UI")]
    public Image fillImage;
    public CanvasGroup canvasGroup;

    [Header("Model")]
    public PoseGaugeModel model;

    LayoutElement layout;
    float fadeTimer;
    float defaultHeight;

    // ===== 追加：GaugeManager 用 =====
    public PoseGaugeModel Model => model;

    public bool IsVisible =>
        layout != null &&
        layout.preferredHeight > 0f &&
        canvasGroup.alpha > 0.01f;
    // =================================

    void Start()
    {
        layout = GetComponent<LayoutElement>();

        defaultHeight = layout.preferredHeight > 0
            ? layout.preferredHeight
            : 30f;

        canvasGroup.alpha = 0f;
        layout.preferredHeight = 0f;
    }

    void Update()
    {
        if (model == null) return;

        fillImage.fillAmount = model.NormalizedValue;

        if (model.IsActive || model.IsChanging)
            Show();
        else
            FadeOut();
    }

    void Show()
    {
        canvasGroup.alpha = 1f;
        layout.preferredHeight = defaultHeight;
        fadeTimer = 0f;
    }

    void FadeOut()
    {
        fadeTimer += Time.deltaTime;
        if (fadeTimer < fadeOutDelay) return;

        canvasGroup.alpha = Mathf.MoveTowards(
            canvasGroup.alpha,
            0f,
            fadeOutSpeed * Time.deltaTime
        );

        if (canvasGroup.alpha <= 0.01f)
            layout.preferredHeight = 0f;
    }
}
