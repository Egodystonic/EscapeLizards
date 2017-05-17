namespace Egodystonic.EscapeLizards.Editor {
	partial class EditHistoryForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditHistoryForm));
			this.historyList = new System.Windows.Forms.ListBox();
			this.undoButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// historyList
			// 
			this.historyList.FormattingEnabled = true;
			this.historyList.ItemHeight = 16;
			this.historyList.Location = new System.Drawing.Point(13, 13);
			this.historyList.Name = "historyList";
			this.historyList.Size = new System.Drawing.Size(271, 532);
			this.historyList.TabIndex = 0;
			// 
			// undoButton
			// 
			this.undoButton.Location = new System.Drawing.Point(82, 552);
			this.undoButton.Name = "undoButton";
			this.undoButton.Size = new System.Drawing.Size(201, 23);
			this.undoButton.TabIndex = 1;
			this.undoButton.Text = "Undo Selected And Below";
			this.undoButton.UseVisualStyleBackColor = true;
			// 
			// EditHistoryForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(296, 584);
			this.Controls.Add(this.undoButton);
			this.Controls.Add(this.historyList);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "EditHistoryForm";
			this.Text = "History";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox historyList;
		private System.Windows.Forms.Button undoButton;
	}
}