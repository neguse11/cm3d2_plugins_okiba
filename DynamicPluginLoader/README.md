# 動的なコードのロードを可能にするUnityInjector用プラグイン

## 「カスタムメイド3D2攻略wiki」から来た方へ

[導入の説明](../INSTALL.md)を読んでください。このページに、あなたの欲しいものは無いです


## 概要

 - プラグイン開発者向けです
 - ゲームを再起動することなく、プラグインを再読み込みします
    - 実験を繰り返す場合に便利かもしれません
 - ゲーム実行中に UnityInjector\DynamicPlugins\ 下に DynamicPluginBase を継承したクラスを持つ DLL をコピーすると、自動的に読み込み、実行を行います
    - 同じ DLL を更新した場合、ゲーム内で動作しているインスタンスを停止し、新しい DLL を再読み込みします


## コンパイル

[config.batの設定](../INSTALL.md)を行った後、このディレクトリの compile.bat を実行することでコンパイルができます


## 動作確認

 - compile.bat を実行し、プラグインを生成
 - ゲームを起動し、タイトル画面まで待つ
 - 起動後、Example\compile.bat を実行
    - ゲーム画面左上に「DynamicPluginExample」の文字が出ることを確認
 - Example\DynamicPluginExample.cs の OnGUI() 内のコードを
```
var str = string.Format("{0} : counter={1}", GetType().Name, counter);
```

から

```
var str = string.Format("HOGE {0} : counter={1}", GetType().Name, counter);
```

に変更し、ゲームは起動したままで、再び Example\compile.bat を実行

 - ゲーム画面左上に「HOGE」の文字が追加されたことを確認


## 制限

 - ダイナミックプラグインに対して、デバッガが動作しません :(
    - mdb を読ませる方法が分かりません
 - DLLをアンロードするために、プラグインごとに異なる AppDomain で動作します
    - このため GetType(string) 等が動作しません
    - 動作させたい場合は DynamicPluginBase.PrimaryAppDomain 経由で実行してください
 - UnityInjectorBase を継承しているように見せかけていますが、実際の動作は異なります
    - プラグインは異なる AppDomain で動作するため、GameObject への Component ではなく、単なる MonoBehaviour のインスタンスとして動作します
    - 例えば Update は DynamicPluginManger.Update がプラグイン単位で明示的に実行しています
    - このため「MonoBehaviourなら本来こうなるはず」と期待する動作が行えない場合が多々あります
    - 対応できるものは追加して対応しようと思うので、必要なものがあれば教えてください


## 履歴

 - 0.1.0
    - 最初の版
