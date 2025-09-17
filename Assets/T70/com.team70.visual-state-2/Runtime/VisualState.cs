using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace nano.vs2
{
	public interface IVSListener
	{
		void OnVSChange();
	}

    [System.Serializable]
    public struct AmountData
    {
        public float amount;
        public AmountTweenType type;
    }
    public enum LoopType
    {
        None,
        Pingpong,
        Restart
    }
    public enum EaseType
    {
        easeInQuad,
        easeOutQuad,
        easeInOutQuad,
        easeInCubic,
        easeOutCubic,
        easeInOutCubic,
        easeInQuart,
        easeOutQuart,
        easeInOutQuart,
        easeInQuint,
        easeOutQuint,
        easeInOutQuint,
        easeInSine,
        easeOutSine,
        easeInOutSine,
        easeInExpo,
        easeOutExpo,
        easeInOutExpo,
        easeInCirc,
        easeOutCirc,
        easeInOutCirc,
        linear,
        spring,
        /* GFX47 MOD START */
        //bounce,
        easeInBounce,
        easeOutBounce,
        easeInOutBounce,
        /* GFX47 MOD END */
        easeInBack,
        easeOutBack,
        easeInOutBack,
        /* GFX47 MOD START */
        //elastic,
        easeInElastic,
        easeOutElastic,
        easeInOutElastic,
        /* GFX47 MOD END */
        punch
    }
    [System.Serializable]
    public enum AmountTweenType
    {
        Idle,
        Up,
        Down,
        UpToZero

    }
    [Serializable]
    public class StateChangeEvent : UnityEvent
    {
    }
    public class VisualState : MonoBehaviour
    {
        [SerializeField]
        public StateChangeEvent OnBeforeChange = new StateChangeEvent();
        [SerializeField]
        public StateChangeEvent OnUpdateChange = new StateChangeEvent();
        [SerializeField]
        public StateChangeEvent OnAfterChange = new StateChangeEvent();

        //public AnimationCurve curve = AnimationCurve.Linear(0,0,1,1);
        public bool useAnim;
        public EaseType easeType = EaseType.linear;
        public LoopType loopType;

        public bool recordChildrenOnly = true;
        public bool skipChildrenOfOtherVS = true;

        public bool setDefaultStateOnStart;
        public int defaultState;


        [SerializeField] private int _state;
        public int state
        {
            get
            {
                return _state;
            }

            set 
            {
                SetState(value, true);
            }
        }
        public List<string> stateNames = new List<string>();
        public int nStates
        {
            get { return stateNames.Count; }
        }
        [NonSerialized] bool _awaked = false;

        void Awake()
        {
            _awaked = true;
            if (delayAction == null) return;
            delayAction();
        }

        void Start()
        {
            oldState = state;
            if (setDefaultStateOnStart)
            {
                SetState(defaultState, true);
            }
        }
        [SerializeField]
        private List<AmountData> amounts;

        public List<VSPropertyBase> listValue = new List<VSPropertyBase>();


#if UNITY_EDITOR
		[ContextMenu("Clean Up")]
		public void CleanUp()
		{
			for (int i = listValue.Count-1; i>=0; i--)
			{
				if (listValue[i].Validate()) continue;
				listValue.RemoveAt(i);
			}

			EditorUtility.SetDirty(this);
		}
#endif


        private int oldState;
        public List<AmountData> getAmounts()
        {
            return amounts;
        }
        private bool forceSet = false;
        public void SetState(int state)
        {
            SetState(state, false);
        }

		//
		// Action being called before VS script being awake	
		// We need to delay until Awake finish and call the action again
		//
        private Action delayAction = null; 
        
        public void SetState(string stateName, bool force = false)
        {
            var index = stateNames.IndexOf(stateName);
            if(index < 0)
            {
                EditorLogWarningOnce("Invalid State: " + index);
                return;
            }
            SetState(index, force);
        }
		
        public void SetState(int stateIndex, bool force = false)
        {
            if (Application.isPlaying) 
            {
                if (!_awaked)
                {
                    //Debug.LogWarning("Not yet awaked! " + this + " --> " + stateIndex);

                    // gc.Async.Wait(
                    //     ()=> { return _awaked; },
                    //     ()=> {
                    //         Debug.LogWarning("-------------> [Awaked] " + this.transform.parent.name + " --> " + stateIndex);
                    //         SetState(stateIndex, force);
                    //     }
                    // );
                    delayAction = ()=>
                    {
                        SetState(stateIndex, force);
                    };
                    return;
                }
            }
            if (!force && _state == stateIndex) return;
            oldState = _state;
            _state = stateIndex;
            forceSet = force;

#if UNITY_EDITOR
            forceAnim = true;
            if (!Application.isPlaying)
            {
                EditorApplication.update -= RefreshState;
                EditorApplication.update += RefreshState;
            }
            else
#endif
            {
                RefreshState();
            }
        }

#if UNITY_EDITOR
        private bool forceAnim;
        public void EditorPlayState(int fromState, int toState)
        {
            oldState = fromState;
            _state = toState;
            forceAnim = true;

            RefreshState();
        }
#endif
        public void SetStateAmount(int fromstate, int toState, float amount)
        {
            InitStateAmountIfNeeded();
            oldState = fromstate;
            _state = toState;
            for (int i = 0; i < nStates; i++)
            {
                var a = amounts[i];
                if (i == fromstate) a.amount = 1 - amount;
                else if (i == toState) a.amount = amount;
                else a.amount = 0;
                amounts[i] = a;
            }
            ApplyBlend(amounts);
        }
        private void InitStateAmountIfNeeded()
        {
            if (amounts == null || amounts.Count != nStates)
            {
                amounts = new List<AmountData>(nStates);
                for (int i = 0; i < nStates; i++) amounts.Add(new AmountData
                {
                    amount = i == 0 ? 1 : 0
                }
                );
            }
        }
        public void SetAmountForAll(float amount)
        {
            //Debug.Log("amount " + amount);
            InitStateAmountIfNeeded();
            float step = 1f / (nStates - 1);
            for (int i = 0; i < nStates - 1; i++)
            {
                var min = i * step;
                var max = min + step;

                var a = amounts[i];

                var isCurRange = (amount > min && amount <= max);
                if (amount.Equals(0) && i == 0)
                {
                    isCurRange = true;
                }
                //if(amount.Equals(1) && i == nStates - 2)
                //{
                //    isCurRange = false;
                //}
                var realAmount = Mathf.Lerp(0, 1, (amount - min) / step);
                a.amount = isCurRange ? 1 - realAmount : 0;
                amounts[i] = a;

                if (isCurRange)
                {
                    var next = amounts[i + 1];
                    next.amount = realAmount;
                    amounts[i + 1] = next;
                    i += 2;
                    for (; i < nStates; i++)
                    {
                        var last = amounts[i];
                        last.amount = 0;
                        amounts[i] = last;
                    }
                    break;
                }

            }
            ApplyBlend(amounts);
        }
        public void ApplyBlend(List<AmountData> amountDatas)
        {

            for (int k = 0; k < listValue.Count; k++)
            {
                var prop = listValue[k];
                if (prop.isIgnore) continue;
                prop.SetValueAmount(CalculateWeight(amountDatas), oldState, _state);
            }
        }
        public void ApplyBlend(VSPropertyBase prop, List<AmountData> amountDatas)
        {
            {
                if (prop.isIgnore) return;
                prop.SetValueAmount(CalculateWeight(amountDatas), oldState, _state); 
            }
        }
        private List<AmountData> CalculateWeight(List<AmountData> AmountDatas)
        {
            var lst = new List<AmountData>(AmountDatas);
            var totalAmount = 0f;
            for (int i = 0; i < AmountDatas.Count; i++)
            {
                totalAmount += lst[i].amount;
            }
            for (int i = 0; i < lst.Count; i++)
            {
                var a = lst[i];
                a.amount =ease(0,1, (totalAmount.Equals(0f) ? 0 : (a.amount / totalAmount)));
                lst[i] = a;
            }
            return lst;

        }

        private float getMinMaxValue(float value, float min, float max, bool isTarget)
        {
            //var value1 =value -( !isTarget ? (1 - max) : 0);
            //var res =  Mathf.Clamp01( (value1 - min ) /  (max - min) );
            //Debug.Log("input: " + value + "  val1: " + value1 + " res: " + res); 
            //return res;

            return Mathf.Clamp01((value - min) / (max - min));
        }
        private void NotifyBeforeChange()
        {
            OnBeforeChange.Invoke();
        }
        private void NotifyUpdateChange()
        {
            OnUpdateChange.Invoke();
        }
        private void NotifyAfterChange()
        {
            OnAfterChange.Invoke();
        }
        private bool haveDelay = false;
        private bool checkHaveDelay()
        {
            for (int i = 0; i < listValue.Count; i++)
            {
                var over = listValue[i].animOverrideData;
                if (over.min > 0 || over.max < 1) return true;
            }
            return false;
        }


        private static HashSet<string> logMap = new HashSet<string>();

        public static void LogWarningOnce(string str)
        {
            if (logMap.Contains(str)) return;
            Debug.LogWarning(str);
            logMap.Add(str);
        }
        
        public static void EditorLogWarningOnce(string str)
        {
            #if UNITY_EDITOR
            if (logMap.Contains(str)) return;
            Debug.LogWarning("[Editor] " + str);
            logMap.Add(str);
            #endif
        }
        
        public void RefreshState()
        {
            if (!_awaked && Application.isPlaying)
            {
                // gc.Async.Call(()=> {
                //     //Debug.LogWarning("[Awaked 2] " + this);
                //     RefreshState();
                // }); // delay call
                return;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (this != null) UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.SceneView.RepaintAll();    
            }
#endif
            var hasSettingAnim = useAnim;
            var playAnim = Application.isPlaying && hasSettingAnim;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.update -= RefreshState;
                if (hasSettingAnim && forceAnim)
                {
                    playAnim = true;
                }
                forceAnim = false;
            }
#endif
            
            NotifyBeforeChange();
            if (forceSet) playAnim = false;
            if(!playAnim)
            {
                if (!VSUtils.SetState(listValue, state))
                {
                    #if UNITY_EDITOR
                    EditorLogWarningOnce("VisualState Error (missing target / property) on: " + this.name);
                    #endif
                }
                
                NotifyAfterChange();
                return;
            }

            GetEasingFunction();
            haveDelay = checkHaveDelay();
            if(haveDelay)
            {
                //TODO: change state when tweening have delay current not supported
                isTweening = false;
                countTime = 0;
            }
            if (isTweening)
            {
                for (int i = 0; i < nStates; i++)
                {
                    var a = amounts[i];
                    if (i == oldState)
                    {
                        a.type = AmountTweenType.Down;
                    }
                    else if (i == _state)
                    {
                        a.type = AmountTweenType.Up;

                        countTime = a.amount * animTime;
                        //Debug.LogError("count time: " + countTime + " " + a.amount);
                    }
                    else
                    {
                        if (!a.amount.Equals(0))
                        {
                            a.type = AmountTweenType.Down;
                        }
                        else
                        {
                            a.type = AmountTweenType.Idle;
                        }
                    }
                    amounts[i] = a;
                }
            }
            else
            {
                countTime = 0;
                amounts = new List<AmountData>(nStates);
                for (int i = 0; i < nStates; i++)
                {
                    amounts.Add(new AmountData()
                    {
                        amount = i == oldState ? 1 : 0,
                        type = i == oldState ? AmountTweenType.Down : (i == _state ? AmountTweenType.Up : AmountTweenType.Idle)
                    });
                }
            }
            if (haveDelay)
            {
                for (int k = 0; k < listValue.Count; k++)
                {
                    listValue[k].InitStateAmountIfNeeded(nStates);
                    var tempAmounts = listValue[k].amounts;

                    {
                        if (isTweening)
                        {
                            var prop = listValue[k];
                            var min = prop.animOverrideData.min;
                            var max = prop.animOverrideData.max;

                            for (int i = 0; i < nStates; i++)
                            {
                                var percent = amounts[i].amount;
                                var a = tempAmounts[i];
                                if (i == oldState)
                                {
                                    a.type = AmountTweenType.Down;
                                }
                                else if (i == _state)
                                {
                                    a.type = AmountTweenType.Up;
                                }
                                else
                                {
                                    if (!a.amount.Equals(0))
                                    {
                                        a.type = AmountTweenType.Down;
                                    }
                                    else
                                    {
                                        a.type = AmountTweenType.Idle;
                                    }
                                }
                                tempAmounts[i] = a;
                            }
                        }
                        else
                        {
                            countTime = 0;
                            tempAmounts = new List<AmountData>(nStates);
                            for (int i = 0; i < nStates; i++)
                            {
                                tempAmounts.Add(new AmountData()
                                {
                                    amount = i == oldState ? 1 : 0,
                                    type = i == oldState ? AmountTweenType.Down : (i == _state ? AmountTweenType.Up : AmountTweenType.Idle)
                                });
                            }
                        }

                        listValue[k].amounts = tempAmounts;
                    }
                }
            }


                isTweening = true;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    editorLastTimeTween = EditorApplication.timeSinceStartup;
                    EditorApplication.update -= OnUpdate;
                    EditorApplication.update += OnUpdate;
                }
                else
#endif
                {
                    VisualStateHelper.Instance.OnUpdate -= OnUpdate;
                    VisualStateHelper.Instance.OnUpdate += OnUpdate;
                }
            
        }
        public float animTime = 1;
        private float countTime;
        private bool isTweening = false;
        private int tweenDir = 1;
