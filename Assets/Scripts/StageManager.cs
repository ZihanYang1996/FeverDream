using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Serialization;
using System.Collections;

public class StageManager : MonoBehaviour
{
    public Action<bool, StageData> OnPuzzleComplete;

    [Header("UI References")]
    public GameObject puzzlePanel; // PuzzlePanel object

    public Image workspaceOutlineImage; // Image in WorkspaceArea to display selected outline
    public Transform workspaceArea; // WorkspaceArea container
    public Button validateButton; // ValidateButton
    public Button startPuzzleButton; // Start button to open puzzle panel

    [Header("Render Config")]
    public Camera captureCamera; // MaskCamera

    public RenderTexture captureRT; // Mask_RT

    [Header("Stage Data (Don't modify)")]
    public StageData currentStage; // Your StageData asset

    [Header("Clone Capture Config")]
    public Canvas captureCanvas; // Off-screen canvas for clone capture

    public Transform captureWorkspaceArea; // Parent in captureCanvas to clone pieces under

    [Header("Pattern Selector Config")]
    public Transform patternSelectorContainer; // Container for pattern buttons

    public GameObject patternButtonPrefab; // Prefab for pattern button
    public List<StageData> allStages; // List of all stages to generate buttons

    [Header("Export Settings")]
    [SerializeField] private string exportFolderName = "Temp"; // Folder under Assets

    [SerializeField] private string exportFileName = "playerMask"; // PNG file name without extension

    [Header("Normalization Settings")]
    [SerializeField] private int normalizationResolution = 256;

    [Header("Gameplay Timing")]
    [SerializeField] private float countdownDuration = 30f;

    private float countdownTimer;
    private bool puzzleCompleted = false;
    [SerializeField] private TMPro.TextMeshProUGUI countdownText;

    [SerializeField] private Image screenFlashImage;
    [SerializeField] private float flashDuration = 0.2f;

    private Texture2D playerMask;

    void Start()
    {
        // hide puzzle at start
        puzzlePanel.SetActive(false);
        // hide screen flash image
        screenFlashImage.gameObject.SetActive(false);
        // bind Start Demo to show puzzle panel
        startPuzzleButton.onClick.AddListener(ShowPuzzlePanel);
        // bind validate
        validateButton.onClick.AddListener(ValidatePuzzle);
        // Permanently direct captureCamera to render into captureRT
        captureCamera.targetTexture = captureRT;

        // Generate pattern selector buttons
        for (int i = 0; i < allStages.Count; i++)
        {
            int idx = i;
            StageData sd = allStages[i];
            GameObject btnGO = Instantiate(patternButtonPrefab, patternSelectorContainer);
            btnGO.name = "PatternBtn_" + sd.name;
            Image img = btnGO.GetComponent<Image>();
            img.sprite = sd.outlineSprite;
            img.preserveAspect = true;
            Button btn = btnGO.GetComponent<Button>();
            btn.onClick.AddListener(() => SelectStage(idx));
        }

        countdownTimer = countdownDuration;
    }

    /// <summary>
    /// Show puzzle UI and display target outline
    /// </summary>
    public void LoadStage(StageData stage)
    {
        currentStage = stage;
        // set outline sprite
        workspaceOutlineImage.sprite = stage.outlineSprite;
        workspaceOutlineImage.color = Color.white;
        // clear previous pieces
        // Return all pieces to their original positions and parents
        for (int i = workspaceArea.childCount - 1; i >= 0; i--)
        {
            Transform piece = workspaceArea.GetChild(i);
            TangramDraggable draggable = piece.GetComponent<TangramDraggable>();
            if (draggable != null)
            {
                draggable.Reset();
            }
        }

        // show panel
        puzzlePanel.SetActive(true);
    }

    /// <summary>
    /// Switch to selected stage, update workspace outline and reset workspace.
    /// </summary>
    public void SelectStage(int index)
    {
        // ensure panel is visible when selecting a pattern
        puzzlePanel.SetActive(true);
        // Set current stage
        currentStage = allStages[index];
        // Update workspace outline image
        workspaceOutlineImage.sprite = currentStage.outlineSprite;
        workspaceOutlineImage.color = Color.white;
        // clear previous pieces
        // Return all pieces to their original positions and parents
        for (int i = workspaceArea.childCount - 1; i >= 0; i--)
        {
            Transform piece = workspaceArea.GetChild(i);
            TangramDraggable draggable = piece.GetComponent<TangramDraggable>();
            if (draggable != null)
            {
                draggable.Reset();
            }
        }
    }

    /// <summary>
    /// Displays the puzzle panel without loading a stage.
    /// </summary>
    public void ShowPuzzlePanel()
    {
        puzzlePanel.SetActive(true);
        BeginCountdown();
    }

