using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace _1121538_徐霈綺_CardGame
{
    public partial class Form1 : Form
    {
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

        private List<int> deck = new List<int>();
        private List<int> playerHand = new List<int>();
        private List<int> dealerHand = new List<int>();

        private Button btnHit = new Button();
        private Button btnStand = new Button();
        private Button btnRestart = new Button();
        private Label lblPlayerScore = new Label();
        private Label lblDealerScore = new Label();
        private Label lblResult = new Label();
        private Panel panelPlayer = new Panel();
        private Panel panelDealer = new Panel();
        private PictureBox pbDeck = new PictureBox();

        // 籌碼與下注相關
        private int totalChips = 500;
        private int currentBet = 0;
        private Label lblTotalChips = new Label();
        private Label lblCurrentBet = new Label();
        private Button btnDeal = new Button();
        private List<Button> betButtons = new List<Button>();

        // 標題畫面相關
        private Panel panelTitle = new Panel();
        private Label lblGameTitle = new Label();
        private Button btnStartGame = new Button();
        private string backMusicTempFilePath;

        // 選擇角色畫面與技能相關
        private Panel panelCharacterScreen = new Panel();

        private bool isCapooSelected = false;
        private bool capooSkillUsed = false;

        private bool isDogSelected = false;
        private bool dogSkillUsed = false;
        private Button btnDogSkill = new Button();

        private bool isEldritchSelected = false;

        // 垃圾話系統相關
        private Timer hesitationTimer = new Timer();
        private Label lblDealerTalk = new Label();
        private Random randomizer = new Random();

        private string takeCardTempFilePath;
        private string placeCardTempFilePath;
        private string placeChipTempFilePath;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            SetupTitleScreen();
            SetupCharacterScreen();
            ExtractAudioFiles();
            ShowTitleScreen();
        }

        private void ExtractAudioFiles()
        {
            takeCardTempFilePath = Path.Combine(Path.GetTempPath(), "takecard.mp3");
            placeCardTempFilePath = Path.Combine(Path.GetTempPath(), "placecard.mp3");
            placeChipTempFilePath = Path.Combine(Path.GetTempPath(), "placechip.mp3");
            backMusicTempFilePath = Path.Combine(Path.GetTempPath(), "backmusic.mp3");

            if (Properties.Resources.backmusic != null)
            {
                using (FileStream fs = new FileStream(backMusicTempFilePath, FileMode.Create, FileAccess.Write))
                {
                    Properties.Resources.backmusic.CopyTo(fs);
                }
            }

            if (Properties.Resources.placechip != null)
            {
                using (FileStream fs = new FileStream(placeChipTempFilePath, FileMode.Create, FileAccess.Write))
                {
                    Properties.Resources.placechip.CopyTo(fs);
                }
            }

            if (Properties.Resources.takecard != null)
            {
                using (FileStream fs = new FileStream(takeCardTempFilePath, FileMode.Create, FileAccess.Write))
                {
                    Properties.Resources.takecard.CopyTo(fs);
                }
            }
            if (Properties.Resources.placecard != null)
            {
                using (FileStream fs = new FileStream(placeCardTempFilePath, FileMode.Create, FileAccess.Write))
                {
                    Properties.Resources.placecard.CopyTo(fs);
                }
            }
        }

        private void PlayAudio(string filePath)
        {
            if (File.Exists(filePath))
            {
                string command = $"play \"{filePath}\" from 0";
                mciSendString(command, null, 0, IntPtr.Zero);
            }
        }

        private void PlayBackgroundMusic(string filePath, int volume = 1000)
        {
            if (File.Exists(filePath))
            {
                mciSendString($"open \"{filePath}\" type mpegvideo alias bgm", null, 0, IntPtr.Zero);
                mciSendString($"setaudio bgm volume to {volume}", null, 0, IntPtr.Zero);
                mciSendString("play bgm repeat", null, 0, IntPtr.Zero);
            }
        }

        private void StopBackgroundMusic()
        {
            mciSendString("stop bgm", null, 0, IntPtr.Zero);
            mciSendString("close bgm", null, 0, IntPtr.Zero);
        }

        private void SetupTitleScreen()
        {
            panelTitle.Size = new Size(950, 650);
            panelTitle.Location = new Point(0, 0);
            panelTitle.BackColor = Color.DarkGreen;

            lblGameTitle.Text = "21點 (Blackjack)";
            lblGameTitle.Font = new Font("Arial", 48, FontStyle.Bold);
            lblGameTitle.ForeColor = Color.Yellow;
            lblGameTitle.AutoSize = true;
            lblGameTitle.Location = new Point(200, 150);

            btnStartGame.Text = "開始遊戲";
            btnStartGame.Font = new Font("Arial", 20, FontStyle.Bold);
            btnStartGame.Size = new Size(200, 60);
            btnStartGame.Location = new Point(360, 350);
            btnStartGame.Click += BtnStartGame_Click;

            panelTitle.Controls.Add(lblGameTitle);
            panelTitle.Controls.Add(btnStartGame);
            this.Controls.Add(panelTitle);
        }

        private void ShowTitleScreen()
        {
            panelTitle.BringToFront();
            panelTitle.Visible = true;
            PlayBackgroundMusic(backMusicTempFilePath, 1000);
        }

        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            StopBackgroundMusic();
            panelTitle.Visible = false;
            PlayBackgroundMusic(backMusicTempFilePath, 300);
            panelCharacterScreen.BringToFront();
            panelCharacterScreen.Visible = true;
        }

        private void SetupCharacterScreen()
        {
            panelCharacterScreen.Size = new Size(950, 650);
            panelCharacterScreen.Location = new Point(0, 0);
            panelCharacterScreen.BackColor = Color.DarkGreen;
            panelCharacterScreen.Visible = false;

            Label lblSelectTitle = new Label();
            lblSelectTitle.Text = "請選擇角色";
            lblSelectTitle.Font = new Font("Arial", 32, FontStyle.Bold);
            lblSelectTitle.ForeColor = Color.Yellow;
            lblSelectTitle.AutoSize = true;
            lblSelectTitle.Location = new Point(340, 30);

            // ================== 咖波區塊 ==================
            PictureBox pbCapoo = new PictureBox();
            pbCapoo.Image = (Image)Properties.Resources.ResourceManager.GetObject("character1");
            pbCapoo.SizeMode = PictureBoxSizeMode.Zoom;
            pbCapoo.Size = new Size(180, 180);
            pbCapoo.Location = new Point(70, 120);

            Label lblCapooDesc = new Label();
            lblCapooDesc.Text = "角色：咖波\n技能：吃牌\n抽牌導致爆牌時，強制「吃掉」\n最後抽的牌，並強制停牌。\n每局限一次。";
            lblCapooDesc.Font = new Font("Arial", 12, FontStyle.Bold);
            lblCapooDesc.ForeColor = Color.White;
            lblCapooDesc.AutoSize = true;
            lblCapooDesc.Location = new Point(45, 330);
            lblCapooDesc.TextAlign = ContentAlignment.MiddleCenter;

            Button btnSelectCapoo = new Button();
            btnSelectCapoo.Text = "選擇咖波";
            btnSelectCapoo.Font = new Font("Arial", 14, FontStyle.Bold);
            btnSelectCapoo.Size = new Size(140, 45);
            btnSelectCapoo.Location = new Point(90, 480);
            btnSelectCapoo.Click += BtnSelectCapoo_Click;

            // ================== 狗狗區塊 ==================
            PictureBox pbDog = new PictureBox();
            pbDog.Image = (Image)Properties.Resources.ResourceManager.GetObject("character2");
            pbDog.SizeMode = PictureBoxSizeMode.Zoom;
            pbDog.Size = new Size(180, 180);
            pbDog.Location = new Point(380, 120);

            Label lblDogDesc = new Label();
            lblDogDesc.Text = "角色：忠心柴柴\n技能：偷看挖寶\n抽牌前讓狗狗看下一張牌，\n可決定正常拿走或棄置重抽。\n每局限一次。";
            lblDogDesc.Font = new Font("Arial", 12, FontStyle.Bold);
            lblDogDesc.ForeColor = Color.White;
            lblDogDesc.AutoSize = true;
            lblDogDesc.Location = new Point(355, 330);
            lblDogDesc.TextAlign = ContentAlignment.MiddleCenter;

            Button btnSelectDog = new Button();
            btnSelectDog.Text = "選擇狗狗";
            btnSelectDog.Font = new Font("Arial", 14, FontStyle.Bold);
            btnSelectDog.Size = new Size(140, 45);
            btnSelectDog.Location = new Point(400, 480);
            btnSelectDog.Click += BtnSelectDog_Click;

            // ================== 不可名狀之物區塊 ==================
            PictureBox pbEldritch = new PictureBox();
            pbEldritch.Image = (Image)Properties.Resources.ResourceManager.GetObject("character3");
            pbEldritch.SizeMode = PictureBoxSizeMode.Zoom;
            pbEldritch.Size = new Size(180, 180);
            pbEldritch.Location = new Point(690, 120);

            Label lblEldritchDesc = new Label();
            lblEldritchDesc.Text = "角色：不可名狀\n技能：深淵盲牌\n回合開始即發動。莊家的明牌\n將被黑霧強制覆蓋，玩家必須\n在未知底細的情況下盲打。";
            lblEldritchDesc.Font = new Font("Arial", 12, FontStyle.Bold);
            lblEldritchDesc.ForeColor = Color.White;
            lblEldritchDesc.AutoSize = true;
            lblEldritchDesc.Location = new Point(655, 330);
            lblEldritchDesc.TextAlign = ContentAlignment.MiddleCenter;

            Button btnSelectEldritch = new Button();
            btnSelectEldritch.Text = "選擇不可名狀";
            btnSelectEldritch.Font = new Font("Arial", 14, FontStyle.Bold);
            btnSelectEldritch.Size = new Size(140, 45);
            btnSelectEldritch.Location = new Point(710, 480);
            btnSelectEldritch.Click += BtnSelectEldritch_Click;

            panelCharacterScreen.Controls.Add(lblSelectTitle);
            panelCharacterScreen.Controls.Add(pbCapoo);
            panelCharacterScreen.Controls.Add(lblCapooDesc);
            panelCharacterScreen.Controls.Add(btnSelectCapoo);
            panelCharacterScreen.Controls.Add(pbDog);
            panelCharacterScreen.Controls.Add(lblDogDesc);
            panelCharacterScreen.Controls.Add(btnSelectDog);
            panelCharacterScreen.Controls.Add(pbEldritch);
            panelCharacterScreen.Controls.Add(lblEldritchDesc);
            panelCharacterScreen.Controls.Add(btnSelectEldritch);

            this.Controls.Add(panelCharacterScreen);
        }

        private void BtnSelectCapoo_Click(object sender, EventArgs e)
        {
            isCapooSelected = true;
            isDogSelected = false;
            isEldritchSelected = false;
            panelCharacterScreen.Visible = false;
            StartBettingPhase();
        }

        private void BtnSelectDog_Click(object sender, EventArgs e)
        {
            isDogSelected = true;
            isCapooSelected = false;
            isEldritchSelected = false;
            panelCharacterScreen.Visible = false;
            StartBettingPhase();
        }

        private void BtnSelectEldritch_Click(object sender, EventArgs e)
        {
            isEldritchSelected = true;
            isDogSelected = false;
            isCapooSelected = false;
            panelCharacterScreen.Visible = false;
            StartBettingPhase();
        }

        private void SetupUI()
        {
            this.Text = "21點遊戲 (Blackjack)";
            this.Size = new Size(950, 650);
            this.BackColor = Color.DarkGreen;

            btnHit.Text = "抽牌 (Hit)";
            btnHit.Location = new Point(50, 540);
            btnHit.Size = new Size(100, 40);
            btnHit.Click += BtnHit_Click;

            btnStand.Text = "停牌 (Stand)";
            btnStand.Location = new Point(170, 540);
            btnStand.Size = new Size(100, 40);
            btnStand.Click += BtnStand_Click;

            btnRestart.Text = "下一局 / 重新開始";
            btnRestart.Location = new Point(290, 540);
            btnRestart.Size = new Size(130, 40);
            btnRestart.Click += BtnRestart_Click;

            // 狗狗技能專屬按鈕
            btnDogSkill.Text = "狗狗挖寶技能";
            btnDogSkill.Location = new Point(230, 255);
            btnDogSkill.Size = new Size(110, 35);
            btnDogSkill.Font = new Font("Arial", 10, FontStyle.Bold);
            btnDogSkill.BackColor = Color.LightYellow;
            btnDogSkill.Click += BtnDogSkill_Click;
            btnDogSkill.Visible = false;

            lblDealerScore.Text = "莊家點數: ?";
            lblDealerScore.Location = new Point(50, 30);
            lblDealerScore.ForeColor = Color.White;
            lblDealerScore.AutoSize = true;
            lblDealerScore.Font = new Font("Arial", 14, FontStyle.Bold);

            panelDealer.Location = new Point(50, 60);
            panelDealer.Size = new Size(800, 150);

            lblPlayerScore.Text = "玩家點數: 0";
            lblPlayerScore.Location = new Point(50, 260);
            lblPlayerScore.ForeColor = Color.White;
            lblPlayerScore.AutoSize = true;
            lblPlayerScore.Font = new Font("Arial", 14, FontStyle.Bold);

            panelPlayer.Location = new Point(50, 290);
            panelPlayer.Size = new Size(800, 150);

            lblResult.Text = "";
            lblResult.Location = new Point(450, 545);
            lblResult.ForeColor = Color.Yellow;
            lblResult.AutoSize = true;
            lblResult.Font = new Font("Arial", 16, FontStyle.Bold);

            // 垃圾話標籤設定
            lblDealerTalk.Text = "";
            lblDealerTalk.Location = new Point(320, 215); // 放在莊家牌跟玩家牌中間
            lblDealerTalk.ForeColor = Color.Cyan;
            lblDealerTalk.AutoSize = true;
            lblDealerTalk.Font = new Font("Microsoft JhengHei", 16, FontStyle.Italic | FontStyle.Bold);
            this.Controls.Add(lblDealerTalk);
            lblDealerTalk.BringToFront();

            // 設定垃圾話計時器 (5秒 = 5000毫秒)
            hesitationTimer.Interval = 5000;
            hesitationTimer.Tick += HesitationTimer_Tick;

            for (int i = 3; i > 0; i--)
            {
                PictureBox deckBackground = new PictureBox();
                deckBackground.Image = Properties.Resources.back;
                deckBackground.SizeMode = PictureBoxSizeMode.StretchImage;
                deckBackground.Size = new Size(100, 140);
                deckBackground.Location = new Point(800 + i * 2, 180 - i * 2);
                this.Controls.Add(deckBackground);
            }

            pbDeck.Image = Properties.Resources.back;
            pbDeck.SizeMode = PictureBoxSizeMode.StretchImage;
            pbDeck.Size = new Size(100, 140);
            pbDeck.Location = new Point(800, 180);

            lblTotalChips.Text = "總籌碼: " + totalChips;
            lblTotalChips.Location = new Point(50, 480);
            lblTotalChips.ForeColor = Color.White;
            lblTotalChips.AutoSize = true;
            lblTotalChips.Font = new Font("Arial", 12, FontStyle.Bold);

            lblCurrentBet.Text = "目前下注: " + currentBet;
            lblCurrentBet.Location = new Point(200, 480);
            lblCurrentBet.ForeColor = Color.Yellow;
            lblCurrentBet.AutoSize = true;
            lblCurrentBet.Font = new Font("Arial", 12, FontStyle.Bold);

            btnDeal.Text = "確認下注 / 發牌";
            btnDeal.Location = new Point(780, 475);
            btnDeal.Size = new Size(120, 40);
            btnDeal.Click += BtnDeal_Click;

            int[] betAmounts = { 100, 50, 25, 10, 5 };
            for (int i = 0; i < betAmounts.Length; i++)
            {
                Button btnBet = new Button();
                btnBet.Text = "+" + betAmounts[i].ToString();
                btnBet.Tag = betAmounts[i];
                btnBet.Location = new Point(350 + i * 65, 480);
                btnBet.Size = new Size(55, 30);
                btnBet.Click += BtnBet_Click;
                betButtons.Add(btnBet);
                this.Controls.Add(btnBet);
            }

            Button btnBetAllIn = new Button();
            btnBetAllIn.Text = "All In";
            btnBetAllIn.Tag = -1;
            btnBetAllIn.Location = new Point(350 + 5 * 65, 480);
            btnBetAllIn.Size = new Size(60, 30);
            btnBetAllIn.Click += BtnBet_Click;
            betButtons.Add(btnBetAllIn);
            this.Controls.Add(btnBetAllIn);

            this.Controls.Add(lblTotalChips);
            this.Controls.Add(lblCurrentBet);
            this.Controls.Add(btnDeal);
            this.Controls.Add(btnDogSkill);
            this.Controls.Add(btnHit);
            this.Controls.Add(btnStand);
            this.Controls.Add(btnRestart);
            this.Controls.Add(lblDealerScore);
            this.Controls.Add(panelDealer);
            this.Controls.Add(lblPlayerScore);
            this.Controls.Add(panelPlayer);
            this.Controls.Add(lblResult);
            this.Controls.Add(pbDeck);
            pbDeck.BringToFront();
        }

        // ================== 垃圾話計時器與邏輯 ==================
        private void ResetHesitationTimer()
        {
            lblDealerTalk.Text = "";
            hesitationTimer.Stop();
            hesitationTimer.Start();
        }

        private void StopHesitationTimer()
        {
            lblDealerTalk.Text = "";
            hesitationTimer.Stop();
        }

        private void HesitationTimer_Tick(object sender, EventArgs e)
        {
            // 根據不同角色給予不同的嘲諷
            if (isEldritchSelected)
            {
                string[] eldritchTalks = {
                    "放棄掙扎吧...",
                    "理智正在流失...",
                    "你看不透這片黑霧的...",
                    "抽牌...投入深淵的懷抱吧..."
                };
                lblDealerTalk.ForeColor = Color.MediumPurple;
                lblDealerTalk.Text = "『" + eldritchTalks[randomizer.Next(eldritchTalks.Length)] + "』";
            }
            else
            {
                string[] trashTalks = {
                    "怎麼啦？怕爆牌嗎？再抽一張啦！",
                    "才這點分數就想贏？算了吧，快抽牌！",
                    "猶豫什麼？幸運女神在對你微笑呢，Hit！",
                    "我看你這牌...不抽絕對會輸喔。",
                    "不敢抽？那這局的籌碼我就收下囉！",
                    "快點決定啦，後面的客人在等呢！"
                };
                lblDealerTalk.ForeColor = Color.Cyan;
                lblDealerTalk.Text = "莊家：「" + trashTalks[randomizer.Next(trashTalks.Length)] + "」";
            }

            // 嘲諷一次後先暫停計時，等玩家下一步動作再重置，避免瘋狂洗頻
            hesitationTimer.Stop();
        }

        private void InitializeDeck()
        {
            deck.Clear();
            for (int i = 1; i <= 52; i++)
            {
                deck.Add(i);
            }
            deck = deck.OrderBy(x => randomizer.Next()).ToList();
        }

        private Image GetCardImage(int cardIndex)
        {
            string imageName = "pic" + cardIndex;
            return (Image)Properties.Resources.ResourceManager.GetObject(imageName) ?? Properties.Resources.Image1;
        }

        private int GetCardValue(int cardIndex)
        {
            int value = ((cardIndex - 1) / 4) + 1;
            if (value > 10) return 10;
            if (value == 1) return 11;
            return value;
        }

        private int CalculateScore(List<int> hand)
        {
            int score = 0;
            int aces = 0;
            foreach (int card in hand)
            {
                int val = GetCardValue(card);
                if (val == 11) aces++;
                score += val;
            }
            while (score > 21 && aces > 0)
            {
                score -= 10;
                aces--;
            }
            return score;
        }

        private void StartBettingPhase()
        {
            StopHesitationTimer(); // 確保下注階段不會跳出垃圾話

            playerHand.Clear();
            dealerHand.Clear();
            panelPlayer.Controls.Clear();
            panelDealer.Controls.Clear();
            lblResult.Text = "請下注...";

            currentBet = 0;
            UpdateChipsUI();

            btnHit.Enabled = false;
            btnStand.Enabled = false;
            btnDogSkill.Visible = false;
            btnRestart.Enabled = false;
            btnDeal.Enabled = true;

            foreach (var btn in betButtons)
            {
                btn.Enabled = true;
            }
        }

        private void UpdateChipsUI()
        {
            lblTotalChips.Text = "總籌碼: " + totalChips;
            lblCurrentBet.Text = "目前下注: " + currentBet;
        }

        private void BtnBet_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int amount = (int)btn.Tag;

            if (amount == -1) // All In
            {
                amount = totalChips;
            }

            if (totalChips >= amount && amount > 0)
            {
                totalChips -= amount;
                currentBet += amount;
                PlayAudio(placeChipTempFilePath);
                UpdateChipsUI();
            }
        }

        private async void BtnDeal_Click(object sender, EventArgs e)
        {
            if (currentBet == 0)
            {
                MessageBox.Show("請先下注！");
                return;
            }

            capooSkillUsed = false;
            dogSkillUsed = false;

            btnDeal.Enabled = false;
            foreach (var btn in betButtons)
            {
                btn.Enabled = false;
            }
            btnRestart.Enabled = true;

            InitializeDeck();

            if (isEldritchSelected)
                lblResult.Text = "深淵黑霧籠罩...請盲打";
            else
                lblResult.Text = "";

            btnHit.Enabled = true;
            btnStand.Enabled = true;

            if (isDogSelected)
            {
                btnDogSkill.Visible = true;
                btnDogSkill.Enabled = true;
            }

            await DrawCard(playerHand, panelPlayer, false);
            await DrawCard(dealerHand, panelDealer, true); // 莊家第一張永遠暗牌
            await DrawCard(playerHand, panelPlayer, false);
            await DrawCard(dealerHand, panelDealer, isEldritchSelected); // 不可名狀：第二張也暗牌

            UpdateScores(false);
            CheckBlackjack();

            // 發牌結束，玩家可以開始動作，啟動猶豫計時器
            if (CalculateScore(playerHand) < 21)
            {
                ResetHesitationTimer();
            }
        }

        private async Task DrawCard(List<int> hand, Panel panel, bool isHidden)
        {
            PlayAudio(takeCardTempFilePath);

            int card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);

            Point targetLocation = new Point((hand.Count - 1) * 30, 0);
            Point targetFormLocation = panel.PointToScreen(targetLocation);
            targetFormLocation = this.PointToClient(targetFormLocation);

            PictureBox animatingCard = new PictureBox();
            animatingCard.Size = new Size(100, 140);
            animatingCard.SizeMode = PictureBoxSizeMode.StretchImage;
            animatingCard.Image = Properties.Resources.back;
            animatingCard.Location = pbDeck.Location;
            this.Controls.Add(animatingCard);
            animatingCard.BringToFront();

            int steps = 15;
            float dx = (targetFormLocation.X - animatingCard.Location.X) / (float)steps;
            float dy = (targetFormLocation.Y - animatingCard.Location.Y) / (float)steps;
            float currX = animatingCard.Location.X;
            float currY = animatingCard.Location.Y;

            for (int i = 0; i < steps; i++)
            {
                currX += dx;
                currY += dy;
                animatingCard.Location = new Point((int)currX, (int)currY);
                await Task.Delay(15);
            }

            this.Controls.Remove(animatingCard);
            animatingCard.Dispose();

            PictureBox pb = new PictureBox();
            pb.SizeMode = PictureBoxSizeMode.StretchImage;
            pb.Size = new Size(100, 140);
            pb.Location = targetLocation;

            if (isHidden)
            {
                pb.Image = Properties.Resources.back;
                pb.Name = "hiddenCard";
                pb.Tag = card;
            }
            else
            {
                pb.Image = GetCardImage(card);
            }

            panel.Controls.Add(pb);
            pb.BringToFront();

            PlayAudio(placeCardTempFilePath);
        }

        private void UpdateScores(bool showDealerScore)
        {
            lblPlayerScore.Text = "玩家點數: " + CalculateScore(playerHand);
            if (showDealerScore)
            {
                lblDealerScore.Text = "莊家點數: " + CalculateScore(dealerHand);
            }
            else
            {
                if (isEldritchSelected)
                {
                    lblDealerScore.Text = "莊家點數: ? + ?";
                }
                else
                {
                    int visibleScore = CalculateScore(dealerHand.Skip(1).ToList());
                    lblDealerScore.Text = "莊家點數: " + visibleScore + " + ?";
                }
            }
        }

        private void CheckBlackjack()
        {
            int playerScore = CalculateScore(playerHand);
            if (playerScore == 21)
            {
                EndGame("玩家21點！玩家獲勝！", 1);
            }
        }

        private async Task ProcessPlayerHit()
        {
            StopHesitationTimer(); // 玩家有動作，先暫停計時並清除文字

            await DrawCard(playerHand, panelPlayer, false);
            UpdateScores(false);

            if (CalculateScore(playerHand) > 21)
            {
                if (isCapooSelected && !capooSkillUsed)
                {
                    MessageBox.Show("咖波發動技能！「吃掉」了導致爆牌的那張牌！", "技能發動", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    capooSkillUsed = true;

                    playerHand.RemoveAt(playerHand.Count - 1);

                    PictureBox lastCardPb = null;
                    int maxX = -1;
                    foreach (Control ctrl in panelPlayer.Controls)
                    {
                        if (ctrl is PictureBox && ctrl.Location.X > maxX)
                        {
                            maxX = ctrl.Location.X;
                            lastCardPb = (PictureBox)ctrl;
                        }
                    }
                    if (lastCardPb != null)
                    {
                        panelPlayer.Controls.Remove(lastCardPb);
                        lastCardPb.Dispose();
                    }

                    UpdateScores(false);
                    BtnStand_Click(null, EventArgs.Empty);
                }
                else
                {
                    EndGame("玩家爆牌！莊家獲勝！", -1);
                }
            }
            else
            {
                // 玩家抽牌後還沒爆牌，重新開始 5 秒猶豫倒數
                ResetHesitationTimer();
            }
        }

        private async void BtnHit_Click(object sender, EventArgs e)
        {
            await ProcessPlayerHit();
        }

        private async void BtnDogSkill_Click(object sender, EventArgs e)
        {
            if (dogSkillUsed || deck.Count == 0) return;

            StopHesitationTimer(); // 發動技能時先停止計時

            int nextCard = deck[0];
            int cardValue = GetCardValue(nextCard);

            DialogResult result = MessageBox.Show(
                $"汪汪！柴柴衝進牌堆挖寶，下張牌的點數是 {cardValue}！\n\n按下「是(Yes)」乖乖拿走這張牌\n按下「否(No)」叫柴柴呸掉並抽下一張未知牌",
                "狗狗挖寶技能",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            dogSkillUsed = true;
            btnDogSkill.Enabled = false;

            if (result == DialogResult.Yes)
            {
                await ProcessPlayerHit();
            }
            else
            {
                MessageBox.Show("呸！柴柴把牌吐掉了，你改抽了下一張未知的牌。", "技能發動", MessageBoxButtons.OK, MessageBoxIcon.Information);
                deck.RemoveAt(0);
                await ProcessPlayerHit();
            }
        }

        private async void BtnStand_Click(object sender, EventArgs e)
        {
            StopHesitationTimer(); // 玩家選擇停牌，停止計時

            btnHit.Enabled = false;
            btnStand.Enabled = false;
            btnDogSkill.Enabled = false;

            var hiddenCards = panelDealer.Controls.Find("hiddenCard", false).OfType<PictureBox>().ToList();
            foreach (var hiddenPb in hiddenCards)
            {
                hiddenPb.Image = GetCardImage((int)hiddenPb.Tag);
                hiddenPb.Name = "revealedCard";
            }
            UpdateScores(true);
            await Task.Delay(500);

            while (CalculateScore(dealerHand) < 17)
            {
                await DrawCard(dealerHand, panelDealer, false);
                UpdateScores(true);
                await Task.Delay(500);
            }

            int playerScore = CalculateScore(playerHand);
            int dealerScore = CalculateScore(dealerHand);

            if (dealerScore > 21)
            {
                EndGame("莊家爆牌！玩家獲勝！", 1);
            }
            else if (playerScore > dealerScore)
            {
                EndGame("玩家獲勝！", 1);
            }
            else if (dealerScore > playerScore)
            {
                EndGame("莊家獲勝！", -1);
            }
            else
            {
                EndGame("平手！", 0);
            }
        }

        private void EndGame(string message, int winStatus)
        {
            StopHesitationTimer(); // 確保遊戲結束時不會跳出垃圾話

            if (winStatus == 1)
            {
                totalChips += currentBet * 2;
            }
            else if (winStatus == 0)
            {
                totalChips += currentBet;
            }

            currentBet = 0;
            UpdateChipsUI();

            lblResult.Text = message;
            btnHit.Enabled = false;
            btnStand.Enabled = false;
            btnDogSkill.Enabled = false;

            var hiddenCards = panelDealer.Controls.Find("hiddenCard", false).OfType<PictureBox>().ToList();
            if (hiddenCards.Count > 0)
            {
                foreach (var hiddenPb in hiddenCards)
                {
                    hiddenPb.Image = GetCardImage((int)hiddenPb.Tag);
                    hiddenPb.Name = "revealedCard";
                }
                UpdateScores(true);
            }
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            if (totalChips <= 0)
            {
                MessageBox.Show("籌碼已耗盡，為您補滿500籌碼重新開始！");
                totalChips = 500;
            }
            StartBettingPhase();
        }
    }
}