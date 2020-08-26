using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using System.IO;

// TODO: use JsonUtility in modern Unity
// using UnityEngine.JSONSerializeModule
using Boomlagoon.JSON;

public class FileUpload : MonoBehaviour
{

    [DllImport("__Internal")]
    private static extern void JsAlert(string msg);

    [DllImport("__Internal")]
    private static extern void JsCreateHiddenFileInput();

    [DllImport("__Internal")]
    private static extern void JsShowFileInput();

    public GameObject loadingSpinner;

    IEnumerator Start()
    {
        yield return null;

        #if UNITY_WEBGL && !UNITY_EDITOR
        JsCreateHiddenFileInput();
        #endif

        // var genes = LoadPresetGeneSet("geneset");
        // StartCoroutine(GameObject.Find("ScriptHolder").GetComponent<Colour>().ColourByGeneSet(genes));
    }

    public void ShowFileUploadDialog()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        JsShowFileInput();
        #endif
    }

    public void OnReceiveUpload(string jsonText)
    {
        GeneSet genes = null;
        try {
            loadingSpinner.SetActive(true);
            var json = JSONObject.Parse(jsonText);
            var filename = json.GetString("filename");
            var data = json.GetString("data");

            Debug.Log("OnReceiveUpload filename: " + filename);
            Debug.Log("OnReceiveUpload data: " + data);

            genes = ParseGeneSet(data);

            DebugLogGeneSet(genes);
        }
        catch {
            loadingSpinner.SetActive(false);
        }

        StartCoroutine(GameObject.Find("ScriptHolder").GetComponent<Colour>().ColourByGeneSet(genes));
    }

    public GeneSet LoadPresetGeneSet(string resourceName)
    {
        GeneSet genes = null;
        try
        {
            loadingSpinner.SetActive(true);

            TextAsset textAsset = Resources.Load("genesets/" + resourceName) as TextAsset;
            genes = ParseGeneSet(textAsset.text);
            Resources.UnloadAsset(textAsset);

            DebugLogGeneSet(genes);
        }
        catch {
            loadingSpinner.SetActive(false);
        }

        return genes;
    }

    public void ShowPresetGeneSet(string genesetName)
    {
        var genes = LoadPresetGeneSet(genesetName);
        StartCoroutine(GameObject.Find("ScriptHolder").GetComponent<Colour>().ColourByGeneSet(genes));
    }

    public void DebugLogGeneSet(GeneSet genes)
    {
        Debug.Log("Gene set name: " + genes.Name);
        Debug.Log("Gene set description: " + genes.Description);
        Debug.Log("Gene set genes: " + string.Join(", ", genes.Genes.ToArray()));
    }

    public GeneSet ParseGeneSet(string text)
    {
        /*
         * Parse a list of gene names, one per line. Also detects and parses GSEA/MSigDB format
         * 'text', 'grp' and 'gmx' format (where the first two header lines are gene set name 
         *  and description)
        */

        var geneset = new GeneSet();


        string name = "";
        string description = "";
        List<string> genes = new List<string>();

        // We also split on escaped endline sequences to deal with strings deserialized from JSON
        string[] lines = text.Split(
                             new[] { "\r\n", "\r", "\n", "\\r\\n", "\\r", "\\n" },
                             System.StringSplitOptions.RemoveEmptyEntries);

        int count = 0;
        foreach (var line in lines)
        {
            count++;
            if (count == 2 &&
                (line.Substring(0, 2) == "> " || line.Substring(0, 2) == "# "))
            {
                name = genes[0];
                genes.RemoveAt(0);
                description = line.Substring(2).Trim();
                continue;
            }
            var geneName = line.Trim();
            if (!string.IsNullOrEmpty(geneName))
            {
                genes.Add(geneName);
            }
        }

        geneset.Name = name;
        geneset.Description = description;
        geneset.Genes = genes;

        return geneset;
    }

}
