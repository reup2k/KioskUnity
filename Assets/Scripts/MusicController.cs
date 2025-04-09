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
    [SerializeField] private TMP_Text authorNameText;
    [SerializeField] private List<Image> albumImages;
    [SerializeField] private List<Transform> rotatingObjects;
    [SerializeField] private Transform needle;
    [SerializeField] private Vector3 needleStartRotation;
    [SerializeField] private Vector3 needlePlayRotation;
    [SerializeField] private float needleRotationSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float rotationAcceleration = 2f;
    [SerializeField] private Slider musicProgressSlider;

    private List<TrackData> tracks = new List<TrackData>();
    private int currentTrackIndex = 0;
    private bool isPlaying = false;
    private float currentRotationSpeed = 0f;
    private bool isScrubbing = false;

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

        if (musicProgressSlider != null)
        {
            musicProgressSlider.onValueChanged.AddListener(OnMusicProgressChanged);
        }

        // Adicione os dados das músicas aqui
        AddTracks();

        PlayTrack(0); // Start with the first track
    }


    private void AddTracks()
    {
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
            AlbumImageURL = "https://t2.genius.com/unsafe/194x194/https%3A%2F%2Fimages.genius.com%2F2f8cae9b56ed9c643520ef2fd62cd378.1000x1000x1.png"
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

        tracks.Add(new TrackData
        {
            Name = "GOT DAMN",
            Author = "Gunna",
            FileURL = "https://drive.google.com/uc?export=download&id=1cd2bjDAVzsdFxR0idCEFESLCh681bUG0",
            AlbumImageURL = "https://t2.genius.com/unsafe/504x504/https%3A%2F%2Fimages.genius.com%2F78aeff6ead193177f72596016bc71c84.500x500x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "Sing About me I'm Dying of Thirst",
            Author = "Kendrick Lamar",
            FileURL = "https://drive.google.com/uc?export=download&id=1cpFbqOehjPX0u5hrZf3VJt08HKkikPDR",
            AlbumImageURL = "https://t2.genius.com/unsafe/425x425/https%3A%2F%2Fimages.genius.com%2Fb4d6d87f080c362200ce55ed35ec65bb.1000x1000x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "ss",
            Author = "Ken Carson",
            FileURL = "https://drive.google.com/uc?export=download&id=15kE6PycMdH0C4dTum6WqIm2q6prkjLeA",
            AlbumImageURL = "https://t2.genius.com/unsafe/504x504/https%3A%2F%2Fimages.genius.com%2F8b454f160a18386800e044deb3b99826.1000x1000x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "Denial is a river",
            Author = "Doechii",
            FileURL = "https://drive.google.com/uc?export=download&id=19ELaTKQktVOIdqALM--6gWyed8B5qrB2",
            AlbumImageURL = "https://t2.genius.com/unsafe/425x425/https%3A%2F%2Fimages.genius.com%2Fa899e6cbe67695bfa7e2a716c50d7583.1000x1000x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "BACKDOOR",
            Author = "Playboi Carti",
            FileURL = "https://drive.google.com/uc?export=download&id=10ZnZ4RtqA0FMHQr0OWh9tZktFYqBG6ed",
            AlbumImageURL = "https://t2.genius.com/unsafe/504x504/https%3A%2F%2Fimages.genius.com%2F84387c03968c8d51fd8be652624f112a.1000x1000x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "OUTWEST",
            Author = "JACKBOYS and Travis Scott",
            FileURL = "https://drive.google.com/uc?export=download&id=1wraEyquM8a_S_BkZK0nizRPHqAvJF45D",
            AlbumImageURL = "https://t2.genius.com/unsafe/425x425/https%3A%2F%2Fimages.genius.com%2Fae2e8ef74082a5c8240ca1867a6abae7.906x906x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "No Pole",
            Author = "Don Toliver",
            FileURL = "https://drive.google.com/uc?export=download&id=1EAmzqIpqU57Tlo2uIj2L-n7lJxC8JPWw",
            AlbumImageURL = "https://t2.genius.com/unsafe/504x504/https%3A%2F%2Fimages.genius.com%2Fb7300a9c092294d918b149a750c65579.999x999x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "Get You",
            Author = "Daniel Ceaser",
            FileURL = "https://drive.google.com/uc?export=download&id=1lHI6muDK0XgNhM36GY5Xa8nxNE1qyicX",
            AlbumImageURL = "https://t2.genius.com/unsafe/425x425/https%3A%2F%2Fimages.genius.com%2F339757a068c3f5c7f231dcc66e174dbd.500x500x1.jpg"
        });

        tracks.Add(new TrackData
        {
            Name = "Yebba’s Heartbreak",
            Author = "Drake & Yebba",
            FileURL = "https://drive.google.com/uc?export=download&id=1qDDTu8wpkONB2XwTSwEhUEJzg475F7fm",
            AlbumImageURL = "https://t2.genius.com/unsafe/504x504/https%3A%2F%2Fimages.genius.com%2Ffa02d8bc4c7ee74b5a1408c2be032fea.1000x1000x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "Ordinary World",
            Author = "Duran Duran",
            FileURL = "https://drive.google.com/uc?export=download&id=1E_wPsmo-0nEHmjHxcab2gBODYgRpMWm8",
            AlbumImageURL = "https://t2.genius.com/unsafe/425x425/https%3A%2F%2Fimages.genius.com%2Fea86cc63dd332aed64181cbdb05802f1.980x1000x1.jpg"
        });

        tracks.Add(new TrackData
        {
            Name = "Wish",
            Author = "Trippie Redd",
            FileURL = "https://drive.google.com/uc?export=download&id=1jcfVUAhGqFRnD-TAwd9nLDMwCuQMhSrS",
            AlbumImageURL = "https://t2.genius.com/unsafe/504x504/https%3A%2F%2Fimages.genius.com%2F862dcd7eb8f08acd6279f600b3b20cdd.1000x1000x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "Creep",
            Author = "Radiohead",
            FileURL = "https://drive.google.com/uc?export=download&id=1XCYZO53G_rKF2QT5yCUK7c6Z4ghr9Tag",
            AlbumImageURL = "https://t2.genius.com/unsafe/425x425/https%3A%2F%2Fimages.genius.com%2F3e947d45997532b243ccd37bde484492.800x800x1.png"
        });

        tracks.Add(new TrackData
        {
            Name = "Like a Tattoo",
            Author = "Sade",
            FileURL = "https://drive.google.com/uc?export=download&id=1-re9y_-IUx1UATakG--1Mdi20rrrFqmX",
            AlbumImageURL = "https://t2.genius.com/unsafe/504x504/https%3A%2F%2Fimages.genius.com%2F9f56e2b608e7164a504b3373cc507c9d.600x600x1.png"
        });


        tracks.Add(new TrackData
        {
            Name = "One wish",
            Author = "Ray J",
            FileURL = "https://drive.google.com/uc?export=download&id=1tOaMTJNVGItM55FmVMtuBOWq57KdYwN6",
            AlbumImageURL = "https://t2.genius.com/unsafe/425x425/https%3A%2F%2Fimages.genius.com%2Fa836243c9c3fb292362907271e3a2137.640x640x1.jpg"
        });

        tracks.Add(new TrackData
        {
            Name = "Sozinho",
            Author = "Caetano Veloso",
            FileURL = "https://drive.google.com/uc?export=download&id=1djAAxpjFn939AzX11t4o1mvMLsdkSJ6b",
            AlbumImageURL = "https://t2.genius.com/unsafe/425x425/https%3A%2F%2Fimages.genius.com%2F9c853f0e9b33eb484ca1610bd8743243.1000x994x1.jpg"
        });

        tracks.Add(new TrackData
        {
            Name = "LVL",
            Author = "A$AP Rocky",
            FileURL = "https://drive.google.com/uc?export=download&id=1qiuZ_J9awos4Q-MWXEhsBMHBq_Ven1M2",
            AlbumImageURL = "https://t2.genius.com/unsafe/504x504/https%3A%2F%2Fimages.genius.com%2F6077103e9d38a005bdae189d39d96c17.1000x1000x1.jpg"
        });

        tracks.Add(new TrackData
        {
            Name = "Die for You",
            Author = "The Weekend",
            FileURL = "https://drive.google.com/uc?export=download&id=1QbN20swAoXN92nUQm0mBNvkCBaWbda0T",
            AlbumImageURL = "https://t2.genius.com/unsafe/504x504/https%3A%2F%2Fimages.genius.com%2F6077103e9d38a005bdae189d39d96c17.1000x1000x1.jpg"
        });

        PlayTrack(0); // Start with the first track
    }

    private void Update()
    {
        if (rotatingObjects != null && rotatingObjects.Count > 0)
        {
            foreach (var rotatingObject in rotatingObjects)
            {
                if (rotatingObject != null)
                {
                    rotatingObject.Rotate(Vector3.forward * currentRotationSpeed * Time.deltaTime);
                }
            }
        }

        if (audioSource != null && musicProgressSlider != null && audioSource.clip != null && !isScrubbing)
        {
            musicProgressSlider.value = audioSource.time / audioSource.clip.length;
        }

        if (audioSource != null && !audioSource.isPlaying && isPlaying && audioSource.time >= audioSource.clip.length)
        {
            NextTrack();
        }
    }

    private Coroutine loadAudioCoroutine;
