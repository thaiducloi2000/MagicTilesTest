using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace com.team70
{
    public class Async : MonoBehaviour
    {
        internal class Locker
        {
        }
        
        public static Action onFrameUpdate;
        internal static Locker _locker;
        internal static List<AsyncCallback> _queue;
        internal static Dictionary<string, AsyncCallback> _idMap;

#if STANDALONE
    [RuntimeInitializeOnLoadMethod] 
    static void Initialize() 
    {
        CreateInstance();
    }
#endif

        void Awake()
        {
            if (_api != null && _api != this)
            {
                Debug.LogWarning("Multiple Async found!!!");
                Destroy(this);
                return;
            }

            _api = this;
            _isDestroyed = false;
            _isPlaying = true;
            // gameObject.hideFlags = HideFlags.HideAndDontSave;

            _locker = new Locker();
            _queue = new List<AsyncCallback>();
            _idMap = new Dictionary<string, AsyncCallback>();

            // DontDestroyOnLoad(_api);
        }

        void OnDestroy()
        {
            _isDestroyed = true;
        }

        void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(UpdateLoop());
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator UpdateLoop()
        {
            while (true)
            {
                realTime = Time.realtimeSinceStartup;
                gameTime = Time.time;
                if (onFrameUpdate != null) onFrameUpdate();
                
                if (_queue == null || _queue.Count == 0)
                {
                    yield return 0;
                    continue;
                }

                for (var i = _queue.Count - 1; i >= 0; i--)
                {
                    current = _queue[i];
                    if (current.time > realTime) continue;

                    if (current.isAlive)
                    {
                        if (current.isWaiting)
                        {
                            // if (delayCounter++ > 500) continue;
                            // delayCounter = 0;
                            //Debug.LogWarning("Waiting : " + current.id + " --> " + current.WaitFunc.Target + ":" + current.WaitFunc.Method.Name);
                            continue;
                        }

                        current.WaitFunc = null; //the wait is over !
                        current.repeatCount--;
                        current.time = (current.speedUp ? current.time : realTime) + current.repeatDelay;
                        try
                        {
                            if (current.callback != null) current.callback();
                        }
                        catch (Exception e)
                        {
                            DiscordLogger.HandleLog(e.Message, e.StackTrace, LogType.Exception);
                            // ignored
                        }
                    }
                    else
                    {
                        current.isDie = true;
                        lock (_locker)
                        {
                            if (!string.IsNullOrEmpty(current.id) && _idMap.ContainsKey(current.id))
                            {
                                _idMap.Remove(current.id);
                            }
                            _queue.RemoveAt(i);
                        }
                    }
                }

                current = null;
                yield return 0;
            }
        }

        // ----------------------------- INTERNAL ----------------------------	    

        internal void SetId(AsyncCallback cb, string newId)
        {
            if (cb.id == newId) return;
            if (!string.IsNullOrEmpty(cb.id))
            {
                lock (_locker)
                {
                    _idMap.Remove(cb.id);
                }
            }

            cb.id = newId;
            if (string.IsNullOrEmpty(newId)) return;

            lock (_locker)
            {
                if (_idMap.ContainsKey(newId))
                {
                    _idMap[newId] = cb;
                }
                else
                {
                    _idMap.Add(newId, cb);
                }
            }
        }

        AsyncCallback iCall(Action action, Func<bool> waitFunc, float delay, string id)
        {
            var result = new AsyncCallback
            {
                id = id,
                callback = action,
                WaitFunc = waitFunc,
                time = realTime + delay
            };

            lock (_locker)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    if (_idMap.ContainsKey(id))
                    {
                        _queue.Remove(_idMap[id]);
                        _idMap[id] = result;
                    }
                    else
                    {
                        _idMap.Add(id, result);
                    }
                }
                _queue.Add(result);
                //_needSort = true;
            }

            // Update values
            result.WaitFunc = waitFunc;
            return result;
        }

        internal void iKill(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("Kill Error - id should not be null or empty");
                return;
            }

            lock (_locker)
            {
                AsyncCallback cb;
                if (!_idMap.TryGetValue(id, out cb)) return;
                _queue.Remove(cb);
                _idMap.Remove(id);
            }
        }

        internal void iKillGroup(string group)
        {
            for (var i = _queue.Count - 1; i >= 0; i--)
            {
                var cb = _queue[i];
                if (cb.@group != group) continue;

                lock (_locker)
                {
                    _queue.RemoveAt(i);
                    if (!string.IsNullOrEmpty(cb.id)) _idMap.Remove(cb.id);
                }
            }
        }

        public AsyncCallback GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("GetById Error - id should not be null or empty");
                return null;
            }

            AsyncCallback result;
            lock (_locker)
            {
                _idMap.TryGetValue(id, out result);
            }
            return result;
        }

        //------------------------ STATIC -------------------------

        static Async _api;
        private static bool _isDestroyed;
        private static bool _isPlaying;
        public static float realTime;
        public static float gameTime;
        public static AsyncCallback current;

        internal static Async Api
        {
            get
            {
                if (_isDestroyed) return null;
                if (_isPlaying) return _api;
                return Application.isPlaying ? CreateInstance() : null;
            }
        }

        public static Async CreateInstance()
        {
            if (_isDestroyed) return null;
            if (!Application.isPlaying) return null;
            if (_api != null) return _api;
            _api = new GameObject("~Async").AddComponent<Async>();
            return _api;
        }

        public static AsyncCallback Call(Action action, float delay = 0, string id = null)
        {
            if (Api == null) return null;
            return Api.iCall(action, null, delay, id);
        }

        public static AsyncCallback Wait(Func<bool> wait, Action action, float delay = 0f, string id = null)
        {
            if (Api == null) return null;
            return Api.iCall(action, wait, delay, id);
        }

        public static void Kill(string id)
        {
            if (Api == null) return;
            Api.iKill(id);
        }

        public static void KillGroup(string group)
        {
            if (Api == null) return;
            Api.iKillGroup(group);
        }

        public class AsyncCallback
        {
            internal bool isDie;

            // internal object data;
            internal Action callback;
            internal Func<bool> WaitFunc;

            internal string id;
            internal string group;

            internal int repeatCount;
            internal float repeatDelay;

            internal float time;
            internal bool speedUp; // allows multiple callback calls within the same frame if time is fast-forward (usually because of a hang)

            internal bool isAlive
            {
                get { return !isDie && repeatCount >= 0; }
            }

            internal bool isWaiting
            {
                get { return WaitFunc != null && !WaitFunc(); }
            }

            public AsyncCallback SetId(string newId)
            {
                if (newId == id) return this;
                Api.SetId(this, newId);
                return this;
            }

            public AsyncCallback SetGroup(string newGroup)
            {
                group = newGroup;
                return this;
            }

            public AsyncCallback Delay(float delay, bool add = false)
            {
                time = (add ? Async.realTime : time) + delay;
                return this;
            }

            public AsyncCallback Repeat(int count, float? interval = null)
            {
                repeatCount = count;
                if (interval != null) repeatDelay = interval.Value;
                return this;
            }

            public AsyncCallback Stop()
            {
                isDie = true;
                return this;
            }

            public void AllowSpeedUp(bool value)
            {
                speedUp = value;
            }
        }
    }
}