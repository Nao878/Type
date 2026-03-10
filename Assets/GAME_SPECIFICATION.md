# ゲーム仕様書 — Typing RPG

> **最終更新日**: 2026-03-10
> **バージョン**: 1.0.0
> **プロジェクト名**: Type (Typing RPG)
> **エンジン**: Unity (C#)

---

## 📋 更新ルール（AI向け）

この仕様書はゲームシステムのソースコードに基づいています。
**ゲームのシステムが更新された場合、該当セクションのみを上書き更新してください。**

- 新規セクション追加時は既存セクションと重複しないよう確認してください。
- 「最終更新日」と「バージョン」を必ず更新してください。
- 各セクションはアトミック（独立）な単位で記述されているため、必要な箇所だけを変更してください。
- 削除された機能は該当セクションごと削除してください。
- **変更履歴** セクションに変更内容の概要を追記してください。

---

## 1. ゲーム概要

### 1.1 ジャンル
**タイピングRPG** — リアルタイムバトル形式のタイピングゲーム。

### 1.2 コンセプト
プレイヤーは英単語をタイピングすることで、味方パーティのスキルを発動させ、敵を倒すことを目指す。タイピングの速さと正確さがバトルの勝敗を左右するリアルタイムバトルシステム。

### 1.3 プラットフォーム
PC（キーボード入力必須）

---

## 2. ゲームフロー

### 2.1 シーン構成

| シーン名 | 説明 |
|---------|------|
| `TitleScene` | タイトル画面。ボタン押下で `MainScene` へ遷移 |
| `MainScene`（※未配置、SceneSetup エディタで構築） | メインバトル画面 |

### 2.2 ゲーム進行フロー

```
[タイトル画面] → [バトル開始] → [リアルタイムバトル] → [勝利 or ゲームオーバー]
```

1. タイトル画面でボタンを押してバトルシーンに遷移
2. バトル開始：味方4人 vs 敵1体のリアルタイムバトル
3. プレイヤーは英単語をタイピングしてスキルを発動
4. 敵は一定間隔で自動攻撃
5. 敵HP=0 で勝利、味方全滅でゲームオーバー

---

## 3. キャラクターシステム

### 3.1 味方パーティ

- パーティ人数: **4人固定**
- 各キャラクターは `ScriptableObject`（`CharacterData`）で定義

| パラメータ | 初期値 | 説明 |
|-----------|--------|------|
| `characterName` | キャラ1〜4 | キャラクター名 |
| `maxHp` | 10 | 最大HP |
| `currentHP` | 10 | 現在HP（maxHPと同値で開始） |
| `isInvincible` | false | 無敵状態フラグ |
| `invincibilityTimer` | 0 | 無敵の残り時間（秒） |

#### 登録キャラクター（ScriptableObjectアセット）

| アセット名 | キャラ名 | 画像ファイル |
|-----------|---------|-------------|
| `1RingoCharacterData` | リンゴ | `GlassMan.jpg` |
| `2NitoCharacterData` | ニト | `Gentleman.jpg` |
| `3SyobonCharacterData` | ショボン | `CatGirl.jpg` |
| `4EnzyouCharacterData` | エンジョウ | `YellowGirl.jpg` |

#### 味方の状態

- **生存**: `currentHP > 0`
- **戦闘不能**: `currentHP <= 0`（UIでグレーアウト表示）
- **無敵**: `isInvincible = true` のとき被ダメージ無効

### 3.2 敵キャラクター

- 敵は **1体固定**（ロボット）
- `Enemy` クラスで管理

| パラメータ | 初期値 | 説明 |
|-----------|--------|------|
| `maxHP` | 50 | 最大HP |
| `currentHP` | 50 | 現在HP |
| `baseDamage` | 2 | 基本攻撃力 |
| `baseAttackInterval` | 10秒 | 基本攻撃間隔 |

#### 敵の攻撃ロジック

- **攻撃間隔**: `baseAttackInterval` の 80%〜120% のランダム間隔（8秒〜12秒）
- **ターゲット選択**: 生存中の味方からランダムに1人選択
- **ダメージ計算**: `baseDamage`（攻撃力減少デバフ適用時は10%減、最低1ダメージ）
- **次のターゲット**: 攻撃後に次の攻撃対象を事前決定（`future` スキルで確認可能）

---

## 4. バトルシステム

### 4.1 リアルタイムバトル

- ターン制ではなく **リアルタイム進行**
- 敵は自動で一定間隔攻撃
- プレイヤーは英単語をタイピングしてスキルを発動
- バトルは `Update()` ループで毎フレーム処理

### 4.2 勝利条件

- **勝利**: 敵のHP ≤ 0
- **ゲームオーバー**: 味方全員のHP ≤ 0（全滅）

### 4.3 バフ・デバフシステム

#### 味方側バフ

| バフ名 | 効果 | 管理変数 |
|--------|------|---------|
| 攻撃バフ | 全与ダメージ × 2倍 | `isBuffActive`, `buffTimer` |
| スピードバフ | タイピング判定緩和 | `isSpeedBuffActive`, `speedBuffTimer` |
| 無敵 | 被ダメージ無効（個別キャラ） | `PartyMember.isInvincible` |

#### 敵側デバフ（状態異常）

| デバフ名 | 効果 | 管理変数 |
|---------|------|---------|
| 毒 (Poison) | 1秒ごとに指定ダメージ | `isPoisoned`, `poisonTimer` |
| スロー (Slow) | 攻撃タイマー進行速度半減 | `isSlowed`, `slowTimer`, `slowMultiplier` |
| フリーズ (Freeze) | 攻撃タイマー完全停止 | `isFrozen`, `freezeTimer` |
| 攻撃力減少 (Reduce) | 攻撃力永続10%減（重複不可） | `isAttackReduced` |

### 4.4 ダメージ計算式

```
最終ダメージ = baseDamage
  × (isBuffActive ? buffDamageMultiplier : 1.0)
  × (applyBelieve && Random < 0.3 ? 3.0 : 1.0)
→ 四捨五入
```

---

## 5. タイピングシステム

### 5.1 入力方式

- キーボードからの **アルファベット入力のみ** 受付（`char.IsLetter`）
- 入力は自動的に **小文字に変換**
- **バックスペース** で1文字削除
- **Enter** でも入力確定（スキル発動チェック）

### 5.2 リアルタイムスキル発動

- 文字入力のたびに現在の入力文字列をスキル辞書と照合
- 辞書に一致する単語が完成した時点で **即座にスキル発動**（Enter不要）
- 発動後、入力文字列は自動クリア

### 5.3 スピードバフ

- `speed` スキル発動中はタイピング判定が緩和される（曖昧検索）
- 効果時間: 5秒

---

## 6. スキル一覧

スキルは英単語をタイピングすることで発動する。`SkillDatabase` で辞書管理されており、拡張可能。

### 6.1 基本スキル（4種）

| スキル名 | 英単語 | 効果 |
|---------|--------|------|
| アップル | `apple` | 最もHPが低い味方を **2回復** |
| ストップ | `stop` | **10秒間**、敵の攻撃タイマー進行速度 **0.5倍** |
| ポイズン | `poison` | 敵に毒付与（**10秒間、1秒ごとに1ダメージ**） |
| バフ | `buff` | **10秒間**、味方の全与ダメージ **2倍** |

### 6.2 追加スキル（15種）

| スキル名 | 英単語 | 効果 |
|---------|--------|------|
| プロテクト | `protect` | **5秒間**、ランダムな味方1人を**無敵化** |
| アタック | `attack` | 敵に基本ダメージ **3**（バフ適用） |
| スピード | `speed` | **5秒間**、タイピング判定緩和 |
| シェア | `share` | 生存パーティ全員のHPを**平均化** |
| イレース | `erase` | 敵の攻撃カウントダウンを**リセット** |
| フューチャー | `future` | 次に狙われる味方を**強調表示**（3秒間） |
| チェンジ | `change` | 敵の攻撃力をランダムに変更（1〜3）、または攻撃力減少をリセット |
| リデュース | `reduce` | 敵の攻撃力を永続 **10%減少**（重複不可） |
| アクティブ | `active` | 全継続中バフ/デバフの効果時間を **3秒延長** |
| ビリーブ | `believe` | 敵に3ダメージ + **30%の確率でダメージ3倍** |
| イグノア | `ignore` | 敵に防御無視 **固定5ダメージ** |
| サプライ | `supply` | 味方全員のHPを **1回復** |
| フリーズ | `freeze` | **3秒間**、敵の攻撃タイマー完全停止 |
| ディバイド | `divide` | 敵の現在HPの **10%分のダメージ**（最低1、バフ適用） |
| フィニッシュ | `finish` | 敵HPが **5以下（10%以下）なら即座に勝利** |

### 6.3 スキルの拡張

- `SkillDatabase.AddSkill(string word, Action skillAction)` メソッドで動的にスキル追加が可能
- 辞書型（`Dictionary<string, Action>`）で管理されており、新規スキルの追加が容易

---

## 7. UI構成

### 7.1 レイアウト概要

```
┌──────────────────────────────────────────────┐
│  [バフ表示：右上]                               │
│                                                │
│         ┌──────────┐                           │
│         │  敵キャラ  │                           │
│         │  HP バー   │                           │
│         │ 状態異常   │                           │
│         └──────────┘                           │
│                                                │
│  ┌──────── タイピング入力欄 ────────┐           │
│  │  [スキル発動表示]                 │           │
│  │  [現在の入力テキスト]             │           │
│  └────────────────────────────────┘           │
│                                                │
│  ┌────┐  ┌────┐  ┌────┐  ┌────┐             │
│  │ｷｬﾗ1│  │ｷｬﾗ2│  │ｷｬﾗ3│  │ｷｬﾗ4│             │
│  │HP  │  │HP  │  │HP  │  │HP  │             │
│  └────┘  └────┘  └────┘  └────┘             │
│                                                │
│  [ゲームオーバー / 勝利パネル：中央]             │
└──────────────────────────────────────────────┘
```

### 7.2 UI要素一覧

| カテゴリ | 要素 | 説明 |
|---------|------|------|
| 敵UI | `enemyImage` | 敵キャラクター画像 |
| 敵UI | `enemyHPBar` | HPバー（Filled Image） |
| 敵UI | `enemyHPText` | HP数値テキスト（例: `50/50`） |
| 敵UI | `poisonEffectIcon` | 毒状態アイコン（紫） |
| 敵UI | `freezeEffectIcon` | フリーズ状態アイコン（シアン） |
| 敵UI | `slowEffectIcon` | スロー状態アイコン（黄） |
| 味方UI | `partyMemberImages[0-3]` | 各キャラクター画像（戦闘不能時グレーアウト） |
| 味方UI | `partyHPBars[0-3]` | HPバー（HP30%以下で赤色表示） |
| 味方UI | `partyHPTexts[0-3]` | HP数値テキスト |
| 味方UI | `protectEffectIcons[0-3]` | 無敵状態アイコン（黄色） |
| 味方UI | `targetHighlights[0-3]` | 次ターゲットのハイライト表示（赤） |
| タイピング | `currentInputText` | 現在の入力文字列表示 |
| タイピング | `skillActivationText` | スキル発動テキスト（1.5秒表示） |
| バフ | `buffTimerText` | 攻撃バフ残り時間（オレンジ色） |
| バフ | `speedBuffTimerText` | スピードバフ残り時間（シアン色） |
| 終了 | `gameOverPanel` | ゲームオーバー画面（赤文字「GAME OVER」） |
| 終了 | `victoryPanel` | 勝利画面（緑文字「VICTORY!」） |

### 7.3 エフェクト表現

| エフェクト | 対象 | 内容 |
|-----------|------|------|
| ダメージフラッシュ | 味方キャラ画像 | 0.2秒間赤色フラッシュ |
| スキル発動表示 | 画面中央上 | スキル名を大文字で1.5秒間表示 |
| ターゲットハイライト | 味方キャラ | 次にターゲットされるキャラに赤い半透明オーバーレイ（3秒間） |

---

## 8. データ設計

### 8.1 ScriptableObject

| クラス名 | 用途 | アセットメニュー |
|---------|------|----------------|
| `CharacterData` | キャラクターの基本データ定義 | `Game/CharacterData` |
| `SkillSettings` | スキルパラメータの設定 | `Game/SkillSettings` |

### 8.2 SkillSettings パラメータ

| パラメータ | 型 | デフォルト値 | 説明 |
|-----------|---|------------|------|
| `appleHealAmount` | int | 20 | Appleスキルの回復量 |
| `poisonDamage` | int | 1 | 毒の1ティックあたりダメージ |
| `poisonInterval` | float | 1.0 | 毒のダメージ間隔（秒） |
| `poisonDuration` | float | 5.0 | 毒の持続時間（秒） |
| `stopDelayTime` | float | 2.0 | Stopスキルの遅延時間 |
| `debuffDuration` | float | 3.0 | デバフの持続時間（秒) |
| `debuffDamageMultiplier` | int | 2 | デバフのダメージ倍率 |

