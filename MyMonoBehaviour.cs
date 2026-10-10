using System;
using System.Reflection;
using Core.Pooling;
using UnityEngine;
using Utilities;
using Utilities.Attributes;
using Object = UnityEngine.Object;

namespace Core.MyMonoBehaviour
{
    public abstract class MyMonoBehaviour : MonoBehaviour, IMyMonoBehaviour
    {
        #region Unity Methods
        private void OnValidate()
        {
            if (!ValidateRequiredFields())
            {
                enabled = false;
            }

            MyOnValidate();
        }

        private void Awake()
        {
            MyAwake();
        }

        private void OnEnable()
        {
            var poolableObject = GetComponent<IPoolableObject>();

            if (poolableObject != null && !poolableObject.PoolingInitialized) return;

            if (this is IMyUpdate myUpdate)
            {
                UpdateManager.Instance.Register(myUpdate);
            }
            
            if (this is IMyLateUpdate myLateUpdate)
            {
                UpdateManager.Instance.Register(myLateUpdate);
            }
            
            if (this is IMyFixedUpdate myFixedUpdate)
            {
                UpdateManager.Instance.Register(myFixedUpdate);
            }

            MyOnEnable();
        }

        private void Start()
        {
            MyStart();
        }

        private void OnDisable()
        {
            var poolableObject = GetComponent<IPoolableObject>();

            if (poolableObject != null && !poolableObject.PoolingInitialized) return;
            
            if (UpdateManager.HasInstance)
            {
                if (this is IMyUpdate myUpdate)
                {
                    UpdateManager.Instance.Unregister(myUpdate);
                }
            
                if (this is IMyLateUpdate myLateUpdate)
                {
                    UpdateManager.Instance.Unregister(myLateUpdate);
                }
            
                if (this is IMyFixedUpdate myFixedUpdate)
                {
                    UpdateManager.Instance.Unregister(myFixedUpdate);
                }
            }
            
            MyOnDisable();
        }

        private void OnDestroy()
        {
            MyOnDestroy();
        }
        #endregion

        #region IMyMonoBehaviour Interface
        public virtual void MyOnValidate() {}
        public virtual void MyAwake() {}
        public virtual void MyOnEnable() {}
        public virtual void MyStart() {}
        public virtual void MyOnDisable() {}
        public virtual void MyOnDestroy() {}
        #endregion

        #region Private Methods
        private bool ValidateRequiredFields()
        {
            if (!enabled)
            {
                return false;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fields = GetType().GetFields(flags);

            foreach (var field in fields)
            {
                // Only consider serializable fields
                var isSerializable = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;

                if (!isSerializable)
                    continue;

                // Check for your attribute
                if (field.GetCustomAttribute<Required>() == null)
                    continue;

                var value = field.GetValue(this);

                switch (value)
                {
                    // Check if field is container
                    case Array array when array.Length == 0:
                        MyLogger.LogWarning(
                            $"<color=white>{field.Name}</color> is empty on <color=white>{gameObject.name}</color>",
                            this);
                        return false;
                    // Example: validate reference is not null
                    case null:
                    case Object uObj when uObj == null:
                        MyLogger.LogWarning(
                            $"<color=white>{field.Name}</color> is not assigned on <color=white>{gameObject.name}</color>",
                            this);
                        return false;
                }
            }

            return true;
        }
        #endregion
    }
}