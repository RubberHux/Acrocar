using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class LevelEditor : MonoBehaviour
{
    public GameObject level; // container object for entire level
    public GameObject spawnPoint; // initial spawner for car
    public GameObject carLoader; // car loader object (prefab)
    public GameObject editorCamera; // camera for level editor (separate from when playing)
    public List<GameObject> objectList; // list of objects which can be created
    public Material normalMaterial; // default material to slap on objects
    public Material grappleMaterial; // material for grappleable stuff
    public Material lavaMaterial; // material for lava
    public Material breakableMaterial; // material for breakable stuff
    public TMP_Text addText; // text showing which object to add
    public TMP_Dropdown editorModes; // modes for the editor
    public GameObject propertiesWindow; // window for object properties
    public TMP_Text playButton; // button for toggling play/edit mode

    private GameObject currObject; // current object selected
    private GameObject copiedObject; // object currently copied
    private GameObject spawnedCarLoader; // actual car loader spawned when playtesting
    private bool interacting; // is current object being interacted with?
    private Vector3 clickPoint; // point on object where it was clicked
    private Vector3 mousePos; // current mouse position in the world
    private Vector3 prevMousePos; // previous mouse pos (used for delta calcs)
    private Vector3 moveOffset; // offset between click point and object pos
    private int objectIndex; // current index of objectList
    private bool playing; // playtesting the level?
    private float lastClickTime;

    private enum EditorMode { Move, Scale, Rotate };
    private EditorMode currentMode;

    private void Start()
    {
        objectIndex = 0;
        interacting = false;
        playing = false;
        editorModes.onValueChanged.AddListener(delegate { ChangeMode(); });
        currentMode = EditorMode.Move;
        carLoader.GetComponent<CarLoader>().is2D = true;
        propertiesWindow.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.x));
        Vector3 mouseDelta = mousePos - prevMousePos;

        if (!playing)
        {
            if (!interacting && Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // try raycasting to see if mouse is clicked over a level object
                if (Physics.Raycast(ray, out hit, 100))
                {
                    currObject = FindRootParent(hit.transform.gameObject);
                    //HighlightObject(true);

                    clickPoint = new Vector3(0, mousePos.y, mousePos.z);
                    moveOffset = clickPoint - currObject.transform.position;
                    interacting = true;

                    propertiesWindow.SetActive(true);
                    propertiesWindow.GetComponent<PropertiesWindow>().SetObject(currObject);
                }
                // no level object or UI element is clicked on
                else if (!EventSystem.current.IsPointerOverGameObject())
                {
                    // set current object to null and hide the property window
                    //if (currObject) HighlightObject(false);
                    currObject = null;
                    propertiesWindow.SetActive(false);

                    // if double click into the void, create new object
                    if (Time.time - lastClickTime < 0.2f) CreateObject(objectList[objectIndex], mousePos);
                    lastClickTime = Time.time;
                }
            }

            if (Input.GetMouseButtonUp(0) && currObject && interacting) interacting = false;

            // if clicking and holding on object: move, scale or rotate based on current mode
            if (currObject && interacting)
            {
                if (Input.GetMouseButton(0))
                {
                    switch (currentMode)
                    {
                        // move object
                        case EditorMode.Move:
                            currObject.transform.SetPositionAndRotation(mousePos - moveOffset, currObject.transform.rotation);
                            break;

                        // scale object
                        case EditorMode.Scale:
                            // set scale changed based on click position
                            Vector3 scaleRot = currObject.transform.rotation.eulerAngles;
                            scaleRot.x *= -1;
                            Vector3 scaleChange = Quaternion.Euler(scaleRot) * mouseDelta;
                            Vector3 rotatedOffset = Quaternion.Euler(scaleRot) * moveOffset;
                            if (rotatedOffset.y < 0) scaleChange.y *= -1;
                            if (rotatedOffset.z < 0) scaleChange.z *= -1;

                            // make sure scale cannot be too small
                            Vector3 oldScale = currObject.transform.localScale;
                            if (oldScale.y + scaleChange.y <= 1) scaleChange.y = 0;
                            if (oldScale.z + scaleChange.z <= 1) scaleChange.z = 0;

                            // set the new scale
                            currObject.transform.localScale += scaleChange;

                            // move object based on new scale
                            Vector3 posChange = currObject.transform.rotation * new Vector3(0, rotatedOffset.y < 0 ? -scaleChange.y / 2 : scaleChange.y / 2, rotatedOffset.z < 0 ? -scaleChange.z / 2 : scaleChange.z / 2);
                            currObject.transform.SetPositionAndRotation(currObject.transform.position + posChange, currObject.transform.rotation);

                            break;

                        // rotate object around its center point
                        case EditorMode.Rotate:
                            Quaternion rot = currObject.transform.rotation;
                            rot.x += mouseDelta.z / 10;
                            currObject.transform.SetPositionAndRotation(currObject.transform.position, rot);
                            break;
                    }

                }
            }

            // destroy current object with backspace
            if (Input.GetKeyDown(KeyCode.Backspace) && currObject != spawnPoint)
            {
                Destroy(currObject);
                propertiesWindow.SetActive(false);
            }

            // copy/paste current object
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C)) copiedObject = currObject;
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
            {
                GameObject pasted = CreateObject(copiedObject, mousePos);
                pasted.transform.rotation = copiedObject.transform.rotation;
            }

            // change to-be-added object via scrolling (will change later)
            objectIndex = (int)Mathf.Clamp(objectIndex + Input.mouseScrollDelta.y, 0, objectList.Count - 1);
            addText.text = "Add object: " + objectList[objectIndex].name;

            prevMousePos = mousePos;
        }
    }

    private void ChangeMode()
    {
        currentMode = (EditorMode)editorModes.value;
    }

    private GameObject CreateObject(GameObject origObject, Vector3 spawnPos)
    {
        Debug.Log("Creating " + objectList[objectIndex].name);
        GameObject newObject = Instantiate(origObject, spawnPos, Quaternion.identity);
        newObject.name = origObject.name;
        newObject.transform.parent = level.transform;

        return newObject;
    }

    private void HighlightObject(bool highlight)
    {
        Color objColor = currObject.GetComponent<MeshRenderer>().material.color;
        //if (highlight) objColor = new Color(objColor.r, objColor.g, objColor.b, 1.0f);
        //else objColor = new Color(objColor.r, objColor.g, objColor.b, 0.0f);
        currObject.GetComponent<MeshRenderer>().material.SetColor("_Color", objColor);
    }

    public void Play()
    {
        // if not playing, hide all editor UI and spawn a car loader
        if (!playing)
        {
            spawnedCarLoader = Instantiate(carLoader, spawnPoint.transform.position, Quaternion.identity);
            //Instantiate(carLoader, spawnPoint.transform);

            spawnPoint.SetActive(false);
            editorCamera.SetActive(false);
            editorModes.gameObject.SetActive(false);
            addText.gameObject.SetActive(false);
            propertiesWindow.SetActive(false);

            playButton.text = "Back";
            playing = true;
        }
        // if playing, destroy car loader with player and camera, then bring back all UI
        else
        {
            Destroy(spawnedCarLoader);

            spawnPoint.SetActive(true);
            editorCamera.SetActive(true);
            editorModes.gameObject.SetActive(true);
            addText.gameObject.SetActive(true);
            
            currObject = null;
            playButton.text = "Play";
            playing = false;
        }
    }

    // find root parent of object (useful when clicking on child object accidentaly)
    private GameObject FindRootParent(GameObject objToCheck)
    {
        while (objToCheck.transform.parent.name != "Level")
        {
            objToCheck = objToCheck.transform.parent.gameObject;
        }

        return objToCheck;
    }
}
