using UnityEngine;
using UnityEngine.UI;

public class EnemyAlertWorldUI : MonoBehaviour
{
    public EnemyAlert enemyAlert;
    public Slider alertSlider;
    public Vector3 offset = new Vector3(0, 2.0f, 0);

    Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (alertSlider != null)
        {
            // ★ 値の範囲を明示（Sliderが0-1のままだと動いてないように見える）
            alertSlider.minValue = 0f;
            alertSlider.maxValue = 100f;
        }
    }

    void LateUpdate()
    {
        if (enemyAlert == null || alertSlider == null) return;
        if (mainCamera == null) mainCamera = Camera.main;

        // ★ 敵に追従
        transform.position = enemyAlert.transform.position + offset;

        // ゲージ更新
        alertSlider.value = enemyAlert.alertValue;

        // 発覚していない時は非表示（0より大きい時だけ表示）
        alertSlider.gameObject.SetActive(alertSlider.value > 0.01f);

        // ビルボード
        if (mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(
                transform.position - mainCamera.transform.position
            );
        }
    }
}
