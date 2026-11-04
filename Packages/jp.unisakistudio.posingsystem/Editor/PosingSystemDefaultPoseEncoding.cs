#if NDMF && MODULAR_AVATAR
using System.Linq;
using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingSystemConverter
    {
        // The reference template predates high sync bits. Standard poses must decode/reset
        // the complete encoding too, otherwise 256/257/258 alias 0/1/2 and stale bits survive.
        internal static void NormalizeDefaultPoseEncoding(AnimatorController controller, IAssetSaver saver)
        {
            var selector = controller.layers.Single(l => l.name == "USSPS_LocomotionType").stateMachine;
            var playback = controller.layers.Single(l => l.name == "USSPS_Locomotion").stateMachine;
            foreach (var pair in new[] { (type:0, pose:0), (type:11, pose:1), (type:21, pose:2) })
            {
                var encoder = selector.entryTransitions.FirstOrDefault(t => t.conditions.Length == 1 &&
                    t.conditions[0].parameter == "LocomotionType" && t.conditions[0].mode == AnimatorConditionMode.Equals &&
                    t.conditions[0].threshold == pair.type)?.destinationState;
                if (encoder == null) continue;
                foreach (var driver in encoder.behaviours.OfType<VRCAvatarParameterDriver>().Where(d =>
                    d.parameters.Any(p => p.name == "USSPS_Pose" && p.type == VRC_AvatarParameterDriver.ChangeType.Set && p.value == pair.pose)))
                    for (var bit=0;bit<8;bit++)
                        if (!driver.parameters.Any(p => p.name == "USSPS_Pose" + bit))
                            driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter {
                                name="USSPS_Pose"+bit, type=VRC_AvatarParameterDriver.ChangeType.Set, value=0 });
                foreach (var entry in playback.entryTransitions.Where(t => t.conditions.Any(c =>
                    c.parameter == "USSPS_Pose" && c.mode == AnimatorConditionMode.Equals && c.threshold == pair.pose) &&
                    t.destinationState != null && new[] { "Standing", "Standing_Desktop", "Crouching", "Crouching_Desktop", "Prone", "Prone_Desktop" }.Contains(t.destinationState.name)))
                {
                    for (var bit=0;bit<8;bit++)
                        if (!entry.conditions.Any(c => c.parameter == "USSPS_Pose"+bit))
                            entry.AddCondition(AnimatorConditionMode.IfNot, 0, "USSPS_Pose"+bit);
                    var state=entry.destinationState;
                    var source=state.transitions.FirstOrDefault(t=>t.isExit && t.conditions.Length==1 &&
                        t.conditions[0].parameter=="USSPS_Pose" && t.conditions[0].mode==AnimatorConditionMode.NotEqual && t.conditions[0].threshold==pair.pose);
                    if (source==null) continue;
                    var transitions=new List<AnimatorStateTransition>();
                    foreach(var current in state.transitions)
                    {
                        transitions.Add(current);
                        if(current!=source)continue;
                        for(var bit=0;bit<8;bit++)
                        {
                            var parameter="USSPS_Pose"+bit;
                            if(state.transitions.Any(t=>t.isExit && t.conditions.Length==1 && t.conditions[0].parameter==parameter && t.conditions[0].mode==AnimatorConditionMode.If))continue;
                            var exit=new AnimatorStateTransition(); EditorUtility.CopySerialized(source,exit);
                            exit.conditions=new[]{new AnimatorCondition {parameter=parameter,mode=AnimatorConditionMode.If,threshold=0}};
                            SaveGeneratedAsset(exit,controller,saver); transitions.Add(exit);
                        }
                    }
                    state.transitions=transitions.ToArray();
                }
            }
        }
    }
}
#endif
