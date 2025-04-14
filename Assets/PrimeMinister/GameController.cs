using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

[System.Serializable]
public class PMMetricChanges
{
    public int ECONOMY;
    public int PUBLICOPINION;
    public int MILITARY;
    public int OPPOSITIONPOWER;
}

[System.Serializable]
public class PMActionMetricChanges
{
    public PMMetricChanges left;
    public PMMetricChanges right;
}

[System.Serializable]
public class PMSubscenario
{
    public int id;
    public string statement;
    public string leftAction;
    public string rightAction;
    public PMActionMetricChanges metricChanges;
    public string relatedLaw;
}

[System.Serializable]
public class PMScenario
{
    public string scenarioName;
    public string description;
    public List<PMSubscenario> subscenarios;
}

[System.Serializable]
public class PMLaw
{
    public string name;
    public string description;
    public string explanation;
}

[System.Serializable]
public class PMLawsData
{
    public List<PMLaw> laws;
}

public class GameController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI economyText;
    public TextMeshProUGUI publicOpinionText;
    public TextMeshProUGUI militaryText;
    public TextMeshProUGUI oppositionPowerText;
    public TextMeshProUGUI subscenarioText;
    public TextMeshProUGUI leftActionText;
    public TextMeshProUGUI rightActionText;
    public Image cardImage;
    public GameObject lawPanel;
    public TextMeshProUGUI lawTitleText;
    public TextMeshProUGUI lawDescriptionText;
    public TextMeshProUGUI lawExplanationText;

    [Header("Game Settings")]
    public float cardDragThreshold = 100f;
    public float cardReturnSpeed = 10f;
    public List<string> scenarioNames;

    public GameOverPanel gameOverPanel;
    // Game state
    private Dictionary<string, int> metrics = new Dictionary<string, int>()
    {
        { "ECONOMY", 50 },
        { "PUBLICOPINION", 50 },
        { "MILITARY", 50 },
        { "OPPOSITIONPOWER", 50 }
    };

    private List<PMScenario> allScenarios = new List<PMScenario>();
    private Dictionary<string, PMLaw> lawsDictionary = new Dictionary<string, PMLaw>();
    private PMScenario currentScenario;
    private PMSubscenario currentSubscenario;
    private int currentSubscenarioIndex = 0;
    private Vector3 cardOriginalPosition;
    private bool isDragging = false;
    private bool isAnimating = false;

    void Start()
    {
        LoadLaws();
        LoadScenarios();
        cardOriginalPosition = cardImage.transform.position;
        lawPanel.SetActive(false);
        
        if (allScenarios.Count > 0)
        {
            StartNewScenario(allScenarios[0]);
        }
        
        UpdateMetricsUI();
    }

    void LoadLaws()
    {
        TextAsset lawsJson = Resources.Load<TextAsset>("Data/Laws/Laws");
        if (lawsJson != null)
        {
            PMLawsData lawsData = JsonUtility.FromJson<PMLawsData>(lawsJson.text);
            foreach (PMLaw law in lawsData.laws)
            {
                lawsDictionary[law.name] = law;
            }
            Debug.Log($"Loaded {lawsDictionary.Count} laws");
        }
        else
        {
            Debug.LogError("Could not load Laws.json");
        }
    }

    void LoadScenarios()
    {
        foreach (string scenarioName in scenarioNames)
        {
            TextAsset scenarioJson = Resources.Load<TextAsset>($"Data/Scenarios/{scenarioName}");
            if (scenarioJson != null)
            {
                PMScenario scenario = JsonUtility.FromJson<PMScenario>(scenarioJson.text);
                allScenarios.Add(scenario);
                Debug.Log($"Loaded scenario: {scenario.scenarioName} with {scenario.subscenarios.Count} subscenarios");
            }
            else
            {
                Debug.LogError($"Could not load scenario: {scenarioName}");
            }
        }
    }

    void StartNewScenario(PMScenario scenario)
    {
        currentScenario = scenario;
        currentSubscenarioIndex = 0;
        
        if (currentScenario.subscenarios.Count > 0)
        {
            ShowSubscenario(currentScenario.subscenarios[0]);
        }
        else
        {
            Debug.LogError("Scenario has no subscenarios");
        }
    }

    void ShowSubscenario(PMSubscenario subscenario)
    {
        currentSubscenario = subscenario;
        subscenarioText.text = subscenario.statement;
        leftActionText.text = subscenario.leftAction;
        rightActionText.text = subscenario.rightAction;
        
        // Hide action texts initially
        leftActionText.gameObject.SetActive(false);
        rightActionText.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleCardDragging();
    }

    void HandleCardDragging()
    {
        if (isAnimating)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == cardImage.gameObject)
            {
                isDragging = true;
            }
        }

        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = cardImage.transform.position.z;
            cardImage.transform.position = new Vector3(mousePos.x, cardImage.transform.position.y, cardImage.transform.position.z);
            
            // Show action text based on drag direction
            float dragDistance = cardImage.transform.position.x - cardOriginalPosition.x;
            leftActionText.gameObject.SetActive(dragDistance < -20);
            rightActionText.gameObject.SetActive(dragDistance > 20);
            
            // Check if card is dragged beyond threshold
            if (Mathf.Abs(dragDistance) > cardDragThreshold)
            {
                isDragging = false;
                isAnimating = true;
                
                bool isLeft = dragDistance < 0;
                ProcessDecision(isLeft);
                
                // Animate card off screen
                Vector3 targetPos = cardOriginalPosition;
                targetPos.x += isLeft ? -Screen.width : Screen.width;
                
                StartCoroutine(AnimateCard(targetPos, () => {
                    // Reset card position and show next subscenario
                    cardImage.transform.position = cardOriginalPosition;
                    NextSubscenario();
                    isAnimating = false;
                }));
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            StartCoroutine(AnimateCard(cardOriginalPosition, () => {
                leftActionText.gameObject.SetActive(false);
                rightActionText.gameObject.SetActive(false);
            }));
        }
    }

    IEnumerator AnimateCard(Vector3 targetPosition, Action onComplete)
    {
        while (Vector3.Distance(cardImage.transform.position, targetPosition) > 0.1f)
        {
            cardImage.transform.position = Vector3.Lerp(cardImage.transform.position, targetPosition, Time.deltaTime * cardReturnSpeed);
            yield return null;
        }
        
        cardImage.transform.position = targetPosition;
        onComplete?.Invoke();
    }


    void UpdateMetricsUI()
    {
        economyText.text = $"ECONOMY: {metrics["ECONOMY"]}";
        publicOpinionText.text = $"PUBLIC OPINION: {metrics["PUBLICOPINION"]}";
        militaryText.text = $"MILITARY: {metrics["MILITARY"]}";
        oppositionPowerText.text = $"OPPOSITION POWER: {metrics["OPPOSITIONPOWER"]}";
    }

    void ShowLaw(PMLaw law)
    {
        lawTitleText.text = law.name;
        lawDescriptionText.text = law.description;
        lawExplanationText.text = law.explanation;
        
        lawPanel.SetActive(true);
        StartCoroutine(HideLawPanelAfterDelay(5f));
    }

    IEnumerator HideLawPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lawPanel.SetActive(false);
    }

    void NextSubscenario()
    {
        currentSubscenarioIndex++;
        
        if (currentSubscenarioIndex < currentScenario.subscenarios.Count)
        {
            ShowSubscenario(currentScenario.subscenarios[currentSubscenarioIndex]);
        }
        else
        {
            // End of current scenario, pick a new one or end game
            if (allScenarios.Count > 1)
            {
                int randomIndex = UnityEngine.Random.Range(0, allScenarios.Count);
                while (allScenarios[randomIndex] == currentScenario)
                {
                    randomIndex = UnityEngine.Random.Range(0, allScenarios.Count);
                }
                StartNewScenario(allScenarios[randomIndex]);
            }
            else
            {
                // Restart the same scenario if there's only one
                currentSubscenarioIndex = 0;
                ShowSubscenario(currentScenario.subscenarios[0]);
            }
        }
    }

    void CheckGameEndConditions()
    {
        foreach (var metric in metrics)
        {
            if (metric.Value <= 0 || metric.Value >= 100)
            {
                string message = "";
                string relatedLaw = "";
                
                if (metric.Key == "ECONOMY")
                {
                    message = metric.Value <= 0 ? 
                        "Economic collapse! The country has gone bankrupt." : 
                        "Economic miracle! The country is now a global economic powerhouse.";
                    relatedLaw = metric.Value <= 0 ? "Law of Diminishing Returns" : "Pareto Principle";
                }
                else if (metric.Key == "PUBLICOPINION")
                {
                    message = metric.Value <= 0 ? 
                        "Public revolt! The citizens have lost all faith in your leadership." : 
                        "Overwhelming popularity! You've become the most beloved leader in history.";
                    relatedLaw = metric.Value <= 0 ? "Goodhart's Law" : "Chekhov's Gun";
                }
                else if (metric.Key == "MILITARY")
                {
                    message = metric.Value <= 0 ? 
                        "Military collapse! The country is defenseless." : 
                        "Military dominance! Your country has become a global superpower.";
                    relatedLaw = metric.Value <= 0 ? "Peter Principle" : "Matthew Principle";
                }
                else if (metric.Key == "OPPOSITIONPOWER")
                {
                    message = metric.Value <= 0 ? 
                        "Opposition crushed! You've consolidated complete political control." : 
                        "Opposition triumph! You've been voted out of office.";
                    relatedLaw = metric.Value <= 0 ? "Parkinson's Law" : "Murphy's Law";
                }
                
                // In CheckGameEndConditions method, change Law to PMLaw
                if (lawsDictionary.ContainsKey(relatedLaw))
                {
                    PMLaw law = lawsDictionary[relatedLaw];
                    gameOverPanel.ShowGameOver(message, law.name, law.description + "\n\n" + law.explanation);
                }
                else
                {
                    gameOverPanel.ShowGameOver(message, "", "");
                }
                
                return;
            }
        }
    }

    // Move ResetGame method outside of CheckGameEndConditions and make it public
    public void ResetGame()
    {
        // Reset metrics
        metrics["ECONOMY"] = 50;
        metrics["PUBLICOPINION"] = 50;
        metrics["MILITARY"] = 50;
        metrics["OPPOSITIONPOWER"] = 50;
        
        UpdateMetricsUI();
        
        // Start a new scenario
        if (allScenarios.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, allScenarios.Count);
            StartNewScenario(allScenarios[randomIndex]);
        }
    }

    public void UpdateDragPosition(float dragDistance)
    {
        // Show action text based on drag direction
        leftActionText.gameObject.SetActive(dragDistance < -20);
        rightActionText.gameObject.SetActive(dragDistance > 20);
    }

    public void ResetActionTexts()
    {
        leftActionText.gameObject.SetActive(false);
        rightActionText.gameObject.SetActive(false);
    }

    public void ProcessDecision(bool isLeft)
    {
        PMMetricChanges changes = isLeft ? currentSubscenario.metricChanges.left : currentSubscenario.metricChanges.right;
        
        // Apply changes to metrics
        metrics["ECONOMY"] += changes.ECONOMY;
        metrics["PUBLICOPINION"] += changes.PUBLICOPINION;
        metrics["MILITARY"] += changes.MILITARY;
        metrics["OPPOSITIONPOWER"] += changes.OPPOSITIONPOWER;
        
        // Clamp values between 0 and 100
        // Create a temporary list of keys to avoid modifying the dictionary during enumeration
        List<string> keys = new List<string>(metrics.Keys);
        foreach (string key in keys)
        {
            metrics[key] = Mathf.Clamp(metrics[key], 0, 100);
        }
        
        UpdateMetricsUI();
        
        // Show related law
        if (!string.IsNullOrEmpty(currentSubscenario.relatedLaw) && lawsDictionary.ContainsKey(currentSubscenario.relatedLaw))
        {
            ShowLaw(lawsDictionary[currentSubscenario.relatedLaw]);
        }
        
        // Check win/lose conditions
        CheckGameEndConditions();
        
        // Move to next subscenario
        NextSubscenario();
    }
}