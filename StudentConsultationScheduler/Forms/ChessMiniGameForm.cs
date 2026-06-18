using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace StudentConsultationScheduler.Forms
{
    public class ChessMiniGameForm : Form
    {
        private ChessBoardPanel boardPanel;
        private Label lblStatus;
        private ListBox lstMoves;
        private char[,] board = new char[8, 8];
        private readonly Stack<char[,]> history = new Stack<char[,]>();
        private Point? selected = null;
        private List<Point> legalMoves = new List<Point>();
        private bool whiteTurn = true;
        private bool gameOver = false;
        private int moveNo = 1;

        public ChessMiniGameForm()
        {
            InitializeUi();
            NewGame();
        }

        private void InitializeUi()
        {
            Text = "西洋棋小遊戲";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(980, 720);
            MinimumSize = new Size(900, 650);
            BackColor = Color.FromArgb(242, 246, 252);

            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 78;
            header.BackColor = Color.FromArgb(31, 63, 109);
            Controls.Add(header);

            Label title = new Label();
            title.Text = "西洋棋小遊戲";
            title.Font = new Font("Microsoft JhengHei UI", 20, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Location = new Point(22, 14);
            title.Size = new Size(260, 36);
            header.Controls.Add(title);

            Label hint = new Label();
            hint.Text = "點選棋子後會顯示合法位置；將軍、將死會自動提示。";
            hint.Font = new Font("Microsoft JhengHei UI", 9);
            hint.ForeColor = Color.FromArgb(222, 232, 246);
            hint.Location = new Point(25, 52);
            hint.Size = new Size(720, 22);
            header.Controls.Add(hint);

            Panel right = new Panel();
            right.Dock = DockStyle.Right;
            right.Width = 260;
            right.Padding = new Padding(12);
            right.BackColor = Color.White;
            Controls.Add(right);

            Button btnNew = MakeButton("新遊戲", 0);
            btnNew.Click += delegate { NewGame(); };
            right.Controls.Add(btnNew);

            Button btnUndo = MakeButton("悔棋", 48);
            btnUndo.Click += delegate { Undo(); };
            right.Controls.Add(btnUndo);

            lblStatus = new Label();
            lblStatus.Font = new Font("Microsoft JhengHei UI", 11, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(31, 63, 109);
            lblStatus.Location = new Point(12, 118);
            lblStatus.Size = new Size(230, 62);
            lblStatus.Text = "白方回合";
            right.Controls.Add(lblStatus);

            Label moveTitle = new Label();
            moveTitle.Text = "走棋紀錄";
            moveTitle.Font = new Font("Microsoft JhengHei UI", 10, FontStyle.Bold);
            moveTitle.Location = new Point(12, 184);
            moveTitle.Size = new Size(120, 24);
            right.Controls.Add(moveTitle);

            lstMoves = new ListBox();
            lstMoves.Font = new Font("Consolas", 10);
            lstMoves.Location = new Point(12, 212);
            lstMoves.Size = new Size(230, 430);
            right.Controls.Add(lstMoves);

            boardPanel = new ChessBoardPanel();
            boardPanel.Dock = DockStyle.Fill;
            boardPanel.Margin = new Padding(18);
            boardPanel.BoardProvider = delegate { return board; };
            boardPanel.SelectedProvider = delegate { return selected; };
            boardPanel.LegalProvider = delegate { return legalMoves; };
            boardPanel.MouseDown += BoardPanel_MouseDown;
            boardPanel.Resize += delegate { boardPanel.Invalidate(); };
            Controls.Add(boardPanel);
            boardPanel.BringToFront();
        }

        private Button MakeButton(string text, int y)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Microsoft JhengHei UI", 10, FontStyle.Bold);
            btn.Location = new Point(12, 14 + y);
            btn.Size = new Size(230, 36);
            btn.BackColor = Color.FromArgb(36, 105, 190);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void NewGame()
        {
            board = new char[8, 8];
            string black = "♜♞♝♛♚♝♞♜";
            string white = "♖♘♗♕♔♗♘♖";
            for (int c = 0; c < 8; c++)
            {
                board[0, c] = black[c];
                board[1, c] = '♟';
                board[6, c] = '♙';
                board[7, c] = white[c];
            }
            history.Clear();
            lstMoves.Items.Clear();
            selected = null;
            legalMoves.Clear();
            whiteTurn = true;
            gameOver = false;
            moveNo = 1;
            UpdateStatus(false);
            boardPanel.Invalidate();
        }

        private void BoardPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (gameOver)
            {
                MessageBox.Show("這局已結束，請按『新遊戲』重新開始。", "遊戲結束", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Point cell;
            if (!boardPanel.PointToCell(e.Location, out cell)) return;
            int r = cell.Y;
            int c = cell.X;
            char piece = board[r, c];

            if (selected.HasValue)
            {
                Point target = new Point(c, r);
                if (ContainsPoint(legalMoves, target))
                {
                    MovePiece(selected.Value.Y, selected.Value.X, r, c);
                    return;
                }
            }

            if (piece != '\0' && IsWhite(piece) == whiteTurn)
            {
                selected = new Point(c, r);
                legalMoves = GetSafeLegalMoves(r, c);
            }
            else
            {
                selected = null;
                legalMoves.Clear();
            }
            boardPanel.Invalidate();
        }

        private void MovePiece(int sr, int sc, int tr, int tc)
        {
            history.Push(CloneBoard(board));
            char piece = board[sr, sc];
            char captured = board[tr, tc];
            board[tr, tc] = piece;
            board[sr, sc] = '\0';

            if (piece == '♙' && tr == 0) board[tr, tc] = '♕';
            if (piece == '♟' && tr == 7) board[tr, tc] = '♛';

            string move = (whiteTurn ? moveNo.ToString() + ". " : "... ") + PieceName(piece) + " " + CellName(sc, sr) + " → " + CellName(tc, tr);
            if (captured != '\0') move += " x " + PieceName(captured);
            lstMoves.Items.Add(move);
            if (!whiteTurn) moveNo++;
            whiteTurn = !whiteTurn;
            selected = null;
            legalMoves.Clear();
            SystemSounds.Asterisk.Play();
            EvaluatePositionAfterMove(true);
            boardPanel.Invalidate();
        }

        private void Undo()
        {
            if (history.Count == 0) return;
            board = history.Pop();
            if (lstMoves.Items.Count > 0) lstMoves.Items.RemoveAt(lstMoves.Items.Count - 1);
            whiteTurn = !whiteTurn;
            moveNo = Math.Max(1, (lstMoves.Items.Count / 2) + 1);
            selected = null;
            legalMoves.Clear();
            gameOver = false;
            UpdateStatus(IsKingInCheck(whiteTurn));
            boardPanel.Invalidate();
        }

        private void EvaluatePositionAfterMove(bool showMessage)
        {
            bool inCheck = IsKingInCheck(whiteTurn);
            bool hasMove = HasAnySafeMove(whiteTurn);

            if (inCheck && !hasMove)
            {
                gameOver = true;
                string loser = whiteTurn ? "白方" : "黑方";
                string winner = whiteTurn ? "黑方" : "白方";
                lblStatus.Text = loser + "被將死\n" + winner + "獲勝";
                if (showMessage)
                {
                    MessageBox.Show(loser + "被將軍且沒有任何合法移動，" + winner + "獲勝！", "將死通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            if (!inCheck && !hasMove)
            {
                gameOver = true;
                lblStatus.Text = "和棋\n無合法移動";
                if (showMessage)
                {
                    MessageBox.Show("目前沒有任何合法移動，但國王沒有被將軍，判定為和棋。", "和棋通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            gameOver = false;
            UpdateStatus(inCheck);
        }

        private void UpdateStatus(bool inCheck)
        {
            string side = whiteTurn ? "白方" : "黑方";
            lblStatus.Text = side + "回合" + (inCheck ? "\n目前被將軍！" : "\n點選自己的棋子移動");
        }

        private List<Point> GetSafeLegalMoves(int r, int c)
        {
            List<Point> result = new List<Point>();
            char piece = board[r, c];
            if (piece == '\0') return result;
            bool movingWhite = IsWhite(piece);
            List<Point> candidates = GetLegalMoves(r, c);

            foreach (Point target in candidates)
            {
                char targetPiece = board[target.Y, target.X];
                if (IsKingPiece(targetPiece))
                    continue;

                char[,] clone = CloneBoard(board);
                clone[target.Y, target.X] = clone[r, c];
                clone[r, c] = '\0';
                if (clone[target.Y, target.X] == '♙' && target.Y == 0) clone[target.Y, target.X] = '♕';
                if (clone[target.Y, target.X] == '♟' && target.Y == 7) clone[target.Y, target.X] = '♛';

                if (!IsKingInCheckOnBoard(clone, movingWhite))
                    result.Add(target);
            }
            return result;
        }

        private bool HasAnySafeMove(bool whiteSide)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (board[r, c] != '\0' && IsWhite(board[r, c]) == whiteSide)
                    {
                        if (GetSafeLegalMoves(r, c).Count > 0)
                            return true;
                    }
                }
            }
            return false;
        }

        private bool IsKingInCheck(bool whiteKing)
        {
            return IsKingInCheckOnBoard(board, whiteKing);
        }

        private bool IsKingInCheckOnBoard(char[,] targetBoard, bool whiteKing)
        {
            char king = whiteKing ? '♔' : '♚';
            int kr = -1;
            int kc = -1;
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (targetBoard[r, c] == king)
                    {
                        kr = r;
                        kc = c;
                        break;
                    }
                }
                if (kr >= 0) break;
            }
            if (kr < 0) return true;
            return IsSquareAttacked(targetBoard, kr, kc, !whiteKing);
        }

        private bool IsSquareAttacked(char[,] targetBoard, int targetR, int targetC, bool byWhite)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    char p = targetBoard[r, c];
                    if (p == '\0' || IsWhite(p) != byWhite)
                        continue;
                    if (PieceAttacksSquare(targetBoard, r, c, targetR, targetC))
                        return true;
                }
            }
            return false;
        }

        private bool PieceAttacksSquare(char[,] targetBoard, int r, int c, int targetR, int targetC)
        {
            char p = targetBoard[r, c];
            bool white = IsWhite(p);
            char lower = NormalizePiece(p);
            int dr = targetR - r;
            int dc = targetC - c;

            if (lower == 'p')
            {
                int dir = white ? -1 : 1;
                return dr == dir && Math.Abs(dc) == 1;
            }
            if (lower == 'n')
            {
                return (Math.Abs(dr) == 2 && Math.Abs(dc) == 1) || (Math.Abs(dr) == 1 && Math.Abs(dc) == 2);
            }
            if (lower == 'k')
            {
                return Math.Abs(dr) <= 1 && Math.Abs(dc) <= 1 && (dr != 0 || dc != 0);
            }
            if (lower == 'b')
            {
                return Math.Abs(dr) == Math.Abs(dc) && PathClear(targetBoard, r, c, targetR, targetC);
            }
            if (lower == 'r')
            {
                return (dr == 0 || dc == 0) && PathClear(targetBoard, r, c, targetR, targetC);
            }
            if (lower == 'q')
            {
                bool diagonal = Math.Abs(dr) == Math.Abs(dc);
                bool straight = dr == 0 || dc == 0;
                return (diagonal || straight) && PathClear(targetBoard, r, c, targetR, targetC);
            }
            return false;
        }

        private bool PathClear(char[,] targetBoard, int sr, int sc, int tr, int tc)
        {
            int dr = Math.Sign(tr - sr);
            int dc = Math.Sign(tc - sc);
            int r = sr + dr;
            int c = sc + dc;
            while (r != tr || c != tc)
            {
                if (targetBoard[r, c] != '\0')
                    return false;
                r += dr;
                c += dc;
            }
            return true;
        }

        private List<Point> GetLegalMoves(int r, int c)
        {
            List<Point> moves = new List<Point>();
            char p = board[r, c];
            if (p == '\0') return moves;
            bool white = IsWhite(p);
            char lower = NormalizePiece(p);

            if (lower == 'p')
            {
                int dir = white ? -1 : 1;
                int startRow = white ? 6 : 1;
                AddPawnMoves(moves, r, c, dir, startRow, white);
            }
            else if (lower == 'n')
            {
                int[,] dirs = new int[,] { { -2, -1 }, { -2, 1 }, { -1, -2 }, { -1, 2 }, { 1, -2 }, { 1, 2 }, { 2, -1 }, { 2, 1 } };
                for (int i = 0; i < dirs.GetLength(0); i++) AddStep(moves, r + dirs[i, 0], c + dirs[i, 1], white);
            }
            else if (lower == 'b')
            {
                AddSlide(moves, r, c, -1, -1, white); AddSlide(moves, r, c, -1, 1, white); AddSlide(moves, r, c, 1, -1, white); AddSlide(moves, r, c, 1, 1, white);
            }
            else if (lower == 'r')
            {
                AddSlide(moves, r, c, -1, 0, white); AddSlide(moves, r, c, 1, 0, white); AddSlide(moves, r, c, 0, -1, white); AddSlide(moves, r, c, 0, 1, white);
            }
            else if (lower == 'q')
            {
                AddSlide(moves, r, c, -1, -1, white); AddSlide(moves, r, c, -1, 1, white); AddSlide(moves, r, c, 1, -1, white); AddSlide(moves, r, c, 1, 1, white);
                AddSlide(moves, r, c, -1, 0, white); AddSlide(moves, r, c, 1, 0, white); AddSlide(moves, r, c, 0, -1, white); AddSlide(moves, r, c, 0, 1, white);
            }
            else if (lower == 'k')
            {
                for (int dr = -1; dr <= 1; dr++) for (int dc = -1; dc <= 1; dc++) if (dr != 0 || dc != 0) AddStep(moves, r + dr, c + dc, white);
            }
            return moves;
        }

        private void AddPawnMoves(List<Point> moves, int r, int c, int dir, int startRow, bool white)
        {
            if (Inside(r + dir, c) && board[r + dir, c] == '\0')
            {
                moves.Add(new Point(c, r + dir));
                if (r == startRow && board[r + 2 * dir, c] == '\0') moves.Add(new Point(c, r + 2 * dir));
            }
            if (Inside(r + dir, c - 1) && board[r + dir, c - 1] != '\0' && IsWhite(board[r + dir, c - 1]) != white) moves.Add(new Point(c - 1, r + dir));
            if (Inside(r + dir, c + 1) && board[r + dir, c + 1] != '\0' && IsWhite(board[r + dir, c + 1]) != white) moves.Add(new Point(c + 1, r + dir));
        }

        private void AddStep(List<Point> moves, int r, int c, bool white)
        {
            if (!Inside(r, c)) return;
            if (board[r, c] == '\0' || IsWhite(board[r, c]) != white) moves.Add(new Point(c, r));
        }

        private void AddSlide(List<Point> moves, int r, int c, int dr, int dc, bool white)
        {
            r += dr; c += dc;
            while (Inside(r, c))
            {
                if (board[r, c] == '\0') moves.Add(new Point(c, r));
                else
                {
                    if (IsWhite(board[r, c]) != white) moves.Add(new Point(c, r));
                    break;
                }
                r += dr; c += dc;
            }
        }

        private bool Inside(int r, int c) { return r >= 0 && r < 8 && c >= 0 && c < 8; }
        private bool IsWhite(char p) { return "♔♕♖♗♘♙".IndexOf(p) >= 0; }
        private bool IsKingPiece(char p) { return p == '♔' || p == '♚'; }
        private char NormalizePiece(char p)
        {
            if (p == '♙' || p == '♟') return 'p';
            if (p == '♘' || p == '♞') return 'n';
            if (p == '♗' || p == '♝') return 'b';
            if (p == '♖' || p == '♜') return 'r';
            if (p == '♕' || p == '♛') return 'q';
            return 'k';
        }
        private string PieceName(char p) { return p.ToString(); }
        private string CellName(int c, int r) { return ((char)('a' + c)).ToString() + (8 - r).ToString(); }
        private bool ContainsPoint(List<Point> points, Point p) { foreach (Point q in points) if (q == p) return true; return false; }

        private char[,] CloneBoard(char[,] src)
        {
            char[,] clone = new char[8, 8];
            Array.Copy(src, clone, src.Length);
            return clone;
        }

        private class ChessBoardPanel : Panel
        {
            public Func<char[,]> BoardProvider;
            public Func<Point?> SelectedProvider;
            public Func<List<Point>> LegalProvider;
            private readonly Font pieceFont = new Font("Segoe UI Symbol", 34, FontStyle.Regular, GraphicsUnit.Pixel);
            private readonly Font coordFont = new Font("Microsoft JhengHei UI", 8, FontStyle.Bold);

            public ChessBoardPanel()
            {
                DoubleBuffered = true;
                BackColor = Color.FromArgb(242, 246, 252);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Rectangle rect = GetBoardRect();
                int cell = rect.Width / 8;
                char[,] b = BoardProvider == null ? null : BoardProvider();
                List<Point> legal = LegalProvider == null ? new List<Point>() : LegalProvider();
                Point? selected = SelectedProvider == null ? null : SelectedProvider();

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (SolidBrush border = new SolidBrush(Color.FromArgb(31, 63, 109))) e.Graphics.FillRectangle(border, rect.X - 6, rect.Y - 6, cell * 8 + 12, cell * 8 + 12);

                for (int r = 0; r < 8; r++)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        Rectangle cellRect = new Rectangle(rect.X + c * cell, rect.Y + r * cell, cell, cell);
                        Color square = ((r + c) % 2 == 0) ? Color.FromArgb(238, 216, 183) : Color.FromArgb(153, 102, 61);
                        using (SolidBrush brush = new SolidBrush(square)) e.Graphics.FillRectangle(brush, cellRect);

                        if (selected.HasValue && selected.Value.X == c && selected.Value.Y == r)
                        {
                            using (SolidBrush brush = new SolidBrush(Color.FromArgb(180, 255, 230, 60))) e.Graphics.FillRectangle(brush, cellRect);
                        }
                        foreach (Point p in legal)
                        {
                            if (p.X == c && p.Y == r)
                            {
                                using (SolidBrush brush = new SolidBrush(Color.FromArgb(170, 70, 190, 95))) e.Graphics.FillEllipse(brush, cellRect.X + cell / 2 - 11, cellRect.Y + cell / 2 - 11, 22, 22);
                            }
                        }

                        if (b != null && b[r, c] != '\0')
                        {
                            string text = b[r, c].ToString();
                            SizeF size = e.Graphics.MeasureString(text, pieceFont);
                            using (SolidBrush brush = new SolidBrush(IsWhitePiece(b[r, c]) ? Color.WhiteSmoke : Color.FromArgb(20, 20, 25)))
                            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                            {
                                float px = cellRect.X + (cell - size.Width) / 2;
                                float py = cellRect.Y + (cell - size.Height) / 2 + 2;
                                e.Graphics.DrawString(text, pieceFont, shadow, px + 1, py + 1);
                                e.Graphics.DrawString(text, pieceFont, brush, px, py);
                            }
                        }

                        if (r == 7)
                        {
                            using (SolidBrush brush = new SolidBrush(Color.FromArgb(80, 20, 20, 20))) e.Graphics.DrawString(((char)('a' + c)).ToString(), coordFont, brush, cellRect.X + 4, cellRect.Bottom - 16);
                        }
                        if (c == 0)
                        {
                            using (SolidBrush brush = new SolidBrush(Color.FromArgb(80, 20, 20, 20))) e.Graphics.DrawString((8 - r).ToString(), coordFont, brush, cellRect.X + 4, cellRect.Y + 3);
                        }
                    }
                }
            }

            public bool PointToCell(Point location, out Point cellPoint)
            {
                Rectangle rect = GetBoardRect();
                int cell = rect.Width / 8;
                cellPoint = Point.Empty;
                if (!rect.Contains(location)) return false;
                int c = (location.X - rect.X) / cell;
                int r = (location.Y - rect.Y) / cell;
                if (c < 0 || c >= 8 || r < 0 || r >= 8) return false;
                cellPoint = new Point(c, r);
                return true;
            }

            private Rectangle GetBoardRect()
            {
                int size = Math.Min(ClientSize.Width - 42, ClientSize.Height - 42);
                if (size < 320) size = Math.Min(ClientSize.Width, ClientSize.Height);
                size = size - (size % 8);
                int x = (ClientSize.Width - size) / 2;
                int y = (ClientSize.Height - size) / 2;
                return new Rectangle(x, y, size, size);
            }

            private bool IsWhitePiece(char p) { return "♔♕♖♗♘♙".IndexOf(p) >= 0; }
        }
    }
}
