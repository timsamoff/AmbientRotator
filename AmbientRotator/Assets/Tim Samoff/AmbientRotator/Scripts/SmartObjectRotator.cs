using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Range
{
    public float low;
    public float high;

    public Range(float low, float high)
    {
        this.low = low;
        this.high = high;
    }
}

public class SmartObjectRotator : MonoBehaviour
{
    public enum TransitionType
    {
        Linear,
        ExponentialLogarithmic
    }

    [Header("Rotation Settings")]
    [SerializeField]
    private float rotationSpeed = 100f;

    [SerializeField]
    private float timeBeforeInitialRotation = 0f;

    [SerializeField]
    private float destinationDuration = 1f;

    [SerializeField]
    private float transitionDuration = 1f;

    private float originalDestinationDuration;

    [SerializeField]
    private bool randomizeStartRotation = true;

    [SerializeField]
    private TransitionType transitionType = TransitionType.Linear;

    [SerializeField]
    private float logarithmicRolloff = 1.0f;

    // Define ranges for each axis using the custom Range struct
    [Header("Rotation Ranges (-1.0 - 1.0)")]
    [SerializeField]
    private Range xRange = new Range(-1.0f, 1.0f);

    [SerializeField]
    private Range yRange = new Range(-1.0f, 1.0f);

    [SerializeField]
    private Range zRange = new Range(-1.0f, 1.0f);

    [Header("Rotation Values")]
    [SerializeField, Range(-1.0f, 1.0f)]
    private float xRotation = 0.0f;

    [SerializeField, Range(-1.0f, 1.0f)]
    private float yRotation = 0.0f;

    [SerializeField, Range(-1.0f, 1.0f)]
    private float zRotation = 0.0f;

    private void Start()
    {
        // Temporarily set destinationDuration
        originalDestinationDuration = destinationDuration;
        destinationDuration = timeBeforeInitialRotation;

        // Debug.Log("Temporary destinationDuration set to:" + " " + timeBeforeInitialRotation);

        if (randomizeStartRotation)
        {
            // Set the initial rotation to a completely random angle
            xRotation = GetClampedRandomRotation(xRange);
            yRotation = GetClampedRandomRotation(yRange);
            zRotation = GetClampedRandomRotation(zRange);
        }

        // Rotate the object to the random initial rotation
        transform.Rotate(new Vector3(xRotation, yRotation, zRotation) * rotationSpeed);

        // Start the coroutine for continuous rotation
        StartCoroutine(ChangeRotation());
    }

    private IEnumerator ChangeRotation()
    {
        while (true)
        {
            // Keep the current values for destinationDuration
            float elapsedTime = 0f;

            while (elapsedTime < destinationDuration)
            {
                // Rotate the object using rotationSpeed
                transform.Rotate(new Vector3(xRotation, yRotation, zRotation) * rotationSpeed * Time.deltaTime);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Transition to new destination values using transitionDuration
            float targetXRot = RoundRotation(GetClampedRandomRotation(xRange));
            float targetYRot = RoundRotation(GetClampedRandomRotation(yRange));
            float targetZRot = RoundRotation(GetClampedRandomRotation(zRange));

            float initialXRot = xRotation;
            float initialYRot = yRotation;
            float initialZRot = zRotation;

            elapsedTime = 0f; // Reset the time for the transition to the next clamped value

            // Transition to the new destination values
            while (elapsedTime < transitionDuration)
            {
                float t = elapsedTime / transitionDuration;

                if (transitionType == TransitionType.Linear)
                {
                    xRotation = Mathf.Lerp(initialXRot, targetXRot, t);
                    yRotation = Mathf.Lerp(initialYRot, targetYRot, t);
                    zRotation = Mathf.Lerp(initialZRot, targetZRot, t);
                }
                else if (transitionType == TransitionType.ExponentialLogarithmic)
                {
                    xRotation = Mathf.Lerp(initialXRot, targetXRot, Mathf.Pow(t, logarithmicRolloff));
                    yRotation = Mathf.Lerp(initialYRot, targetYRot, Mathf.Pow(t, logarithmicRolloff));
                    zRotation = Mathf.Lerp(initialZRot, targetZRot, Mathf.Pow(t, logarithmicRolloff));
                }

                // Rotate the object using rotationSpeed
                transform.Rotate(new Vector3(xRotation, yRotation, zRotation) * rotationSpeed * Time.deltaTime);

                elapsedTime += Time.deltaTime;
                yield return null;

                // Revert destinationDuration back to the original value
                destinationDuration = originalDestinationDuration;

                // Debug.Log("destinationDuration set to:" + " " + destinationDuration);
            }

            // Match the target values
            xRotation = targetXRot;
            yRotation = targetYRot;
            zRotation = targetZRot;

            yield return null; // Delay before choosing the next random rotation
        }
    }

    private float GetClampedRandomRotation(Range range)
    {
        float[] possibleValues = { -1.0f, -0.9f, -0.8f, -0.7f, -0.6f, -0.5f, -0.4f, -0.3f, -0.2f, -0.1f, 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
        return Mathf.Clamp(possibleValues[Random.Range(0, possibleValues.Length)], range.low, range.high);
    }

    private float RoundRotation(float value)
    {
        return Mathf.Round(value * 10f) / 10f;
    }
}