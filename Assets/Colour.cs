using UnityEngine;
using UnityEngine.UI; // Needed to access onscreen elements
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
/*****
 * 
 * Arranges for the colouring of the heart due to the gene selected, also controls colour blind modes and computing Pearson correlations
 * 
 *****/

public class Colour : MonoBehaviour {

	public struct DataStringFloatArray // To store geneIdName
	{
		public DataStringFloatArray(string stringValue, float[] floatValues)
		{
			StringData = stringValue;
			FloatDatas = floatValues;
		}

		public string StringData { get; private set; }
		public float[] FloatDatas { get; private set; }
	}

	public struct DataIntFloat // To store the comparison values
	{
		public DataIntFloat(int intValue, float floatValue)
		{
			IntegerData = intValue;
			FloatData = floatValue;
		}

		public int IntegerData { get; private set; }
		public float FloatData { get; private set; }
	}

	public static List<DataStringFloatArray> values = new List<DataStringFloatArray>(); // This holds the name and all the 18 values of the gene 
	public static List<DataIntFloat> valuesComp = new List<DataIntFloat>(); // The comparison values to the current gene

	// Provides easy access to all pieces of the heart to iterate over
	public static string[] hp = new string[18] {"A_1","A_2","A_3","A_4","B_1","B_2","B_3","B_4","C_1","C_2","C_3","C_4","D_1","D_2","D_3","E_1","E_2","E_3"};
	public static int valuesCount = 0;

	private static float max;
	private static bool norm;
	private static string currentGene = "";
	//private static string baseGene = "";
	private static bool cb = false;

	public InputField IF; // Allows you to connect to the inputField to modify it
	public GameObject content; // Allows generating table and hiding/showing it
	public Button eyeButton;
	public Button colourButton;
	public Sprite eyeCross;
	public Sprite colourBlind;

	public Text gText; // Allows you access to the geneBtn text
	public Text mText; // Mode text

	// Run on initial load
	void Start() 
	{
		//LoadDataset ();
		LoadDatasetFORWEBPLAYER();
		resetColour();
	}


	// Reset everything to how it was at the start
	public void Reset(){
		cb = false;
		eyeButton.image.overrideSprite = null;
		colourButton.image.overrideSprite = null;
		IF.text = "";
		resetColour ();
		gText.text = "Current gene: None";
		currentGene = "";
		//baseGene = "";

		if (norm) {
			normalise ();
		}
	}

	// Parse the expression value database in to memory as an array of strings and floats
	public void LoadDatasetFORWEBPLAYER(){

		TextAsset textAsset = Resources.Load ("fernP4") as TextAsset; //string input =  result.text;
		string[] wArray = textAsset.text.Split("\n" [0]);
		Resources.UnloadAsset (textAsset);

		Debug.Log (wArray.Length);

		string line;
		int i = wArray.Length;

		max = 0;
		norm = false;

		float current = 0;

		while(valuesCount<i)
		{
			line = wArray [valuesCount];
			char delim = '\t';
			string[] cols;
			cols = line.Split (delim);
			float[] floats = new float[18];

			// Populate the array with values
			for (int j = 1; j < cols.Length; j++) {
				current = float.Parse(cols [j], CultureInfo.InvariantCulture.NumberFormat);
				floats [j - 1] = current; // Create a temporary array of floats and add expression values

				if (current > max) {
					max = current;
				}
			}
			values.Add (new DataStringFloatArray(cols[0].ToLower(), floats)); // Add the gene expression values to values array of floats

			valuesCount++;

		}
			
		// Sort list (List already sorted in CSV) but: (Excel and C# have different sorting methods, e.g. for '_')
		//values.Sort((s1, s2) => s1.StringData.CompareTo(s2.StringData));
	}

	// Find the expression values corresponding with the entered gene, then start the colouring and similarity calculating processes
	public void ColourFromText(string geneName, bool panel = false){ 

		currentGene = geneName;
		// Search the array for the gene name and get its index (location)
		int found = -1;
		for (int i = 0; i < valuesCount; i++) {
			if (geneName == values [i].StringData) {
				found = i;
				break;
			}
		}

		// If the gene name was found, load that dataset into the pieces
		if (found > -1) {

			if (panel) {
				computeDistancesP (found);
				//baseGene = currentGene;
			}

			// Update current gene info
			gText.text = "Current gene: " + SentenceCase(geneName);

			float lMax = -1;
			float lMin = 100;
			if (norm) { // Find the local min and max if in normalised mode
				for (int i = 0; i < 18; i++) {
					if (values [found].FloatDatas [i] > lMax){
						lMax = values [found].FloatDatas [i];
					}
					if (values [found].FloatDatas [i] < lMin){
						lMin = values [found].FloatDatas [i];
					}
				}
			}
			for (int i = 0; i < 18; i++) {
				colourHeartPiece (hp [i], values [found].FloatDatas [i], lMax, lMin);
			}

		} else {
			// Update current gene info
			gText.text = "Current gene: None";
			resetColour ();
			//Debug.Log ("Gene with name " + geneName + " not found.");
			content.GetComponent<PanelScript>().sleep();
		}
	}

	public static string SentenceCase(string input) {
		if (input.Length < 1)
			return input;
		string sentence = input.ToLower();
		return "<i>" + sentence[0].ToString().ToUpper() + sentence.Substring(1) + "</i>";
	}


