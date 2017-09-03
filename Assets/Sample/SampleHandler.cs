using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UUebView;

public class SampleHandler : MonoBehaviour, IUUebViewEventHandler {
	GameObject scrollViewContent;
	GameObject uuebView;

	// Use this for initialization
    void Start () {
		var scrollView = GameObject.Find("Scroll View");
		scrollViewContent = GameObject.Find("Scroll View/Viewport/Content");

		var scrollViewSize = scrollView.GetComponent<RectTransform>().sizeDelta;
		uuebView = UUebViewComponent.GenerateSingleViewFromUrl(this.gameObject, "resources://items.html", scrollViewSize);
		uuebView.transform.SetParent(scrollViewContent.transform, false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Reload () {
		uuebView.GetComponent<UUebViewComponent>().Core.Reload();
	}

    public void OnElementLongTapped(ContentType type, string param, string id)
    {
        // throw new System.NotImplementedException();
    }

    public void OnElementTapped(ContentType type, GameObject element, string param, string id)
    {
        if (type == ContentType.IMAGE) {
			element.AddComponent<Rigidbody>();
		}
    }

    public void OnLoaded()
    {
        var contentSizeDelta = scrollViewContent.GetComponent<RectTransform>().sizeDelta;
		contentSizeDelta = new Vector2(contentSizeDelta.x, uuebView.GetComponent<RectTransform>().sizeDelta.y);
    }

    public void OnLoadFailed(ContentType type, int code, string reason)
    {
        // throw new System.NotImplementedException();
    }

    public void OnLoadStarted()
    {
        // throw new System.NotImplementedException();
    }

    public void OnProgress(double progress)
    {
        // throw new System.NotImplementedException();
    }

    public void OnUpdated()
    {
        // throw new System.NotImplementedException();
    }
}
