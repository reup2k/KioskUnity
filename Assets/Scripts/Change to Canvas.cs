using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChangeToCanvas : MonoBehaviour
{
    public Canvas[] canvases; // Assign all your canvases in the inspector
    public float inactivityTime = 10f; // Time before returning to Canvas1

    private Canvas currentCanvas;
    private float inactivityTimer;

    private void Start()
    {
        if (canvases != null && canvases.Length > 0)
        {
            currentCanvas = canvases[0]; // Set Canvas1 as the default
            ShowCanvas(currentCanvas);
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

        currentCanvas = canvases[canvasIndex];
        ShowCanvas(currentCanvas);

        // Reset the inactivity timer
        inactivityTimer = inactivityTime;
    }

    private void ShowCanvas(Canvas canvas)
    {
        foreach (Canvas c in canvases)
        {
            c.gameObject.SetActive(false);
        }
        canvas.gameObject.SetActive(true);
    }
}