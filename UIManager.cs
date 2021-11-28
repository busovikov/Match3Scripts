using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public Text noMatches;

    public void ShowNoMatches()
    {
        noMatches.GetComponent<Animator>().SetTrigger("Nomatch");
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
