using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using YukiFrameWork.Manager.FSM;
using System;
using System.Reflection;
using System.Linq;
using System.IO;
using LitJson;
using UnityEditor.Animations;

namespace YukiFrameWork.FSM.Editors
{    
#if UNITY_EDITOR
    [CustomEditor(typeof(StateManager))]
    public class StateEditor : Editor
    {
        private SerializedProperty serializedInitType;

        private SerializedProperty serializedIsModifier;

        private static string nameofid = "-1";
        private static string currentName;
        
        private static int animNameid;//����id        
        private static StateData data;
        private static List<string> statesName = new List<string>();//�������е�״̬��           
        public List<UnityEngine.Object> objects;
        private List<string> behavioursName = new List<string> ();
        private static string jsonpath;
        private string overPath;
        private static bool isProcess;//��ѯ�Ƿ������״̬�ű���
                                      //private static List<string> stateAnimsName = new List<string>();
        public void OnEnable()
        {
            jsonpath = Application.streamingAssetsPath + "/" + target.name;
            //overPath = Path.Combine(Application.streamingAssetsPath, target.name + ".Json");
            if (!Directory.Exists(jsonpath))
            {
                Directory.CreateDirectory(jsonpath);
                AssetDatabase.Refresh();
            }
            StateManager manager = (StateManager)target;
            LoadStateOfData(manager);

            serializedInitType = serializedObject.FindProperty("initType");
            serializedIsModifier = serializedObject.FindProperty("isModifier");
        }

        public override void OnInspectorGUI()
        {           
            base.OnInspectorGUI();
            StateManager manager = (StateManager)target;

            manager.NormalID = EditorGUILayout.IntField("Ĭ��״̬��", manager.NormalID);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("��ʼ���ű�");
            EditorGUILayout.PropertyField(serializedInitType, GUIContent.none);
            EditorGUILayout.EndHorizontal();
            manager.initType = (InitType)serializedInitType.intValue;

            InitState(ref manager.states);          
       
            AddStateMechine(ref manager);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("�����޸Ŀ���");
            EditorGUILayout.PropertyField(serializedIsModifier, GUIContent.none);
            EditorGUILayout.EndHorizontal();
            manager.isModifier = serializedIsModifier.boolValue;

            CheckStateValue(ref manager);

            AddStateScript(manager);
            
            ShowStateBehavior();
        }

