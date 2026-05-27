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

        private string takeCardTempFilePath;
        private string placeCardTempFilePath;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            ExtractAudioFiles();
            StartNewGame();
        }

        private void ExtractAudioFiles()
        {
            takeCardTempFilePath = Path.Combine(Path.GetTempPath(), "takecard.mp3");
            placeCardTempFilePath = Path.Combine(Path.GetTempPath(), "placecard.mp3");

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

        private void SetupUI()
        {
            this.Text = "21點遊戲 (Blackjack)";
            this.Size = new Size(800, 600);
            this.BackColor = Color.DarkGreen;

            btnHit.Text = "抽牌 (Hit)";
            btnHit.Location = new Point(50, 500);
            btnHit.Size = new Size(100, 40);
            btnHit.Click += BtnHit_Click;

            btnStand.Text = "停牌 (Stand)";
            btnStand.Location = new Point(160, 500);
            btnStand.Size = new Size(100, 40);
            btnStand.Click += BtnStand_Click;

            btnRestart.Text = "重新開始";
            btnRestart.Location = new Point(270, 500);
            btnRestart.Size = new Size(100, 40);
            btnRestart.Click += BtnRestart_Click;

            lblDealerScore.Text = "莊家點數: ?";
            lblDealerScore.Location = new Point(50, 30);
            lblDealerScore.ForeColor = Color.White;
            lblDealerScore.AutoSize = true;
            lblDealerScore.Font = new Font("Arial", 14, FontStyle.Bold);

            panelDealer.Location = new Point(50, 60);
            panelDealer.Size = new Size(700, 150);

            lblPlayerScore.Text = "玩家點數: 0";
            lblPlayerScore.Location = new Point(50, 260);
            lblPlayerScore.ForeColor = Color.White;
            lblPlayerScore.AutoSize = true;
            lblPlayerScore.Font = new Font("Arial", 14, FontStyle.Bold);

            panelPlayer.Location = new Point(50, 290);
            panelPlayer.Size = new Size(700, 150);

            lblResult.Text = "";
            lblResult.Location = new Point(450, 500);
            lblResult.ForeColor = Color.Yellow;
            lblResult.AutoSize = true;
            lblResult.Font = new Font("Arial", 16, FontStyle.Bold);

            this.Controls.Add(btnHit);
            this.Controls.Add(btnStand);
            this.Controls.Add(btnRestart);
            this.Controls.Add(lblDealerScore);
            this.Controls.Add(panelDealer);
            this.Controls.Add(lblPlayerScore);
            this.Controls.Add(panelPlayer);
            this.Controls.Add(lblResult);
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

        private async void StartNewGame()
        {
            InitializeDeck();
            playerHand.Clear();
            dealerHand.Clear();
            panelPlayer.Controls.Clear();
            panelDealer.Controls.Clear();
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
            await Task.Delay(300); // 模擬抽牌延遲

            int card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);

            PictureBox pb = new PictureBox();
            pb.SizeMode = PictureBoxSizeMode.StretchImage;
            pb.Size = new Size(100, 140);
            pb.Location = new Point((hand.Count - 1) * 30, 0);
            
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
                EndGame("玩家21點！玩家獲勝！");
            }
        }

        private async void BtnHit_Click(object sender, EventArgs e)
        {
            await DrawCard(playerHand, panelPlayer, false);
            UpdateScores(false);
            
            if (CalculateScore(playerHand) > 21)
            {
                EndGame("玩家爆牌！莊家獲勝！");
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
                EndGame("莊家爆牌！玩家獲勝！");
            }
            else if (playerScore > dealerScore)
            {
                EndGame("玩家獲勝！");
            }
            else if (dealerScore > playerScore)
            {
                EndGame("莊家獲勝！");
            }
            else
            {
                EndGame("平手！");
            }
        }

        private void EndGame(string message)
        {
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
            StartNewGame();
        }
    }
}
