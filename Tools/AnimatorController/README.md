# AnimatorControllerのコード定義・検証

仕様・要件・測定結果は `docs/animator-controller-master.md`、現行全レイヤーの記録は `docs/animator-controller-reference.md`。

製品のC#生成器はUnity Editorで動作する。ここにあるPythonは開発・移行用で、製品利用者には不要。Python 3.10とPyYAMLが必要。環境へ自動インストールはしない。

## 基準テンプレートを更新する場合

1. `Packages/jp.unisakistudio.posingsystem/Resources/PosingSystem_Locomotion.controller` の変更をレビューする。
2. リポジトリ直下から次のコマンドでC#定義と参照付録を更新する。
3. 基準SHA-256と個数を固定するPythonテストの期待値を、変更理由とともに更新する。未知のフィールド・SDK型を無条件に許可しない。
4. 等価性、保存、選択、固定、同期、NDMF統合、既存回帰試験を実行する。

```powershell
python Tools/AnimatorController/export_definition.py --template Packages/jp.unisakistudio.posingsystem/Resources/PosingSystem_Locomotion.controller --output Packages/jp.unisakistudio.posingsystem/Editor/PosingSystemControllerDefinition.Generated.cs
python Tools/AnimatorController/export_specification.py
python -m unittest discover -s Tools/AnimatorController -p 'test_*.py' -v
```

生成内容を変更せず確認する場合は最初のコマンドに `--check` を付ける。自動生成定義への直接編集ではなく、最適化処理は `PosingSystemControllerOptimization.cs` へ記述する。

## Unity試験

この開発プロジェクト用の `run-tests.ps1` は、既にUnityが起動していれば停止し、出力の上書きも拒否する。実行前にTemp/Library内の危険なリンクを検査する。起動中のバッチUnityに対してC#・Assets・Packagesを編集しない。

```powershell
./Tools/AnimatorController/run-tests.ps1 -Run unique-run-name -Filter 'PosingSystemControllerCompilerTests|PosingSystemControllerSelectionTests|Compiler_'
```

Unity 2022.3.22f1とホストPythonの場所はスクリプト内のこのPC用設定。別PCではその場所を明示的に変更する。`-Filter jp.unisakistudio.posingsystemeditor.tests` は既存の回帰試験も対象とし、専用試験より時間がかかる。XMLの完了結果を判定し、ログが進行しただけで成功にしない。

比較コピーのUnityメニューは `Tools > ゆにさきスタジオ > 実験 > 選択アバターの最適化比較コピーを作成`。元アバターとアップロードIDを保護し、比較コピーをコード生成した共通Controllerでプレビルドする。通常ビルドへの一括切替は行わない。

## 開発コピーと正本

本開発プロジェクトの6商品Packagesは実体コピー。VPM正本へ自動同期しない。反映前に、差分対象の正本が他の作業で変わっていないかハッシュを照合する。

`recover_package_sources.py` は2026-10-27の復旧専用記録であり、通常の同期・更新には使用しない。復旧は完了済み。Temp/Libraryへ生きたJunctionやsymlinkを退避しない。詳細は `docs/package-source-recovery-2026-10-27.md`。
