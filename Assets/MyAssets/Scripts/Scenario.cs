[System.Serializable]
public class Scenario {
    public string text;
    // Impacts on metrics when the decision is positive.
    public int economyImpact;
    public int publicImpact;
    public int militaryImpact;
    public int oppositionImpact;

    public Scenario(string t, int eco, int pub, int mil, int opp) {
        text = t;
        economyImpact = eco;
        publicImpact = pub;
        militaryImpact = mil;
        oppositionImpact = opp;
    }
}
