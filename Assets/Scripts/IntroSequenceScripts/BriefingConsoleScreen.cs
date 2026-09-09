/*
    BriefingConsoleScreen.cs
    - Used to give the player the background information for their mission
    Contributor(s): Jake Schott
    Last Updated: 9/8/2026
*/

using System.Collections;
using TMPro;
using UnityEngine;

public class BriefingConsoleScreen : MonoBehaviour, IDescribable
{
    //CLASS CONSTANTS
    public static string[][] PAGE_MESSAGES = new string[][]
    {
        new string[] { "GOOD MORNING ENSIGN ", "- OPERATION HOLIDAY IS A GO", "- ALL SAFETY CHECKS ARE VERIFIED", "- EVERY TEAM MEMBER IS ACCOUNTED FOR", "- YOU ARE THE LAST TO REPORT", "PROCEED TO NEXT PAGE" }, //page 1
        new string[] { "YOUR TARGET IS SCC-3002", "- U.S.S. RENEWAL, MANIFEST CLASS", "- MAXIMUM CREW SIZE OF 50", "- READY FOR LONG-DISTANCE TRAVEL", "- CURRENTLY UNDERGOING RENOVATIONS", "PROCEED TO NEXT PAGE" }, //page 2
        new string[] { "TARGET SHIP IS CURRENTLY UNGUARDED", "- MOVE WITH HASTE", "- DO NOT STOP FOR ANYTHING OR ANYONE", "- REMAIN CALM AND PROFESSIONAL", "- STICK TO THE PLAN", "PROCEED TO NEXT PAGE" }, //page 3
        new string[] { "MEET YOUR TEAM ABOARD SCC-3002", "- DEPART IMMEDIATELY", "- ENGAGE WARP DRIVE", "- MAINTAIN RADIO SILENCE", "- RENDEZVOUS AT DEEP SPACE FIVE",  "PROCEED TO NEXT PAGE" }, //page 4
        new string[] { "AS A REMINDER", "- YOU WILL PASS THROUGH ITEM STORAGE", "- YOU WILL PASS THROUGH CONSOLE REPAIR", "- YOU WILL ARRIVE AT HANGAR CONTROL", "- YOU WILL PROCEED TO HANGAR B3", "PROCEED TO NEXT PAGE" }, //page 5
        new string[] { "WHEN YOU ARRIVE AT HANGAR CONTROL", "- ASSUME CONTROL CONSOLE", "- SET AUTHORIZATION CODE TO ", "- SET CLEARANCE DESIGNATION TO ", "- IF CORRECT, DOOR WILL OPEN", "PROCEED TO NEXT PAGE" }, //page 6
        new string[] { "GOOD LUCK ENSIGN ", "- IF YOU ARE CAUGHT, I CANNOT HELP YOU", "- IF YOU DEFY THESE INSTRUCTIONS, I CANNOT HELP YOU", "- EXIT DOOR IS NOW UNLOCKED", "- THIS MESSAGE WILL AUTO-DELETE IN 10 MINUTES" }, //page 7
        new string[] { "I HOPE TO SEE YOU FACE-TO-FACE AT DEEP SPACE FIVE IN APPROXIMATELY 4 DAYS TO DISCUSS YOUR NEXT STEPS", "YOUR VALUE CANNOT BE UNDERSTATED", "YOUR TRAINING IS YOUR STRENGTH", "YOUR FRIEND ON THE INSIDE,", "- W.G." } //page 8
    };
    private static float DELETE_DELAY = 600.0f; //10 minutes

    private static string CONTROL_NAME = "BRIEFING CONSOLE DISPLAY";
    private static string CONTROL_INFO = "Used to display messages from SCC personnel.";

    public GameObject briefing_console_report_display;
    public GameObject briefing_room_exit_door;
    public GameObject briefing_room_exit_door_display;
    private BriefingConsoleOptions briefing_console_options;

    private bool is_enabled = false;
    private bool message_completed = false;
    private bool message_deleted = false;
    private int current_page = 0;
    private Coroutine intro_coroutine = null;
    private Coroutine message_display_coroutine = null;

    private HUDInfo hud_info;

