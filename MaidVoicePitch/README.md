# 声の音程を変えるUnityInjector用プラグイン

※ 動作には「拡張セーブデータパッチ CM3D2.ExternalSaveData.Patcher」が必要です ※

声の音程やスライダーの拡張などを行います


## 動作確認と操作方法

 - コンパイル後 C:\KISS\CM3D2_KAIZOU\CM3D2x64.exe を起動して、動作を確認してください
 - 「F4」キーを押すと、フリーコメント欄の内容が反映されます。記述方法は従来どおりです
 - 改造版の CM3D2.AddModsSlider.Plugin と併用するのを推奨します


## 拡張セーブデータに追加されるデータ

拡張セーブデータ内の `/savedata/maids/maid/plugins/plugin[@name = "CM3D2.MaidVoicePitch"]/prop` に保存されます。
タグの構成は以下のようになります

```
<savedata target="...">
  <maids>
    <maid guid="..." lastname="..." firstname="..." createtime="...">
      <plugins>
        <plugin name="CM3D2.MaidVoicePitch">
          <prop name="名前その１" value="値その１" />
          <prop name="名前その２" value="値その２" />
          ...
        </plugin>
```

「名前」と「値」の一覧は以下の通りです

 - 「XMLファイル名」は、"test.xml" を指定した場合 "C:\KISS\CM3D2_KAIZOU\test.xml" を読み込みます