#if UNITY_EDITOR
        private double editorLastTimeTween;
#endif
        private void OnUpdate_backup()
        {
            var preAmount = countTime / animTime;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {

                countTime += (float)(EditorApplication.timeSinceStartup - editorLastTimeTween) * tweenDir;
                editorLastTimeTween = EditorApplication.timeSinceStartup;
            }
            else
#endif
            {
                countTime += Time.deltaTime * tweenDir;
            }
            float amount = countTime / animTime;

            //test for delay
                



            var isComplete = tweenDir > 0 ? countTime >= animTime : countTime <= 0;
            float delta = amount - preAmount;

           
            // if (isComplete && loopType != LoopType.None)
            // {
            //     isComplete = false;
            //     if (loopType == LoopType.Restart)
            //     {
            //         countTime = 0;
            //         amount= 1;
            //         Debug.Log(amount);
            //     }
            //     else tweenDir = tweenDir *= -1;

            // }
            int downStateCount = 0;
            for (int i = 0; i < nStates; i++)
            {
                if (amounts[i].type == AmountTweenType.Down) downStateCount++;
            }

            for (int i = 0; i < nStates; i++)
            {
                var a = amounts[i];
                if (a.type == AmountTweenType.Idle) continue;
                //TODO: amount more than 2 element
                if (a.type == AmountTweenType.Down)
                {
                    a.amount -= delta / downStateCount;
                    // a.amount =1- amount;
                    if (a.amount <= 0)
                    {
                        a.amount = 0;
                        a.type = AmountTweenType.Idle;
                    }
                }
                else
                {
                    a.amount += delta;
                    // a.amount = amount;
                    //if (a.amount >= 1)
                    //{
                    //    a.amount = 0;
                    //}
                }



                amounts[i] = a;

            }


            ApplyBlend(amounts);
            NotifyUpdateChange();
            if (isComplete)
            {
                isTweening = false;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorApplication.update -= OnUpdate;
                }
                else
#endif
                {
                    VisualStateHelper.Instance.OnUpdate -= OnUpdate;
                }
                if (loopType != LoopType.None)
                {

                    isComplete = false;
                    if (loopType == LoopType.Restart)
                    {
                        var cache = state;
                        SetState(oldState, true);
                        SetState(cache);
                    }
                    else
                    {
                        SetState(oldState);
                    }

                }
                NotifyAfterChange();
            }

        }


        private void OnUpdate()
        {
            var preAmount = countTime / animTime;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {

                countTime += (float)(EditorApplication.timeSinceStartup - editorLastTimeTween) * tweenDir;
                editorLastTimeTween = EditorApplication.timeSinceStartup;
            }
            else
#endif
            {
                countTime += Time.deltaTime * tweenDir; 
            }
            float amount = countTime / animTime;
            var isComplete = tweenDir > 0 ? countTime >= animTime : countTime <= 0;
            float delta = amount - preAmount;


            #region BaseAmount
            int downStateCount1 = 0;
            for (int i = 0; i < nStates; i++)
            {
                if (amounts[i].type == AmountTweenType.Down) downStateCount1++;
            }

            for (int i = 0; i < nStates; i++)
            {
                var a = amounts[i];
                if (a.type == AmountTweenType.Idle) continue;
                //TODO: amount more than 2 element
                if (a.type == AmountTweenType.Down)
                {
                    a.amount -= delta / downStateCount1;
                    // a.amount =1- amount;
                    if (a.amount <= 0)
                    {
                        a.amount = 0;
                        a.type = AmountTweenType.Idle;
                    }
                }
                else
                {
                    a.amount += delta;
                }
                amounts[i] = a;

            }
            #endregion

            if (!haveDelay)
            {
                ApplyBlend(amounts);
            }
            else
            {
                for (int k = 0; k < listValue.Count; k++)
                {
                    var prop = listValue[k];
                    var min = prop.animOverrideData.min;
                    var max = prop.animOverrideData.max;
                    var propDelta = 0.0f;
                    if (amount < min || amount > max)
                    {
                        propDelta = 0;
                    }
                    else
                    {
                        propDelta = delta * (1f / (max - min));

                    }
                    var amountTemp = prop.amounts;
                    int downStateCount = 0;
                    for (int i = 0; i < nStates; i++)
                    {
                        if (amounts[i].type == AmountTweenType.Down) downStateCount++;
                    }
                    if (downStateCount == 0) downStateCount = 1;
                    for (int i = 0; i < nStates; i++)
                    {


                        var a = amountTemp[i];
                        if (a.type == AmountTweenType.Idle) continue;
                        //TODO: amount more than 2 element
                        if (a.type == AmountTweenType.Down)
                        {
                            a.amount -= propDelta / downStateCount;
                            // a.amount =1- amount;
                            if (a.amount <= 0)
                            {
                                a.amount = 0;
                                a.type = AmountTweenType.Idle;
                            }
                        }
                        else
                        {
                            a.amount += propDelta;
                            // a.amount = amount;
                            //if (a.amount >= 1)
                            //{
                            //    a.amount = 0;
                            //}
                        }



                        amountTemp[i] = a;

                    }
                    prop.amounts = amountTemp;

                    ApplyBlend(prop, prop.amounts);
                }
            }



           

            NotifyUpdateChange();
            if (isComplete)
            {
                isTweening = false;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorApplication.update -= OnUpdate;
                }
                else
#endif
                {
                    VisualStateHelper.Instance.OnUpdate -= OnUpdate;
                }
                if (loopType != LoopType.None && Application.isPlaying)
                {

                    isComplete = false;
                    if (loopType == LoopType.Restart)
                    {
                        var cache = state;
                        SetState(oldState, true);
                        SetState(cache);
                    }
                    else
                    {
                        SetState(oldState);
                    }

                }
                NotifyAfterChange();
            }

        }


        #region Editor
