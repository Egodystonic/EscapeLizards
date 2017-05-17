namespace Egodystonic.EscapeLizards.Editor {
	partial class TextInputForm {
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
			this.textBox = new System.Windows.Forms.TextBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.label = new System.Windows.Forms.Label();
			this.okButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// textBox
			// 
			this.textBox.Location = new System.Drawing.Point(16, 38);
			this.textBox.Name = "textBox";
			this.textBox.Size = new System.Drawing.Size(448, 22);
			this.textBox.TabIndex = 0;
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(389, 66);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label.Location = new System.Drawing.Point(13, 13);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(46, 17);
			this.label.TabIndex = 2;
			this.label.Text = "label1";
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(308, 66);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 3;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// TextInputForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(476, 94);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.label);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.textBox);
			this.Name = "TextInputForm";
			this.Text = "TextInputForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBox;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label label;
		private System.Windows.Forms.Button okButton;
	}
}