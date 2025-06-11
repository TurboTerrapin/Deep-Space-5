/*
    UniversalCommunicator.cs
    - Handles inputs for communicator keyboard
    - Displays to code screen
    Contributor(s): Jake Schott
    Last Updated: 5/17/2025
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class UniversalCommunicator : NetworkBehaviour
{
    //CLASS CONSTANTS
    private static Color[] COLOR_OPTIONS = new Color[4] { new Color(0f, 0.84f, 1f), new Color(0.129f, 1f, 0.04f), new Color(0.69f, 0f, 0.69f), new Color(0.84f, 0.62f, 0f) };
    private static float POINTER_SPEED = 0.2f;

    public List<GameObject> character_displays = null;
    public GameObject code_display;

    private static List<char> string_code_index = new List<char>{'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';'};

    private List<int> code_index = new List<int>(); //0-11, corresponds to A0-A5, B0-B5 where B5 is 11 and A0 is 0
    private List<int> code_is_numeric = new List<int>(); //1 means number (ex. 5), 0 is symbol (ex. square)
    private List<int> code_color = new List<int>(); //0 is blue, 1 is green, 2 is pink, 3 is orange

    private Coroutine pointer_shift_coroutine = null;

    IEnumerator shiftPointer()
    {
        float animation_time = POINTER_SPEED;

        float starting_x = code_display.transform.GetChild(25).transform.localPosition.x;
        float dest_x = Mathf.Lerp(-0.14f, 0.14f, (1.0f - code_index.Count / 7.0f));

        //move pointer
        while (animation_time > 0.0f)
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
            animation_time = Mathf.Max(0.0f, animation_time - dt);
            code_display.transform.GetChild(25).transform.localPosition =
                new Vector3(Mathf.Lerp(starting_x, dest_x, 1.0f - (animation_time / POINTER_SPEED)),
                            0.0283f,
                            0.0f);

            yield return null;
        }

        pointer_shift_coroutine = null;
    }

    public void resetDisplay()
    {
        transmitResetUpdateRPC();
    }

    public void inputCharacter(int index)
    {
        if (code_index.Count < 8)
        {
            code_index.Add(index);
            code_color.Add(transform.gameObject.GetComponent<ColorSelector>().getCurrColor());
            code_is_numeric.Add(transform.gameObject.GetComponent<SymbolToggle>().getIsNumeric());
            string index_as_code = "";
            string code_color_as_code = "";
            string code_is_numeric_as_code = "";
            for (int i = 0; i < code_index.Count; i++)
            {
                index_as_code += string_code_index[code_index[i]];
                code_color_as_code += code_color[i].ToString();
                code_is_numeric_as_code += code_is_numeric[i].ToString();
            }
            transmitCharacterUpdateRPC(index, index_as_code, code_color_as_code, code_is_numeric_as_code);
        }
    }
    public void updateDisplay()
    {
        //hide everything
        for (int i = 0; i <= 7; i++)
        {
            code_display.transform.GetChild(i + 1).gameObject.SetActive(false);
            code_display.transform.GetChild(i + 9).gameObject.SetActive(false);
        }
        
        //show current numbers/shapes
        for (int i = 0; i < code_index.Count; i++)
        {
            if (code_is_numeric[i] == 0) //symbol
            {
                code_display.transform.GetChild(i + 9).gameObject.GetComponent<UnityEngine.UI.RawImage>().texture = character_displays[code_index[i]].transform.GetChild(2).gameObject.GetComponent<RawImage>().texture;
                code_display.transform.GetChild(i + 9).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = COLOR_OPTIONS[code_color[i]];
                code_display.transform.GetChild(i + 9).gameObject.SetActive(true);
            }
            else //numeric
            {
                code_display.transform.GetChild(i + 1).gameObject.GetComponent<TMP_Text>().SetText(character_displays[code_index[i]].transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text);
                code_display.transform.GetChild(i + 1).gameObject.GetComponent<TMP_Text>().color = COLOR_OPTIONS[code_color[i]];
                code_display.transform.GetChild(i + 1).gameObject.SetActive(true);
            }
        }

        //adjust pointer
        if (code_index.Count < 8)
        {
            transform.gameObject.GetComponent<CharacterInput>().activate();
            code_display.transform.GetChild(25).gameObject.SetActive(true);

            if (pointer_shift_coroutine != null)
            {
                StopCoroutine(pointer_shift_coroutine);
            }

            pointer_shift_coroutine = StartCoroutine(shiftPointer());
        }
        else
        {
            transform.gameObject.GetComponent<CharacterInput>().deactivate();
            code_display.transform.GetChild(25).gameObject.SetActive(false);
        }
    }

    public List<int> getCodeIndexes()
    {
        return new List<int>(code_index);
    }

    public List<int> getCodeColors()
    {
        return new List<int>(code_color);
    }
    public List<int> getCodeIsNumeric()
    {
        return new List<int>(code_is_numeric);
    }

    [Rpc(SendTo.Everyone)]
    private void transmitResetUpdateRPC()
    {
        code_index.Clear();
        code_color.Clear();
        code_is_numeric.Clear();

        transform.gameObject.GetComponent<ResetDisplay>().pushResetButton();
    }

    [Rpc(SendTo.Everyone)]
    private void transmitCharacterUpdateRPC(int button_index, string indexes, string colors, string is_numeric)
    {
        int[] temp_code_index = indexes.ToIntArray();
        int[] temp_code_color = colors.ToIntArray();
        int[] temp_is_numeric = is_numeric.ToIntArray();

        code_index.Clear();
        code_color.Clear();
        code_is_numeric.Clear();

        for (int i = 0; i < indexes.Length; i++)
        {
            code_index.Add(temp_code_index[i] - 48);
            code_color.Add(temp_code_color[i] - 48);
            code_is_numeric.Add(temp_is_numeric[i] - 48);
        }

        transform.gameObject.GetComponent<CharacterInput>().pushButton(button_index);
    }
}