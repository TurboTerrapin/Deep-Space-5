/*
    TranslationHelper.cs
    - Helps with moving things around in a linear or exponential fashion
    Contributor(s): Jake Schott
    Last Updated: 10/29/2026
*/

using System.Collections;
using UnityEngine;

public class TranslationHelper : MonoBehaviour
{
    public Coroutine linearTranslation(Transform to_translate, Vector3 end, float time)
    {
        return StartCoroutine(translateLinear(to_translate, end, time));
    }

    public Coroutine linearTranslation(Transform to_translate, Transform start, Transform end, float time)
    {
        return StartCoroutine(translateLinear(to_translate, start, end, time));
    }

    public Coroutine exponentialTranslation(Transform to_translate, Vector3 end, bool ease_in, float time)
    {
        return StartCoroutine(translateExponential(to_translate, end, ease_in, time));
    }

    public Coroutine exponentialTranslation(Transform to_translate, Transform start, Transform end, bool ease_in, float time)
    {
        return StartCoroutine(translateExponential(to_translate, start, end, ease_in, time));
    }

    IEnumerator translateLinear(Transform to_translate, Vector3 end, float time)
    {
        Vector3 start = to_translate.localPosition;

        float anim_time = time;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            to_translate.transform.localPosition = Vector3.Lerp(end, start, anim_time / time);

            yield return null;
        }
    }

    IEnumerator translateLinear(Transform to_translate, Transform start, Transform end, float time)
    {
        float anim_time = time;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            to_translate.transform.localPosition = Vector3.Lerp(end.localPosition, start.localPosition, anim_time / time);
            to_translate.transform.localRotation = Quaternion.Lerp(end.localRotation, start.localRotation, anim_time / time);

            yield return null;
        }
    }

    private AnimationCurve getAnimationCurve(float time, bool ease_in)
    {
        if (ease_in == false)
        {
            time += (time * 0.1f);
        }
        return AnimationCurve.EaseInOut(0.0f, 0.0f, time, 1.0f);
    }

    IEnumerator translateExponential(Transform to_translate, Vector3 end, bool ease_in, float time)
    {
        Vector3 start = to_translate.localPosition;

        float anim_time = time;
        AnimationCurve animation_curve = getAnimationCurve(time, ease_in);
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            float anim_percentage = animation_curve.Evaluate(anim_time);
            to_translate.transform.localPosition = Vector3.Lerp(end, start, anim_percentage);

            yield return null;
        }
    }

    IEnumerator translateExponential(Transform to_translate, Transform start, Transform end, bool ease_in, float time)
    {
        float anim_time = time;
        AnimationCurve animation_curve = getAnimationCurve(time, ease_in);
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            float anim_percentage = animation_curve.Evaluate(anim_time);
            to_translate.transform.localPosition = Vector3.Lerp(end.localPosition, start.localPosition, anim_percentage);
            to_translate.transform.localRotation = Quaternion.Lerp(end.localRotation, start.localRotation, anim_percentage);

            yield return null;
        }
    }
}