	// Calculate the similarity distances (Pearson) between our current gene and the other genes
	public void computeDistancesP(int found){
		
		valuesComp.Clear();

		float [] valuesI = values[found].FloatDatas;

		// Compute mean of I row (CurrentV)
		float valuesImean = 0;
		for (int j = 0; j < 18; j++) {
			valuesImean = valuesImean + valuesI [j];
		}
		valuesImean = valuesImean * (1 / (float)18); // Correct

		for (int i = 0; i < valuesCount; i++) {
			// Pearsons Correlation Coefficient

			// CurrentV = I
			// NextV	= J

			float[] valuesJ = values [i].FloatDatas;

			// Compute mean of J row (NextV)
			float valuesJmean = 0;
			for (int j = 0; j < 18; j++) {
				valuesJmean = valuesJmean + valuesJ [j];
			}
			valuesJmean = valuesJmean * (1 / (float)18); //Correct

			// Compute Zone1
			float zone1 = 0;
			for (int j = 0; j < 18; j++) {
				zone1 = zone1 + ((valuesI [j] - valuesImean) * (valuesJ [j] - valuesJmean));
			}
			//zone1 = Mathf.Sqrt (zone1);

			// Compute Zone2
			float zone2 = 0;
			for (int j = 0; j < 18; j++) {
				zone2 = zone2 + Mathf.Pow((valuesI[j] - valuesImean), 2);
			}
			zone2 = Mathf.Sqrt (zone2);

			// Compute Zone3
			float zone3 = 0;
			for (int j = 0; j < 18; j++) {
				zone3 = zone3 + Mathf.Pow((valuesJ[j] - valuesJmean), 2);
			}
			zone3 = Mathf.Sqrt (zone3);

			float sum = zone1/(zone2*zone3); 				// This Gives R

			if (sum > 0) {									// This Gives R^2
				sum = Mathf.Pow(sum,2) ; 	
			} else {
				sum = -1 * Mathf.Pow(sum,2) ;
			}
				
			if (!float.IsNaN (sum)) {
				valuesComp.Add (new DataIntFloat (i, sum));
			} else {
				valuesComp.Add (new DataIntFloat (i, 0));
			}

		}

		// Remove ABS if you don't want negative (inversee) correlations in your 'Similar Genes' panel
		valuesComp.Sort((s1, s2) => Mathf.Abs(s2.FloatData).CompareTo(Mathf.Abs(s1.FloatData))); // Sort so that those with most similarity are first
		content.GetComponent<PanelScript>().generateTable(found);
	}



	// Colours a given heart piece based on the expression value
	public void colourHeartPiece(string heartPiece, float exp, float lMax, float lMin){

		// Declare variables
		float rgb = 255;
		float t;

		// Convert value to a number between 0 and 1
		if (norm) {
			t = (exp-lMin) / (lMax-lMin);
		} else {
			t = exp / (max);
		}

		// Create a gradient
		Gradient g = new Gradient();
		GradientColorKey[] gck;

		if (cb) { // For colour blindness you can use 2 easy to see colours

			gck = new GradientColorKey[2];

			gck[0].color = Color.blue;
			gck[0].time = 0.0F;
			gck[1].color = Color.yellow;
			gck[1].time = 1.0F;

		} else { // Otherwise we can use heatmap colours from blue to red
			
			gck = new GradientColorKey[5];

			gck[0].color = new Color (0 / rgb, 0 / rgb, 244 / rgb); // Blue
			gck[0].time = 0.0F;
			gck[1].color = new Color (24 / rgb, 226 / rgb, 240 / rgb); // Cyan
			gck[1].time = 0.25F;
			gck[2].color = new Color (255 / rgb, 255 / rgb, 0 / rgb); // Yellow
			gck[2].time = 0.50F;
			gck[3].color = new Color (255 / rgb, 170 / rgb, 0 / rgb); // Orange
			gck[3].time = 0.75F;
			gck[4].color = new Color (254 / rgb, 0 / rgb, 0 / rgb); // Red
			gck[4].time = 1.0F;

		}

		g.SetKeys(gck, new GradientAlphaKey[0]); // Make all colours visible

		// Associate a decimal with a colour and change the heart piece
		GameObject.Find(heartPiece).GetComponent<Renderer>().material.color = g.Evaluate(t); 
			
	}



	// Resets the colour of all the heart pieces back to white
	public void resetColour(){
		
		// Resets all heart pieces to white
		for (int i=0; i<18; i++){
			GameObject.Find(hp[i]).GetComponent<Renderer>().material.color = Color.white;
		}

	}

	// Toggles the colour blind mode
	public void eyePress(){
		
		cb = !cb;

		if (cb) { // If they are colourblind
			eyeButton.image.overrideSprite = eyeCross;
			colourButton.image.overrideSprite = colourBlind;
		} else { // If they are not colourblind
			eyeButton.image.overrideSprite = null;
			colourButton.image.overrideSprite = null;
		}

		// Swap colour modes if a gene is active
		if (currentGene != "") {
			ColourFromText (currentGene, false);
		}

	}

	// Stretches the values over all the colour range for more resolution
	public void normalise() {
		
		norm = !norm;

		// Swap normalise modes if a gene is active
		if (currentGene != "") {
			ColourFromText (currentGene, false);
		}


	}

}
