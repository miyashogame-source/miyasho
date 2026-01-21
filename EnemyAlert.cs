using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class EnemyAlert : MonoBehaviour
{
    [Header("Alert (0=Safe / 100=Fail)")]
    public float alertValue = 0f;
    public float alertIncrease = 100f;
    public float alertDecrease = 20f;

    [Header("Alert Increase Stages (VisionStrength 0-1)")]
    [Range(0f, 1f)] public float stage1 = 0.15f;
    [Range(0f, 1f)] public float stage2 = 0.35f;
    [Range(0f, 1f)] public float stage3 = 0.60f;
    [Range(0f, 1f)] public float stage4 = 0.85f;

    [Header("Proximity Suspicion Boost")]
    public float suspiciousBoostRadius = 6f;
    public float nearBoostMaxMultiplier = 2.0f;
    public float proximityOnlyIncrease = 25f;

    [Header("Face Each Other")]
    [Tooltip("向き合い完了までの秒数（unscaled）")]
    public float turnTime = 0.5f;

    [Tooltip("プレイヤー側で“回すべきTransform”。未設定なら Player(Tag) を回す。")]
    public Transform playerLookTarget;

    [Tooltip("敵側で“回すべきTransform”。未設定ならこのEnemyのtransformを回す。")]
    public Transform enemyLookTarget;

    public bool IsFailed => alertValue >= 100f;

    EnemyVision vision;
    NavMeshAgent agent;

    Transform playerRoot;
    Transform playerAnimatorTr;
    PlayerState playerState;
    Animator playerAnimator;
    PlayerInput playerInput;
    MonoBehaviour playerMoveScript;

    bool prevCanSee;
    bool gameOverTriggered;

    void Start()
    {
        vision = GetComponent<EnemyVision>();
        agent = GetComponent<NavMeshAgent>();

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerRoot = p.transform;

        var animatorObj = GameObject.Find("PlayerAnimator");
        if (animatorObj != null)
        {
            playerAnimatorTr = animatorObj.transform;
            playerState = animatorObj.GetComponent<PlayerState>();
            playerAnimator = animatorObj.GetComponent<Animator>();
            playerInput = animatorObj.GetComponent<PlayerInput>();
        }

        if (playerInput == null && playerRoot != null)
            playerInput = playerRoot.GetComponent<PlayerInput>();

        if (playerRoot != null)
        {
            foreach (var mb in playerRoot.GetComponents<MonoBehaviour>())
            {
                if (mb == null) continue;
                var name = mb.GetType().Name;
                if (name == "ThirdPersonController" || name == "FirstPersonController")
                {
                    playerMoveScript = mb;
                    break;
                }
            }
            if (playerMoveScript == null)
                playerMoveScript = playerRoot.GetComponent<MonoBehaviour>();
        }

        // ★ 未指定ならデフォルト
        if (enemyLookTarget == null) enemyLookTarget = transform;

        // ★ playerLookTarget が未指定なら「見た目が入ってそうな方」を優先
        if (playerLookTarget == null)
            playerLookTarget = (playerAnimatorTr != null) ? playerAnimatorTr : playerRoot;
    }

    void Update()
    {
        if (vision == null) return;
        if (gameOverTriggered) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        bool canSee = vision.CanSeePlayer;
        bool isSuspicious = (playerState != null) ? playerState.IsSuspicious : false;

        float delta = 0f;

        if (isSuspicious)
        {
            float dist = PlayerDistanceFlat();
            float nearMul = GetNearBoostMultiplier(dist);

            if (canSee)
            {
                float visionMul = GetAlertMultiplier(vision.VisionStrength);
                delta += alertIncrease * visionMul * nearMul * Time.deltaTime;
            }
            else
            {
                if (proximityOnlyIncrease > 0f && dist <= suspiciousBoostRadius)
                    delta += proximityOnlyIncrease * nearMul * Time.deltaTime;
                else
                    delta -= alertDecrease * Time.deltaTime;
            }
        }
        else
        {
            delta -= alertDecrease * Time.deltaTime;
        }

        alertValue = Mathf.Clamp(alertValue + delta, 0f, 100f);

        if (alertValue >= 100f)
            TriggerGameOver();

        if (canSee != prevCanSee)
        {
            Debug.Log(canSee ? "▶ 視認開始" : "◀ 視認ロスト");
            prevCanSee = canSee;
        }
    }

    float PlayerDistanceFlat()
    {
        Transform p = (playerRoot != null) ? playerRoot : playerAnimatorTr;
        if (p == null) return Mathf.Infinity;

        Vector3 a = transform.position; a.y = 0f;
        Vector3 b = p.position; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    float GetNearBoostMultiplier(float dist)
    {
        if (suspiciousBoostRadius <= 0.01f) return 1f;
        if (dist >= suspiciousBoostRadius) return 1f;

        float t = 1f - Mathf.Clamp01(dist / suspiciousBoostRadius);
        return Mathf.Lerp(1f, Mathf.Max(1f, nearBoostMaxMultiplier), t);
    }

    void TriggerGameOver()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        Debug.Log("🚨 発見確定");

        // 敵の移動停止
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            // ★ 回転の上書きを止める（重要）
            agent.updateRotation = false;
        }

        if (playerInput != null) playerInput.enabled = false;
        if (playerMoveScript != null) playerMoveScript.enabled = false;

        if (playerRoot != null)
        {
            var rb = playerRoot.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        if (playerAnimator != null)
            playerAnimator.SetTrigger("GameOver");

        StartCoroutine(FaceEachOtherHard());

        if (GameManager.Instance != null)
            GameManager.Instance.StartGameOver();
    }

    IEnumerator FaceEachOtherHard()
    {
        Transform p = playerLookTarget;
        Transform e = enemyLookTarget;

        if (p == null || e == null) yield break;

        // 目標回転（水平回転のみ）
        Quaternion pTarget = YawLookRotation(from: p.position, to: e.position);
        Quaternion eTarget = YawLookRotation(from: e.position, to: p.position);

        float elapsed = 0f;

        // turnTime 秒で到達するような角速度（最低保証つき）
        float pMaxDeg = Mathf.Max(30f, Quaternion.Angle(p.rotation, pTarget) / Mathf.Max(0.01f, turnTime));
        float eMaxDeg = Mathf.Max(30f, Quaternion.Angle(e.rotation, eTarget) / Mathf.Max(0.01f, turnTime));

        while (elapsed < turnTime)
        {
            elapsed += Time.unscaledDeltaTime;

            p.rotation = Quaternion.RotateTowards(p.rotation, pTarget, pMaxDeg * Time.unscaledDeltaTime);
            e.rotation = Quaternion.RotateTowards(e.rotation, eTarget, eMaxDeg * Time.unscaledDeltaTime);

            yield return null;
        }

        p.rotation = pTarget;
        e.rotation = eTarget;
    }

    static Quaternion YawLookRotation(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return Quaternion.identity;
        return Quaternion.LookRotation(dir.normalized, Vector3.up);
    }

    float GetAlertMultiplier(float strength)
    {
        if (strength < stage1) return 0.10f;
        if (strength < stage2) return 0.30f;
        if (strength < stage3) return 0.60f;
        if (strength < stage4) return 1.00f;
        return 1.40f;
    }
}
