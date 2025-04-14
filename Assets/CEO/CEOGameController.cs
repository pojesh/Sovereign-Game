using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

[System.Serializable]
public class CEOMetricChanges
{
    public int PROFIT;
    public int EMPLOYEESATISFACTION;
    public int MARKETSHARE;
    public int BOARDAPPROVAL;
}

[System.Serializable]
public class CEOActionMetricChanges
{
    public CEOMetricChanges left;
    public CEOMetricChanges right;
}

[System.Serializable]
public class CEOSubscenario
{
    public int id;
    public string statement;
    public string leftAction;
    public string rightAction;
    public CEOActionMetricChanges metricChanges;
    public string relatedPrinciple;
}

[System.Serializable]
public class CEOScenario
{
    public string scenarioName;
    public string description;
    public List<CEOSubscenario> subscenarios;
}

[System.Serializable]
public class CEOPrinciple
{
    public string name;
    public string description;
    public string explanation;
}

[System.Serializable]
public class CEOPrinciplesData
{
    public List<CEOPrinciple> principles;
}

// Change the class name from GameController to CEOGameController
public class CEOGameController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI profitText;
    public TextMeshProUGUI employeeSatisfactionText;
    public TextMeshProUGUI marketShareText;
    public TextMeshProUGUI boardApprovalText;
    public TextMeshProUGUI subscenarioText;
    public TextMeshProUGUI leftActionText;
    public TextMeshProUGUI rightActionText;
    public Image cardImage;
    public GameObject principlePanel;
    public TextMeshProUGUI principleTitleText;
    public TextMeshProUGUI principleDescriptionText;
    public TextMeshProUGUI principleExplanationText;

    [Header("Game Settings")]
    public float cardDragThreshold = 100f;
    public float cardReturnSpeed = 10f;
    public List<string> scenarioNames;

    public CEOGameOverPanel gameOverPanel;
    
    // Game state
    private Dictionary<string, int> metrics = new Dictionary<string, int>()
    {
        { "PROFIT", 50 },
        { "EMPLOYEESATISFACTION", 50 },
        { "MARKETSHARE", 50 },
        { "BOARDAPPROVAL", 50 }
    };

    private List<CEOScenario> allScenarios = new List<CEOScenario>();
    private Dictionary<string, CEOPrinciple> principlesDictionary = new Dictionary<string, CEOPrinciple>();
    private CEOScenario currentScenario;
    private CEOSubscenario currentSubscenario;
    private int currentSubscenarioIndex = 0;
    private Vector3 cardOriginalPosition;
    private bool isDragging = false;
    private bool isAnimating = false;

    void Start()
    {
        LoadPrinciples();
        LoadScenarios();
        cardOriginalPosition = cardImage.transform.position;
        principlePanel.SetActive(false);
        
        if (allScenarios.Count > 0)
        {
            StartNewScenario(allScenarios[0]);
        }
        
        UpdateMetricsUI();
    }

    void LoadPrinciples()
    {
        TextAsset principlesJson = Resources.Load<TextAsset>("Data/Principles/Principles");
        if (principlesJson != null)
        {
            CEOPrinciplesData principlesData = JsonUtility.FromJson<CEOPrinciplesData>(principlesJson.text);
            foreach (CEOPrinciple principle in principlesData.principles)
            {
                principlesDictionary[principle.name] = principle;
            }
            Debug.Log($"Loaded {principlesDictionary.Count} principles");
        }
        else
        {
            Debug.LogError("Could not load Principles.json");
        }
    }

    void LoadScenarios()
    {
        foreach (string scenarioName in scenarioNames)
        {
            TextAsset scenarioJson = Resources.Load<TextAsset>($"Data/Scenarios/{scenarioName}");
            if (scenarioJson != null)
            {
                CEOScenario scenario = JsonUtility.FromJson<CEOScenario>(scenarioJson.text);
                allScenarios.Add(scenario);
                Debug.Log($"Loaded scenario: {scenario.scenarioName} with {scenario.subscenarios.Count} subscenarios");
            }
            else
            {
                Debug.LogError($"Could not load scenario: {scenarioName}");
            }
        }
    }

    void StartNewScenario(CEOScenario scenario)
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

    void ShowSubscenario(CEOSubscenario subscenario)
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
        profitText.text = $"PROFIT: {metrics["PROFIT"]}";
        employeeSatisfactionText.text = $"EMPLOYEE SATISFACTION: {metrics["EMPLOYEESATISFACTION"]}";
        marketShareText.text = $"MARKET SHARE: {metrics["MARKETSHARE"]}";
        boardApprovalText.text = $"BOARD APPROVAL: {metrics["BOARDAPPROVAL"]}";
    }

    void ShowPrinciple(CEOPrinciple principle)
    {
        principleTitleText.text = principle.name;
        principleDescriptionText.text = principle.description;
        principleExplanationText.text = principle.explanation;
        
        principlePanel.SetActive(true);
        StartCoroutine(HidePrinciplePanelAfterDelay(5f));
    }

    IEnumerator HidePrinciplePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        principlePanel.SetActive(false);
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
                string relatedPrinciple = "";
                
                if (metric.Key == "PROFIT")
                {
                    message = metric.Value <= 0 ? 
                        "Bankruptcy! The company has gone under." : 
                        "Incredible success! The company is now a global financial powerhouse.";
                    relatedPrinciple = metric.Value <= 0 ? "Cash Flow Principle" : "Profit Maximization";
                }
                else if (metric.Key == "EMPLOYEESATISFACTION")
                {
                    message = metric.Value <= 0 ? 
                        "Mass resignation! All your employees have quit." : 
                        "Perfect workplace! Your company is rated the best place to work.";
                    relatedPrinciple = metric.Value <= 0 ? "Employee Engagement" : "Company Culture";
                }
                else if (metric.Key == "MARKETSHARE")
                {
                    message = metric.Value <= 0 ? 
                        "Market failure! Your products have been completely rejected." : 
                        "Market domination! Your company has become a monopoly.";
                    relatedPrinciple = metric.Value <= 0 ? "Product-Market Fit" : "Blue Ocean Strategy";
                }
                else if (metric.Key == "BOARDAPPROVAL")
                {
                    message = metric.Value <= 0 ? 
                        "Fired! The board has unanimously voted to remove you as CEO." : 
                        "Legendary CEO! The board has given you unprecedented authority.";
                    relatedPrinciple = metric.Value <= 0 ? "Corporate Governance" : "Leadership Excellence";
                }
                
                if (principlesDictionary.ContainsKey(relatedPrinciple))
                {
                    CEOPrinciple principle = principlesDictionary[relatedPrinciple];
                    gameOverPanel.ShowGameOver(message, principle.name, principle.description + "\n\n" + principle.explanation);
                }
                else
                {
                    gameOverPanel.ShowGameOver(message, "", "");
                }
                
                return;
            }
        }
    }

    public void ResetGame()
    {
        // Reset metrics
        metrics["PROFIT"] = 50;
        metrics["EMPLOYEESATISFACTION"] = 50;
        metrics["MARKETSHARE"] = 50;
        metrics["BOARDAPPROVAL"] = 50;
        
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
        CEOMetricChanges changes = isLeft ? currentSubscenario.metricChanges.left : currentSubscenario.metricChanges.right;
        
        // Apply changes to metrics
        metrics["PROFIT"] += changes.PROFIT;
        metrics["EMPLOYEESATISFACTION"] += changes.EMPLOYEESATISFACTION;
        metrics["MARKETSHARE"] += changes.MARKETSHARE;
        metrics["BOARDAPPROVAL"] += changes.BOARDAPPROVAL;
        
        // Clamp values between 0 and 100
        // Create a temporary list of keys to avoid modifying the dictionary during enumeration
        List<string> keys = new List<string>(metrics.Keys);
        foreach (string key in keys)
        {
            metrics[key] = Mathf.Clamp(metrics[key], 0, 100);
        }
        
        UpdateMetricsUI();
        
        // Show related principle
        if (!string.IsNullOrEmpty(currentSubscenario.relatedPrinciple) && principlesDictionary.ContainsKey(currentSubscenario.relatedPrinciple))
        {
            ShowPrinciple(principlesDictionary[currentSubscenario.relatedPrinciple]);
        }
        
        // Check win/lose conditions
        CheckGameEndConditions();
        
        // Move to next subscenario
        NextSubscenario();
    }
}