    /// <summary>
    /// Capture player's workspace, compute IoU and log result
    /// </summary>
    public void ValidatePuzzle()
    {
        // Clone workspace pieces into off-screen capture canvas
        List<GameObject> clones = new List<GameObject>();
        foreach (Transform orig in workspaceArea)
        {
            // Instantiate clone under captureWorkspaceArea, preserving world position
            GameObject clone = Instantiate(orig.gameObject, captureWorkspaceArea, worldPositionStays: true);
            clones.Add(clone);
        }

        Debug.Log("[StageManager] ValidatePuzzle called");
        // Enable capture canvas so captureCamera sees clones
        captureCanvas.gameObject.SetActive(true);

        // Render clones into the RenderTexture
        captureCamera.Render();
        RenderTexture.active = captureRT;

        // Read pixels into Texture2D
        if (playerMask == null ||
            playerMask.width != captureRT.width ||
            playerMask.height != captureRT.height)
        {
            playerMask = new Texture2D(captureRT.width, captureRT.height, TextureFormat.RGBA32, false);
        }

        playerMask.ReadPixels(new Rect(0, 0, captureRT.width, captureRT.height), 0, 0);
        playerMask.Apply();

        // Convert captured mask to pure black and white
        playerMask = BinarizeMask(playerMask);

        // Export captured mask to Assets folder
        string folderPath = Path.Combine(Application.dataPath, exportFolderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, exportFileName + ".png");
        File.WriteAllBytes(filePath, playerMask.EncodeToPNG());
        Debug.Log($"Player mask exported to: {filePath}");

        // Cleanup RenderTexture binding
        RenderTexture.active = null;

        // Disable capture canvas so it doesn’t display in main UI
        captureCanvas.gameObject.SetActive(false);

        // Normalize and align both masks
        Texture2D normTarget = NormalizeAndAlignMask(currentStage.maskTexture);
        Texture2D normPlayer = NormalizeAndAlignMask(playerMask);

        // Compute IoU on normalized masks (no translation needed)
        float iou = ComputeTranslatedIoU(normTarget, normPlayer, 0, 0);
        bool success = iou >= currentStage.iouThreshold;
        Debug.Log($"[Validate] IoU = {iou:F2} => {(success ? "Success" : "Fail")}");

        if (success && !puzzleCompleted)
        {
            HandlePuzzleSuccess();
        }
        else if (!success)
        {
            // If not successful, flash red, but player can still retry
            StartCoroutine(FlashRedEffect());
        }

        // Destroy clones
        foreach (var c in clones)
        {
            Destroy(c);
        }
    }

