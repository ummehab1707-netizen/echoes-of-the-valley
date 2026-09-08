using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using EchoesOfTheValley.Backbone;

public class PuzzleController : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    public RectTransform targetSlot;
    public float snapThreshold = 50f;
    public float proximityThreshold = 100f;
    private bool isCompleted = false;
    private bool hasPlayedProximitySound = false;
    private float startTime;

    [Header("Audio & Visual Rewards")]
    public AudioSource successAudio;
    public AudioSource proximityAudio;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
        startTime = Time.realtimeSinceStartup;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isCompleted) return;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        hasPlayedProximitySound = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isCompleted || rectTransform == null) return;
        
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null) return;

        Camera currentCamera = null;
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            currentCamera = eventData.pressEventCamera ?? Camera.main;
        }

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, 
            eventData.position, 
            currentCamera, 
            out localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }

        if (targetSlot != null && !isCompleted)
        {
            float distanceToTarget = Vector2.Distance(rectTransform.anchoredPosition, targetSlot.anchoredPosition);
            
            if (distanceToTarget <= proximityThreshold)
            {
                if (!hasPlayedProximitySound && proximityAudio != null && !proximityAudio.isPlaying)
                {
                    proximityAudio.Play();
                    hasPlayedProximitySound = true;
                }
            }
            else
            {
                hasPlayedProximitySound = false;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (targetSlot == null) return;

        float distance = Vector2.Distance(rectTransform.anchoredPosition, targetSlot.anchoredPosition);
        
        if (distance < snapThreshold)
        {
            rectTransform.anchoredPosition = targetSlot.anchoredPosition;
            if (!isCompleted)
            {
                isCompleted = true;
                float elapsedTimeMs = (Time.realtimeSinceStartup - startTime) * 1000f;
                Debug.Log($"Puzzle Completed! Response Time: {elapsedTimeMs:F2} ms");
                
                // --- HOOKED TO PERSON 4 BACKBONE HERE ---
                if (TelemetryLogger.Instance != null)
                {
                    TelemetryLogger.Instance.RecordGameSession("PATIENT_01", "AssameseGamosaMotif", (int)elapsedTimeMs, 0);
                }
                
                StartCoroutine(PlaySuccessPopAnimation());
                if (successAudio != null)
                {
                    successAudio.Play();
                }
            }
        }
    }

    IEnumerator PlaySuccessPopAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;

        elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }
}