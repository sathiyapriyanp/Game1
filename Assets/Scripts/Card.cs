using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public int id;
    public Sprite cardBack;
    public Sprite cardFront;

    private Image image;
    private Button button;
    private bool isFlippingOpen;
    private bool isFlippingClose;
    public bool flipFinished = false;
    private float flipAmount = 1;
    public float flipSpeed = 4;

    void Start()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
    }

    public void FlipCard()
    {
        if (CardManager.instance.choice1 == 0)
        {
            CardManager.instance.choice1 = id;
            CardManager.instance.AddChoosenCard(this.gameObject);
            StartCoroutine(FlipOpen());
            button.interactable = false;
        }
        else if (CardManager.instance.choice2 == 0)
        {
            CardManager.instance.choice2 = id;
            CardManager.instance.AddChoosenCard(this.gameObject);
            StartCoroutine(FlipOpen());
            button.interactable = false;
            StartCoroutine(WaitThenCompare());
        }
    }

    IEnumerator WaitThenCompare()
    {
        while (!CardManager.instance.choosenCards[0].GetComponent<Card>().flipFinished ||
               !CardManager.instance.choosenCards[1].GetComponent<Card>().flipFinished)
        {
            yield return null;
        }
        StartCoroutine(CardManager.instance.CompareCards());
    }

    IEnumerator FlipOpen()
    {
        flipFinished = false;

        while (flipAmount > 0)
        {
            flipAmount -= Time.deltaTime * flipSpeed;
            flipAmount = Mathf.Clamp01(flipAmount);
            transform.localScale = new Vector3(flipAmount, transform.localScale.y, transform.localScale.z);

            if (flipAmount <= 0)
            {
                image.sprite = cardFront;
                break;
            }
            yield return null;
        }

        while (flipAmount < 1)
        {
            flipAmount += Time.deltaTime * flipSpeed;
            flipAmount = Mathf.Clamp01(flipAmount);
            transform.localScale = new Vector3(flipAmount, transform.localScale.y, transform.localScale.z);

            if (flipAmount >= 1)
            {
                flipFinished = true;
                break;
            }
            yield return null;
        }
    }

    IEnumerator FlipClose()
    {
        flipFinished = false;

        while (flipAmount > 0)
        {
            flipAmount -= Time.deltaTime * flipSpeed;
            flipAmount = Mathf.Clamp01(flipAmount);
            transform.localScale = new Vector3(flipAmount, transform.localScale.y, transform.localScale.z);

            if (flipAmount <= 0)
            {
                image.sprite = cardBack;
                break;
            }
            yield return null;
        }

        while (flipAmount < 1)
        {
            flipAmount += Time.deltaTime * flipSpeed;
            flipAmount = Mathf.Clamp01(flipAmount);
            transform.localScale = new Vector3(flipAmount, transform.localScale.y, transform.localScale.z);

            if (flipAmount >= 1)
            {
                break;
            }
            yield return null;
        }
        button.interactable = true;
    }

    public void CloseCard()
    {
        StartCoroutine(FlipClose());
    }
}