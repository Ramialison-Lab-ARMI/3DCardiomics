using UnityEngine;
using UnityEngine.UI;

// Needed to access onscreen elements
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Linq;

/*****
 * 
 * Arranges for the colouring of the heart due to the gene selected, also controls colour blind modes and computing Pearson correlations
 * 
 *****/

public class Colour : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void JsAlert(string msg);

    public struct NamedFloatArray // To store geneIdName with expression values for each part
    {
        public NamedFloatArray(string name, float[] values)
        {
            Name = name;
            Values = values;
        }

        public string Name { get; private set; }

        public float[] Values { get; private set; }
    }

    public struct IntFloatPair // To store the comparison values
    {
        public IntFloatPair(int intValue, float floatValue)
        {
            Index = intValue;
            Value = floatValue;
        }

        public int Index { get; private set; }

        public float Value { get; private set; }
    }

    public static List<NamedFloatArray> values = new List<NamedFloatArray>();
    // This holds the name and all the 18 values of the gene
    public static List<IntFloatPair> valuesComp = new List<IntFloatPair>();
    // The comparison values to the current gene

    // Provides easy access to all pieces of the heart to iterate over
    public static string[] hp = new string[18] {
        "A_1", "A_2", "A_3", "A_4",
        "B_1", "B_2", "B_3", "B_4",
        "C_1", "C_2", "C_3", "C_4",
        "D_1", "D_2", "D_3",
        "E_1", "E_2", "E_3" };
    public static int valuesCount = 0;

    private float maxValue;
    private static string currentGene = "";
    public GeneSet CurrentGeneSet = null;
    //private static string baseGene = "";
    private static bool cb = false;
    private bool norm = true;

    public HashSet<string> allGeneNames = null;
    public Dictionary<string, int> expressionDataNameIndexMapping = null;

    private bool _allGenes_ready = false;

    public InputField IF;
    // Allows you to connect to the inputField to modify it
    public GameObject content;
    // Allows generating table and hiding/showing it
    public Button eyeButton;
    public Button colourButton;
    public Sprite eyeCross;

    public Button normButton;
    public Sprite normEnabledSprite;
    public Sprite normDisabledSprite;

    public Sprite colourBlind;

    public GameObject loadingSpinner;

    public Text gText;
    // Allows you access to the geneBtn text
    public Text mText;
    // Mode text


    // Run on initial load
    void Start()
    {
        // cache the array of all mouse gene names
        // allGeneNames = ValidGeneNames.names; // GetValidGeneNames();
        var _initValidGeneNamesAsync = StartCoroutine(InitValidGeneNames());
        // Debug.Log(allGeneNames.Count);

        //LoadDataset();
        LoadDatasetFORWEBPLAYER();
        resetColour();

        // Must be called AFTER LoadDatasetFORWEBPLAYER / LoadDataset
        InitExpressionDataNameIndexMapping();

        // For testing gene set display in Unity Editor
#if UNITY_EDITOR
        var fileUpload = GameObject.Find("ScriptHolder").GetComponent<FileUpload>();
        fileUpload.ShowPresetGeneSet("cluster_1");
#endif
    }


    // Reset everything to how it was at the start
    public void Reset()
    {
        cb = false;
        eyeButton.image.overrideSprite = null;
        colourButton.image.overrideSprite = null;
        IF.text = "";
        resetColour();
        gText.text = "Current gene: None";
        currentGene = "";
        CurrentGeneSet = null;
        //baseGene = "";

        if (norm)
        {
            toggleNormalised();
        }

        SetGeneSetLabels("", "");
        SetMaxValue();
    }

    public void ResetAll()
    {
        Reset();
        var scripts = GameObject.Find("ScriptHolder");
        var compare = GameObject.Find("Compare").GetComponentInChildren<Compare>();
        var dpanel = GameObject.Find("Dropdown/dList/dContent");

        scripts.GetComponent<Explode>().Reset();

        GameObject.Find("MainCamera").GetComponent<CameraRotation>().Reset();
        GameObject.Find("MainCamera").GetComponent<Selection>().Reset();
        if (compare) compare.reset();
        if (dpanel) dpanel.GetComponentInChildren<dPanel>().reset();
    }

    // Parse the expression value database in to memory as an array of strings and floats
    public void LoadDatasetFORWEBPLAYER()
    {
#if USE_REAL_DATA
        var csvFilenameBase = "fernP2_real";
#else
        var csvFilenameBase = "fake_mouse_expression_data";
#endif
        TextAsset textAsset = Resources.Load(csvFilenameBase) as TextAsset; //string input =  result.text;
        string[] wArray = textAsset.text.Split("\n"[0]);
        Resources.UnloadAsset(textAsset);

        string line;
        int i = wArray.Length;

        maxValue = 0;
        // norm = false;

        float current = 0;

        while (valuesCount < i)
        {
            line = wArray[valuesCount];
            // char delim = '\t';
            char delim = ',';
            string[] cols;
            cols = line.Split(delim);
            float[] floats = new float[18];

            // Populate the array with values
            for (int j = 1; j < cols.Length; j++)
            {
                current = float.Parse(cols[j], CultureInfo.InvariantCulture.NumberFormat);
                floats[j - 1] = current; // Create a temporary array of floats and add expression values

                if (current > maxValue)
                {
                    maxValue = current;
                }
            }
            values.Add(new NamedFloatArray(cols[0].ToLower(), floats)); // Add the gene expression values to values array of floats

            valuesCount++;

        }

        // Sort list (List already sorted in CSV) but: (Excel and C# have different sorting methods, e.g. for '_')
        values.Sort((s1, s2) => string.Compare(s1.Name, s2.Name, System.StringComparison.OrdinalIgnoreCase));
    }

    public void SetMaxValue()
    {
        maxValue = 0;
        for (int i = 0; i < 18; i++)
        {
            maxValue = Mathf.Max(maxValue, values[i].Values.Max());
        }
    }

    public int FindIndexOfGene(string geneName)
    {
        // Search the array for the gene name and return its index (location), or -1 if not found.
        for (int i = 0; i < valuesCount; i++)
        {
            if (geneName.ToLower() == values[i].Name.ToLower()) return i;
        }
        return -1;
    }

    public void InitExpressionDataNameIndexMapping() {
        expressionDataNameIndexMapping = new Dictionary<string, int>(valuesCount);
        for (int i = 0; i < valuesCount; i++)
        {
            expressionDataNameIndexMapping.Add(values[i].Name.ToLower(), i);
        }
    }

    public static float[] NormalizeFloatArray(IEnumerable<float> data, float min = 0.0f, float max = 1.0f)
    {
        float _max = data.Max();
        float _min = data.Min();
        float range = _max - _min;

        return data
            .Select(d => (d - _min) / range)
            .Select(n => (float)((1 - n) * min + n * max))
            .ToArray();
    }

    public IEnumerator InitValidGeneNames(string csvFilenameBase = "valid_gene_names")
    {
        int yield_every = 1000;

        TextAsset textAsset = Resources.Load(csvFilenameBase) as TextAsset;
        // Debug.Log("textAsset.name" + textAsset.name);
        string[] lines = textAsset.text.Split(
            new[] { "\r\n", "\r", "\n", "\\r\\n", "\\r", "\\n" },
            System.StringSplitOptions.RemoveEmptyEntries);

        allGeneNames = new HashSet<string>();
        int count = 0;
        foreach (var line in lines)
        {
            if (line[0] == "#"[0] || string.IsNullOrEmpty(line.Trim()))
            {
                continue;
            }
            allGeneNames.Add(line.Trim().ToLower());
            count++;
            if ((count % yield_every) == 0) {
                // yield return null;
                // Debug.Log(allGeneNames.Count);
                yield return new WaitForSeconds(.05f);
            }
        }

        // return allGeneNames;
        _allGenes_ready = true;
        // Debug.Log(allGeneNames.Count);
    }

    public HashSet<string> GetMGIGeneNames(string csvFilenameBase = "GENE-DESCRIPTION-TSV_MGI_9.tsv")
    {
        // Parse gene names from:
        //
        //  http://download.alliancegenome.org/3.1.1/GENE-DESCRIPTION-TSV/MGI/GENE-DESCRIPTION-TSV_MGI_9.tsv
        //
        //  as `Resources/GENE-DESCRIPTION-TSV_MGI_9.tsv.txt`

        TextAsset textAsset = Resources.Load(csvFilenameBase) as TextAsset;
        Debug.Log("textAsset.name" + textAsset.name);
        string[] lines = textAsset.text.Split(
            new[] { "\r\n", "\r", "\n", "\\r\\n", "\\r", "\\n" },
            System.StringSplitOptions.RemoveEmptyEntries);

        var geneNames = new HashSet<string>();
        foreach (var line in lines)
        {
            if (line[0] == "#"[0] || string.IsNullOrEmpty(line.Trim()))
            {
                continue;
            }
            string[] fields = line.Split(new[] { "\t"[0] });
            geneNames.Add(fields[1].Trim().ToLower());
        }

        return geneNames;
    }

    // Find the expression values corresponding with the entered gene, then start the colouring and similarity calculating processes
    public void ColourFromText(string geneName, bool panel = false)
    {
        SetMaxValue();
        currentGene = geneName.Trim();
        CurrentGeneSet = null;
        int geneIndex = FindIndexOfGene(geneName);

        // If the gene name was found, load that dataset into the pieces
        if (geneIndex > -1)
        {

            if (panel)
            {
                computeDistancesP(geneIndex);
                //baseGene = currentGene;
            }

            // Update current gene info
            gText.text = "Current gene: " + SentenceCase(geneName);
            SetGeneSetLabels("", "");

            float lMax = -1;
            float lMin = 100;
            if (norm)
            { // Find the local min and max if in normalised mode
                for (int i = 0; i < 18; i++)
                {
                    if (values[geneIndex].Values[i] > lMax)
                    {
                        lMax = values[geneIndex].Values[i];
                    }
                    if (values[geneIndex].Values[i] < lMin)
                    {
                        lMin = values[geneIndex].Values[i];
                    }
                }
            }
            for (int i = 0; i < 18; i++)
            {
                colourHeartPiece(hp[i], values[geneIndex].Values[i], lMax, lMin);
            }

        }
        else
        {
            // Update current gene info
            gText.text = "Current gene: None";
            resetColour();
            //Debug.Log ("Gene with name " + geneName + " not found.");
            content.GetComponent<PanelScript>().sleep();
        }
    }

    public IEnumerator ColourByGeneSet(GeneSet geneset)
    {
        yield return new WaitForSeconds(5f);
        int yield_every = 10;
        float yield_time = 0.05f;

        loadingSpinner.SetActive(true);

        // allGeneNames gets populated on startup - we can't do anything until it's filled
        while (!_allGenes_ready) {
            yield return new WaitForSeconds(0.2f);
        }

        var _normalizePerGene = true;
        //Debug.Log(geneset.Genes.Count);

        // ResetAll();

        currentGene = "";
        CurrentGeneSet = geneset;

        // valid genes in the uploaded gene set which are absent in the expression data
        var missingGenes = new List<string>();

        var averageValues = new float[18];

        int count = 0;
        foreach (string geneName in geneset.Genes)
        {
            count++;
            var geneNameLower = geneName.ToLower();

            if (!expressionDataNameIndexMapping.ContainsKey(geneNameLower))
            {
                // Debug.Log("WARNING: Gene name " + geneName + " not found in expression dataset.");
                missingGenes.Add(geneName);
#if UNITY_WEBGL
                //JsAlert(geneName + " is not a valid gene name.");
#endif
                //return;
                if (count % yield_every == 0) {
                    yield return new WaitForSeconds(yield_time);
                }
                continue;
            }

            if (!allGeneNames.Contains(geneNameLower))
            {
                Debug.LogError("ERROR: Gene '" + geneName + "' is not a valid mouse gene name.");
                JsAlert(geneName + " is not a valid MGI mouse gene symbol (GRCm38, MGI vM9). " +
                        "Maybe you need to convert your gene identifiers at http://www.informatics.jax.org/batch ?");
                loadingSpinner.SetActive(false);
                //return;
                yield break;
            }

            var geneIndex = expressionDataNameIndexMapping[geneNameLower];
            // var geneIndex = FindIndexOfGene(geneName);
            var expressionForGene = values[geneIndex].Values.ToArray();
            if (_normalizePerGene)
            {
                expressionForGene = NormalizeFloatArray(expressionForGene);
            }

#if UNITY_EDITOR
            Debug.Log("geneName, geneIndex: " + geneName + ", " + geneIndex);
#endif

#if UNITY_EDITOR
            for (int i = 0; i < 18; i++)
            {
                Debug.Log("Gene: " + geneName + ", Piece " + i.ToString() + ", Value: " + expressionForGene[i].ToString());
            }
#endif
            // sum
            for (int i = 0; i < 18; i++)
            {
                averageValues[i] += expressionForGene[i];
            }

            if (count % yield_every == 0) {
                yield return new WaitForSeconds(yield_time);
            }
        }

        if (missingGenes.Count > 0)
        {
            var _warningMsg = "WARNING: Some genes in the geneset are valid mouse genes but are absent from the expression dataset: " + string.Join(", ", missingGenes);
            Debug.Log(_warningMsg);
            JsAlert(_warningMsg);
        }

        // divide by number of genes in the set to obtain the average
        for (int i = 0; i < 18; i++)
        {
            averageValues[i] /= geneset.Genes.Count;
        }
        yield return null;

#if UNITY_EDITOR
        for (int i = 0; i < 18; i++)
        {
            Debug.Log("Piece " + i.ToString() + " average: " + averageValues[i].ToString());
        }
        Debug.Log("Min: " + averageValues.Min().ToString());
        Debug.Log("Max: " + averageValues.Max().ToString());
        yield return null;
#endif

        // apply average colours to model
        maxValue = averageValues.Max();
        var minValue = averageValues.Min();
        // norm = true;
        for (int i = 0; i < 18; i++)
        {
            colourHeartPiece(hp[i], averageValues[i], maxValue, minValue);
            yield return null;
        }

        SetGeneSetLabels(geneset.Name, geneset.Description);

        loadingSpinner.SetActive(false);
    }

    public void SetGeneSetLabels(string name, string description)
    {
        GameObject.Find("GeneSetName").GetComponent<Text>().text = name;
        var descLabel = GameObject.Find("GeneSetDescription").GetComponent<Text>();
        descLabel.text = description;

        /*
        descLabel.alignment = TextAnchor.MiddleRight;
        if (description.Length > 120)
        {
            descLabel.alignment = TextAnchor.MiddleLeft;
        }
        */
    }

    public static string SentenceCase(string input)
    {
        if (input.Length < 1)
            return input;
        string sentence = input.ToLower();
        return "<i>" + sentence[0].ToString().ToUpper() + sentence.Substring(1) + "</i>";
    }


    // Calculate the similarity distances (Pearson) between our current gene and the other genes
    public void computeDistancesP(int found)
    {

        valuesComp.Clear();

        float[] valuesI = values[found].Values;

        // Compute mean of I row (CurrentV)
        float valuesImean = 0;
        for (int j = 0; j < 18; j++)
        {
            valuesImean = valuesImean + valuesI[j];
        }
        valuesImean = valuesImean * (1 / (float)18); // Correct

        for (int i = 0; i < valuesCount; i++)
        {
            // Pearsons Correlation Coefficient

            // CurrentV = I
            // NextV	= J

            float[] valuesJ = values[i].Values;

            // Compute mean of J row (NextV)
            float valuesJmean = 0;
            for (int j = 0; j < 18; j++)
            {
                valuesJmean = valuesJmean + valuesJ[j];
            }
            valuesJmean = valuesJmean * (1 / (float)18); //Correct

            // Compute Zone1
            float zone1 = 0;
            for (int j = 0; j < 18; j++)
            {
                zone1 = zone1 + ((valuesI[j] - valuesImean) * (valuesJ[j] - valuesJmean));
            }
            //zone1 = Mathf.Sqrt (zone1);

            // Compute Zone2
            float zone2 = 0;
            for (int j = 0; j < 18; j++)
            {
                zone2 = zone2 + Mathf.Pow((valuesI[j] - valuesImean), 2);
            }
            zone2 = Mathf.Sqrt(zone2);

            // Compute Zone3
            float zone3 = 0;
            for (int j = 0; j < 18; j++)
            {
                zone3 = zone3 + Mathf.Pow((valuesJ[j] - valuesJmean), 2);
            }
            zone3 = Mathf.Sqrt(zone3);

            float sum = zone1 / (zone2 * zone3); 				// This Gives R

            if (sum > 0)
            {									// This Gives R^2
                sum = Mathf.Pow(sum, 2);
            }
            else
            {
                sum = -1 * Mathf.Pow(sum, 2);
            }

            if (!float.IsNaN(sum))
            {
                valuesComp.Add(new IntFloatPair(i, sum));
            }
            else
            {
                valuesComp.Add(new IntFloatPair(i, 0));
            }

        }

        // Remove ABS if you don't want negative (inversee) correlations in your 'Similar Genes' panel
        valuesComp.Sort((s1, s2) => Mathf.Abs(s2.Value).CompareTo(Mathf.Abs(s1.Value))); // Sort so that those with most similarity are first
        content.GetComponent<PanelScript>().generateTable(found);
    }



    // Colours a given heart piece based on the expression value
    public void colourHeartPiece(string heartPiece, float exp, float lMax, float lMin)
    {

        // Declare variables
        float rgb = 255;
        float t;

        // Convert value to a number between 0 and 1
        if (norm)
        {
            t = (exp - lMin) / (lMax - lMin);
        }
        else
        {
            t = exp / (maxValue);
        }

        // Create a gradient
        Gradient g = new Gradient();
        GradientColorKey[] gck;

        // TODO: Idea - sample colors across the width (or height) of a given texture.
        //       This way any gradient can be provided and the Low-High legend will always match
        //       the colors in use.
        if (cb)
        { // For colour blindness you can use 2 easy to see colours

            gck = new GradientColorKey[2];

            gck[0].color = Color.blue;
            gck[0].time = 0.0F;
            gck[1].color = Color.yellow;
            gck[1].time = 1.0F;

        }
        else
        { // Otherwise we can use heatmap colours from blue to red

            gck = new GradientColorKey[5];

            gck[0].color = new Color(0 / rgb, 0 / rgb, 244 / rgb); // Blue
            gck[0].time = 0.0F;
            gck[1].color = new Color(24 / rgb, 226 / rgb, 240 / rgb); // Cyan
            gck[1].time = 0.25F;
            gck[2].color = new Color(255 / rgb, 255 / rgb, 0 / rgb); // Yellow
            gck[2].time = 0.50F;
            gck[3].color = new Color(255 / rgb, 170 / rgb, 0 / rgb); // Orange
            gck[3].time = 0.75F;
            gck[4].color = new Color(254 / rgb, 0 / rgb, 0 / rgb); // Red
            gck[4].time = 1.0F;

        }

        g.SetKeys(gck, new GradientAlphaKey[0]); // Make all colours visible

        // Associate a decimal with a colour and change the heart piece
        GameObject.Find(heartPiece).GetComponent<Renderer>().material.color = g.Evaluate(t);

    }



    // Resets the colour of all the heart pieces back to white
    public void resetColour()
    {

        // Resets all heart pieces to white
        for (int i = 0; i < 18; i++)
        {
            GameObject.Find(hp[i]).GetComponent<Renderer>().material.color = Color.white;
        }

    }

    // Toggles the colour blind mode
    public void toggleColourBlind()
    {

        cb = !cb;

        if (cb)
        { // If they are colourblind
            eyeButton.image.overrideSprite = eyeCross;
            colourButton.image.overrideSprite = colourBlind;
        }
        else
        { // If they are not colourblind
            eyeButton.image.overrideSprite = null;
            colourButton.image.overrideSprite = null;
        }

        // Swap colour modes if a gene is active
        if (currentGene != "")
        {
            ColourFromText(currentGene, false);
        }
        if (CurrentGeneSet != null)
        {
            StartCoroutine(ColourByGeneSet(CurrentGeneSet));
        }

    }

    // Stretches the values over all the colour range for more resolution
    public void toggleNormalised()
    {

        norm = !norm;

        if (!norm)
        {
            normButton.image.overrideSprite = normDisabledSprite;
        }
        else
        {
            normButton.image.overrideSprite = null;
        }


        // Swap normalise modes if a gene is active
        if (currentGene != "")
        {
            ColourFromText(currentGene, false);
        }
        if (CurrentGeneSet != null)
        {
            StartCoroutine(ColourByGeneSet(CurrentGeneSet));
        }

    }

}
