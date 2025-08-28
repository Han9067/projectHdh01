using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
public class CursorManager : AutoSingleton<CursorManager>
{
    public string cName = "default";
    Dictionary<string, Texture2D> cursorTextures = new Dictionary<string, Texture2D>();
    private void Awake()
    {
        string[] strArr = { "default", "attack", "notMove", "enter" };
        foreach (string str in strArr)
        {
            Texture2D cursorTexture = Resources.Load<Texture2D>($"Images/UI/Cursor/cursor_{str}");
            cursorTextures.Add(str, cursorTexture);
        }
    }
    public void SetCursor(string name)
    {
        cName = name;
        Cursor.SetCursor(cursorTextures[cName], Vector2.zero, CursorMode.Auto);
    }
    public bool IsCursor(string name)
    {
        return cName == name;
    }
}
