using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

/*****

 * 
 * Rotates the camera around an invisible cube ("CentreCube"), placed in the middle of the heart.
 * 
 *****/

public class CameraRotation : MonoBehaviour
{

	public Vector3 camPos = Vector3.zero;
	public Quaternion camRot;

	public GameObject big;


	// Saves the camera starting position
	void Start()
	{
		camPos = transform.position;
		camRot = transform.rotation;
	}

	// Resets camera position to the middle
	public void Reset()
	{

		// Smooth reset maybe? (Tried Lerping, looks ugly from half the points as doesn't go smoothly around

		transform.position = camPos;
		transform.rotation = camRot;
	}

	// So we don't drag the heart around while moving the scrollbars
	bool down = false;

	void Update()
	{
		// Mouse control of camera rotation
		if (!down && Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			down = true;
		}
		if (down)
		{
			transform.RotateAround(GameObject.Find("CentreCube").transform.position, Vector3.up, Input.GetAxis("Mouse X") * 7);
			transform.RotateAround(GameObject.Find("CentreCube").transform.position, -transform.right, Input.GetAxis("Mouse Y") * 5);

			if (!Input.GetMouseButton(0))
			{
				down = false;
			}
		}
		// Could put touchscreen input here

		// Keyboard control of camera rotation
		if (Input.GetKey(KeyCode.UpArrow))
		{
			//transform.RotateAround (GameObject.Find ("CentreCube").transform.position, Vector3.right, 200 * Time.deltaTime);
			transform.RotateAround(GameObject.Find("CentreCube").transform.position, -transform.right, 200 * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			//transform.RotateAround (GameObject.Find ("CentreCube").transform.position, Vector3.left, 200 * Time.deltaTime);
			transform.RotateAround(GameObject.Find("CentreCube").transform.position, transform.right, 200 * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			transform.RotateAround(GameObject.Find("CentreCube").transform.position, Vector3.down, 200 * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			transform.RotateAround(GameObject.Find("CentreCube").transform.position, Vector3.up, 200 * Time.deltaTime);
		}
	}

	public void mouseOn()
	{
		big.SetActive(true);
	}

	public void mouseOff()
	{
		big.SetActive(false);
	}

}
