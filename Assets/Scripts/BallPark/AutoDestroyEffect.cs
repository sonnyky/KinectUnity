using UnityEngine;
using System.Collections;

public class AutoDestroyEffect : MonoBehaviour
{
    private float lifetime = 5.0f;
    // Use this for initialization
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {

     

    }
}
