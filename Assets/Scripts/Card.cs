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
    private bool isFlipping;
    private bool isOpen;
    private float flipAmount = 1f;

    public float flipSpeed = 4f;
    public bool IsOpen => isOpen;
    public bool IsFlipping => isFlipping;

    private void Start()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        button.onClick.AddListener(FlipCard);
    }

    public void Initialize(Sprite front)
    {
        cardFront = front;
        cardBack = GetComponent<Image>().sprite;
        image = GetComponent<Image>();
    }

    public void FlipCard()
    {
        if (isFlipping || isOpen || CardManager.instance.IsComparing())
            return;

        SoundManager.instance.PlaySound(SoundManager.instance.flipSound);
        CardManager.instance.IncrementClicks();
        CardManager.instance.AddChosenCard(this);
        StartCoroutine(FlipAnimation(true));
    }

    public void ForceCloseCard()
    {
        if (!isOpen || isFlipping) return;
        SoundManager.instance.PlaySound(SoundManager.instance.flipSound);
        StartCoroutine(FlipAnimation(false));
    }

    public void DisableInteraction()
    {
        button.interactable = false;
    }

    public bool IsMatch(Card other) => cardFront == other.cardFront;
    public bool IsInteractable() => button.interactable;

    private IEnumerator FlipAnimation(bool opening)
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

        // Swap sprite
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
    }
}
