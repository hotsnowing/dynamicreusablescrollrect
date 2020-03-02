using UniRx;
using UnityEngine;

namespace DynamicScroll
{
    public abstract class DynamicScrollItemBase : MonoBehaviour
    {
        #region CachedTransform 
        public RectTransform RtfmCached {
            get {
                if (__rtfmCached.IsNotNull()) {
                    return __rtfmCached;
                }

                __rtfmCached = GetComponent<RectTransform>();
                return __rtfmCached;
            }
        }

        private RectTransform __rtfmCached;

        public Transform TfmCached {
            get {
                if (__tfmCached.IsNotNull()) {
                    return __tfmCached;
                }

                __tfmCached = transform;
                return __tfmCached;
            }
        }

        private Transform __tfmCached;

        public GameObject ObjCached {
            get {
                if (__objCached.IsNotNull()) {
                    return __objCached;
                }

                __objCached = gameObject;
                return __objCached;
            }
        }

        private GameObject __objCached;
        #endregion

        DynamicScrollViewBase.ItemSizeChanged savedCallback;
        public int Index { get; set; }

        protected abstract void UpdateScrollItem(int index);

        public virtual void OnShow()
        {
            ObjCached.SetActive(true);
        }
        
        public virtual void OnHide()
        {
            ObjCached.SetActive(false);
        }

        public void Initialize(DynamicScrollViewBase.ItemSizeChanged callback, Transform tfmParent, int index)
        {
            RtfmCached.SetParent(tfmParent);
            RtfmCached.anchorMin = new Vector2(0,1);
            RtfmCached.anchorMax = new Vector2(0,1);
            RtfmCached.pivot = new Vector2(0, 1);
            savedCallback = callback;
            
            UpdateScrollItemDequeued(index);
        }

        public void Reset()
        {
            savedCallback = null;
            OnHide();
        }

        public void UpdateScrollItemDequeued(int index)
        {
            Index = index;
            UpdateScrollItem(index);
        }
    
        public void MarkAsChanged()
        {
            if (savedCallback != null)
            {
                savedCallback.Invoke();
            }
        }
    }    

}

