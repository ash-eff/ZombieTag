using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weather : MonoBehaviour
{
    Animator anim;
    public AudioSource rainSource;
    public AudioSource thunderSource;
    public AudioClip thunder1;
    public AudioClip thunder2;
    public AudioClip thunder3;

    void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(Lightning());
        rainSource.Play();
    }

    IEnumerator Lightning()
    {
        while (true)
        {
            float waitTime = Random.Range(4f, 8f);
            int flash = Random.Range(1, 4);
            int thunder = Random.Range(1, 4);
            yield return new WaitForSeconds(waitTime);
            if(flash == 1)
            {
                anim.SetTrigger("Flash");
            }
            if (flash == 2)
            {
                anim.SetTrigger("Flash2");
            }
            if (flash == 3)
            {
                anim.SetTrigger("Flash3");
            }
            yield return new WaitForSeconds(.5f);
            if (thunder == 1)
            {
                thunderSource.PlayOneShot(thunder1);
            }
            if (thunder == 2)
            {
                thunderSource.PlayOneShot(thunder2);
            }
            if (thunder == 3)
            {
                thunderSource.PlayOneShot(thunder3);
            }
            //if (thunder == 4)
            //{
            //    thunderSource.PlayOneShot(thunder4);
            //}
            //if (thunder == 5)
            //{
            //    thunderSource.PlayOneShot(thunder5);
            //}
        }
    }
}
