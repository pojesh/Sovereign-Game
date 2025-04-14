using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CEOGameOverPanel : MonoBehaviour
{
    public TextMeshProUGUI gameOverMessageText;
    public TextMeshProUGUI relatedPrincipleTitleText;
    public TextMeshProUGUI relatedPrincipleDescriptionText;
    public Button restartButton;
    
    private CEOGameController gameController;
    
    void Awake()
    {
        gameController = FindObjectOfType<CEOGameController>();
        restartButton.onClick.AddListener(OnRestartClicked);
        gameObject.SetActive(false);
    }
    
    public void ShowGameOver(string message, string principleName, string principleDescription)
    {
        gameOverMessageText.text = message;
        relatedPrincipleTitleText.text = principleName;
        relatedPrincipleDescriptionText.text = principleDescription;
        gameObject.SetActive(true);
    }
    
    void OnRestartClicked()
    {
        gameObject.SetActive(false);
        gameController.ResetGame();
    }
}