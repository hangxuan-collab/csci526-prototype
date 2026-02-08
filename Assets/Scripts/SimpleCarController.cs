using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleCarController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 15f;
    public float dragFactor = 1.5f;
    public float turnSpeed = 90f;

    [Header("Power cut rule")]
    public float speedThreshold = 30f;        // speed that triggers the power cut
    public float powerCutDuration = 3f;       // seconds to disable forward power

    // internal state
    float currentSpeed = 0f;
    bool powerCutActive = false;
    float powerCutTimer = 0f;

    void Update()
    {
        float dt = Time.deltaTime;
        float rawMoveInput = GetVertical();   // -1..1 (player input)
        float turnInput = GetHorizontal();

        // --- Predictive overspeed detection ---
        // If applying a forward acceleration this frame would push us over the threshold,
        // trigger the power cut immediately (so holding W from start will still trigger).
        if (!powerCutActive && rawMoveInput > 0f)
        {
            float proposed = currentSpeed + acceleration * dt;
            if (Mathf.Abs(proposed) >= speedThreshold)
            {
                powerCutActive = true;
                powerCutTimer = powerCutDuration;
            }
        }

        // Also trigger if current speed is already beyond threshold (safety)
        if (!powerCutActive && Mathf.Abs(currentSpeed) >= speedThreshold)
        {
            powerCutActive = true;
            powerCutTimer = powerCutDuration;
        }

        // Update power cut timer
        if (powerCutActive)
        {
            powerCutTimer -= dt;
            if (powerCutTimer <= 0f)
            {
                powerCutActive = false;
                powerCutTimer = 0f;
            }
        }

        // --- Movement input handling ---
        // During power cut forward power is removed. But we want the car to decelerate
        // even if the player continues to hold W. To do that, we treat forward input as "no input"
        // while power cut is active (so passive drag applies). Braking/reverse still works.
        float moveInput = rawMoveInput;
        if (powerCutActive && moveInput > 0f)
        {
            // ignore forward input ¡ª behave as if no input was given
            moveInput = 0f;
        }

        if (moveInput > 0f)
        {
            // normal forward acceleration
            currentSpeed += acceleration * dt;
        }
        else if (moveInput < 0f)
        {
            // braking/reverse behavior (brake toward zero if moving forward)
            if (currentSpeed > 0f)
            {
                float brakeForce = acceleration * 2f; // simple braking scale
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeForce * (-moveInput) * dt);
            }
            else
            {
                // reverse acceleration
                currentSpeed += acceleration * moveInput * dt;
            }
        }
        else
        {
            // no input (or forward ignored during power cut): passive drag
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                Mathf.Abs(currentSpeed) * dragFactor * dt
            );
        }

        // Apply rotation
        transform.Rotate(Vector3.up * turnInput * turnSpeed * dt);

        // Apply translation
        transform.Translate(Vector3.forward * currentSpeed * dt);
    }

    // Input helpers (new Input System)
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

    // Public getters (useful for UI)
    public float GetCurrentSpeed() => currentSpeed;
    public bool IsPowerCutActive() => powerCutActive;
    public float GetPowerCutTimer() => powerCutTimer;

    // On-screen debug
    void OnGUI()
    {
        GUI.Box(new Rect(8, 8, 320, 112), "Speed Info");
        GUI.Label(new Rect(16, 30, 300, 20), $"Speed: {currentSpeed:F2}");
        GUI.Label(new Rect(16, 52, 300, 20), $"Threshold: {speedThreshold:F1}");
        GUI.Label(new Rect(16, 74, 300, 20), $"PowerCut: {powerCutActive} ({powerCutTimer:F1}s)");
        GUI.Label(new Rect(16, 94, 300, 16), powerCutActive ? "FORWARD POWER CUT (forward ignored)" : "");
    }
}