#if UNITY_EDITOR
        public static VisualState recordingTarget;
        public Action OnDataStructChanged;
        public Action OnDataChanged;
        [NonSerialized]
        public bool isRecording;
        private bool dirty;


        public void StartRecord()
        {
            if (nStates <= 0) Create2State();
            isRecording = true;
            recordingTarget = this;

            Undo.postprocessModifications -= OnModify;
            Undo.postprocessModifications += OnModify;
        }

        public void StopRecord()
        {
            recordingTarget = null;
            isRecording = false;
            Undo.postprocessModifications -= OnModify;
        }
        private void Reset()
        {
            Create2State();
            InitStateAmountIfNeeded();
        }
        public void Create2State()
        {
            {

                stateNames = new List<string>()
                    {
                        "0", "1"
                    };
                Resize(2);
                SetState(1);
            }
            NotifyDataStructChanged();
        }
        private void NotifyDataStructChanged()
        {
            if (OnDataStructChanged != null) OnDataStructChanged();
        }
        private void NotifyDataChanged()
        {
            if (OnDataChanged != null) OnDataChanged();
        }
        public void CopyProperty(VSPropertyBase prop, Object target)
        {
            var clone = (VSPropertyBase)prop.Clone();
            clone.target = target;

            var vs = FindProperty(listValue, target, clone.property);
            if (vs != null)
            {
                for (int i = 0; i < clone.stateValues.Count; i++)
                {
                    vs.stateValues[i] = clone.stateValues[i];
                }
                NotifyDataChanged();
                return;
            }

            listValue.Add(clone);
            NotifyDataStructChanged();
        }

        public void AddState(int cloneIndex)
        {
            if (Application.isPlaying) return;
            if (nStates <= 0)
            {
                Create2State();
                return;
            }
            if (cloneIndex >= nStates)
            {
                EditorLogWarningOnce("Sonething wrong: clone state out of range: " + cloneIndex + " nState: " + nStates);
                return;
            }

            stateNames.Add(copyName(stateNames[cloneIndex]));
            VSUtils.AddState(listValue, cloneIndex);
            NotifyDataStructChanged();
        }
        public void RemoveState(int stateIndex)
        {
            stateNames.RemoveAt(stateIndex);
            for (int i = 0; i < listValue.Count; i++)
            {
                listValue[i].RemoveState(stateIndex);
            }

            NotifyDataStructChanged();
        }
        public void RemoveProperty(VSPropertyBase prop)
        {
            listValue.Remove(prop);
            NotifyDataStructChanged();
        }
        private string copyName(string source)
        {
            string num = "";
            for (int i = source.Length - 1; i >= 0; i--)
            {
                if (!Char.IsDigit(source[i]))
                {
                    break;
                }
                if (string.IsNullOrEmpty(num))
                {
                    num += source[i].ToString();
                }
                else
                {
                    num = num.Insert(0, source[i].ToString());
                }

            }
            if (string.IsNullOrEmpty(num))
            {
                return source + " 1";
            }
            return source.Substring(0, source.Length - num.Length) + (int.Parse(num) + 1);

        }
        public void Resize(int size)
        {
            if (Application.isPlaying) return;
            if (listValue == null)
            {
                listValue = new List<VSPropertyBase>();
            }
            VSUtils.Resize(stateNames, size, string.Empty);
            VSUtils.ResizeProperty(listValue, size);


#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        UndoPropertyModification[] OnModify(UndoPropertyModification[] modifications)
        {
            if (Application.isPlaying) return modifications;

            for (var i = 0; i < modifications.Length; i++)
            {
                var m = modifications[i];


                PropertyModification[] arr = VisualState_Unity.GetValueModification(m);
                var p1 = arr[0];
                var p2 = arr[1];

                if (!ShouldRecordTarget(p1.target)) continue;

                if (!AddChange(p1, p2))
                {
                    //Debug.Log(
                    //	string.Format("target={0} ref={1} prop={2} val={3}", p1.target , p1.objectReference, p1.propertyPath, p1.value) + "\n" +
                    //	string.Format("target={0} ref={1} prop={2} val={3}", p2.target , p2.objectReference, p2.propertyPath, p2.value)
                    //);
                }
                else
                {
#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
#endif
                }
            }

            return modifications;
        }
        bool ShouldRecordTarget(Object target)
        {
            if (this == null) return false;
            if (target == this) return false;

            var t = (target is Component) ? (target as Component).transform :
                    (target is GameObject) ? (target as GameObject).transform : null;

            if (t == null) return false;
            if (t == transform) return true;

            if (recordChildrenOnly)
            {
                while (t.parent != null)
                {
                    t = t.parent;

                    if (t == transform) return true; //target being manged by this VisualState

                    if (skipChildrenOfOtherVS)
                    {
                        var c = t.GetComponent<VisualState>();
                        if (c != null) return false; //this target is managed by other VisualState    
                    }
                }

                return false;
            }

            return true;
        }


        bool AddChange(PropertyModification from, PropertyModification to)
        {
            if (Application.isPlaying) return false;
            var p = from.propertyPath;
            if (VisualState_Unity.PROPERTIES_IGNORES.Contains(p)) return false;
            var changedName = string.Empty;
            // Debug.Log(from.target.GetType());
            if (VisualState_Unity.PROPERTIES_CHANGED_NAME.TryGetValue(p, out changedName))
            {
                p = changedName;
            }else
            {
                var type = from.target.GetType();
                if (VisualState_Unity.COM_PROPERTIES_CHANGED_NAME.ContainsKey(type))
                {
                    if (VisualState_Unity.COM_PROPERTIES_CHANGED_NAME[type].TryGetValue(p, out changedName))
                    {
                        p = changedName;
                    }
                }
            }


            var arr = p.Split('.');
            var isStruct = arr.Length >= 2;
            // p = arr[0];

            var vs = FindProperty(listValue, from.target, p);
            if (vs != null)
            {
                vs.WriteChange(_state, to);
                NotifyDataChanged();
                return true;
            }

            // not existed, create new!
            vs = NewProperty<VSPropertyBase>(null, p, from, to, isStruct);
            if (vs != null)
            {
                listValue.Add(vs);
                NotifyDataStructChanged();
                return true;
            }


            return false;
        }
        public T FindProperty<T>(List<T> list, Object tar, string prop) where T : VSPropertyBase
        {
            for (var i = 0; i < list.Count; i++)
            {
                var v = list[i];
                if (v.property == prop)
                {
                    if (tar == null || (v.target == tar)) return v;
                }
            }

            return null;
        }
        public T NewProperty<T>(VSGetSet f, string prop, PropertyModification from, PropertyModification to, bool isStruct) where T : VSPropertyBase, new()
        {
            //prop = from.propertyPath;
            if (f == null)
            {
                f = VSGetSet.Create(from.target, prop);
                if (f == null)
                {
                    Debug.LogWarning("Unsupported : " + from.target + ":" + prop + "  " + from.value + " " + from.objectReference);

                    Debug.Log("===Property===");
                    var type = from.target.GetType();
                    foreach (var item in type.GetProperties(VSGetSet.FLAGS))
                    {
                        Debug.Log(item.Name +  "  " + item.PropertyType);
                    }

                    Debug.Log("===Field===");
                    var fields = type.GetFields(VSGetSet.FLAGS);
                    foreach (var item in fields)
                    {
                        Debug.Log(item.Name);
                    }

                    return null; // unsupported
                }
            }
            var result = new T() { property = prop, target = from.target, field = f, isStructValue = false };
            result.WriteDefault(nStates, from, prop);
            result.WriteChange(_state, to);
            return result;
        }





        //for draw window data
        public Dictionary<string, VSWindowData> group;
        public class VSWindowData
        {
            public GameObject gameObject;
            public List<VSPropertyBase> lst;
        }
        public void InitForWindow()
        {


            group = new Dictionary<string, VSWindowData>();
            for (int i = 0; i < listValue.Count; i++)
            {
                var obj = (listValue[i].target as Component).gameObject;
                if (obj == null)
                {
                    EditorLogWarningOnce("something wrong object null : " + listValue[i].target);
                    continue;
                }
                var key = obj.GetInstanceID() + "_" + listValue[i].target.GetInstanceID();

                VSWindowData data = null;
                if (!group.TryGetValue(key, out data))
                {

                    data = new VSWindowData
                    {
                        gameObject = obj,
                        lst = new List<VSPropertyBase>()
                    };
                    group.Add(key, data);
                }
                data.lst.Add(listValue[i]);


            }
        }
#endif

        #endregion

        public delegate float EasingFunction(float start, float end, float Value);
        private EasingFunction _ease;
        public void ClearEasing()
        {
            _ease = null;
        }
        private EasingFunction ease
        {
            get
            {
                if (_ease != null) return _ease;
                GetEasingFunction();
                return _ease;
            }
            set
            {
                _ease = value;
            }
        }
        void GetEasingFunction()
        {
            switch (easeType)
            {
                case EaseType.easeInQuad:
                    _ease = new EasingFunction(easeInQuad);
                    break;
                case EaseType.easeOutQuad:
                    _ease = new EasingFunction(easeOutQuad);
                    break;
                case EaseType.easeInOutQuad:
                    _ease = new EasingFunction(easeInOutQuad);
                    break;
                case EaseType.easeInCubic:
                    _ease = new EasingFunction(easeInCubic);
                    break;
                case EaseType.easeOutCubic:
                    _ease = new EasingFunction(easeOutCubic);
                    break;
                case EaseType.easeInOutCubic:
                    _ease = new EasingFunction(easeInOutCubic);
                    break;
                case EaseType.easeInQuart:
                    _ease = new EasingFunction(easeInQuart);
                    break;
                case EaseType.easeOutQuart:
                    _ease = new EasingFunction(easeOutQuart);
                    break;
                case EaseType.easeInOutQuart:
                    _ease = new EasingFunction(easeInOutQuart);
                    break;
                case EaseType.easeInQuint:
                    _ease = new EasingFunction(easeInQuint);
                    break;
                case EaseType.easeOutQuint:
                    _ease = new EasingFunction(easeOutQuint);
                    break;
                case EaseType.easeInOutQuint:
                    _ease = new EasingFunction(easeInOutQuint);
                    break;
                case EaseType.easeInSine:
                    _ease = new EasingFunction(easeInSine);
                    break;
                case EaseType.easeOutSine:
                    _ease = new EasingFunction(easeOutSine);
                    break;
                case EaseType.easeInOutSine:
                    _ease = new EasingFunction(easeInOutSine);
                    break;
                case EaseType.easeInExpo:
                    _ease = new EasingFunction(easeInExpo);
                    break;
                case EaseType.easeOutExpo:
                    _ease = new EasingFunction(easeOutExpo);
                    break;
                case EaseType.easeInOutExpo:
                    _ease = new EasingFunction(easeInOutExpo);
                    break;
                case EaseType.easeInCirc:
                    _ease = new EasingFunction(easeInCirc);
                    break;
                case EaseType.easeOutCirc:
                    _ease = new EasingFunction(easeOutCirc);
                    break;
                case EaseType.easeInOutCirc:
                    _ease = new EasingFunction(easeInOutCirc);
                    break;
                case EaseType.linear:
                    _ease = new EasingFunction(linear);
                    break;
                case EaseType.spring:
                    _ease = new EasingFunction(spring);
                    break;
                /* GFX47 MOD START */
                /*case EaseType.bounce:
                    _ease = new EasingFunction(bounce);
                    break;*/
                case EaseType.easeInBounce:
                    _ease = new EasingFunction(easeInBounce);
                    break;
                case EaseType.easeOutBounce:
                    _ease = new EasingFunction(easeOutBounce);
                    break;
                case EaseType.easeInOutBounce:
                    _ease = new EasingFunction(easeInOutBounce);
                    break;
                /* GFX47 MOD END */
                case EaseType.easeInBack:
                    _ease = new EasingFunction(easeInBack);
                    break;
                case EaseType.easeOutBack:
                    _ease = new EasingFunction(easeOutBack);
                    break;
                case EaseType.easeInOutBack:
                    _ease = new EasingFunction(easeInOutBack);
                    break;
                /* GFX47 MOD START */
                /*case EaseType.elastic:
                    _ease = new EasingFunction(elastic);
                    break;*/
                case EaseType.easeInElastic:
                    _ease = new EasingFunction(easeInElastic);
                    break;
                case EaseType.easeOutElastic:
                    _ease = new EasingFunction(easeOutElastic);
                    break;
                case EaseType.easeInOutElastic:
                    _ease = new EasingFunction(easeInOutElastic);
                    break;
                    /* GFX47 MOD END */
            }
        }

        #region Easing Curves

        private float linear(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, value);
        }

        private float clerp(float start, float end, float value)
        {
            float min = 0.0f;
            float max = 360.0f;
            float half = Mathf.Abs((max - min) * 0.5f);
            float retval = 0.0f;
            float diff = 0.0f;
            if ((end - start) < -half)
            {
                diff = ((max - start) + end) * value;
                retval = start + diff;
            }
            else if ((end - start) > half)
            {
                diff = -((max - end) + start) * value;
                retval = start + diff;
            }
            else retval = start + (end - start) * value;
            return retval;
        }

        private float spring(float start, float end, float value)
        {
            value = Mathf.Clamp01(value);
            value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
            return start + (end - start) * value;
        }

        private float easeInQuad(float start, float end, float value)
        {
            end -= start;
            return end * value * value + start;
        }

        private float easeOutQuad(float start, float end, float value)
        {
            end -= start;
            return -end * value * (value - 2) + start;
        }

        private float easeInOutQuad(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value + start;
            value--;
            return -end * 0.5f * (value * (value - 2) - 1) + start;
        }

        private float easeInCubic(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value + start;
        }

        private float easeOutCubic(float start, float end, float value)
        {
            value--;
            end -= start;
            return end * (value * value * value + 1) + start;
        }

        private float easeInOutCubic(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value * value + start;
            value -= 2;
            return end * 0.5f * (value * value * value + 2) + start;
        }

        private float easeInQuart(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value * value + start;
        }

        private float easeOutQuart(float start, float end, float value)
        {
            value--;
            end -= start;
            return -end * (value * value * value * value - 1) + start;
        }

        private float easeInOutQuart(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value * value * value + start;
            value -= 2;
            return -end * 0.5f * (value * value * value * value - 2) + start;
        }

        private float easeInQuint(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value * value * value + start;
        }

        private float easeOutQuint(float start, float end, float value)
        {
            value--;
            end -= start;
            return end * (value * value * value * value * value + 1) + start;
        }

        private float easeInOutQuint(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value * value * value * value + start;
            value -= 2;
            return end * 0.5f * (value * value * value * value * value + 2) + start;
        }

        private float easeInSine(float start, float end, float value)
        {
            end -= start;
            return -end * Mathf.Cos(value * (Mathf.PI * 0.5f)) + end + start;
        }

        private float easeOutSine(float start, float end, float value)
        {
            end -= start;
            return end * Mathf.Sin(value * (Mathf.PI * 0.5f)) + start;
        }

        private float easeInOutSine(float start, float end, float value)
        {
            end -= start;
            return -end * 0.5f * (Mathf.Cos(Mathf.PI * value) - 1) + start;
        }

        private float easeInExpo(float start, float end, float value)
        {
            end -= start;
            return end * Mathf.Pow(2, 10 * (value - 1)) + start;
        }

        private float easeOutExpo(float start, float end, float value)
        {
            end -= start;
            return end * (-Mathf.Pow(2, -10 * value) + 1) + start;
        }

        private float easeInOutExpo(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * Mathf.Pow(2, 10 * (value - 1)) + start;
            value--;
            return end * 0.5f * (-Mathf.Pow(2, -10 * value) + 2) + start;
        }

        private float easeInCirc(float start, float end, float value)
        {
            end -= start;
            return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
        }

        private float easeOutCirc(float start, float end, float value)
        {
            value--;
            end -= start;
            return end * Mathf.Sqrt(1 - value * value) + start;
        }

        private float easeInOutCirc(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return -end * 0.5f * (Mathf.Sqrt(1 - value * value) - 1) + start;
            value -= 2;
            return end * 0.5f * (Mathf.Sqrt(1 - value * value) + 1) + start;
        }

        /* GFX47 MOD START */
        private float easeInBounce(float start, float end, float value)
        {
            end -= start;
            float d = 1f;
            return end - easeOutBounce(0, end, d - value) + start;
        }
        /* GFX47 MOD END */

        /* GFX47 MOD START */
        //private float bounce(float start, float end, float value){
        private float easeOutBounce(float start, float end, float value)
        {
            value /= 1f;
            end -= start;
            if (value < (1 / 2.75f))
            {
                return end * (7.5625f * value * value) + start;
            }
            else if (value < (2 / 2.75f))
            {
                value -= (1.5f / 2.75f);
                return end * (7.5625f * (value) * value + .75f) + start;
            }
            else if (value < (2.5 / 2.75))
            {
                value -= (2.25f / 2.75f);
                return end * (7.5625f * (value) * value + .9375f) + start;
            }
            else
            {
                value -= (2.625f / 2.75f);
                return end * (7.5625f * (value) * value + .984375f) + start;
            }
        }
        /* GFX47 MOD END */

        /* GFX47 MOD START */
        private float easeInOutBounce(float start, float end, float value)
        {
            end -= start;
            float d = 1f;
            if (value < d * 0.5f) return easeInBounce(0, end, value * 2) * 0.5f + start;
            else return easeOutBounce(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
        }
        /* GFX47 MOD END */

        private float easeInBack(float start, float end, float value)
        {
            end -= start;
            value /= 1;
            float s = 1.70158f;
            return end * (value) * value * ((s + 1) * value - s) + start;
        }

        private float easeOutBack(float start, float end, float value)
        {
            float s = 1.70158f;
            end -= start;
            value = (value) - 1;
            return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
        }

        private float easeInOutBack(float start, float end, float value)
        {
            float s = 1.70158f;
            end -= start;
            value /= .5f;
            if ((value) < 1)
            {
                s *= (1.525f);
                return end * 0.5f * (value * value * (((s) + 1) * value - s)) + start;
            }
            value -= 2;
            s *= (1.525f);
            return end * 0.5f * ((value) * value * (((s) + 1) * value + s) + 2) + start;
        }

        private float punch(float amplitude, float value)
        {
            float s = 9;
            if (value == 0)
            {
                return 0;
            }
            else if (value == 1)
            {
                return 0;
            }
            float period = 1 * 0.3f;
            s = period / (2 * Mathf.PI) * Mathf.Asin(0);
            return (amplitude * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * 1 - s) * (2 * Mathf.PI) / period));
        }

        /* GFX47 MOD START */
        private float easeInElastic(float start, float end, float value)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (value == 0) return start;

            if ((value /= d) == 1) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return -(a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
        }
        /* GFX47 MOD END */

        /* GFX47 MOD START */
        //private float elastic(float start, float end, float value){
        private float easeOutElastic(float start, float end, float value)
        {
            /* GFX47 MOD END */
            //Thank you to rafael.marteleto for fixing this as a port over from Pedro's UnityTween
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (value == 0) return start;

            if ((value /= d) == 1) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p * 0.25f;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
        }

        /* GFX47 MOD START */
        private float easeInOutElastic(float start, float end, float value)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (value == 0) return start;

            if ((value /= d * 0.5f) == 2) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (value < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
            return a * Mathf.Pow(2, -10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
        }
        /* GFX47 MOD END */

        #endregion

    }
}

