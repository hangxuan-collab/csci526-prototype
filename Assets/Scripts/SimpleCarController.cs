using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleCarController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 15f;
    public float dragFactor = 1.5f;
    public float turnSpeed = 90f;

    [Header("Power Cut Rule")]
    public float speedThreshold = 30f;      // Speed that triggers the power cut
    public float powerCutDuration = 3f;     // Seconds to disable forward power

    // Internal state
    float currentSpeed = 0f;
    bool powerCutActive = false;
    float powerCutTimer = 0f;

    void Update()
    {
        float dt = Time.deltaTime;
        float rawMoveInput = GetVertical();
        float turnInput = GetHorizontal();

        // Predictive overspeed detection
        if (!powerCutActive && rawMoveInput > 0f)
        {
            float proposed = currentSpeed + acceleration * dt;
            if (Mathf.Abs(proposed) >= speedThreshold)
            {
                powerCutActive = true;
                powerCutTimer = powerCutDuration;
            }
        }

        if (!powerCutActive && Mathf.Abs(currentSpeed) >= speedThreshold)
        {
            powerCutActive = true;
            powerCutTimer = powerCutDuration;
        }

        // Power cut timer
        if (powerCutActive)
        {
            powerCutTimer -= dt;
            if (powerCutTimer <= 0f)
            {
                powerCutActive = false;
                powerCutTimer = 0f;
            }
        }

        // Movement handling
        float moveInput = rawMoveInput;
        if (powerCutActive && moveInput > 0f)
            moveInput = 0f;

        if (moveInput > 0f)
        {
            currentSpeed += acceleration * dt;
        }
        else if (moveInput < 0f)
        {
            if (currentSpeed > 0f)
            {
                float brakeForce = acceleration * 2f;
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeForce * (-moveInput) * dt);
            }
            else
            {
                currentSpeed += acceleration * moveInput * dt;
            }
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                Mathf.Abs(currentSpeed) * dragFactor * dt
            );
        }

        // Apply rotation and movement
        transform.Rotate(Vector3.up * turnInput * turnSpeed * dt);
        transform.Translate(Vector3.forward * currentSpeed * dt);
    }

    // 🔹 NEW: Reset power cut when camera switches
    public void ResetPowerCut()
    {
        powerCutActive = false;
        powerCutTimer = 0f;
    }

    // Input helpers
    float GetVertical()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) return 1f;
            if (Keyboard.current.sKey.isPressed) return -1f;
        }
        return Gamepad.current != null
            ? Gamepad.current.leftStick.ReadValue().y
            : 0f;
    }

    float GetHorizontal()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed) return 1f;
            if (Keyboard.current.aKey.isPressed) return -1f;
        }
        return Gamepad.current != null
            ? Gamepad.current.leftStick.ReadValue().x
            : 0f;
    }

    // Debug getters
    public float GetCurrentSpeed() => currentSpeed;
    public bool IsPowerCutActive() => powerCutActive;
    public float GetPowerCutTimer() => powerCutTimer;

    // On-screen debug UI (prototype visualization)
    void OnGUI()
    {
        GUI.Box(new Rect(8, 8, 320, 112), "Speed Info");
        GUI.Label(new Rect(16, 30, 300, 20), $"Speed: {currentSpeed:F2}");
        GUI.Label(new Rect(16, 52, 300, 20), $"Threshold: {speedThreshold:F1}");
        GUI.Label(new Rect(16, 74, 300, 20), $"PowerCut: {powerCutActive} ({powerCutTimer:F1}s)");
        GUI.Label(new Rect(16, 94, 300, 16),
            powerCutActive ? "FORWARD POWER CUT ACTIVE" : "");
    }
}