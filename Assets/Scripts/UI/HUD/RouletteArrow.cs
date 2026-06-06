// Responsibility: Rotate an arrow to point to the active roulette icon.

using UnityEngine;

public class RouletteArrow : MonoBehaviour
{
    [Header("Config")]
    public float initialSpeed = 800f;  
    public float finalSpeed = 60f;   
    public float spinDuration = 2f;  

    private float currentSpeed;
    private float elapsed;
    private bool isSpinning;
    private float targetAngle;
    private bool isBraking;

    public bool IsSpinning => isSpinning;
    public void StartSpin()
    {
        isSpinning = true;
        isBraking = false;
        elapsed = 0f;
        currentSpeed = initialSpeed;
    }
    
    public void BrakeTo(float angle)
    {
        targetAngle = angle;
        isBraking = true;
    }

    private void Update()
    {
        if (!isSpinning) return;

        if (!isBraking)
        {
            float t = elapsed / spinDuration;
            currentSpeed = Mathf.Lerp(initialSpeed, finalSpeed, t);
            transform.Rotate(0f, 0f, -currentSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
        }
        else
        {
            float current = transform.localEulerAngles.z;
            float next = Mathf.MoveTowardsAngle(current, targetAngle, currentSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0f, 0f, next);
            currentSpeed = Mathf.Max(currentSpeed - 300f * Time.deltaTime, 20f);

            if (Mathf.Abs(Mathf.DeltaAngle(next, targetAngle)) < 0.5f)
            {
                transform.localEulerAngles = new Vector3(0f, 0f, targetAngle);
                isSpinning = false;
            }
        }
    }

    public float GetAngleForIndex(int index, RectTransform[] iconPositions, RectTransform center)
    {
        Vector2 centerPos = Vector2.zero;
        Vector2 iconPos = iconPositions[index].anchoredPosition;
        Vector2 direction = iconPos - centerPos;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
    }
}