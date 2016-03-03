using UnityEngine;
using System.Collections;

public class StlInterpreter
{
    private Vector3 Normal;
    public Vector3 Min { get; set; }
    public Vector3 Max { get; set; }

    public void ClearAll()
    {
        camScript.currentVertices.Clear();
        camScript.MM.ClearAll();
        Min = Vector3.one * 1000;
        Max = Vector3.one * -1000;
    }
    public Vector3 centroid
    {
        get
        {
            
            var c = (Min + Max) / 2.0f;
            return c;
        }
    }

    public StlInterpreter()
    {
        Min = Vector3.one * 1000;
        Max = Vector3.one * -1000;
    }

    public void normal (string _line)
    {
        float x;
        float y;
        float z;
        var split = _line.Split('l');
        split[1].TrimStart(' ');
        var coords = split[1].Split(' ');
        var xString = coords[0];
        var yString = coords[1];
        var zString = coords[2];
        var xStrSplit = xString.Split('e');
        if (float.TryParse(xStrSplit[0], out x))
        {
            float xE;
            if (float.TryParse(xStrSplit[1], out xE))
                x *= (Mathf.Pow(10f, xE));
        }

        var yStrSplit = yString.Split('e');
        if (float.TryParse(yStrSplit[0], out y))
        {
            float yE;
            if (float.TryParse(yStrSplit[1], out yE))
                y *= (Mathf.Pow(10f, yE));
        }

        var zStrSplit = zString.Split('e');
        if (float.TryParse(zStrSplit[0], out z))
        {
            float zE;
            if (float.TryParse(zStrSplit[1], out zE))
                z *= (Mathf.Pow(10f, zE));
        }
        Normal = new Vector3(x, y, z);
    }

    public void outerloop()
    {
    }

	public void endloop (string _line)
	{
		try
		{
            var mm = GameObject.Find("MESH").GetComponent<MakeMesh>();
            mm.AddTriangle(camScript.currentVertices[0], camScript.currentVertices[1], camScript.currentVertices[2], Normal);
            camScript.currentVertices.Clear ();
		}
		catch{}
	}

	public void vertex (string _line)
	{
		{
			float x;
			float y;
			float z;
			var coordSep = _line.Split('x');
			var coords = coordSep[1].TrimStart(' ').Split(' ');
			
			var xString = coords[0];
			var yString = coords[1];
			var zString = coords[2];
			var xStrSplit = xString.Split ('e');
			if (float.TryParse(xStrSplit[0], out x))
			{
				float xE;
				if (float.TryParse(xStrSplit[1], out xE))
					x *= (Mathf.Pow (10f, xE));
			}
			
			var yStrSplit = yString.Split ('e');
			if (float.TryParse(yStrSplit[0], out y))
			{
				float yE;
				if (float.TryParse(yStrSplit[1], out yE))
					y *= (Mathf.Pow (10f, yE));
			}
			
			var zStrSplit = zString.Split ('e');
			if (float.TryParse(zStrSplit[0], out z))
			{
				float zE;
				if (float.TryParse(zStrSplit[1], out zE))
					z *= (Mathf.Pow (10f, zE));
			}
			var newVertex = new Vector3 (x,y,z) * LoadFile.stlScale;
            var max = Max;
            var min = Min;
            if (x > Max.x) max.x = x;
            if (x < Min.x) min.x = x;
            if (y > Max.y) max.y = y;
            if (y < Min.y) min.y = y;
            if (z > Max.z) max.z = z;
            if (z < Min.z) min.z = z;
            Max = max;
            Min = min;
            if (min.z <= camScript.Min.z)
                camScript.Min.z = min.z;

            camScript.currentVertices.Add (newVertex);
		}
	}
}
