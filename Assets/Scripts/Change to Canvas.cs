using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChangeToCanvas : MonoBehaviour
{
    public Canvas[] canvases; // Assign all your canvases in the inspector
    public float inactivityTime = 10f; // Time before returning to Canvas1
    public float transitionDuration = 1f; // Duration of the fade transition

    private Canvas currentCanvas;
    private float inactivityTimer;

    private void Start()
    {
        if (canvases != null && canvases.Length > 0)
        {
            currentCanvas = canvases[0]; // Set Canvas1 as the default
            ShowCanvas(currentCanvas, instant: true);
        }
        inactivityTimer = inactivityTime;
    }

    private void Update()
    {
        // Ensure the canvases array is not empty
        if (canvases == null || canvases.Length == 0)
            return;

        // Detect user interaction and reset the inactivity timer
        if (Input.anyKey || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            inactivityTimer = inactivityTime;
        }

        // Decrease the inactivity timer
        inactivityTimer -= Time.deltaTime;

        // If the timer runs out, return to Canvas1
        if (inactivityTimer <= 0 && currentCanvas != canvases[0])
        {
            ChangeCanvas(0); // Return to Canvas1
        }
    }

    public void ChangeCanvas(int canvasIndex)
    {
        if (canvasIndex < 0 || canvasIndex >= canvases.Length || canvases[canvasIndex] == currentCanvas)
            return;

        Canvas newCanvas = canvases[canvasIndex];
        StartCoroutine(SmoothTransition(currentCanvas, newCanvas));

        currentCanvas = newCanvas;

        // Reset the inactivity timer
        inactivityTimer = inactivityTime;
    }

    private IEnumerator SmoothTransition(Canvas fromCanvas, Canvas toCanvas)
    {
        CanvasGroup fromCanvasGroup = fromCanvas.GetComponent<CanvasGroup>();
        CanvasGroup toCanvasGroup = toCanvas.GetComponent<CanvasGroup>();

        if (fromCanvasGroup == null)
        {
            fromCanvasGroup = fromCanvas.gameObject.AddComponent<CanvasGroup>();
        }

        if (toCanvasGroup == null)
        {
            toCanvasGroup = toCanvas.gameObject.AddComponent<CanvasGroup>();
        }

        toCanvas.gameObject.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            float alpha = elapsedTime / transitionDuration;

            if (fromCanvasGroup != null)
                fromCanvasGroup.alpha = 1f - alpha;

            if (toCanvasGroup != null)
                toCanvasGroup.alpha = alpha;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (fromCanvasGroup != null)
            fromCanvasGroup.alpha = 0f;

        if (toCanvasGroup != null)
            toCanvasGroup.alpha = 1f;

        fromCanvas.gameObject.SetActive(false);
    }

    private void ShowCanvas(Canvas canvas, bool instant = false)
    {
        foreach (Canvas c in canvases)
        {
            if (c == canvas)
            {
                c.gameObject.SetActive(true);
                CanvasGroup canvasGroup = c.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = c.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = instant ? 1f : 0f;
            }
            else
            {
                c.gameObject.SetActive(false);
            }
        }
    }
}