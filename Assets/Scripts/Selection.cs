using UnityEngine;
using UnityEngine.UI;

 // Needed to access onscreen elements
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Linq;

/*****
 * 
 * Allows the selection of groups of heart pieces for differential expression analysis
 * 
 *****/

public class Selection : MonoBehaviour
{
    public Camera CC;
    // Allows access to camera (to shoot detection ray)
    public Text pT;
    // Allows us access to "Selected piece" text
    public GameObject LH;
    // Access to the ListHolder (to turn the data panel on and off)

    public GameObject ES;

    string hName = "";
    // Name of heart piece that is highlighted

    Vector3 originalPosition;

    void Update()
    {
        // Send out ray to see if mouse is hovering over a heart piece

        if (Input.GetMouseButtonDown(0) && Compare.first != 0 && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        { // and not over a UI element?
			
            Ray ray = CC.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            { // If it hits a piece of heart

                hName = hit.transform.name;

                if (Compare.first == 1 && !Compare.pieces2.Contains(hName))
                { // Green
                    if (Compare.pieces1.Contains(hName))
                    {
                        // Remove it
                        outRemove(hName, 1);
                    }
                    else
                    {
                        // Add it
                        outAdd(hName, 1);
                    }
                }
						
                if (Compare.first == 2 && !Compare.pieces1.Contains(hName))
                { // Red
                    if (Compare.pieces2.Contains(hName))
                    {
                        // Remove it
                        outRemove(hName, 2);
                    }
                    else
                    {
                        // Add it
                        outAdd(hName, 2);
                    }
                }

            }

        }

    }

    private void outAdd(string hName, int comp)
    {
        if (comp == 1)
        {
            Compare.pieces1.Add(hName);
        }
        else
        {
            Compare.pieces2.Add(hName);
        }
        makeOutline(hName);
    }

    private void outRemove(string hName, int comp)
    {
        if (comp == 1)
        {
            Compare.pieces1.Remove(hName);
        }
        else
        {
            Compare.pieces2.Remove(hName);
        }
        makeNormal(hName);
    }

    private void makeOutline(string oName)
    {
		
        Material sMat = GameObject.Find(oName).transform.GetComponent<Renderer>().material;
        sMat.shader = Shader.Find("Outlined/Silhouetted Bumped Diffuse");
        if (Compare.first == 1)
        {
            sMat.SetColor("_OutlineColor", new Color(0, 0, 1)); 
        }
        else
        {
            sMat.SetColor("_OutlineColor", new Color(1, 0, 0));
        }
        sMat.SetFloat("_Outline", 0.002F);

    }

    private void makeNormal(string hName)
    {
        GameObject.Find(hName).GetComponent<Renderer>().material.shader = Shader.Find("Standard");
    }

    /* To make them shine a bit different
	private void makeReflective(string oName) {
		Material mat = GameObject.Find (oName).transform.GetComponent<Renderer> ().material;
		mat.shader = Shader.Find ("Specular");
	}
	*/

    public void complementAdd()
    {
        // Get the piecies not in Pieces1 or Pieces2
        List<string> hp = new List<string> { "A_1", "A_2", "A_3", "A_4", "B_1", "B_2", "B_3", "B_4", "C_1", "C_2", "C_3", "C_4", "D_1", "D_2", "D_3", "E_1", "E_2", "E_3" };

        foreach (string piece in Compare.pieces1.Concat(Compare.pieces2))
        {
            hp.Remove(piece);
        }
			
        foreach (string piece in hp)
        {
            outAdd(piece, 2);
        }

    }

    public void Reset()
    { // Reset text and put any out pieces in
        pT.text = "Selected piece: None";
        LH.SetActive(false);

        foreach (string hp in Colour.hp)
        {
            outRemove(hp, 1);
            outRemove(hp, 2);
        }
    }
		
    // Uncomment to take screenshots by pressing the 'K' key during runtime


    /**
	public static string ScreenShotName(int width, int height) {
		return string.Format("/Users/alex/Downloads/screen_{1}x{2}_{3}.png", 
			Application.dataPath, 
			width, height, 
			System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}
		
	private bool takeHiResShot = false;
	
	public void TakeHiResShot() {
		takeHiResShot = true;
	}

	
	void LateUpdate() {
		
		takeHiResShot |= Input.GetKeyDown("k");

		if (takeHiResShot) {

			int scale = 7;

			int resWidth = 1088*scale; 
			int resHeight = 642*scale;
			
			RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
			CC.targetTexture = rt;
			Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
			CC.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
			CC.targetTexture = null;
			RenderTexture.active = null; // JC: added to avoid errors
			Destroy(rt);
			byte[] bytes = screenShot.EncodeToPNG();
			string filename = ScreenShotName(resWidth, resHeight);
			System.IO.File.WriteAllBytes(filename, bytes);
			Debug.Log(string.Format("Took screenshot to: {0}", filename));
			takeHiResShot = false;

		}
			
	}
	**/

		
}
