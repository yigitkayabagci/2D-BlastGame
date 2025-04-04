using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class PopupManager : MonoBehaviour
{
    [Header("Win Popup")]
    [SerializeField] private GameObject winPopup;   
    [SerializeField] private Button winCloseButton;    
    [SerializeField] private Image winStarImage;       
    [SerializeField] private Image winParticleImage;    

    [Header("Lose Popup")]
    [SerializeField] private GameObject losePopup;    
    [SerializeField] private Button loseRetryButton;    
    [SerializeField] private Button loseExitButton;    

    private void Start()
    {
        HideAllPopups();
    }

    private void HideAllPopups()
    {
        if (winPopup != null)
            winPopup.SetActive(false);
        if (losePopup != null)
            losePopup.SetActive(false);
    }

    public void ShowLevelWinPopup(Action onClose = null)
    {
        HideAllPopups();
        if (winPopup != null)
        {
            winPopup.SetActive(true);
            
            if (winStarImage != null)
            {
                winStarImage.transform.localScale = Vector3.zero;
                winStarImage.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
            }
            
            if (winParticleImage != null)
            {
                winParticleImage.transform.localScale = Vector3.one * 0.5f;
                Color initialColor = winParticleImage.color;
                winParticleImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, 1f);
                winParticleImage.transform.DOScale(Vector3.one * 2f, 1f).SetEase(Ease.OutQuad);
                winParticleImage.DOFade(0f, 1f);
            }
            
            winCloseButton.onClick.RemoveAllListeners();
            winCloseButton.onClick.AddListener(() =>
            {
                onClose?.Invoke();
                HideAllPopups();
            });
        }
        else
        {
            Debug.LogWarning("Win popup referansı atanmamış!");
        }
    }

    public void ShowLevelFailedPopup(Action onRetry = null, Action onExit = null)
    {
        HideAllPopups();
        if (losePopup != null)
        {
            losePopup.SetActive(true);
            
            loseRetryButton.onClick.RemoveAllListeners();
            loseRetryButton.onClick.AddListener(() =>
            {
                onRetry?.Invoke();
                HideAllPopups();
            });
            
            loseExitButton.onClick.RemoveAllListeners();
            loseExitButton.onClick.AddListener(() =>
            {
                onExit?.Invoke();
                HideAllPopups();
            });
        }
        else
        {
            Debug.LogWarning("Lose popup referansı atanmamış!");
        }
    }
}
