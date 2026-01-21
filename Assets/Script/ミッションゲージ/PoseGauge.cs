using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
public class PoseGaugeUI : MonoBehaviour
{
    [Header("Pose Setting")]
    public PoseType poseType;

    [Header("Gauge Setting")]
    public float maxValue = 100f;
    public float increaseSpeed = 30f;
    public float decreaseSpeed = 0f;

    [Header("Fade Setting")]
    public float fadeOutDelay = 1.5f;
    public float fadeOutSpeed = 3f;

    [Header("UI")]
    public Image fillImage;
    public CanvasGroup canvasGroup;

    Animator animator;
    LayoutElement layout;

    float currentValue;
    float lastValue;
    float fadeTimer;

    float defaultHeight;

    void Start()
    {
        animator = GameObject.Find("PlayerAnimator")?.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("PoseGaugeUI: Animator not found");
            enabled = false;
            return;
        }

        layout = GetComponent<LayoutElement>();
        defaultHeight = layout.preferredHeight > 0 ? layout.preferredHeight : 30f;

        currentValue = 0f;
        canvasGroup.alpha = 0f;
        layout.preferredHeight = 0f;
    }

    void Update()
    {
        bool isPoseActive = IsPoseActive();

        lastValue = currentValue;

        if (isPoseActive)
        {
            currentValue += increaseSpeed * Time.deltaTime;
            fadeTimer = 0f;
        }
        else
        {
            currentValue -= decreaseSpeed * Time.deltaTime;
            fadeTimer += Time.deltaTime;
        }

        currentValue = Mathf.Clamp(currentValue, 0f, maxValue);
        fillImage.fillAmount = currentValue / maxValue;

        bool isChanging = !Mathf.Approximately(currentValue, lastValue);

        if (isPoseActive || isChanging)
        {
            // 表示
            canvasGroup.alpha = 1f;
            layout.preferredHeight = defaultHeight;
            fadeTimer = 0f;
        }
        else if (fadeTimer > fadeOutDelay)
        {
            // フェードアウト
            canvasGroup.alpha = Mathf.MoveTowards(
                canvasGroup.alpha,
                0f,
                fadeOutSpeed * Time.deltaTime
            );

            if (canvasGroup.alpha <= 0.01f)
            {
                // 完全非表示 → 高さ0
                layout.preferredHeight = 0f;
            }
        }
    }

    bool IsPoseActive()
    {
        switch (poseType)
        {
            case PoseType.Nowpose1: return animator.GetBool("Nowpose1");
            case PoseType.Nowpose2: return animator.GetBool("Nowpose2");
            case PoseType.Nowpose3: return animator.GetBool("Nowpose3");
            case PoseType.Nowpose4: return animator.GetBool("Nowpose4");
            case PoseType.Nowpose5: return animator.GetBool("Nowpose5");
        }
        return false;
    }
}
