using UnityEngine;
using UIPlugs.ScrollCircleMaker;

public class CURDItem : MonoBehaviour
{
    public GameObject itemContent;
    private CURDMaker baseMaker;
    private System.Random random = new System.Random();
    // Start is called before the first frame update
    void Start()
    {
        baseMaker = itemContent.GetComponent<ScrollCircleComponent>().baseMaker as CURDMaker;
    }

    public void AddItem(int idx)
    {
        baseMaker.Helper.AddItem(random.Next(0, 1000),idx);
    }

    public void DelItem(int idx)
    {
        baseMaker.Helper.DelItem(idx);
    }

    public void UpdateItem(int idx)
    {
        baseMaker.Helper.UpdateItem(random.Next(0,10000),idx);
    }

    public void SwapItem(int nextIdx)
    {
        baseMaker.Helper.SwapItem(0,nextIdx);
    }
}
