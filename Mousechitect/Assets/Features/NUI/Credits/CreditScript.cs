using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditScript : MonoBehaviour
{
    
    IEnumerator PlayMusic()
    {
        yield return new WaitForSeconds(1f);
        transform.GetComponent<AudioSource>().Play();
    }
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlayMusic());

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
