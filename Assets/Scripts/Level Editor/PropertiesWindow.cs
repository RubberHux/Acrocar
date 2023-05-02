using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PropertiesWindow : MonoBehaviour
{
    public TMP_Dropdown objectProperty; // properties for an object (normal, grapple, lava, ...)
    public TMP_InputField rotation; // rotation of object (in euler angle)
    public TMP_InputField width; // width of object

    public Material defaultMaterial;
    public Material grappleMaterial;
    public Material lavaMaterial;

    public UnityEvent propertyEvent;

    private enum Property { Normal, Grapple, Lava };
    private GameObject currObject; // current object to show properties for
    private bool changingRotation;
    private bool changingWidth;

    // Start is called before the first frame update
    void Start()
    {
        objectProperty.onValueChanged.AddListener(delegate { SetProperty(); });
        changingRotation = false;
        changingWidth = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!changingRotation) rotation.text = currObject.transform.rotation.eulerAngles.x.ToString();
        if (!changingWidth) width.text = currObject.transform.localScale.z.ToString();
    }

    public void SetObject(GameObject setObject)
    {
        currObject = setObject;

        Property setProperty;
        if (currObject.CompareTag("GrappleSurface")) setProperty = Property.Grapple;
        else if (currObject.CompareTag("Lava")) setProperty = Property.Lava;
        else setProperty = Property.Normal;

        objectProperty.SetValueWithoutNotify((int)setProperty);
        rotation.text = currObject.transform.rotation.eulerAngles.x.ToString();
    }

    public void ToggleRotationSet()
    {
        changingRotation = !changingRotation;
    }

    public void SetRotation()
    {
        Vector3 newRotation = currObject.transform.rotation.eulerAngles;
        newRotation.x = int.Parse(rotation.text);
        currObject.transform.SetPositionAndRotation(currObject.transform.position, Quaternion.Euler(newRotation));
    }

    public void ToggleWidthSet()
    {
        changingWidth = !changingWidth;
    }

    public void SetWidth()
    {
        Vector3 newScale = currObject.transform.localScale;
        newScale.z = int.Parse(width.text);
        Vector3 scaleChange = newScale - currObject.transform.localScale;
        currObject.transform.localScale = newScale;

        // move object based on new scale
        Vector3 posChange = currObject.transform.rotation * new Vector3(0, scaleChange.y / 2, scaleChange.z / 2);
        currObject.transform.SetPositionAndRotation(currObject.transform.position + posChange, currObject.transform.rotation);
    }

    private void SetProperty()
    {
        switch ((Property)objectProperty.value)
        {
            case Property.Normal:
                currObject.tag = "Block";
                currObject.layer = 0;
                currObject.GetComponent<BoxCollider>().isTrigger = false;
                currObject.GetComponent<MeshRenderer>().material = defaultMaterial;

                Destroy(currObject.GetComponent<DeathMaterialScript>());
                break;

            case Property.Grapple:
                currObject.tag = "GrappleSurface";
                currObject.layer = 3;
                currObject.GetComponent<BoxCollider>().isTrigger = false;
                currObject.GetComponent<MeshRenderer>().material = grappleMaterial;

                Destroy(currObject.GetComponent<DeathMaterialScript>());
                break;

            case Property.Lava:
                currObject.tag = "Lava";
                currObject.layer = 0;
                currObject.GetComponent<BoxCollider>().isTrigger = true;
                currObject.GetComponent<MeshRenderer>().material = lavaMaterial;

                currObject.AddComponent<DeathMaterialScript>();
                break;
        }
    }
}
