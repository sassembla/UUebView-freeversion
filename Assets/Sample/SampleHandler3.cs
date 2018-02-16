using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UUebView;

public class SampleHandler3 : MonoBehaviour, IUUebViewEventHandler
{
    public void OnElementLongTapped(ContentType type, string param, string id)
    {

    }

    public void OnElementTapped(ContentType type, GameObject element, string param, string id)
    {

    }

    public void OnLoaded(string[] treeIds)
    {

    }

    public void OnLoadFailed(ContentType type, int code, string reason)
    {

    }

    public void OnLoadStarted()
    {

    }

    public void OnProgress(double progress)
    {

    }

    public void OnUpdated(string[] newTreeIds)
    {

    }
}