| 名前                    | 値            | 既定値 | 概要 |
| ------                  | ------        | ------ | ---- |
| PITCH                   | -1.0～1.0     | 0      | 声のピッチを変更。+0.05なら5%速く、-0.16なら16%遅く再生 |
| SLIDER_TEMPLATE         | XMLファイル名 |（なし）| エディット画面のスライダーの限界値設定ファイルを指定 |
| FACE_SCRIPT_TEMPLATE    | XMLファイル名 |（なし）| スクリプト内の@Faceと@FaceBlendによる表情指定を置き換えるファイルを指定 |
| PROPSET_OFF             | True/False    | False  | Trueなら、スクリプト内の@PropSetによる首や肩スライダーの補正を抑制 |
| FACE_OFF                | True/False    | False  | Trueなら、スクリプト内の@Faceによる表情変化を抑制 |
| FACEBLEND_OFF           | True/False    | False  | Trueなら、スクリプト内の@FaceBlendによる頬と涙の表情変化を抑制 |
| EYETOCAMERA_OFF         | True/False    | False  | Trueなら、スクリプト内の@EyeToCameraによる視線指定を抑制 |
| MUHYOU                  | True/False    | False  | Trueなら、全ての表情変化を抑制 |
| HYOUJOU_OFF             | True/False    | False  | Trueなら、目と口の表情変化を抑制 |
| LIPSYNC_OFF             | True/False    | False  | Trueなら、リップシンク(口パク)を抑制 |
| LIPSYNC_INTENISTY       | True/False    | False  | Trueなら、リップシンク(口パク)強度設定を行う |
| LIPSYNC_INTENISTY.value | 0.0～1.0      | 1      | リップシンク(口パク)強度。1.0で最大、0.0で最小 |
| EYETOCAM                | -1,0,1        | 0      | 視線方向制御。1:常にカメラを向く、-1:常にカメラを向かない、0:スクリプトに従う |
| HEADTOCAM               | -1,0,1        | 0      | 顔方向制御。1:常にカメラを向く、-1:常にカメラを向かない、0:スクリプトに従う |
| MABATAKI                | 0.0～1.0      | 1      | 目の開度。1.0で最大(目を開く)、0.0で最小(目を閉じる) |
| EYEBALL.width           | 0.0～5.0      | 1      | 瞳の幅。1.0で通常、0.0で最小 |
| EYEBALL.height          | 0.0～5.0      | 1      | 瞳の高さ。1.0で通常、0.0で最小 |
| FARMFIX                 | True/False    | False  | Trueなら、前腕補正処理を行い、前腕が細くなるのを抑制する |
| WIDESLIDER              | True/False    | False  | Trueなら、拡張スライダー処理を行い、スライダーの限界値を大きくする |
| EYE_RATIO               | 0.1～10.0     | 1      | 目の縦横比。1.0で通常 |
| EYE_ANG.angle           | -60.0～+60.0  | 0      | 目の角度 |
| EYE_ANG.x               | -999～+999    | 0      | 目の回転中心位置の補正値(1) |
| EYE_ANG.y               | -999～+999    | 0      | 〃 (2) |
| THISCL.depth            | 0.1～5.0      | 1      | 足全体のスケーリング(奥行き) |
| THISCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| THIPOS.x                | -999～+999    | 0      | 足の位置 (左右) |
| THIPOS.z                | -999～+999    | 0      | 〃  (前後) |
| PELSCL.height           | 0.1～5.0      | 1      | 骨盤スケーリング (高さ) |
| PELSCL.depth            | 0.1～5.0      | 1      | 〃 (奥行き) |
| PELSCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| SKTSCL.height           | 0.1～5.0      | 1      | スカート周辺スケーリング (高さ) |
| SKTSCL.depth            | 0.1～5.0      | 1      | 〃 (奥行き) |
| SKTSCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| SPISCL.height           | 0.1～5.0      | 1      | 胴(下腹部)スケーリング (高さ) |
| SPISCL.depth            | 0.1～5.0      | 1      | 〃 (奥行き) |
| SPISCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| S0ASCL.height           | 0.1～5.0      | 1      | 胴(腹部)スケーリング (高さ) |
| S0ASCL.depth            | 0.1～5.0      | 1      | 〃 (奥行き) |
| S0ASCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| S1_SCL.height           | 0.1～5.0      | 1      | 胴(みぞおち)スケーリング (高さ) |
| S1_SCL.depth            | 0.1～5.0      | 1      | 〃 (奥行き) |
| S1_SCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| S1ASCL.height           | 0.1～5.0      | 1      | 胴(肋骨)スケーリング (高さ) |
| S1ASCL.depth            | 0.1～5.0      | 1      | 〃 (奥行き) |
| S1ASCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| HEAD_TRACK              | True/False    | False  | 顔と目の追従動作の改善 |
| HEAD_TRACK.lateral      |  0.0～90.0    | 30     | 顔の横方向可動角度 |
| HEAD_TRACK.above        |-90.0～90.0    | 30     | 〃上方向〃 |
| HEAD_TRACK.below        |-90.0～90.0    | 15     | 〃下方向〃 |
| HEAD_TRACK.behind       |  0.0～180.0   | 90     | 顔が追従をあきらめる角度 |
| HEAD_TRACK.speed        |  0.0～1.0     |  0.04  | 顔の回転速度 |
| HEAD_TRACK.ofsx         |-90.0～90.0    |  0     | 横回転オフセット (負で右、正で左を向く) |
| HEAD_TRACK.ofsy         |-90.0～90.0    |  0     | 縦回転オフセット (負で下、正で上を向く) |
| HEAD_TRACK.ofsz         |-90.0～90.0    |  0     | 回転オフセット (負で右、正で左にかしげる) |
| EYE_TRACK.inside        |  0.0～90.0    | 35     | 目の内側可動角度 |
| EYE_TRACK.outside       |  0.0～90.0    | 65     | 〃外側〃 |
| EYE_TRACK.above         |-90.0～90.0    | 10     | 〃上方向〃 |
| EYE_TRACK.below         |-90.0～90.0    | 20     | 〃下方向〃 |
| EYE_TRACK.behind        |  0.0～180.0   | 90     | 目が追従をあきらめる角度 |
| EYE_TRACK.speed         |  0.0～1.0     |  0.07  | 目の回転速度 |
| EYE_TRACK.ofsx          |-90.0～90.0    |  0     | 横オフセット (負で寄り目、正で離れ目) |
| EYE_TRACK.ofsy          |-90.0～90.0    |  0     | 縦オフセット (負で下、正で上) |


