# 開発プロジェクト側の検証ソース

`PosingSystemConversionValidationTests.cs` は、UnisakiStudioTools の既存 `Assets/Tests/Editor/PosingSystemEditorTests.asmdef` 配下で実行した検証ソースの保存用コピーです。Assets 側は Git 対象外のため、今回修正した PS002／アバター外の呼出し拒否と非変更検証をここへ記録しています。Tools 配下のコピーは Unity に自動コンパイルされません。

この開発環境では `Assets/Tests/Editor/PosingSystemConversionValidationTests.cs` に同一内容を反映済みです。別環境へ移す場合は既存の Editor テストアセンブリへ配置し、同じテストを二重に配置しないでください。製品パッケージ自身のテストは `Packages/jp.unisakistudio.posingsystem/Tests/Editor` にあります。