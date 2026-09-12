using System.Collections;
using UnityEngine;

public class MoleTarget : MonoBehaviour
{
    private WhackAMoleGame owner;
    private Coroutine animationRoutine;
    private Vector3 hiddenPosition;
    private Vector3 visiblePosition;

    public bool IsActive { get; private set; }

    public void Initialize(WhackAMoleGame game, Vector3 hiddenLocalPosition, Vector3 visibleLocalPosition)
    {
        owner = game;
        hiddenPosition = hiddenLocalPosition;
        visiblePosition = visibleLocalPosition;
        transform.localPosition = hiddenPosition;
        IsActive = false;
    }

    public void Pop(float visibleDuration)
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(PopRoutine(visibleDuration));
    }

    public void Hit()
    {
        if (!IsActive)
            return;

        owner.RegisterHit(this);
        HideImmediately();
    }

    public void HideImmediately()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        IsActive = false;
        transform.localPosition = hiddenPosition;
    }

    private IEnumerator PopRoutine(float visibleDuration)
    {
        IsActive = true;

        yield return MoveTo(visiblePosition, 0.14f);
        yield return new WaitForSeconds(visibleDuration);
        yield return MoveTo(hiddenPosition, 0.12f);

        IsActive = false;
        animationRoutine = null;
    }

    private IEnumerator MoveTo(Vector3 target, float duration)
    {
        Vector3 start = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localPosition = target;
    }
}
