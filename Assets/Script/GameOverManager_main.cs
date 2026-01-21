using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Over UI (Same Scene)")]
    public GameObject gameOverUIRoot;      // ★ CanvasのGameOverパネル等を指定
    public Animator gameOverAnimator;      // 任意（UIアニメ用）
    public float pauseDelay = 0.2f;        // UI表示→少し待って停止 などに使う
    public bool pauseTimeScale = true;     // ★ Time.timeScaleを0にするか

    public bool IsGameOver { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // シーン遷移しないならDontDestroyOnLoadは不要だが、残しても害は少ない
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (gameOverUIRoot != null)
            gameOverUIRoot.SetActive(false);
    }

    public void StartGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        if (gameOverUIRoot != null)
            gameOverUIRoot.SetActive(true);

        if (gameOverAnimator != null)
            gameOverAnimator.SetTrigger("GameOver");

        if (pauseTimeScale)
            StartCoroutine(PauseAfterDelayRealtime());
    }

    IEnumerator PauseAfterDelayRealtime()
    {
        if (pauseDelay > 0f)
            yield return new WaitForSecondsRealtime(pauseDelay);

        Time.timeScale = 0f;
    }

    // Retry/Title ボタン側で Time.timeScale=1 に戻すのを忘れずに
}
