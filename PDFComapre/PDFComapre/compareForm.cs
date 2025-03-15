using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CompareUtility;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading;
using NPOI.SS.Formula.Functions;
using System.Web.UI;
using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using MetroFramework.Controls;

namespace PDFCompare
{
	public partial class compareForm : Form
	{
		public compareForm()
		{
			InitializeComponent();
			this.WindowState = FormWindowState.Maximized;
			materialTabControl1.SelectedTab = selectFilesTabPage;
			materialTabSelector1.BaseTabControl = materialTabControl1;
			labelErrorMessage.Visible = false;
			progressBar1.Visible = false;
			copyrightLabel.Text = $"© {DateTime.Now.Year} COFORGE | www.Coforge.com";

		}


		private Dictionary<string, ComboBox> multipleSourceComboBox = new Dictionary<string, ComboBox>();
		private Dictionary<string, ComboBox> multipleTargetComboBox = new Dictionary<string, ComboBox>();
		private Dictionary<string, TextBox> multipleSourceRangeTextBox = new Dictionary<string, TextBox>();
		private Dictionary<string, TextBox> multipleTargetRangeTextBox = new Dictionary<string, TextBox>();

		private Dictionary<string, Label> multipleResultStatus = new Dictionary<string, Label>();
		private Dictionary<string, WebBrowser> multipleWebResults = new Dictionary<string, WebBrowser>();
		private Dictionary<string, DataGridView> multipleDatagridViews = new Dictionary<string, DataGridView>();
		private string resultPath = ConfigurationManager.AppSettings["ResultPath"].ToString();

		private int currentResultIndex = 0;
		private int initialResultIndex = 0;
		private int lastResultIndex = 0;

		private void btnCompare_Click(object sender, EventArgs e)
		{
			// Dont start comparing if source or target in combobox is not selected
			for (int i = 0; i < multipleSourceComboBox.Count; i++)
			{
				if (multipleSourceComboBox[$"sourceComboBox_{i}"] != null && 
					(multipleSourceComboBox[$"sourceComboBox_{i}"].SelectedItem == null || 
					multipleTargetComboBox[$"targetComboBox_{i}"].SelectedItem == null))
				{
					return;
				}
			}


			//labelResultPath.Text = string.Empty;
			labelResultPath.Text = $"Results will be saved at: {resultPath}";

			// Start comparing only if there is some file selected
			if (!backgroundWorker1.IsBusy && (multipleSourceComboBox.Count > 0 && multipleTargetComboBox.Count > 0))
			{
				btnCompare.Cursor = Cursors.No;
				btnCompare.Enabled = false;
				progressBar1.Visible = true; //show the progress bar
				progressBar1.Value = 0;
				multipleWebResults.Clear();
				multipleDatagridViews.Clear();
				multipleResultStatus.Clear();
				resultContentPanel.Controls.Clear();

				//Create datagridview or webbrowser to be used in background worker's DoWork event
				for (int i = 0; i < multipleSourceComboBox.Count; i++)
				{
					if (multipleSourceComboBox[$"sourceComboBox_{i}"] == null || multipleTargetComboBox[$"targetComboBox_{i}"] == null)
					{
						continue;
					}

					// Create textbox for status message
					Label resultStatus = new Label
					{
						Name = $"resultStatus_{i}",
						BorderStyle = BorderStyle.None,
						//Size = new Size(30, 100),
						Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Visible = false,
						
					};

					resultContentPanel.Controls.Add(resultStatus);
					resultStatus.Dock = DockStyle.Top;
					// Store the result textbox in the Dictionary with its unique name
					multipleResultStatus.Add(resultStatus.Name, resultStatus);


					// Image comparison
					if (text_OR_imageBtn.Checked)
					{
						DataGridView datagridview = new DataGridView
						{
							Name = $"datagridview_{i}",
							AllowUserToAddRows = false,
							AllowUserToDeleteRows = false,
							AllowUserToResizeColumns = false,
							AllowUserToResizeRows = false,
							AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
							ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
							//Dock = DockStyle.Top,
							//Location = new Point(0, i * 300),
							MinimumSize = new Size(30, 31),
							Margin = new System.Windows.Forms.Padding(4, 5, 4, 5),
							Size = new Size(300, 300),
							ReadOnly = true,
							ShowCellErrors = false,
							ShowCellToolTips = false,
							ShowEditingIcon = false,
							ShowRowErrors = false,
							RowHeadersVisible = false,
						};

						resultContentPanel.Controls.Add(datagridview);
						datagridview.Dock = DockStyle.Fill;
						// Store the DataGridView in the Dictionary with its unique name
						multipleDatagridViews.Add(datagridview.Name, datagridview);
					}

					// Text comparison
					else
					{
						WebBrowser webResult = new WebBrowser
						{
							Name = $"webResult_{i}",
							//Dock = DockStyle.Top,
							//Location = new Point(0, i * 300),
							MinimumSize = new Size(30, 31),
							Margin = new System.Windows.Forms.Padding(4, 5, 4, 5),
							//Size = new Size(300, 300),

						};

						resultContentPanel.Controls.Add(webResult);
						webResult.Dock = DockStyle.Fill;
						// Store the WebBrowser in the Dictionary with its unique name
						multipleWebResults.Add(webResult.Name, webResult);
					}

				}

				backgroundWorker1.RunWorkerAsync();

			}            

		}

