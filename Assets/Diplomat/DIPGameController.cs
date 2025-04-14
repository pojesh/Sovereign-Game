using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

[System.Serializable]
public class DIPMetricChanges
{
    public int FOREIGNRELATIONS;
    public int DOMESTICSUPPORT;
    public int NATIONALSECURITY;
    public int ECONOMICTIES;
}

[System.Serializable]
public class DIPActionMetricChanges
{
    public DIPMetricChanges left;
    public DIPMetricChanges right;
}

[System.Serializable]
public class DIPSubscenario
{
    public int id;
    public string statement;
    public string leftAction;
    public string rightAction;
    public DIPActionMetricChanges metricChanges;
    public string relatedDoctrine;
}

[System.Serializable]
public class DIPScenario
{
    public string scenarioName;
    public string description;
    public List<DIPSubscenario> subscenarios;
}

[System.Serializable]
public class DIPDoctrine
{
    public string name;
    public string description;
    public string explanation;
}

[System.Serializable]
public class DIPDoctrinesData
{
    public List<DIPDoctrine> doctrines;
}

public class DIPGameController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI foreignRelationsText;
    public TextMeshProUGUI domesticSupportText;
    public TextMeshProUGUI nationalSecurityText;
    public TextMeshProUGUI economicTiesText;
    public TextMeshProUGUI subscenarioText;
    public TextMeshProUGUI leftActionText;
    public TextMeshProUGUI rightActionText;
    public Image cardImage;
    public GameObject doctrinePanel;
    public TextMeshProUGUI doctrineTitleText;
    public TextMeshProUGUI doctrineDescriptionText;
    public TextMeshProUGUI doctrineExplanationText;

    [Header("Game Settings")]
    public float cardDragThreshold = 100f;
    public float cardReturnSpeed = 10f;
    public List<string> scenarioNames;

    public DIPGameOverPanel gameOverPanel;
    
    // Game state
    private Dictionary<string, int> metrics = new Dictionary<string, int>()
    {
        { "FOREIGNRELATIONS", 50 },
        { "DOMESTICSUPPORT", 50 },
        { "NATIONALSECURITY", 50 },
        { "ECONOMICTIES", 50 }
    };

    private List<DIPScenario> allScenarios = new List<DIPScenario>();
    private Dictionary<string, DIPDoctrine> doctrinesDictionary = new Dictionary<string, DIPDoctrine>();
    private DIPScenario currentScenario;
    private DIPSubscenario currentSubscenario;
    private int currentSubscenarioIndex = 0;
    private Vector3 cardOriginalPosition;
    private bool isDragging = false;
    private bool isAnimating = false;

    void Start()
    {
        LoadDoctrines();
        LoadScenarios();
        cardOriginalPosition = cardImage.transform.position;
        doctrinePanel.SetActive(false);
        
        if (allScenarios.Count > 0)
        {
            StartNewScenario(allScenarios[0]);
        }
        
        UpdateMetricsUI();
    }

    void LoadDoctrines()
    {
        TextAsset doctrinesJson = Resources.Load<TextAsset>("Data/Doctrines/Doctrines");
        if (doctrinesJson != null)
        {
            DIPDoctrinesData doctrinesData = JsonUtility.FromJson<DIPDoctrinesData>(doctrinesJson.text);
            foreach (DIPDoctrine doctrine in doctrinesData.doctrines)
            {
                doctrinesDictionary[doctrine.name] = doctrine;
            }
            Debug.Log($"Loaded {doctrinesDictionary.Count} doctrines");
        }
        else
        {
            Debug.LogError("Could not load Doctrines.json");
        }
    }

    void LoadScenarios()
    {
        foreach (string scenarioName in scenarioNames)
        {
            TextAsset scenarioJson = Resources.Load<TextAsset>($"Data/Scenarios/{scenarioName}");
            if (scenarioJson != null)
            {
                DIPScenario scenario = JsonUtility.FromJson<DIPScenario>(scenarioJson.text);
                allScenarios.Add(scenario);
                Debug.Log($"Loaded scenario: {scenario.scenarioName} with {scenario.subscenarios.Count} subscenarios");
            }
            else
            {
                Debug.LogError($"Could not load scenario: {scenarioName}");
            }
        }
    }

    void StartNewScenario(DIPScenario scenario)
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

    void ShowSubscenario(DIPSubscenario subscenario)
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
        foreignRelationsText.text = $"FOREIGN RELATIONS: {metrics["FOREIGNRELATIONS"]}";
        domesticSupportText.text = $"DOMESTIC SUPPORT: {metrics["DOMESTICSUPPORT"]}";
        nationalSecurityText.text = $"NATIONAL SECURITY: {metrics["NATIONALSECURITY"]}";
        economicTiesText.text = $"ECONOMIC TIES: {metrics["ECONOMICTIES"]}";
    }

    void ShowDoctrine(DIPDoctrine doctrine)
    {
        doctrineTitleText.text = doctrine.name;
        doctrineDescriptionText.text = doctrine.description;
        doctrineExplanationText.text = doctrine.explanation;
        
        doctrinePanel.SetActive(true);
        StartCoroutine(HideDoctrinePanelAfterDelay(5f));
    }

    IEnumerator HideDoctrinePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        doctrinePanel.SetActive(false);
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
                string relatedDoctrine = "";
                
                if (metric.Key == "FOREIGNRELATIONS")
                {
                    message = metric.Value <= 0 ? 
                        "Diplomatic isolation! Your country has been ostracized by the international community." : 
                        "Diplomatic triumph! Your country is now a respected global leader.";
                    relatedDoctrine = metric.Value <= 0 ? "Isolationism" : "Multilateralism";
                }
                else if (metric.Key == "DOMESTICSUPPORT")
                {
                    message = metric.Value <= 0 ? 
                        "Public outrage! Your diplomatic policies have been completely rejected at home." : 
                        "National unity! Your diplomatic approach has united the country.";
                    relatedDoctrine = metric.Value <= 0 ? "Public Diplomacy" : "National Interest";
                }
                else if (metric.Key == "NATIONALSECURITY")
                {
                    message = metric.Value <= 0 ? 
                        "Security crisis! Your diplomatic failures have endangered the nation." : 
                        "Peace achieved! Your diplomacy has secured unprecedented national security.";
                    relatedDoctrine = metric.Value <= 0 ? "Balance of Power" : "Collective Security";
                }
                else if (metric.Key == "ECONOMICTIES")
                {
                    message = metric.Value <= 0 ? 
                        "Economic sanctions! Your country faces severe trade restrictions." : 
                        "Economic integration! Your diplomacy has created unprecedented prosperity.";
                    relatedDoctrine = metric.Value <= 0 ? "Protectionism" : "Free Trade";
                }
                
                if (doctrinesDictionary.ContainsKey(relatedDoctrine))
                {
                    DIPDoctrine doctrine = doctrinesDictionary[relatedDoctrine];
                    gameOverPanel.ShowGameOver(message, doctrine.name, doctrine.description + "\n\n" + doctrine.explanation);
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
        metrics["FOREIGNRELATIONS"] = 50;
        metrics["DOMESTICSUPPORT"] = 50;
        metrics["NATIONALSECURITY"] = 50;
        metrics["ECONOMICTIES"] = 50;
        
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
        DIPMetricChanges changes = isLeft ? currentSubscenario.metricChanges.left : currentSubscenario.metricChanges.right;
        
        // Apply changes to metrics
        metrics["FOREIGNRELATIONS"] += changes.FOREIGNRELATIONS;
        metrics["DOMESTICSUPPORT"] += changes.DOMESTICSUPPORT;
        metrics["NATIONALSECURITY"] += changes.NATIONALSECURITY;
        metrics["ECONOMICTIES"] += changes.ECONOMICTIES;
        
        // Clamp values between 0 and 100
        // Create a temporary list of keys to avoid modifying the dictionary during enumeration
        List<string> keys = new List<string>(metrics.Keys);
        foreach (string key in keys)
        {
            metrics[key] = Mathf.Clamp(metrics[key], 0, 100);
        }
        
        UpdateMetricsUI();
        
        // Show related doctrine
        if (!string.IsNullOrEmpty(currentSubscenario.relatedDoctrine) && doctrinesDictionary.ContainsKey(currentSubscenario.relatedDoctrine))
        {
            ShowDoctrine(doctrinesDictionary[currentSubscenario.relatedDoctrine]);
        }
        
        // Check win/lose conditions
        CheckGameEndConditions();
        
        // Move to next subscenario
        NextSubscenario();
    }
}