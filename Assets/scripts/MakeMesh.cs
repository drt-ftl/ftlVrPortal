using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MakeMesh : MonoBehaviour
{
    private Mesh mesh;
    public static List<int> tris = new List<int>();
    public static List<Vector3> verts = new List<Vector3>();
    public static List<Vector3> normals = new List<Vector3>();
    private List<Color> colors = new List<Color>();
    private List<Vector2> uvs = new List<Vector2>();

    //private Mesh mesh_b;
    //private List<int> tris_b = new List<int>();
    //private List<Vector3> verts_b = new List<Vector3>();
    //private List<Vector3> normals_b = new List<Vector3>();
    //private GameObject back;
    bool GO = false;
    int d = 0;
    public List<Material> materials = new List<Material>();
    int currentMaterial = 0;

    public Vector3 Min   { get; set; }
    public Vector3 Max { get; set; }

    private Color Hidden = new Color(0f, 0f, 0f, 0f);

    public void ClearAll()
    {
        GO = false;
        mesh.Clear();
        //mesh_b.Clear();

        tris.Clear();
        verts.Clear();
        normals.Clear();
        colors.Clear();
        uvs.Clear();
        d = 0;
        currentMaterial = 0;
        //tris_b.Clear();
        //verts_b.Clear();
        //normals_b.Clear();
        Min = Vector3.one * 1000;
        Max = Vector3.one * -1000;
    }
    public void Begin()
    {
        gameObject.transform.position = Vector3.zero;
        Min = Vector3.one * 1000;
        Max = Vector3.one * -1000;
        if (gameObject.GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>();
        if (gameObject.GetComponent<MeshRenderer>() == null)
            gameObject.AddComponent<MeshRenderer>();

            mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        var rnd = GetComponent<MeshRenderer>();
        rnd.material = material;
        rnd.receiveShadows = true;
        rnd.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        //back = GameObject.Find("BACK");
        //if (back.GetComponent<MeshFilter>() == null)
        //    back.AddComponent<MeshFilter>();
        //if (back.GetComponent<MeshRenderer>() == null)
        //    back.AddComponent<MeshRenderer>();

        //mesh_b = back.GetComponent<MeshFilter>().mesh;
        //mesh_b.Clear();

        //var rnd_b = back.GetComponent<MeshRenderer>();
        //rnd_b.material = material;
        //rnd_b.receiveShadows = true;
        //rnd_b.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        GO = true;
    }


    public Material material { get; set;}
    public Color color { get; set; }

    public Vector3 Normal (Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var dir = Vector3.Cross(p2 - p1, p3 - p1);
        var norm = Vector3.Normalize(dir);
        return norm;
    }

    //public Vector3 NormalBack (Vector3 p1, Vector3 p2, Vector3 p3)
    //{
    //    var dir = Vector3.Cross(p3 - p1, p3 - p2);
    //    var norm = Vector3.Normalize(dir);
    //    return norm;
    //}

    public void AddTriangle (Vector3 p1, Vector3 p2, Vector3 p3, Vector3 norm, bool _binary)
    {
        var count = tris.Count;

        tris.Add(count);
        tris.Add(count + 1);
        tris.Add(count + 2);
        if (_binary)
        {
            verts.Add(p3);
            verts.Add(p2);
            verts.Add(p1);
            uvs.Add(p3);
            uvs.Add(p2);
            uvs.Add(p1);
        }
        else
        {
            verts.Add(p1);
            verts.Add(p2);
            verts.Add(p3);
            uvs.Add(p1);
            uvs.Add(p2);
            uvs.Add(p3);
        }
        for (int i = 0; i < 3; i++)
            normals.Add(norm);
        for (int i = 0; i < 3; i++)
            colors.Add(InspectorL.stlColor);

        //tris_b.Add(count);
        //tris_b.Add(count + 1);
        //tris_b.Add(count + 2);
        //verts_b.Add(p1);
        //verts_b.Add(p2);
        //verts_b.Add(p3);
        //for (int i = 0; i < 3; i++)
        //    normals_b.Add(-norm);
    }

    public Vector3 centroid
    {
        get
        {
            var c = (Min + Max) / 2.0f;
            return c;
        }
    }

    public void MergeMesh()
    {
        foreach (var vert in verts)
        {
            var max = Max;
            var min = Min;
            if (vert.x > Max.x) max.x = vert.x;
            if (vert.x < Min.x) min.x = vert.x;
            if (vert.y > Max.y) max.y = vert.y;
            if (vert.y < Min.y) min.y = vert.y;
            if (vert.z > Max.z) max.z = vert.z;
            if (vert.z < Min.z) min.z = vert.z;
            Max = max;
            Min = min;
        }
        var count = verts.Count;
        for (int i = 0; i < count; i++)
        {
            verts[i] -= centroid;
            //verts_b[i] -= centroid;
        }
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();
        mesh.Optimize();


        //mesh_b.vertices = verts_b.ToArray();
        //mesh_b.triangles = tris_b.ToArray();
        //mesh_b.normals = normals_b.ToArray();
        //mesh_b.uv = uvs.ToArray();
        //mesh_b.colors = colors.ToArray();

        //mesh_b.RecalculateNormals();
        //mesh_b.Optimize();

        
        camScript.folderWindow.GetComponent<PanelFades>().FadeOut();
        Camera.main.GetComponent<camScript>().mainMenu.GetComponent<PanelFades>().FadeOut();
    }

    public Vector3[] GetTriangleVertices(int id)
    {
        var pts = new Vector3[3];
        var index = id * 3;
        pts[0] = mesh.vertices[index];
        pts[1] = mesh.vertices[index + 1];
        pts[2] = mesh.vertices[index + 2];
        return pts;
    }

    public Mesh GetMesh()
    {
        return mesh;
    }

    void Update()
    {
        if (!GO) return;
        int low = (int)InspectorL.stlTimeSliderMin;
        int high = (int)InspectorL.stlTimeSlider;
        var count = colors.Count;
        for (int i = 0; i < count - 1; i++)
        {
            if ((i / 3) >= low && (i / 3) < high)
            {
                colors[i] = InspectorL.stlColor;
                Camera.main.GetComponent<camScript>().Mat.SetColor("_SpecColor", colors[i]);
            }
            else colors[i] = Hidden;
            Camera.main.GetComponent<camScript>().Mat.SetColor("_SpecColor", Hidden);
        }
        mesh.colors = colors.ToArray();
        
    }
    public void ChangeMaterial(int selection)
    {
        GetComponent<MeshRenderer>().material = materials[selection];
    }

}
