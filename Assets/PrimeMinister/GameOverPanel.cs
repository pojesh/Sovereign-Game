using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverPanel : MonoBehaviour
{
    public TextMeshProUGUI gameOverMessageText;
    public TextMeshProUGUI relatedLawTitleText;
    public TextMeshProUGUI relatedLawDescriptionText;
    public Button restartButton;
    
    private GameController gameController;
    
    void Awake()
    {
        gameController = FindObjectOfType<GameController>();
        restartButton.onClick.AddListener(OnRestartClicked);
        gameObject.SetActive(false);
    }
    
    public void ShowGameOver(string message, string lawName, string lawDescription)
    {
        gameOverMessageText.text = message;
        relatedLawTitleText.text = lawName;
        relatedLawDescriptionText.text = lawDescription;
        gameObject.SetActive(true);
    }
    
    void OnRestartClicked()
    {
        gameObject.SetActive(false);
        gameController.ResetGame();
    }
}