		private void CompareFiles(int i)
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => CompareFiles(i)));
				return;
			}

			var currentDrive = Path.GetPathRoot(System.Reflection.Assembly.GetEntryAssembly().Location);
			//var ComparisonReportFile = Path.Combine(ConfigurationManager.AppSettings["ResultPath"].ToString(), @"ComparisonReport.xls");
			var ComparisonReportFile = Path.Combine(resultPath, "ComparisonReport.xls");
			//"Q:\\Automation & Performance\\Functional Automation\\DCRegression\\Results\\Excess-Data\\PDF-Diff_Excel-Reports\\PDF_Comparator_Results\\ComparisonReport.xls";

			string File1diff = multipleSourceComboBox[$"sourceComboBox_{i}"].SelectedItem.ToString();
			string File2diff = multipleTargetComboBox[$"targetComboBox_{i}"].SelectedItem.ToString();

			string sourcePageRange = multipleSourceRangeTextBox[$"sourceRangeTextBox_{i}"].Text;
			string targetPageRange = multipleTargetRangeTextBox[$"targetRangeTextBox_{i}"].Text;


			if (text_OR_imageBtn.Checked)
			{
				try
				{
					if (!string.IsNullOrEmpty(File1diff.Trim()) && !string.IsNullOrEmpty(File2diff.Trim()))
					{

						var result = Program.fnPDFDiff_FormTemplate(File1diff, File2diff, ComparisonReportFile, sourcePageRange, targetPageRange, true);

						if (result.Message.Split('|')[1] != "Page Numbers are Not Same" && result.Message.Split('|')[1] != "Both PDF are same." && result.Message.Split('|')[1] != "Please provide source and target same range to compare due to different number of pages.")
						{
							string resultImage = Path.Combine(resultPath, "Reports");

							DirectoryInfo resultfolder = new DirectoryInfo(resultImage);
							DirectoryInfo latestdir = resultfolder.GetDirectories().OrderByDescending(f => f.CreationTime).FirstOrDefault();

							ImageList imageList = new ImageList();
							string imagePath = Path.Combine(latestdir.FullName, "Differences");

							if (Directory.Exists(imagePath))
							{
								string[] filePaths = Directory.GetFiles(imagePath, "*.jpg").Where(x => x.Contains("CombinedDiff")).ToArray();
								ShowImages(filePaths, i);
							}
						}

						if (result.Message.Split('|')[1] == "Please provide source and target same range to compare due to different number of pages.")
						{
							labelErrorMessage.Text = "Please provide source and target same range to compare due to different number of pages.";
							labelErrorMessage.BackColor = Color.Red;
							labelErrorMessage.Visible = true;
							labelResultPath.Visible = false;

						}

						if (result.Message.Split('|')[2] != string.Empty && result.Message.Split('|')[2] == "True" && result.Message.Split('|')[1] != "Please provide source and target same range to compare due to different number of pages.")
						{
							multipleResultStatus[$"resultStatus_{i}"].Text = "Pass";
							multipleResultStatus[$"resultStatus_{i}"].BackColor = Color.Green;
							multipleResultStatus[$"resultStatus_{i}"].Visible = true;
						}
						else if (result.Message.Split('|')[2] != string.Empty && result.Message.Split('|')[2] == "False" && result.Message.Split('|')[1] != "Please provide source and target same range to compare due to different number of pages.")
						{
							multipleResultStatus[$"resultStatus_{i}"].Text = "Fail";
							multipleResultStatus[$"resultStatus_{i}"].BackColor = Color.Red;
							multipleResultStatus[$"resultStatus_{i}"].Visible = true;
						}

					}
					else
					{
						labelErrorMessage.Text = "Files does not exist.";
						labelErrorMessage.BackColor = Color.Red;
						labelErrorMessage.Visible = true;
						labelResultPath.Visible = false;
					}

				}
				catch (Exception ex)
				{
					labelErrorMessage.Text = "Failure";
					labelErrorMessage.Visible = true;
					labelResultPath.Visible = false;
				}
			}
			else
			{
				try
				{
					if (!string.IsNullOrEmpty(File1diff.Trim()) && !string.IsNullOrEmpty(File2diff.Trim()))
					{
						PDFComparer pdfcompare = new PDFComparer();
						//List<int> list = Common.PagesToCompare(pagesToCompare);

						var result = Program.fnPDFDiff_FormTemplate(File1diff, File2diff, ComparisonReportFile, sourcePageRange, targetPageRange, false);

						string res = result.CompareText; //pdfcompare.CompareTwoPDFReport(File1diff, File2diff, list);
						multipleWebResults[$"webResult_{i}"].Visible = true;
						if (result.Message.Split('|')[2] != string.Empty && result.Message.Split('|')[2] == "True")
						{
							multipleResultStatus[$"resultStatus_{i}"].Text = "Pass";
							multipleResultStatus[$"resultStatus_{i}"].BackColor = Color.Green;
							multipleResultStatus[$"resultStatus_{i}"].Visible = true;
						}
						else if (result.Message.Split('|')[2] != string.Empty && result.Message.Split('|')[2] == "False")
						{
							multipleResultStatus[$"resultStatus_{i}"].Text = "Fail";
							multipleResultStatus[$"resultStatus_{i}"].BackColor = Color.Red;
							multipleResultStatus[$"resultStatus_{i}"].Visible = true;
						}

						if (res == "Files does not exist." || res == "Fail")
						{
							multipleWebResults[$"webResult_{i}"].Visible = false;
							labelErrorMessage.Text = res;
							labelErrorMessage.BackColor = Color.Red;
							labelErrorMessage.Visible = true;
							labelResultPath.Visible = false;
						}
						else
						{
							multipleWebResults[$"webResult_{i}"].DocumentText = res;
						}
					}
					else
					{
						labelErrorMessage.Text = "Files does not exist.";
						labelErrorMessage.BackColor = Color.Red;
						labelErrorMessage.Visible = true;
						labelResultPath.Visible = false;
					}
				}
				catch (Exception ex)
				{
					multipleWebResults[$"webResult_{i}"].Visible = false;
				}
			}

		}

		private void ShowImages(string[] files, int i)
		{
			if (files != null && files.Count() > 0)
			{
				multipleDatagridViews[$"datagridview_{i}"].Visible = true;
				DataTable table = new DataTable();
				table.Columns.Add("Images", typeof(Image));
				for (int j = 0; j < files.Count(); j++)
				{
					table.Rows.Add(Image.FromFile(files[j]));
				}

				multipleDatagridViews[$"datagridview_{i}"].AutoGenerateColumns = false;


				multipleDatagridViews[$"datagridview_{i}"].ColumnCount = 0;


				multipleDatagridViews[$"datagridview_{i}"].ColumnHeadersVisible = false;
				DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
				//imageColumn.Name = "Images";
				imageColumn.DataPropertyName = "Images";
				//imageColumn.HeaderText = "Images";
				imageColumn.ImageLayout = DataGridViewImageCellLayout.Stretch;
				multipleDatagridViews[$"datagridview_{i}"].Columns.Insert(0, imageColumn);
				multipleDatagridViews[$"datagridview_{i}"].RowTemplate.Height = 1000;
				//multipleDatagridViews[$"datagridview_{i}"].Columns[0].Width = 900;
				multipleDatagridViews[$"datagridview_{i}"].DataSource = table;
			}
			else
			{
				multipleDatagridViews[$"datagridview_{i}"].Visible = false;
			}

		}

		public Image ResizeImage(Image image, Size size, bool preserveAspectRatio = true)
		{
			int newWidth;
			int newHeight;
			if (preserveAspectRatio)
			{
				int originalWidth = image.Width;
				int originalHeight = image.Height;
				float percentWidth = (float)size.Width / (float)originalWidth;
				float percentHeight = (float)size.Height / (float)originalHeight;
				float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
				newWidth = (int)(originalWidth * percent);
				newHeight = (int)(originalHeight * percent);
			}
			else
			{
				newWidth = size.Width;
				newHeight = size.Height;
			}
			Image newImage = new Bitmap(newWidth, newHeight);
			using (Graphics graphicsHandle = Graphics.FromImage(newImage))
			{
				graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
			}
			return newImage;
		}


		private void CombineImages(string[] files, string imagePath)
		{
			string finalImage = Path.Combine(imagePath, "FinalImage.jpg");

			List<int> imageHeights = new List<int>();

			int nIndex = 0;
			int width = 0;
			int height = 0;

			foreach (var file in files)
			{
				Image img = Image.FromFile(file);

				imageHeights.Add(img.Height);
				width += img.Width;
				img.Dispose();
			}

			imageHeights.Sort();
			height = imageHeights[imageHeights.Count - 1];
			Bitmap img3 = new Bitmap(width, height);
			Graphics g = Graphics.FromImage(img3);
			g.Clear(SystemColors.AppWorkspace);

			foreach (var file in files)
			{
				Image img = Image.FromFile(file);
				if (nIndex == 0)
				{
					g.DrawImage(img, new Point(0, 0));
					nIndex++;
					width = img.Width;
				}
				else
				{
					g.DrawImage(img, new Point(width, 0));
					width += img.Width;
				}

				img.Dispose();
			}

			g.Dispose();
			img3.Save(finalImage, System.Drawing.Imaging.ImageFormat.Jpeg);
			img3.Dispose();

			//picResult.Image = Image.FromFile(finalImage);

		}

		private void btnbrwSource_Click(object sender, EventArgs e)
		{
			OpenFileDialog fileChooser = new OpenFileDialog();
			fileChooser.Filter = "Pdf Files|*.pdf| Word Files|*.doc; *.docx";
			fileChooser.Multiselect = true;
			fileChooser.Title = "Select Source File(s)";

			if (fileChooser.ShowDialog() == DialogResult.OK)
			{
				//txtSource.Text = fileChooser.FileName;
				txtSource.Text = string.Join(";", fileChooser.FileNames);
				btnbrwSource.Enabled = false;
				btnbrwSource.BackColor = Color.Gray;
				btnbrwSource.Text = "Files  Selected!";
			}

		}

		private void btnbrwTarget_Click(object sender, EventArgs e)
		{
			OpenFileDialog fileChooser = new OpenFileDialog();
			fileChooser.Filter = "Pdf Files|*.pdf| Word Files|*.doc; *.docx";
			fileChooser.Multiselect = true;
			fileChooser.Title = "Select Target File(s)";

			if (fileChooser.ShowDialog() == DialogResult.OK)
			{
				//txtTarget.Text = fileChooser.FileName;
				txtTarget.Text = string.Join(";", fileChooser.FileNames);
				btnbrwTarget.Enabled = false;
				btnbrwTarget.BackColor = Color.Gray;
				btnbrwTarget.Text = "Files  Selected!";
			}
		}


		private void txtComparingPagesNumber_KeyPress(object sender, KeyPressEventArgs e)
		{
			AllowOnlyNumbersToPressWithCommaAndDashSeparation(e);
		}

		private void targetRangeTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			AllowOnlyNumbersToPressWithCommaAndDashSeparation(e);
		}

		private static void AllowOnlyNumbersToPressWithCommaAndDashSeparation(KeyPressEventArgs e)
		{
			for (int h = 58; h <= 127; h++)
			{
				if (e.KeyChar == h)
				{
					e.Handled = true;
				}
			}
			for (int k = 32; k <= 47; k++)
			{
				if (e.KeyChar != 44 && e.KeyChar != 45)
				{
					if (e.KeyChar == k && e.KeyChar != 44)
					{
						e.Handled = true;
					}
				}
			}
		}


		// Event handler for review button click
		private void reviewButton_Click(object sender, EventArgs e)
		{
			if(txtSource.Text == string.Empty || txtTarget.Text == string.Empty)
			{
				return;
			}

			materialTabControl1.SelectedTab = reviewTabPage;

			// Clear existing controls
			reviewContentPanel.Controls.Clear();

			// Clear combobox and range dictionaries
			multipleSourceComboBox.Clear();
			multipleTargetComboBox.Clear();

			// Add panels with comboboxes and labels
			string[] sourceFiles = txtSource.Text.Split(';');
			string[] targetFiles = txtTarget.Text.Split(';');
			int minNoOfComarisons = Math.Min(sourceFiles.Length, targetFiles.Length);

			if (minNoOfComarisons > 0)
			{
				for (int i = 0; i < minNoOfComarisons; i++)
				{
					AddNewPanel(i, sourceFiles, targetFiles);
				}

				ValidateSourceAndTarget();
			}

		}


		//Function to create new section/panel in the review tab
		private void AddNewPanel(int i, string[] sourceFiles, string[] targetFiles)
		{
			Panel panel = new Panel
			{
				Dock = DockStyle.Top,
			};

			//************ Source ************//
			Label sourceLabel = new Label
			{
				Text = "Source:",
				Location = new Point(210, 10),
				AutoSize = true
			};

			ComboBox sourceComboBox = new ComboBox
			{
				Name = $"sourceComboBox_{i}",
				Location = new Point(300, 10),
				Size = new Size(250, 21),
				DropDownStyle = ComboBoxStyle.DropDownList,
            };
			sourceComboBox.Items.AddRange(sourceFiles);

			MetroButton sourceButton = new MetroButton
			{
				Text = ". . .",
				Location = new Point(560, 10),
				Size = new Size(24, 24),
            };
			sourceButton.Click += (s, ev) => OpenFileDialogForComboBox(sourceComboBox);
			//sourceButton.Click += (s, ev) => OpenFileDialogForComboBox_Validating(sourceComboBox, null);
			Label sourceRangeLabel = new Label
			{
				Text = "Select Range:",
				Location = new Point(210, 40),
				AutoSize = true,
			};


			TextBox sourceRangeTextBox = new TextBox
			{
				Name = $"sourceRangeTextBox_{i}",
				Location = new Point(300, 40),
				Size = new Size(250, 21),
                BorderStyle = BorderStyle.FixedSingle,
            };
            

            //************ Target ************//
            Label targetLabel = new Label
			{
				Text = "Target:",
				Location = new Point(610, 10),
				AutoSize = true
			};

			ComboBox targetComboBox = new ComboBox
			{
				Name = $"targetComboBox_{i}",
				Location = new Point(700, 10),
				Size = new Size(250, 21),
				DropDownStyle = ComboBoxStyle.DropDownList,
            };
			targetComboBox.Items.AddRange(targetFiles);

			if (sourceFiles.Length > 0 && targetFiles.Length > 0)
			{
				sourceComboBox.SelectedIndex = i;
				targetComboBox.SelectedIndex = i;
			}


			MetroButton targetButton = new MetroButton
			{
				Text = ". . .",
				Location = new Point(960, 10),
				Size = new Size(24, 24),
            };
			targetButton.Click += (s, ev) => OpenFileDialogForComboBox(targetComboBox);
			//targetButton.Click += (s, ev) => OpenFileDialogForComboBox_Validating(targetComboBox, null);
			Label targetRangeLabel = new Label
			{
				Text = "Select Range:",
				Location = new Point(610, 40),
				AutoSize = true,
			};

			TextBox targetRangeTextBox = new TextBox
			{
				Name = $"targetRangeTextBox_{i}",
				Location = new Point(700, 40),
				Size = new Size(250, 29),
                BorderStyle = BorderStyle.FixedSingle,
            };

			Button deleteButton = new Button
			{
				Size = new Size(30, 30),
				Text = "X",
				ForeColor = Color.Red,
				FlatStyle = FlatStyle.Flat,
				Location = new Point(1050, 10),
			};

			deleteButton.Click += (s, e) => DeletePanelButton_Click(panel, i);
            sourceRangeTextBox.TextChanged += (s, ev) => RangeTextBox_TextChanged(sourceComboBox, targetComboBox);
            targetRangeTextBox.TextChanged += (s, ev) => RangeTextBox_TextChanged(sourceComboBox, targetComboBox);

            panel.Controls.Add(sourceLabel);
			panel.Controls.Add(sourceComboBox);
			panel.Controls.Add(sourceButton);
			panel.Controls.Add(sourceRangeLabel);
			panel.Controls.Add(sourceRangeTextBox);
			panel.Controls.Add(targetLabel);
			panel.Controls.Add(targetComboBox);
			panel.Controls.Add(targetButton);
			panel.Controls.Add(targetRangeLabel);
			panel.Controls.Add(targetRangeTextBox);
			panel.Controls.Add(deleteButton);

			reviewContentPanel.Controls.Add(panel);
			reviewContentPanel.Controls.SetChildIndex(panel, 0);

			// Store the ComboBoxes in the Dictionary with their unique names
			multipleSourceComboBox.Add(sourceComboBox.Name, sourceComboBox);
			multipleSourceRangeTextBox.Add(sourceRangeTextBox.Name, sourceRangeTextBox);

			multipleTargetComboBox.Add(targetComboBox.Name, targetComboBox);
			multipleTargetRangeTextBox.Add(targetRangeTextBox.Name, targetRangeTextBox);

			sourceComboBox.SelectedIndexChanged += (s, ev) => combobox_SelectedIndexChanged();
			targetComboBox.SelectedIndexChanged += (s, ev) => combobox_SelectedIndexChanged();

        }

        private void RangeTextBox_TextChanged(ComboBox sourceComboBox, ComboBox targetComboBox)
        {
            // Change the ForeColor of the corresponding ComboBox
            sourceComboBox.ForeColor = Color.Black; // Change to the desired color
			targetComboBox.ForeColor = Color.Black;
		}


        //Event handler for addSection button click
        private void addSectionButton_Click(object sender, EventArgs e)
		{
			string[] dummy = new string[] {};
			AddNewPanel(multipleSourceComboBox.Count, dummy, dummy);
			ValidateSourceAndTarget();
		}


		//Event handler for comboboxes selected index changed
		private void combobox_SelectedIndexChanged()
		{
			ValidateSourceAndTarget();
		}


		//Function to check whether source and target are same or not
		private void ValidateSourceAndTarget()
		{

			bool srcTrgtSame = false;
			bool allComboBoxesFilled = true;
			bool pagesSame = true;
			PDFComparer pDFComaprer = new PDFComparer();

			for (int i = 0; i < multipleSourceComboBox.Count; i++)
			{
                // If it is a deleted combobox, skip it
                if (multipleSourceComboBox[$"sourceComboBox_{i}"] == null)
				{
					continue;
				}

                // The moment we find a combobox which is not filled, we break out of the loop
                if ((multipleSourceComboBox[$"sourceComboBox_{i}"] != null) && multipleSourceComboBox[$"sourceComboBox_{i}"].SelectedItem == null || multipleTargetComboBox[$"targetComboBox_{i}"].SelectedItem == null)
				{
					allComboBoxesFilled = false;
					break;
				}

				string sourceFilePath = multipleSourceComboBox[$"sourceComboBox_{i}"].SelectedItem.ToString();
				string targetFilePath = multipleTargetComboBox[$"targetComboBox_{i}"].SelectedItem.ToString();


                if (sourceFilePath == targetFilePath)
				{
					srcTrgtSame = true;
					multipleSourceComboBox[$"sourceComboBox_{i}"].ForeColor = Color.Red;
					multipleTargetComboBox[$"targetComboBox_{i}"].ForeColor = Color.Red;
				}
				else
				{
					multipleSourceComboBox[$"sourceComboBox_{i}"].ForeColor = Color.Black;
					multipleTargetComboBox[$"targetComboBox_{i}"].ForeColor = Color.Black;

					//Initialize GhostscriptVersionInfo and Rasterizer
					if (Environment.Is64BitOperatingSystem)
					{
						pDFComaprer._lastInstalledVersion = new GhostscriptVersionInfo(new Version(0, 0, 0), @"gsdll64.dll", string.Empty, GhostscriptLicense.GPL);
					}
					else
					{
                        pDFComaprer._lastInstalledVersion = new GhostscriptVersionInfo(new Version(0, 0, 0), @"gsdll32.dll", string.Empty, GhostscriptLicense.GPL);
                    }
                    pDFComaprer._rasterizer = new GhostscriptRasterizer();
				}

				//Check if the number of pages is the same
				if (text_OR_imageBtn.Checked)
				{
					pagesSame = false;
					multipleSourceComboBox[$"sourceComboBox_{i}"].ForeColor = Color.Red;
					multipleTargetComboBox[$"targetComboBox_{i}"].ForeColor = Color.Red;
				}
            }

			if (srcTrgtSame)
			{
				MessageBox.Show("Source and target cannot be same!!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				btnCompare.Enabled = false;

			}
			else if (!allComboBoxesFilled)
			{
				labelErrorMessage.Text = "Please select files for all comparisons.";
				labelErrorMessage.BackColor = Color.Red;
				labelErrorMessage.Visible = true;
				btnCompare.Enabled = false;
			}
			//else if (!pagesSame)
			//{
			//	MessageBox.Show("Source and target files must have the same number of pages!!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			//	//btnCompare.Enabled = false;
			//}
			else
			{
				allComboBoxesFilled = true;
				labelErrorMessage.Text = string.Empty;
				labelErrorMessage.Visible = false;
				btnCompare.Enabled = true;
			}
		}


		// Event handler for Select files in reviewtab
		private void OpenFileDialogForComboBox(ComboBox comboBox)
		{
			OpenFileDialog fileChooser = new OpenFileDialog
			{
				Filter = "Pdf Files|*.pdf| Word Files|*.doc; *.docx",
				Multiselect = false,
				Title = "Select File"
			};

			if (fileChooser.ShowDialog() == DialogResult.OK)
			{
				string selectedFile = fileChooser.FileName;
				if (!comboBox.Items.Contains(selectedFile))
				{
					comboBox.Items.Add(selectedFile);
				}
				comboBox.SelectedItem = selectedFile;

				//ValidateSourceAndTarget();
			}
		}
		//private void OpenFileDialogForComboBox_Validating(object sender, CancelEventArgs e)
		//{
		//	ValidateSourceAndTarget();
		//}

		// Event handler for delete button in review tab
		private void DeletePanelButton_Click(Panel panel, int i)
		{
			// Remove the panel from the form
			panel.Controls.Clear();
			reviewContentPanel.Controls.Remove(panel);

			// Make the associated controls from the dictionaries null
			multipleSourceComboBox[$"sourceComboBox_{i}"] = null;
			multipleTargetComboBox[$"targetComboBox_{i}"] = null;
			multipleSourceRangeTextBox[$"sourceRangeTextBox_{i}"] = null;
			multipleTargetRangeTextBox[$"targetRangeTextBox_{i}"] = null;

			labelErrorMessage.Text = string.Empty;
			labelErrorMessage.Visible = false;
			btnCompare.Enabled = true;
        }

		//************** Carousel Feature **************//
		private void ShowResult(int index)
		{
			if (multipleSourceComboBox[$"sourceComboBox_{index}"] == null || multipleTargetComboBox[$"targetComboBox_{index}"] == null)
			{
				return;
			}

			foreach (var key in multipleResultStatus.Keys)
			{
				multipleResultStatus[key].Visible = false;
			}
			foreach (var key in multipleWebResults.Keys)
			{
				multipleWebResults[key].Visible = false;
			}
			foreach (var key in multipleDatagridViews.Keys)
			{
				multipleDatagridViews[key].Visible = false;
			}

			if (multipleResultStatus.ContainsKey($"resultStatus_{index}"))
			{
				multipleResultStatus[$"resultStatus_{index}"].Visible = true;
			}
			if (multipleWebResults.ContainsKey($"webResult_{index}"))
			{
				multipleWebResults[$"webResult_{index}"].Visible = true;
			}
			if (multipleDatagridViews.ContainsKey($"datagridview_{index}"))
			{
				multipleDatagridViews[$"datagridview_{index}"].Visible = true;
			}

			currentResultIndex = index;
		}

		private void UpdateCarouselButtons()
		{
			previousResult.Enabled = !(currentResultIndex == initialResultIndex);
			nextResult.Enabled = !(currentResultIndex == lastResultIndex);
		}

		private void previousResult_Click(object sender, EventArgs e)
		{
			if (currentResultIndex > 0)
			{
				do
				{
					currentResultIndex--;
				}
				while (currentResultIndex >= 0 && (multipleSourceComboBox[$"sourceComboBox_{currentResultIndex}"] == null || multipleTargetComboBox[$"targetComboBox_{currentResultIndex}"] == null));
				ShowResult(currentResultIndex);
			}
			UpdateCarouselButtons();

		}

		private void nextResult_Click(object sender, EventArgs e)
		{
			if (currentResultIndex < multipleSourceComboBox.Count - 1)
			{
				do
				{
					currentResultIndex++;
				} while (currentResultIndex < multipleSourceComboBox.Count && (multipleSourceComboBox[$"sourceComboBox_{currentResultIndex}"] == null || multipleTargetComboBox[$"targetComboBox_{currentResultIndex}"] == null));

				ShowResult(currentResultIndex);
			}
			UpdateCarouselButtons();
		}


		private bool isUserInitiated = true;

		private void ResetSourceAndTargetFiles()
		{
			txtSource.Text = string.Empty;
			txtTarget.Text = string.Empty;
			btnbrwSource.Enabled = true;
			btnbrwSource.BackColor = Color.FromArgb(255, 255, 255);
			btnbrwSource.Text = "Select Source File(s)";
			btnbrwTarget.Enabled = true;
			btnbrwTarget.BackColor = Color.FromArgb(255, 255, 255);
			btnbrwTarget.Text = "Select Target File(s)";

			text_OR_imageBtn.Checked = false;

			reviewContentPanel.Controls.Clear();
			resultContentPanel.Controls.Clear();
			materialTabControl1.SelectedTab = selectFilesTabPage;

			multipleDatagridViews.Clear();
			multipleWebResults.Clear();

			multipleSourceComboBox.Clear();
			multipleTargetComboBox.Clear();
			multipleSourceRangeTextBox.Clear();
			multipleTargetRangeTextBox.Clear();
			multipleResultStatus.Clear();
			//labelErrorMessage.Clear();
			labelErrorMessage.Visible = false;
		}
		// Reset everything
		private void resetButton_Click(object sender, EventArgs e)
		{
			ResetSourceAndTargetFiles();

		}

		// Main logic to compare files inside do_work
		private void background_DoWork(object sender, DoWorkEventArgs e)
		{
			for (int i = 0; i < multipleSourceComboBox.Count; i++)
			{
				if (multipleSourceComboBox[$"sourceComboBox_{i}"] == null || multipleTargetComboBox[$"targetComboBox_{i}"] == null)
				{
					continue;
				}
				backgroundWorker1.ReportProgress((i + 1) * 100 / multipleSourceComboBox.Count);
				CompareFiles(i);
			}
		}

		private void background_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			progressBar1.Value = e.ProgressPercentage;
		}

		private void background_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			progressBar1.Visible = false;
			currentResultIndex = 0;

			while(multipleSourceComboBox[$"sourceComboBox_{currentResultIndex}"] == null)
			{
				currentResultIndex += 1;
			}

			initialResultIndex = currentResultIndex;
			ShowResult(currentResultIndex);

			materialTabControl1.SelectedTab = resultTabPage;
			btnCompare.Enabled = true;
			btnCompare.Cursor = Cursors.Default;

			lastResultIndex = multipleSourceComboBox.Count - 1;
			while (multipleSourceComboBox[$"sourceComboBox_{lastResultIndex}"] == null)
			{
				lastResultIndex -= 1;
			}

			previousResult.Enabled = false;
			nextResult.Enabled = !(initialResultIndex == lastResultIndex);
		}

		private void materialTabControl1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (isUserInitiated && materialTabControl1.SelectedTab == selectFilesTabPage)
			{
				ResetSourceAndTargetFiles();
			}

			if (materialTabControl1.SelectedTab == resultTabPage)
			{
				labelResultPath.Visible = true;
			}
			else
			{
				labelResultPath.Visible = false;
			}
		}

		private void labelResultPath_Click(object sender, EventArgs e)
		{
			if (Directory.Exists(resultPath))
			{
				System.Diagnostics.Process.Start("explorer.exe", resultPath);
			}
			else
			{
				MessageBox.Show("The specified folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void btnSelectResultPath_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the folder to save the results";
				if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
				{
					resultPath = Path.Combine(folderBrowserDialog.SelectedPath, "CompareToolResult");

					labelResultPathDisplay.Text = $"Results will be saved at: {resultPath} ";
					labelResultPath.Text = $"Results saved at: {resultPath}";
					MessageBox.Show($"Results will be saved at: {resultPath}", "Result Path Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

	}
}
