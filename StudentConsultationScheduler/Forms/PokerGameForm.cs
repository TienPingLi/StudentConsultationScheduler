using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace StudentConsultationScheduler.Forms
{
    public class PokerGameForm : Form
    {
        private readonly Random rng = new Random();
        private readonly List<PokerCard> deck = new List<PokerCard>();
        private readonly List<PokerCard> hand = new List<PokerCard>();
        private readonly Button[] cardButtons = new Button[5];
        private TextBox txtBet;
        private Label lblMoney;
        private Label lblResult;
        private Button btnDeal;
        private Button btnDraw;
        private Button btnReset;
        private int money = 1000;
        private int currentBet = 0;
        private bool[] hold = new bool[5];
        private bool waitingForDraw = false;

        public PokerGameForm()
        {
            InitializeUi();
            ResetDeck();
            UpdateUi();
        }

        private void InitializeUi()
        {
            Text = "五張撲克 - 下注小遊戲";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(860, 520);
            MinimumSize = new Size(820, 500);
            BackColor = Color.FromArgb(18, 96, 61);

            Label title = new Label();
            title.Text = "五張撲克";
            title.Font = new Font("Microsoft JhengHei UI", 24, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Location = new Point(26, 18);
            title.Size = new Size(280, 44);
            Controls.Add(title);

            Label rules = new Label();
            rules.Text = "流程：輸入下注金額 → 發牌 → 點牌保留 → 換牌 → 依牌型計算獎金。";
            rules.Font = new Font("Microsoft JhengHei UI", 10);
            rules.ForeColor = Color.FromArgb(220, 245, 232);
            rules.Location = new Point(30, 65);
            rules.Size = new Size(680, 26);
            Controls.Add(rules);

            lblMoney = new Label();
            lblMoney.Font = new Font("Microsoft JhengHei UI", 14, FontStyle.Bold);
            lblMoney.ForeColor = Color.White;
            lblMoney.Location = new Point(620, 24);
            lblMoney.Size = new Size(220, 34);
            lblMoney.TextAlign = ContentAlignment.MiddleRight;
            Controls.Add(lblMoney);

            int x = 40;
            for (int i = 0; i < cardButtons.Length; i++)
            {
                Button card = new Button();
                card.Text = "?";
                card.Font = new Font("Segoe UI Symbol", 24, FontStyle.Bold);
                card.Location = new Point(x, 120);
                card.Size = new Size(140, 190);
                card.Tag = i;
                card.BackColor = Color.White;
                card.ForeColor = Color.FromArgb(30, 30, 35);
                card.FlatStyle = FlatStyle.Flat;
                card.FlatAppearance.BorderSize = 3;
                card.FlatAppearance.BorderColor = Color.WhiteSmoke;
                card.Click += Card_Click;
                Controls.Add(card);
                cardButtons[i] = card;
                x += 155;
            }

            Label lblBet = new Label();
            lblBet.Text = "下注金額";
            lblBet.Font = new Font("Microsoft JhengHei UI", 11, FontStyle.Bold);
            lblBet.ForeColor = Color.White;
            lblBet.Location = new Point(40, 350);
            lblBet.Size = new Size(80, 28);
            Controls.Add(lblBet);

            txtBet = new TextBox();
            txtBet.Text = "50";
            txtBet.Font = new Font("Microsoft JhengHei UI", 12);
            txtBet.Location = new Point(125, 348);
            txtBet.Size = new Size(95, 29);
            Controls.Add(txtBet);

            btnDeal = MakeButton("發牌 / 下注", 245, 344, 120);
            btnDeal.Click += BtnDeal_Click;
            Controls.Add(btnDeal);

            btnDraw = MakeButton("換牌結算", 380, 344, 120);
            btnDraw.Click += BtnDraw_Click;
            Controls.Add(btnDraw);

            btnReset = MakeButton("重新開始", 515, 344, 120);
            btnReset.Click += delegate { money = 1000; ClearRound(); UpdateUi(); };
            Controls.Add(btnReset);

            lblResult = new Label();
            lblResult.Text = "請輸入下注金額後開始。";
            lblResult.Font = new Font("Microsoft JhengHei UI", 13, FontStyle.Bold);
            lblResult.ForeColor = Color.White;
            lblResult.Location = new Point(40, 405);
            lblResult.Size = new Size(760, 70);
            Controls.Add(lblResult);

        }

        private Button MakeButton(string text, int x, int y, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Microsoft JhengHei UI", 10, FontStyle.Bold);
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, 38);
            btn.BackColor = Color.White;
            btn.ForeColor = Color.FromArgb(30, 45, 60);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 230);
            return btn;
        }

        private void BtnDeal_Click(object sender, EventArgs e)
        {
            int bet;
            if (!int.TryParse(txtBet.Text.Trim(), out bet) || bet <= 0)
            {
                MessageBox.Show("下注金額必須是正整數。", "下注錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (bet > money)
            {
                MessageBox.Show("資金不足，請降低下注金額。", "下注錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            currentBet = bet;
            money -= bet;
            waitingForDraw = true;
            for (int i = 0; i < hold.Length; i++) hold[i] = false;
            ResetDeck();
            hand.Clear();
            for (int i = 0; i < 5; i++) hand.Add(DrawCard());
            lblResult.Text = "點擊想保留的牌，再按「換牌結算」。";
            UpdateUi();
        }

        private void BtnDraw_Click(object sender, EventArgs e)
        {
            if (!waitingForDraw || hand.Count != 5)
                return;

            for (int i = 0; i < 5; i++)
            {
                if (!hold[i])
                    hand[i] = DrawCard();
            }

            PokerResult result = EvaluateHand(hand);
            int payout = currentBet * result.Rate;
            money += payout;
            waitingForDraw = false;
            lblResult.Text = "結果：" + result.Name + "，賠率 x" + result.Rate + "，獲得 " + payout + " 元。" + (payout == 0 ? " 再試一次！" : "");
            UpdateUi();
        }

        private void Card_Click(object sender, EventArgs e)
        {
            if (!waitingForDraw) return;
            Button btn = sender as Button;
            if (btn == null) return;
            int index = (int)btn.Tag;
            hold[index] = !hold[index];
            UpdateUi();
        }

        private void ClearRound()
        {
            waitingForDraw = false;
            currentBet = 0;
            hand.Clear();
            for (int i = 0; i < hold.Length; i++) hold[i] = false;
            lblResult.Text = "請輸入下注金額後開始。";
        }

        private void UpdateUi()
        {
            lblMoney.Text = "資金：" + money + " 元";
            btnDraw.Enabled = waitingForDraw;
            btnDeal.Enabled = !waitingForDraw;
            txtBet.Enabled = !waitingForDraw;

            for (int i = 0; i < cardButtons.Length; i++)
            {
                if (i < hand.Count)
                {
                    cardButtons[i].Text = hand[i].DisplayText;
                    cardButtons[i].ForeColor = hand[i].IsRed ? Color.Firebrick : Color.FromArgb(20, 25, 30);
                    cardButtons[i].FlatAppearance.BorderColor = hold[i] ? Color.Gold : Color.WhiteSmoke;
                    cardButtons[i].BackColor = hold[i] ? Color.FromArgb(255, 250, 205) : Color.White;
                }
                else
                {
                    cardButtons[i].Text = "?";
                    cardButtons[i].ForeColor = Color.FromArgb(30, 30, 35);
                    cardButtons[i].FlatAppearance.BorderColor = Color.WhiteSmoke;
                    cardButtons[i].BackColor = Color.White;
                }
            }
        }

        private void ResetDeck()
        {
            deck.Clear();
            string[] suits = new string[] { "♠", "♥", "♦", "♣" };
            for (int s = 0; s < suits.Length; s++)
                for (int r = 2; r <= 14; r++)
                    deck.Add(new PokerCard(suits[s], r));

            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                PokerCard temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
        }

        private PokerCard DrawCard()
        {
            PokerCard card = deck[0];
            deck.RemoveAt(0);
            return card;
        }

        private PokerResult EvaluateHand(List<PokerCard> cards)
        {
            List<int> ranks = cards.Select(c => c.Rank).OrderBy(x => x).ToList();
            bool flush = cards.All(c => c.Suit == cards[0].Suit);
            bool straight = IsStraight(ranks);
            var groups = cards.GroupBy(c => c.Rank).Select(g => g.Count()).OrderByDescending(c => c).ToList();

            if (flush && ranks.SequenceEqual(new List<int> { 10, 11, 12, 13, 14 })) return new PokerResult("Royal Flush", 250);
            if (flush && straight) return new PokerResult("Straight Flush", 50);
            if (groups[0] == 4) return new PokerResult("Four of a Kind", 25);
            if (groups[0] == 3 && groups.Count > 1 && groups[1] == 2) return new PokerResult("Full House", 9);
            if (flush) return new PokerResult("Flush", 6);
            if (straight) return new PokerResult("Straight", 4);
            if (groups[0] == 3) return new PokerResult("Three of a Kind", 3);
            if (groups[0] == 2 && groups.Count > 1 && groups[1] == 2) return new PokerResult("Two Pair", 2);
            if (groups[0] == 2) return new PokerResult("One Pair", 1);
            return new PokerResult("High Card", 0);
        }

        private bool IsStraight(List<int> ranks)
        {
            List<int> distinct = ranks.Distinct().OrderBy(x => x).ToList();
            if (distinct.Count != 5) return false;
            if (distinct.SequenceEqual(new List<int> { 2, 3, 4, 5, 14 })) return true;
            for (int i = 1; i < distinct.Count; i++)
                if (distinct[i] != distinct[i - 1] + 1)
                    return false;
            return true;
        }

        private class PokerCard
        {
            public string Suit;
            public int Rank;
            public PokerCard(string suit, int rank) { Suit = suit; Rank = rank; }
            public bool IsRed { get { return Suit == "♥" || Suit == "♦"; } }
            public string DisplayText { get { return RankText + "\n" + Suit; } }
            private string RankText
            {
                get
                {
                    if (Rank <= 10) return Rank.ToString();
                    if (Rank == 11) return "J";
                    if (Rank == 12) return "Q";
                    if (Rank == 13) return "K";
                    return "A";
                }
            }
        }

        private class PokerResult
        {
            public string Name;
            public int Rate;
            public PokerResult(string name, int rate) { Name = name; Rate = rate; }
        }
    }
}
