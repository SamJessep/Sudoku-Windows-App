﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku
{
    public partial class EditorForm : FormView, IEditor
    {
        private EditorController controller;

        public EditorForm()
        {
            InitializeComponent();
        }

        public new void Start()
        {
            Show();
        }

        public void SetController(EditorController c)
        {
            controller = c;
        }

        private void GenerateBtn_Click(object sender, EventArgs e)
        {
            controller.MakeGameTemplate();
        }

        private void LoadFile_Click(object sender, EventArgs e)
        {
            controller.LoadGameFile();

        }

        private void Export_Click(object sender, EventArgs e)
        {
            controller.ExportSudoku();
        }

        public int getSquareWidth() => (int)SquareWidth.Value;

        public int getSquareHeight() => (int)SquareHeight.Value;

        public int getTargetTime() => (int)TargetTime.Value;

        public void UpdateGameUI(GameSettings gameSettings)
        {
            SquareWidth.Value = gameSettings.SquareWidth;
            SquareHeight.Value = gameSettings.SquareHeight;
            TargetTime.Value = gameSettings.TargetTime;
        }

        public void ClearTemplate()
        {
            TemplateArea.Controls.Clear();
        }

        public void DrawTemplateComponent(Game game)
        {
            GameGrid GG = new GameGrid(game, 50, controller);
            Panel SudokuPanel = GG.MakeSudoku();
            EnableLockedCells(SudokuPanel);
            //Center add 10 px padding to top
            SudokuPanel.Location = new Point((TemplateArea.Width - SudokuPanel.Width) / 2, 10);
            TemplateArea.Controls.Add(SudokuPanel);
            ToggleExportBtn(true);
        }

        public void EnableLockedCells(Panel gamePanel)
        {
            foreach(Panel square in gamePanel.Controls["SudokuGame"].Controls)
            {
                foreach(Button cell in square.Controls)
                {
                    cell.Enabled = true;
                }
            }
        }

        public string GetSaveFilePath()
        {
            string path = string.Empty;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = "CSV|*.csv",
                Title = "Save the sudoku game file"
            };
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs =
                    (System.IO.FileStream)saveFileDialog1.OpenFile();
                Show("Sudoku Game saved to: " + fs.Name);
                fs.Close();
                path = fs.Name;
            }
            return path;
        }

        public void ToggleExportBtn(bool enabled)
        {
            Export.Enabled = enabled;
        }

    }
}
