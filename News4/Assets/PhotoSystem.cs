using System.Collections;
using System.IO;
using UnityEngine;

public class PhotoSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera captureCamera;
    [SerializeField] private ShotWindowSystem shotWindow;

    [Header("Input")]
    [SerializeField] private KeyCode shootKey = KeyCode.Space;

    [Header("Capture Area (Normalized)")]
    [SerializeField] private Rect frameViewport = new Rect(0f, 0f, 1f, 1f);

    [Header("Output Resolution")]
    [SerializeField] private bool useScreenResolution = true;
    [SerializeField] private int outputWidth = 1920;
    [SerializeField] private int outputHeight = 1080;

    [Header("Saving")]
    [SerializeField] private bool saveToDisk = true;
    [SerializeField] private string folderName = "Shots";
    [SerializeField] private string fileNamePrefix = "shot";
    [SerializeField] private bool includeTimestamp = true;

    private RenderTexture renderTexture;
    private Texture2D readTexture;
    private bool isCapturing;
    private int shotIndex;

    public Rect FrameViewport => frameViewport;

    private void Awake()
    {
        if (captureCamera == null)
        {
            captureCamera = GetComponent<Camera>();
        }

        EnsureRenderTexture();
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(shootKey))
        {
            if (shotWindow == null || shotWindow.CanShoot)
            {
                StartCoroutine(CaptureRoutine());
            }
        }
    }

    private void EnsureRenderTexture()
    {
        int width = useScreenResolution ? Screen.width : outputWidth;
        int height = useScreenResolution ? Screen.height : outputHeight;

        if (width <= 0 || height <= 0)
        {
            return;
        }

        if (renderTexture != null && (renderTexture.width != width || renderTexture.height != height))
        {
            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }

        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = "PhotoSystemRT"
            };
        }
    }

    private IEnumerator CaptureRoutine()
    {
        if (isCapturing)
        {
            yield break;
        }

        isCapturing = true;
        yield return new WaitForEndOfFrame();

        EnsureRenderTexture();
        if (renderTexture == null || captureCamera == null)
        {
            isCapturing = false;
            yield break;
        }

        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = captureCamera.targetTexture;

        captureCamera.targetTexture = renderTexture;
        captureCamera.Render();

        RenderTexture.active = renderTexture;

        Rect pixelRect = GetPixelRect(renderTexture);
        int readWidth = Mathf.Max(1, Mathf.RoundToInt(pixelRect.width));
        int readHeight = Mathf.Max(1, Mathf.RoundToInt(pixelRect.height));

        if (readTexture == null || readTexture.width != readWidth || readTexture.height != readHeight)
        {
            readTexture = new Texture2D(readWidth, readHeight, TextureFormat.RGBA32, false)
            {
                name = "PhotoSystemRead"
            };
        }

        readTexture.ReadPixels(pixelRect, 0, 0);
        readTexture.Apply();

        captureCamera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;

        if (saveToDisk)
        {
            SaveTexture(readTexture);
        }

        shotWindow?.RegisterShot();
        isCapturing = false;
    }

    private Rect GetPixelRect(RenderTexture rt)
    {
        float x = Mathf.Clamp01(frameViewport.x);
        float y = Mathf.Clamp01(frameViewport.y);
        float w = Mathf.Clamp01(frameViewport.width);
        float h = Mathf.Clamp01(frameViewport.height);

        w = Mathf.Clamp(w, 0f, 1f - x);
        h = Mathf.Clamp(h, 0f, 1f - y);

        return new Rect(
            Mathf.RoundToInt(x * rt.width),
            Mathf.RoundToInt(y * rt.height),
            Mathf.RoundToInt(w * rt.width),
            Mathf.RoundToInt(h * rt.height)
        );
    }

    private void SaveTexture(Texture2D tex)
    {
        string dir = Path.Combine(Application.persistentDataPath, folderName);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string timestamp = includeTimestamp ? System.DateTime.Now.ToString("yyyyMMdd_HHmmss") : string.Empty;
        string indexPart = shotIndex.ToString("0000");
        string fileName = includeTimestamp
            ? $"{fileNamePrefix}_{timestamp}_{indexPart}.png"
            : $"{fileNamePrefix}_{indexPart}.png";

        string path = Path.Combine(dir, fileName);
        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes(path, png);
        shotIndex++;
    }
}