        private void LoadStateOfData(StateManager manager)
        {
            if (manager.states != null)
            {
                foreach (var info in manager.states.stateData)
                {
                    if (info.name.Length == 0) continue;
                    string jsonOverPath = Path.Combine(jsonpath, info.name + ".Json");
                    if(File.Exists(jsonOverPath))
                    { 
                    string jsonData = File.ReadAllText(jsonOverPath);
                    FSMData data = JsonMapper.ToObject<FSMData>(jsonData);
                        if (data != null)
                        {                           
                            foreach (var name in data.statesName)
                            {
                                Type type = Type.GetType(name);
                                if (type != null)
                                {
                                    var obj = (StateBehaviour)Activator.CreateInstance(type);
                                    if (obj != null)
                                    {
                                        if (info.stateBehaviours.Find(x => x.GetType() == obj.GetType()) == null)
                                        {                         
                                            info.stateBehaviours.Add(obj);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }          
        }

        private void ShowStateBehavior()
        {
            //Debug.Log(target);
            if (data != null)
            {
                for (int i = 0; i < data.stateBehaviours.Count; i++)
                {
                    FieldInfo[] infos = data.stateBehaviours[i].GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(data.stateBehaviours[i].ToString());
                    if (GUILayout.Button("ɾ���ű�"))
                    {
                        Debug.Log(Path.Combine(jsonpath, data.name + ".Json"));
                        overPath = Path.Combine(jsonpath, data.name + ".Json");
                        string json = File.ReadAllText(overPath);
                        FSMData fsm = JsonMapper.ToObject<FSMData>(json);
                        if (fsm.statesName.Contains(data.stateBehaviours[i].ToString()))
                        {
                            fsm.statesName.Remove(data.stateBehaviours[i].ToString());
                            data.stateBehaviours.RemoveAt(i);
                            string jsonData = JsonMapper.ToJson(fsm);
                            File.WriteAllText(overPath, jsonData);
                            AssetDatabase.Refresh();
                        }

                    }
                    EditorGUILayout.EndHorizontal();
                    foreach (var info in infos)
                    {
                        Type tempType = info.FieldType.BaseType;
                        while (tempType != null)
                        {                           
                            if (tempType == typeof(MonoBehaviour))
                            {
                                objects = new List<UnityEngine.Object>();
                                var targetValue = data.stateBehaviours[i];
                                UnityEngine.Object obj = info.GetValue(targetValue) as UnityEngine.Object;
                                var newValue = EditorGUILayout.ObjectField(info.Name, obj, info.FieldType, true);                                
                                break;
                            }
                            tempType = tempType.BaseType;
                        }                       
                        
                    }                   
                }
            }
        }

        private void AddStateMechine(ref StateManager manager)
        {
            if (GUILayout.Button("����״̬������"))
            {
                manager.StartCoroutine(LoadState(manager));                              
            }
        }

        private void AddStateScript(StateManager manager)
        {
            if (data != null && manager.states != null && serializedIsModifier.boolValue)
            {
                if (!isProcess)
                {
                    if (GUILayout.Button("���״̬�ű�"))
                    {
                        foreach (var info in manager.states.stateData)
                        {
                            if (info.name.Length == 0)
                            {
                                Debug.LogWarning("����������");
                                return;
                            }

                        }
                        isProcess = true;
                    }
                }
                else
                {
                    Type type = typeof(StateBehaviour);
                    var assembly = Assembly.GetAssembly(type).GetTypes();
                    foreach (var info in assembly)
                    {
                        if (info.BaseType == type)
                        {
                            if (GUILayout.Button(info.Name))
                            {
                                isProcess = false;
                                var obj = (StateBehaviour)Activator.CreateInstance(info);
                                if (obj == null)
                                {
                                    Debug.Log("ʵ������ʧ��");
                                    return;
                                }
                                StateMechine mechine = manager.states;
                                if (mechine != null)
                                {
                                    data = mechine.stateData.Find(x => x.id.ToString() == nameofid || x.name == nameofid);
                                    StateBehaviour repetition = null;
                                    if (data != null && data.stateBehaviours.Count > 0)
                                    {
                                        repetition = data.stateBehaviours.Find(x => x.ToString() == obj.ToString());
                                    }
                                    if (data != null && repetition == null)
                                    {
                                        if (data.stateBehaviours.Count <= 0)
                                        {
                                            obj.currentID = 0;
                                        }
                                        else obj.currentID = data.stateBehaviours.Count - 1;
                                        data.stateBehaviours.Add(obj);

                                        List<string> statesName = new List<string>();
                                        foreach (var s in data.stateBehaviours)
                                        {
                                            statesName.Add(s.ToString());
                                        }
                                        FSMData sMData = new FSMData(statesName);
                                        var statesJson = JsonMapper.ToJson(sMData);
                                        if (data.name.Length == 0)
                                        {
                                            Debug.LogWarning("��д״̬��");
                                            return;
                                        }
                                        overPath = Path.Combine(jsonpath, data.name + ".Json");
                                        File.WriteAllText(overPath, statesJson);
                                        AssetDatabase.Refresh();
                                    }
                                }
                            }                        
                        }
                    }
                    if (GUILayout.Button("ȡ��"))
                    {
                        isProcess = false;
                    }
                }
            }
        }

        private void CheckStateValue(ref StateManager manager)
        {
            if (manager.states != null)
            {
                if (serializedIsModifier.boolValue)
                {
                    nameofid = EditorGUILayout.TextField("������״̬id����name", nameofid);

                    StateMechine stateMechine = manager.states;

                    if (stateMechine != null)
                    {
                        data = stateMechine.stateData.Find(x => x.id.ToString() == nameofid || x.name == nameofid);
                        if (data != null)
                        {                           
                            data.isAnim = EditorGUILayout.Toggle("����ϵͳ", data.isAnim);
                            if (data.isAnim)
                            {
                                data.type = (AnimType)EditorGUILayout.EnumPopup("ѡ�񶯻���ʽ", data.type);
                                switch (data.type)
                                {
                                    case AnimType.Animator:
                                        data.anim = null;
                                        if (data.animator == null) data.animator = manager.GetComponent<Animator>();
                                        data.animator = (Animator)EditorGUILayout.ObjectField("�°涯��", data.animator, typeof(Animator), true);
                                        if (data.animator != null)
                                        {
                                                                                       
                                            manager.stateNames = new List<string>();
                                            var stateAnimatorsName = manager.stateNames;
                                           
                                            AnimatorController controller = data.animator.runtimeAnimatorController as AnimatorController;

                                            AnimationClip[] parameters = controller.animationClips;
                                            Debug.Log(parameters.Length);
                                            foreach (var param in parameters)
                                            {
                                                stateAnimatorsName.Add(param.name);
                                            }
                                            if (data.AnimatorName!=null)
                                            {
                                                if (data.AnimatorName != stateAnimatorsName[animNameid])
                                                {
                                                    animNameid = stateAnimatorsName.FindIndex(x => x == data.AnimatorName);
                                                }
                                            }
                                            animNameid = EditorGUILayout.Popup("��������:", animNameid, stateAnimatorsName.ToArray());
                                            data.AnimatorName = stateAnimatorsName[animNameid];
                                        }
                                        break;
                                    case AnimType.Animation:
                                        data.animator = null;
                                        if (data.anim == null) data.anim = manager.GetComponent<Animation>();
                                        data.anim = (Animation)EditorGUILayout.ObjectField("�ɰ涯��", data.anim, typeof(Animation), true);
                                        if (data.anim != null)
                                        {
                                            Animation animation = manager.GetComponent<Animation>();

                                            var anim = AnimationUtility.GetAnimationClips(animation.gameObject).ToArray();
                                            manager.stateNames = new List<string>();
                                            foreach (var info in anim)
                                            {
                                                manager.stateNames.Add(info.name);
                                            }
                                            var stateAnimsName = manager.stateNames;

                                            //stateAnims.Add(data.anim.GetClipCount().ToString());                                          
                                            //�����ʱ���ֲ���������״̬��
                                            if (currentName != nameofid && data.clip != null)
                                            {
                                                //ǿ�и��Ķ���idΪ��ǰ״̬����
                                                animNameid = Array.IndexOf(anim, data.clip);
                                                currentName = nameofid;                                               
                                            }
                                            animNameid = EditorGUILayout.Popup("��������:", animNameid, stateAnimsName.ToArray());

                                            if (anim != null && anim.Length > 0 && animNameid != -1 && anim[animNameid] != null)
                                            {
                                                //��������Ѿ������б����򲻸���
                                                if (data.anim.clip == anim[animNameid])
                                                {
                                                    data.clip = anim[animNameid];
                                                }
                                                else
                                                {
                                                    data.clip = anim[animNameid];
                                                }
                                            }
                                        }
                                        break;
                                }

                                data.animLength = EditorGUILayout.FloatField("�������ų���", data.animLength);
                                data.animSpeed = EditorGUILayout.FloatField("���������ٶ�", data.animSpeed);
                                data.isLoop = EditorGUILayout.Toggle("�Ƿ�ѭ��", data.isLoop);
                                data.isNextState = EditorGUILayout.Toggle("�Զ�������һ��״̬", data.isNextState);
                                if (data.isNextState)
                                {
                                    foreach (StateData stateData in stateMechine.stateData)
                                    {
                                        if (stateData.name != data.name)
                                            statesName.Add(stateData.name);
                                    }
                                    data.nextState = EditorGUILayout.IntField("��һ��״̬��id:", data.nextState);
                                }
                                else statesName.Clear();
                            }
                            else
                            {
                                data.anim = null;
                                data.animator = null;
                                data.animLength = 100;
                                data.animSpeed = 1;
                                data.nextState = -1;
                            }
                        }
                    }
                }
                else
                {
                    nameofid = "-1";
                }
            }
            else
            {
                nameofid = "-1";
            }
        }

        private void InitState(ref StateMechine mechine)
        {
            if (mechine != null)
            {
                if (GUILayout.Button("��ʼ��״̬��"))
                {
                    mechine.Init();
                    Debug.Log("��ʼ���ɹ�");
                }
            }
        }
        private IEnumerator LoadState(StateManager manager)
        {
            if (manager.states == null)
            {
                manager.states = manager.GetComponent<StateMechine>();
                if (manager.states == null)
                {
                    var temp = manager.gameObject.AddComponent<StateMechine>();
                    yield return new WaitUntil(() => temp != null);
                    manager.states = temp;
                    manager.states.InitState();
                }
            }
            else
            {
                Debug.LogWarning("�˶�������״̬��");
                yield break;
            }
        }
    }
#endif
}