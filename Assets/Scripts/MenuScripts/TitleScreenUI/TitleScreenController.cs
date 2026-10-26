using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class TitleScreenController : MonoBehaviour
{
    // TitleScreen
    public TMP_Text PressAnyText;
    public Image DeepSpaceFiveText;
    public Image Border;
    public TMP_Text VersionLabel;
    public float FadeDuration = 1.5f; // Time for a full fade in/out
    public GameObject TitleScreenContents;
    public GameObject TitleScreenCamera;
    public GameObject TitleScreenCameraPositions;
    private bool PressAnyTextAppears = false;

    // Audio
    [SerializeField] AudioSource MusicSource;
    public AudioClip TitleScreenAudio;
    public AudioClip SFXTest;

    // Rings
    public GameObject[] RingHalves = new GameObject[2];
    public static float[] SPIN_SPEEDS = new float[3] { 12.5f, 50.0f, 22.5f };

    // MainMenu
    public GameObject MainMenu;

    // LoadHandler.cs handles hiding/showing TitleScreenContents
    private void Start()
    {
        // Set alpha of everything to 0
        SetRingAlpha(0f);
        DeepSpaceFiveText.color = new Color(1, 1, 1, 0);
        Border.color = new Color(1, 1, 1, 0);
        PressAnyText.alpha = 0f;
        VersionLabel.alpha = 0f;

        StartCoroutine(IntroSequence());
    }

    private void Update()
    {
        SpinRings();

        if (Input.anyKeyDown && PressAnyTextAppears && TitleScreenContents.activeSelf)
        {
            SwitchToMainMenu();
            StartCoroutine(PushCameraBack(7.0f));
            VersionLabel.alpha = 0f;
        }
    }

    public void PlaceCameraAtStart()
    {
        TitleScreenCamera.transform.position = TitleScreenCameraPositions.transform.GetChild(0).position;
    }

    IEnumerator PushCameraBack(float duration)
    {
        Vector3 startingPosition = TitleScreenCameraPositions.transform.GetChild(0).position;
        Vector3 destinationPosition = TitleScreenCameraPositions.transform.GetChild(1).position;
        TitleScreenCamera.transform.position = startingPosition;

        float time = duration;
        AnimationCurve animation_curve = AnimationCurve.EaseInOut(0.0f, 0.0f, time, 1.0f);
        while (time > 0.0f)
        {
            time = Mathf.Max(0.0f, time - Time.deltaTime);

            float animation_percentage = animation_curve.Evaluate(time);
            TitleScreenCamera.transform.position = Vector3.Lerp(destinationPosition, startingPosition, animation_percentage);

            yield return null;
        }
    }

    IEnumerator IntroSequence()
    {
        StartCoroutine(FadeTo(DeepSpaceFiveText, 1f, 2f));

        yield return StartCoroutine(FadeRings(1f, 2f));

        StartCoroutine(FadeTo(Border, 1f, 2f));

        StartCoroutine(FadeTo(VersionLabel, 1f, 2f));

        StartCoroutine(FadePressAnyButtonText(PressAnyText));

        PressAnyTextAppears = true;
    }

    IEnumerator FadePressAnyButtonText(TMP_Text text)
    {
        while (true)
        {
            yield return StartCoroutine(FadeTo(text, 1f, FadeDuration)); // Fade in
            yield return StartCoroutine(FadeTo(text, 0f, FadeDuration)); // Fade out
        }
    }

    private void SpinRings()
    {
        for (int i = 0; i < 3; i++)
        {
            float z = RingHalves[0].transform.GetChild(i).GetComponent<Transform>().rotation.eulerAngles.z + SPIN_SPEEDS[i] * Time.deltaTime;
            RingHalves[0].transform.GetChild(i).localRotation = Quaternion.Euler(0.0f, 0.0f, z);
            RingHalves[1].transform.GetChild(i).localRotation = Quaternion.Euler(0.0f, 0.0f, z);
        }
    }

    IEnumerator FadeRings(float targetAlpha, float duration)
    {
        float startAlpha = RingHalves[0].transform.GetChild(0).GetComponent<Image>().color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            
            SetRingAlpha(alpha);

            yield return null;
        }

        SetRingAlpha(targetAlpha);
    }

    public void SetRingAlpha(float alpha)
    {
        for (int i = 0; i < 3; i++)
        {
            Image ring = RingHalves[0].transform.GetChild(i).GetComponent<Image>();
            Color color = ring.color;
            color.a = alpha;
            ring.color = color;
            RingHalves[1].transform.GetChild(i).GetComponent<Image>().color = color;
        }
    }

    // Both Image and TMP_Text inherit from Graphic
    IEnumerator FadeTo(Graphic graphic, float targetAlpha, float duration)
    {
        Color color = graphic.color;
        float startAlpha = color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime * 0.8f;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            graphic.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        graphic.color = new Color(color.r, color.g, color.b, targetAlpha);
    }

    private void SwitchToMainMenu()
    {
        TitleScreenContents.SetActive(false);
        MainMenu.SetActive(true);
    }
}