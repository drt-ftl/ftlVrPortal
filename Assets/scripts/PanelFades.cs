using UnityEngine;
using System.Collections;

public class PanelFades : MonoBehaviour {


    private float factor = 0.1f;

    public bool Visible()
    {
        if (transform.localPosition.z < 0)
            return false;
        return true;
    }

    public void FadeOut()
    {
        var pos = transform.localPosition;
        pos.z = -5000;
        transform.localPosition = pos;
    }

    public void FadeIn()
    {
        var pos = transform.localPosition;
        pos.z = 0;
        transform.localPosition = pos;
    }

    IEnumerator fadeOut()
    {
        var z = transform.localPosition.z;
        for (float f = z; f <= 2.4f; f += factor)
        {
            var pos = Vector3.zero;
            pos.y = Screen.height / 2;
            pos.z = f;
            transform.localPosition = pos;
            if (f + factor >= 2.4)
            yield return null;
        }
    }

    IEnumerator fadeIn()
    {
        var z = transform.localPosition.z;
        for (float f = z; f >= 0f; f -= factor)
        {
            var pos = Vector3.zero;
            pos.y = Screen.height / 2;
            pos.z = f;
            transform.localPosition = pos;
            yield return null;
        }
    }
}
