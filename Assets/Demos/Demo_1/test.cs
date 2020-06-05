using System.Collections;
using System.Collections.Generic;
using UIPlugs.ScrollCircleMaker;
using UnityEngine;

public class test : MonoBehaviour
{
    public GameObject tmpGameObject;
    testCircleMaker testMaker;
    // Start is called before the first frame update
    void Start()
    {
       testMaker = (testCircleMaker)tmpGameObject.GetComponent<ScrollCircleComponent>().baseMaker;      
    }

    public void DelItem()
    {
        testMaker.Helper.DelItem(0);
    }

    public void AddItem()
    {    
        testMaker.Helper.AddItem(0);
    }
}
