namespace jp.unisakistudio.posingsystemeditor
{
    static class PosingAnimationAdjustmentWindowText
    {
        public static string Title => PosingSystemLocalization.Text("アニメーション調整");
        public static string AdjustmentStopped => PosingSystemLocalization.Text("アニメーションの調整は現在停止中です");
        public static string PosingSystemNotFound => PosingSystemLocalization.Text("PosingSystem が見つかりません。対象のコンポーネントを選択した状態で再度開いてください。");
        public static string SaveChangesMessage => PosingSystemLocalization.Text("保存されていない変更があります。保存しますか？");
        public static string Close => PosingSystemLocalization.Text("閉じる");

        public static string BatchOperations => PosingSystemLocalization.Text("一括操作");
        public static string AllReset => PosingSystemLocalization.Text("全姿勢初期化");
        public static string AllResetTooltip => PosingSystemLocalization.Text("全ての調整値を0にリセットします。");
        public static string AllReload => PosingSystemLocalization.Text("全姿勢再読込");
        public static string AllReloadTooltip => PosingSystemLocalization.Text("全ての調整値をファイルに保存されている値に戻します。");
        public static string AllSaveDiff => PosingSystemLocalization.Text("全差分保存");
        public static string AllSaveDiffTooltip => PosingSystemLocalization.Text("変更のあるアニメーションの調整値を保存します。");
        public static string AllDuplicateSave => PosingSystemLocalization.Text("全調整データ複製保存");
        public static string AllDuplicateSaveTooltip => PosingSystemLocalization.Text("変更のあるアニメーションの調整値を複製して保存します。元々設定されていたファイルは変更されません");
        public static string Animation => PosingSystemLocalization.Text("アニメーション");
        public static string NoAdjustableAnimation => PosingSystemLocalization.Text("調整可能なアニメーションが見つかりません。PosingSystem の設定を確認してください。");

        public static string SelectAnimation => PosingSystemLocalization.Text("アニメーションを選択してください。");
        public static string AnimationNotAssigned => PosingSystemLocalization.Text("アニメーションが割り当てられていません。");
        public static string MotionNotAnimationClip => PosingSystemLocalization.Text("現在のモーションは AnimationClip ではありません。差分調整は AnimationClip のみ対応しています。");
        public static string AnimationInfo => PosingSystemLocalization.Text("アニメーション情報");
        public static string Name => PosingSystemLocalization.Text("名前");
        public static string CurrentAnimation => PosingSystemLocalization.Text("現在のアニメーション");
        public static string Combine => PosingSystemLocalization.Text("合成");
        public static string CombineTooltip => PosingSystemLocalization.Text("現在のアニメーションに調整アニメーションを合成したものを新しいアニメーションクリップとして保存します。");
        public static string AdjustmentAnimation => PosingSystemLocalization.Text("調整アニメーション");
        public static string Duplicate => PosingSystemLocalization.Text("複製");
        public static string DuplicateTooltip => PosingSystemLocalization.Text("調整アニメーションを複製します。");

        public static string ErrorTitle => PosingSystemLocalization.Text("エラー");
        public static string CombineMissingAdjustment => PosingSystemLocalization.Text("合成する調整アニメーションが存在しません。");
        public static string DuplicateMissingAdjustment => PosingSystemLocalization.Text("複製する調整アニメーションが存在しません。");
        public static string Ok => PosingSystemLocalization.Text("OK");
        public static string CompleteTitle => PosingSystemLocalization.Text("完了");
        public static string CombineSavedFormat => PosingSystemLocalization.Text("アニメーションを合成して保存しました。\n新しいファイル: {0}");
        public static string DuplicateSavedFormat => PosingSystemLocalization.Text("調整アニメーションを複製して保存しました。\n新しいファイル: {0}");
        public static string ConfirmTitle => PosingSystemLocalization.Text("確認");
        public static string DuplicateAllConfirm => PosingSystemLocalization.Text("全ての調整データを複製保存しますか？\n変更のあるアニメーションの調整アニメーションが複製され、変更が保存されます。");
        public static string Yes => PosingSystemLocalization.Text("はい");
        public static string No => PosingSystemLocalization.Text("いいえ");
        public static string DuplicateAllCompleteFormat => PosingSystemLocalization.Text("{0}個の調整アニメーションを複製保存しました。");
        public static string InfoTitle => PosingSystemLocalization.Text("情報");
        public static string NoChangesToDuplicate => PosingSystemLocalization.Text("複製保存する変更はありませんでした。");

        public static string CurrentReset => PosingSystemLocalization.Text("初期化");
        public static string CurrentResetTooltip => PosingSystemLocalization.Text("調整値をすべて0にリセットします。");
        public static string CurrentReload => PosingSystemLocalization.Text("再読込");
        public static string CurrentReloadTooltip => PosingSystemLocalization.Text("ファイルに保存されている値に戻します。");
        public static string Save => PosingSystemLocalization.Text("保存");
        public static string SaveTooltip => PosingSystemLocalization.Text("調整値をファイルに保存します。");
        public static string DuplicateAndSave => PosingSystemLocalization.Text("複製して保存");
        public static string DuplicateAndSaveTooltip => PosingSystemLocalization.Text("調整アニメーションを複製して保存します。元々設定されていたファイルは変更されません");

        public static string Height => PosingSystemLocalization.Text("高さ");
        public static string Rotation => PosingSystemLocalization.Text("回転");
        public static string LimitBreak => PosingSystemLocalization.Text("限界突破");
        public static string LimitBreakTooltip => PosingSystemLocalization.Text("限界突破をオンにするとパラメータの設定幅が5倍になります");
        public static string ReloadButton => PosingSystemLocalization.Text("U");
        public static string ReloadTooltip => PosingSystemLocalization.Text("変更を破棄して保存されていた値に戻します");
        public static string ResetButton => PosingSystemLocalization.Text("×");
        public static string ResetTooltip => PosingSystemLocalization.Text("0にリセットします");

        public static string RootAxisX => PosingSystemLocalization.Text("X");
        public static string RootAxisY => PosingSystemLocalization.Text("Y");
        public static string RootAxisZ => PosingSystemLocalization.Text("Z");
        public static string RootAxisYaw => PosingSystemLocalization.Text("回転");
        public static string RootAxisPitch => PosingSystemLocalization.Text("前後傾き");
        public static string RootAxisRoll => PosingSystemLocalization.Text("左右傾き");

        public static string CopyFromOppositeFormat => PosingSystemLocalization.Text("{0}からコピー");
        public static string CopyFromOppositeTooltip => PosingSystemLocalization.Text("左右反対側の部位の調整値をこの部位へ適用します");

        public static string AvatarAnimatorNotFound => PosingSystemLocalization.Text("Animator を持つアバターが見つかりません。");
        public static string SaveAllComplete => PosingSystemLocalization.Text("全ての変更された差分を保存しました。");
        public static string ResetAllConfirm => PosingSystemLocalization.Text("全ての姿勢の調整値をリセットしますか？");
        public static string ReloadAllConfirm => PosingSystemLocalization.Text("全ての姿勢の調整値を再読み込みしますか？未保存の変更は失われます。");
    }
}