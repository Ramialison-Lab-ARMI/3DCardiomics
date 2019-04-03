using UnityEngine;
using UnityEngine.UI;

 // Needed to access onscreen elements
using UnityEngine.Networking; 
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Runtime.InteropServices;

/*****
 * 
 * Controls the differential expression UI and handling of DE requests to the server
 * 
 *****/

public class Compare : MonoBehaviour
{

    public struct NamedDoubleArray // To store geneIdName
    {
        public NamedDoubleArray(string name, double[] values)
        {
            Name = name;
            Values = values;
        }

        public string Name { get; private set; }

        public double[] Values { get; private set; }
    }

    public GameObject col1;
    // Our template column1 cell (The text)
    public GameObject col2;
    // Our template column2 cell (The button)
    //public GameObject LH;
    public GameObject SH;
    // Access to the script holder (to use 'Colour' class methods)
    public Scrollbar SB;
    public Button BC;
    public GameObject img;
    public GameObject btnWarning;
    public GameObject dlButton;
    public GameObject sortButton;
    public GameObject panelBig;
    public GameObject btnComplement;
    public Text cText;
    public Text sText;

    public static int first;
    public static List<string> pieces1 = new List<string>();
    public static List<string> pieces2 = new List<string>();
    private static string csvUrl;

    private List<NamedDoubleArray> values = new List<NamedDoubleArray>();
    private string pieceString;
    private string contrasts;
    private int upto;
    private System.Collections.IEnumerator cr;


    // For the button ** COMMENT FOR IPHONE **
    [DllImport("__Internal")]
    private static extern void JsOpenWindow(string url);

    public static string[] hp = new string[18] { "A_1", "A_2", "A_3", "A_4", "B_1", "B_2", "B_3", "B_4", "C_1", "C_2", "C_3", "C_4", "D_1", "D_2", "D_3", "E_1", "E_2", "E_3" };

    // Use this for initialization
    void Start()
    {
        first = 0;
    }