private Coroutine loadImageCoroutine;

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

    if (trackNameText != null)
    {
        trackNameText.text = "A Carregar Audio...";
    }

    // Cancel previous coroutines if they are running
    if (loadAudioCoroutine != null)
    {
        StopCoroutine(loadAudioCoroutine);
    }
    if (loadImageCoroutine != null)
    {
        StopCoroutine(loadImageCoroutine);
    }

    // Start new coroutines
    loadAudioCoroutine = StartCoroutine(LoadAudioClip(tracks[index].FileURL));
    loadImageCoroutine = StartCoroutine(LoadAlbumImage(tracks[index].AlbumImageURL));

    if (authorNameText != null)
    {
        authorNameText.text = tracks[index].Author;
    }
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

            if (trackNameText != null)
            {
                trackNameText.text = tracks[currentTrackIndex].Name;
            }

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
        currentRotationSpeed = rotationSpeed;
    }

    private IEnumerator SmoothStopRotation()
    {
        while (currentRotationSpeed > 0f)
        {
            currentRotationSpeed -= rotationAcceleration * Time.deltaTime;
            yield return null;
        }
        currentRotationSpeed = 0f;
    }

    private void OnMusicProgressChanged(float value)
    {
        if (audioSource != null && audioSource.clip != null)
        {
            isScrubbing = true;
            audioSource.time = value * audioSource.clip.length;
            isScrubbing = false;
        }
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