using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHeldEvent : MonoBehaviour
{
    PlacementIndicator controllerScript;
    private bool buttonHeldDown = false;
    private string currentButtonName = null;
    [SerializeField]
    private int rotationValue = 5;
    [SerializeField]
    private Vector3 scaleValue = new Vector3(0.01f, 0.01f, 0.01f);

    void Start()
    {
        GameObject controller = GameObject.Find("Controller");
        controllerScript = controller.GetComponent<PlacementIndicator>();

    }

    // Update is called once per frame
    void Update()
    {
        if (buttonHeldDown && controllerScript.selectedObject != null)
        {
            if(currentButtonName == "ScaleArrowUp")
            {
                controllerScript.selectedObject.transform.localScale += scaleValue;
            }
            else if(currentButtonName == "ScaleArrowDown" && controllerScript.selectedObject.transform.localScale != new Vector3(0.01f, 0.01f, 0.01f))
            {
                controllerScript.selectedObject.transform.localScale -= scaleValue;
            }
            else if(currentButtonName == "RotateArrowRight")
            {
                controllerScript.selectedObject.transform.Rotate(new Vector3(0, rotationValue, 0), Space.Self);
            }
            else if(currentButtonName == "RotateArrowLeft")
            {
                controllerScript.selectedObject.transform.Rotate(new Vector3(0, -rotationValue, 0), Space.Self);
            }
        }
    }
    public void HoldButton(Button button)
    {
        buttonHeldDown = true;
        currentButtonName = button.name;
    }

    public void ReleaseButton(Button button)
    {
        buttonHeldDown = false;

    }
}
