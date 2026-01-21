using UnityEngine;

[RequireComponent(typeof(PoseGaugeView))]
public class PoseGaugeAutoRegister : MonoBehaviour
{
    GaugeManager manager;
    PoseGaugeView view;

    void Start()
    {
        manager = FindObjectOfType<GaugeManager>();
        view = GetComponent<PoseGaugeView>();

        if (manager != null)
            manager.Register(view);
    }

    void OnDestroy()
    {
        if (manager != null)
            manager.Unregister(view);
    }
}
