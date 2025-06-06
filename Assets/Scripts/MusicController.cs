using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class MusicController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<Button> playPauseButtons;
    [SerializeField] private List<Sprite> playIcons;
    [SerializeField] private List<Sprite> pauseIcons;

    [Header("Buttons")]
    [SerializeField] private Button buyButton; // Added buyButton


    [Header("Track Navigation")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    [Header("Track Display")]
    [SerializeField] private List<TMP_Text> trackNameTexts;
    [SerializeField] private List<TMP_Text> authorNameTexts;
    [SerializeField] private List<Image> albumImages;
    [SerializeField] private TMP_Dropdown sortDropdown; // Dropdown para ordenação

    [Header("Vinyl Settings")]
    [SerializeField] private Image vinylImage;
    [SerializeField] private TMP_Text vinylAuthorText;
    [SerializeField] private TMP_Text vinylNameText;
    [SerializeField] private TMP_Text vinylPriceText;
    [SerializeField] private TMP_Text vinylBiographyText;
    [SerializeField] private TMP_Text shelfLocationText; // Added shelfLocationText
    [SerializeField] private RectTransform locationMarker; // Added locationMarker

    [Header("Rotation Settings")]
    [SerializeField] private List<Transform> rotatingObjects;
    [SerializeField] private Transform needle;
    [SerializeField] private Vector3 needleStartRotation;
    [SerializeField] private Vector3 needlePlayRotation;
    [SerializeField] private float needleRotationSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float rotationAcceleration = 2f;

    [Header("Progress Settings")]
    [SerializeField] private List<Slider> musicProgressSliders;

    [Header("Track List")]
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private Transform trackListContainer;
    [SerializeField] private GameObject trackItemPrefab;

    [Header("Tracks Data")]
    public List<TrackData> tracks = new List<TrackData>();

    // Private fields
    private int currentTrackIndex = 0;
    private bool isPlaying = false;
    private float currentRotationSpeed = 0f;
    private TrackItem currentHighlightedItem;
    private bool isScrubbing = false;


    private void PopulateTrackList()
    {
        // Limpar os itens existentes
        foreach (Transform child in trackListContainer)
        {
            child.gameObject.SetActive(false);
        }

        // Recriar os itens com base na lista ordenada
        for (int i = 0; i < tracks.Count; i++)
        {
            var track = tracks[i];
            GameObject trackItem = Instantiate(trackItemPrefab, trackListContainer);

            if (trackItem == null)
            {
                Debug.LogError("Failed to instantiate trackItemPrefab.");
                continue;
            }

            TrackItem trackItemScript = trackItem.GetComponent<TrackItem>();

            if (trackItemScript == null)
            {
                Debug.LogError("TrackItemScript is null. Make sure the prefab has the TrackItem script attached.");
                continue;
            }

            Debug.Log($"Instantiated track item for: {track.Name}");
            StartCoroutine(LoadTrackItem(track, trackItemScript, trackItem, i));
        }
    }
    public void SortTracks(int sortOption)
    {
        switch (sortOption)
        {
            case 0: // A-Z
                tracks.Sort((a, b) => a.Name.CompareTo(b.Name));
                break;
            case 1: // Z-A
                tracks.Sort((a, b) => b.Name.CompareTo(a.Name));
                break;
            case 2: // Preço Mais Baixo
                tracks.Sort((a, b) => a.Price.CompareTo(b.Price));
                break;
            case 3: // Preço Mais Alto
                tracks.Sort((a, b) => b.Price.CompareTo(a.Price));
                break;
            default:
                Debug.LogWarning("Opção de ordenação inválida.");
                return;
        }

        // Atualizar a exibição da lista após a ordenação
        PopulateTrackList();
    }

    public void UpdateVinylInfo(TrackData track)
    {
        if (vinylImage != null)
        {
            StartCoroutine(LoadVinylImage(track.AlbumImageURL));
        }

        if (vinylAuthorText != null)
        {
            vinylAuthorText.text = track.Author;
        }

        if (vinylNameText != null)
        {
            vinylNameText.text = track.Name;
        }

        if (vinylPriceText != null)
        {
            vinylPriceText.text = $"${track.Price:F2}";
        }

        if (vinylBiographyText != null)
        {
            vinylBiographyText.text = track.Biography; // Use the Biography field
        }

        if (shelfLocationText != null)
        {
            shelfLocationText.text = track.ShelfLocation; // Exibir a localização da estante
        }

        if (locationMarker != null)
        {
            locationMarker.anchoredPosition = track.StorePosition; // Define a posição do marcador
        }
    }

    private void FilterTracks(string searchText)
    {
        // If search text is empty, show all tracks
        var filteredTracks = string.IsNullOrEmpty(searchText)
            ? tracks
            : tracks.FindAll(track =>
                track.Name.ToLower().Contains(searchText.ToLower()) ||
                track.Author.ToLower().Contains(searchText.ToLower())
            );

        // Deactivate all child objects
        for (int i = 0; i < trackListContainer.childCount; i++)
        {
            trackListContainer.GetChild(i).gameObject.SetActive(false);
        }

        // Reactivate only the filtered tracks
        for (int i = 0; i < filteredTracks.Count; i++)
        {
            Transform trackItemTransform;

            // If there are enough existing child objects, reuse them
            if (i < trackListContainer.childCount)
            {
                trackItemTransform = trackListContainer.GetChild(i);
            }
            else
            {
                // Otherwise, instantiate a new track item
                GameObject trackItem = Instantiate(trackItemPrefab, trackListContainer);
                trackItemTransform = trackItem.transform;
            }

            // Activate and populate the track item
            trackItemTransform.gameObject.SetActive(true);
            TrackItem trackItemScript = trackItemTransform.GetComponent<TrackItem>();

            if (trackItemScript != null)
            {
                int trackIndex = tracks.IndexOf(filteredTracks[i]);
                StartCoroutine(LoadTrackItem(filteredTracks[i], trackItemScript, trackItemTransform.gameObject, trackIndex));
            }
            else
            {
                Debug.LogError("TrackItem script is missing on the instantiated prefab.");
            }
        }

        // Deactivate any extra child objects that are not needed
        for (int i = filteredTracks.Count; i < trackListContainer.childCount; i++)
        {
            trackListContainer.GetChild(i).gameObject.SetActive(false);
        }

        // Handle the case where there is only one element
        if (filteredTracks.Count == 1)
        {
            Transform singleItem = trackListContainer.GetChild(0);
            singleItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Align to the top
        }
    }
    private IEnumerator LoadVinylImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite vinylSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            vinylImage.sprite = vinylSprite;
        }
        else
        {
            Debug.LogError("Failed to load vinyl image: " + request.error);
        }
    }

    private IEnumerator LoadTrackItem(TrackData track, TrackItem trackItemScript, GameObject trackItem, int index)
    {
        if (trackItemScript == null)
        {
            Debug.LogError("TrackItemScript is null. Make sure the prefab has the TrackItem script attached.");
            yield break;
        }

        Debug.Log($"Loading track: {track.Name}, Author: {track.Author}, Price: {track.Price}");

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(track.AlbumImageURL);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite albumSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

            Debug.Log($"Image loaded successfully for track: {track.Name}");

            // Configure the item in the list
            trackItemScript.Setup(albumSprite, track.Name, track.Author, track.Price, track.ShelfLocation, this, index);

            // Enable the prefab clone after it is fully populated
            trackItem.SetActive(true);
        }
        else
        {
            Debug.LogError($"Failed to load album image for track {track.Name}: {request.error}");
        }
    }


    private void Start()
    {
        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(FilterTracks);
        }

        if (playPauseButtons != null && playPauseButtons.Count > 0)
        {
            foreach (var button in playPauseButtons)
            {
                if (button != null)
                {
                    button.onClick.AddListener(TogglePlayPause);
                }
            }
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextTrack);
        }

        if (previousButton != null)
        {
            previousButton.onClick.AddListener(PreviousTrack);
        }

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(() =>
            {
                if (currentTrackIndex >= 0 && currentTrackIndex < tracks.Count)
                {
                    UpdateVinylInfo(tracks[currentTrackIndex]);
                }
            });
        }

        foreach (var slider in musicProgressSliders)
        {
            if (slider != null)
            {
                Debug.Log($"Slider {slider.name} is being initialized.");
                slider.onValueChanged.AddListener(OnMusicProgressChanged);
            }
            else
            {
                Debug.LogError("Slider is null.");
            }
        }

        if (sortDropdown != null)
        {
            sortDropdown.onValueChanged.AddListener(SortTracks);
        }

        AddTracks();
        PopulateTrackList();

        int randomIndex = Random.Range(0, tracks.Count);
        PlayTrack(randomIndex);
    }

    private void AddTracks()
    {
        tracks.Add(new TrackData
        {
            Name = "Melhor sozinha",
            Author = "Luisa Sonza",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/LuisaSonzaMelhorSozinha.ogg",
            AlbumImageURL = "https://images.genius.com/f8f0d85159dd0b50fcd7af15aa0b799e.720x720x1.png",
            Price = 24.99f,
            Biography = "Melhor Sozinha é um single da cantora brasileira Luísa Sonza, lançado em 2021. A música fala sobre empoderamento feminino e a decisão de ficar sozinha após um relacionamento tóxico. Tornou-se um hino de autoestima e independência.",
            ShelfLocation = "A1",
            StorePosition = new Vector2(150, 205)
        });

        tracks.Add(new TrackData
        {
            Name = "Do you no wrong",
            Author = "Richie Campbell",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/RichieCampbellDoYouNoWrong.ogg",
            AlbumImageURL = "https://images.genius.com/784e024c3e172917e130450f3c23bee3.1000x1000x1.png",
            Price = 22.50f,
            Biography = "Richie Campbell é um dos maiores nomes do reggae em Portugal. Do You No Wrong é uma das suas músicas mais conhecidas, combinando influências de reggae com soul e R&B. A letra fala sobre lealdade e respeito num relacionamento.",
            ShelfLocation = "A2",
            StorePosition = new Vector2(80, 114)
        });

        tracks.Add(new TrackData
        {
            Name = "N95",
            Author = "Kendrick Lamar",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/KendrickLamarN95.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/N95.png",
            Price = 29.99f,
            Biography = "N95 é uma faixa do álbum Mr. Morale & The Big Steppers de Kendrick Lamar, lançado em 2022. A música critica o materialismo e a hipocrisia na sociedade, usando a máscara N95 como metáfora para o que realmente protege as pessoas.",
            ShelfLocation = "A3",
            StorePosition = new Vector2(200, 8.7f)
        });

        tracks.Add(new TrackData
        {
            Name = "White Ferrari",
            Author = "Frank Ocean",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/FrankOceanWhiteFerrari.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/whiteferrari.png",
            Price = 34.99f,
            Biography = "White Ferrari é uma balada emocional do álbum Blonde de Frank Ocean 2016. A música fala sobre amor, perda e memórias, com referências aos Beatles. É considerada uma das canções mais pessoais e poéticas de Ocean.",
            ShelfLocation = "A4",
            StorePosition = new Vector2(50, -85)
        });

        tracks.Add(new TrackData
        {
            Name = "Self Care",
            Author = "Mac Miller",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/MacMillerSelfCare.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/SelfCare.png",
            Price = 27.50f,
            Biography = "Self Care é uma faixa do álbum Swimming de Mac Miller 2018, lançado pouco antes da sua morte. A música reflete sobre saúde mental, vícios e o processo de cura. O videoclipe mostra Miller enterrando-se simbolicamente num caixão.",
            ShelfLocation = "A5",
            StorePosition = new Vector2(280, -180)
        });

        tracks.Add(new TrackData
        {
            Name = "Wait for you",
            Author = "Future",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/FutureWAITFORU.ogg",
            AlbumImageURL = "https://images.genius.com/c07c1aa672e8578e2a5b34cc2b005d40.1000x1000x1.png",
            Price = 21.99f,
            Biography = "Wait for You é uma colaboração entre Future, Drake e Tems, lançada em 2022. A música fala sobre esperar por alguém que não está emocionalmente disponível. Tornou-se um sucesso global, destacando-se pela produção atmosférica e vocais emotivos.",
            ShelfLocation = "A1",
            StorePosition = new Vector2(250, 205)
        });

        tracks.Add(new TrackData
        {
            Name = "GOT DAMN",
            Author = "Gunna",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/GunnaGOTDAMN.ogg",
            AlbumImageURL = "https://images.genius.com/78aeff6ead193177f72596016bc71c84.500x500x1.png",
            Price = 23.50f,
            Biography = "Gunna é conhecido pelo seu estilo de trap melódico. GOT DAMN é uma faixa que mostra o seu flow característico e letras sobre sucesso, riqueza e estilo de vida luxuoso. Faz parte do álbum DS4EVER lançado em 2022.",
            ShelfLocation = "A2",
            StorePosition = new Vector2(180, 114)
        });

        tracks.Add(new TrackData
        {
            Name = "Sing About me Im Dying of Thirst",
            Author = "Kendrick Lamar",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/SingAboutMeImDyingOfThirst.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/SingAboutMe.png",
            Price = 32.99f,
            Biography = "Esta faixa de 12 minutos do álbum good kid, m.A.A.d city 2012 é uma das obras-primas de Kendrick Lamar. Dividida em duas partes, a música explora temas como violência, mortalidade e redenção, com narrativas vívidas da vida em Compton.",
            ShelfLocation = "A3",
            StorePosition = new Vector2(120, 8.7f)
        });

        tracks.Add(new TrackData
        {
            Name = "ss",
            Author = "Ken Carson",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/KenCarsonss.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/SS.png",
            Price = 19.99f,
            Biography = "Ken Carson é uma estrela emergente da cena do rap underground. ss apresenta sua estética de som agressivo e letras sobre estilo de vida extravagante. Faz parte do movimento rage rap que ganhou popularidade nos anos 2020.",
            ShelfLocation = "A4",
            StorePosition = new Vector2(220, -85)
        });

        tracks.Add(new TrackData
        {
            Name = "Denial is a river",
            Author = "Doechii",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/DoechiiDenialIsARiverShow.ogg",
            AlbumImageURL = "https://images.genius.com/a899e6cbe67695bfa7e2a716c50d7583.1000x1000x1.png",
            Price = 20.50f,
            Biography = "Doechii é uma rapper e cantora conhecida pelo seu estilo versátil e letras inteligentes. Denial is a River brinca com o ditado negar é um rio no Egito, explorando temas de autoengano e relações complicadas com um flow único.",
            ShelfLocation = "A5",
            StorePosition = new Vector2(90, -180)
        });

        tracks.Add(new TrackData
        {
            Name = "BACKDOOR",
            Author = "Playboi Carti",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/PlayboiCartiBACKD00R.ogg",
            AlbumImageURL = "https://images.genius.com/84387c03968c8d51fd8be652624f112a.1000x1000x1.png",
            Price = 25.99f,
            Biography = "Playboi Carti é conhecido pelo seu estilo de rap experimental. BACKDOOR é uma faixa do álbum Whole Lotta Red 2020 que mostra sua entrega vocal única e produção inovadora, ajudando a definir o som do rap no início dos anos 2020.",
            ShelfLocation = "A1",
            StorePosition = new Vector2(300, 205)
        });

        tracks.Add(new TrackData
        {
            Name = "OUTWEST",
            Author = "JACKBOYS and Travis Scott",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/JACKBOYSTravisScottOUTWEST.ogg",
            AlbumImageURL = "https://images.genius.com/ae2e8ef74082a5c8240ca1867a6abae7.906x906x1.png",
            Price = 28.50f,
            Biography = "OUTWEST é uma colaboração entre o coletivo JACKBOYS fundado por Travis Scott e o rapper Young Thug. Lançada em 2019, a música apresenta um beat cativante e letras sobre estilo de vida luxuoso, tornando-se popular nas plataformas de streaming.",
            ShelfLocation = "A2",
            StorePosition = new Vector2(40, 114)
        });

        tracks.Add(new TrackData
        {
            Name = "No Pole",
            Author = "Don Toliver",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/DonToliverNoPole.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/nopole.png",
            Price = 23.99f,
            Biography = "Don Toliver é conhecido pela sua voz distinta e estilo que mistura R&B com trap. No Pole é uma faixa que mostra sua capacidade de criar melodias cativantes, com letras sobre relacionamentos e o estilo de vida de uma estrela do rap.",
            ShelfLocation = "A3",
            StorePosition = new Vector2(270, 8.7f)
        });

        tracks.Add(new TrackData
        {
            Name = "Get You",
            Author = "Daniel Ceaser",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/DanielCaesarGetYou.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/GetYou.jpg",
            Price = 26.50f,
            Biography = "Daniel Caesar é um cantor e compositor canadiano conhecido pelo seu R&B soulful. Get You é uma balada romântica do seu álbum de estreia Freudian 2017, destacando-se pelos vocais suaves e letras sobre amor e devoção.",
            ShelfLocation = "A4",
            StorePosition = new Vector2(130, -85)
        });

        tracks.Add(new TrackData
        {
            Name = "Yebbas Heartbreak",
            Author = "Drake & Yebba",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/YebbasHeartbreak.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/YebbasHeartbreak.png",
            Price = 27.99f,
            Biography = "Yebbas Heartbreak é uma colaboração entre Drake e a cantora Yebba, incluída no álbum Certified Lover Boy 2021. A música destaca os vocais impressionantes de Yebba enquanto explora temas de desgosto amoroso e vulnerabilidade emocional.",
            ShelfLocation = "A5",
            StorePosition = new Vector2(190, -180)
        });

        tracks.Add(new TrackData
        {
            Name = "Wish",
            Author = "Trippie Redd",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/DiploWishTrippieRed.ogg",
            AlbumImageURL = "https://images.genius.com/862dcd7eb8f08acd6279f600b3b20cdd.1000x1000x1.png",
            Price = 21.50f,
            Biography = "Trippie Redd é conhecido pelo seu estilo emocional de rap. Wish é uma faixa que combina melodias melancólicas com letras sobre desgosto amoroso e arrependimento, mostrando a versatilidade do artista entre rap e cantos melódicos.",
            ShelfLocation = "A2",
            StorePosition = new Vector2(240, 114)
        });

        tracks.Add(new TrackData
        {
            Name = "Creep",
            Author = "Radiohead",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/RadioheadCreep.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/Creep.png",
            Price = 35.99f,
            Biography = "Creep é o single de estreia do Radiohead, lançado em 1992. A música fala sobre inseguranças e autoaversão, tornando-se um hino dos excluídos. Apesar do sucesso, a banda acabou por se distanciar da canção durante anos antes de voltar a tocá-la ao vivo.",
            ShelfLocation = "A3",
            StorePosition = new Vector2(160, 8.7f)
        });

        tracks.Add(new TrackData
        {
            Name = "Like a Tattoo",
            Author = "Sade",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/SadeLikeaTattoo.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/LikeAtattoo.png",
            Price = 32.50f,
            Biography = "Sade é conhecida pelo seu jazz suave e soul. Like a Tattoo é uma balada do álbum Love Deluxe 1992 que fala sobre memórias permanentes como tatuagens. A voz suave de Sade e a produção minimalista criam uma atmosfera íntima e emocional.",
            ShelfLocation = "A4",
            StorePosition = new Vector2(290, -85)
        });

        tracks.Add(new TrackData
        {
            Name = "One wish",
            Author = "Ray J",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/RayJOneWish.ogg",
            AlbumImageURL = "https://images.genius.com/a836243c9c3fb292362907271e3a2137.640x640x1.jpg",
            Price = 18.99f,
            Biography = "One Wish é um sucesso de Ray J lançado em 2005. A música R&B fala sobre arrependimento e o desejo de ter uma segunda chance no amor. Tornou-se um dos maiores hits do cantor, mostrando sua habilidade para baladas emocionais.",
            ShelfLocation = "A5",
            StorePosition = new Vector2(110, -180)
        });

        tracks.Add(new TrackData
        {
            Name = "Sozinho",
            Author = "Caetano Veloso",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/CaetanoVelosoSozinho.ogg",
            AlbumImageURL = "https://images.genius.com/9c853f0e9b33eb484ca1610bd8743243.1000x994x1.jpg",
            Price = 24.99f,
            Biography = "Sozinho é uma das canções mais conhecidas de Caetano Veloso, lançada em 1998. A letra fala sobre a solidão e o desejo por companhia, com o característico estilo poético do artista. Tornou-se um clássico da MPB e é frequentemente regravada.",
            ShelfLocation = "A1",
            StorePosition = new Vector2(200, 205)
        });

        tracks.Add(new TrackData
        {
            Name = "LVL",
            Author = "ASAP Rocky",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/ASAPRockyLVL.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/LVL.jpg",
            Price = 25.50f,
            Biography = "LVL é uma faixa do álbum AT.LONG.LAST.ASAP 2015 de ASAP Rocky. A música apresenta uma produção atmosférica e letras que alternam entre introspecção e ostentação, mostrando a versatilidade do rapper nova-iorquino.",
            ShelfLocation = "A2",
            StorePosition = new Vector2(140, 114)
        });

        tracks.Add(new TrackData
        {
            Name = "Die for You",
            Author = "The Weeknd",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/TheWeekndDieForYou.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/dieforyou.png",
            Price = 26.99f,
            Biography = "Die for You é uma faixa do álbum Starboy 2016 de The Weeknd. A música fala sobre um amor tão intenso que se estaria disposto a morrer por ele, com os vocais emotivos característicos do artista. Ganhou nova popularidade anos após o lançamento através do TikTok.",
            ShelfLocation = "A3",
            StorePosition = new Vector2(30, 8.7f)
        });

        tracks.Add(new TrackData
        {
            Name = "Positions",
            Author = "Ariana Grande",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/arianagrandePositions.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/Positions.jpg",
            Price = 40.00f,
            Biography = "Positions é a faixa-título do sexto álbum de estúdio de Ariana Grande, lançado em 2020. A música combina elementos de pop e R&B, explorando temas de amor e dedicação em um relacionamento. Tornou-se um grande sucesso, alcançando o topo das paradas globais.",
            ShelfLocation = "A4",
            StorePosition = new Vector2(30, -85)
        });

        tracks.Add(new TrackData
        {
            Name = "Die for You",
            Author = "The Weeknd",
            FileURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/musicasParaKiosk/TheWeekndDieForYou.ogg",
            AlbumImageURL = "http://193.137.7.33/~aluno27649/TrabalhoGrupo1/KioskImagens/dieforyou.png",
            Price = 26.99f,
            Biography = "Die for You é uma faixa do álbum Starboy 2016 de The Weeknd. A música fala sobre um amor tão intenso que se estaria disposto a morrer por ele, com os vocais emotivos característicos do artista. Ganhou nova popularidade anos após o lançamento através do TikTok.",
            ShelfLocation = "A3",
            StorePosition = new Vector2(30, 8.7f)
        });
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

        if (audioSource != null && audioSource.clip != null && !isScrubbing)
        {
            foreach (var slider in musicProgressSliders)
            {
                if (slider != null)
                {
                    slider.value = audioSource.time / audioSource.clip.length;
                }
            }
        }

        if (audioSource != null && !audioSource.isPlaying && isPlaying && audioSource.time >= audioSource.clip.length)
        {
            NextTrack();
        }
    }

    private Coroutine loadAudioCoroutine;
    private Coroutine loadImageCoroutine;

    public void PlayTrack(int index)
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

        // Stop the currently playing music
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            isPlaying = false;
        }

        // Remove highlight from the previously highlighted item
        if (currentHighlightedItem != null)
        {
            currentHighlightedItem.SetHighlight(false);
        }

        currentTrackIndex = index;

        // Highlight the currently playing track
        if (index < trackListContainer.childCount) // Ensure the index is within bounds
        {
            Transform trackItemTransform = trackListContainer.GetChild(index);
            TrackItem trackItem = trackItemTransform.GetComponent<TrackItem>();
            if (trackItem != null)
            {
                trackItem.SetHighlight(true);
                currentHighlightedItem = trackItem;
            }
        }
        else
        {
            Debug.LogError($"Index {index} is out of bounds for trackListContainer with {trackListContainer.childCount} children.");
        }

        // Update track name and author
        foreach (var trackNameText in trackNameTexts)
        {
            if (trackNameText != null)
            {
                trackNameText.text = tracks[index].Name;
            }
        }

        foreach (var authorNameText in authorNameTexts)
        {
            if (authorNameText != null)
            {
                authorNameText.text = tracks[index].Author;
            }
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

        // Reset all sliders
        foreach (var slider in musicProgressSliders)
        {
            if (slider != null)
            {
                slider.value = 0f;
            }
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

            while (audioSource.clip.loadState != AudioDataLoadState.Loaded)
            {
                yield return null;
            }

            foreach (var trackNameText in trackNameTexts)
            {
                if (trackNameText != null)
                {
                    trackNameText.text = tracks[currentTrackIndex].Name;
                }
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
        if (playPauseButtons != null && playPauseButtons.Count > 0)
        {
            for (int i = 0; i < playPauseButtons.Count; i++)
            {
                if (playPauseButtons[i] != null && playPauseButtons[i].image != null)
                {
                    playPauseButtons[i].image.sprite = isPlaying
                        ? (i < pauseIcons.Count ? pauseIcons[i] : null)
                        : (i < playIcons.Count ? playIcons[i] : null);
                }
            }
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
    public float Price; // Novo campo para o preço
    public string Biography; // New field for the biography
    public string ShelfLocation; // Localização da estante na loja
    public Vector2 StorePosition; // Posição exata na planta da loja

}