> **注意**: `SkillSettings` のパラメータは定義されているが、現在のスキル実装ではハードコーディングされた値が使用されている場合がある。将来的に `SkillSettings` を参照するよう統一することが推奨される。

---

## 9. スクリプト構成

### 9.1 ファイル一覧

| パス | クラス名 | 役割 |
|------|---------|------|
| `Assets/Scripts/GameManager.cs` | `GameManager`, `PartyMember` | ゲーム全体管理（シングルトン）、味方データ管理 |
| `Assets/Scripts/Enemy.cs` | `Enemy` | 敵キャラクター管理（HP、攻撃、状態異常） |
| `Assets/Scripts/SkillDatabase.cs` | `SkillDatabase` | スキル辞書管理、スキル発動ロジック |
| `Assets/Scripts/TypingController.cs` | `TypingController` | キーボード入力監視、スキル発動トリガー |
| `Assets/Scripts/UIManager.cs` | `UIManager` | UI要素管理、表示更新 |
| `Assets/Scripts/TitleScene/Title.cs` | `Title` | タイトル画面のシーン遷移処理 |
| `Assets/Scripts/Data/SkillSettings.cs` | `SkillSettings` | スキルパラメータ設定（ScriptableObject） |
| `Assets/Scripts/Data/CharaConfig/CharacterData.cs` | `CharacterData` | キャラクター基本データ定義（ScriptableObject） |
| `Assets/Scripts/Editor/SceneSetup.cs` | `SceneSetup` | エディタ用シーン自動構築ツール |

