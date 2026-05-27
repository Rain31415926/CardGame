# 🃏 21點狂熱：異界賭局 (Blackjack: Multiverse Casino)

這是一款使用 C# Windows Forms 開發的 21 點卡牌遊戲。除了經典的 21 點規則與籌碼下注機制外，本專案首創了「專屬角色技能」與「莊家心理戰 (垃圾話)」系統，為傳統的撲克牌遊戲帶來更豐富的策略性與沉浸式對戰體驗。

## 📸 遊戲畫面截圖 (Screenshots)

### 1. 遊戲標題畫面
進入遊戲時的精美深綠色賭桌標題背景，伴隨著高音量的經典背景音樂。
<img width="1160" height="795" alt="image" src="https://github.com/user-attachments/assets/902cafb7-8f18-424a-a2ea-ccd16bd5ff1d" />

### 2. 異界角色選擇
提供三位風格迥異的角色（咖波、柴柴、不可名狀）供玩家自由選擇，每位角色皆有專屬繪圖與細緻的技能說明。
<img width="1152" height="776" alt="image" src="https://github.com/user-attachments/assets/906dca8f-6843-43f8-9ea2-2e20208fea68" />


### 3. 賭桌下注介面
具備多種面額籌碼（+100, +50, +25, +10, +5）與一鍵 All In 功能，點擊下注時會伴隨清脆的籌碼碰撞音效。
<img width="1155" height="776" alt="image" src="https://github.com/user-attachments/assets/3ff67428-c768-4bcb-9c18-473c04219d2d" />


### 4. 遊戲對戰與莊家垃圾話
實際對戰畫面，包含非同步流暢飛牌動畫。當玩家猶豫超過 5 秒時，莊家會動態跳出毒舌垃圾話，若使用特定角色則會轉化為深淵低語。
<img width="1154" height="792" alt="image" src="https://github.com/user-attachments/assets/22e93344-697f-41bb-9f1b-756dc477db05" />

特殊深淵低語：
<img width="1160" height="791" alt="image" src="https://github.com/user-attachments/assets/f2bc2e55-0044-461e-b0ee-8fda9c671194" />


## ✨ 遊戲特色 (Core Features)

* **💰 完整籌碼經濟系統：** 玩家擁有初始 500 籌碼。若不幸破產，系統會自動發放救濟金讓玩家東山再起。
* **🗣️ 莊家心理戰 (垃圾話系統)：** 內建「猶豫計時器」。當玩家思考超過 5 秒未作決策時，莊家會隨機給出嘲諷的「垃圾話」來誘惑玩家抽牌，大幅提升賭桌上的壓迫感與真實感。
* **🎬 流暢視覺與多軌音效：** 實作非同步的流暢飛牌動畫，並支援無衝突的背景音樂 (BGM) 與多重音效（發牌聲、下注聲）疊加播放。

## 🎭 異界角色與技能系統 (Character Skills)

玩家在進入賭局前，可從三位風格迥異的角色中擇一參賽，每位角色都擁有足以扭轉戰局的專屬技能（每局限發動一次）：

1. **🐛 貓貓蟲咖波 (Bugcat Capoo)**
   * **技能【吃牌】：** 絕對的防禦機制。當抽牌不幸導致爆牌時，咖波會強制「吃掉」最後抽出的那張牌，讓點數退回安全值並強制停牌。
2. **🐕 狗狗 (Loyal Shiba)**
   * **技能【偷看挖寶】：** 預知未來的能力。抽牌前可派狗狗衝進牌堆看下一張牌的點數，玩家可根據點數決定要「正常收下」或是叫狗狗「呸掉」改抽下一張未知的牌。
3. **🐙 不可名狀 (The Unnameable)**
   * **技能【深淵盲牌】：** 終極的高難度挑戰。發動後莊家的明牌將被強制的「黑霧」覆蓋。此外，莊家的嘲諷也會轉變為令人理智狂掉的克蘇魯深淵低語。

## 🛠️ 開發環境與技術棧 (Tech Stack)

* **開發語言：** C#
* **開發框架：** Windows Forms (.NET Framework 4.8)
* **開發工具：** Visual Studio
* **核心技術應用：**
  * **非同步動畫實作：** 使用 `async`/`await` 與 `Task.Delay` 實作非阻塞式 (Non-blocking) 的 UI 卡牌飛行動畫。
  * **底層音效控制：** 透過引入 `winmm.dll` 呼叫 `mciSendString` API，達成 BGM 與多種音效同步播放且可調音量的需求。
  * **動態介面生成：** 使用程式碼動態生成 PictureBox 與標籤，並透過座標演算呈現牌堆與發牌視覺效果。
  * **Timer 事件驅動：** 利用 `System.Windows.Forms.Timer` 完美控制遊戲中的等待與垃圾話觸發邏輯。
