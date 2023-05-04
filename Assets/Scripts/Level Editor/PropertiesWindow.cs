using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class PropertiesWindow : MonoBehaviour
{
    public Toggle moveable, breakable, grappleable, lava;
    public TMP_InputField rotation, width, height, xPos, yPos; // input fields for position, rotation & dimensions
    public TMP_Text objectName;
    public Button deleteButton;

    public Material defaultMaterial;
    public Material grappleMaterial;
    public Material lavaMaterial;

    public UnityEvent propertyEvent;

    private enum Property { Normal, Grapple, Lava, Moveable };
    private GameObject currObject; // current object to show properties for
    private bool valueChange; // currently changing value in input field?

    // Start is called before the first frame update
    void Start()
    {
        valueChange = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!valueChange)
        {
            rotation.text = Math.Round(currObject.transform.rotation.eulerAngles.x, 2).ToString();
            width.text = Math.Round(currObject.transform.localScale.z, 2).ToString();
            height.text = Math.Round(currObject.transform.localScale.y, 2).ToString();
            xPos.text = Math.Round(currObject.transform.position.z, 2).ToString();
            yPos.text = Math.Round(currObject.transform.position.y, 2).ToString();
        }
    }

    public void SetObject(GameObject setObject)
    {
        currObject = setObject;
        objectName.text = currObject.name;
        SetPropertiesToCurrent();
        rotation.text = Math.Round(currObject.transform.rotation.eulerAngles.x, 2).ToString();
        width.text = Math.Round(currObject.transform.localScale.z, 2).ToString();
    }

    private void SetPropertiesToCurrent()
    {
        rotation.gameObject.SetActive(true);
        width.gameObject.SetActive(true);
        height.gameObject.SetActive(true);
        deleteButton.gameObject.SetActive(true);

        if (currObject.CompareTag("Block"))
        {
            moveable.SetIsOnWithoutNotify(false);
            breakable.SetIsOnWithoutNotify(false);
            grappleable.SetIsOnWithoutNotify(false);
            lava.SetIsOnWithoutNotify(false);

            moveable.gameObject.SetActive(true);
            breakable.gameObject.SetActive(true);
            grappleable.gameObject.SetActive(true);
            lava.gameObject.SetActive(true);

            if (IsLava())
            {
                lava.SetIsOnWithoutNotify(true);
                grappleable.gameObject.SetActive(false);
                breakable.gameObject.SetActive(false);
            }
            else if (IsGrappleable())
            {
                grappleable.SetIsOnWithoutNotify(true);
                lava.gameObject.SetActive(false);
            }

            if (IsMoveable()) moveable.SetIsOnWithoutNotify(true);
            if (IsBreakable())
            {
                breakable.SetIsOnWithoutNotify(true);
                lava.gameObject.SetActive(false);
            }
        }
        else
        {
            moveable.gameObject.SetActive(false);
            breakable.gameObject.SetActive(false);
            grappleable.gameObject.SetActive(false);
            lava.gameObject.SetActive(false);
        }

        if (currObject.CompareTag("SpawnPoint"))
        {
            rotation.gameObject.SetActive(false);
            width.gameObject.SetActive(false);
            height.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
        }
    }

    public void ToggleValueChange()
    {
        valueChange = !valueChange;
    }

    public void SetRotation()
    {
        Vector3 newRotation = currObject.transform.rotation.eulerAngles;
        newRotation.x = int.Parse(rotation.text);
        currObject.transform.SetPositionAndRotation(currObject.transform.position, Quaternion.Euler(newRotation));
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

    public void SetHeight()
    {
        Vector3 newScale = currObject.transform.localScale;
        newScale.y = int.Parse(height.text);
        Vector3 scaleChange = newScale - currObject.transform.localScale;
        currObject.transform.localScale = newScale;

        // move object based on new scale
        Vector3 posChange = currObject.transform.rotation * new Vector3(0, scaleChange.y / 2, scaleChange.z / 2);
        currObject.transform.SetPositionAndRotation(currObject.transform.position + posChange, currObject.transform.rotation);
    }

    public void SetXPosition()
    {
        Vector3 newPos = currObject.transform.position;
        newPos.z = int.Parse(xPos.text);
        currObject.transform.SetPositionAndRotation(newPos, currObject.transform.rotation);
    }

    public void SetYPosition()
    {
        Vector3 newPos = currObject.transform.position;
        newPos.y = int.Parse(yPos.text);
        currObject.transform.SetPositionAndRotation(newPos, currObject.transform.rotation);

    }

    public void ToggleGrappleable()
    {
        // if not grappleable, make it so
        if (!IsGrappleable())
        {
            currObject.layer = 3;
            //currObject.GetComponent<BoxCollider>().isTrigger = false;
            currObject.GetComponent<MeshRenderer>().material = grappleMaterial;

            lava.gameObject.SetActive(false);
        }
        else
        {
            currObject.layer = 0;
            currObject.GetComponent<MeshRenderer>().material = defaultMaterial;

            if (!IsBreakable()) lava.gameObject.SetActive(true);
        }
    }

    public void ToggleLava()
    {
        if (!IsLava())
        {
            grappleable.gameObject.SetActive(false);
            breakable.gameObject.SetActive(false);

            currObject.GetComponent<MeshRenderer>().material = lavaMaterial;
            currObject.GetComponent<BoxCollider>().isTrigger = true;
            currObject.AddComponent<DeathMaterialScript>();
        }
        else
        {
            grappleable.gameObject.SetActive(true);
            breakable.gameObject.SetActive(true);

            currObject.GetComponent<MeshRenderer>().material = defaultMaterial;
            currObject.GetComponent<BoxCollider>().isTrigger = false;
            Destroy(currObject.GetComponent<DeathMaterialScript>());
        }
    }

    public void ToggleMoveable()
    {
        if (!IsMoveable())
        {
            currObject.AddComponent<Rigidbody>();
            currObject.GetComponent<Rigidbody>().mass = 500;
            currObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            currObject.GetComponent<Rigidbody>().useGravity = true;
            currObject.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        }
        else
        {
            Destroy(currObject.GetComponent<Rigidbody>());
        }
    }

    public void ToggleBreakable()
    {
        if (!IsBreakable())
        {
            lava.gameObject.SetActive(false);

            currObject.AddComponent<Destructible>();
            currObject.GetComponent<Destructible>().breakForce = 10000;
        }
        else
        {
            if (!IsGrappleable()) lava.gameObject.SetActive(true);

            Destroy(currObject.GetComponent<Destructible>());
        }
    }

    private bool IsGrappleable()
    {
        return currObject.layer == 3;
    }

    private bool IsLava()
    {
        return currObject.TryGetComponent(out DeathMaterialScript deathscript);
    }

    private bool IsMoveable()
    {
        return currObject.TryGetComponent(out Rigidbody rigidbody);
    }

    private bool IsBreakable()
    {
        return currObject.TryGetComponent(out Destructible destructible);
    }
}
