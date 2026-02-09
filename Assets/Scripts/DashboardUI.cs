using UnityEngine;
using TMPro;

public class DashboardUI : MonoBehaviour
{
    public SimpleCarController car;
    public TextMeshProUGUI speedText;

    void Update()
    {
        if (car == null || speedText == null)
            return;

        float speed = Mathf.Abs(car.GetCurrentSpeed());
        speedText.text = $"Speed: {speed:F1}";
    }
}