## SLIDER_TEMPLATEで指定するXMLファイルの書式

以下のような書式で指定します

 - `<slider>`タグ内の属性は`<slider name="セーブファイル内のパラメーター名" min="最小値" max="最大値"/>`となります

```
<?xml version="1.0" encoding="utf-8"?>
<!-- その１>>804、その２>>12 さんの値 -->
<slidertemplate>
  <sliders>
   <slider name="EyeScl"     min="-100" max="200"/>
   <slider name="EyePosX"    min="-100" max="130"/>
   <slider name="EyePosY"    min="-100" max="200"/>
   <slider name="DouPer"     min="-100" max="150"/>
   <slider name="sintyou"    min="-200" max="100"/>
   <slider name="MuneL"      min="0"    max="195"/>
   <slider name="MuneUpDown" min="-50"  max="300"/>
   <slider name="MuneYori"   min="-50"  max="200"/>
   <slider name="West"       min="-30"  max="100"/>
   <slider name="Hara"       min="0"    max="200"/>
   <slider name="kata"       min="-200" max="100"/>
   <slider name="KubiScl"    min="0"    max="200"/>
   <slider name="koshi"      min="-160" max="200"/>
  </sliders>
</slidertemplate>
```


## FACE_SCRIPT_TEMPLATEで指定するXMLファイルの書式

以下のような書式で指定します

 - keyが元の表情名、valueが置き換えた表情名です

