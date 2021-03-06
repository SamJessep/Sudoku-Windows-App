﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Security;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public partial class SudokuGameForm : FormView, IGameView
    {
        private const int BoxWidth = 50;
        GameController controller;
        Panel GamePanel;
        private int textFontSize = 20;

        public SudokuGameForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void OpenBtn_Clicked(object sender, EventArgs e)
        {
            controller.SelectGame();
        }

        private void SaveBtn_Clicked(object sender, EventArgs e)
        {
            controller.SaveGame();
        }

        private void CheckForComplete(object sender, EventArgs e)
        {
            if (controller.game.isPuzzleCompleted())
            {
                controller.GameWon();
                UpdateHighScore();
            }
        }

        private void EditorBtn_Clicked(object sender, EventArgs e)
        {
            controller.StartEditor();
        }

        private void ResetBtn_Clicked(object sender, EventArgs e)
        {
            controller.ResetGame();
        }

        public new void SetController(GameController theController)
        {
            controller = theController;
        }

        public new void Start()
        {
            Application.EnableVisualStyles();
            Application.Run(this);
        }

        public new void Stop()
        {
            Close();
        }

        public void DrawSudoku(Game game)
        {
            int H = 80 + BoxWidth * (game.gridHeight + 2);
            int W = 40 + BoxWidth * (game.gridWidth + 1);
            SetWindowSize(W, H);
            GamePanel = new GameGrid(game, 50, controller).MakeSudoku();
            GamePanel.Location = new Point((Width - GamePanel.Width) / 2, MenuPanel.Height);
            Controls.Add(GamePanel);
            UpdateHighScore();
            AddCellValidator(GamePanel);
            ValidateCells(this, EventArgs.Empty);
        }

        public (bool, bool) ChooseGame()
        {
            LoadGameForm f = new LoadGameForm(this, controller.game);
            bool gameWasChosen = f.ShowDialog() == DialogResult.OK;
            bool loadingGameSave = f.loadingGameSave;
            ToggleGameMenu(gameWasChosen);
            return (gameWasChosen, loadingGameSave);
        }

        public void ResetGame()
        {
            Controls.Remove(Controls["Sudoku"]);
            DrawSudoku(controller.game);
            controller.StartTimer();
            UpdateTime();
        }

        public void UpdateTime()
        {
            Label TimeLbl = GameTimer;
            TimeLbl.Text = controller.GetTimeAsString();
        }

        public void UpdateHighScore()
        {
            Label HighScoreLbl = Highscore;
            HighScoreLbl.Text = "Highscore: " + controller.game.highScore;
        }

        public void ToggleGameMenu(bool visible)
        {
            GameHUD.Visible = visible;
        }

        private void SetWindowSize(int w, int h)
        {
            Width = w;
            Height = h;
        }

        public void ClearGameScreen()
        {
            if (Controls.ContainsKey("Sudoku"))
            {
                Controls["Sudoku"].Controls.Clear();
                Controls.Remove(Controls["Sudoku"]);
            }
        }


        private void ValidateCells(object sender, EventArgs e)
        {
            //loop through each square, one loop check one square col and row
            for(int i = 0; i< controller.game.numberOfSquares; i++)
            {
                List<Button> rowColList = new List<Button>();
                Panel square = (Panel)GamePanel.Controls["SudokuGame"].Controls[i];

                bool SquareValid = controller.game.SquareValid(i);
                bool ColValid = controller.game.ColumnValid(i);
                bool RowValid = controller.game.RowValid(i);

                
                rowColList.AddRange(GetButtons(i));
                rowColList.AddRange(GetButtons(i, true));

                ShowSquareStatus(square.Controls.OfType<Button>(), SquareValid);
                ShowRowColStatus(i, ColValid, RowValid);
            }
        }

        public Label AddLabel(string name, string text, int row, int column)
        {
            int x = BoxWidth * column;
            int y = BoxWidth * row;
            int fontSize = text.ToCharArray().Length > 1 ? (int)(textFontSize * 0.75) : textFontSize;
            Label l = new Label
            {
                Name = name + row + "_" + column,
                Height = BoxWidth,
                Width = BoxWidth,
                Font = new Font("Arial", fontSize),
                Text = text,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = true,
                Location = new Point(x, y)
            };
            return l;
        }

        private void ShowSquareStatus(IEnumerable<Button> btns, bool valid)
        {
            foreach (Button btn in btns)
            {
                btn.BackColor = valid ? Color.Green : Color.White;
            }
        }

        private List<Button> GetButtons(int index , bool isCol = false)
        {
            int nameIndex = isCol ? 1 : 0;
            List<Button> btns = new List<Button>();
            foreach (Panel square in GamePanel.Controls["SudokuGame"].Controls)
            {
                foreach(Button cell in square.Controls)
                {
                    if (index == int.Parse(cell.Name.Split('_')[nameIndex]))
                    {
                        btns.Add(cell);
                    }
                }
            }
            return btns;
        }
        
        private List<Button> GetButtonsFromSquare(int squareIndex)
        {
            List<Button> btns = new List<Button>();
            foreach (Button cell in GamePanel.Controls["SudokuGame"].Controls[squareIndex.ToString()].Controls)
            {
              btns.Add(cell);
            }
            return btns;
        }

        public dynamic GetButtonByName(string name)
        {
            foreach (Panel square in GamePanel.Controls["SudokuGame"].Controls)
            {
                foreach (Button cell in square.Controls)
                {
                    if (cell.Name == name)
                    {
                        return cell;
                    }
                }
            }
            return false;
        }

        private void AddCellValidator(Panel GamePanel)
        {
            int n = controller.game.numberOfSquares;
            for(int i = 0; i<n; i++)
            {
                GamePanel.Controls.Add(MakeTick(n,i));
                GamePanel.Controls.Add(MakeTick(i,n));
                Control square = GamePanel.Controls["SudokuGame"].Controls[i];
                foreach (Button cell in square.Controls)
                {
                    cell.Click += ValidateCells;
                    cell.Click += CheckForComplete;
                }
            }
        }

        private Control MakeTick(int row, int col)
        {
            PictureBox tick = new PictureBox
            {
                Name = row + "_" + col,
                BackgroundImage = Image.FromFile("..//..//Images//check.png"),
                Size = new Size(50, 50),
                Location = new Point(GamePanel.Controls["SudokuGame"].Left + col*50, row*50),
                BackgroundImageLayout = ImageLayout.Zoom,
                Visible = false
            };
            return tick;
        }

        private void ShowRowColStatus(int index, bool colValid, bool rowValid)
        {
            PictureBox Coltick = (PictureBox)GamePanel.Controls[controller.game.numberOfSquares + "_" + index];
            PictureBox Rowtick = (PictureBox)GamePanel.Controls[index + "_" + controller.game.numberOfSquares];
            Rowtick.Visible = (Rowtick.Name.Split('_')[0] == index.ToString() && rowValid);
            Coltick.Visible = (Coltick.Name.Split('_')[1] == index.ToString() && colValid);
        }

        private void HintBtn_Click(object sender, EventArgs e)
        {
            int[] hint = controller.game.GetHint();
            if (hint.Length > 1)
            {
                int row = controller.game.GetRowByIndex(hint[0]);
                int col = controller.game.GetColumnByIndex(hint[0]);
                Button btn = (Button)GetButtonByName(row + "_" + col);
                btn.BackColor = Color.Yellow;
                btn.Text = hint[1].ToString();
                controller.game.SetCell(hint[1], hint[0]);
                CheckForComplete(this, EventArgs.Empty);
            }
        }

        public void UpdateCellOnView(int value, int index)
        {
            int col = controller.game.GetColumnByIndex(index);
            int row = controller.game.GetRowByIndex(index);
            Button btn = GetButtonByName(row + "_" + col);
            btn.Text = value == 0 ? "" : value.ToString();
        }
    }
}
