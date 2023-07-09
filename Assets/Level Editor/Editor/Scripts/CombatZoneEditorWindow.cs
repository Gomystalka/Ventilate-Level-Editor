using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatZoneEditorWindow : ILevelEditorWindow
{
    public string Title => "Combat Zone Editor";

    public Texture2D Icon { get; set; }

    public byte Order => 2;

    public LevelEditorWindow OwnerWindow { get; set; }

    public GUIContent GetTitleContent()
    {
        return GUIContent.none;
    }

    public void OnGUI()
    {
    }

    public void OnDestroy()
    {
    }

    public void OnDisable()
    {
    }

    public void OnEnable()
    {
    }

    public void OnWindowOpened()
    {
        LevelEditorMessageSystem.Push("Opened Combat Zone Editor", 1f, LevelEditorMessageSystem.MessageType.Info);
    }

    public void OnWindowClosed()
    {

    }
}
