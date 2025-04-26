using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TangramDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, ICanvasRaycastFilter
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image image;

    private Vector2 originalPosition;
    private Transform originalParent;
    private bool isDragging = false;

    private bool isPointerDown = false;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        image = GetComponent<Image>();

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
    }

    void Update()
    {
        if (isPointerDown)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                rectTransform.Rotate(0, 0, scroll * 15f);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        RectTransform workspaceRect = GameObject.Find("WorkspaceArea").GetComponent<RectTransform>();

        if (IsRectTransformFullyInside(rectTransform, workspaceRect, eventData.pressEventCamera))
        {
            transform.SetParent(workspaceRect, true);
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;

        // If no drag occurred (only rotation), return to original position and rotation
        if (!isDragging && transform.parent == originalParent)
        {
            ReturnToOriginalPosition();
        }
    }

    private void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.rotation = Quaternion.identity;
    }

    // uiCamera: null for Screen Space - Overlay, use the camera for Screen Space - Camera
    private bool IsRectTransformFullyInside(RectTransform child, RectTransform parent, Camera uiCamera)
    {
        Vector3[] childCorners = new Vector3[4];
        child.GetWorldCorners(childCorners);

        foreach (Vector3 corner in childCorners)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(parent, corner, uiCamera))
            {
                return false;
            }
        }
        return true;
    }
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (!image || !image.sprite || !image.sprite.texture)
            return true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 localPos);
        Rect rect = rectTransform.rect;

        float normalizedX = (localPos.x - rect.x) / rect.width;
        float normalizedY = (localPos.y - rect.y) / rect.height;

        if (normalizedX < 0 || normalizedX > 1 || normalizedY < 0 || normalizedY > 1)
            return false;

        Sprite sprite = image.sprite;
        Texture2D texture = sprite.texture;
        Rect spriteRect = sprite.rect;

        int texX = Mathf.FloorToInt(spriteRect.x + spriteRect.width * normalizedX);
        int texY = Mathf.FloorToInt(spriteRect.y + spriteRect.height * normalizedY);

        if (texX < 0 || texX >= texture.width || texY < 0 || texY >= texture.height)
            return false;

        Color pixel = texture.GetPixel(texX, texY);
        return pixel.a > 0.1f;
    }
}
