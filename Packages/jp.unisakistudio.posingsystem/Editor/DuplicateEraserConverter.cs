
#region

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if NDMF
using nadena.dev.ndmf;
#endif
using jp.unisakistudio.posingsystem;

#endregion

#if NDMF

[assembly: ExportsPlugin(typeof(jp.unisakistudio.posingsystemeditor.DuplicateEraserConverter))]

namespace jp.unisakistudio.posingsystemeditor
{
    public class DuplicateEraserConverter : Plugin<DuplicateEraserConverter>
    {
        /// <summary>
        /// This name is used to identify the plugin internally, and can be used to declare BeforePlugin/AfterPlugin
        /// dependencies. If not set, the full type name will be used.
        /// </summary>
        public override string QualifiedName => "jp.unisakistudio.posingsytemeditor.duplicate-eraser";

        /// <summary>
        /// The plugin name shown in debug UIs. If not set, the qualified name will be shown.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                var request = UnityEditor.PackageManager.Client.List(true, true);
                while (!request.IsCompleted) { }
                if (request.Status == UnityEditor.PackageManager.StatusCode.Success) { return "ゆにさきポーズシステム・重複オブジェクト削除機能" + request.Result.FirstOrDefault(pkg => pkg.name == "jp.unisakistudio.posingsystem").version; }

                return "ゆにさきポーズシステム・重複オブジェクト削除機能";
            }
        }

        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving).Run("Erase duplicate components", ctx =>
            {
                var avatarGameObject = ctx.AvatarRootObject;

                var erasers = new List<DuplicateEraser>(avatarGameObject.GetComponentsInChildren<DuplicateEraser>());

                while (erasers.Count >= 2)
                {
                    Debug.Log(string.Format("DuplicateEraseComponent：重複削除開始", erasers[0].transform.parent.gameObject.name));
                    for (int i = erasers.Count - 1; i >= 1; i--)
                    {
                        if (erasers[i].ID == erasers[0].ID)
                        {
                            Debug.Log(string.Format("DuplicateEraseComponent：{0} 削除", erasers[i].transform.parent.gameObject.name));
                            GameObject.DestroyImmediate(erasers[i].gameObject);
                            erasers.RemoveAt(i);
                        }
                    }
                    erasers.RemoveAt(0);
                    Debug.Log("DuplicateEraseComponent：完了");
                }

                erasers = new List<DuplicateEraser>(avatarGameObject.GetComponentsInChildren<DuplicateEraser>());
                while (erasers.Count > 0)
                {
                    GameObject.DestroyImmediate(erasers[0]);
                    erasers.RemoveAt(0);
                }
                Debug.Log("DuplicateEraseComponent：終了");
            });
        }
    }
}

#endif
