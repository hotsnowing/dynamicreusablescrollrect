
using UnityEngine;
using UnityEngine.UI;

namespace DynamicScroll
{
    public class DynamicScrollItemExample : DynamicScrollItemBase
    {
        public RectTransform rtfmInnerBack;
        public Text lblNumber;

        protected override void UpdateScrollItem(int index)
        {
            ObjCached.name = $"ScrollItem{index}";
            lblNumber.text = index.ToString();
        }

        public void OnClickItem()
        {
        
        }
    }    
}
