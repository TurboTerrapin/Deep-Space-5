using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class HUDInfo
{
    private string control_name; //ex. "IMPULSE THROTTLE"
    private List<string> input_descriptions = null; //ex. "INCREASE"
    private List<int> corresponding_inputs = null; //based on primary_inputs in ControlScript  
    public HUDInfo(string title)
    {
        control_name = title;
    }

    public bool setInputs(List<string> new_descriptions, List<int> new_inputs)
    {
        input_descriptions = new_descriptions;
        corresponding_inputs = new_inputs;
        return true;
    }
    public string getName()
    {
        return control_name;
    }
    public int numOptions()
    {
        return input_descriptions.Count;
    }

    public List<string> getInputDescriptions()
    {
        return input_descriptions;
    }

    public List<int> getInputIndexes()
    {
        return corresponding_inputs;
    }

    public bool isInteractable()
    {
        return (numOptions() > 0);
    }
}
