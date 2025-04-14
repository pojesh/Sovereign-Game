using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DIPGameOverPanel : MonoBehaviour
{
    public TextMeshProUGUI gameOverMessageText;
    public TextMeshProUGUI relatedDoctrineTitleText;
    public TextMeshProUGUI relatedDoctrineDescriptionText;
    public Button restartButton;
    
    private DIPGameController gameController;
    
    void Awake()
    {
        gameController = FindObjectOfType<DIPGameController>();
        restartButton.onClick.AddListener(OnRestartClicked);
        gameObject.SetActive(false);
    }
    
    public void ShowGameOver(string message, string doctrineName, string doctrineDescription)
    {
        gameOverMessageText.text = message;
        relatedDoctrineTitleText.text = doctrineName;
        relatedDoctrineDescriptionText.text = doctrineDescription;
        gameObject.SetActive(true);
    }
    
    void OnRestartClicked()
    {
        gameObject.SetActive(false);
        gameController.ResetGame();
    }
}