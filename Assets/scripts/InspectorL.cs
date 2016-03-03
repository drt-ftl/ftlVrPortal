using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

public class InspectorL : MonoBehaviour
{
    #region declarations
    private Rect mainRect;
    private Rect thisRect;
    private Vector2 scrollPosition;

    public static float stlTimeSlider = 1;
    public static float stlTimeSliderPrev = 1;
    public static float stlTimeSliderMin = 1;
    private int typeInt = 0;
    private string[] toolbarStrings = { "STL"};
    public static Color stlColor = new Color(1.0f, 0.5f, 0.5f, 1f);
    private Vector3 _p1;
    private Vector3 _p2;
    private Vector3 _p3;
    private float _p1x;
    private float _p1y;
    private float _p1z;
    private float _p2x;
    private float _p2y;
    private float _p2z;
    private float _p3x;
    private float _p3y;
    private float _p3z;
    private List<string> prevTypeList = new List<string>();
    public enum LastLoaded { STL,None};
    public static LastLoaded lastLoaded;
    int margin = 10;
    int max = 50;
    public static GUIStyle KeyStyle;
    public static GUIStyle KeyStyleR;

    public static float stlVisSlider = 1;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    #endregion

    void Start()
    {
        KeyStyle = new GUIStyle();
        KeyStyle.fontSize = 9;
        KeyStyle.fontStyle = FontStyle.Italic;
        KeyStyle.normal.textColor = Color.black;

        KeyStyleR = new GUIStyle(KeyStyle);
        KeyStyleR.alignment = TextAnchor.UpperRight;
    }

    public Rect MainRect
    {
        get { return mainRect; }
        set
        {
            mainRect = value;
            thisRect = new Rect(margin, margin, mainRect.width - (margin * 2), mainRect.height - (margin * 2));
        }
    }
    public void InspectorWindowL(int id)
    {
        var availableToggles = GetAvailableToggles();
        GUILayout.BeginArea(thisRect);
        {
            GUILayout.Space(25);
            GUILayout.Space(10);
            // Load
            if ((!LoadFile.stlCodeLoaded))
            {
                if (GUILayout.Button("Load"))
                {
                    //MessageBox(new IntPtr(0), "Hello World!", "Hello Dialog", 3);
                    
                    LoadFile.loading = true;
                    GetComponent<LoadFile>().loadFile();
                }
            }

            if (availableToggles != null)
            {
                // Toggle File Type
                typeInt = GUILayout.Toolbar(typeInt, availableToggles.ToArray(), GUILayout.Width(220));

                // Visibility
                VisibiltySlider();
                GUILayout.Space(18);

                //Code Panel
                if (GetCode() != null)
                {
                    GUILayout.Box("<color=lime>" + GetCode()[0] + "</color>" + GetCode()[1], GUILayout.Width(225), GUILayout.Height(180));
                    GUILayout.Space(10);
                }

                // Time Sliders
                TimeSliders();

                //Coordinates
                CoordinateBoxes();

                // Clear
                GUILayout.Space(8);
                switch (GetAvailableToggles()[typeInt])
                {
                    case "STL":
                        if (GUILayout.Button("Clear STL"))
                        {
                            ClearSTL(0);
                            ClearSTL(1);
                        }
                        break;
                    default:
                        break;
                }
            }
            // Quit
            if (GUILayout.Button("Quit"))
            {
                Application.Quit();
            }
        }
        GUILayout.EndArea();
    }

    public void ClearSTL(int i)
    {
        if (i == 0)
        {
            LoadFile.stlCodeLoaded = false;
            GameObject.Find("MESH").GetComponent<MakeMesh>().ClearAll();
        }
        else
            GameObject.Find("MESH").GetComponent<MakeMesh>().Begin();
    }

