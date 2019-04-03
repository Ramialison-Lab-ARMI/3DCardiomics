using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

/*****
 * 
 * Controls the population of genes and values in the right-hand pearson correlation panel
 * 
 *****/

public class PanelScript : MonoBehaviour
{

    public GameObject col1;
    // Our template column1 cell (The text)
    public GameObject col2;
    // Our template column2 cell (The button)
    public GameObject LH;
    public GameObject SH;
    // Access to the script holder (to use 'Colour' class methods)
    public Scrollbar SB;

    private int upto;

    void Start()
    {
        LH.SetActive(false);
    }

    // Fills the data grid of the 'ListHolder' table and makes it visible
    public void generateTable(int geneId)
    {

        upto = 1;

        // Destroy all previous cells before making table again
        LH.SetActive(true);

        // Scroll to top
        SB.value = 1;

        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("clone");
        foreach (GameObject target in gameObjects)
        {
            GameObject.Destroy(target);
        }
        GameObject[] gameObjects1 = GameObject.FindGameObjectsWithTag("more");
        foreach (GameObject target in gameObjects1)
        {
            GameObject.Destroy(target);
        }
			
        // Start making rows of table
        add20(false);

    }

    private void colBtnActions(string gName, int gId)
    { // Colours and generates table from geneId 

        SH.GetComponent<Colour>().ColourFromText(gName);

        //generateTable (gId); // If we want to generate a new table every time we preview a gene
    }

    private void moreBtnActions()
    { // Adds 20 more genes to the distance table

        add20(true);

    }

    public static string SentenceCase(string input)
    {
        if (input.Length < 1)
            return input;
        string sentence = input.ToLower();
        return "<i>" + sentence[0].ToString().ToUpper() + sentence.Substring(1) + "</i>";
    }

    private void add20(bool more = false)
    {

        int rows = 20;

        if (rows > Colour.valuesCount - upto)
        {
            rows = Colour.valuesCount - upto;
        }
			
        if (more)
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("more");
            foreach (GameObject target in gameObjects)
            {
                GameObject.Destroy(target);
            }
        }

        for (int i = upto; i < upto + rows; i++)
        {

            // Initiate grid
            GridLayoutGroup grid = this.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(120, 20);

            string geneName = Colour.values[Colour.valuesComp[i].Index].Name;
            float conf = Colour.valuesComp[i].Value;

            // Initiate column 3 (button)
            GameObject newCell1 = Instantiate(col2) as GameObject;
            newCell1.tag = "clone";
            Button btn = newCell1.GetComponent<Button>();
            btn.name = geneName;
            btn.onClick.AddListener(delegate
                {
                    colBtnActions(geneName, i);
                });
            btn.GetComponentInChildren<Text>().text = SentenceCase(geneName);
            newCell1.transform.SetParent(this.gameObject.transform, false); // Load column 2 in to table

            // Initiate column 2 (similarity level (float))
            GameObject newCell2 = Instantiate(col1) as GameObject;
            newCell2.tag = "clone";
            Text txt2 = newCell2.GetComponent<Text>();

            //txt2.text = conf.ToString("+0.0000;-0.0000"); //.ToString ("+0.00;−0.00");
            txt2.text = conf.ToString("+00.00%;-00.00%");

            newCell2.transform.SetParent(this.gameObject.transform, false); // Load column 1 in to table


        }

        upto = upto + rows;


        if (upto < 400 && upto < Colour.valuesCount - 1)
        {
            GameObject moreCell = Instantiate(col2) as GameObject;
            moreCell.tag = "more";
            Button moreBtn = moreCell.GetComponent<Button>();
            moreBtn.name = @"More";
            moreBtn.image.color = new Color(234 / 255F, 255 / 255F, 239 / 255F);
            moreBtn.onClick.AddListener(delegate
                {
                    moreBtnActions();
                });
            moreBtn.GetComponentInChildren<Text>().text = @"More..";
            moreBtn.transform.SetParent(this.gameObject.transform, false); // Load column 2 in to table
        }



    }

    public void wake()
    {
        LH.SetActive(true);
    }

    public void sleep()
    {
        LH.SetActive(false);
    }

}
