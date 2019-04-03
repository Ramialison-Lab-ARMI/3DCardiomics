using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

/*****
 * 
 * Populates the gene name suggestion dropdown
 * 
 *****/

public class dPanel : MonoBehaviour
{

    public GameObject col2;
    // Our template column2 cell (The button)
    public GameObject DD;
    // Dropdown holder
    public GameObject SH;
    // Script holder
    public GameObject scrollBar;


    public InputField IF;

    void Start()
    {
        DD.SetActive(false);
    }

    // Generates our suggested genes dropdown from the input in 'InputField'
    public void generateTable(string geneName)
    {

        // First remove all existing suggestions
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("dClone");
        foreach (GameObject target in gameObjects)
        {
            GameObject.Destroy(target);
        }

        // Find where up to in Colour.GeneIdName table (for dropdown selection) we are
        int upTo = 0;
        while (upTo < Colour.valuesCount && String.Compare(Colour.values[upTo].Name, geneName) < 0)
        {
            upTo++;
        }

        // So we dont accidentally try and print more genes than we actually have
        int max = 20; 
        if (Colour.valuesCount - upTo < 20)
        {
            max = Colour.valuesCount - upTo;
        }

        // Start making rows of table
        for (int i = upTo; i < upTo + max; i++)
        {

            // Initiate grid
            GridLayoutGroup grid = this.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(120, 20);

            // The button column
            GameObject newCell1 = Instantiate(col2) as GameObject;
            newCell1.tag = "dClone";
            Button btn = newCell1.GetComponent<Button>();
            btn.name = "Gene";//Colour.geneId [i];
            string gName = Colour.values[i].Name;
            btn.onClick.AddListener(delegate
                {
                    btnAction(gName);
                });
            btn.GetComponentInChildren<Text>().text = SentenceCase(gName);
            newCell1.transform.SetParent(this.gameObject.transform, false); // Load the column in to the table

        }
    }

    public void btnAction(string gName)
    {  // Function linked to button which uses ColourFromText in Colour class
        scrollBar.GetComponent<Scrollbar>().value = 1;

        IF.text = gName;
        SH.GetComponent<Colour>().ColourFromText(gName, true);
        DD.SetActive(false);
    }

    public void wake()
    {
        if (IF.text != "")
        {
            DD.SetActive(true);
            generateTable(IF.text.ToLower());
        }
        else
        {
            scrollBar.GetComponent<Scrollbar>().value = 1;
            DD.SetActive(false);
        }
    }

    public void sleep()
    {
        DD.SetActive(false);
    }

    public void reset()
    {
		
    }

    public static string SentenceCase(string input)
    {
        if (input.Length < 1)
            return input;
        string sentence = input.ToLower();
        return "<i>" + sentence[0].ToString().ToUpper() + sentence.Substring(1) + "</i>";
    }

}
