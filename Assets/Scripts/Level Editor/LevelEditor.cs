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
    public GameObject objectList; // list of objects which can be created
    public Material normalMaterial; // default material to slap on objects
    public Material grappleMaterial; // material for grappleable stuff
    public Material lavaMaterial; // material for lava
    public Material breakableMaterial; // material for breakable stuff
    public GameObject editorModes; // modes for the editor
    public GameObject propertiesWindow; // window for object properties
    public TMP_Text playButton; // button for toggling play/edit mode

    private GameObject currObject; // current object selected
    private GameObject objectToAdd; // the type of object added when double clicking
    private GameObject copiedObject; // object currently copied
    private GameObject spawnedCarLoader; // actual car loader spawned when playtesting
    private GameObject levelBackup;
    private bool interacting; // is current object being interacted with?
    private Vector3 clickPoint; // point on object where it was clicked
    private Vector3 mousePos; // current mouse position in the world
    private Vector3 prevMousePos; // previous mouse pos (used for delta calcs)
    private Vector3 moveOffset; // offset between click point and object pos
    private bool playing; // playtesting the level?
    private float lastClickTime;

    private enum EditorMode { Move, Scale, Rotate };
    private EditorMode currentMode;

    private void Start()
    {
        interacting = false;
        playing = false;
        propertiesWindow.SetActive(false);

        currentMode = EditorMode.Move;
        carLoader.GetComponent<CarLoader>().is2D = true;
        objectToAdd = null;
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
                if (Physics.Raycast(ray, out hit, 100) && !MouseOverUIElement())
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
                else if (!MouseOverUIElement())
                {
                    // set current object to null and hide the property window
                    //if (currObject) HighlightObject(false);
                    currObject = null;
                    propertiesWindow.SetActive(false);

                    // if double click into the void, create new object
                    if (Time.time - lastClickTime < 0.2f) CreateObject(objectToAdd, mousePos);
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
                            if (!currObject.CompareTag("SpawnPoint"))
                            {
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
                            }

                            break;

                        // rotate object around its center point
                        case EditorMode.Rotate:
                            if (!currObject.CompareTag("SpawnPoint"))
                            {
                                Quaternion rot = currObject.transform.rotation;
                                rot.x += mouseDelta.z / 10;
                                currObject.transform.SetPositionAndRotation(currObject.transform.position, rot);
                            }
                            break;
                    }

                }
            }

            // copy/paste current object
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C)) copiedObject = currObject;
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
            {
                GameObject pasted = CreateObject(copiedObject, mousePos);
                pasted.transform.rotation = copiedObject.transform.rotation;
            }

            prevMousePos = mousePos;
        }
    }

    private bool MouseOverUIElement()
    {
        PointerEventData currMousePos = new PointerEventData(EventSystem.current);
        currMousePos.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(currMousePos, results);
        return results.Count > 0;
    }

    public void DeleteObject()
    {
        Destroy(currObject);
        propertiesWindow.SetActive(false);
    }

    public void SetMode(int mode)
    {
        currentMode = (EditorMode)mode;
    }

    public void SetObjectToAdd(GameObject obj)
    {
        objectToAdd = obj;
    }

    private GameObject CreateObject(GameObject origObject, Vector3 spawnPos)
    {
        GameObject newObject = null;
        if (origObject != null)
        {
            newObject = Instantiate(origObject, spawnPos, Quaternion.identity);
            newObject.name = origObject.name;
            newObject.transform.parent = level.transform;
        } 

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
        UIController uiController = GameObject.FindGameObjectWithTag("UI").GetComponent<UIController>();

        // if not playing, hide all editor UI and spawn a car loader
        if (!playing)
        {
            SaveLevel();

            spawnedCarLoader = Instantiate(carLoader, spawnPoint.transform.position, Quaternion.identity);
            //Instantiate(carLoader, spawnPoint.transform);

            spawnPoint.SetActive(false);
            editorCamera.SetActive(false);
            editorModes.gameObject.SetActive(false);
            propertiesWindow.SetActive(false);
            objectList.SetActive(false);

            playButton.text = "Back";
            playing = true;
            uiController.SetState(UIController.GameState.Playing);

            ToggleAllMoveable();
        }
        // if playing, destroy car loader with player and camera, then bring back all UI
        else
        {
            LoadLevel();

            Destroy(spawnedCarLoader);
            Destroy(uiController.winInstance);

            spawnPoint.SetActive(true);
            editorCamera.SetActive(true);
            editorModes.gameObject.SetActive(true);
            objectList.SetActive(true);

            Time.timeScale = 1;
            uiController.SetState(UIController.GameState.LevelEditor);

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

    private void ToggleAllMoveable()
    {
        foreach (Transform t in level.transform)
        {
            if (t.TryGetComponent(out Rigidbody rigidbody)) rigidbody.isKinematic = false;
        }
    }

    public void SetMaterial(Material mat)
    {
        if (currObject.CompareTag("Block")) currObject.GetComponent<MeshRenderer>().material = mat;
    }

    public void SaveLevel()
    {
        levelBackup = Instantiate(level);
        levelBackup.SetActive(false);
        //string savedLevel = JsonUtility.ToJson(level);
        //Debug.Log(savedLevel);
    }

    public void LoadLevel()
    {
        levelBackup.SetActive(true);
        Destroy(level);
        level = levelBackup;
        level.name = "Level";
        spawnPoint = GameObject.Find("Level/SpawnPoint");
    }
}
