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
            var a = w.bytes;
            if (CheckForStlBinary(a))
            {
                var parseStlBinary = new Parse_StlBinary("", camScript.MM, a);
            }

            else
            {
                print("FALZ");
                var line = w.text;

                line = line.Replace(" facet", "|facet");
                line = line.Replace("outer loop", "|outer loop");
                line = line.Replace("endloop", "|endloop");
                line = line.Replace("vertex", "|vertex");
                line = line.Replace("endfacet", "|endfacet");
                var _lines = line.Split('|');
                foreach (var _line in _lines)
                {
                    camScript.linesOfStl.Add(_line);
                }
                foreach (var l in camScript.linesOfStl)
                {
                    Camera.main.GetComponent<camScript>().scanSTL(l);
                }
                GameObject.Find("MESH").GetComponent<MakeMesh>().MergeMesh();
            }
        }
        StopCheck();
    }

    public bool CheckForStlBinary(byte[] _bytes)
    {
        var _isBinary = false;
        var readBytesArray = _bytes;
        var numBytes = readBytesArray.Length;
        if (numBytes < 84)
            return false;
        var numFacets = new byte[4];
        for (int i = 80; i < 84; i++)
        {
            numFacets[i - 80] = readBytesArray[i];
        }
        var num_facets = BitConverter.ToInt32(numFacets, 0);
        var predictedNumFacets = (84 + 50 * num_facets);
        if (numBytes == predictedNumFacets)
        {
            _isBinary = true;
        }
        else
        {
            _isBinary = false;
        }
        return _isBinary;
    }
}