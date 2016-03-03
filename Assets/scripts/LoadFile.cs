using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class LoadFile : MonoBehaviour 
{
    #region declarations
    public static Vector3 Min = new Vector3(1000,1000,1000);
    public static Vector3 Max = new Vector3(-1000, -1000, -1000);
	public static StlInterpreter stlInterpreter = new StlInterpreter();
	public Material Mat;
	public static bool loading = false;
	private bool drawn = true;
	private int vertexCount = 0;
	public static List <Vector3> currentVertices = new List<Vector3>();
	[HideInInspector]
	public List<Vector3> vertices = new List<Vector3> ();
    public static Dictionary<int, bool> LaserOnModelRef = new Dictionary<int, bool>();
    public static List<int> model_code_xrefSTL = new List<int>();
	public static int firstStlLineInCode = 0;
	public static List<string> stlCode = new List<string>();
	public static bool stlCodeLoaded = false;
    public static Transform stlHolder;
    private enum Type {STL,DMC,JOB,GCD,AMF,CS,CCT}
	private Type type;
	public static float stlScale = 1f;
    public static float runTime = 0;
    public static float playbackTime = 0;
    public static float playbackStartTime;
    public static float maxPlaybackTime = 0;
    public static bool playback = false;

    private GUIStyle windowStyle = new GUIStyle ();
	[DllImport("user32.dll")]
	private static extern void OpenFileDialog();
	[DllImport("user32.dll")]
	private static extern void ShowDialog ();
    [DllImport("user32.dll")]
    private static extern void SaveFileDialog();
    MakeMesh MM;
    #endregion

    public void Start()
	{
		stlHolder = GameObject.Find ("stlHolder").transform;
        var tbSpace = 20;
        GetComponent<InspectorL>().MainRect = new Rect(5, tbSpace, 250, 570);
        windowStyle.fontSize = 50;
        MM = GameObject.Find("MESH").GetComponent<MakeMesh>();
        MM.material = Mat;
        MM.Begin();
    }

    void OnApplicationQuit()
    {
    }
	public void loadFile()
	{
		loading = true;
        vertices.Clear();
		System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog ();
		openFileDialog.InitialDirectory = Application.dataPath + "/Samples/CCAT Tests";
        var sel = "";
        if (!stlCodeLoaded)
            sel += "STL Files (*.STL)|*.STL|";
        sel = sel.TrimEnd('|');
        openFileDialog.Filter = sel;
        openFileDialog.FilterIndex = 1;
		openFileDialog.RestoreDirectory = false;

		if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		{
			try
			{
				var fileName = openFileDialog.FileName;
				if (fileName.EndsWith("STL"))
					type = Type.STL;
                if (fileName != null)
				{
					var reader = new StreamReader(fileName);
					while (!reader.EndOfStream)
					{
						string line = reader.ReadLine();
						switch (type)
						{
						case Type.STL:
							scanSTL(line);
							break;
                        default:
							break;
						}
					}
                    var il = GetComponent<InspectorL>();
					switch (type)
					{
					    case Type.STL:
                            InspectorL.lastLoaded = InspectorL.LastLoaded.STL;
                            GameObject.Find("MESH").GetComponent<MakeMesh>().MergeMesh();
                            InspectorL.stlTimeSlider = MM.GetMesh().vertices.Length / 3 - 1;
                            InspectorL.stlTimeSliderMin = 0;
                            InspectorL.stlVisSlider = 1;
                            stlCodeLoaded = true;
                            break;
                        default:
						    break;
					}
                    var camPos = Camera.main.transform.position;
                    camPos.z = Min.z * 8.0f;
                    Camera.main.transform.position = camPos;
					loading = false;
					drawn = false;
				}
			}
			catch {}
		}
	}

	void OnGUI()
	{     
        var groupRectL = GetComponent<InspectorL>().MainRect;
        GUI.Window(0, groupRectL, GetComponent<InspectorL>().InspectorWindowL, "Inspector");
    }

	void scanSTL(string _line)
    {
        //if (_line == "\r\n") return;
        _line = _line.Trim();
		stlCode.Add (_line.ToString () + "\r\n");
        var chunks = _line.Split(' ');
		if (_line.Contains ("outer")) 
		{
			currentVertices.Clear();
            stlInterpreter.outerloop();
        }
		else if (_line.Contains("endloop"))
		{
			stlInterpreter.endloop(_line);
		}
		else if (_line.Contains("vertex"))
		{
			stlInterpreter.vertex(_line);
		}

        else if (_line.Contains("normal"))
        {
            stlInterpreter.normal(_line);
        }
    }
}
