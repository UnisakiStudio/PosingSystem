using System.Collections;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class AnimationDefineDrawerTests
    {
        private AnimationNameTestData data;
        private AnimationNameTestWindow window;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            data = ScriptableObject.CreateInstance<AnimationNameTestData>();
            data.first.displayName = "Original";
            data.first.previewImage = Texture2D.whiteTexture;
            data.second.displayName = "Second";
            data.second.previewImage = Texture2D.whiteTexture;
            window = ScriptableObject.CreateInstance<AnimationNameTestWindow>();
            window.Initialize(data);
            window.ShowUtility();
            window.Focus();
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            if (window != null)
            {
                // End active text editing before destroying the temporary GUI view.
                window.SendEvent(Event.KeyboardEvent("return"));
                window.Close();
            }
            if (data != null) Object.DestroyImmediate(data);
        }

        [UnityTest]
        public IEnumerator Typing_CommitsOnEnterWithoutInvalidatingThumbnail_AndSupportsUndoRedo()
        {
            Undo.IncrementCurrentGroup();
            BeginNameEdit(0);
            foreach (var character in "New pose")
            {
                Type(character);
                Assert.That(data.first.displayName, Is.EqualTo("Original"), "Typing must not serialize each character.");
                Assert.That(data.first.previewImage, Is.SameAs(Texture2D.whiteTexture));
            }

            window.SendEvent(Event.KeyboardEvent("return"));
            yield return null;
            Assert.That(data.first.displayName, Is.EqualTo("New pose"));
            Assert.That(data.first.previewImage, Is.SameAs(Texture2D.whiteTexture), "A name change must not trigger avatar photography.");

            Undo.FlushUndoRecordObjects();
            Undo.IncrementCurrentGroup();
            Undo.PerformUndo();
            Assert.That(data.first.displayName, Is.EqualTo("Original"));
            Assert.That(data.first.previewImage, Is.SameAs(Texture2D.whiteTexture));
            Undo.PerformRedo();
            Assert.That(data.first.displayName, Is.EqualTo("New pose"));
            Assert.That(data.first.previewImage, Is.SameAs(Texture2D.whiteTexture));
        }

        [UnityTest]
        public IEnumerator MovingToAnotherName_CommitsOnlyTheEditedRow()
        {
            BeginNameEdit(0);
            foreach (var character in "Renamed") Type(character);
            yield return null;
            BeginNameEdit(1);
            window.Repaint();
            yield return null;
            yield return null;

            Assert.That(data.first.displayName, Is.EqualTo("Renamed"));
            Assert.That(data.second.displayName, Is.EqualTo("Second"));
            Assert.That(data.first.previewImage, Is.SameAs(Texture2D.whiteTexture));
            Assert.That(data.second.previewImage, Is.SameAs(Texture2D.whiteTexture));

            Type('B');
            window.SendEvent(Event.KeyboardEvent("return"));
            yield return null;
            Assert.That(data.first.displayName, Is.EqualTo("Renamed"));
            Assert.That(data.second.displayName, Is.EqualTo("B"));
            Assert.That(data.second.previewImage, Is.SameAs(Texture2D.whiteTexture));
        }

        [UnityTest]
        public IEnumerator Escape_CancelsPendingName()
        {
            BeginNameEdit(0);
            Type('X');
            window.SendEvent(Event.KeyboardEvent("escape"));
            BeginNameEdit(1);
            window.Repaint();
            yield return null;
            yield return null;

            Assert.That(data.first.displayName, Is.EqualTo("Original"));
            Assert.That(data.first.previewImage, Is.SameAs(Texture2D.whiteTexture));
        }

        [UnityTest]
        public IEnumerator RotationChange_StillInvalidatesThumbnail()
        {
            // The rotation toggle is next to the thumbnail on the fourth line.
            var toggleX = 20 + 5 * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            Click(new Vector2(toggleX, 10 + 3 * EditorGUIUtility.singleLineHeight + 8));
            yield return null;

            Assert.That(data.first.isRotate, Is.True);
            Assert.That(data.first.previewImage, Is.Null);
            Assert.That(data.second.previewImage, Is.SameAs(Texture2D.whiteTexture));
        }

        private void BeginNameEdit(int row)
        {
            Click(new Vector2(100, 10 + row * 130 + 8));
            window.SendEvent(Event.KeyboardEvent(Application.platform == RuntimePlatform.OSXEditor ? "%a" : "^a"));
        }

        private void Type(char character)
        {
            window.SendEvent(new Event { type = EventType.KeyDown, character = character });
            window.SendEvent(new Event { type = EventType.KeyUp, character = character });
        }

        private void Click(Vector2 position)
        {
            window.SendEvent(new Event { type = EventType.MouseDown, mousePosition = position, button = 0, clickCount = 1 });
            window.SendEvent(new Event { type = EventType.MouseUp, mousePosition = position, button = 0, clickCount = 1 });
        }
    }

    public class AnimationNameTestData : ScriptableObject
    {
        public PosingSystem.AnimationDefine first = new PosingSystem.AnimationDefine();
        public PosingSystem.AnimationDefine second = new PosingSystem.AnimationDefine();
    }

    public class AnimationNameTestWindow : EditorWindow
    {
        private SerializedObject serializedData;
        private readonly AnimationDefineDrawer drawer = new AnimationDefineDrawer();

        public void Initialize(AnimationNameTestData data)
        {
            serializedData = new SerializedObject(data);
            position = new Rect(100, 100, 640, 300);
            titleContent = new GUIContent("Animation name regression test");
        }

        private void OnGUI()
        {
            if (serializedData == null) return;
            serializedData.Update();
            var height = EditorGUIUtility.singleLineHeight * 6;
            drawer.OnGUI(new Rect(10, 10, 600, height), serializedData.FindProperty("first"), GUIContent.none);
            drawer.OnGUI(new Rect(10, 140, 600, height), serializedData.FindProperty("second"), GUIContent.none);
            serializedData.ApplyModifiedProperties();
        }

        private void OnDisable()
        {
            serializedData?.Dispose();
            serializedData = null;
        }
    }
}
