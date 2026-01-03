using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class SkEffObj : MonoBehaviour
{
    public SPRAnimation anim;
    void Start()
    {
        anim.Play();
        anim.EndEvent.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
