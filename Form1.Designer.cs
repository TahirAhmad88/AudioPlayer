using System.Drawing;
using System.Windows.Forms;

namespace AudioPlayerApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private System.Windows.Forms.Label lblTrackStart;
        private System.Windows.Forms.Label lblTrackEnd;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox picArt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox trackList;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblVolume;
        private System.Windows.Forms.TrackBar trackVolume;
        private System.Windows.Forms.Panel pBar;
        private System.Windows.Forms.Panel pBarFill;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Timer timer1;
        private AxWMPLib.AxWindowsMediaPlayer player;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.lblTrackStart = new System.Windows.Forms.Label();
            this.lblTrackEnd = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.picArt = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.trackList = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblVolume = new System.Windows.Forms.Label();
            this.trackVolume = new System.Windows.Forms.TrackBar();
            this.pBar = new System.Windows.Forms.Panel();
            this.pBarFill = new System.Windows.Forms.Panel();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.player = new AxWMPLib.AxWindowsMediaPlayer();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picArt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackVolume)).BeginInit();
            this.pBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.player)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTrackStart
            // 
            this.lblTrackStart.BackColor = System.Drawing.Color.Transparent;
            this.lblTrackStart.Font = new System.Drawing.Font("Digital-7", 18F, System.Drawing.FontStyle.Bold);
            this.lblTrackStart.ForeColor = System.Drawing.Color.Lime;
            this.lblTrackStart.Location = new System.Drawing.Point(20, 44);
            this.lblTrackStart.Name = "lblTrackStart";
            this.lblTrackStart.Size = new System.Drawing.Size(72, 34);
            this.lblTrackStart.TabIndex = 0;
            this.lblTrackStart.Text = "00:00";
            this.lblTrackStart.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTrackEnd
            // 
            this.lblTrackEnd.BackColor = System.Drawing.Color.Transparent;
            this.lblTrackEnd.Font = new System.Drawing.Font("Digital-7", 18F, System.Drawing.FontStyle.Bold);
            this.lblTrackEnd.ForeColor = System.Drawing.Color.Lime;
            this.lblTrackEnd.Location = new System.Drawing.Point(541, 44);
            this.lblTrackEnd.Name = "lblTrackEnd";
            this.lblTrackEnd.Size = new System.Drawing.Size(73, 34);
            this.lblTrackEnd.TabIndex = 1;
            this.lblTrackEnd.Text = "00:00";
            this.lblTrackEnd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.panel1.Controls.Add(this.picArt);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.txtSearch);
            this.panel1.Controls.Add(this.trackList);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.lblVolume);
            this.panel1.Controls.Add(this.trackVolume);
            this.panel1.Location = new System.Drawing.Point(20, 84);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(594, 186);
            this.panel1.TabIndex = 2;
            // 
            // picArt
            // 
            this.picArt.BackColor = System.Drawing.Color.White;
            this.picArt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picArt.Location = new System.Drawing.Point(15, 15);
            this.picArt.Name = "picArt";
            this.picArt.Size = new System.Drawing.Size(139, 140);
            this.picArt.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picArt.TabIndex = 0;
            this.picArt.TabStop = false;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(15, 156);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "By Tahir Ahmad";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtSearch
            // 
            this.txtSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Location = new System.Drawing.Point(160, 15);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(360, 20);
            this.txtSearch.TabIndex = 2;
            // 
            // trackList
            // 
            this.trackList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.trackList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.trackList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.trackList.IntegralHeight = false;
            this.trackList.Location = new System.Drawing.Point(160, 41);
            this.trackList.Name = "trackList";
            this.trackList.Size = new System.Drawing.Size(360, 136);
            this.trackList.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(530, 159);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Volume";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblVolume
            // 
            this.lblVolume.BackColor = System.Drawing.Color.Transparent;
            this.lblVolume.ForeColor = System.Drawing.Color.White;
            this.lblVolume.Location = new System.Drawing.Point(530, 14);
            this.lblVolume.Name = "lblVolume";
            this.lblVolume.Size = new System.Drawing.Size(60, 20);
            this.lblVolume.TabIndex = 5;
            this.lblVolume.Text = "50%";
            this.lblVolume.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // trackVolume
            // 
            this.trackVolume.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.trackVolume.Location = new System.Drawing.Point(535, 37);
            this.trackVolume.Maximum = 100;
            this.trackVolume.Name = "trackVolume";
            this.trackVolume.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackVolume.Size = new System.Drawing.Size(45, 118);
            this.trackVolume.TabIndex = 6;
            this.trackVolume.TickFrequency = 10;
            this.trackVolume.Value = 50;
            // 
            // pBar
            // 
            this.pBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.pBar.Controls.Add(this.pBarFill);
            this.pBar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pBar.Location = new System.Drawing.Point(20, 280);
            this.pBar.Name = "pBar";
            this.pBar.Size = new System.Drawing.Size(594, 18);
            this.pBar.TabIndex = 3;
            // 
            // pBarFill
            // 
            this.pBarFill.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.pBarFill.Enabled = false;
            this.pBarFill.Location = new System.Drawing.Point(0, 0);
            this.pBarFill.Name = "pBarFill";
            this.pBarFill.Size = new System.Drawing.Size(0, 18);
            this.pBarFill.TabIndex = 0;
            // 
            // btnPreview
            // 
            this.btnPreview.BackColor = System.Drawing.Color.Black;
            this.btnPreview.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPreview.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnPreview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPreview.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnPreview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnPreview.Location = new System.Drawing.Point(20, 312);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(90, 32);
            this.btnPreview.TabIndex = 4;
            this.btnPreview.TabStop = false;
            this.btnPreview.Text = "Previous";
            this.btnPreview.UseVisualStyleBackColor = false;
            // 
            // btnNext
            // 
            this.btnNext.BackColor = System.Drawing.Color.Black;
            this.btnNext.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNext.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnNext.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnNext.Location = new System.Drawing.Point(118, 312);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(90, 32);
            this.btnNext.TabIndex = 5;
            this.btnNext.TabStop = false;
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = false;
            // 
            // btnPlay
            // 
            this.btnPlay.BackColor = System.Drawing.Color.Black;
            this.btnPlay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPlay.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlay.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnPlay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnPlay.Location = new System.Drawing.Point(216, 312);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(90, 32);
            this.btnPlay.TabIndex = 6;
            this.btnPlay.TabStop = false;
            this.btnPlay.Text = ">";
            this.btnPlay.UseVisualStyleBackColor = false;
            // 
            // btnPause
            // 
            this.btnPause.BackColor = System.Drawing.Color.Black;
            this.btnPause.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPause.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPause.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnPause.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnPause.Location = new System.Drawing.Point(314, 312);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(90, 32);
            this.btnPause.TabIndex = 7;
            this.btnPause.TabStop = false;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = false;
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.Black;
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnStop.Location = new System.Drawing.Point(412, 312);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(90, 32);
            this.btnStop.TabIndex = 8;
            this.btnStop.TabStop = false;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = false;
            // 
            // btnOpen
            // 
            this.btnOpen.BackColor = System.Drawing.Color.Black;
            this.btnOpen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOpen.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpen.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnOpen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnOpen.Location = new System.Drawing.Point(510, 312);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(90, 32);
            this.btnOpen.TabIndex = 9;
            this.btnOpen.TabStop = false;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = false;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 200;
            // 
            // player
            // 
            this.player.Enabled = true;
            this.player.Location = new System.Drawing.Point(20, -2);
            this.player.Name = "player";
            this.player.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("player.OcxState")));
            this.player.Size = new System.Drawing.Size(594, 80);
            this.player.TabIndex = 10;
            this.player.TabStop = false;
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(634, 360);
            this.Controls.Add(this.lblTrackStart);
            this.Controls.Add(this.lblTrackEnd);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pBar);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.player);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AudioPlayer - Ready - Press F1 for shortcuts";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picArt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackVolume)).EndInit();
            this.pBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.player)).EndInit();
            this.ResumeLayout(false);

        }
    }
}