```
<?xml version="1.0" encoding="utf-8"?>
<facescripttemplate>
  <faceblends>
    <faceblend key="通常"           value="通常"           />
    <faceblend key="びっくり"       value="びっくり"       />
    <faceblend key="目を見開いて"   value="目を見開いて"   />
    <faceblend key="発情"           value="発情"           />
    <faceblend key="頬０涙０"       value="頬０涙０"       />
    <faceblend key="頬０涙１"       value="頬０涙１"       />
    <faceblend key="頬０涙２"       value="頬０涙２"       />
    <faceblend key="頬０涙３"       value="頬０涙３"       />
    <faceblend key="頬１涙０"       value="頬１涙０"       />
    <faceblend key="頬１涙１"       value="頬１涙１"       />
    <faceblend key="頬１涙２"       value="頬１涙２"       />
    <faceblend key="頬１涙３"       value="頬１涙３"       />
    <faceblend key="頬２涙０"       value="頬２涙０"       />
    <faceblend key="頬２涙１"       value="頬２涙１"       />
    <faceblend key="頬２涙２"       value="頬２涙２"       />
    <faceblend key="頬２涙３"       value="頬２涙３"       />
    <faceblend key="頬３涙０"       value="頬３涙０"       />
    <faceblend key="頬３涙０よだれ" value="頬３涙０よだれ" />
    <faceblend key="頬３涙１"       value="頬３涙１"       />
    <faceblend key="頬３涙１よだれ" value="頬３涙１よだれ" />
    <faceblend key="頬３涙２"       value="頬３涙２"       />
    <faceblend key="頬３涙２よだれ" value="頬３涙２よだれ" />
    <faceblend key="頬３涙３"       value="頬３涙３"       />
    <faceblend key="頬３涙３よだれ" value="頬３涙３よだれ" />
  </faceblends>
  <faces>
    <face key="あーん"             value="あーん"             />
    <face key="きょとん"           value="きょとん"           />
    <face key="ためいき"           value="ためいき"           />
    <face key="にっこり"           value="にっこり"           />
    <face key="びっくり"           value="びっくり"           />
    <face key="ぷんすか"           value="ぷんすか"           />
    <face key="まぶたギュ"         value="まぶたギュ"         />
    <face key="むー"               value="むー"               />
    <face key="エロフェラ愛情"     value="エロフェラ愛情"     />
    <face key="エロフェラ快楽"     value="エロフェラ快楽"     />
    <face key="エロフェラ嫌悪"     value="エロフェラ嫌悪"     />
    <face key="エロフェラ通常"     value="エロフェラ通常"     />
    <face key="エロメソ泣き"       value="エロメソ泣き"       />
    <face key="エロ愛情２"         value="エロ愛情２"         />
    <face key="エロ我慢１"         value="エロ我慢１"         />
    <face key="エロ我慢２"         value="エロ我慢２"         />
    <face key="エロ我慢３"         value="エロ我慢３"         />
    <face key="エロ期待"           value="エロ期待"           />
    <face key="エロ怯え"           value="エロ怯え"           />
    <face key="エロ興通常３"       value="エロ興通常３"       />
    <face key="エロ興奮０"         value="エロ興奮０"         />
    <face key="エロ興奮１"         value="エロ興奮１"         />
    <face key="エロ興奮２"         value="エロ興奮２"         />
    <face key="エロ興奮３"         value="エロ興奮３"         />
    <face key="エロ緊張"           value="エロ緊張"           />
    <face key="エロ嫌悪１"         value="エロ嫌悪１"         />
    <face key="エロ好感１"         value="エロ好感１"         />
    <face key="エロ好感２"         value="エロ好感２"         />
    <face key="エロ好感３"         value="エロ好感３"         />
    <face key="エロ絶頂"           value="エロ絶頂"           />
    <face key="エロ舌責"           value="エロ舌責"           />
    <face key="エロ舌責快楽"       value="エロ舌責快楽"       />
    <face key="エロ痛み１"         value="エロ痛み１"         />
    <face key="エロ痛み２"         value="エロ痛み２"         />
    <face key="エロ痛み３"         value="エロ痛み３"         />
    <face key="エロ痛み我慢"       value="エロ痛み我慢"       />
    <face key="エロ痛み我慢２"     value="エロ痛み我慢２"     />
    <face key="エロ痛み我慢３"     value="エロ痛み我慢３"     />
    <face key="エロ通常１"         value="エロ通常１"         />
    <face key="エロ通常２"         value="エロ通常２"         />
    <face key="エロ通常３"         value="エロ通常３"         />
    <face key="エロ放心"           value="エロ放心"           />
    <face key="エロ羞恥１"         value="エロ羞恥１"         />
    <face key="エロ羞恥２"         value="エロ羞恥２"         />
    <face key="エロ羞恥３"         value="エロ羞恥３"         />
    <face key="エロ舐め愛情"       value="エロ舐め愛情"       />
    <face key="エロ舐め愛情２"     value="エロ舐め愛情２"     />
    <face key="エロ舐め快楽"       value="エロ舐め快楽"       />
    <face key="エロ舐め快楽２"     value="エロ舐め快楽２"     />
    <face key="エロ舐め嫌悪"       value="エロ舐め嫌悪"       />
    <face key="エロ舐め通常"       value="エロ舐め通常"       />
    <face key="ジト目"             value="ジト目"             />
    <face key="ダンスウインク"     value="ダンスウインク"     />
    <face key="ダンスキス"         value="ダンスキス"         />
    <face key="ダンスジト目"       value="ダンスジト目"       />
    <face key="ダンス困り顔"       value="ダンス困り顔"       />
    <face key="ダンス真剣"         value="ダンス真剣"         />
    <face key="ダンス微笑み"       value="ダンス微笑み"       />
    <face key="ダンス目とじ"       value="ダンス目とじ"       />
    <face key="ダンス憂い"         value="ダンス憂い"         />
    <face key="ダンス誘惑"         value="ダンス誘惑"         />
    <face key="ドヤ顔"             value="ドヤ顔"             />
    <face key="引きつり笑顔"       value="引きつり笑顔"       />
    <face key="疑問"               value="疑問"               />
    <face key="泣き"               value="泣き"               />
    <face key="居眠り安眠"         value="居眠り安眠"         />
    <face key="興奮射精後１"       value="興奮射精後１"       />
    <face key="興奮射精後２"       value="興奮射精後２"       />
    <face key="苦笑い"             value="苦笑い"             />
    <face key="困った"             value="困った"             />
    <face key="思案伏せ目"         value="思案伏せ目"         />
    <face key="少し怒り"           value="少し怒り"           />
    <face key="照れ"               value="照れ"               />
    <face key="照れ叫び"           value="照れ叫び"           />
    <face key="笑顔"               value="笑顔"               />
    <face key="接吻"               value="接吻"               />
    <face key="絶頂射精後１"       value="絶頂射精後１"       />
    <face key="絶頂射精後２"       value="絶頂射精後２"       />
    <face key="恥ずかしい"         value="恥ずかしい"         />
    <face key="痛み３"             value="痛み３"             />
    <face key="痛みで目を見開いて" value="痛みで目を見開いて" />
    <face key="通常"               value="通常"               />
    <face key="通常射精後１"       value="通常射精後１"       />
    <face key="通常射精後２"       value="通常射精後２"       />
    <face key="怒り"               value="怒り"               />
    <face key="発情"               value="発情"               />
    <face key="悲しみ２"           value="悲しみ２"           />
    <face key="微笑み"             value="微笑み"             />
    <face key="閉じフェラ愛情"     value="閉じフェラ愛情"     />
    <face key="閉じフェラ快楽"     value="閉じフェラ快楽"     />
    <face key="閉じフェラ嫌悪"     value="閉じフェラ嫌悪"     />
    <face key="閉じフェラ通常"     value="閉じフェラ通常"     />
    <face key="閉じ目"             value="閉じ目"             />
    <face key="閉じ舐め愛情"       value="閉じ舐め愛情"       />
    <face key="閉じ舐め快楽"       value="閉じ舐め快楽"       />
    <face key="閉じ舐め快楽２"     value="閉じ舐め快楽２"     />
    <face key="閉じ舐め嫌悪"       value="閉じ舐め嫌悪"       />
    <face key="閉じ舐め通常"       value="閉じ舐め通常"       />
    <face key="頬１涙０"           value="頬１涙０"           />
    <face key="目を見開いて"       value="目を見開いて"       />
    <face key="目口閉じ"           value="目口閉じ"           />
    <face key="優しさ"             value="優しさ"             />
    <face key="誘惑"               value="誘惑"               />
    <face key="余韻弱"             value="余韻弱"             />
    <face key="拗ね"               value="拗ね"               />
  </faces>
</facescripttemplate>
```


