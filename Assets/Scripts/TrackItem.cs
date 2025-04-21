using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrackItem : MonoBehaviour
{
    [Header("Track Display")]
    [SerializeField] private Image albumImage;
    [SerializeField] private TMP_Text trackNameText;
    [SerializeField] private TMP_Text authorNameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text shelfLocationText; // Novo campo para a localização da estante


    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button buyButton;

    [Header("Highlight Settings")]
    [SerializeField] private GameObject backgroundHighlight; // Reference to the background highlight

    // Private fields
    private MusicController musicController;
    private int trackIndex;

    public void Setup(Sprite albumSprite, string trackName, string authorName, float price, string shelfLocation, MusicController controller, int index)
    {
        if (albumImage != null) albumImage.sprite = albumSprite;
        if (trackNameText != null) trackNameText.text = trackName;
        if (authorNameText != null) authorNameText.text = authorName;
        if (priceText != null) priceText.text = $"${price:F2}";

        musicController = controller;
        trackIndex = index;

        if (playButton != null)
        {
            playButton.onClick.AddListener(() => musicController.PlayTrack(trackIndex));
        }

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(() => controller.UpdateVinylInfo(controller.tracks[index]));
        }
        if (shelfLocationText != null)
        {
            shelfLocationText.text = shelfLocation; // Exibir a localização da estante
        }
    }


    public void SetHighlight(bool isHighlighted)
    {
        if (backgroundHighlight != null)
        {
            backgroundHighlight.SetActive(isHighlighted);
        }
    }
}