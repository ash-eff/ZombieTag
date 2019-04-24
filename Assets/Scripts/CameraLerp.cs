using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraLerp : MonoBehaviour
{
    float lerpTime = 15f;
    float currentLerpTime;

    float moveDistance = 20f;

    Vector3 startPos;
    Vector3 endPos;

    protected void Start()
    {
        startPos = transform.position;
        endPos = transform.position + new Vector3(-1,1, 0) * moveDistance;
    }

    protected void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartGame();
        }

        currentLerpTime += Time.deltaTime;
        if (currentLerpTime > lerpTime)
        {
            currentLerpTime = lerpTime;
        }

        float perc = currentLerpTime / lerpTime;
        transform.position = Vector3.Lerp(startPos, endPos, perc);

        if(transform.position == endPos)
        {
            currentLerpTime = 0;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