## コンパイル方法

config.batの設定を行った後、このディレクトリの compile.bat を実行することでコンパイルができます


## 履歴

 - 0.2.8
   - ２人目以降のメイドに対してWIDESLIDERが正常に動作していなかったのを修正 (その３>>263, >>274, >>276, >>277)
   - 同、スライダーテンプレートが正常に設定されていなかったのを修正
 - 0.2.7
   - AddModsSlider用の[スライダーコールバック名をMaidVoicePitch_UpdateSlidersに変更](https://github.com/CM3D2-01/CM3D2.AddModsSlider.Plugin/compare/decf77137a...37e285d14f)
   - デフォルトのスライダーテンプレートを追加 (その３>>223, >>226)
   - FARMFIX等のトグル操作がその場で反映されないのを修正 (その３>>228)
 - 0.2.6
   - AddModsSlider用のスライダーコールバックUpdateSlidersを追加
 - 0.2.5
   - 顔がモーションに追従している場合はオフセット角度をつけないように修正 (その２>>41)
 - 0.2.4
   - README.mdに拡張セーブデータ、SLIDER_TEMPLATE、FACE_SCRIPT_TEMPLATEの説明を追加
   - 毎フレーム更新する対象を削減
   - いくつかのパラメーターを廃止
     - FACE_ANIME_SPEED (表情アニメーション速度) 廃止
     - MABATAKI_SPEED (まばたき速度) 廃止
     - PELVIS (骨盤部と衣装のコリジョン) 廃止 (PELVIS, PELVIS.x, PELVIS.y, PELVIS.z)
   - いくつかのパラメーターの動作を変更
     - AddModsSlider 0.1.0.7 で LIPSYNC が正常に動作していなかったため、仕様を変更
     - EYE_ANG.angleで目の角度を変えた際、眼球の角度の補正を追加
   - 顔と目の動作改善を追加
     - 顔の制御 HEAD_TRACK { .lateral, .above, .below, .behind, .speed, .ofsx, .ofsy }
       - HEAD_TRACK.lateral : 左右の角度制限
       - HEAD_TRACK.above : 上方の角度制限
       - HEAD_TRACK.below : 下方の角度制限
       - HEAD_TRACK.behind : 追従をあきらめる角度
       - HEAD_TRACK.speed : 追従速度
     - 目の制御 EYE_TRACK { .inside, .outside, .above, .below, .behind, .speed, .ofsx, .ofsy }
       - EYE_TRACK.inside : 内側の角度制限
       - EYE_TRACK.outside : 外側の角度制限
       - EYE_TRACK.above : 上方の角度制限
       - EYE_TRACK.below : 下方の角度制限
       - EYE_TRACK.behind : 追従をあきらめる角度
       - EYE_TRACK.speed : 追従速度
       - EYE_TRACK.ofsx : 横方向の中央位置の補正値 (負の値でより目、0でデフォルト、正の値で離れ目)
       - EYE_TRACK.speed : 縦方向の中央位置の補正値 (負の値で下、0でデフォルト、正の値で上)
  - ModsParam.xml への追加例
```
<mod id="HEAD_TRACK" description="顔の追従処理の改善">
    <value prop_name="HEAD_TRACK.lateral" min="1"   max="90"  label="横角"   type="num" />
    <value prop_name="HEAD_TRACK.above"   min="1"   max="90"  label="上角"   type="num" />
    <value prop_name="HEAD_TRACK.below"   min="1"   max="90"  label="下角"   type="num" />
    <value prop_name="HEAD_TRACK.behind"  min="1"   max="180" label="無視角" type="num" />
    <value prop_name="HEAD_TRACK.speed"   min="0"   max="0.3" label="速度"   type="num" />
    <value prop_name="HEAD_TRACK.ofsx"    min="-30" max="30"  label="横オ"   type="num" />
    <value prop_name="HEAD_TRACK.ofsy"    min="-30" max="30"  label="縦オ"   type="num" />
    <value prop_name="HEAD_TRACK.ofsz"    min="-30" max="30"  label="回オ"   type="num" />
</mod>

<mod id="EYE_TRACK" description="目の追従処理の改善">
    <value prop_name="EYE_TRACK.outside" min="1"   max="90"  label="内角"   type="num" />
    <value prop_name="EYE_TRACK.inside"  min="1"   max="90"  label="外角"   type="num" />
    <value prop_name="EYE_TRACK.above"   min="-10" max="90"  label="上角"   type="num" />
    <value prop_name="EYE_TRACK.below"   min="-10" max="90"  label="下角"   type="num" />
    <value prop_name="EYE_TRACK.behind"  min="1"   max="180" label="無視角" type="num" />
    <value prop_name="EYE_TRACK.speed"   min="0"   max="0.3" label="速度"   type="num" />
    <value prop_name="EYE_TRACK.ofsx"    min="-10" max="10"  label="横オ"   type="num" />
    <value prop_name="EYE_TRACK.ofsy"    min="-5"  max="5"   label="縦オ"   type="num" />
</mod>
```
 - 0.2.7
   - デフォルトのスライダーテンプレートファイル "UnityInjector/Config/MaidVoicePitchSlider.xml" を追加
      - スライダーテンプレートが指定されていない場合、このファイルを用いるようになります
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
