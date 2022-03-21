using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
public class PlacementIndicator : MonoBehaviour
{
    [SerializeField]
    private ARSession session;

    [SerializeField]
    private GameObject placementIndicator;

    [SerializeField]
    private List<GameObject> objectToPlaceList = new List<GameObject>();
    private int objectToPlaceIndex = 0;
    [SerializeField]
    private GameObject changeObjectButtons;
    private Text objectToPlaceText;

    [SerializeField]
    private ARPlaneManager planeManager;

    //ray casting
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    [SerializeField]
    private ARRaycastManager raycastManager;

    //placement indicators
    private bool placementPoseIsValid = false;
    private Pose placementIndicatorPose;

    //screen touching
    private bool onTouchHold = false;
    private Vector2 touchPosition = default;
    //private bool touchingObject = false;

    //placing and selecting object
    [HideInInspector]
    public GameObject selectedObject = null;
    private GameObject newObjectPlaced = null;
    private List<GameObject> objectPlacedList = new List<GameObject>();
    [SerializeField]
    private int maxObjectPlacedCount = 5;
    private int placedObjectCount = 0;
    private int currentNameNum = -1;

    //UI
    [SerializeField]
    private Button placeObjectButton;
    [SerializeField]
    private Text scanningText;
    [SerializeField]
    private Button togglePlaneDetectionButton;
    [SerializeField]
    private GameObject selectObjectButtons;
    void Start()
    {        
        //raycastManager = FindObjectOfType<ARRaycastManager>();
        
        objectToPlaceText = changeObjectButtons.GetComponentInChildren<Text>();
        objectToPlaceText.text = objectToPlaceList[objectToPlaceIndex].name;

    }

