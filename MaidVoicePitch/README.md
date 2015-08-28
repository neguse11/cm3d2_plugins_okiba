# 声の音程を変えるUnityInjector用プラグイン

※ 動作には「拡張セーブデータパッチ CM3D2.ExternalSaveData.Patcher」が必要です ※

声の音程やスライダーの拡張などを行います


## 動作確認と操作方法

 - コンパイル後 C:\KISS\CM3D2_KAIZOU\CM3D2x64.exe を起動して、動作を確認してください
 - F4」キーを押すと、フリーコメント欄の内容が反映されます。記述方法は従来どおりです
 - 改造版の CM3D2.AddModsSlider.Plugin と併用するのを推奨します


## 履歴

 - 0.2.3
   - 使用しなくなった拡張セーブデータを削除する処理を追加
 - 0.2.2
   - @Faceコマンドを無視する「FACE_OFF」を追加 (Trueで無視、Falseで通常通り) (その２>>433)
   - @FaceBlendコマンドを無視する「FACEBLEND_OFF」を追加 (Trueで無視、Falseで通常通り) (その２>>433)
   - 「SLIDER_TEMPLATE」の読み込みをしていなかったのを修正 (その２>>504, >>508)
   - 「*.enable」系の名称を変更。対象は "PELVIS.enable", "FARMFIX.enable", "SPISCL.enable", "S0ASCL.enable", "S1_SCL.enable", "S1ASCL.enable" (その２>>509)
 - 0.2.1
   - 骨盤コリジョンスイッチ "PELVIS.enable" を追加
   - 前腕のゆがみ修正 "FARMFIX.enable" を追加 (その２>>89, ForeArmFix)
   - 胴スケール指定 "SPISCL.*", "S0ASCL.*", "S1_SCL.*", "S1ASCL.*" を追加 (*はenable, width, depth, height) (その２>>90, SpineScale)
   - スカートスケール指定 "SKTSCL.*" を追加 (*はenable, width, depth, height) (その２>>218)
 - 0.2.0
   - 拡張セーブデータに移行
 - 0.1.11
    - EYETOCAM#" が指定されていない場合に、瞳が動かなくなるバグを修正 (その２>>216, >>217, >>219)
 - 0.1.10
    - PROPSET_OFF#" 等が正常に動作しないときがあるのを修正 (その２>>89, >>96, >>141)
 - 0.1.9
   - - スケール指定 "#TEST_PELSCL=W.WW,D.DD,H.HH#" を追加 (テスト中) (その１>>961, >>970)
   - 脚部スケール指定 "#TEST_THISCL=W.WW,D.DD#" を追加 (テスト中) (その１>>961, >>970)
   - 股関節位置指定 "#TEST_THIPOS=X.XX,Z.ZZ#" を追加 (テスト中) (その１>>961, >>970)
 - 0.1.8
    - EyeToCameraを無視する "#TEST_EYETOCAMERA_OFF#" を追加 (テスト中) (その1>>697)
    - スライダー範囲指定 "#TEST_SLIDER_TEMPLATE=filename#" を追加 (テスト中) (その１>>804, >>807, その２>>11, >>12)
    - 目の縦横比変更 "#TEST_EYE_RATIO=N.NN#" を追加 (その1>>923, EyeScaleRotate)
    - 目の角度変更 "#TEST_EYE_ANG=N.NN,X.XX,Y.YY#" を追加 (その1>>923, EyeScaleRotate)
 - 0.1.7
    - 版でも動作するように修正 (その1>>592)
    - スライダー範囲拡大の名称を "#WIDESLIDER#" に変更
    - @PropSet無視の名称を "#PROPSET_OFF#" に変更
    - まばたき範囲指定を "#MABATAKI=N.NN#" に仕様変更
    - 瞳サイズ変更 "#EYEBALL=N.NN,M.MM#" を追加
    - リップシンク強度指定 "#TEST_LIPSYNC=N.NN#" を追加 (テスト中) (その1>>624)
    - まばたき速度指定 "#TEST_MABATAKI_SPEED=N.NN#" を追加 (テスト中)
    - 表情変化速度指定 "#TEST_FACE_ANIME_SPEED=N.NN#" を追加 (テスト中)
    - 骨盤部コリジョン指定 "#TEST_PELVIS=X.XX,Y.YY,Z.ZZ#" を追加 (テスト中)
    - 表情テンプレート指定 "#TEST_FACE_SCRIPT_TEMPLATE=filename# を追加 (テスト中)
 - 0.1.6
   - コンパイルバグ修正その２ (その1>>541)
 - 0.1.5
   - バグ修正
 - 0.1.4
   - 夜伽スクリプトでの@PropSetの動作を制限する指定 "#TEST_PROPSET_OFF#" を追加 (その1>>466)
 - 0.1.3
   - UnityInjectorベースに変更 (その1>>436)
   - まばたき範囲指定 "#MABATAKI=N.NN,M.MM#" を追加
   - 表情変化オフ指定 "#HYOUJOU_OFF#" を追加
   - リップシンクオフ指定 "#LIPSYNC_OFF#" を追加
   - 目をカメラに向ける指定 "#EYETOCAM#" を追加
   - 顔をカメラに向ける指定 "#HEADTOCAM#" を追加
 - 0.1.2
   - 説明を修正 (その1>>391)
   - ピッチ指定を "#PITCH=N.NN#" に変更
   - 無表情指定 "#MUHYOU#" を追加
 - 0.1.1
   - 説明を修正 (その1>>306)
   - フリーコメント欄が空の場合に正常に動作していなかったのを修正 (その1>>308)
 - 0.1.0
   - 最初の版
