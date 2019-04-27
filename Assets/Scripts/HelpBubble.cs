using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HelpBubble : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro helpText;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(HelpType());
    }

    IEnumerator HelpType()
    {
        while (true)
        {
            helpText.text = "h";
            yield return new WaitForSeconds(.2f);
            helpText.text = "he";
            yield return new WaitForSeconds(.2f);
            helpText.text = "hel";
            yield return new WaitForSeconds(.2f);
            helpText.text = "help";
            yield return new WaitForSeconds(.2f);
            helpText.text = "help!";
            yield return new WaitForSeconds(.2f);
        }
    }
}