    public void VisibiltySlider()
    {
        if (GetAvailableToggles() == null) return;
        switch (GetAvailableToggles()[typeInt])
        {
            case "STL":
                stlVisSlider = GUILayout.HorizontalSlider(stlVisSlider, 0, 1, GUILayout.Width(220), GUILayout.Height(12));
                GUILayout.Label("Visibility: " + (stlVisSlider * 100).ToString("f0"), KeyStyle, GUILayout.Width(250));
                stlColor.a = stlVisSlider;

                break;
            default:
                break;
        }
    }
    public void TimeSliders()
    {
        if (GetAvailableToggles() == null) return;
        switch (GetAvailableToggles()[typeInt])
        {
            case "STL":
                stlTimeSlider = GUILayout.HorizontalSlider(stlTimeSlider, 1, GameObject.Find("MESH").GetComponent<MakeMesh>().GetMesh().vertices.Length / 3 - 1, GUILayout.Width(220), GUILayout.Height(12));
                GUILayout.BeginHorizontal(GUILayout.Width(220));
                {
                    GUILayout.Label("Path # (Max): " + stlTimeSlider.ToString("f0"), KeyStyle, GUILayout.Width(110));
                    var stlCodeNumber = LoadFile.model_code_xrefSTL[(int)stlTimeSlider];
                    GUILayout.Label("Code Line #: " + stlCodeNumber.ToString(), KeyStyleR, GUILayout.Width(110));
                }
                GUILayout.EndHorizontal();
                stlTimeSliderMin = GUILayout.HorizontalSlider(stlTimeSliderMin, 0, stlTimeSlider, GUILayout.Width(220), GUILayout.Height(12));
                GUILayout.BeginHorizontal(GUILayout.Width(220));
                {
                    GUILayout.Label("Path # (Min): " + stlTimeSliderMin.ToString("f0"), KeyStyle, GUILayout.Width(110));
                    var stlCodeNumber = LoadFile.model_code_xrefSTL[(int)stlTimeSliderMin];
                    GUILayout.Label("Code Line #: " + stlCodeNumber.ToString(), KeyStyleR, GUILayout.Width(110));
                }
                GUILayout.EndHorizontal();
                if (stlTimeSliderMin >= stlTimeSlider && stlTimeSlider >= 0)
                    stlTimeSliderMin = stlTimeSlider;
                GUILayout.Label(scrollPosition.y.ToString(), KeyStyle, GUILayout.Width(250));
                if (stlTimeSlider != stlTimeSliderPrev)
                    scrollPosition.y = 0;
                stlTimeSliderPrev = stlTimeSlider;
                break;
            default:
                break;
        }
    }
    private List<string> GetAvailableToggles ()
    {
        var list = new List<string>();
        if (LoadFile.stlCodeLoaded)
            list.Add("STL");
        if (list.Contains(lastLoaded.ToString()))
        {
            typeInt = list.IndexOf(lastLoaded.ToString());
            lastLoaded = LastLoaded.None;
        }
        if (list.Count == 0)
            return null;
        return list;
    }
    private string[] GetCode ()
    {
        var code = new string[2];
        var first = "";
        var bulk = "";
        if (GetAvailableToggles() == null) return null;
        switch (GetAvailableToggles()[typeInt])
        {
            case "STL":
                max = 50;
                if (max > LoadFile.stlCode.Count - 1)
                    max = LoadFile.stlCode.Count - 1;
                if (stlTimeSlider >= LoadFile.firstStlLineInCode)
                {
                    var ts = (int)stlTimeSlider;
                    if (LoadFile.model_code_xrefSTL.Count - 1 < ts)
                        ts = LoadFile.model_code_xrefSTL.Count - 1;
                    var firstLineIndex = LoadFile.model_code_xrefSTL[ts];
                    first = LoadFile.stlCode[firstLineIndex];
                    for (int i = firstLineIndex + 1; i < firstLineIndex + max; i++)
                    {
                        if (LoadFile.stlCode.Count > i)
                            bulk += LoadFile.stlCode[i];
                    }
                }
                else
                {
                    first = LoadFile.stlCode[LoadFile.firstStlLineInCode];
                    for (int i = LoadFile.firstStlLineInCode + 1; i < (int)stlTimeSlider + max; i++)
                    {
                        if (LoadFile.stlCode.Count > i)
                            bulk += LoadFile.stlCode[i];
                    }
                }
                break;
            default:
                break;
        }
        code[0] = first;
        code[1] = bulk;
        return code;
    }
    private void CoordinateBoxes()
    {
        GUILayout.BeginHorizontal();
        if (GetAvailableToggles() == null) return;
        switch (GetAvailableToggles()[typeInt])
        {
            #region STL
            case "STL":
                var p = GameObject.Find("MESH").GetComponent<MakeMesh>().GetTriangleVertices((int)stlTimeSlider);
                var p1 = p[0];
                var p2 = p[1];
                var p3 = p[2];
                GUILayout.BeginVertical();
                GUILayout.Label("<size=10><color=black> Entity </color></size>");
                GUILayout.Label("<size=10><color=black> X </color></size>");
                GUILayout.Label("<size=10><color=black> Y </color></size>");
                GUILayout.Label("<size=10><color=black> Z </color></size>");
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("<size=10><color=black> Vertex 1</color></size>");
                GUILayout.TextField(p1.x.ToString("f3"));
                GUILayout.TextField(p1.y.ToString("f3"));
                GUILayout.TextField(p1.z.ToString("f3"));
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("<size=10><color=black> Vertex 2</color></size>");
                GUILayout.TextField(p2.x.ToString("f3"));
                GUILayout.TextField(p2.y.ToString("f3"));
                GUILayout.TextField(p2.z.ToString("f3"));
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("<size=10><color=black> Vertex 3</color></size>");
                GUILayout.TextField(p3.x.ToString("f3"));
                GUILayout.TextField(p3.y.ToString("f3"));
                GUILayout.TextField(p3.z.ToString("f3"));
                GUILayout.EndVertical();
                break;
            #endregion
            default:
                break;
        }
        GUILayout.EndHorizontal();
    }
    void Update()
    {
        if (!LoadFile.stlCodeLoaded) return;
        var availableToggles = GetAvailableToggles();
        if (availableToggles == null) return;
        if (availableToggles.Count - 1 < typeInt)
            typeInt = availableToggles.Count - 1;
        switch (availableToggles[typeInt])
        {
            case "STL":
                if (Input.GetKeyDown(KeyCode.UpArrow) && stlTimeSlider < GameObject.Find("MESH").GetComponent<MakeMesh>().GetMesh().vertices.Length / 3 - 1)
                    stlTimeSlider++;
                if (Input.GetKeyDown(KeyCode.DownArrow) && stlTimeSlider > 1)
                    stlTimeSlider--;
                break;
            default:
                break;
        }
    }
}

