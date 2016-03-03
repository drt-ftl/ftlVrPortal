using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class camScript : MonoBehaviour
{
    public GameObject button;
    public GameObject onlineButton;
    public GameObject userButton;
    public GameObject cursor;
    public Material Mat;
    public Dropdown materialDD;
    public Dropdown skyboxDD;
    public Dropdown landscapeDD;
    public Dropdown propDD;
    public static string baseUrl = "http://www.ftllabscorp.com/VR/";
    private GameObject _cursor;
    public static MakeMesh MM;

    public GameObject mainMenu;
    public static GameObject folderWindow;
    public static GameObject sceneWindow;
    private List<GameObject> fileButtons = new List<GameObject>();
    private StlInterpreter stlInterpreter;
    public static List<Vector3> currentVertices = new List<Vector3>();
    public static Vector3 Min = new Vector3(1000, 1000, 1000);
    public static Vector3 Max = new Vector3(-1000, -1000, -1000);
    public List<Material> skyboxes = new List<Material>();
    public List<GameObject> landscapes = new List<GameObject>();
    public List<GameObject> props = new List<GameObject>();
    public static GameObject currentLandscape;
    public static GameObject currentProp;
    private float mouseYoffset = 0;
    private WebTextFileChecker OnlineBrowser;
    public phpConnector phpConn;
    private int activePageNumber = 0;
    public static string BBT = "myModels";
    public static string ARG = "a";
    public static int PG = 0;
    public static bool EOL = false;
    bool Online = false;
    public static List<string> history = new List<string>();
    public Text browseLabel;

    public GameObject loginPanel;
    public InputField usernameBox;
    public InputField pwBox;
    bool loggedIn = false;
    public static string USERNAME = "";
    public static string PASSWORD = "";

    void Start ()
    {
        stlInterpreter = new StlInterpreter();
        folderWindow = GameObject.Find("Folder Window");
        sceneWindow = GameObject.Find("Scene Window");
        OnlineBrowser = GameObject.Find("Online Browser").GetComponent<WebTextFileChecker>();

        folderWindow.GetComponent<PanelFades>().FadeOut();
        sceneWindow.GetComponent<PanelFades>().FadeOut();
        _cursor = Instantiate(cursor) as GameObject;
        _cursor.transform.SetParent(mainMenu.transform);
        var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y + mouseYoffset, -1f));
        var rot = new Quaternion(0, 0, 0, 0);
        _cursor.transform.localPosition = pos;
        _cursor.transform.localRotation = rot;
        MM = GameObject.Find("MESH").GetComponent<MakeMesh>();
        MM.material = Mat;
        MM.Begin();

        for (int i = 0; i < landscapes.Count; i++)
        {
            var l = Instantiate(landscapes[i]);
            l.name = landscapes[i].name;
            l.SetActive(false);
            landscapes[i] = l;
        }

        for (int i = 0; i < props.Count; i++)
        {
            var p = Instantiate(props[i]);
            p.name = props[i].name;
            p.SetActive(false);
            props[i] = p;
        }

        var options = new List<Dropdown.OptionData>(skyboxes.Count);
        foreach (var skybox in skyboxes)
        {
            var newOption = new Dropdown.OptionData(skybox.name);
            options.Add(newOption);
        }
        skyboxDD.options = options;

        options = new List<Dropdown.OptionData>(MM.materials.Count);
        foreach (var mat in MM.materials)
        {
            var newOption = new Dropdown.OptionData(mat.name);
            options.Add(newOption);
        }
        materialDD.options = options;

        options = new List<Dropdown.OptionData>(landscapes.Count);
        var none = new Dropdown.OptionData("None");
        options.Add(none);
        foreach (var landscape in landscapes)
        {
            var newOption = new Dropdown.OptionData(landscape.name);
            options.Add(newOption);
        }
        landscapeDD.options = options;

        options = new List<Dropdown.OptionData>(props.Count);
        options.Add(none);
        foreach (var prop in props)
        {
            var newOption = new Dropdown.OptionData(prop.name);
            options.Add(newOption);
        }
        propDD.options = options;
        currentProp = null;



        materialDD.onValueChanged.AddListener(delegate {
            MaterialDropdownValueChangedHandler(materialDD);
        });

        skyboxDD.onValueChanged.AddListener(delegate {
            SkyboxDropdownValueChangedHandler(skyboxDD);
        });

        landscapeDD.onValueChanged.AddListener(delegate {
            LandscapeDropdownValueChangedHandler(landscapeDD);
        });

        propDD.onValueChanged.AddListener(delegate {
            PropDropdownValueChangedHandler(propDD);
        });

        RenderSettings.skybox = skyboxes[0];
        landscapes[0].SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ShowPanel();
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();
        if (Input.GetKeyDown(KeyCode.L))
            loginPanel.GetComponent<PanelFades>().FadeIn();
    }

	void OnGUI()
    {
        
        _cursor.transform.SetParent(GameObject.Find("Canvas").transform);
        var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y + mouseYoffset, 1.5f));
        var rot = new Quaternion(0, 0, 0, 0);
        _cursor.transform.position = pos;
        _cursor.transform.localRotation = rot;
    }

    void ShowPanel()
    {
        if (!mainMenu.active)
            mainMenu.active = true;
        if (mainMenu.GetComponent<PanelFades>().Visible() 
            || folderWindow.GetComponent<PanelFades>().Visible() 
            || sceneWindow.GetComponent<PanelFades>().Visible()
            || loginPanel.GetComponent<PanelFades>().Visible())
        {
            mainMenu.GetComponent<PanelFades>().FadeOut();
            folderWindow.GetComponent<PanelFades>().FadeOut();
            sceneWindow.GetComponent<PanelFades>().FadeOut();
            loginPanel.GetComponent<PanelFades>().FadeOut();
        }
        else
            mainMenu.GetComponent<PanelFades>().FadeIn();
    }

    public void Login()
    {
        USERNAME = usernameBox.text;
        PASSWORD = pwBox.text;
        loginPanel.GetComponent<PanelFades>().FadeOut();
    }

    void CursorControl()
    {

    }

    public void browseFiles()
    {
        foreach (var b in fileButtons)
        {
            Destroy(b);
        }
        fileButtons.Clear();
        Online = false;
        var path = Application.dataPath + "/Models";
        var filenamesArray = Directory.GetFiles(path);
        var filenamesList = new List<string>();
        foreach(var a in filenamesArray)
        {
            filenamesList.Add(a);
        }
        CommonBrowse(filenamesList, false);
    }

    public void browseFilesOnline_specific(bool userList)
    {
        foreach (var b in fileButtons)
        {
            Destroy(b);
        }
        fileButtons.Clear();
        Online = true;
        if (userList)
        {
            phpConn.GetUsersList("byUser", ARG, PG, USERNAME, PASSWORD);
        }
        else phpConn.GetModelsList(BBT, ARG, PG, USERNAME, PASSWORD);            
    }

    public void general()//(string type, string arguments, int page)
    {
        BBT = "general";
        ARG = "_";
        browseFilesOnline_specific(false);
        browseLabel.text = "Public";
    }

    public void myModels()//(string type, string arguments, int page)
    {
        BBT = "myModels";
        ARG = "_";
        browseFilesOnline_specific(false);
        browseLabel.text = "My Models";
    }

    public void UserList()//(string type, string arguments, int page)
    {
        BBT = "userList";
        ARG = "_";
        browseFilesOnline_specific(true);
        browseLabel.text = "Users";
    }

    public void Favorites()//(string type, string arguments, int page)
    {
        BBT = "favorites";
        browseFilesOnline_specific(false);
        browseLabel.text = "Favorites";
    }

    public void ByUser(GameObject _gameObject)//(string type, string arguments, int page)
    {
        var u = _gameObject.name.Replace(" ", "_");
        BBT = "byUser";
        ARG = u;
        browseFilesOnline_specific(false);
    }

    public void PhpBrowse(List<string> modelPaths, List<string> imagePaths)
    {
        fileButtons.Clear();
        folderWindow.GetComponent<PanelFades>().FadeIn();
        mainMenu.GetComponent<PanelFades>().FadeOut();
            for (int i = 0; i < modelPaths.Count ; i++)
            {
                if (i >= modelPaths.Count) return;
                var newButton = Instantiate(onlineButton) as GameObject;
                newButton.name = modelPaths[i];
                newButton.transform.SetParent(folderWindow.transform);
                newButton.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
                newButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
                var pos = new Vector3(0, 60 - i * 60, 0);
                pos.y += 60;
                var rot = new Quaternion(0, 0, 0, 0);
                newButton.transform.localScale = Vector3.one;
                newButton.transform.localRotation = rot;
                newButton.transform.localPosition = pos;
                newButton.GetComponentInChildren<Text>().text = "  " + Path.GetFileName(modelPaths[i]).Replace("_", " ");
                newButton.GetComponent<ButtonScript>().GO(true, imagePaths[i]);
                
                fileButtons.Add(newButton);
            }
            if (PG == 0)
                GameObject.Find("PREV").GetComponent<Button>().interactable = false;
    }

    public void PhpBrowseUsers(List<string> modelPaths, List<string> imagePaths)
    {
        fileButtons.Clear();
        folderWindow.GetComponent<PanelFades>().FadeIn();
        mainMenu.GetComponent<PanelFades>().FadeOut();
        for (int i = 0; i < modelPaths.Count; i++)
        {
            if (i >= modelPaths.Count) return;
            var newButton = Instantiate(userButton) as GameObject;
            newButton.name = modelPaths[i];
            newButton.transform.SetParent(folderWindow.transform);
            newButton.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
            newButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
            var pos = new Vector3(0, 60 - i * 60, 0);
            pos.y += 60;
            var rot = new Quaternion(0, 0, 0, 0);
            newButton.transform.localScale = Vector3.one;
            newButton.transform.localRotation = rot;
            newButton.transform.localPosition = pos;
            newButton.GetComponentInChildren<Text>().text = "  " + Path.GetFileName(modelPaths[i]).Replace("_", " ");
            newButton.GetComponent<ButtonScript>().GO(true, imagePaths[i]);

            fileButtons.Add(newButton);
        }
        if (PG == 0)
            GameObject.Find("PREV").GetComponent<Button>().interactable = false;
    }

    void CommonBrowse(List<string> filenamesList, bool online)
    {
        browseLabel.text = "Local";
        foreach (var b in fileButtons)
        {
            Destroy(b);
        }
        fileButtons.Clear();
        folderWindow.GetComponent<PanelFades>().FadeIn();
        mainMenu.GetComponent<PanelFades>().FadeOut();
        var filesPerPage = 5;

        var filenames = new List<string>();
        foreach (var fna in filenamesList)
        {
            if (fna.EndsWith(".STL") || fna.EndsWith(".stl"))
                filenames.Add(fna);
        }
        var numFiles = filenames.Count;
        var numPages = Mathf.CeilToInt(numFiles / filesPerPage);
        for (int i = (PG * filesPerPage); i < (PG * filesPerPage) + filesPerPage; i++)
        {
            var index = i - (PG * filesPerPage);
            if (i >= filenames.Count)
            {
                GameObject.Find("NEXT").GetComponent<Button>().interactable = false;
                return;
            }
            GameObject newButton;
            if (!online)
                newButton = Instantiate(button) as GameObject;
            else
                newButton = Instantiate(onlineButton) as GameObject;
            newButton.name = filenames[i];
            newButton.transform.SetParent(folderWindow.transform);
            newButton.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
            newButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
            var pos = new Vector3(0, 60 - index * 60, 0);
            pos.y += 60;
            var rot = new Quaternion(0, 0, 0, 0);
            newButton.transform.localScale = Vector3.one;
            newButton.transform.localRotation = rot;
            newButton.transform.localPosition = pos;
            newButton.GetComponentInChildren<Text>().text = "  " + Path.GetFileName(filenames[i]);
            newButton.GetComponent<ButtonScript>().GO(false, "");

            fileButtons.Add(newButton);
        }

        //DRT THURSDAY ADDING BBT, ARG, PG
        //NEED TO HAVE PHP PAGE SEND NOTICE IF LAST PAGE TO DISABLE NEXT BUTTON

        if (PG == 0)
            GameObject.Find("PREV").GetComponent<Button>().interactable = false;
    }

    public void loadFile(string fileName)
    {
        stlInterpreter.ClearAll();
        print("LOADING");
        var reader = new StreamReader(fileName);

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            scanSTL(line);
        }
        GameObject.Find("MESH").GetComponent<MakeMesh>().MergeMesh();
    }

    public void loadFileFromWeb(string fileName)
    {
        stlInterpreter.ClearAll();
        OnlineBrowser.GetFile(fileName);
        print(fileName);
    }

    public void scanSTL(string _line)
    {
        _line = _line.Trim();
        if (_line.Contains("outer"))
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

    public void EditScene()
    {
        sceneWindow.GetComponent<PanelFades>().FadeIn();
        folderWindow.GetComponent<PanelFades>().FadeOut();
        mainMenu.GetComponent<PanelFades>().FadeOut();
    }

    private void MaterialDropdownValueChangedHandler(Dropdown target)
    {
        MM.ChangeMaterial(target.value);
    }

    private void SkyboxDropdownValueChangedHandler(Dropdown target)
    {
        RenderSettings.skybox = skyboxes[target.value]; 
    }

    private void LandscapeDropdownValueChangedHandler(Dropdown target)
    {
        var tmp = currentLandscape;
        if (target.value == 0)
        {
            currentLandscape = null;
        }
        else
        {
            currentLandscape = landscapes[target.value - 1];
            currentLandscape.SetActive(true);
        }
        if (tmp != null)
            tmp.SetActive(false);
    }

    private void PropDropdownValueChangedHandler(Dropdown target)
    {
        var tmp = currentProp;
        if (target.value == 0)
        {
            currentProp = null;
        }
        else
        {
            currentProp = props[target.value - 1];
            currentProp.SetActive(true);
        }
        if (tmp != null)
            tmp.SetActive(false);
    }

    public void NextPage()
    {
            foreach (var button in fileButtons)
            {
                    var pos = button.transform.localPosition;
                    pos.z = -5000;
                    button.transform.localPosition = pos;
            }
            PG++;
            if (PG > 0)
                GameObject.Find("PREV").GetComponent<Button>().interactable = true;
            if (PG > fileButtons.Count - 2)
                GameObject.Find("NEXT").GetComponent<Button>().interactable = false;
            foreach (var button in fileButtons)
            {
                var pos = button.transform.localPosition;
                pos.z = 0;
                button.transform.localPosition = pos;
            }
            if (Online)
            {
                if (BBT == "userList")
                    browseFilesOnline_specific(true);
                else
                    browseFilesOnline_specific(false);
            }
            else
                browseFiles();        
    }

    public void PrevPage()
    {
        if (PG > 0)
        {
            foreach (var button in fileButtons)
            {
                    var pos = button.transform.localPosition;
                    pos.z = -5000;
                    button.transform.localPosition = pos;
            }
            PG--;
            if (PG <= 0)
                GameObject.Find("PREV").GetComponent<Button>().interactable = false;
            //DRT THURSDAY LEFT OFF HERE ALSO
            GameObject.Find("NEXT").GetComponent<Button>().interactable = true;
            foreach (var button in fileButtons)
            {
                var pos = button.transform.localPosition;
                pos.z = 0;
                button.transform.localPosition = pos;
            }
            if (Online)
            {
                if (BBT == "userList")
                    browseFilesOnline_specific(true);
                else
                    browseFilesOnline_specific(false);
            }
            else
                browseFiles();
        }
    }
}
