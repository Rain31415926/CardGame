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
        private PictureBox pbDeck = new PictureBox(); // 新增實體牌堆

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

        // 選擇角色畫面相關
        private Panel panelCharacterScreen = new Panel();
        private bool isCapooSelected = false;
        private bool capooSkillUsed = false;

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
            PlayBackgroundMusic(backMusicTempFilePath, 1000); // 正常音量
        }

        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            StopBackgroundMusic();
            panelTitle.Visible = false;
            PlayBackgroundMusic(backMusicTempFilePath, 300); // 低音量 (範圍約0-1000)
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
            lblSelectTitle.Font = new Font("Arial", 36, FontStyle.Bold);
            lblSelectTitle.ForeColor = Color.Yellow;
            lblSelectTitle.AutoSize = true;
            lblSelectTitle.Location = new Point(330, 50);

            PictureBox pbCapoo = new PictureBox();
            pbCapoo.Image = (Image)Properties.Resources.ResourceManager.GetObject("character1");
            pbCapoo.SizeMode = PictureBoxSizeMode.Zoom;
            pbCapoo.Size = new Size(200, 200);
            pbCapoo.Location = new Point(375, 150);

            Label lblCapooDesc = new Label();
            lblCapooDesc.Text = "角色：咖波\n技能：每局限發動一次。當抽牌導致爆牌時，\n強制「吃掉」最後抽到的那張牌，讓點數退回，\n並強制進入「停牌」。";
            lblCapooDesc.Font = new Font("Arial", 14, FontStyle.Bold);
            lblCapooDesc.ForeColor = Color.White;
            lblCapooDesc.AutoSize = true;
            lblCapooDesc.Location = new Point(275, 370);
            lblCapooDesc.TextAlign = ContentAlignment.MiddleCenter;

            Button btnSelectCapoo = new Button();
            btnSelectCapoo.Text = "選擇咖波";
            btnSelectCapoo.Font = new Font("Arial", 16, FontStyle.Bold);
            btnSelectCapoo.Size = new Size(150, 50);
            btnSelectCapoo.Location = new Point(400, 480);
            btnSelectCapoo.Click += BtnSelectCapoo_Click;

            panelCharacterScreen.Controls.Add(lblSelectTitle);
            panelCharacterScreen.Controls.Add(pbCapoo);
            panelCharacterScreen.Controls.Add(lblCapooDesc);
            panelCharacterScreen.Controls.Add(btnSelectCapoo);
            this.Controls.Add(panelCharacterScreen);
        }

        private void BtnSelectCapoo_Click(object sender, EventArgs e)
        {
            isCapooSelected = true;
            panelCharacterScreen.Visible = false;
            StartBettingPhase();
        }

        private void SetupUI()
        {
            this.Text = "21點遊戲 (Blackjack)";
            this.Size = new Size(950, 650); // 增加視窗寬度與高度
            this.BackColor = Color.DarkGreen;

            btnHit.Text = "抽牌 (Hit)";
            btnHit.Location = new Point(50, 540);
            btnHit.Size = new Size(100, 40);
            btnHit.Click += BtnHit_Click;

            btnStand.Text = "停牌 (Stand)";
            btnStand.Location = new Point(170, 540); // 增加間距
            btnStand.Size = new Size(100, 40);
            btnStand.Click += BtnStand_Click;

            btnRestart.Text = "下一局 / 重新開始";
            btnRestart.Location = new Point(290, 540); // 增加間距
            btnRestart.Size = new Size(130, 40); // 加大按鈕
            btnRestart.Click += BtnRestart_Click;

            lblDealerScore.Text = "莊家點數: ?";
            lblDealerScore.Location = new Point(50, 30);
            lblDealerScore.ForeColor = Color.White;
            lblDealerScore.AutoSize = true;
            lblDealerScore.Font = new Font("Arial", 14, FontStyle.Bold);

            panelDealer.Location = new Point(50, 60);
            panelDealer.Size = new Size(800, 150); // 增加寬度

            lblPlayerScore.Text = "玩家點數: 0";
            lblPlayerScore.Location = new Point(50, 260);
            lblPlayerScore.ForeColor = Color.White;
            lblPlayerScore.AutoSize = true;
            lblPlayerScore.Font = new Font("Arial", 14, FontStyle.Bold);

            panelPlayer.Location = new Point(50, 290);
            panelPlayer.Size = new Size(800, 150); // 增加寬度

            lblResult.Text = "";
            lblResult.Location = new Point(450, 545); // 往下移
            lblResult.ForeColor = Color.Yellow;
            lblResult.AutoSize = true;
            lblResult.Font = new Font("Arial", 16, FontStyle.Bold);

            // 建立牌堆厚度效果 (底部的幾張牌)
            for (int i = 3; i > 0; i--)
            {
                PictureBox deckBackground = new PictureBox();
                deckBackground.Image = Properties.Resources.back;
                deckBackground.SizeMode = PictureBoxSizeMode.StretchImage;
                deckBackground.Size = new Size(100, 140);
                // 產生稍微錯開的堆疊效果
                deckBackground.Location = new Point(800 + i * 2, 180 - i * 2); // 往右移
                this.Controls.Add(deckBackground);
            }

            // 設置牌堆圖示 (最上面那一張)
            pbDeck.Image = Properties.Resources.back;
            pbDeck.SizeMode = PictureBoxSizeMode.StretchImage;
            pbDeck.Size = new Size(100, 140);
            pbDeck.Location = new Point(800, 180); // 往右移

            // 籌碼標籤
            lblTotalChips.Text = "總籌碼: " + totalChips;
            lblTotalChips.Location = new Point(50, 480); // 調整位置
            lblTotalChips.ForeColor = Color.White;
            lblTotalChips.AutoSize = true;
            lblTotalChips.Font = new Font("Arial", 12, FontStyle.Bold);

            lblCurrentBet.Text = "目前下注: " + currentBet;
            lblCurrentBet.Location = new Point(200, 480); // 調整位置
            lblCurrentBet.ForeColor = Color.Yellow;
            lblCurrentBet.AutoSize = true;
            lblCurrentBet.Font = new Font("Arial", 12, FontStyle.Bold);

            // 發牌按鈕
            btnDeal.Text = "確認下注 / 發牌";
            btnDeal.Location = new Point(780, 475); // 向右移
            btnDeal.Size = new Size(120, 40);
            btnDeal.Click += BtnDeal_Click;

            // 下注按鈕
            int[] betAmounts = { 100, 50, 25, 10, 5 };
            for (int i = 0; i < betAmounts.Length; i++)
            {
                Button btnBet = new Button();
                btnBet.Text = "+" + betAmounts[i].ToString();
                btnBet.Tag = betAmounts[i];
                btnBet.Location = new Point(350 + i * 65, 480); // 調整間距為 65
                btnBet.Size = new Size(55, 30); // 微調大小
                btnBet.Click += BtnBet_Click;
                betButtons.Add(btnBet);
                this.Controls.Add(btnBet);
            }

            // All in 按鈕
            Button btnBetAllIn = new Button();
            btnBetAllIn.Text = "All In";
            btnBetAllIn.Tag = -1; // -1 代表 All In
            btnBetAllIn.Location = new Point(350 + 5 * 65, 480);
            btnBetAllIn.Size = new Size(60, 30);
            btnBetAllIn.Click += BtnBet_Click;
            betButtons.Add(btnBetAllIn);
            this.Controls.Add(btnBetAllIn);

            this.Controls.Add(lblTotalChips);
            this.Controls.Add(lblCurrentBet);
            this.Controls.Add(btnDeal);

            this.Controls.Add(btnHit);
            this.Controls.Add(btnStand);
            this.Controls.Add(btnRestart);
            this.Controls.Add(lblDealerScore);
            this.Controls.Add(panelDealer);
            this.Controls.Add(lblPlayerScore);
            this.Controls.Add(panelPlayer);
            this.Controls.Add(lblResult);
            this.Controls.Add(pbDeck); // 加入畫面
            pbDeck.BringToFront(); // 確保最上面的牌在最上層
        }

        private void InitializeDeck()
        {
            deck.Clear();
            for (int i = 1; i <= 52; i++)
            {
                deck.Add(i);
            }
            Random rnd = new Random();
            deck = deck.OrderBy(x => rnd.Next()).ToList();
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
            if (value == 1) return 11; // 預設A為11
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
            playerHand.Clear();
            dealerHand.Clear();
            panelPlayer.Controls.Clear();
            panelDealer.Controls.Clear();
            lblResult.Text = "請下注...";

            currentBet = 0;
            UpdateChipsUI();

            btnHit.Enabled = false;
            btnStand.Enabled = false;
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

            capooSkillUsed = false; // 重置咖波技能狀態

            btnDeal.Enabled = false;
            foreach (var btn in betButtons)
            {
                btn.Enabled = false;
            }
            btnRestart.Enabled = true;

            InitializeDeck();
            lblResult.Text = "";
            btnHit.Enabled = true;
            btnStand.Enabled = true;

            await DrawCard(playerHand, panelPlayer, false);
            await DrawCard(dealerHand, panelDealer, true);
            await DrawCard(playerHand, panelPlayer, false);
            await DrawCard(dealerHand, panelDealer, false);

            UpdateScores(false);
            CheckBlackjack();
        }

        private async Task DrawCard(List<int> hand, Panel panel, bool isHidden)
        {
            PlayAudio(takeCardTempFilePath);
            
            int card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);

            // 目標在 Panel 中的相對位置
            Point targetLocation = new Point((hand.Count - 1) * 30, 0);

            // 計算目標在 Form 中的絕對座標，以處理動畫軌跡
            Point targetFormLocation = panel.PointToScreen(targetLocation);
            targetFormLocation = this.PointToClient(targetFormLocation);

            // 建立用來跑動畫的虛擬卡片
            PictureBox animatingCard = new PictureBox();
            animatingCard.Size = new Size(100, 140);
            animatingCard.SizeMode = PictureBoxSizeMode.StretchImage;
            animatingCard.Image = Properties.Resources.back; // 飛行中都是背面
            animatingCard.Location = pbDeck.Location;
            this.Controls.Add(animatingCard);
            animatingCard.BringToFront();

            // 動畫迴圈
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

            // 動畫結束，移除虛擬卡片
            this.Controls.Remove(animatingCard);
            animatingCard.Dispose();

            // 正式將卡片加入 Panel 中
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
                int visibleScore = CalculateScore(dealerHand.Skip(1).ToList());
                lblDealerScore.Text = "莊家點數: " + visibleScore + " + ?";
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

        private async void BtnHit_Click(object sender, EventArgs e)
        {
            await DrawCard(playerHand, panelPlayer, false);
            UpdateScores(false);

            if (CalculateScore(playerHand) > 21)
            {
                if (isCapooSelected && !capooSkillUsed)
                {
                    MessageBox.Show("咖波發動技能！「吃掉」了導致爆牌的那張牌！");
                    capooSkillUsed = true;

                    // 退回最後一張牌
                    playerHand.RemoveAt(playerHand.Count - 1);

                    // 移除畫面上的最後一張牌
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
                    BtnStand_Click(sender, e); // 強制停牌
                }
                else
                {
                    EndGame("玩家爆牌！莊家獲勝！", -1);
                }
            }
        }

        private async void BtnStand_Click(object sender, EventArgs e)
        {
            btnHit.Enabled = false;
            btnStand.Enabled = false;

            // 翻開莊家暗牌
            PictureBox hiddenPb = panelDealer.Controls.Find("hiddenCard", false).FirstOrDefault() as PictureBox;
            if (hiddenPb != null)
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
            if (winStatus == 1)
            {
                totalChips += currentBet * 2; // 贏回下注及獎金
            }
            else if (winStatus == 0)
            {
                totalChips += currentBet; // 退回下注
            }
            // 莊家獲勝時籌碼已被扣除
            currentBet = 0;
            UpdateChipsUI();

            lblResult.Text = message;
            btnHit.Enabled = false;
            btnStand.Enabled = false;

            // 如果遊戲結束時暗牌還沒翻開，翻開它
            PictureBox hiddenPb = panelDealer.Controls.Find("hiddenCard", false).FirstOrDefault() as PictureBox;
            if (hiddenPb != null)
            {
                hiddenPb.Image = GetCardImage((int)hiddenPb.Tag);
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
