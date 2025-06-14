using System;
using UnityEngine;
using System.Collections.Generic;

namespace _Main
{
    [CreateAssetMenu(fileName = "ContextHolder", menuName = "SnL/ContextHolder", order = 0)]
    public sealed class ContextHolderSo : ScriptableObject
    {
        [SerializeField] private Context[] contexts;
        [SerializeField] private int cacheLimit;
        
        private readonly Dictionary<int, Context> cachedContexts = new();

        public Context GetPreviousContext(Context currentContext, bool autoCache = true)
        {
            var index = currentContext.StateIndex;
            if (autoCache)
            {
                AttemptToCache(currentContext, index);
            }
            return index == 0 ? null : GetInstance(index - 1);
        }
        
        public Context GetNextContext(Context currentContext, bool autoCache = true)
        {
            var index = currentContext.StateIndex;
            if (autoCache)
            {
                AttemptToCache(currentContext, index);
            }
            return index == contexts.Length - 1 ? null : GetInstance(index + 1);
        }

        public Context GetDefaultContext()
        {
            return contexts.Length == 0 ? null : GetInstance(0);
        }

        private Context GetInstance(int index)
        {
            if (cachedContexts.TryGetValue(index, out var instance))
            {
                instance.gameObject.SetActive(true);
                return instance;
            }
            
            var context = Instantiate(contexts[index]);
            context.StateIndex = index;
            cachedContexts.Add(index, context);
            return context;
        }

        private void AttemptToCache(Context context, int contextIndex)
        {
            if (cachedContexts.Count < cacheLimit)
            {
                Debug.Log($"Caching: {context}");
                context.gameObject.SetActive(false);
                return;
            }
            
            var isInCache = cachedContexts.ContainsKey(contextIndex);
            if (isInCache)
            {
                Debug.Log($"Uncaching: {context}");
                cachedContexts.Remove(contextIndex);
            }
            
            Destroy(context.gameObject);
        }

        public void Dispose()
        {
            foreach (var (_, instance) in cachedContexts)
            {
                if (instance)
                {
                    Destroy(instance.gameObject);
                }
            }
            cachedContexts.Clear();
        }
    }
}