    // Update is called once per frame
    void Update()
    {
        UpdateUI();
        UpdatePlacementIndicatorPose();
        UpdatePlacementIndicator();
        DragObject();
        
    }
    void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid && selectedObject == null)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementIndicatorPose.position, placementIndicatorPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }

    }
    void UpdateUI()
    {
        
        if (selectedObject != null)
        {
            placeObjectButton.gameObject.SetActive(false);
            selectObjectButtons.SetActive(true);
            changeObjectButtons.SetActive(false);
        }
        else
        {
            if (placementPoseIsValid)
            {
                scanningText.gameObject.SetActive(false);
                placeObjectButton.gameObject.SetActive(true);
                changeObjectButtons.SetActive(true);

            }
            else
            {
                scanningText.gameObject.SetActive(true);
                placeObjectButton.gameObject.SetActive(false);
                changeObjectButtons.SetActive(false);
            }
            selectObjectButtons.SetActive(false);
        }
    }


    void UpdatePlacementIndicatorPose()
    {
        Vector2 screenPosition = Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));
        raycastManager.Raycast(screenPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            //placementIndicatorPose = hits[0].pose;

            //Vector3 cameraForward = Camera.current.transform.forward;
            //Vector3 cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            //placementIndicatorPose.rotation = Quaternion.LookRotation(cameraBearing);
            placementIndicatorPose.position = hits[0].pose.position;
            placementIndicatorPose.rotation = hits[0].pose.rotation;
        }
       
    }
    public void PlaceObject()
    {
        if (placementPoseIsValid && placedObjectCount < maxObjectPlacedCount && selectedObject == null)
        {
            Debug.Log("placing object " + objectToPlaceList[objectToPlaceIndex].name);

            newObjectPlaced = Instantiate(objectToPlaceList[objectToPlaceIndex], placementIndicator.transform.position, placementIndicator.transform.rotation);
            newObjectPlaced.GetComponent<Outline>().enabled = false;
            placedObjectCount++;
            if(currentNameNum == -1)
            {
                newObjectPlaced.name = placedObjectCount.ToString() + "-" + objectToPlaceList[objectToPlaceIndex].name;
            }
            else
            {
                newObjectPlaced.name = currentNameNum.ToString() + "-" + objectToPlaceList[objectToPlaceIndex].name;
                currentNameNum = -1;
            }
            objectPlacedList.Add(newObjectPlaced);

            Debug.Log("placed object "+ newObjectPlaced.name);

            //foreach (var x in objectPlacedList)
            //{
            //    Debug.Log("currently in list: " + x.name);
            //}

        }
        else
        {
            Debug.Log("maximum object reached: " + placedObjectCount.ToString());
            return;
        }
                        
    }

    
    public void DeleteObject()
    {
        if (selectedObject != null)
        {
            int dashIndex = selectedObject.name.IndexOf('-');
            currentNameNum = int.Parse(selectedObject.name.Substring(0, dashIndex));
            Debug.Log("removing: " + selectedObject.name);
            Debug.Log("currentNameNum: " + currentNameNum.ToString());

            objectPlacedList.Remove(selectedObject);
            Destroy(selectedObject.gameObject);
            selectedObject = null;
            placedObjectCount--;
            if(placedObjectCount == 0)
            {
                currentNameNum = -1;
            }
        }

        foreach (var x in objectPlacedList)
        {
            Debug.Log("currently in list: " + x.name);
        }
    }

    public void ClearObjects()
    {
        for (int i = 0; i < objectPlacedList.Count; i++)
        {
            Destroy(objectPlacedList[i].gameObject);            
        }
        objectPlacedList = new List<GameObject>();
        selectedObject = null;
        placedObjectCount = 0;
        currentNameNum = -1;

        Debug.Log("clearing scene");
             
        
        //foreach (var x in objectPlacedList)
        //{
        //    Debug.Log("currently in list: " + x.name);
        //}
    }

    public void ResetScene()
    {
        ClearObjects();
        session.Reset();
    }
    public void TogglePlaneDetection()
    {
        planeManager.enabled = !planeManager.enabled;

        foreach (ARPlane plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(planeManager.enabled);
        }
        togglePlaneDetectionButton.GetComponentInChildren<Text>().text = planeManager.enabled ? "Disable Plane Detection" : "Enable Plane Detection";
        Debug.Log("toggling button to" + planeManager.enabled.ToString());
    }

    public void SelectObjectRight()
    {
        if (objectToPlaceIndex == objectToPlaceList.Count - 1)
        {
            objectToPlaceIndex = 0;
        }
        else objectToPlaceIndex++;

        Debug.Log("current index" + objectToPlaceIndex.ToString());
        objectToPlaceText.text = objectToPlaceList[objectToPlaceIndex].name;

    }
    public void SelectObjectLeft()
    {
        if (objectToPlaceIndex == 0)
        {
            objectToPlaceIndex = objectToPlaceList.Count - 1;
        }
        else objectToPlaceIndex--;

        Debug.Log("current index" + objectToPlaceIndex.ToString());
        objectToPlaceText.text = objectToPlaceList[objectToPlaceIndex].name;


    }


    void DragObject()
    {
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPosition = touch.position;

            //prevent touch object under the UI
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;
                                       

            if (touch.phase == TouchPhase.Began)
            {

                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hitObject;
                if(Physics.Raycast(ray, out hitObject))
                {
                    if (hitObject.collider.gameObject.tag == "Spawnable")
                    {
                        //onTouchHold = true;

                        //select object
                        for (int i = 0; i < objectPlacedList.Count; i++)
                        {
                            if (objectPlacedList[i].name == hitObject.collider.gameObject.name)
                            {
                                //touchingObject = true;
                                if (selectedObject != null)
                                {
                                    selectedObject.GetComponent<Outline>().enabled = false;
                                }
                                selectedObject = objectPlacedList[i];
                                Debug.Log("currently selected: " + selectedObject.name);
                                selectedObject.GetComponent<Outline>().enabled = true;
                            }
                        }
                    }

                    Debug.Log("Name is: " + hitObject.collider.gameObject.name);
                    Debug.Log("Tag is: " + hitObject.collider.gameObject.tag);
                    /*onTouchHold = true;
                    Destroy(hitObject.transform);*/

                }
                else
                {
                    if(selectedObject != null)
                    {
                        selectedObject.GetComponent<Outline>().enabled = false;
                    }
                    selectedObject = null;
                    Debug.Log("no object selected");

                }
            }

            if (touch.phase == TouchPhase.Moved)
            {
                touchPosition = touch.position;
                onTouchHold = true;
                
            }

            if (touch.phase == TouchPhase.Ended)
            {
                onTouchHold = false;

                //if (touchingObject)
                //{
                //    touchingObject = false;
                //}

            }

            //if (onTouchHold && selectedObject != null && touchingObject)
            if (onTouchHold && selectedObject != null)
            {
                if (raycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
                {
                    Pose hitPose = hits[0].pose;                                        
                    selectedObject.transform.position = hitPose.position;
                    selectedObject.transform.rotation = hitPose.rotation;                                       
                }
            }            
        }                 
    }
}
