using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DynamicScroll
{
    public abstract class DynamicScrollViewBase : ScrollRect
    {
        public const float GAP = 1.0F;
        public const float FREE_SPACE_RATIO = 0.5F;
        public abstract float GetLength(int index);
        public abstract bool IsExist(int index);

        public delegate DynamicScrollItemBase MakeItemFunc(int index);

        public delegate void ItemSizeChanged();

        #region CachedTransform

        public RectTransform RtfmCached
        {
            get
            {
                if (__rtfmCached.IsNotNull())
                {
                    return __rtfmCached;
                }

                __rtfmCached = GetComponent<RectTransform>();
                return __rtfmCached;
            }
        }

        private RectTransform __rtfmCached;

        public Transform TfmCached
        {
            get
            {
                if (__tfmCached.IsNotNull())
                {
                    return __tfmCached;
                }

                __tfmCached = transform;
                return __tfmCached;
            }
        }

        private Transform __tfmCached;

        public GameObject ObjCached
        {
            get
            {
                if (__objCached.IsNotNull())
                {
                    return __objCached;
                }

                __objCached = gameObject;
                return __objCached;
            }
        }

        private GameObject __objCached;

        #endregion

        public float spacing = 10F;
        private float freeSpace = 0;
        private List<string> debugList = new List<string>();
        
        private float Top
        {
            get { return 0; }
        }

        private float Bottom
        {
            get { return 0; }
        }

        private int GetScrollIndex(float scrollHeight)
        {
            
            
            return 0;
        }

        private float ScrollPosition
        {
            get { return 0; }
        }

        private float Height
        {
            get { return 0; }
        }


        private LinkedList<DynamicScrollItemBase> scrollItemLinkedList = new LinkedList<DynamicScrollItemBase>();
        private Queue<DynamicScrollItemBase> scrollItemQueue = new Queue<DynamicScrollItemBase>();
        private List<float> itemSizeList = new List<float>();
        private List<float> itemSizeSumList = new List<float>();

        private bool isInitialized = false;

        private RectTransform rtfmTopPivot;
        private RectTransform rtfmBottomPivot;

        private MakeItemFunc savedMakeItemFunction = null;

        public virtual void Show(MakeItemFunc makeItemFunc , int dstIndex = 0)
        {
            itemSizeList.Clear();
            itemSizeSumList.Clear();
            
            savedMakeItemFunction = makeItemFunc;
            Initialize();
            //UpdateScroll();
        }

        public void Hide()
        {
            int queueCount = scrollItemQueue.Count;
            for (int i = 0; i < queueCount; ++i)
            {
                var item = scrollItemQueue.Dequeue();
                item.Reset();
                scrollItemQueue.Enqueue(item);
            }
        }

        public void ScrollTo(int srcIndex, int dstIndex, bool animate, bool centerOnChild)
        {
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
        }

        void Initialize()
        {
            AddDebugToUI("Initialize");
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;

            // initialize

            this.UpdateScroll();
            SetContentPosition(0);
            this.UpdateScroll();

            this.UpdateAsObservable().Subscribe(unit =>
            {
                if (true || Input.GetKeyDown(KeyCode.U))
                {
                    UpdateScroll();
                }
            });
/*            
            this.OnValueChangedAsObservable().Subscribe(offset =>
            {
                UpdateScroll();
                AddDebugUI("OnValuChanged.ScrollOffset:" + offset.ToString());
            });
*/            
        }

        List<float> CalculateSum()
        {
            float length = 0;
            List<float> sumList = new List<float>() {0};

            for (int i = 0; ; ++i)
            {
                if (IsExist(i) == false)
                {
                    break;
                }
                
                length = GetLength(i);
                if (sumList.Count < 1)
                {
                    sumList.Add(length + spacing);
                }
                else
                {
                    sumList.Add(sumList[sumList.Count-1] + length + spacing);
                }
            }

            return sumList;
        }

        int CalculateScrollIndex(List<float> sumList, float scrollPosition)
        {
            if (sumList.Count < 1)
            {
                AddDebugToUI("List.Count : 0");
                return 0;
            }

            if (scrollPosition >= sumList[sumList.Count-2])
            {
                AddDebugToUI("calculate.count:" + (sumList.Count-2));
                return sumList.Count - 2;
            }

            for (int i = sumList.Count-2; i > -1 ; --i)
            {
                if (scrollPosition > sumList[i])
                {
                    AddDebugToUI("calculate.count:" + i);
                    return i;
                }
            }

            return 0;
        }

        string MakeSumString(List<float> sumList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < sumList.Count; ++i)
            {
                sb.Append(sumList[i]);
                if (i < sumList.Count - 1)
                {
                    sb.Append(",");
                }
            }

            return sb.ToString();
        }

        void CalculatePossibleIndex(List<float> sumList, float endPos, int start, out int end) {
            end = start;

            for (int i = start; i < sumList.Count - 1; ++i) {
                AddDebugToUI("CalculatePossibleIndex." + i);
                if (sumList[i] > endPos) {
                    AddDebugToUI("CalculatePossibleIndexEnd." + i + ", " + sumList[i] + "," + endPos);
                    // 위쪽을 기준으로 했으니 한 칸 더 내려가서 리턴
                    if (i < sumList.Count - 2) {
                        end = i + 1;
                    }
                    else {
                        end = i;
                    }
                    return;
                }
            }

            end = sumList.Count - 2;
        }

        void SetContentHeight(float height)
        {
            Vector2 size = content.sizeDelta;
            size.y = height;
            content.sizeDelta = size;
        }

        void SetContentPosition(float y)
        {
            content.anchoredPosition = new Vector2(0,y);
        }
        
        LinkedListNode<DynamicScrollItemBase> DequeueTopItem(List<float> sumList, int scrollIndex)
        {
            var item = Dequeue(sumList,scrollIndex);
            scrollItemLinkedList.AddFirst(item);
            
            return scrollItemLinkedList.First;
        }

        void SetPosition(List<float> sumList, DynamicScrollItemBase item, int scrollIndex)
        {
            item.RtfmCached.anchoredPosition = new Vector2(0, -sumList[scrollIndex]);
        }

        bool ShouldRefreshAll(int startIndex, int endIndex)
        {
            if (scrollItemLinkedList.Count < 1)
            {
                return true;
            }

            var first = scrollItemLinkedList.First.Value;
            var last = scrollItemLinkedList.Last.Value;

            if (first.Index > startIndex - 1 && first.Index < endIndex + 1)
            {
                return false;
            }

            if (last.Index > startIndex - 1 && last.Index < endIndex + 1)
            {
                return false;
            }

            return true;
        }

        void ValidateAndAddItem(List<float> sumList, int startIndex, int endIndex)
        {
            RemoveUnusedOrCreateAtLeastOne(sumList,startIndex,endIndex);
            Fill(sumList,startIndex,endIndex);
        }

        void Fill(List<float> sumList, int startIndex, int endIndex)
        {
            for (int i = scrollItemLinkedList.First.Value.Index - 1; i >= startIndex; --i)
            {
                var item = Dequeue(sumList,i);
                scrollItemLinkedList.AddFirst(item);
                AddDebugToUI("Fill.Top:" + i);
            }

            for (int i = scrollItemLinkedList.Last.Value.Index + 1; i <= endIndex; ++i)
            {
                var item = Dequeue(sumList,i);
                scrollItemLinkedList.AddLast(item);
                AddDebugToUI("Fill.Bottom:" + i); 
            }
        }
        
        void RemoveUnusedOrCreateAtLeastOne(List<float> sumList, int startIndex, int endIndex) 
        {
            AddDebugToUI("RemoveOrCreate.Count:" +scrollItemLinkedList.Count);

            if (scrollItemLinkedList.Count > 0)
            {
                List<DynamicScrollItemBase> listToRemove = new List<DynamicScrollItemBase>();
                var node = scrollItemLinkedList.First;
                while (node != null)
                {
                    var itemIndex = node.Value.Index;
                    if (itemIndex >= startIndex && itemIndex <= endIndex)
                    {
                        node = node.Next;
                        continue;
                    }

                    Enqueue(node.Value);
                    listToRemove.Add(node.Value);
                    
                    node = node.Next;
                }

                for (int i = 0; i < listToRemove.Count; ++i)
                {
                    scrollItemLinkedList.Remove(listToRemove[i]);
                }
            }

            if (scrollItemLinkedList.Count < 1)
            {
                AddDebugToUI("RemoveOrCreate.Count==0. DequeueTopItem");
                DequeueTopItem(sumList, startIndex);
            }
        }
        
        void DequeueToTop(float startBottomPos)
        {
            float position = 0;
            var node = scrollItemLinkedList.Last;

            // 위로 검사.
            while ( node != null)
            {
                if (node.Value.RtfmCached.anchoredPosition.y < startBottomPos)
                {
                    Enqueue(node.Value);
                }

                node = node.Previous;
            }
            
        }
        
        void QueueIntoTop(float startBottomPos)
        {
            float position = 0;
            var node = scrollItemLinkedList.First;

            // 위로 추가.
            while (true)
            {
            }
        }

        void Enqueue(DynamicScrollItemBase item)
        {
            item.OnHide();
            scrollItemQueue.Enqueue(item);
        }
        
        DynamicScrollItemBase Dequeue(List<float> sumList, int index)
        {
            if (scrollItemQueue.Count < 1)
            {
                var created = savedMakeItemFunction.Invoke(index);
                created.Initialize(OnChangedItemSize,content,index);
                SetPosition(sumList, created, index);
                created.OnShow();
                return created;
            }

            var dequeue = scrollItemQueue.Dequeue();
            dequeue.UpdateScrollItemDequeued(index);
            SetPosition(sumList, dequeue, index);
            dequeue.OnShow();
            return dequeue;
        }

        void DequeueOrQueueIntoBottom(float destScrollPositionEnd)
        {
            
        }

        void CheckLinkSelf(float destScrollPositionStart, float destScrollPositionEnd)
        {
            
        }

        void EnqueueAll()
        {
            if (scrollItemLinkedList.Count < 1)
            {
                return;
            }

            var node = scrollItemLinkedList.First;
            while (node != null)
            {
                Enqueue(node.Value);
                node = node.Next;
            }
            scrollItemLinkedList.Clear();
        }

        void ClearDebugUI() {
            
            debugList.Clear();
        }

        void AddDebugToUI(string debugString) {
            
            debugList.Add(debugString);
            return;
            Debug.Log(debugString);
        }

        void OnGUI()
        {
            GUI.color=Color.black;
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = 20;
            
            for (int i = 0; i < debugList.Count; ++i)
            {
                GUILayout.BeginArea(new Rect(0,i * 50, 400, 50));
                GUILayout.Label(debugList[i], labelStyle);
                GUILayout.EndArea();
            }
        }
        
        void UpdateScroll()
        {
            ClearDebugUI();
            AddDebugToUI("UpdateScroll");
            
            float currentScrollHeight = viewport.rect.size.y;
            freeSpace = FREE_SPACE_RATIO * currentScrollHeight;

            List<float> sumList = CalculateSum();

            float currentScrollPosition = content.anchoredPosition.y;
            int startScrollIndex = CalculateScrollIndex(sumList, currentScrollPosition - freeSpace);

            float scrollPositionStart = sumList[startScrollIndex] - freeSpace;
            float scrollPositionEnd = sumList[startScrollIndex] + currentScrollHeight + freeSpace;

            SetContentHeight(sumList.LastOrDefault());

            AddDebugToUI("startPos:"+scrollPositionStart+",endPos:"+scrollPositionEnd);
            
            CalculatePossibleIndex(sumList, scrollPositionEnd, startScrollIndex, out int endScrollIndex);
  
            {
                AddDebugToUI("[Sum]:"+MakeSumString(sumList));
                AddDebugToUI("sumList.Count:" + sumList.Count);
                AddDebugToUI("startScrollIndex:" + startScrollIndex);
                AddDebugToUI("endScrollIndex:" + endScrollIndex);
                AddDebugToUI("scrollHeight : " + currentScrollHeight);
            }

            ValidateAndAddItem(sumList,startScrollIndex, endScrollIndex);
        }

        void CheckScrollIndex()
        {
            
        }

        void MakeItem()
        {
            while (DoesNeedMore())
            {
            }
        }

        void OnChangedScrollPosition()
        {
            //UpdateScroll();
        }

        void OnChangedItemSize()
        {
            //UpdateScroll();
        }

        bool DoesNeedMore()
        {



            return false;
        }


    }

    public static class ScrollViewExtension
    {
        public static T LastOrDefault<T>(this List<T> list)
        {
            if (list.Count < 1)
            {
                return default;
            }

            return list.Last();
        }

        public static T Last<T>(this List<T> list)
        {
            return list[list.Count - 1];
        }
        public static T GetOrAddComponent<T>(this DynamicScrollViewBase scrollView) where T:Component
        {
            var component = scrollView.ObjCached.GetComponent<T>();
            if (component == null)
            {
                component = scrollView.ObjCached.AddComponent<T>();
            }
            return component;
        }
    }
}