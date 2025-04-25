using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
   [HideInInspector] public int id;
    public Sprite cardBack;
    [HideInInspector] public Sprite cardFront;

    private Image image;
    private Button button;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
    }

   //Card Open
   public 
}
