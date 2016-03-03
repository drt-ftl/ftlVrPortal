using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class ButtonScript : MonoBehaviour
{
    GameObject thumbnailObject;
    bool online = false;
    
	public void GO (bool _online, string url)
    {
        online = _online;
        if (GetComponentInChildren<thumbnail>())
        {
            thumbnailObject = GetComponentInChildren<thumbnail>().gameObject;
            if(online)
                StartCoroutine(GetThumbnailOnline(url));
            else
            {
                StartCoroutine(GetThumbnailLocal());
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Click()
    {
        camScript.PG = 0;
        if (online)
            Camera.main.GetComponent<camScript>().loadFileFromWeb(gameObject.name);
        else
            Camera.main.GetComponent<camScript>().loadFile(gameObject.name);
    }

    public void ClickUser()
    {
        camScript.PG = 0;
        camScript.BBT = "byUser";
        var u = gameObject.name.Replace(" ", "_");
        camScript.ARG = u;
        Camera.main.GetComponent<camScript>().browseFilesOnline_specific(false);
        Camera.main.GetComponent<camScript>().browseLabel.text = u;
    }

    private IEnumerator GetThumbnailLocal()
    {
        thumbnailObject = GetComponentInChildren<thumbnail>().gameObject;
        var _name = Path.GetFileName(name.Trim());
        var shortName = _name.Substring(0, _name.LastIndexOf(".STL")).Trim();
        var url = "file:" + Application.dataPath + "/Models/" + shortName + ".png";
        print(url);
        WWW w = new WWW(url);
        yield return w;
        if (w.texture != null)
        {
            var renderer = thumbnailObject.GetComponent<Image>();
            Sprite sprite = new Sprite();
            sprite = Sprite.Create(w.texture, new Rect(0, 0, w.texture.width, w.texture.height), new Vector2(0, 0), 2000.0f);

            renderer.sprite = sprite;
            //renderer.material = mat;
        }
    }

    private IEnumerator GetThumbnailOnline(string url)
    {
        thumbnailObject = GetComponentInChildren<thumbnail>().gameObject;
        WWW w = new WWW(url);
        yield return w;
        if (w.texture != null)
        {
            var renderer = thumbnailObject.GetComponent<Image>();
            Sprite sprite = new Sprite();
            sprite = Sprite.Create(w.texture, new Rect(0, 0, w.texture.width, w.texture.height), new Vector2(0, 0), 2000.0f);
            renderer.sprite = sprite;
            //renderer.material = mat;
        }
    }
}
