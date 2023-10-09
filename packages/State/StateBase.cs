using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace YukiFrameWork.States
{  
    [Serializable]
    public class StateBase
    {
        [Header("×´Ì¬Ãû")]
        public string name;

        [Header("×´Ì¬ÏÂ±ê")]
        public int index;

        public List<BehaviourBase> behaviours = new List<BehaviourBase>();

        [HideInInspector]
        public List<StateBehaviour> stateBehaviours = new List<StateBehaviour>();       

        public Rect rect;
     
        [HideInInspector]
        public float initRectPositionX;
        
        [HideInInspector]
        public float initRectPositionY;

        public StateMechine stateMechine => stateManager.stateMechine;

        public StateManager stateManager = null;

        public virtual void Init(StateManager stateManager)
        {
            this.stateManager = stateManager;
            foreach (var item in behaviours)
            {                              
                Type type = Type.GetType(item.name);
                
                if (type != null)
                {
                    StateBehaviour obj = Activator.CreateInstance(type) as StateBehaviour;                    
                    obj.name = item.name;
                    obj.ID = item.ID;
                    obj.IsActive = item.IsActive;
                    
                    obj.SetStateManager(stateManager);
                    obj.Init();
                    
                    stateBehaviours.Add(obj);
                }
                
            }            
        }

        public void AddBehaviours(StateBehaviour behaviour)
        {
            behaviours.Add(behaviour);
        }

        public void RemoveBehviours(string name)
        {
            behaviours.Remove(behaviours.Find(x => x.name == name));
        }
    }
}