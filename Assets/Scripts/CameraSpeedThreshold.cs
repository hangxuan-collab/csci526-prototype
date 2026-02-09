using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSpeedThreshold : MonoBehaviour
{
    [Header("Camera References")]
    public Camera cameraFP;
    public Camera cameraTP;

    [Header("Speed Thresholds")]
    public float fpSpeedThreshold = 20f;    // Narrow window (hard)
    public float tpSpeedThreshold = 35f;    // Wide window (safe)

    SimpleCarController car;
    bool isFirstPerson = false;

    void Start()
    {
        car = GetComponent<SimpleCarController>();
        SetThirdPerson();
    }

    void Update()
    {
        // NEW INPUT SYSTEM CHECK
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            Debug.Log("C pressed - switching camera");
            ToggleCamera();
        }
    }

    void ToggleCamera()
    {
        isFirstPerson = !isFirstPerson;

        if (isFirstPerson)
            SetFirstPerson();
        else
            SetThirdPerson();
    }

    void SetFirstPerson()
    {
        cameraTP.gameObject.SetActive(false);
        cameraFP.gameObject.SetActive(true);

        car.speedThreshold = fpSpeedThreshold;
    }

    void SetThirdPerson()
    {
        cameraFP.gameObject.SetActive(false);
        cameraTP.gameObject.SetActive(true);

        car.speedThreshold = tpSpeedThreshold;
    }
}