    private void Start()
    {
        briefing_console_options = GetComponent<BriefingConsoleOptions>();

        //set player name to certain things
        briefing_console_options.briefing_console_facial_recognition_display.transform.GetChild(2).GetChild(1).GetComponent<TMP_Text>().SetText(CustomizeCharacterMenu.GetFirstName() + "\n" + CustomizeCharacterMenu.GetLastName());
        briefing_console_report_display.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().SetText("ENCRYPTED MESSAGE FOR\n" + CustomizeCharacterMenu.GetFirstName() + " " + CustomizeCharacterMenu.GetLastName());
        PAGE_MESSAGES[0][0] += CustomizeCharacterMenu.GetLastName();
        PAGE_MESSAGES[6][0] += CustomizeCharacterMenu.GetLastName();
        PAGE_MESSAGES[5][2] += ReferenceAssistor.Instance.intro_sequence_manager.getOverrideCode();
        PAGE_MESSAGES[5][3] += IntroSequenceManager.SHAPE_NAMES[ReferenceAssistor.Instance.intro_sequence_manager.getOverrideShape()];

        hud_info = new HUDInfo(CONTROL_NAME);
        hud_info.setInfo(CONTROL_INFO);
    }

    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }

    private void resetCoroutines()
    {
        if (intro_coroutine != null)
        {
            StopCoroutine(intro_coroutine);
            intro_coroutine = null;
        }
        if (message_display_coroutine != null)
        {
            StopCoroutine(message_display_coroutine);
            intro_coroutine = null;
        }
    }

    public void setEnabled(bool enabled)
    {
        is_enabled = enabled;
        resetCoroutines();

        briefing_console_options.briefing_console_facial_recognition_display.transform.GetChild(1).gameObject.SetActive(!enabled);
        briefing_console_options.briefing_console_facial_recognition_display.transform.GetChild(2).gameObject.SetActive(enabled);

        if (is_enabled == true)
        {
            current_page = 0;
            intro_coroutine = StartCoroutine(intro());
        }
        else
        {
            hideAll();
            briefing_console_report_display.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>().fillAmount = 0.0f;
            briefing_console_report_display.transform.GetChild(2).gameObject.SetActive(true);
            briefing_console_report_display.transform.GetChild(2).GetComponent<UnityEngine.UI.Image>().fillAmount = 0.0f;
            briefing_console_options.deactivate();
        }
    }

    public bool getEnabled()
    {
        return is_enabled;
    }

    public int getCurrentPage()
    {
        return current_page;
    }

    private void hideAll()
    {
        for (int i = 3; i < briefing_console_report_display.transform.childCount; i++)
        {
            briefing_console_report_display.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void displayPage()
    {
        hideAll();
        resetCoroutines();
        briefing_console_report_display.transform.GetChild(4).gameObject.SetActive(true);
        briefing_console_report_display.transform.GetChild(4).GetComponent<TMP_Text>().SetText("PAGE " + (current_page + 1).ToString());
        briefing_console_report_display.transform.GetChild(5).gameObject.SetActive(true);
        briefing_console_report_display.transform.GetChild(5).GetComponent<TMP_Text>().SetText("");
        message_display_coroutine = StartCoroutine(messageReadout());
    }

    IEnumerator messageReadout()
    {
        string message = "";
        for (int i = 0; i < PAGE_MESSAGES[current_page].Length; i++)
        {
            for (int s = 0; s < PAGE_MESSAGES[current_page][i].Length; s++)
            {
                message += PAGE_MESSAGES[current_page][i][s];
                briefing_console_report_display.transform.GetChild(5).GetComponent<TMP_Text>().SetText(message);
                yield return new WaitForSeconds(0.03f);
            }
            message += "\n";
        }
        message_display_coroutine = null;
    }

    public void next()
    {
        if (message_deleted == true)
        {
            return;
        }

        current_page++;
        if (current_page > (PAGE_MESSAGES.Length - 1))
        {
            current_page = PAGE_MESSAGES.Length - 1;
        }
        if (current_page ==  PAGE_MESSAGES.Length - 2)
        {
            completeMessage();
        }
        displayPage();
    }

    public void back()
    {
        if (message_deleted == true)
        {
            return;
        }

        current_page--;
        if (current_page < 0)
        {
            current_page = 0;
        }
        displayPage();
    }

    public void completeMessage() //called when reached the second-to-last page (sets 10-minute countdown to deletion and unlocks door)
    {
        if (message_completed == true)
        {
            return;
        }

        //start auto-delete delay
        StartCoroutine(autoDeleteDelay());

        //enable door
        briefing_room_exit_door.transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;
        briefing_room_exit_door.transform.GetChild(1).GetComponent<BoxCollider>().enabled = false;
        briefing_room_exit_door.transform.GetChild(2).gameObject.SetActive(true);

        //change door locked symbol
        briefing_room_exit_door.transform.GetChild(3).GetComponent<MeshRenderer>().material = ReferenceAssistor.Instance.pure_black;
        briefing_room_exit_door.transform.GetChild(4).GetComponent<MeshRenderer>().material = ReferenceAssistor.Instance.lit_green;
        briefing_room_exit_door_display.transform.GetChild(1).gameObject.SetActive(false);
        briefing_room_exit_door_display.transform.GetChild(2).gameObject.SetActive(true);
    }

    IEnumerator autoDeleteDelay()
    {
        yield return new WaitForSeconds(DELETE_DELAY);

        hideAll();
        message_deleted = true;
        resetCoroutines();
        briefing_console_report_display.transform.GetChild(6).gameObject.SetActive(is_enabled);
        briefing_console_options.deactivate();
    }

    IEnumerator intro()
    {
        float anim_time = 1.0f;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            float fill_amount = 1.0f - (anim_time / 1.0f);
            briefing_console_report_display.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>().fillAmount = fill_amount;
            briefing_console_report_display.transform.GetChild(2).GetComponent<UnityEngine.UI.Image>().fillAmount = fill_amount;

            yield return null;
        }

        yield return new WaitForSeconds(0.25f);

        briefing_console_report_display.transform.GetChild(2).gameObject.SetActive(false);
        if (message_deleted == true)
        {
            //if deleted, show no messages and stop
            briefing_console_report_display.transform.GetChild(6).gameObject.SetActive(true);
            intro_coroutine = null;
            yield break;
        }
        
        briefing_console_report_display.transform.GetChild(3).gameObject.SetActive(true);

        anim_time = 1.0f;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            briefing_console_report_display.transform.GetChild(3).GetChild(2).GetComponent<UnityEngine.UI.Image>().fillAmount = 1.0f - (anim_time / 1.0f);

            yield return null;
        }

        yield return new WaitForSeconds(0.25f);

        briefing_console_report_display.transform.GetChild(3).gameObject.SetActive(false);
        displayPage();
        briefing_console_options.activate();

        intro_coroutine = null;
    }
}