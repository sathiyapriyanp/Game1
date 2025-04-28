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
    private bool isFlipping = false;
    private float flipAmount = 1f;
    public float flipSpeed = 4f;
    private bool isOpen = false;

    public bool IsOpen => isOpen;
    public bool IsFlipping => isFlipping;

    private void Start()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        button.onClick.AddListener(FlipCard);
    }

    public void FlipCard()
    {
        if (isFlipping || isOpen || CardManager.instance.IsComparing())
            return;
        SoundManager.instance.PlaySound(SoundManager.instance.flipSound);

        // Increment the click count
        CardManager.instance.UpdateClicksUI(); // Track click count on each flip
        CardManager.instance.AddChosenCard(this);
        StartCoroutine(FlipAnimation(true));
        CardManager.instance.IncrementClicks();
    }

    public void ForceCloseCard()
    {
        if (!isOpen || isFlipping) return;
        SoundManager.instance.PlaySound(SoundManager.instance.flipSound);
        StartCoroutine(FlipAnimation(false));
    }

    IEnumerator FlipAnimation(bool opening)
    {
        isFlipping = true;

        // Shrink
        while (flipAmount > 0)
        {
            flipAmount -= Time.deltaTime * flipSpeed;
            flipAmount = Mathf.Clamp01(flipAmount);
            transform.localScale = new Vector3(flipAmount, 1, 1);
            yield return null;
        }

        // Change sprite
        image.sprite = opening ? cardFront : cardBack;
        isOpen = opening;

        // Expand
        while (flipAmount < 1)
        {
            flipAmount += Time.deltaTime * flipSpeed;
            flipAmount = Mathf.Clamp01(flipAmount);
            transform.localScale = new Vector3(flipAmount, 1, 1);
            yield return null;
        }

        isFlipping = false;

        if (opening && CardManager.instance.ReadyToCompare())
        {
            CardManager.instance.StartCompareProcess();
        }
    }
}