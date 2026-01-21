using UnityEngine;

public class PlayerState : MonoBehaviour
{
    Animator animator;

    public bool IsSuspicious { get; private set; }

    bool prevSuspicious;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogError("PlayerState: Animator が見つかりません");
        }
    }

    void Update()
    {
        if (animator == null) return;

        IsSuspicious = animator.GetBool("Suspicious");

        // ★ 状態が変わった瞬間だけログ
        if (IsSuspicious != prevSuspicious)
        {
            if (IsSuspicious)
                Debug.Log("▶ Suspicious状態");
            else
                Debug.Log("◀ 通常状態");

            prevSuspicious = IsSuspicious;
        }
    }
}
