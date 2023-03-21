using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spill : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject mLiquid;
    //public GameObject mLiquidMesh;
    private int mSloshSpeed = 100;
    private int mRotateSpeed = 80;
    private int difference = 20;

    // Update is called once per frame
    void Update()
    {
        Slosh();
       // mLiquid.transform.Rotate(Vector3.up * mRotateSpeed * Time.deltaTime, Space.Self);
    }


    private void Slosh()
    {
        Quaternion inverseRotation = Quaternion.Inverse(transform.localRotation);

        Vector3 finalRotation = Quaternion.RotateTowards(mLiquid.transform.localRotation, inverseRotation, mSloshSpeed * Time.deltaTime).eulerAngles;

        finalRotation.z = ClampRotationValue(finalRotation.z, difference);
        finalRotation.x = ClampRotationValue(finalRotation.x, difference);

        mLiquid.transform.localEulerAngles = finalRotation;


    }


    private float ClampRotationValue(float value,float difference)
    {
        float returnValue = 0.0f;
        if (value > 180)
        {
            returnValue = Mathf.Clamp(value, 360 - difference, 360);
        }
        else
        {
            returnValue = Mathf.Clamp(value, 0, difference);
        }

        return returnValue;
    }
}
