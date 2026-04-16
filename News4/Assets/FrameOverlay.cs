using UnityEngine;

public class FrameOverlay : MonoBehaviour
{
    [Header("Viewport Source")]
    [SerializeField] private PhotoSystem photoSystem;
    [SerializeField] private Rect frameViewport = new Rect(0f, 0f, 1f, 1f);

    [Header("Visibility")]
    [SerializeField] private ShotWindowSystem shotWindow;
    [SerializeField] private bool showOnlyWhenCanShoot = false;

    [Header("Style")]
    [SerializeField] private Color color = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private float thickness = 2f;

    private void OnGUI()
    {
        if (showOnlyWhenCanShoot && shotWindow != null && !shotWindow.CanShoot)
        {
            return;
        }

        Rect viewport = photoSystem != null ? photoSystem.FrameViewport : frameViewport;
        Rect rect = ViewportToScreenRect(viewport);
        DrawRectBorder(rect, thickness, color);
    }

    private Rect ViewportToScreenRect(Rect viewport)
    {
        float x = viewport.x * Screen.width;
        float y = (1f - viewport.y - viewport.height) * Screen.height;
        float w = viewport.width * Screen.width;
        float h = viewport.height * Screen.height;
        return new Rect(x, y, w, h);
    }

    private void DrawRectBorder(Rect rect, float border, Color borderColor)
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        Color previous = GUI.color;
        GUI.color = borderColor;
        Texture2D tex = Texture2D.whiteTexture;

        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, border), tex);
        GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - border, rect.width, border), tex);
        GUI.DrawTexture(new Rect(rect.x, rect.y, border, rect.height), tex);
        GUI.DrawTexture(new Rect(rect.x + rect.width - border, rect.y, border, rect.height), tex);

        GUI.color = previous;
    }
}
