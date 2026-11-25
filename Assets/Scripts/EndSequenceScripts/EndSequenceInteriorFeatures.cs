/*
    EndSequenceInteriorFeatures.cs
    - Helps with some of the kinetic elements inside Deep Space Five
    Contributor(s): Jake Schott
    Last Updated: 11/21/2026
*/

using UnityEngine;
using System.Collections;
using TMPro;

public class EndSequenceInteriorFeatures : MonoBehaviour
{
    //CLASS CONSTANTS
    private static float CENTER_LOGO_SPIN_SPEED = 12.5f;
    private static float ENERGY_PULSE_SPIN_SPEED = 25.0f;
    private static float VERTICAL_ENERGY_CIRCLES_MOVE_TIME = 4.0f;
    private static float PIE_CHART_SPIN_SPEED = 50.0f;
    private static float RANDOM_IMAGE_HIGHLIGHT_DELAY = 0.2f;

    public GameObject center_logo;
    public GameObject rotational_energy_pulse;
    public GameObject vertical_energy_circles_a;
    public GameObject vertical_energy_circles_b;
    public GameObject pie_chart_one_third;
    public GameObject pie_chart_two_thirds;
    public GameObject indented_squares;
    public GameObject slashed_circles;
    public GameObject power_circles;
    public GameObject now_boarding_sound;
    public GameObject gate_b_entrance_display;
    public GameObject[] gate_b_displays = new GameObject[4];

    private Coroutine deep_space_five_music_coroutine = null;

    private void OnEnable()
    {
        StartCoroutine(verticalEnergyCirclesMover());
        StartCoroutine(randomImageHighlighter());
    }

    private void rotateHelper(Transform t, float speed)
    {
        float z = t.localRotation.eulerAngles.z + speed * Time.deltaTime;
        t.localRotation = Quaternion.Euler(0.0f, 0.0f, z);
    }

    private void randomizeImageHighlights(Transform t)
    {
        Color c = ReferenceAssistor.COLOR_OPTIONS[0];
        foreach (Transform image in t)
        {
            c.a = 1.0f;
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                c.a = 0.2f;
            }
            image.GetComponent<UnityEngine.UI.Image>().color = c;
        }
    }

    private void Update()
    {
        //spin center logo
        rotateHelper(center_logo.transform, CENTER_LOGO_SPIN_SPEED);

        //spin energy pulse
        rotateHelper(rotational_energy_pulse.transform, ENERGY_PULSE_SPIN_SPEED);

        //spin pie charts
        rotateHelper(pie_chart_one_third.transform, PIE_CHART_SPIN_SPEED);
        rotateHelper(pie_chart_two_thirds.transform, PIE_CHART_SPIN_SPEED);
    }

    IEnumerator verticalEnergyCirclesMover()
    {
        while (true)
        {
            if (ReferenceAssistor.Instance != null)
            {
                vertical_energy_circles_a.transform.localPosition = Vector3.zero;
                ReferenceAssistor.Instance.translation_helper.linearTranslation(vertical_energy_circles_a.transform, new Vector3(0.0f, 0.0f, 6.0f), VERTICAL_ENERGY_CIRCLES_MOVE_TIME);
            }

            yield return new WaitForSeconds(VERTICAL_ENERGY_CIRCLES_MOVE_TIME * 0.5f);

            if (ReferenceAssistor.Instance != null)
            {
                vertical_energy_circles_b.transform.localPosition = Vector3.zero;
                ReferenceAssistor.Instance.translation_helper.linearTranslation(vertical_energy_circles_b.transform, new Vector3(0.0f, 0.0f, 6.0f), VERTICAL_ENERGY_CIRCLES_MOVE_TIME);
            }

            yield return new WaitForSeconds(VERTICAL_ENERGY_CIRCLES_MOVE_TIME * 0.5f);
        }
    }

    IEnumerator randomImageHighlighter()
    {
        while (true)
        {
            randomizeImageHighlights(indented_squares.transform);
            randomizeImageHighlights(slashed_circles.transform);
            randomizeImageHighlights(power_circles.transform);

            yield return new WaitForSeconds(RANDOM_IMAGE_HIGHLIGHT_DELAY);
        }
    }

    public void enableBoardingSequence()
    {
        now_boarding_sound.GetComponent<AudioSource>().Play();
        StartCoroutine(gateSignFlasher());
        foreach (GameObject display in gate_b_displays)
        {
            display.transform.GetChild(4).GetChild(1).GetComponent<TMP_Text>().SetText("BOARDING");
        }
        gate_b_entrance_display.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().SetText("BOARDING");
    }

    IEnumerator gateSignFlasher()
    {
        CanvasGroup[] gate_sign_canvas_groups = new CanvasGroup[5];
        for (int i = 0; i < 4; i++)
        {
            gate_sign_canvas_groups[i] = gate_b_displays[i].GetComponent<CanvasGroup>();
        }
        gate_sign_canvas_groups[4] = gate_b_entrance_display.transform.GetChild(1).GetComponent<CanvasGroup>();
        while (true)
        {
            foreach (CanvasGroup cg in gate_sign_canvas_groups)
            {
                cg.alpha = 1.0f;
            }

            yield return new WaitForSeconds(0.2f);

            foreach (CanvasGroup cg in gate_sign_canvas_groups)
            {
                cg.alpha = 0.2f;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    public void playDeepSpaceFiveMusic()
    {
        if (deep_space_five_music_coroutine == null)
        {
            deep_space_five_music_coroutine = StartCoroutine(deepSpaceFiveMusicPlayer());
        }
    }

    IEnumerator deepSpaceFiveMusicPlayer()
    {
        int index = 0;
        while (EndSequenceReferenceAssistor.Instance.deep_space_five_speaker_music != null && EndSequenceReferenceAssistor.Instance.deep_space_five_speaker_music.activeSelf == true)
        {
            AudioSource song = EndSequenceReferenceAssistor.Instance.deep_space_five_speaker_music.transform.GetChild(index).GetComponent<AudioSource>();
            song.Play();
            while (song.isPlaying == true)
            {
                yield return null;
            }
            index++;
            if (index > song.transform.parent.childCount - 1)
            {
                index = 0;
            }
        }
        deep_space_five_music_coroutine = null;
    }
}