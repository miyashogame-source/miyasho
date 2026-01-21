using UnityEngine;

public class PlayerAnimationInput : MonoBehaviour
{
    Animator animator;

    bool isPose = false;           // スクリプト側のポーズ状態
    string currentPoseTrigger;     // 現在のポーズトリガー名

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator が見つかりません");
        }
    }

    void Update()
    {
        if (animator == null) return;

        // ===== WASD入力でポーズ解除 =====
        if (isPose && IsMoveKeyDown())
        {
            ExitPose();
            return;
        }

        // ===== pose1〜pose5 =====
        HandlePoseKey(KeyCode.Alpha1, "pose1");
        HandlePoseKey(KeyCode.Alpha2, "pose2");
        HandlePoseKey(KeyCode.Alpha3, "pose3");
        HandlePoseKey(KeyCode.Alpha4, "pose4");
        HandlePoseKey(KeyCode.Alpha5, "pose5");

        // ===== しゃがみ =====
        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.SetTrigger("Crouch");
        }
    }

    void HandlePoseKey(KeyCode key, string triggerName)
    {
        if (!Input.GetKeyDown(key)) return;

        if (isPose && currentPoseTrigger == triggerName)
        {
            // 同じポーズキー → 解除
            ExitPose();
        }
        else
        {
            // 新しいポーズ開始
            animator.SetTrigger(triggerName);
            currentPoseTrigger = triggerName;
            isPose = true;
            animator.SetBool("IsPose", true);
        }
    }

    void ExitPose()
    {
        // 同じトリガーを再度押した扱い
        if (!string.IsNullOrEmpty(currentPoseTrigger))
        {
            animator.SetTrigger(currentPoseTrigger);
        }

        isPose = false;
        animator.SetBool("IsPose", false);
        currentPoseTrigger = null;
    }

    bool IsMoveKeyDown()
    {
        return Input.GetKeyDown(KeyCode.W)
            || Input.GetKeyDown(KeyCode.A)
            || Input.GetKeyDown(KeyCode.S)
            || Input.GetKeyDown(KeyCode.D);
    }
}
