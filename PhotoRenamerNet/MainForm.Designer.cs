namespace PhotoRenamerNet
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LayoutCommon = new System.Windows.Forms.TableLayoutPanel();
            this.TextBoxPath = new System.Windows.Forms.TextBox();
            this.ButtonStart = new System.Windows.Forms.Button();
            this.LayoutLabels = new System.Windows.Forms.TableLayoutPanel();
            this.LabelPath = new System.Windows.Forms.Label();
            this.LabelCurrent = new System.Windows.Forms.Label();
            this.LayoutButtonAndProgress = new System.Windows.Forms.TableLayoutPanel();
            this.ButtonPath = new System.Windows.Forms.Button();
            this.ProgressBar = new TextProgressBar();
            this.LayoutCommon.SuspendLayout();
            this.LayoutLabels.SuspendLayout();
            this.LayoutButtonAndProgress.SuspendLayout();
            this.SuspendLayout();
            // 
            // LayoutCommon
            // 
            this.LayoutCommon.ColumnCount = 2;
            this.LayoutCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LayoutCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LayoutCommon.Controls.Add(this.TextBoxPath, 0, 0);
            this.LayoutCommon.Controls.Add(this.ButtonStart, 1, 1);
            this.LayoutCommon.Controls.Add(this.LayoutLabels, 0, 1);
            this.LayoutCommon.Controls.Add(this.LayoutButtonAndProgress, 1, 0);
            this.LayoutCommon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LayoutCommon.Location = new System.Drawing.Point(0, 0);
            this.LayoutCommon.Margin = new System.Windows.Forms.Padding(0);
            this.LayoutCommon.Name = "LayoutCommon";
            this.LayoutCommon.RowCount = 2;
            this.LayoutCommon.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LayoutCommon.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LayoutCommon.Size = new System.Drawing.Size(819, 113);
            this.LayoutCommon.TabIndex = 0;
            // 
            // TextBoxPath
            // 
            this.TextBoxPath.Dock = System.Windows.Forms.DockStyle.Top;
            this.TextBoxPath.Location = new System.Drawing.Point(3, 3);
            this.TextBoxPath.Name = "TextBoxPath";
            this.TextBoxPath.Size = new System.Drawing.Size(403, 20);
            this.TextBoxPath.TabIndex = 0;
            this.TextBoxPath.Text = "Y:\\!Фотографии\\2018\\05 Май";
            // 
            // ButtonStart
            // 
            this.ButtonStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonStart.Location = new System.Drawing.Point(412, 59);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(404, 51);
            this.ButtonStart.TabIndex = 2;
            this.ButtonStart.Text = "Start";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // LayoutLabels
            // 
            this.LayoutLabels.ColumnCount = 1;
            this.LayoutLabels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LayoutLabels.Controls.Add(this.LabelPath, 0, 0);
            this.LayoutLabels.Controls.Add(this.LabelCurrent, 0, 1);
            this.LayoutLabels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LayoutLabels.Location = new System.Drawing.Point(0, 56);
            this.LayoutLabels.Margin = new System.Windows.Forms.Padding(0);
            this.LayoutLabels.Name = "LayoutLabels";
            this.LayoutLabels.RowCount = 2;
            this.LayoutLabels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LayoutLabels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LayoutLabels.Size = new System.Drawing.Size(409, 57);
            this.LayoutLabels.TabIndex = 3;
            // 
            // LabelPath
            // 
            this.LabelPath.AutoSize = true;
            this.LabelPath.Location = new System.Drawing.Point(3, 0);
            this.LabelPath.Name = "LabelPath";
            this.LabelPath.Size = new System.Drawing.Size(0, 13);
            this.LabelPath.TabIndex = 0;
            // 
            // LabelCurrent
            // 
            this.LabelCurrent.AutoSize = true;
            this.LabelCurrent.Location = new System.Drawing.Point(3, 28);
            this.LabelCurrent.Name = "LabelCurrent";
            this.LabelCurrent.Size = new System.Drawing.Size(0, 13);
            this.LabelCurrent.TabIndex = 1;
            // 
            // LayoutButtonAndProgress
            // 
            this.LayoutButtonAndProgress.ColumnCount = 1;
            this.LayoutButtonAndProgress.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LayoutButtonAndProgress.Controls.Add(this.ButtonPath, 0, 0);
            this.LayoutButtonAndProgress.Controls.Add(this.ProgressBar, 0, 1);
            this.LayoutButtonAndProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LayoutButtonAndProgress.Location = new System.Drawing.Point(409, 0);
            this.LayoutButtonAndProgress.Margin = new System.Windows.Forms.Padding(0);
            this.LayoutButtonAndProgress.Name = "LayoutButtonAndProgress";
            this.LayoutButtonAndProgress.RowCount = 2;
            this.LayoutButtonAndProgress.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LayoutButtonAndProgress.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LayoutButtonAndProgress.Size = new System.Drawing.Size(410, 56);
            this.LayoutButtonAndProgress.TabIndex = 4;
            // 
            // ButtonPath
            // 
            this.ButtonPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonPath.Location = new System.Drawing.Point(3, 3);
            this.ButtonPath.Name = "ButtonPath";
            this.ButtonPath.Size = new System.Drawing.Size(404, 22);
            this.ButtonPath.TabIndex = 0;
            this.ButtonPath.Text = "...";
            this.ButtonPath.UseVisualStyleBackColor = true;
            this.ButtonPath.Click += new System.EventHandler(this.ButtonPath_Click);
            // 
            // ProgressBar
            // 
            this.ProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProgressBar.Location = new System.Drawing.Point(3, 28);
            this.ProgressBar.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(404, 28);
            this.ProgressBar.TabIndex = 1;
            this.ProgressBar.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(819, 113);
            this.Controls.Add(this.LayoutCommon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MainForm";
            this.Text = "Photo renamer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.LayoutCommon.ResumeLayout(false);
            this.LayoutCommon.PerformLayout();
            this.LayoutLabels.ResumeLayout(false);
            this.LayoutLabels.PerformLayout();
            this.LayoutButtonAndProgress.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel LayoutCommon;
        private System.Windows.Forms.TextBox TextBoxPath;
        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.TableLayoutPanel LayoutLabels;
        private System.Windows.Forms.Label LabelPath;
        private System.Windows.Forms.Label LabelCurrent;
        private System.Windows.Forms.TableLayoutPanel LayoutButtonAndProgress;
        private System.Windows.Forms.Button ButtonPath;
        private TextProgressBar ProgressBar;
    }
}