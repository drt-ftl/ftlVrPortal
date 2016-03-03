/* WebTextFileChecker.cs
This checks the text in a text file on the web - believe it!
*/

using UnityEngine;
using System;
using System.Net;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class WebTextFileChecker : MonoBehaviour
{
    public static string modelTxt = "";
    string currentUrl = "";

    void Start()
    {
    }

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
        StartCoroutine(Check(url));
    }

    public void StopCheck()
    {
        StopCoroutine(Check(currentUrl));
    }

    private IEnumerator Check(string url)
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
            var lines = modelTxt.Split("\r\n"[0]);


            foreach (var line in lines)
            {
                Camera.main.GetComponent<camScript>().scanSTL(line);
            }
            GameObject.Find("MESH").GetComponent<MakeMesh>().MergeMesh();
        }
        StopCheck();
    }
}