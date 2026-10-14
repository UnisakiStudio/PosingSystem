using System.Linq;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingSystemConverter
    {
        private static AnimationClip PrepareRolloverSide(PosingSystem ps, PosingSystem.AnimationDefine pose, GameObject avatar)
        {
            var source = pose.rolloverSideAnimation;
            var side = Object.Instantiate(source);
            AnimationClip automatic = null, combined = null;
            try
            {
                var sourcePose = ps.defines.SelectMany(d => d.animations).FirstOrDefault(a => a.animationClip == source);
                var adjustment = ps.UsesAutoPoseFor(sourcePose) ? null : sourcePose?.adjustmentClip;
                if (ps.UsesAutoPoseFor(sourcePose))
                    adjustment = automatic = CreateAutomaticPoseAdjustmentClip(ps, avatar.GetComponent<Animator>(), source, FindPoseKey(ps, sourcePose));
                if (adjustment != null)
                {
                    AdditiveCurveUtility.MultiplyRootQCurves(source, adjustment, side);
                    foreach (var binding in AnimationUtility.GetCurveBindings(adjustment).Where(AdditiveCurveUtility.IsRootTBinding))
                        AnimationUtility.SetEditorCurve(side, binding, AdditiveCurveUtility.AddCurve(
                            AnimationUtility.GetEditorCurve(side, binding) ?? new AnimationCurve(), AnimationUtility.GetEditorCurve(adjustment, binding)));
                }
                combined = PosingSystemAvatarAdjustmentUtility.CreateCombinedMuscleAdjustmentClip(ps, adjustment);
                var indices = AdditiveCurveUtility.GetAdjustedMuscleIndices(combined);
                indices.UnionWith(PosingSystemAvatarAdjustmentUtility.GetAffectedMuscleIndices(ps));
                if (indices.Count > 0) ApplyMuscleAdjustmentsWithRebake(avatar, source, combined, side, indices, ps);
                return side;
            }
            catch { Object.DestroyImmediate(side); throw; }
            finally { if (automatic != null) Object.DestroyImmediate(automatic); if (combined != null) Object.DestroyImmediate(combined); }
        }
    }
}
