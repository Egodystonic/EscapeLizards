namespace Egodystonic.EscapeLizards.Editor {
	partial class MatDescForm {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MatDescForm));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.matNameField = new System.Windows.Forms.TextBox();
			this.specularColourDialog = new System.Windows.Forms.ColorDialog();
			this.label4 = new System.Windows.Forms.Label();
			this.specPowerField = new System.Windows.Forms.NumericUpDown();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.textureFileButton = new System.Windows.Forms.Button();
			this.generateMipsCheckBox = new System.Windows.Forms.CheckBox();
			this.normalFileButton = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.specularFileButton = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.emissiveFileButton = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.specPowerField)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(29, 43);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(86, 17);
			this.label1.TabIndex = 1;
			this.label1.Text = "Texture File:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 15);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(103, 17);
			this.label2.TabIndex = 3;
			this.label2.Text = "Material Name:";
			// 
			// matNameField
			// 
			this.matNameField.Location = new System.Drawing.Point(121, 12);
			this.matNameField.Name = "matNameField";
			this.matNameField.Size = new System.Drawing.Size(292, 22);
			this.matNameField.TabIndex = 2;
			// 
			// specularColourDialog
			// 
			this.specularColourDialog.AnyColor = true;
			this.specularColourDialog.FullOpen = true;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(205, 167);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(134, 17);
			this.label4.TabIndex = 7;
			this.label4.Text = "Specular Tightness:";
			// 
			// specPowerField
			// 
			this.specPowerField.DecimalPlaces = 2;
			this.specPowerField.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
			this.specPowerField.Location = new System.Drawing.Point(345, 165);
			this.specPowerField.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.specPowerField.Name = "specPowerField";
			this.specPowerField.Size = new System.Drawing.Size(68, 22);
			this.specPowerField.TabIndex = 8;
			this.specPowerField.Value = new decimal(new int[] {
            75,
            0,
            0,
            131072});
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(261, 203);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(342, 203);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 10;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// textureFileButton
			// 
			this.textureFileButton.Location = new System.Drawing.Point(121, 40);
			this.textureFileButton.Name = "textureFileButton";
			this.textureFileButton.Size = new System.Drawing.Size(292, 23);
			this.textureFileButton.TabIndex = 11;
			this.textureFileButton.UseVisualStyleBackColor = true;
			// 
			// generateMipsCheckBox
			// 
			this.generateMipsCheckBox.AutoSize = true;
			this.generateMipsCheckBox.Location = new System.Drawing.Point(35, 166);
			this.generateMipsCheckBox.Name = "generateMipsCheckBox";
			this.generateMipsCheckBox.Size = new System.Drawing.Size(123, 21);
			this.generateMipsCheckBox.TabIndex = 12;
			this.generateMipsCheckBox.Text = "Generate Mips";
			this.generateMipsCheckBox.UseVisualStyleBackColor = true;
			// 
			// normalFileButton
			// 
			this.normalFileButton.Location = new System.Drawing.Point(121, 69);
			this.normalFileButton.Name = "normalFileButton";
			this.normalFileButton.Size = new System.Drawing.Size(292, 23);
			this.normalFileButton.TabIndex = 14;
			this.normalFileButton.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(32, 72);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(83, 17);
			this.label5.TabIndex = 13;
			this.label5.Text = "Normal File:";
			// 
			// specularFileButton
			// 
			this.specularFileButton.Location = new System.Drawing.Point(121, 98);
			this.specularFileButton.Name = "specularFileButton";
			this.specularFileButton.Size = new System.Drawing.Size(292, 23);
			this.specularFileButton.TabIndex = 16;
			this.specularFileButton.UseVisualStyleBackColor = true;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(21, 101);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(94, 17);
			this.label6.TabIndex = 15;
			this.label6.Text = "Specular File:";
			// 
			// emissiveFileButton
			// 
			this.emissiveFileButton.Location = new System.Drawing.Point(121, 127);
			this.emissiveFileButton.Name = "emissiveFileButton";
			this.emissiveFileButton.Size = new System.Drawing.Size(292, 23);
			this.emissiveFileButton.TabIndex = 18;
			this.emissiveFileButton.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(22, 130);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(93, 17);
			this.label3.TabIndex = 17;
			this.label3.Text = "Emissive File:";
			// 
			// MatDescForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(429, 236);
			this.Controls.Add(this.emissiveFileButton);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.specularFileButton);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.normalFileButton);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.generateMipsCheckBox);
			this.Controls.Add(this.textureFileButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.specPowerField);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.matNameField);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MatDescForm";
			this.Text = "MatDescForm";
			((System.ComponentModel.ISupportInitialize)(this.specPowerField)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox matNameField;
		private System.Windows.Forms.ColorDialog specularColourDialog;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown specPowerField;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button textureFileButton;
		private System.Windows.Forms.CheckBox generateMipsCheckBox;
		private System.Windows.Forms.Button normalFileButton;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button specularFileButton;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button emissiveFileButton;
		private System.Windows.Forms.Label label3;
	}
}