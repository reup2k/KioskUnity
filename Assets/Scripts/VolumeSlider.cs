using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider; // Reference to the UI Slider
    [SerializeField] private AudioSource audioSource; // Reference to the AudioSource

    private void Start()
    {
        // Ensure the slider starts with the current volume
        if (volumeSlider != null && audioSource != null)
        {
            volumeSlider.value = audioSource.volume;

            // Add a listener to handle value changes
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    // Method to set the volume based on the slider value
    private void SetVolume(float value)
    {
        if (audioSource != null)
        {
            audioSource.volume = value;
        }
    }

    private void OnDestroy()
    {
        // Remove the listener to avoid memory leaks
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
        }
    }
}