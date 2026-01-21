using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("Vision (Cone)")]
    public float viewDistance = 10f;
    public float normalViewAngle = 70f;     // 通常の視野角（全角）
    public float alertedViewAngle = 140f;   // 発見時の視野角（全角）
    public float viewAngleChangeSpeed = 1.5f;

    [Header("Forward Hemisphere Vision (Near)")]
    public float hemisphereRadius = 4f;

    [Header("Eye / Origin")]
    [Tooltip("視界判定の基準Transform。未指定ならこのtransform（+eyeHeight）")]
    public Transform eyes;

    [Tooltip("目線の高さ（eyes未指定時のみ使用）")]
    public float eyeHeight = 0.2f;

    [Tooltip("プレイヤーを見る高さ（胴体あたり）")]
    public float playerCenterHeight = 1.0f;

    [Header("Raycast")]
    [Tooltip("遮蔽物として扱うLayer（壁/箱/地形など）。Playerは含めない")]
    public LayerMask obstacleLayer;

    public bool CanSeePlayer { get; private set; }

    // 視認強度（0〜1）
    public float VisionStrength { get; private set; }

    Transform player;
    float currentViewAngle;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (eyes == null) eyes = transform;

        currentViewAngle = normalViewAngle;
    }

    void Update()
    {
        if (player == null)
        {
            CanSeePlayer = false;
            VisionStrength = 0f;
            return;
        }

        // 角度更新（前フレームのCanSeePlayerで広げる/戻す）
        UpdateViewAngle();

        // 視界判定（strength付き）
        CanSeePlayer = CheckVision(out float strength);
        VisionStrength = strength;
    }

    void UpdateViewAngle()
    {
        float targetAngle = CanSeePlayer ? alertedViewAngle : normalViewAngle;

        currentViewAngle = Mathf.Lerp(
            currentViewAngle,
            targetAngle,
            Time.deltaTime * viewAngleChangeSpeed
        );
    }

    bool CheckVision(out float strength)
    {
        strength = 0f;

        // 扇形
        if (CheckConeVision(out float coneStr))
        {
            strength = coneStr;
            return true;
        }

        // 正面半球（近距離の救済）
        if (CheckForwardHemisphere(out float hemiStr))
        {
            strength = hemiStr;
            return true;
        }

        return false;
    }

    // =========================
    // 扇形視野
    // =========================
    bool CheckConeVision(out float strength)
    {
        strength = 0f;

        Vector3 origin = GetOrigin();
        Vector3 target = GetPlayerCenter();

        Vector3 toPlayer = target - origin;
        float dist = toPlayer.magnitude;

        if (dist > viewDistance) return false;
        if (dist < 0.001f) return false;

        Vector3 dir = toPlayer / dist;

        float half = currentViewAngle * 0.5f;
        float angle = Vector3.Angle(GetForward(), dir);
        if (angle > half) return false;

        if (!HasLineOfSight(origin, target, dist))
            return false;

        // 強度（距離×中心寄り）
        float dist01 = 1f - Mathf.Clamp01(dist / viewDistance);
        float angle01 = 1f - Mathf.Clamp01(angle / half);
        strength = Mathf.Clamp01(dist01 * angle01);

        return true;
    }

    // =========================
    // 正面半球視野（近距離）
    // =========================
    bool CheckForwardHemisphere(out float strength)
    {
        strength = 0f;

        Vector3 origin = GetOrigin();
        Vector3 target = GetPlayerCenter();

        Vector3 toPlayer = target - origin;
        float dist = toPlayer.magnitude;

        if (dist > hemisphereRadius) return false;
        if (dist < 0.001f) return false;

        Vector3 dir = toPlayer / dist;

        // 前方半球のみ
        if (Vector3.Dot(GetForward(), dir) <= 0f)
            return false;

        if (!HasLineOfSight(origin, target, dist))
            return false;

        // 半球強度（近いほど強い）
        float dist01 = 1f - Mathf.Clamp01(dist / hemisphereRadius);
        strength = Mathf.Clamp01(dist01);

        return true;
    }

    // =========================
    // 視線が通っているか判定
    // 「最初に当たったのがPlayerなら見える」
    // =========================
    bool HasLineOfSight(Vector3 origin, Vector3 target, float distance)
    {
        Vector3 dir = (target - origin).normalized;

        RaycastHit hit;

        // まず全レイヤーで当てて、Playerが最初ならOK（参考コード寄せ）
        // ただし trigger は無視（必要なら変更してOK）
        if (Physics.Raycast(origin, dir, out hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != null && hit.collider.CompareTag("Player"))
                return true;

            // 最初に当たったのがPlayer以外なら遮蔽
            return false;
        }

        // 何にも当たらない場合は「見えていない」扱い（参考コードと同じ）
        return false;
    }

    // =========================
    // Origin / Forward
    // =========================
    Vector3 GetOrigin()
    {
        Transform e = (eyes != null) ? eyes : transform;
        Vector3 origin = e.position;
        if (e == transform) origin += Vector3.up * eyeHeight;
        return origin;
    }

    Vector3 GetPlayerCenter()
    {
        return player.position + Vector3.up * playerCenterHeight;
    }

    Vector3 GetForward()
    {
        Transform e = (eyes != null) ? eyes : transform;
        return e.forward;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Transform e = (eyes != null) ? eyes : transform;
        Vector3 origin = e.position;
        if (e == transform) origin += Vector3.up * eyeHeight;

        // 扇形（黄色）
        float drawAngle = Application.isPlaying ? currentViewAngle : normalViewAngle;
        float half = drawAngle * 0.5f;

        Gizmos.color = Color.yellow;
        Vector3 left = Quaternion.Euler(0, -half, 0) * e.forward;
        Vector3 right = Quaternion.Euler(0, half, 0) * e.forward;
        Gizmos.DrawLine(origin, origin + left * viewDistance);
        Gizmos.DrawLine(origin, origin + right * viewDistance);

        // 半球（シアン）
        Gizmos.color = new Color(0, 1, 1, 0.4f);
        Gizmos.DrawWireSphere(
            origin + e.forward * hemisphereRadius * 0.5f,
            hemisphereRadius
        );
    }
#endif
}
