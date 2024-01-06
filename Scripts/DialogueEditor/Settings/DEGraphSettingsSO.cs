using UnityEngine;

public class DEGraphSettingsSO : ScriptableObject
{
    [Header("Start Node")]
    public Color StartNodeTitleColour = new(56f / 255f, 56f / 255f, 56f / 255f);

    [Header("Dialogue Node")]
    public Color DialogueNodeTitleColour = new(56f / 255f, 56f / 255f, 56f / 255f);

    [Header("Action Node")]
    public Color ActionNodeTitleColour = new(56f / 255f, 56f / 255f, 56f / 255f);

    [Header("Dialogue TextField Syntax")]
    public string GenerateChoicesString = "->";
    public string GenerateActionString = "///";

    public void ResetValues()
    {
        Color defaultColour = new(56f / 255f, 56f / 255f, 56f / 255f);
        StartNodeTitleColour = defaultColour;
        DialogueNodeTitleColour = defaultColour;
        ActionNodeTitleColour = defaultColour;

        GenerateChoicesString = "->";
        GenerateActionString = "///";
    }
}
