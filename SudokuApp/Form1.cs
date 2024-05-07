using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SudokuApp
{
    public partial class Form1 : Form
    {
        //Initialization of Solver class and textboxes
        private SudokuSolver solver;
        private TextBox[,] textBoxes;

        //Form constructor
        public Form1()
        {
            InitializeComponent();
            InitializeSudokuBoard();
            InitializeDefaultSudokuBoard();
        }

        //Empty form loading
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //Sudoku board initialization (creating text boxes for for numbers)
        //Task-simplify creating textboxes and subtables..
        private void InitializeSudokuBoard()
        {
            textBoxes = new TextBox[9, 9]; //Initialize field of textboxes
            const int textBoxSize = 40;
            const int startX = 20;
            const int startY = 20;
            const int subTableSize = textBoxSize * 3 + 2; // subtable size to 3x3

            for (int subTableRow = 0; subTableRow < 3; subTableRow++)
            {
                for (int subTableCol = 0; subTableCol < 3; subTableCol++)
                {
                    // Creating subtable 3x3
                    Panel subTablePanel = new Panel
                    {
                        Width = subTableSize,
                        Height = subTableSize,
                        Location = new System.Drawing.Point(startX + subTableCol * (subTableSize + 2), startY + subTableRow * (subTableSize + 2)),
                        BorderStyle = BorderStyle.FixedSingle // Subtable border
                    };

                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 3; col++)
                        {
                            //Creating textbox in subtable
                            int absoluteRow = subTableRow * 3 + row;
                            int absoluteCol = subTableCol * 3 + col;

                            textBoxes[absoluteRow, absoluteCol] = new TextBox
                            {
                                Width = textBoxSize,
                                Height = textBoxSize,
                                Location = new System.Drawing.Point(col * (textBoxSize + 2), row * (textBoxSize + 2)),
                                TextAlign = HorizontalAlignment.Center,
                                Font = new System.Drawing.Font(Font.FontFamily, 14),
                                Tag = new Tuple<int, int>(absoluteRow, absoluteCol)
                            };
                            textBoxes[absoluteRow, absoluteCol].KeyPress += TextBox_KeyPress;
                            subTablePanel.Controls.Add(textBoxes[absoluteRow, absoluteCol]);
                        }
                    }

                    Controls.Add(subTablePanel);
                }
            }
        }

        // Set default numbers into the board
        //Task - creating random class for generating new combinations for each game?
        private void InitializeDefaultSudokuBoard()
        {
            int[,] defaultBoard = new int[9, 9] {
        {5, 3, 0, 0, 7, 0, 0, 0, 0},
        {6, 0, 0, 1, 9, 5, 0, 0, 0},
        {0, 9, 8, 0, 0, 0, 0, 6, 0},
        {8, 0, 0, 0, 6, 0, 0, 0, 3},
        {4, 0, 0, 8, 0, 3, 0, 0, 1},
        {7, 0, 0, 0, 2, 0, 0, 0, 6},
        {0, 6, 0, 0, 0, 0, 2, 8, 0},
        {0, 0, 0, 4, 1, 9, 0, 0, 5},
        {0, 0, 0, 0, 8, 0, 0, 7, 9}
        };
            // Set default numbers to textboxes
            SetBoardToTextBoxes(defaultBoard);

            // Disable textboxes for positions with default values
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (defaultBoard[row, col] != 0)
                    {
                        textBoxes[row, col].Enabled = false;
                    }
                }
            }
        }

        // Event handler for the key press in textboxes
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digits in the textboxes
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }

            // Allow only one digit from 1 to 9
            TextBox textBox = (TextBox)sender;
            if (!char.IsControl(e.KeyChar) && char.IsDigit(e.KeyChar))
            {
                int number = int.Parse(e.KeyChar.ToString());
                if (number < 1 || number > 9)
                {
                    e.Handled = true;
                }
                else
                {
                    textBox.Text = e.KeyChar.ToString();
                    e.Handled = true;
                }
            }
        }

        //Get the sudoku board from textboxes
        private int[,] GetBoardFromTextBoxes()
        {
            //2D array to hold the sudoku board
            int[,] board = new int[9, 9];

            //Loop all the textboxes and parse their content to integers
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (!int.TryParse(textBoxes[row, col].Text, out board[row, col]))
                    {
                        //If parsing fails, set value to 0
                        board[row, col] = 0;
                    }
                }
            }

            return board;
        }

        // Set the sudoku board to textboxes
        private void SetBoardToTextBoxes(int[,] board)
        {
            // Loop through all the textboxes and set their text based on the board values
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    textBoxes[row, col].Text = board[row, col] == 0 ? "" : board[row, col].ToString();
                }
            }
        }

        // Event handler for clicking the Solve button
        private void SolveButton_Click(object sender, EventArgs e)
        {
            //Get the sudoku board from textboxes
            int[,] board = GetBoardFromTextBoxes();
            //Create sudoku solver instance with the current board
            solver = new SudokuSolver(board);

            //Solve the sudoku puzzle
            if (solver.Solve())
            {
                //If solution exists, set solution to textboxes
                SetBoardToTextBoxes(solver.GetSolution());
            }
            else
            {
                //No solution - show messageBox
                MessageBox.Show("No solution exists.");
            }
        }

        //Event handler for clicking Retry button
        private void RetryButton_Click(object sender, EventArgs e)
        {
            //Reset sudoku to default values
            InitializeDefaultSudokuBoard();
        }

        // Method for saving games using serialization
        private void SaveGame(string fileName)
        {
            try
            {
                // Get sudoku board from textboxes
                int[,] board = GetBoardFromTextBoxes();

                // Board serialization to string
                string boardState = SerializeBoard(board);

                // Saving string to file
                File.WriteAllText(fileName, boardState);

                MessageBox.Show("Game saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save game: " + ex.Message);
            }
        }

        // Method for loading game using deserialization
        private void LoadGame(string fileName)
        {
            try
            {
                // Loading string from file
                string boardState = File.ReadAllText(fileName);

                // Deserialization string to sudoku board
                int[,] board = DeserializeBoard(boardState);

                // Setting board to textboxes
                SetBoardToTextBoxes(board);

                MessageBox.Show("Game loaded successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load game: " + ex.Message);
            }
        }

        // Event handler for clicking Save button
        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Getting game name
                string gameName = Microsoft.VisualBasic.Interaction.InputBox("Enter game name:", "Save Game", "SudokuGame");

                // If the game name is empty or whitespace, inform the user and exit the method
                if (string.IsNullOrWhiteSpace(gameName))
                {
                    MessageBox.Show("Please enter a valid game name.");
                    return;
                }

                // Getting the path to the directory where application is located
                string directoryPath = AppDomain.CurrentDomain.BaseDirectory;

                // Creating a directory for game data if it doesn't exist
                string gameDirectory = Path.Combine(directoryPath, "Saved Games");
                Directory.CreateDirectory(gameDirectory); // This will not throw an exception if the directory already exists

                // Constructing the file path for saving the game data
                string fileName = Path.Combine(gameDirectory, $"{gameName}.txt");

                // Saving the game data
                SaveGame(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save game: " + ex.Message);
            }
        }



        // Event handler for clicking Load button
        private void LoadButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Create combo box for selecting a game
                ComboBox gameComboBox = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    DisplayMember = "Name"
                };

                // Getting the path to the directory where application is located
                string directoryPath = AppDomain.CurrentDomain.BaseDirectory;

                // Directory for game data
                string gameDirectory = Path.Combine(directoryPath, "Saved Games");

                // Load saved games
                string[] files = Directory.GetFiles(gameDirectory, "*.txt");

                foreach (string file in files)
                {
                    string gameName = Path.GetFileNameWithoutExtension(file);
                    gameComboBox.Items.Add(gameName);
                }

                // If there are no saved games, end the method
                if (gameComboBox.Items.Count == 0)
                {
                    MessageBox.Show("No games found.");
                    return;
                }

                // Create dialog form with comboBox and OK/Cancel buttons
                Form dialogForm = new Form
                {
                    Text = "Load Game",
                    Size = new System.Drawing.Size(300, 150),
                    StartPosition = FormStartPosition.CenterParent
                };

                // Add comboBox to dialogForm
                gameComboBox.Location = new System.Drawing.Point(50, 30);
                dialogForm.Controls.Add(gameComboBox);

                // Add OK button
                Button okButton = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new System.Drawing.Point(50, 70)
                };
                dialogForm.Controls.Add(okButton);

                // Add Cancel button
                Button cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new System.Drawing.Point(150, 70)
                };
                dialogForm.Controls.Add(cancelButton);

                // Show dialog form and process the result
                DialogResult result = dialogForm.ShowDialog();

                if (result == DialogResult.OK && gameComboBox.SelectedItem != null)
                {
                    string selectedGameName = gameComboBox.SelectedItem.ToString();

                    // Load game from txt file and set board from loaded data
                    string fileName = Path.Combine(gameDirectory, $"{selectedGameName}.txt");
                    LoadGame(fileName);
                }
                else
                {
                    MessageBox.Show("No game selected or operation cancelled.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load game: " + ex.Message);
            }
        }



        // Serialization board state to string
        private string SerializeBoard(int[,] board)
        {
            StringBuilder sb = new StringBuilder();
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    sb.Append(board[row, col]);
                }
            }
            return sb.ToString();
        }

        // Deserialize string to a board state
        private int[,] DeserializeBoard(string boardString)
        {
            int[,] board = new int[9, 9];
            int index = 0;
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    board[row, col] = int.Parse(boardString[index].ToString());
                    index++;
                }
            }
            return board;
        }
    }
}
