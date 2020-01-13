// Soul Reaver PC Texture Utility
// Copyright 2006-2012 Ben Lincoln
// http://www.thelostworlds.net/
//

// This file is part of Soul Reaver PC Texture Utility.

// Soul Reaver PC Texture Utility is free software: you can redistribute it and/or modify
// it under the terms of version 3 of the GNU General Public License as published by
// the Free Software Foundation.

// Soul Reaver PC Texture Utility is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Soul Reaver PC Texture Utility (in the file LICENSE.txt).  
// If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Threading;
using System.Text;

namespace SoulReaverPCTextureUtility
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class frmMain : System.Windows.Forms.Form
	{
        delegate void mDelegate(string nStatus);
        delegate void controlDelegate();

		//threads for opening/exporting files and getting the current texture
		protected Thread openFileThread;
		protected Thread exportThread;
		protected Thread getTextureThread;

		//file objects for IO
		protected FileStream fStream;
		protected BinaryReader bReader;
		protected BinaryWriter bWriter;

		//path to the textures file
		protected string filePath;

		//is the texture file open?
		protected bool fileIsOpen;

		//does the display image need to be updated?
		protected bool imageUpdated;

		//buffer image for the picturebox
		protected Bitmap bufferImage;

		//length of the file in bytes
		protected ulong fileLength;

		//number of textures
		protected uint totalTextures;

		//current texture
		protected uint currentTexture;

		//path to export to
		protected string exportPath;

		//path to import from
		protected string importPath;

        //for doing things the thread-safe way
        protected string currentTextureText;
        protected int currentTextureIndex;

		//form elements
		internal System.Windows.Forms.MainMenu mnuMenu;
		internal System.Windows.Forms.MenuItem mnuFile;
		internal System.Windows.Forms.MenuItem mnuOpenFile;
		internal System.Windows.Forms.MenuItem mnuExit;
		internal System.Windows.Forms.MenuItem mnuExport;
		internal System.Windows.Forms.MenuItem mnuExportCurrent;
		internal System.Windows.Forms.MenuItem mnuExportAll;
		internal System.Windows.Forms.MenuItem mnuImport;
		internal System.Windows.Forms.MenuItem mnuImportTexture;
		internal System.Windows.Forms.MenuItem mnuHelp;
		internal System.Windows.Forms.MenuItem mnuAbout;
		internal System.Windows.Forms.Timer tmrClock;
		internal System.Windows.Forms.GroupBox gboTextureSet;
		internal System.Windows.Forms.Button cmdGo;
		internal System.Windows.Forms.ComboBox cboCurrentTexture;
		internal System.Windows.Forms.PictureBox imgDisplay;
		internal System.Windows.Forms.Panel pnlStatus;
		internal System.Windows.Forms.Label lblStatus;
		private System.ComponentModel.IContainer components;

		public frmMain()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			fileIsOpen = false;
			imageUpdated = false;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));
			this.mnuMenu = new System.Windows.Forms.MainMenu();
			this.mnuFile = new System.Windows.Forms.MenuItem();
			this.mnuOpenFile = new System.Windows.Forms.MenuItem();
			this.mnuExit = new System.Windows.Forms.MenuItem();
			this.mnuExport = new System.Windows.Forms.MenuItem();
			this.mnuExportCurrent = new System.Windows.Forms.MenuItem();
			this.mnuExportAll = new System.Windows.Forms.MenuItem();
			this.mnuImport = new System.Windows.Forms.MenuItem();
			this.mnuImportTexture = new System.Windows.Forms.MenuItem();
			this.mnuHelp = new System.Windows.Forms.MenuItem();
			this.mnuAbout = new System.Windows.Forms.MenuItem();
			this.tmrClock = new System.Windows.Forms.Timer(this.components);
			this.gboTextureSet = new System.Windows.Forms.GroupBox();
			this.cmdGo = new System.Windows.Forms.Button();
			this.cboCurrentTexture = new System.Windows.Forms.ComboBox();
			this.imgDisplay = new System.Windows.Forms.PictureBox();
			this.pnlStatus = new System.Windows.Forms.Panel();
			this.lblStatus = new System.Windows.Forms.Label();
			this.gboTextureSet.SuspendLayout();
			this.pnlStatus.SuspendLayout();
			this.SuspendLayout();
			// 
			// mnuMenu
			// 
			this.mnuMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					this.mnuFile,
																					this.mnuExport,
																					this.mnuImport,
																					this.mnuHelp});
			// 
			// mnuFile
			// 
			this.mnuFile.Index = 0;
			this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					this.mnuOpenFile,
																					this.mnuExit});
			this.mnuFile.Text = "&File";
			// 
			// mnuOpenFile
			// 
			this.mnuOpenFile.Index = 0;
			this.mnuOpenFile.Text = "&Open Texture File";
			this.mnuOpenFile.Click += new System.EventHandler(this.mnuOpenFile_Click);
			// 
			// mnuExit
			// 
			this.mnuExit.Index = 1;
			this.mnuExit.Text = "E&xit";
			this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
			// 
			// mnuExport
			// 
			this.mnuExport.Enabled = false;
			this.mnuExport.Index = 1;
			this.mnuExport.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.mnuExportCurrent,
																					  this.mnuExportAll});
			this.mnuExport.Text = "&Export";
			// 
			// mnuExportCurrent
			// 
			this.mnuExportCurrent.Index = 0;
			this.mnuExportCurrent.Text = "&Current Texture Set";
			this.mnuExportCurrent.Click += new System.EventHandler(this.mnuExportCurrent_Click);
			// 
			// mnuExportAll
			// 
			this.mnuExportAll.Index = 1;
			this.mnuExportAll.Text = "&All Texture Sets";
			this.mnuExportAll.Click += new System.EventHandler(this.mnuExportAll_Click);
			// 
			// mnuImport
			// 
			this.mnuImport.Enabled = false;
			this.mnuImport.Index = 2;
			this.mnuImport.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.mnuImportTexture});
			this.mnuImport.Text = "&Import";
			// 
			// mnuImportTexture
			// 
			this.mnuImportTexture.Index = 0;
			this.mnuImportTexture.Text = "&Replace Current Texture Set";
			this.mnuImportTexture.Click += new System.EventHandler(this.mnuImportTexture_Click);
			// 
			// mnuHelp
			// 
			this.mnuHelp.Index = 3;
			this.mnuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					this.mnuAbout});
			this.mnuHelp.Text = "&Help";
			// 
			// mnuAbout
			// 
			this.mnuAbout.Index = 0;
			this.mnuAbout.Text = "&About";
			this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
			// 
			// tmrClock
			// 
			this.tmrClock.Tick += new System.EventHandler(this.tmrClock_Tick);
			// 
			// gboTextureSet
			// 
			this.gboTextureSet.Controls.Add(this.cmdGo);
			this.gboTextureSet.Controls.Add(this.cboCurrentTexture);
			this.gboTextureSet.Location = new System.Drawing.Point(8, 8);
			this.gboTextureSet.Name = "gboTextureSet";
			this.gboTextureSet.Size = new System.Drawing.Size(256, 56);
			this.gboTextureSet.TabIndex = 5;
			this.gboTextureSet.TabStop = false;
			this.gboTextureSet.Text = "Texture Set";
			// 
			// cmdGo
			// 
			this.cmdGo.Enabled = false;
			this.cmdGo.Location = new System.Drawing.Point(168, 24);
			this.cmdGo.Name = "cmdGo";
			this.cmdGo.Size = new System.Drawing.Size(72, 23);
			this.cmdGo.TabIndex = 1;
			this.cmdGo.Text = "Go";
			this.cmdGo.Click += new System.EventHandler(this.cmdGo_Click);
			// 
			// cboCurrentTexture
			// 
			this.cboCurrentTexture.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboCurrentTexture.Location = new System.Drawing.Point(16, 24);
			this.cboCurrentTexture.Name = "cboCurrentTexture";
			this.cboCurrentTexture.Size = new System.Drawing.Size(136, 21);
			this.cboCurrentTexture.TabIndex = 0;
			// 
			// imgDisplay
			// 
			this.imgDisplay.BackColor = System.Drawing.Color.Black;
			this.imgDisplay.Location = new System.Drawing.Point(8, 72);
			this.imgDisplay.Name = "imgDisplay";
			this.imgDisplay.Size = new System.Drawing.Size(256, 256);
			this.imgDisplay.TabIndex = 4;
			this.imgDisplay.TabStop = false;
			// 
			// pnlStatus
			// 
			this.pnlStatus.Controls.Add(this.lblStatus);
			this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlStatus.Location = new System.Drawing.Point(0, 329);
			this.pnlStatus.Name = "pnlStatus";
			this.pnlStatus.Size = new System.Drawing.Size(272, 32);
			this.pnlStatus.TabIndex = 3;
			// 
			// lblStatus
			// 
			this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblStatus.Location = new System.Drawing.Point(0, 0);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(272, 32);
			this.lblStatus.TabIndex = 0;
			this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// frmMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(272, 361);
			this.Controls.Add(this.pnlStatus);
			this.Controls.Add(this.gboTextureSet);
			this.Controls.Add(this.imgDisplay);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mnuMenu;
			this.Name = "frmMain";
			this.Text = "Soul Reaver PC Texture Utility";
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.gboTextureSet.ResumeLayout(false);
			this.pnlStatus.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new frmMain());
		}

		#region Menu options

		private void mnuOpenFile_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog fDialogue;
			DialogResult result;
			fDialogue = new OpenFileDialog();
			fDialogue.CheckFileExists = true;
			fDialogue.CheckPathExists = true;
			fDialogue.Multiselect = false;
			fDialogue.Title = "Select the Textures.BIG file to open...";
			result = fDialogue.ShowDialog();
			if (result == DialogResult.OK)
			{
				filePath = fDialogue.FileName;
                //openFileThread = new Thread(new ThreadStart(openFile));
                //openFileThread.Start();
                openFile();
			}
		}

		private void mnuExit_Click(object sender, System.EventArgs e)
		{
			closeFile();
            try
            {
                exportThread.Abort();
            }
            catch (Exception ex)
            {
                //do nothing
            }
            try
            {
                openFileThread.Abort();
            }
            catch (Exception ex)
            {
                //do nothing
            }
            try
            {
                getTextureThread.Abort();
            }
            catch (Exception ex)
            {
                //do nothing
            }
			Application.Exit();
		}

		private void mnuExportCurrent_Click(object sender, System.EventArgs e)
		{
			SaveFileDialog saveDialogue;
			DialogResult result;
			saveDialogue = new SaveFileDialog();
	        saveDialogue.AddExtension = true;
			saveDialogue.DefaultExt = "*.PNG";
			saveDialogue.FileName = "Texture-" + zeroFill(cboCurrentTexture.SelectedIndex.ToString(), 5) + ".PNG";
			saveDialogue.Filter = "Portable Network Graphic Images (*.PNG)|*.PNG";
			result = saveDialogue.ShowDialog();
			if (result == DialogResult.OK)
			{
	            exportPath = saveDialogue.FileName;
                exportThread = new Thread(new ThreadStart(exportCurrent));
                exportThread.Start();
                //exportCurrent();
			}
		}

		private void mnuExportAll_Click(object sender, System.EventArgs e)
		{
		    FolderBrowserDialog folderDialogue;
			DialogResult result;
			folderDialogue = new FolderBrowserDialog();
			folderDialogue.ShowNewFolderButton = true;
			result = folderDialogue.ShowDialog();
			if (result == DialogResult.OK)
			{
            exportPath = folderDialogue.SelectedPath;
            exportThread = new Thread(new ThreadStart(exportAll));
            exportThread.Start();
            //exportAll();
			}
		}

		private void mnuImportTexture_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog fDialogue;
			DialogResult result;
			fDialogue = new OpenFileDialog();
			fDialogue.CheckFileExists = true;
			fDialogue.CheckPathExists = true;
			fDialogue.Multiselect = false;
			fDialogue.Title = "Select the PNG file to import...";
			result = fDialogue.ShowDialog();
			if (result == DialogResult.OK)
			{
				currentTexture = (uint)cboCurrentTexture.SelectedIndex;
				importPath = fDialogue.FileName;
                openFileThread = new Thread(new ThreadStart(importTexture));
                openFileThread.Start();
 			}
		}

		private void mnuAbout_Click(object sender, System.EventArgs e)
		{
            MessageBox.Show("Soul Reaver PC Texture Utility v2.3\nCopyright 2006-2007 Ben Lincoln\nhttp://www.thelostworlds.net/", "About This Application", MessageBoxButtons.OK, MessageBoxIcon.None);
		}

		#endregion

		#region Utility functions

		private string zeroFill(string origVal, int length)
		{
			string retString;

			retString = origVal;

			do
				retString = "0" + retString;
			while (retString.Length < length);

	        return retString;
		}

		public void updateStatus(string newStatus)
		{
            lblStatus.Text = "Status: " + newStatus;
		}

		private void closeFile()
		{
			bWriter.Close();
			bReader.Close();
			fStream.Close();
		}

		private void updateTexDropdown()
		{
			int iUT;
			cboCurrentTexture.Items.Clear();
			for (iUT = 0; iUT <= totalTextures; iUT++)
			{
				cboCurrentTexture.Items.Add(iUT);
			}
        cboCurrentTexture.Text = "0";
		}

		private void enableControls()
		{
			mnuExport.Enabled = true;
			mnuImport.Enabled = true;
			cmdGo.Enabled = true;
            mnuOpenFile.Enabled = true;
        }

		private void disableControls()
		{
			mnuExport.Enabled = false;
			mnuImport.Enabled = false;
			cmdGo.Enabled = false;
            mnuOpenFile.Enabled = false;
        }

		#endregion

		#region Form events

		private void frmMain_Load(object sender, System.EventArgs e)
		{
			tmrClock.Enabled = true;
	        updateStatus("Ready");
		}

		private void tmrClock_Tick(object sender, System.EventArgs e)
		{
			if (imageUpdated)
			{
				imageUpdated = false;
				imgDisplay.Image = bufferImage;
			}
		}

		#endregion

		#region File IO

		protected void openFile()
		{
            mDelegate dcUpdateStatus = new mDelegate(updateStatus);
            this.Invoke(dcUpdateStatus, "Opening file");

			try
			{
	            fStream = new FileStream(filePath, FileMode.Open);
	            bReader = new BinaryReader(fStream);
				bWriter = new BinaryWriter(fStream);
			}
			catch (IOException ioEx)
			{
				MessageBox.Show("There was an error opening the file. Make sure it is not in use.", "IO Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			//get file length/number of textures
			FileInfo fInfo;
			fInfo = new FileInfo(filePath);
			fileLength = (ulong)fInfo.Length;

			//number of textures = (filelength - header(4096)) / (256 * 256 * 2)
			totalTextures = (uint)(fileLength - 4096) / (256 * 256 * 2) - 1;

			updateTexDropdown();
			enableControls();
            this.Invoke(dcUpdateStatus, "Ready");
		}

		private void exportCurrent()
		{
            mDelegate dcUpdateStatus = new mDelegate(updateStatus);
            this.Invoke(dcUpdateStatus, "Exporting current texture");
			try
			{
				bufferImage.Save(exportPath, System.Drawing.Imaging.ImageFormat.Png);
			}
			catch (Exception ex)
			{
				MessageBox.Show("There was an error saving the file. Try a different location and/or filename.", "IO Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
            this.Invoke(dcUpdateStatus, "Ready");
		}

		private void exportAll()
		{
            mDelegate dcUpdateStatus = new mDelegate(updateStatus);
            controlDelegate cDisable = new controlDelegate(disableControls);
            controlDelegate cEnable = new controlDelegate(enableControls);
            this.Invoke(cDisable);

			int iEA, jEA;
			Bitmap tempBitmap;

			for (iEA = 0; iEA <= totalTextures; iEA++)
			{
                this.Invoke(dcUpdateStatus, "Exporting texture " + iEA + " of " + totalTextures); 
 				tempBitmap = getTexture((long)(4096 + (iEA * 256 * 256 * 2)));
                tempBitmap.Save(exportPath + "\\Texture-" + zeroFill(iEA.ToString(), 5) + ".PNG", System.Drawing.Imaging.ImageFormat.Png);
			}
            this.Invoke(cEnable);
            this.Invoke(dcUpdateStatus, "Ready");
        }

		#endregion

		#region Image functions

		private void displayCurrentTexture()
		{
            mDelegate dcUpdateStatus = new mDelegate(updateStatus);
            this.Invoke(dcUpdateStatus, "Reading texture");
             //updateStatus("Reading texture: " + cboCurrentTexture.SelectedText);
            bufferImage = getTexture((long)(4096 + (currentTextureIndex * 256 * 256 * 2)));
			imageUpdated = true;
            //updateStatus("Ready");
            this.Invoke(dcUpdateStatus, "Ready");
       }

		private Bitmap getTexture(long offset)
		{
			ushort iGT, jGT;
			ushort a, r, g, b, pixelData;
			int aFactor, rFactor, gFactor, bFactor;
			Bitmap retBitmap;
			Color colour;

			aFactor = 8;
			rFactor = 3;
			gFactor = 3;
			bFactor = 3;

	        colour = new Color();
	        retBitmap = new Bitmap(256, 256);

			fStream.Seek(offset, SeekOrigin.Begin);

			for (iGT = 0; iGT <= 255; iGT++)
			{
				for (jGT = 0; jGT <= 255; jGT++)
				{
	                pixelData = bReader.ReadUInt16();
					a = pixelData;
					r = pixelData;
					g = pixelData;
					b = pixelData;

					//separate out the channels
					a >>= 15;

					r <<= 1;
					r >>= 11;

					g <<= 6;
					g >>= 11;

					b <<= 11;
					b >>= 11;

                    if (a > 0)
                    {
                        a = (ushort)255;
                    }
					r  = (ushort)(r << rFactor);
					g  = (ushort)(g << gFactor);
					b  = (ushort)(b << bFactor);

					colour = Color.FromArgb(a, r, g, b);
					retBitmap.SetPixel(jGT, iGT, colour);
				}
			}

			return retBitmap;
		}

		private void importTexture()
		{
            mDelegate dcUpdateStatus = new mDelegate(updateStatus);
            
			disableControls();
			mnuOpenFile.Enabled = false;
            this.Invoke(dcUpdateStatus, "Importing replacement texture");

			Bitmap tempBitmap;
			tempBitmap = new Bitmap(importPath);
			if ((tempBitmap.Size.Width != 256) || (tempBitmap.Size.Height != 256))
			{
				MessageBox.Show("You MUST use a PNG image that is 256x256 pixels", "Incorrect File Format", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			int iGT, jGT;
			ushort a, r, g, b, pixelData;
			int aFactor, rFactor, gFactor, bFactor;
			Color colour;

			aFactor = 1;
			rFactor = 3;
			gFactor = 3;
			bFactor = 3;

			colour = new Color();

			fStream.Seek(4096 + (currentTexture * 256 * 256 * 2), SeekOrigin.Begin);

			for (iGT = 0; iGT <= 255; iGT++)
			{
				for (jGT = 0; jGT <= 255; jGT++)
				{
					colour = tempBitmap.GetPixel(jGT, iGT);
					a = (ushort)(colour.A >> aFactor);
					r = (ushort)(colour.R >> rFactor);
					g = (ushort)(colour.G >> gFactor);
					b = (ushort)(colour.B >> bFactor);

					a <<= 15;
					r <<= 10;
					g <<= 5;

					pixelData = (ushort)(a | r | g | b);

					bWriter.Write(pixelData);
				}
			}

            this.Invoke(dcUpdateStatus, "Loading rewritten texture");

			cboCurrentTexture.SelectedIndex = (int)currentTexture;

			bufferImage = getTexture(4096 + (currentTexture * 256 * 256 * 2));

			imageUpdated = true;

			enableControls();
			mnuOpenFile.Enabled = true;
            this.Invoke(dcUpdateStatus, "Ready");
		}

		#endregion

		private void cmdGo_Click(object sender, System.EventArgs e)
		{
			//user is a dumbfuck
			if (cboCurrentTexture.SelectedIndex == -1)
			{
				return;
			}
            currentTextureText = cboCurrentTexture.SelectedText;
            currentTextureIndex = cboCurrentTexture.SelectedIndex;
            getTextureThread = new Thread(new ThreadStart(displayCurrentTexture));
            getTextureThread.Start();
 		}


	}
}
