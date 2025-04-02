using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class MusicController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Button playPauseButton;
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private TMP_Text trackNameText;
    [SerializeField] private TMP_Text authorNameText; // Field for author name
    [SerializeField] private List<Image> albumImages; // List of album cover images
    [SerializeField] private List<Transform> rotatingObjects; // List of objects to rotate when music is playing
    [SerializeField] private Transform needle; // Needle to rotate when starting/stopping music
    [SerializeField] private Vector3 needleStartRotation; // Rotation of the needle when music is stopped
    [SerializeField] private Vector3 needlePlayRotation; // Rotation of the needle when music is playing
    [SerializeField] private float needleRotationSpeed = 5f; // Speed of needle rotation
    [SerializeField] private float rotationSpeed = 100f; // Maximum rotation speed
    [SerializeField] private float rotationAcceleration = 2f; // Speed of acceleration/deceleration

    private List<TrackData> tracks = new List<TrackData>();
    private int currentTrackIndex = 0;
    private bool isPlaying = false;
    private float currentRotationSpeed = 0f; // Current rotation speed

    private void Start()
    {
        if (playPauseButton != null)
        {
            playPauseButton.onClick.AddListener(TogglePlayPause);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextTrack);
        }

        if (previousButton != null)
        {
            previousButton.onClick.AddListener(PreviousTrack);
        }

        // Example tracks (you can replace these with your own data)
        tracks.Add(new TrackData
        {
            Name = "Melhor sozinha",
            Author = "Luisa Sonza",
            FileURL = "https://drive.google.com/uc?export=download&id=1xYFYxLgodcQBxsjOUr-OWpqQVYK6IVWd",
            AlbumImageURL = "https://images.genius.com/f8f0d85159dd0b50fcd7af15aa0b799e.720x720x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "Do you no wrong",
            Author = "Richie Campbell",
            FileURL = "https://drive.google.com/uc?export=download&id=1kGWktdN10CdyyfT9KAgWXmwXwIahF-k7",
            AlbumImageURL = "https://t2.genius.com/unsafe/425x425/https%3A%2F%2Fimages.genius.com%2F784e024c3e172917e130450f3c23bee3.1000x1000x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "N95",
            Author = "Kendrick Lamar",
            FileURL = "https://drive.google.com/uc?export=download&id=1gCKot1qI57QJ90mxPAfFuZeleWQhE7nh",
            AlbumImageURL = "https://genius.com/Genius-brasil-traducoes-kendrick-lamar-n95-traducao-em-portugues-lyrics"
        });

        tracks.Add(new TrackData
        {
            Name = "White Ferrari",
            Author = "Frank Ocean",
            FileURL = "https://drive.google.com/uc?export=download&id=17bnGGK7Yuneye52DbhvjappNUPjfIgjS",
            AlbumImageURL = "https://t2.genius.com/unsafe/425x425/https%3A%2F%2Fimages.genius.com%2F750737a023d383b93057b73d546bfe4e.1000x1000x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "Self Care",
            Author = "Mac Miller",
            FileURL = "https://drive.google.com/uc?export=download&id=1RrtFAteCaSsXLTvdHkRoxM5E4GTfBtHw",
            AlbumImageURL = "https://t2.genius.com/unsafe/425x425/https%3A%2F%2Fimages.genius.com%2F1f5cc2dbac307c27261849f4f49771ae.1000x1000x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "Wait for you",
            Author = "Future",
            FileURL = "https://drive.google.com/uc?export=download&id=1WdG2wguiBHvCqfN6Y_ObKcus_sxbG57G",
            AlbumImageURL = "https://t2.genius.com/unsafe/425x425/https%3A%2F%2Fimages.genius.com%2Fc07c1aa672e8578e2a5b34cc2b005d40.1000x1000x1.png"
        });

        PlayTrack(0); // Start with the first track
    }

    private void Update()
    {
        // Smoothly rotate all objects in the list if music is playing
        foreach (var rotatingObject in rotatingObjects)
        {
            if (rotatingObject != null)
            {
                rotatingObject.Rotate(Vector3.forward * currentRotationSpeed * Time.deltaTime);
            }
        }
    }

    private void PlayTrack(int index)
    {
        if (tracks.Count == 0)
        {
            Debug.LogWarning("No tracks available to play.");
            return;
        }

        if (index < 0 || index >= tracks.Count)
        {
            Debug.LogWarning("Invalid track index: " + index);
            return;
        }

        currentTrackIndex = index;
        StartCoroutine(LoadAudioClip(tracks[index].FileURL));
        StartCoroutine(LoadAlbumImage(tracks[index].AlbumImageURL));

        trackNameText.text = tracks[index].Name;
        authorNameText.text = tracks[index].Author;
    }

    private IEnumerator LoadAudioClip(string url)
    {
        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            audioSource.clip = clip;
            audioSource.Play();
            isPlaying = true;
            UpdatePlayPauseIcon();
            StartCoroutine(RotateNeedle(needlePlayRotation));
            StartCoroutine(SmoothStartRotation());
            Debug.Log("Audio is playing!");
        }
        else
        {
            Debug.LogError("Failed to load audio clip: " + request.error);
        }
    }

    private IEnumerator LoadAlbumImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite albumSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

            // Update all album images in the list
            foreach (var image in albumImages)
            {
                if (image != null)
                {
                    image.sprite = albumSprite;
                }
            }
        }
        else
        {
            Debug.LogError("Failed to load album image: " + request.error);
        }
    }

    private void TogglePlayPause()
    {
        if (audioSource == null) return;

        if (isPlaying)
        {
            audioSource.Pause();
            StartCoroutine(RotateNeedle(needleStartRotation));
            StartCoroutine(SmoothStopRotation());
        }
        else
        {
            audioSource.Play();
            StartCoroutine(RotateNeedle(needlePlayRotation));
            StartCoroutine(SmoothStartRotation());
        }

        isPlaying = !isPlaying;
        UpdatePlayPauseIcon();
    }

    private void NextTrack()
    {
        if (tracks.Count == 0)
        {
            Debug.LogWarning("No tracks available to play.");
            return;
        }

        PlayTrack((currentTrackIndex + 1) % tracks.Count);
    }

    private void PreviousTrack()
    {
        if (tracks.Count == 0)
        {
            Debug.LogWarning("No tracks available to play.");
            return;
        }

        PlayTrack((currentTrackIndex - 1 + tracks.Count) % tracks.Count);
    }

    private void UpdatePlayPauseIcon()
    {
        if (playPauseButton != null && playPauseButton.image != null)
        {
            playPauseButton.image.sprite = isPlaying ? pauseIcon : playIcon;
        }
    }

    private IEnumerator RotateNeedle(Vector3 targetRotation)
    {
        if (needle == null) yield break;

        Quaternion startRotation = needle.rotation;
        Quaternion endRotation = Quaternion.Euler(targetRotation);
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            needle.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime);
            elapsedTime += Time.deltaTime * needleRotationSpeed;
            yield return null;
        }

        needle.rotation = endRotation;
    }

    private IEnumerator SmoothStartRotation()
    {
        while (currentRotationSpeed < rotationSpeed)
        {
            currentRotationSpeed += rotationAcceleration * Time.deltaTime;
            yield return null;
        }
        currentRotationSpeed = rotationSpeed; // Ensure it reaches the target speed
    }

    private IEnumerator SmoothStopRotation()
    {
        while (currentRotationSpeed > 0f)
        {
            currentRotationSpeed -= rotationAcceleration * Time.deltaTime;
            yield return null;
        }
        currentRotationSpeed = 0f; // Ensure it stops completely
    }
}

[System.Serializable]
public class TrackData
{
    public string Name;
    public string Author;
    public string FileURL;
    public string AlbumImageURL;
}