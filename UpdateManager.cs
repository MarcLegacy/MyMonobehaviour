using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.Profiling;

namespace Core.MyMonoBehaviour
{
    public partial class UpdateManager : MonoBehaviour
    {
        #region Properties
        public static UpdateManager Instance {
            get
            {
                if (_instance != null) return _instance;
                
                var go = new GameObject(nameof(UpdateManager));
                _instance = go.AddComponent<UpdateManager>();
                DontDestroyOnLoad(go);

                return _instance;
            }
            private set => _instance = value;
        }
    
        public static bool HasInstance => _instance != null;
        #endregion
    
        #region Private Members
        [AutoStaticsCleanup]
        private static UpdateManager _instance;
        
        private readonly UpdateGroup<IMyUpdate> _updates = new(nameof(Update));
        private readonly UpdateGroup<IMyLateUpdate> _lateUpdates = new(nameof(LateUpdate));
        private readonly UpdateGroup<IMyFixedUpdate> _fixedUpdates = new(nameof(FixedUpdate));

        #endregion
    
        #region Methods
        #region Unity Methods
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    
        private void Update() => _updates.Process(invoke => invoke.MyUpdate());

        private void FixedUpdate() => _fixedUpdates.Process(invoke => invoke.MyFixedUpdate());

        private void LateUpdate() => _lateUpdates.Process(invoke => invoke.MyLateUpdate());
    
        private void OnDestroy()
        {
            _updates.Clear();
            _lateUpdates.Clear();
            _fixedUpdates.Clear();
        }
        #endregion
    
        #region Public Methods
        public void Register(IMyUpdate myUpdate) => _updates.QueueAdd(myUpdate);
        public void Register(IMyLateUpdate myLateUpdate) => _lateUpdates.QueueAdd(myLateUpdate);
        public void Register(IMyFixedUpdate myFixedUpdate) => _fixedUpdates.QueueAdd(myFixedUpdate);
        
        public void Unregister(IMyUpdate myUpdate) => _updates.QueueRemove(myUpdate);
        public void Unregister(IMyLateUpdate myLateUpdate) => _lateUpdates.QueueRemove(myLateUpdate);
        public void Unregister(IMyFixedUpdate myFixedUpdate) => _fixedUpdates.QueueRemove(myFixedUpdate);
        #endregion
        #endregion
    }
    
    internal sealed class UpdateGroup<T> 
    { 
        #region Properties
        private readonly FastRemoveUniqueList<T> _items = new(); 
        private readonly UniqueList<T> _toAdd = new(); 
        private readonly UniqueList<T> _toRemove = new(); 
        private readonly string _sampleSuffix;
        #endregion

        #region Methods
        #region Constructors
        public UpdateGroup(string sampleSuffix)
        {
            _sampleSuffix = sampleSuffix;
        }
        #endregion

        #region Public Methods
        public bool Contains(T item) => _items.Contains(item);
        public void QueueAdd(T item)
        {
            if (item == null) return;
            
            _toAdd.Add(item);
        }

        public void QueueRemove(T item)
        {
            if (item == null) return;
            
            _toRemove.Add(item);
        }

        public void Process(System.Action<T> invoke)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
#if ENABLE_PROFILER 
                Profiler.BeginSample($"{item.GetType().Name}.{_sampleSuffix}"); 
#endif 
                invoke(item); 
#if ENABLE_PROFILER 
                Profiler.EndSample(); 
#endif 
            }
            
            for (var i = 0; i < _toAdd.Count; i++)
            {
                _items.Add(_toAdd[i]);
            }

            for (var i = 0; i < _toRemove.Count; i++)
            {
                _items.Remove(_toRemove[i]);
            }

            _toAdd.Clear();
            _toRemove.Clear();
        }

        public void Clear()
        {
            _items.Clear();
            _toAdd.Clear();
            _toRemove.Clear();
        }
        #endregion
        #endregion
    }
}


