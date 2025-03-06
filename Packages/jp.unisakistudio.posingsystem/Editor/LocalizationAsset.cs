using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationAsset
{
    static public UnityEngine.LocalizationAsset JaJpLocalizationAsset()
    {
        var localizationAsset = new UnityEngine.LocalizationAsset();

        localizationAsset.localeIsoCode = "ja-jp";
        localizationAsset.SetLocalizedString("AddStateMachineBehaviourに失敗しました", "AddStateMachineBehaviourに失敗しました。多くの場合原因は別のツールなどにエラーが発生していることです。アバタービルド・アップロード前にConsoleウィンドウを確認してエラーを解消してから再度お試しください");
        localizationAsset.SetLocalizedString("条件式が間違っているパラメータがあります", "条件式が間違っているパラメータがあります。「{0}」ファイルの「{1}」というレイヤーで使用されている「{2}」というパラメータは、AnimatorController内では「{3}」という型ですが、条件式に「{4}」が使われているため条件式が正しく動作しません。このためギミックや処理が正しく動作しない可能性があります。使用しているツールやギミックの相性の問題だと思われるため、このパラメータを使用しているツールの開発者に連絡してください");
        localizationAsset.SetLocalizedString("AnimatorにParameterとして登録されていないパラメータが条件式に使われています", "AnimatorにParameterとして登録されていないパラメータが条件式に使われています。「{0}」ファイルの「{1}」というレイヤーで使用されている「{2}」というパラメータが条件式に使われていますが、AnimatorControllerのParametersにはパラメータがありません。使用しているツールやギミックの相性の問題だと思われるため、このパラメータを使用しているツールの開発者に連絡してください");

        return localizationAsset;
    }

    static public List<UnityEngine.LocalizationAsset> GetList()
    {
        return new() { JaJpLocalizationAsset(), };
    }

    static public nadena.dev.ndmf.localization.Localizer ErrorLocalization()
    {
        return new nadena.dev.ndmf.localization.Localizer("ja-jp", () =>
        {
            return GetList();
        });
    }
}