    /// <summary>
    /// Calculates the centroid of white pixels in a binary mask.
    /// If no white pixels are found, returns the center of the texture.
    /// </summary>
    private Vector2 ComputeCentroid(Texture2D mask)
    {
        Color32[] pixels = mask.GetPixels32();
        int width = mask.width;
        int height = mask.height;
        long sumX = 0, sumY = 0, count = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pixels[y * width + x].r > 128)
                {
                    sumX += x;
                    sumY += y;
                    count++;
                }
            }
        }

        if (count == 0)
        {
            // No shape pixels: default to center
            return new Vector2(width / 2f, height / 2f);
        }

        // Return average position
        return new Vector2(sumX / (float)count, sumY / (float)count);
    }

    /// <summary>
    /// Computes the Intersection-over-Union (IoU) of two binary masks
    /// after translating the player's mask by (dx, dy).
    /// </summary>
    private float ComputeTranslatedIoU(Texture2D target, Texture2D player, int dx, int dy)
    {
        int width = target.width;
        int height = target.height;
        Color32[] targetPixels = target.GetPixels32();
        Color32[] playerPixels = player.GetPixels32();
        int overlap = 0, union = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool t = targetPixels[y * width + x].r > 128;
                int px = x - dx;
                int py = y - dy;
                bool p = false;
                if (px >= 0 && px < width && py >= 0 && py < height)
                {
                    p = playerPixels[py * width + px].r > 128;
                }

                if (t || p) union++;
                if (t && p) overlap++;
            }
        }

        return union > 0 ? overlap / (float)union : 0f;
    }

    /// <summary>
    /// Convert any source texture into a pure black/white mask.
    /// White = shape (any non-black pixel), Black = background.
    /// </summary>
    private Texture2D BinarizeMask(Texture2D source)
    {
        int width = source.width;
        int height = source.height;
        Color32[] src = source.GetPixels32();
        Color32[] dst = new Color32[src.Length];

        for (int i = 0; i < src.Length; i++)
        {
            bool isShape = (src[i].r + src[i].g + src[i].b) > 0;
            dst[i] = isShape
                ? new Color32(255, 255, 255, 255)
                : new Color32(0, 0, 0, 255);
        }

        Texture2D mask = new Texture2D(width, height, TextureFormat.RGBA32, false);
        mask.SetPixels32(dst);
        mask.Apply();
        return mask;
    }


    /// <summary>
    /// Normalizes a binary mask by cropping to its bounding box, scaling to a fixed resolution,
    /// centering its centroid, and rotating to align principal axis.
    /// </summary>
    private Texture2D NormalizeAndAlignMask(Texture2D mask)
    {
        int w = mask.width, h = mask.height;
        Color32[] pixels = mask.GetPixels32();

        // 1) Find bounding box of white pixels
        int minX = w, minY = h, maxX = 0, maxY = 0;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (pixels[y * w + x].r > 128)
                {
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }
            }
        }

        if (maxX < minX || maxY < minY)
        {
            // empty mask -> return blank
            return new Texture2D(normalizationResolution, normalizationResolution, TextureFormat.RGBA32, false);
        }

        // 2) Crop and scale to normalizationResolution
        int cropW = maxX - minX + 1, cropH = maxY - minY + 1;
        Texture2D scaled = new Texture2D(normalizationResolution, normalizationResolution, TextureFormat.RGBA32, false);
        for (int ny = 0; ny < normalizationResolution; ny++)
        {
            for (int nx = 0; nx < normalizationResolution; nx++)
            {
                float u = nx / (float)(normalizationResolution - 1);
                float v = ny / (float)(normalizationResolution - 1);
                int sx = minX + Mathf.RoundToInt(u * (cropW - 1));
                int sy = minY + Mathf.RoundToInt(v * (cropH - 1));
                Color32 c = pixels[sy * w + sx];
                scaled.SetPixel(nx, ny, c.r > 128 ? Color.white : Color.black);
            }
        }

        scaled.Apply();

        // 3) Compute centroid of scaled mask
        Vector2 centroid = ComputeCentroid(scaled);

        // 4) Compute PCA orientation (variance covariance)
        float meanX = 0, meanY = 0, count = 0;
        for (int y = 0; y < normalizationResolution; y++)
        for (int x = 0; x < normalizationResolution; x++)
            if (scaled.GetPixel(x, y).r > 0.5f)
            {
                meanX += x;
                meanY += y;
                count++;
            }

        meanX /= count;
        meanY /= count;
        float covXX = 0, covYY = 0, covXY = 0;
        for (int y = 0; y < normalizationResolution; y++)
        for (int x = 0; x < normalizationResolution; x++)
            if (scaled.GetPixel(x, y).r > 0.5f)
            {
                float dx = x - meanX, dy = y - meanY;
                covXX += dx * dx;
                covYY += dy * dy;
                covXY += dx * dy;
            }

        // principal angle
        float theta = 0.5f * Mathf.Atan2(2 * covXY, covXX - covYY) * Mathf.Rad2Deg;

        // 5) Rotate the scaled mask around its center by -theta
        Texture2D rotated =
            new Texture2D(normalizationResolution, normalizationResolution, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(normalizationResolution / 2f, normalizationResolution / 2f);
        for (int y = 0; y < normalizationResolution; y++)
        {
            for (int x = 0; x < normalizationResolution; x++)
            {
                // inverse rotate pixel
                float dx = x - center.x, dy = y - center.y;
                float rad = -theta * Mathf.Deg2Rad;
                int sx = Mathf.RoundToInt(center.x + dx * Mathf.Cos(rad) - dy * Mathf.Sin(rad));
                int sy = Mathf.RoundToInt(center.y + dx * Mathf.Sin(rad) + dy * Mathf.Cos(rad));
                Color c = (sx >= 0 && sx < normalizationResolution && sy >= 0 && sy < normalizationResolution &&
                           scaled.GetPixel(sx, sy).r > 0.5f)
                    ? Color.white
                    : Color.black;
                rotated.SetPixel(x, y, c);
            }
        }

        rotated.Apply();

        return rotated;
    }

    private IEnumerator CountdownTimer()
    {
        int secondsLeft = Mathf.CeilToInt(countdownDuration);

        while (secondsLeft > 0)
        {
            countdownTimer = secondsLeft;

            if (countdownText != null)
            {
                countdownText.text = secondsLeft.ToString();
            }

            yield return new WaitForSeconds(1f);
            if (puzzleCompleted) yield break;

            secondsLeft--;
        }

        countdownTimer = 0;
        if (countdownText != null)
        {
            countdownText.text = "";
        }

        if (!puzzleCompleted)
        {
            Debug.Log("[StageManager] Countdown expired. Puzzle failed.");
            HandlePuzzleFailure();
        }
    }

    private void HandlePuzzleSuccess()
    {
        puzzleCompleted = true;
        OnPuzzleComplete?.Invoke(true, currentStage);
    }

    private void HandlePuzzleFailure()
    {
        StartCoroutine(FlashRedEffect());
        OnPuzzleComplete?.Invoke(false, currentStage);
    }

    private IEnumerator FlashRedEffect()
    {
        if (screenFlashImage == null) yield break;

        Color originalColor = screenFlashImage.color;
        screenFlashImage.color = new Color(1f, 0f, 0f, 0.5f);
        screenFlashImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(flashDuration);

        screenFlashImage.gameObject.SetActive(false);
        screenFlashImage.color = originalColor;
    }


    /// <summary>
    /// Manually begins the countdown timer, resetting it and updating the display.
    /// </summary>
    public void BeginCountdown()
    {
        countdownTimer = countdownDuration;

        if (countdownText != null)
        {
            countdownText.text = Mathf.CeilToInt(countdownTimer).ToString();
        }

        StartCoroutine(CountdownTimer());
    }
}