### 9.2 クラス依存関係

```
GameManager（シングルトン）
  ├── UIManager         → UI表示の更新
  ├── TypingController  → 入力制御の有効/無効
  ├── SkillDatabase     → スキル発動
  ├── Enemy             → 敵への参照
  └── PartyMember[]     → 味方キャラデータ（4人）

TypingController
  └── SkillDatabase     → スキル発動の依頼

SkillDatabase
  ├── GameManager       → ゲーム状態参照、ダメージ計算
  ├── Enemy             → 敵への効果適用
  └── UIManager         → スキル発動表示

Enemy
  ├── GameManager       → ゲーム状態参照、ターゲット取得
  └── UIManager（間接） → ダメージ表示更新

UIManager
  ├── GameManager       → パーティ状態取得
  └── Enemy             → 敵HP・状態異常の表示
```

---

## 10. アセット構成

### 10.1 フォルダ構造

```
Assets/
├── Images/
│   ├── Chara/              # キャラクター画像
│   │   ├── GlassMan.jpg    # キャラ1（リンゴ）
│   │   ├── Gentleman.jpg   # キャラ2（ニト）
│   │   ├── CatGirl.jpg     # キャラ3（ショボン）
│   │   └── YellowGirl.jpg  # キャラ4（エンジョウ）
│   ├── TypingRPGIcon.png   # ゲームアイコン
│   ├── HPBackImage.png     # HPバー背景
│   ├── Hp_transparent.png  # HP透過画像
│   ├── poison.png          # 毒アイコン
│   └── pause button.png    # ポーズボタン
├── Scenes/
│   └── TitleScene.unity    # タイトルシーン
├── Scripts/                # 全スクリプト（セクション9参照）
├── Settings/               # プロジェクト設定
├── TextMesh Pro/           # TextMeshProアセット
└── _Recovery/              # リカバリ用バックアップシーン
```

