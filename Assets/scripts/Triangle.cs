using UnityEngine;
using System.Collections;

public class Triangle
{
    public Triangle(Vector3 _p1, Vector3 _p2, Vector3 _p3, Vector3 _norm, bool _isBinary)
    {
        p1 = _p1;
        p2 = _p2;
        p3 = _p3;
        if (_norm == Vector3.zero)
        {
            norm = Normal();
        }
        else
            norm = _norm;
        _binary = _isBinary;
    }
    public Vector3 p1 { get; set; }
    public Vector3 p2 { get; set; }
    public Vector3 p3 { get; set; }
    public Vector3 norm { get; set; }
    public bool _binary { get; set; }

    public Vector3 Normal()
    {
        var dir = Vector3.Cross(p2 - p1, p3 - p1);
        var norm = Vector3.Normalize(dir);
        return norm;
    }
}
