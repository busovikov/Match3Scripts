using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject credits;
    public GameObject buttons;
    public GameObject ghost;

    private void Awake()
    {
        credits = transform.Find("Credits").gameObject;
        buttons = transform.Find("Buttons").gameObject;
// ghost = transform.Find("GameObject/Ghost").gameObject;
    }
    public void ToCredits()
    {
        buttons.SetActive(false);
        ghost.SetActive(false);
        credits.SetActive(true);
    }

    public void BackToMainMenu()
    {
        buttons.SetActive(true);
        ghost.SetActive(true);
        credits.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
