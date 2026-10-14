using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    public static class PosingSystemEditorChangeUtility
    {
        // Navigation must end the recycled IMGUI text editor before another row uses its control ID.
        public static void EndTextEditing()
        {
            if (Event.current != null) GUI.FocusControl(null);
            GUIUtility.keyboardControl = 0;
            EditorGUIUtility.editingTextField = false;
        }

        public static bool Commit(SerializedObject serialized)
        {
            EndTextEditing();
            var changed = Apply(serialized);
            Undo.FlushUndoRecordObjects();
            return changed;
        }

        public static bool Apply(SerializedObject serialized)
        {
            if (!serialized.ApplyModifiedProperties()) return false;
            foreach (var target in serialized.targetObjects) MarkChanged(target);
            return true;
        }

        public static void MarkChanged(Object target)
        {
            if (target == null) return;
            if (target is PosingSystem ps) ps.previousErrorCheckTime = System.DateTime.MinValue;
            // Includes edits to shared IK catalogs and other referenced assets.
            PosingSystemHierarchyBadge.InvalidateAll();
            EditorUtility.SetDirty(target);
            if (PrefabUtility.IsPartOfPrefabInstance(target)) PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            var obj = target is Component component ? component.gameObject : target as GameObject;
            if (obj != null && obj.scene.IsValid() && obj.scene.isLoaded && !EditorSceneManager.IsPreviewScene(obj.scene))
                EditorSceneManager.MarkSceneDirty(obj.scene);
        }

        public static void DeleteArrayElement(SerializedProperty array, int index)
        {
            Commit(array.serializedObject);
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("設定を削除");
            array.DeleteArrayElementAtIndex(index);
            Apply(array.serializedObject);
        }
    }
}
