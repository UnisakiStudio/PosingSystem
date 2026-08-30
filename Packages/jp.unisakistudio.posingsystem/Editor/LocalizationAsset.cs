using System;
using System.Collections.Generic;

public class LocalizationAsset
{
    private static readonly IReadOnlyDictionary<string, string> JaJpStrings =
        new Dictionary<string, string>
        {
            ["AddStateMachineBehaviourに失敗しました"] = "AddStateMachineBehaviourに失敗しました。多くの場合原因は別のツールなどにエラーが発生していることです。アバタービルド・アップロード前にConsoleウィンドウを確認してエラーを解消してから再度お試しください",
            ["条件式が間違っているパラメータがあります"] = "条件式が間違っているパラメータがあります。「{0}」ファイルの「{1}」というレイヤーで使用されている「{2}」というパラメータは、AnimatorController内では「{3}」という型ですが、条件式に「{4}」が使われているため条件式が正しく動作しません。このためギミックや処理が正しく動作しない可能性があります。使用しているツールやギミックの相性の問題だと思われるため、このパラメータを使用しているツールの開発者に連絡してください",
            ["AnimatorにParameterとして登録されていないパラメータが条件式に使われています"] = "AnimatorにParameterとして登録されていないパラメータが条件式に使われています。「{0}」ファイルの「{1}」というレイヤーで使用されている「{2}」というパラメータが条件式に使われていますが、AnimatorControllerのParametersにはパラメータがありません。使用しているツールやギミックの相性の問題だと思われるため、このパラメータを使用しているツールの開発者に連絡してください",
            ["オブジェクトの設定が更新されています。再度プレビルドを行ってください"] = "「{0}」オブジェクトの設定が更新されています。再度プレビルドを行ってください",
        };

    static public UnityEngine.LocalizationAsset JaJpLocalizationAsset()
    {
        var localizationAsset = new UnityEngine.LocalizationAsset();

        localizationAsset.localeIsoCode = "ja-jp";
        foreach (var entry in JaJpStrings)
        {
            localizationAsset.SetLocalizedString(entry.Key, entry.Value);
        }

        return localizationAsset;
    }

    static public List<UnityEngine.LocalizationAsset> GetList()
    {
        return new() { JaJpLocalizationAsset(), };
    }

    static public nadena.dev.ndmf.localization.Localizer ErrorLocalization()
    {
        // NDMF retains the Localizer in its error report and may render it after the build's
        // temporary UnityEngine.Objects have been unloaded. Keep the error dictionary entirely
        // managed so a later UI redraw cannot dereference a destroyed LocalizationAsset.
        return new nadena.dev.ndmf.localization.Localizer(
            "ja-jp",
            () => new List<(string, Func<string, string>)>
            {
                ("ja-jp", key => JaJpStrings.TryGetValue(key, out var value) ? value : null),
            });
    }
}
