using System.Collections.Generic;
using UnityEngine;

namespace Elementum.Project.Pool
{
    public sealed class ObjectPool : MonoBehaviour
    {
        [Header("Pool Setup")]
        [SerializeField, Tooltip("Префаб, который будет переиспользоваться.")]
        private GameObject _prefab;

        [SerializeField, Min(1), Tooltip("Сколько объектов создать на старте.")]
        private int _initialSize = 8;

        [SerializeField, Min(1), Tooltip("Жесткий предел количества инстансов.")]
        private int _maxSize = 32;

        [SerializeField, Tooltip("Проверять дубли при возврате в пул.")]
        private bool _collectionCheck = true;

        private readonly Queue<GameObject> _available = new Queue<GameObject>();
        private int _createdCount;

        public int AvailableCount => _available.Count;
        public int CreatedCount => _createdCount;

        private void Awake()
        {
            if (_prefab == null)
            {
                Debug.LogError("[ObjectPool] Prefab is not assigned.", this);
                return;
            }

            int prewarmCount = Mathf.Clamp(_initialSize, 0, _maxSize);
            for (int i = 0; i < prewarmCount; i++)
            {
                GameObject instance = CreateInstance();
                Return(instance);
            }
        }

        public GameObject Get(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject instance;
            if (_available.Count > 0)
            {
                instance = _available.Dequeue();
            }
            else if (_createdCount < _maxSize)
            {
                instance = CreateInstance();
            }
            else
            {
                Debug.LogWarning("[ObjectPool] Max pool size reached.", this);
                return null;
            }

            Transform cachedTransform = instance.transform;
            cachedTransform.SetParent(parent, false);
            cachedTransform.SetPositionAndRotation(position, rotation);

            instance.SetActive(true);
            NotifyTaken(instance);
            return instance;
        }

        public T Get<T>(Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component
        {
            GameObject instance = Get(position, rotation, parent);
            if (instance == null)
            {
                return null;
            }

            if (!instance.TryGetComponent(out T component))
            {
                Debug.LogError($"[ObjectPool] Instance does not contain component: {typeof(T).Name}", this);
                Return(instance);
                return null;
            }

            return component;
        }

        public void Return(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (_collectionCheck && _available.Contains(instance))
            {
                Debug.LogWarning("[ObjectPool] Attempt to return the same instance twice.", this);
                return;
            }

            NotifyReturned(instance);
            instance.SetActive(false);
            instance.transform.SetParent(transform, false);
            _available.Enqueue(instance);
        }

        private GameObject CreateInstance()
        {
            GameObject instance = Instantiate(_prefab, transform);
            _createdCount += 1;
            instance.SetActive(false);
            return instance;
        }

        private static void NotifyTaken(GameObject instance)
        {
            if (instance.TryGetComponent(out IPoolable poolable))
            {
                poolable.OnTakenFromPool();
            }
        }

        private static void NotifyReturned(GameObject instance)
        {
            if (instance.TryGetComponent(out IPoolable poolable))
            {
                poolable.OnReturnedToPool();
            }
        }
    }
}
