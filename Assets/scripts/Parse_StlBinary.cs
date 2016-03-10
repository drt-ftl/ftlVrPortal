using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class Parse_StlBinary
{
    private MakeMesh mm;
    private int num_facets = 0;
    private float divisor = 10000;

    public Parse_StlBinary (string path, MakeMesh _mm, byte[] bytes)
    {
        mm = _mm;
        Read_Text(path, bytes);
    }

    void Read_Text(string _path, byte[] _bytes)
    {
        var l = new List<byte>();
        if (_path == "")
        {
            foreach (var b in _bytes)
            {
                l.Add(b);
            }
        }
        else
        {
            var readBytesArray = File.ReadAllBytes(_path);
            foreach (var b in readBytesArray)
            {
                l.Add(b);
            }
        }
        var numFacetBytes = new byte[4];
        for (int i = 80; i < 84; i++)
        {
            numFacetBytes[i - 80] = l[i];
        }
        num_facets = BitConverter.ToInt32(numFacetBytes, 0);
        GenerateMesh(l);
    }

    public void GenerateMesh (List<byte> l)
    {
        mm.Begin();
        var sPos = 84;
        var chunk = new List<byte>();
        for (int facet = 0; facet < num_facets; facet++)
        {
            if (l.Count > sPos + 50)
            {
                chunk.Clear();
                for (int vector3 = sPos; vector3 < sPos + 50; vector3++)
                {
                    chunk.Add(l[vector3]);
                }//Makes chunk of 50
                var triangle = chunk.ToArray();
                Parse_Vertices(triangle);
                sPos += 50;
            }
        }
        mm.MergeMesh();
    }

    public void Parse_Vertices(byte[] triangle)
    {
        //MonoBehaviour.print(readBytes.Length.ToString());
        var sPos = 0;
        var chunk = new byte[4];

        var p = new Vector3[3];

        for (int i = sPos; i < sPos + 4; i++)
        {
            chunk[i - sPos] = triangle[i];
        }
        var n_i = BitConverter.ToSingle(chunk, 0);
        sPos += 4;

        for (int i = sPos; i < sPos + 4; i++)
        {
            chunk[i - sPos] = triangle[i];
        }
        var n_j = BitConverter.ToSingle(chunk, 0);
        sPos += 4;

        for (int i = sPos; i < sPos + 4; i++)
        {
            chunk[i - sPos] = triangle[i];
        }
        var n_k = BitConverter.ToSingle(chunk, 0);
        sPos += 4;

        var normal = new Vector3(n_i, n_j, n_k);
        normal = normal / divisor;
        for (int ii = 0; ii < 3; ii++)
        {
            for (int i = sPos; i < sPos + 4; i++)
            {
                chunk[i - sPos] = triangle[i];
            }
            var x = (float)BitConverter.ToSingle(chunk, 0);
            sPos += 4;

            for (int i = sPos; i < sPos + 4; i++)
            {
                chunk[i - sPos] = triangle[i];
            }
            var z = (float)BitConverter.ToSingle(chunk, 0);
            sPos += 4;

            for (int i = sPos; i < sPos + 4; i++)
            {
                chunk[i - sPos] = triangle[i];
            }
            var y = (float)BitConverter.ToSingle(chunk, 0);
            sPos += 4;

            p[ii] = new Vector3(x, y, z);
            p[ii] = p[ii];// / divisor;
        }
        
        mm.AddTriangle(p[0], p[1], p[2], normal, true);
    }
}
