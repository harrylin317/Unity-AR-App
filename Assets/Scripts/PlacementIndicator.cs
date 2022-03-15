using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
public class PlacementIndicator : MonoBehaviour
{
    [SerializeField]
    private GameObject placementIndicator;

    [SerializeField]
    private List<GameObject> objectToPlaceList = new List<GameObject>();

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
    private bool touchingObject = false;

    //placing and selecting object
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
    private Button destroyButton;
    [SerializeField]
    private GameObject rotationButtons;
    [SerializeField]
    private GameObject scaleButtons;
    [SerializeField]
    private Text scanningText;
    [SerializeField]
    private Button clearButton;
    void Start()
    {
        //canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        //placeObjectButton = GameObject.Find("PlaceObjectButton").GetComponent<Button>();
        //destroyButton = GameObject.Find("DestroyObjectButton").GetComponent<Button>();
        //rotationSlider = GameObject.Find("DestoryObjectButton").GetComponent<Slider>();

        //raycastManager = FindObjectOfType<ARRaycastManager>();
        clearButton.gameObject.SetActive(true);

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
            destroyButton.gameObject.SetActive(true);
            scaleButtons.gameObject.SetActive(true);
            rotationButtons.gameObject.SetActive(true);
            
        }
        else
        {
            if (placementPoseIsValid)
            {
                scanningText.gameObject.SetActive(false);
                placeObjectButton.gameObject.SetActive(true);

            }
            else
            {
                scanningText.gameObject.SetActive(true);
                placeObjectButton.gameObject.SetActive(false);
            }
            destroyButton.gameObject.SetActive(false);
            scaleButtons.gameObject.SetActive(false);
            rotationButtons.gameObject.SetActive(false);

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
            newObjectPlaced = Instantiate(objectToPlaceList[0], placementIndicator.transform.position, placementIndicator.transform.rotation);
            placedObjectCount++;
            if(currentNameNum == -1)
            {
                newObjectPlaced.name = placedObjectCount.ToString() + "-" + objectToPlaceList[0].name;

            }
            else
            {
                newObjectPlaced.name = currentNameNum.ToString() + "-" + objectToPlaceList[0].name;
                currentNameNum = -1;

            }
            objectPlacedList.Add(newObjectPlaced);

            Debug.Log("placing object");

            foreach (var x in objectPlacedList)
            {
                Debug.Log("currently in list: " + x.name);
            }

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

    public void ClearScene()
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

        foreach (var x in objectPlacedList)
        {
            Debug.Log("currently in list: " + x.name);
        }
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
                                touchingObject = true;
                                selectedObject = objectPlacedList[i];
                                Debug.Log("currently selected: " + selectedObject.name);
                                                               
                            }
                        }
                    }
                    
                    //Debug.Log("Name is: " + hitObject.collider.gameObject.name);
                    //Debug.Log("Tag is: " + hitObject.collider.gameObject.tag);
                    /*onTouchHold = true;
                    Destroy(hitObject.transform);*/

                }
                else
                {
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

                if (touchingObject)
                {
                    touchingObject = false;
                }

            }

            if (onTouchHold && selectedObject != null && touchingObject)
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
