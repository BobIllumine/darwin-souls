using UnityEngine;

public class TimeControl : MonoBehaviour
{
    [SerializeField] private float defaultTime = 1.0f;
    private float currentTime;
    void Awake()
    {
        currentTime = defaultTime;
        SetTimeScale(currentTime);
    }
    void Update()
    {
        // Changing the time scale with keys
        if (Input.GetKeyDown(KeyCode.Plus))
        {
            currentTime *= 2.0f;
            SetTimeScale(currentTime);
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
        {
            currentTime /= 2.0f;
            SetTimeScale(currentTime);
        }
    }

    void SetTimeScale(float newTimeScale)
    {
        // Set the time scale
        Time.timeScale = newTimeScale;

        // Adjust fixedDeltaTime to maintain consistent physics calculations
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        Time.maximumDeltaTime = 0.33f * Time.timeScale;

        Time.maximumParticleDeltaTime = 0.03f * Time.timeScale;

        Debug.Log("Time scale set to: " + Time.timeScale);
        Debug.Log("Fixed delta time set to: " + Time.fixedDeltaTime);
    }
}