    // Links the CSV button to the browser
    public void OpenLinkJSPlugin()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        JsOpenWindow(csvUrl);
        #endif
    }

    // Processes the clicking for contrasts
    public void click()
    {
		
        if (first == 2)
        {

            if (pieces2.Count > 0)
            {
                // Hide complement button
                btnComplement.SetActive(false);

                panelBig.SetActive(true);

                btnWarning.SetActive(false);
                cText.text = "Compare";
                first = 0;

                // Delete existing pieces
                values.Clear();
                destroy();
                SH.GetComponent<Colour>().resetColour();
                getPieces();

                BC.interactable = false;
                img.SetActive(true);

                cr = requestContrasts();
                StartCoroutine(cr);

            }
            else
            {
                btnWarning.SetActive(true);
                Debug.Log("Please select some pieces for Group 2");
            }

        }
        else
        {
            if (first == 0)
            {
                dlButton.SetActive(false);
                sortButton.SetActive(false);

                cText.text = "Select Group 1 Piece(s)";
                first = 1;
            }
            else
            {
                if (first == 1)
                {

                    if (pieces1.Count > 0)
                    {
                        btnWarning.SetActive(false);
                        cText.text = "Select Group 2 Piece(s)";

                        // Show complement button
                        btnComplement.SetActive(true);

                        first = 2;
                    }
                    else
                    {
                        btnWarning.SetActive(true);
                        Debug.Log("Please select some pieces for Group 1");
                    }
                }
            }
        }
    }

    // Creates the URL based on the selected pieces
    public void getPieces()
    {

        pieceString = "3_3_3_3_3_3_3_3_3_3_3_3_3_3_3_3_3_3"; // 0:0	1:2		2:4		3:6	

        for (int i = 0; i < pieces1.Count; i++)
        {
			
            int tmp = Array.IndexOf(hp, pieces1[i]);
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder(pieceString);
            strBuilder[tmp * 2] = '2'; // (*2) to account for the underscores
            pieceString = strBuilder.ToString();

        }
			
        for (int i = 0; i < pieces2.Count; i++)
        {
			
            int tmp = Array.IndexOf(hp, pieces2[i]);
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder(pieceString);
            strBuilder[tmp * 2] = '1';
            pieceString = strBuilder.ToString();

        }
    }

    // Sets up a web thread to download and process the contrasts
    System.Collections.IEnumerator requestContrasts()
    {

        string url = "http://3d-cardiomics.erc.monash.edu/contrast/serve.php?m=" + pieceString;

        Debug.Log(pieceString);

        WWW result = new WWW(url);

        System.Collections.IEnumerator sp = spin();
        StartCoroutine(sp);

        yield return result;

        contrasts = result.text;

        int count = 0;

        while (contrasts.Length < 20 && count < 20)
        {

            yield return new WaitForSeconds(2.0f);

            result = new WWW(url);
            yield return result;

            contrasts = result.text;
            count += 1;
        }

        // Now let's process the contrasts data
#if UNITY_EDITOR
        Debug.Log(contrasts.Length);
#endif

        if (contrasts.Length != 33)
        {

            string[] wArray = contrasts.Split("\n"[0]);

            double current;
            string line;

            for (int counter = 0; counter < wArray.Length; counter++)
            {

                line = wArray[counter];
                char delim = ',';
                string[] cols;
                cols = line.Split(delim);

                if (cols[0] != "")
                {

                    double[] _expression = new double[5];

                    // Populate the array with values

                    for (int j = 1; j < cols.Length; j++)
                    {
                        current = double.Parse(cols[j], CultureInfo.InvariantCulture.NumberFormat);
                        _expression[j - 1] = current; // Create a temporary array with expression values
                    }

                    values.Add(new NamedDoubleArray(cols[0].ToLower(), _expression));

                }

            }
            // Add the gene expression values to values array
            values.Sort((s1, s2) => Math.Abs(s1.Values[4]).CompareTo(Math.Abs(s2.Values[4])));

            Debug.Log(values.Count);


            csvUrl = "http://3d-cardiomics.erc.monash.edu/contrast/serve.php?m=" + pieceString + "&head=1";

            dlButton.SetActive(true);
            sortButton.SetActive(true);

            generateTable();

        }
        else
        {

            // Perform the no results function
            Debug.Log("No Results");
            cText.text = "No Results";
        }

        BC.interactable = true;

        img.SetActive(false);

        StopCoroutine(sp);

    }

    bool sortByFDR = true;

    public void sortBy()
    {

        if (sortByFDR)
        {

            sortByFDR = false;
            sText.text = "FC";

            values.Sort((s1, s2) => Math.Abs(s2.Values[0]).CompareTo(Math.Abs(s1.Values[0])));

        }
        else
        {

            sortByFDR = true;
            sText.text = "FDR";

            values.Sort((s1, s2) => Math.Abs(s1.Values[4]).CompareTo(Math.Abs(s2.Values[4])));
        }

        generateTable();

    }

    // Makes the spinning wheel spin
    System.Collections.IEnumerator spin()
    {
        float duration = 30f;
        float elapsed = 0f;
        float spinSpeed = 180f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            img.transform.Rotate(Vector3.back, spinSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    // Removes genes from memory when we no longer need them
    public void destroy()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("cloneC");
        foreach (GameObject target in gameObjects)
        {
            GameObject.Destroy(target);
        }
        GameObject[] gameObjects1 = GameObject.FindGameObjectsWithTag("moreC");
        foreach (GameObject target in gameObjects1)
        {
            GameObject.Destroy(target);
        }

    }

    // Creates the table based on the downloaded contrasts
    public void generateTable()
    {

        upto = 0;

        // Destroy all previous cells before making table again
        //LH.SetActive (true);

        // Scroll to top
        SB.value = 1;

        destroy();
        // Start making rows of table
        add20(false);

    }

    // What happens when you click on a gene name
    private void colBtnActions(string gName)
    { // Colours and generates table from geneId 

        SH.GetComponent<Colour>().ColourFromText(gName, true);
        //generateTable (gId); // If we want to generate a new table every time we preview a gene
    }

    // What happens when you click on the '20 more' button
    private void moreBtnActions()
    { 

        add20(true);

    }

    public static string SentenceCase(string input)
    {
        if (input.Length < 1)
            return input;
        string sentence = input.ToLower();
        return "<i>" + sentence[0].ToString().ToUpper() + sentence.Substring(1) + "</i>";
    }

    // Adds 20 more genes to the distance table
    private void add20(bool more = false)
    {

        int rows;
        if (values.Count < 20)
        {
            rows = values.Count;
        }
        else
        {
            rows = 20;
        }
			
        if (more)
        { // Destroy the more button
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("moreC");
            foreach (GameObject target in gameObjects)
            {
                GameObject.Destroy(target);
            }
        }

        // Add more buttons
        for (int i = upto; i < upto + rows; i++)
        {

            // Initiate grid
            GridLayoutGroup grid = this.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(120, 20);

            string geneName = values[i].Name;
            double _fc = values[i].Values[0]; // fold change
            double _fdr = values[i].Values[4]; // false discovery rate (FDR)

            // Initiate column 3 (button)
            GameObject newCell1 = Instantiate(col2) as GameObject;
            newCell1.tag = "cloneC";
            Button btn = newCell1.GetComponent<Button>();
            btn.name = geneName;
            btn.onClick.AddListener(delegate
                {
                    colBtnActions(geneName);
                });
            btn.GetComponentInChildren<Text>().text = SentenceCase(geneName);
            newCell1.transform.SetParent(this.gameObject.transform, false); // Load column 2 in to table

            // Initiate column 2 (similarity level (float))
            GameObject newCell2 = Instantiate(col1) as GameObject;
            newCell2.tag = "cloneC";
            Text txt2 = newCell2.GetComponent<Text>();
            if (Math.Abs(_fc) >= 10)
            {
                txt2.text = _fc.ToString("+0.000;-0.000"); //.ToString ("+0.00;−0.00");
            }
            else
            {
                txt2.text = _fc.ToString("+0.0000;-0.0000"); //.ToString ("+0.00;−0.00");
            }
            txt2.text += " " + _fdr.ToString("0.00E00");

            newCell2.transform.SetParent(this.gameObject.transform, false); // Load column 1 in to table

        }

        if (upto < 180 && values.Count - upto > 20)
        {

            GameObject moreCell = Instantiate(col2) as GameObject;
            moreCell.tag = "moreC";
            Button moreBtn = moreCell.GetComponent<Button>();
            moreBtn.name = @"More";
            moreBtn.image.color = new Color(234 / 255F, 255 / 255F, 239 / 255F);
            moreBtn.onClick.AddListener(delegate
                {
                    moreBtnActions();
                });
            moreBtn.GetComponentInChildren<Text>().text = @"More..";
            moreBtn.transform.SetParent(this.gameObject.transform, false); // Load column 2 in to table

            upto = upto + 20;

        }


    }

    // Resets the comparison panel to the start
    public void reset()
    {
        // Refresh to starting state
        btnWarning.SetActive(false);
        cText.text = "Compare";
        first = 0;
        values.Clear();

        // Hide complement button
        btnComplement.SetActive(false);

        if (img.activeSelf)
        {
            StopCoroutine(cr);
            img.SetActive(false);
        }
			
        BC.interactable = true;
        SB.value = 1;
        dlButton.SetActive(false);
        sortButton.SetActive(false);
        // Remove all buttons

        destroy();
        panelBig.SetActive(false);

        sortByFDR = true;
        sText.text = "FDR";
    }

}
