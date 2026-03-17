namespace jp.unisakistudio.posingsystemeditor
{
    static class PosingAnimationAdjustmentWindowText
    {
        public const string Title = "アニメーション調整";
        public const string AdjustmentStopped = "アニメーションの調整は現在停止中です";
        public const string PosingSystemNotFound = "PosingSystem が見つかりません。対象のコンポーネントを選択した状態で再度開いてください。";
        public const string SaveChangesMessage = "保存されていない変更があります。保存しますか？";
        public const string Close = "閉じる";

        public const string BatchOperations = "一括操作";
        public const string AllReset = "全姿勢初期化";
        public const string AllResetTooltip = "全ての調整値を0にリセットします。";
        public const string AllReload = "全姿勢再読込";
        public const string AllReloadTooltip = "全ての調整値をファイルに保存されている値に戻します。";
        public const string AllSaveDiff = "全差分保存";
        public const string AllSaveDiffTooltip = "変更のあるアニメーションの調整値を保存します。";
        public const string AllDuplicateSave = "全調整データ複製保存";
        public const string AllDuplicateSaveTooltip = "変更のあるアニメーションの調整値を複製して保存します。元々設定されていたファイルは変更されません";
        public const string Animation = "アニメーション";
        public const string NoAdjustableAnimation = "調整可能なアニメーションが見つかりません。PosingSystem の設定を確認してください。";

        public const string SelectAnimation = "アニメーションを選択してください。";
        public const string AnimationNotAssigned = "アニメーションが割り当てられていません。";
        public const string MotionNotAnimationClip = "現在のモーションは AnimationClip ではありません。差分調整は AnimationClip のみ対応しています。";
        public const string AnimationInfo = "アニメーション情報";
        public const string Name = "名前";
        public const string CurrentAnimation = "現在のアニメーション";
        public const string Combine = "合成";
        public const string CombineTooltip = "現在のアニメーションに調整アニメーションを合成したものを新しいアニメーションクリップとして保存します。";
        public const string AdjustmentAnimation = "調整アニメーション";
        public const string Duplicate = "複製";
        public const string DuplicateTooltip = "調整アニメーションを複製します。";

        public const string ErrorTitle = "エラー";
        public const string CombineMissingAdjustment = "合成する調整アニメーションが存在しません。";
        public const string DuplicateMissingAdjustment = "複製する調整アニメーションが存在しません。";
        public const string Ok = "OK";
        public const string CompleteTitle = "完了";
        public const string CombineSavedFormat = "アニメーションを合成して保存しました。\n新しいファイル: {0}";
        public const string DuplicateSavedFormat = "調整アニメーションを複製して保存しました。\n新しいファイル: {0}";
        public const string ConfirmTitle = "確認";
        public const string DuplicateAllConfirm = "全ての調整データを複製保存しますか？\n変更のあるアニメーションの調整アニメーションが複製され、変更が保存されます。";
        public const string Yes = "はい";
        public const string No = "いいえ";
        public const string DuplicateAllCompleteFormat = "{0}個の調整アニメーションを複製保存しました。";
        public const string InfoTitle = "情報";
        public const string NoChangesToDuplicate = "複製保存する変更はありませんでした。";

        public const string CurrentReset = "初期化";
        public const string CurrentResetTooltip = "調整値をすべて0にリセットします。";
        public const string CurrentReload = "再読込";
        public const string CurrentReloadTooltip = "ファイルに保存されている値に戻します。";
        public const string Save = "保存";
        public const string SaveTooltip = "調整値をファイルに保存します。";
        public const string DuplicateAndSave = "複製して保存";
        public const string DuplicateAndSaveTooltip = "調整アニメーションを複製して保存します。元々設定されていたファイルは変更されません";

        public const string Height = "高さ";
        public const string Rotation = "回転";
        public const string LimitBreak = "限界突破";
        public const string LimitBreakTooltip = "限界突破をオンにするとパラメータの設定幅が5倍になります";
        public const string ReloadButton = "U";
        public const string ReloadTooltip = "変更を破棄して保存されていた値に戻します";
        public const string ResetButton = "×";
        public const string ResetTooltip = "0にリセットします";

        public const string RootAxisX = "X";
        public const string RootAxisY = "Y";
        public const string RootAxisZ = "Z";
        public const string RootAxisYaw = "回転";
        public const string RootAxisPitch = "前後傾き";
        public const string RootAxisRoll = "左右傾き";

        public const string CopyFromOppositeFormat = "{0}からコピー";
        public const string CopyFromOppositeTooltip = "左右反対側の部位の調整値をこの部位へ適用します";

        public const string AvatarAnimatorNotFound = "Animator を持つアバターが見つかりません。";
        public const string SaveAllComplete = "全ての変更された差分を保存しました。";
        public const string ResetAllConfirm = "全ての姿勢の調整値をリセットしますか？";
        public const string ReloadAllConfirm = "全ての姿勢の調整値を再読み込みしますか？未保存の変更は失われます。";
    }
}