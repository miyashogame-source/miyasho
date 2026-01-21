using UnityEngine;

public class PoseGaugeModel : MonoBehaviour
{
    [Header("Pose Setting")]
    public PoseType poseType;

    [Header("Gauge Setting")]
    public float maxValue = 100f;
    public float increaseSpeed = 30f;
    public float decreaseSpeed = 0f;

    [Header("Debug")]
    public bool debugAlwaysActive = false;

    Animator animator;

    public float CurrentValue { get; private set; }
    public float NormalizedValue => maxValue > 0f ? CurrentValue / maxValue : 0f;
    public bool IsActive { get; private set; }
    public bool IsChanging { get; private set; }

    float lastValue;

    // ===== Pose判定安定化 =====
    bool cachedPoseActive;
    float poseCheckTimer;
    const float POSE_CHECK_INTERVAL = 0.05f; // 約20fps

    void Awake()
    {
        GameObject animatorObj = GameObject.Find("PlayerAnimator");
        if (animatorObj != null)
        {
            animator = animatorObj.GetComponent<Animator>();
        }

        // 初期値を一度だけ確定させる（★重要）
        cachedPoseActive = debugAlwaysActive || IsPoseActive();
        IsActive = cachedPoseActive;
    }

    void Update()
    {
        // ===== Pose判定（一定間隔のみ）=====
        poseCheckTimer += Time.deltaTime;
        if (poseCheckTimer >= POSE_CHECK_INTERVAL)
        {
            poseCheckTimer = 0f;

            if (debugAlwaysActive)
            {
                cachedPoseActive = true;
            }
            else
            {
                cachedPoseActive = IsPoseActive();
            }
        }

        IsActive = cachedPoseActive;

        // ===== 値更新 =====
        lastValue = CurrentValue;

        if (IsActive)
        {
            CurrentValue += increaseSpeed * Time.deltaTime;
        }
        else
        {
            CurrentValue -= decreaseSpeed * Time.deltaTime;
        }

        CurrentValue = Mathf.Clamp(CurrentValue, 0f, maxValue);

        // ===== 変動判定 =====
        IsChanging = Mathf.Abs(CurrentValue - lastValue) > 0.001f;
    }

    bool IsPoseActive()
    {
        if (animator == null) return false;

        switch (poseType)
        {
            case PoseType.Nowpose1:
                return animator.GetBool("Nowpose1");
            case PoseType.Nowpose2:
                return animator.GetBool("Nowpose2");
            case PoseType.Nowpose3:
                return animator.GetBool("Nowpose3");
            case PoseType.Nowpose4:
                return animator.GetBool("Nowpose4");
            case PoseType.Nowpose5:
                return animator.GetBool("Nowpose5");
            default:
                return false;
        }
    }
}
