using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    [SerializeField]
    private GameObject pointerObj;
    private Vector3 screenPos;
    private Vector2 onScreenPos;
    private Vector2 pointerViewportPos;
    private float max;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        screenPos = cam.WorldToViewportPoint(transform.position); //get viewport positions

        if (screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1)
        {
            pointerObj.SetActive(false);
            Debug.Log("already on screen, don't bother with the rest!");
            return;
        }

        pointerObj.SetActive(true);
        onScreenPos = new Vector2(screenPos.x - 0.5f, screenPos.y - 0.5f) * 2; //2D version, new mapping
        max = Mathf.Max(Mathf.Abs(onScreenPos.x), Mathf.Abs(onScreenPos.y)); //get largest offset
        onScreenPos = (onScreenPos / (max * 2)) + new Vector2(0.5f, 0.5f); //undo mapping
        pointerViewportPos = cam.ViewportToWorldPoint(onScreenPos);
        pointerObj.transform.position = new Vector2(pointerViewportPos.x, pointerViewportPos.y);
    }
}
