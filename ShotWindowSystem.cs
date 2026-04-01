using UnityEngine;
using UnityEngine.Events;

public class ShotWindowSystem : MonoBehaviour
{
    [Header("Shot Window")]
    [SerializeField] private bool autoStartOnPlay = true;
    [SerializeField] private float shotWindowDuration = 3f;
    [SerializeField] private bool allowMultipleShots = false;

    [Header("Debug")]
    [SerializeField] private KeyCode debugStartKey = KeyCode.T;

    private float timeRemaining;
    private bool isActive;
    private bool shotUsed;

    public bool IsActive => isActive;
    public float TimeRemaining => timeRemaining;
    public bool CanShoot => isActive && (allowMultipleShots || !shotUsed);

    public UnityEvent OnShotWindowStart;
    public UnityEvent OnShotWindowEnd;

    private void Start()
    {
        if (autoStartOnPlay)
        {
            StartShotWindow();
        }
    }

    private void Update()
    {
        if (debugStartKey != KeyCode.None && Input.GetKeyDown(debugStartKey))
        {
            StartShotWindow();
        }

        if (!isActive)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            EndShotWindow();
        }
    }

    public void StartShotWindow()
    {
        isActive = true;
        shotUsed = false;
        timeRemaining = shotWindowDuration;
        OnShotWindowStart?.Invoke();
    }

    public void EndShotWindow()
    {
        if (!isActive)
        {
            return;
        }

        isActive = false;
        timeRemaining = 0f;
        OnShotWindowEnd?.Invoke();
    }

    public void RegisterShot()
    {
        if (!allowMultipleShots)
        {
            shotUsed = true;
        }
    }
}
