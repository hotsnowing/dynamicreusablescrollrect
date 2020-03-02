namespace DynamicScroll
{
    public class DynamicScrollViewExample : DynamicScrollViewBase
    {
        public override float GetLength(int index)
        {
            return DynamicScrollScene.itemList[index].length;
        }

        public override bool IsExist(int index)
        {
            return index < DynamicScrollScene.itemList.Count;
        }
    }
}