---

## 11. エディタツール

### 11.1 SceneSetup（ワンクリックシーン構築）

- **場所**: `Assets/Scripts/Editor/SceneSetup.cs`
- **メニュー**: `Tools/Setup Main Scene`（※推定）
- **機能**: メインバトルシーンのGameObject・UI要素を自動生成
  - Canvas、EventSystem の自動作成
  - 敵エリア（HPバー、状態異常アイコン）の配置
  - 味方エリア（4人分のキャラ画像、HPバー）の配置
  - タイピング入力エリアの配置
  - ゲーム終了パネルの配置
  - 各コンポーネントのリンク設定

---

## 12. 技術的仕様

### 12.1 デザインパターン

- **Singleton**: `GameManager` はシングルトンパターンで実装
- **Dictionary パターン**: スキルは `Dictionary<string, Action>` で管理
- **ScriptableObject**: キャラクターデータ・スキル設定は ScriptableObject で外部化

### 12.2 使用パッケージ

- **TextMeshPro**: テキスト表示（`TMP_Text`, `TextMeshProUGUI`）
- **UnityEngine.UI**: UI要素（`Image`, `Canvas` 等）

### 12.3 入力システム

- **旧Input System** 使用（`Input.inputString`）
- キーボード入力のみ対応

---

## 13. 既知の課題・改善候補

| # | 内容 | 優先度 |
|---|------|--------|
| 1 | `SkillSettings` ScriptableObject のパラメータが実際のスキルロジックで未参照（ハードコード値使用） | 中 |
| 2 | スピードバフの「曖昧検索」ロジックが未実装（フラグのみ存在） | 中 |
| 3 | MainScene が `.unity` ファイルとして未保存（エディタツールで毎回構築） | 低 |
| 4 | キャラクター名がハードコード（`CharacterData` ScriptableObject との連携が不完全） | 低 |
| 5 | BGM・SE が未実装 | 低 |

---

## 変更履歴

| 日付 | バージョン | 変更内容 |
|------|----------|---------|
| 2026-03-10 | 1.0.0 | 初版作成。全システムの仕様を文書化 |
