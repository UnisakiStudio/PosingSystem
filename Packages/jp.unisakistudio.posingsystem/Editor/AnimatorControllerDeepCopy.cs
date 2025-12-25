using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    /// <summary>
    /// AnimatorControllerの完全なディープコピーを行うユーティリティクラス
    /// </summary>
    public static class AnimatorControllerDeepCopy
    {
        /// <summary>
        /// AnimatorControllerを完全にディープコピーして新しいアセットとして保存
        /// </summary>
        /// <param name="source">コピー元のAnimatorController</param>
        /// <param name="newPath">保存先のパス</param>
        /// <returns>コピーされたAnimatorController</returns>
        public static AnimatorController CloneAnimatorController(AnimatorController source, string newPath)
        {
            if (source == null)
            {
                Debug.LogError("Source AnimatorController is null");
                return null;
            }
            
            // 新しい空のAnimatorControllerを作成
            var clone = new AnimatorController();
            clone.name = System.IO.Path.GetFileNameWithoutExtension(newPath);
            
            // パラメータをコピー
            foreach (var param in source.parameters)
            {
                clone.AddParameter(param.name, param.type);
                var newParam = clone.parameters[clone.parameters.Length - 1];
                newParam.defaultBool = param.defaultBool;
                newParam.defaultFloat = param.defaultFloat;
                newParam.defaultInt = param.defaultInt;
            }
            
            // レイヤーをコピー
            var layers = new AnimatorControllerLayer[source.layers.Length];
            for (int i = 0; i < source.layers.Length; i++)
            {
                var sourceLayer = source.layers[i];
                var newLayer = new AnimatorControllerLayer();
                newLayer.name = sourceLayer.name;
                newLayer.defaultWeight = sourceLayer.defaultWeight;
                newLayer.blendingMode = sourceLayer.blendingMode;
                newLayer.syncedLayerIndex = sourceLayer.syncedLayerIndex;
                newLayer.iKPass = sourceLayer.iKPass;
                newLayer.avatarMask = sourceLayer.avatarMask;
                
                // StateMachineをディープコピー
                newLayer.stateMachine = CloneStateMachine(sourceLayer.stateMachine);
                
                layers[i] = newLayer;
            }
            clone.layers = layers;
            
            // アセットとして保存
            AssetDatabase.CreateAsset(clone, newPath);
            
            // StateMachineもSubAssetとして追加
            foreach (var layer in clone.layers)
            {
                AddStateMachineAsSubAsset(layer.stateMachine, clone);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            return clone;
        }

        private static AnimatorStateMachine CloneStateMachine(AnimatorStateMachine source)
        {
            var globalStateMap = new Dictionary<AnimatorState, AnimatorState>();
            var globalStateMachineMap = new Dictionary<AnimatorStateMachine, AnimatorStateMachine>();
            return CloneStateMachineRecursive(source, globalStateMap, globalStateMachineMap);
        }
        
        private static AnimatorStateMachine CloneStateMachineRecursive(
            AnimatorStateMachine source, 
            Dictionary<AnimatorState, AnimatorState> globalStateMap,
            Dictionary<AnimatorStateMachine, AnimatorStateMachine> globalStateMachineMap)
        {
            // SubAssetは直接Instantiateできないので、新規作成してコピー
            var clone = new AnimatorStateMachine();
            clone.name = source.name;
            globalStateMachineMap[source] = clone;
            
            // StateMachineの基本プロパティをコピー
            clone.anyStatePosition = source.anyStatePosition;
            clone.entryPosition = source.entryPosition;
            clone.exitPosition = source.exitPosition;
            clone.parentStateMachinePosition = source.parentStateMachinePosition;
            
            // Stateをコピー（グローバルマップに登録）
            foreach (var childState in source.states)
            {
                var newState = new AnimatorState();
                newState.name = childState.state.name;
                
                // Stateのプロパティを個別にコピー（Transitionは後で手動で追加）
                newState.motion = childState.state.motion;
                newState.speed = childState.state.speed;
                newState.speedParameter = childState.state.speedParameter;
                newState.speedParameterActive = childState.state.speedParameterActive;
                newState.cycleOffset = childState.state.cycleOffset;
                newState.cycleOffsetParameter = childState.state.cycleOffsetParameter;
                newState.cycleOffsetParameterActive = childState.state.cycleOffsetParameterActive;
                newState.mirror = childState.state.mirror;
                newState.mirrorParameter = childState.state.mirrorParameter;
                newState.mirrorParameterActive = childState.state.mirrorParameterActive;
                newState.iKOnFeet = childState.state.iKOnFeet;
                newState.writeDefaultValues = childState.state.writeDefaultValues;
                newState.tag = childState.state.tag;
                newState.timeParameter = childState.state.timeParameter;
                newState.timeParameterActive = childState.state.timeParameterActive;
                
                // Behavioursをコピー
                foreach (var behaviour in childState.state.behaviours)
                {
                    if (behaviour != null)
                    {
                        var newBehaviour = newState.AddStateMachineBehaviour(behaviour.GetType());
                        EditorUtility.CopySerialized(behaviour, newBehaviour);
                    }
                }
                
                globalStateMap[childState.state] = newState;
                clone.AddState(newState, childState.position);
            }
            
            // 子StateMachineを再帰的にコピー（グローバルマップを共有）
            foreach (var childStateMachine in source.stateMachines)
            {
                var newStateMachine = CloneStateMachineRecursive(childStateMachine.stateMachine, globalStateMap, globalStateMachineMap);
                clone.AddStateMachine(newStateMachine, childStateMachine.position);
            }
            
            // Transitionをコピー（状態参照を更新）- グローバルマップを使用
            foreach (var childState in source.states)
            {
                var sourceState = childState.state;
                var destState = globalStateMap[sourceState];
                
                foreach (var transition in sourceState.transitions)
                {
                    AnimatorStateTransition newTransition = null;
                    
                    // 遷移先の優先順位: destinationState > destinationStateMachine > isExit
                    // Stateへの遷移
                    if (transition.destinationState != null && globalStateMap.ContainsKey(transition.destinationState))
                    {
                        newTransition = destState.AddTransition(globalStateMap[transition.destinationState]);
                    }
                    // StateMachineへの遷移
                    else if (transition.destinationStateMachine != null && globalStateMachineMap.ContainsKey(transition.destinationStateMachine))
                    {
                        newTransition = destState.AddTransition(globalStateMachineMap[transition.destinationStateMachine]);
                    }
                    // Exit Transitionまたは遷移先が見つからない場合
                    else
                    {
                        if (transition.destinationState != null)
                        {
                            Debug.LogWarning($"State '{sourceState.name}': Transition destination state '{transition.destinationState.name}' not found in state map. Creating exit transition instead.");
                        }
                        else if (transition.destinationStateMachine != null)
                        {
                            Debug.LogWarning($"State '{sourceState.name}': Transition destination state machine '{transition.destinationStateMachine.name}' not found. Creating exit transition instead.");
                        }
                        newTransition = destState.AddExitTransition();
                    }
                    
                    if (newTransition != null)
                    {
                        CopyTransitionProperties(transition, newTransition);
                    }
                }
            }
            
            // AnyState Transitionをコピー
            foreach (var transition in source.anyStateTransitions)
            {
                AnimatorStateTransition newTransition = null;
                
                if (transition.isExit)
                {
                    // AnyStateからのExit遷移は作成できないのでスキップ
                    Debug.LogWarning("AnyState exit transition cannot be created. Skipping.");
                    continue;
                }
                else if (transition.destinationState != null)
                {
                    if (globalStateMap.ContainsKey(transition.destinationState))
                    {
                        newTransition = clone.AddAnyStateTransition(globalStateMap[transition.destinationState]);
                    }
                    else
                    {
                        Debug.LogWarning($"AnyState transition destination state '{transition.destinationState.name}' not found. Skipping.");
                        continue;
                    }
                }
                else if (transition.destinationStateMachine != null)
                {
                    if (globalStateMachineMap.ContainsKey(transition.destinationStateMachine))
                    {
                        newTransition = clone.AddAnyStateTransition(globalStateMachineMap[transition.destinationStateMachine]);
                    }
                    else
                    {
                        Debug.LogWarning($"AnyState transition destination state machine '{transition.destinationStateMachine.name}' not found. Skipping.");
                        continue;
                    }
                }
                else
                {
                    Debug.LogWarning("AnyState transition has no destination. Skipping.");
                    continue;
                }
                
                if (newTransition != null)
                {
                    CopyTransitionProperties(transition, newTransition);
                }
            }
            
            // Entry Transitionをコピー
            foreach (var transition in source.entryTransitions)
            {
                AnimatorTransition newTransition = null;
                
                if (transition.isExit)
                {
                    // Entry から Exit への遷移は無効なのでスキップ
                    Debug.LogWarning("Entry to exit transition is invalid. Skipping.");
                    continue;
                }
                else if (transition.destinationState != null)
                {
                    if (globalStateMap.ContainsKey(transition.destinationState))
                    {
                        newTransition = clone.AddEntryTransition(globalStateMap[transition.destinationState]);
                    }
                    else
                    {
                        Debug.LogWarning($"Entry transition destination state '{transition.destinationState.name}' not found. Skipping.");
                        continue;
                    }
                }
                else if (transition.destinationStateMachine != null)
                {
                    if (globalStateMachineMap.ContainsKey(transition.destinationStateMachine))
                    {
                        newTransition = clone.AddEntryTransition(globalStateMachineMap[transition.destinationStateMachine]);
                    }
                    else
                    {
                        Debug.LogWarning($"Entry transition destination state machine '{transition.destinationStateMachine.name}' not found. Skipping.");
                        continue;
                    }
                }
                else
                {
                    Debug.LogWarning("Entry transition has no destination. Skipping.");
                    continue;
                }
                
                if (newTransition != null)
                {
                    CopyTransitionPropertiesBase(transition, newTransition);
                }
            }
            
            // Default Stateを設定
            if (source.defaultState != null && globalStateMap.ContainsKey(source.defaultState))
            {
                clone.defaultState = globalStateMap[source.defaultState];
            }
            
            return clone;
        }
        
        private static void CopyTransitionProperties(AnimatorStateTransition source, AnimatorStateTransition dest)
        {
            CopyTransitionPropertiesBase(source, dest);
            dest.canTransitionToSelf = source.canTransitionToSelf;
            dest.duration = source.duration;
            dest.exitTime = source.exitTime;
            dest.hasExitTime = source.hasExitTime;
            dest.hasFixedDuration = source.hasFixedDuration;
            dest.offset = source.offset;
            dest.interruptionSource = source.interruptionSource;
            dest.orderedInterruption = source.orderedInterruption;
        }
        
        private static void CopyTransitionPropertiesBase(AnimatorTransitionBase source, AnimatorTransitionBase dest)
        {
            dest.isExit = source.isExit;
            dest.mute = source.mute;
            dest.solo = source.solo;
            
            // Conditionsをコピー
            foreach (var condition in source.conditions)
            {
                dest.AddCondition(condition.mode, condition.threshold, condition.parameter);
            }
        }

        private static void AddStateMachineAsSubAsset(AnimatorStateMachine stateMachine, AnimatorController parent)
        {
            if (stateMachine == null) return;
            
            // 既に他のアセットのSubAssetになっていないかチェック
            if (!AssetDatabase.IsSubAsset(stateMachine) && !AssetDatabase.IsMainAsset(stateMachine))
            {
                AssetDatabase.AddObjectToAsset(stateMachine, parent);
                stateMachine.hideFlags = HideFlags.HideInHierarchy;
            }
            else
            {
                Debug.LogWarning($"StateMachine '{stateMachine.name}' is already an asset. Skipping AddObjectToAsset.");
                return;
            }
            
            // Stateを追加
            foreach (var state in stateMachine.states)
            {
                if (state.state != null && !AssetDatabase.IsSubAsset(state.state) && !AssetDatabase.IsMainAsset(state.state))
                {
                    AssetDatabase.AddObjectToAsset(state.state, parent);
                    state.state.hideFlags = HideFlags.HideInHierarchy;
                    
                    // Transitionを追加
                    foreach (var transition in state.state.transitions)
                    {
                        if (transition != null && !AssetDatabase.IsSubAsset(transition) && !AssetDatabase.IsMainAsset(transition))
                        {
                            AssetDatabase.AddObjectToAsset(transition, parent);
                            transition.hideFlags = HideFlags.HideInHierarchy;
                        }
                    }
                    
                    // State内のBehaviourを追加
                    foreach (var behaviour in state.state.behaviours)
                    {
                        if (behaviour != null && !AssetDatabase.IsSubAsset(behaviour) && !AssetDatabase.IsMainAsset(behaviour))
                        {
                            AssetDatabase.AddObjectToAsset(behaviour, parent);
                            behaviour.hideFlags = HideFlags.HideInHierarchy;
                        }
                    }
                }
            }
            
            // StateMachine Behaviourを追加
            foreach (var behaviour in stateMachine.behaviours)
            {
                if (behaviour != null && !AssetDatabase.IsSubAsset(behaviour) && !AssetDatabase.IsMainAsset(behaviour))
                {
                    AssetDatabase.AddObjectToAsset(behaviour, parent);
                    behaviour.hideFlags = HideFlags.HideInHierarchy;
                }
            }
            
            // AnyState Transitionを追加
            foreach (var transition in stateMachine.anyStateTransitions)
            {
                if (transition != null && !AssetDatabase.IsSubAsset(transition) && !AssetDatabase.IsMainAsset(transition))
                {
                    AssetDatabase.AddObjectToAsset(transition, parent);
                    transition.hideFlags = HideFlags.HideInHierarchy;
                }
            }
            
            // Entry Transitionを追加
            foreach (var transition in stateMachine.entryTransitions)
            {
                if (transition != null && !AssetDatabase.IsSubAsset(transition) && !AssetDatabase.IsMainAsset(transition))
                {
                    AssetDatabase.AddObjectToAsset(transition, parent);
                    transition.hideFlags = HideFlags.HideInHierarchy;
                }
            }
            
            // 子StateMachineも再帰的に追加
            foreach (var child in stateMachine.stateMachines)
            {
                AddStateMachineAsSubAsset(child.stateMachine, parent);
            }
        }
    }
}

