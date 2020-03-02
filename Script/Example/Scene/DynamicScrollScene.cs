using System.Collections.Generic;
using UnityEngine;
using DynamicScroll;

public class DynamicScrollScene : MonoBehaviour
{
    public DynamicScrollItemBase protoType;
    public DynamicScrollViewExample scrollView;

    public static List<ScrollData> itemList = new List<ScrollData>();

    void Start()
    {
        Show();
    }

    [ContextMenu("Show")]
    void Show()
    {
        itemList.Clear();
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 300});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 400});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        itemList.Add(new ScrollData(){length = 200});
        
        scrollView.Show(index =>
        {
            Debug.Log("created.index:"+index);
            return Instantiate(protoType);
        });
    }

    [ContextMenu("Hide")]
    void Hide()
    {
        scrollView.Hide();
    }
    
}

public class ScrollData
{
    public float length;
}