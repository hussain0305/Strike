using System;
using UnityEngine;

public class ResetPreviewEvent { }

public class BallPreview : MonoBehaviour
{
    protected MenuContext Context => MainMenu.Context;

    protected string id;
    protected bool propertyFetched = false;
    protected BallProperties properties;

    protected void Init(string _id)
    {
        id = _id;
        properties = Balls.Instance.GetBall(id);
    }
}
