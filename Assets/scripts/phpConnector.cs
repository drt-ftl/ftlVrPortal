using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class phpConnector : MonoBehaviour
{
    //public static List<string> onlineText = new List<string>();
    public static List<string> usernames = new List<string>();
    public static List<string> modelPaths = new List<string>();
    public static List<string> imagePaths = new List<string>();
    public static string userTxt = "";
    public static string modelTxt = "";
    string currentUrl = "";


    public List<string> ListFiles(string urlRoot)
    {
        var list = new List<string>();
        WebRequest request = WebRequest.Create(urlRoot);
        WebResponse response = request.GetResponse();
        Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");

        using (var reader = new StreamReader(response.GetResponseStream()))
        {
            string result = reader.ReadToEnd();

            MatchCollection matches = regex.Matches(result);
            if (matches.Count == 0)
            {
                print("parse failed.");
                return list;
            }

            foreach (Match match in matches)
            {
                if (!match.Success || match.Groups["name"].ToString().Contains("Parent Directory")) { continue; }
                list.Add(match.Groups["name"].ToString());
            }
        }
        return list;
    }

    public string[] FilesArray(string urlRoot)
    {
        string[] arr;
        WebRequest request = WebRequest.Create(urlRoot);
        WebResponse response = request.GetResponse();
        //Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");
        Regex regex = new Regex("");

        using (var reader = new StreamReader(response.GetResponseStream()))
        {
            string result = reader.ReadToEnd();

            MatchCollection matches = regex.Matches(result);
            if (matches.Count == 0)
            {
                print("parse failed.");
                return null;
            }
            arr = new string[matches.Count];
            var i = 0;
            foreach (Match match in matches)
            {
                if (!match.Success
                    || match.Groups["name"].ToString().Contains("Parent Directory")
                    || match.Groups["name"].ToString().EndsWith(".png")) { continue; }
                arr[i] = (match.Groups["name"].ToString());
                i++;
            }
        }
        return arr;
    }

    public void GetFile(string url)
    {
        currentUrl = url;
        StartCoroutine(GetRemoteTextFile(url));
    }

    public void GetModelsList(string type, string arguments, int page, string _u, string _pw)
    {
        var url = camScript.baseUrl + "unityAccess.php?title=" + type + "|" + arguments + "|" + page.ToString() + "|" + _u + "|" + _pw;
        print(url);
        currentUrl = url;
        StartCoroutine(GatherRemoteFileList(url));
    }

    public void GetUsersList(string type, string arguments, int page, string _u, string _pw)
    {
        var url = camScript.baseUrl + "unityAccess.php?title=" + type + "|" + "_" + "|" + page.ToString() + "|" + _u + "|" + _pw;
        currentUrl = url;
        StartCoroutine(GatherRemoteUserList(url));
    }

    public void StopCheck()
    {
        StopCoroutine(GetRemoteTextFile(currentUrl));
    }

    private IEnumerator GetRemoteTextFile(string url)
    {
        WWW w = new WWW(url);
        yield return w;
        if (w.error != null)
        {
            Debug.Log("Error .. " + w.error);
        }
        else
        {
            modelTxt = w.text;
            if (modelTxt.Contains("|EOL"))
            {
                camScript.EOL = true;
            }
            else
                camScript.EOL = false;
            var lines = modelTxt.Split("\r\n"[0]);

            foreach (var line in lines)
            {
                //Camera.main.GetComponent<camScript>().scanSTL(line);
            }
            //GameObject.Find("MESH").GetComponent<MakeMesh>().MergeMesh();
        }
        StopCheck();
    }

    public void StopGather()
    {
        StopCoroutine(GetRemoteTextFile(currentUrl));
    }

    private IEnumerator GatherRemoteFileList(string url)
    {
        WWW w = new WWW(url);
        yield return w;
        if (w.error != null)
        {
            Debug.Log("Error .. " + w.error);
        }
        else
        {

            modelTxt = w.text;
            modelPaths.Clear();
            imagePaths.Clear();
            var lines = modelTxt.Split('|');
            foreach (var line in lines)
            {
                if (line == "") continue;
                if (!line.EndsWith("|EOL"))
                    GameObject.Find("NEXT").GetComponent<Button>().interactable = true;
                print("line: " + line);
                var p = line.Split('*');
                var m1 = p[0].Split('/');
                var mCount = m1.Length;
                if (mCount < 2) continue;
                var mFolder = WWW.EscapeURL(m1[mCount - 2]);
                var mFile = WWW.EscapeURL(m1[mCount - 1]);

                var i1 = p[1].Split('/');
                var iCount = i1.Length;
                var iFolder = WWW.EscapeURL(i1[iCount - 2]);
                var iFile = WWW.EscapeURL(i1[iCount - 1]);
                p[0] = p[0].Replace(m1[mCount - 2], mFolder);
                p[1] = p[1].Replace(i1[iCount - 2], iFolder);

                modelPaths.Add(p[0]);
                imagePaths.Add(p[1]);
            }
            Camera.main.GetComponent<camScript>().PhpBrowse(modelPaths, imagePaths);
        }
        StopGather();
    }

    private IEnumerator GatherRemoteUserList(string url)
    {
        WWW w = new WWW(url);
        yield return w;
        if (w.error != null)
        {
            Debug.Log("Error .. " + w.error);
        }
        else
        {

            userTxt = w.text;
            modelPaths.Clear();
            imagePaths.Clear();
            usernames.Clear();
            var lines = userTxt.Split('|');
            foreach (var line in lines)
            {
                if (line == "") continue;
                if (!line.EndsWith("|EOL"))
                    GameObject.Find("NEXT").GetComponent<Button>().interactable = true;
                var p = line.Split('*');
                if (p.Length < 2) continue;
                var username = p[0];
                var imagePath = p[1];

                usernames.Add(username);
                imagePaths.Add(imagePath);
            }
            Camera.main.GetComponent<camScript>().PhpBrowseUsers(usernames, imagePaths);
        }
        StopGather();
    }
}