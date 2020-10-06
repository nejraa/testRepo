using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using itextsharp.pdfa;
using iTextSharp.text;

namespace ESN100
{
	public partial class Form1 : Form
	{

		private FileInfo[] files;
		private FileInfo[] filesRotated;
		private DirectoryInfo dir;
		private DirectoryInfo dirRotated;
		private string MERGEDPDFFILESPATH = "";
		private string PDFFilesPATH = "";
		private string PATHROTATED;
		public Form1()
		{
			InitializeComponent();
		}
		private void button1_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog folderDlg = new FolderBrowserDialog();
			folderDlg.ShowNewFolderButton = true;

			DialogResult result = folderDlg.ShowDialog();
			if (result == DialogResult.OK)
			{
				textBox1.Text = folderDlg.SelectedPath;
				Environment.SpecialFolder root = folderDlg.RootFolder;
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			bool validation = true;
			PDFFilesPATH = textBox1.Text;
			PATHROTATED = PDFFilesPATH + "Rotated\\";
			try
			{
				if (!Directory.Exists(PATHROTATED))
				{
					DirectoryInfo di = Directory.CreateDirectory(PATHROTATED);
				}
			}
			catch (IOException ioex)
			{
				validation = false;
			}
			MERGEDPDFFILESPATH = PDFFilesPATH + "Merged\\";
			try
			{
				if (!Directory.Exists(MERGEDPDFFILESPATH))
				{
					DirectoryInfo di = Directory.CreateDirectory(MERGEDPDFFILESPATH);
				}
			}
			catch (IOException ioex)
			{
				validation = false;
			}

			if (validateUserEntry(PDFFilesPATH))
			{
				validation = rotatein90DegreesAllfiles();
				if (validation)
				{
					validation = mergeAndShowAllPDFFiles();
				}
			}
		}

		private bool validateUserEntry(string FolderPath)
		{
			// Checks the value of the text.
			if (FolderPath.Length == 0)
			{
				string message = "You did not provide valid folder path.";
				string caption = "Folder path error";
				MessageBoxButtons buttons = MessageBoxButtons.OK;
				DialogResult result;
				result = MessageBox.Show(message, caption, buttons);
				if (result == System.Windows.Forms.DialogResult.OK)
				{
					return false;
				}
			}
			else
			{
				dir = new DirectoryInfo(FolderPath);
				files = dir.GetFiles("*.pdf");
				int fileNum = files.Length;

				if (fileNum == 0)
				{
					string message = string.Concat("No PDF files in selected folder.");
					string caption = "PDF Files";
					MessageBoxButtons buttons = MessageBoxButtons.OK;
					DialogResult result;
					result = MessageBox.Show(message, caption, buttons);
					if (result == System.Windows.Forms.DialogResult.OK)
					{
						return false;
					}
				}
			}
			return true;
		}

		bool rotatein90DegreesAllfiles()
		{
			int rotateInDegree = -90;
			String myFile;
			int pagesCount;

			try
			{
				for (int i = 0; i < files.Length; i++)
				{
					myFile = files[i].FullName;
					PdfReader reader = new PdfReader(myFile);

					pagesCount = reader.NumberOfPages;
					for (int n = 1; n <= pagesCount; n++)
					{
						PdfDictionary page = reader.GetPageN(1);
						PdfNumber rotate = page.GetAsNumber(PdfName.ROTATE);
						page.Put(PdfName.ROTATE, new PdfNumber(rotateInDegree));
					}

					string filePath = PATHROTATED + files[i].Name;
					FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
					PdfStamper stamper = new PdfStamper(reader, fs);
					stamper.Close();
					reader.Close();
				}
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		bool mergeAndShowAllPDFFiles()
		{
			bool validation;
			dirRotated = new DirectoryInfo(PATHROTATED);
			filesRotated = dirRotated.GetFiles("*.pdf");
			int fileNum = filesRotated.Length;

			// merge files
			string pathMerged = MERGEDPDFFILESPATH + "mergedAllFiles.pdf"; 
			//string pathMerged = "C:/ElregPDFViewer/ESN100/ESN100_Echogram_PDF_filesMerged/testFileNejra.pdf";
			Document document = new Document();
			PdfImportedPage page = null;
			PdfReader reader = null;
			FileStream fos;
			try
			{
				fos = new FileStream(pathMerged, FileMode.Create, FileAccess.Write, FileShare.None);
			}
			catch (FileNotFoundException e)
			{
				return false;
			}
			
			try
			{
				PdfCopy pdf = new PdfCopy(document, fos);
				document.Open();
				for (int ii = 0; ii < filesRotated.Length; ii++)
				{
					reader = new PdfReader(filesRotated[ii].FullName);
					for (int i = 0; i < reader.NumberOfPages; i++)
					{
						page = pdf.GetImportedPage(reader, i + 1);
						pdf.AddPage(page);
					}
					pdf.FreeReader(reader);
					reader.Close();
				}
				return true;
			}
			catch (Exception)
			{
				if (reader != null)
				{
					reader.Close();
				}
				return false;
			}
			finally
			{
				if (document != null)
				{
					document.Close();
					axAcroPDF1.LoadFile(pathMerged);
					axAcroPDF1.setShowToolbar(false); //disable pdf toolbar.
					axAcroPDF1.Enabled = true;
				}
			}
			return true;
		}


		private void Form1_Load(object sender, EventArgs e)
		{
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			axAcroPDF1.Width = Screen.PrimaryScreen.WorkingArea.Width - 50;
			axAcroPDF1.Height = Screen.PrimaryScreen.WorkingArea.Height - 100;
		}
		private void Form1_Resize(object sender, EventArgs e)
		{
			// axAcroPDF1.Width = Screen.PrimaryScreen.WorkingArea.Width - 50;
			// axAcroPDF1.Height = Screen.PrimaryScreen.WorkingArea.Height - 100;
		}
	}
}
