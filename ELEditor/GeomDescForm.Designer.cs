namespace Egodystonic.EscapeLizards.Editor {
	partial class GeomDescForm {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GeomDescForm));
			this.shapePropertiesPanel = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.texScalingXTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.extrapolationTextBox = new System.Windows.Forms.TextBox();
			this.extrapolationLabel = new System.Windows.Forms.Label();
			this.insideOutCheckBox = new System.Windows.Forms.CheckBox();
			this.createButton = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.texScalingYTextBox = new System.Windows.Forms.TextBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// shapePropertiesPanel
			// 
			this.shapePropertiesPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.shapePropertiesPanel.ColumnCount = 2;
			this.shapePropertiesPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
			this.shapePropertiesPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
			this.shapePropertiesPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.shapePropertiesPanel.Location = new System.Drawing.Point(0, 0);
			this.shapePropertiesPanel.Name = "shapePropertiesPanel";
			this.shapePropertiesPanel.RowCount = 1;
			this.shapePropertiesPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.shapePropertiesPanel.Size = new System.Drawing.Size(346, 412);
			this.shapePropertiesPanel.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(352, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(110, 24);
			this.label1.TabIndex = 1;
			this.label1.Text = "Tex Scaling";
			// 
			// texScalingXTextBox
			// 
			this.texScalingXTextBox.Location = new System.Drawing.Point(356, 36);
			this.texScalingXTextBox.Name = "texScalingXTextBox";
			this.texScalingXTextBox.Size = new System.Drawing.Size(56, 22);
			this.texScalingXTextBox.TabIndex = 2;
			this.texScalingXTextBox.Text = "1.0";
			this.texScalingXTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(352, 74);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 24);
			this.label2.TabIndex = 3;
			this.label2.Text = "Inside-Out";
			// 
			// extrapolationTextBox
			// 
			this.extrapolationTextBox.Location = new System.Drawing.Point(356, 168);
			this.extrapolationTextBox.Name = "extrapolationTextBox";
			this.extrapolationTextBox.Size = new System.Drawing.Size(142, 22);
			this.extrapolationTextBox.TabIndex = 6;
			this.extrapolationTextBox.Text = "1";
			this.extrapolationTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// extrapolationLabel
			// 
			this.extrapolationLabel.AutoSize = true;
			this.extrapolationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.extrapolationLabel.Location = new System.Drawing.Point(352, 141);
			this.extrapolationLabel.Name = "extrapolationLabel";
			this.extrapolationLabel.Size = new System.Drawing.Size(119, 24);
			this.extrapolationLabel.TabIndex = 5;
			this.extrapolationLabel.Text = "Extrapolation";
			// 
			// insideOutCheckBox
			// 
			this.insideOutCheckBox.AutoSize = true;
			this.insideOutCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.insideOutCheckBox.Location = new System.Drawing.Point(400, 101);
			this.insideOutCheckBox.Name = "insideOutCheckBox";
			this.insideOutCheckBox.Size = new System.Drawing.Size(90, 21);
			this.insideOutCheckBox.TabIndex = 7;
			this.insideOutCheckBox.Text = "Flip faces";
			this.insideOutCheckBox.UseVisualStyleBackColor = true;
			// 
			// createButton
			// 
			this.createButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.createButton.Location = new System.Drawing.Point(356, 322);
			this.createButton.Name = "createButton";
			this.createButton.Size = new System.Drawing.Size(142, 45);
			this.createButton.TabIndex = 8;
			this.createButton.Text = "Create";
			this.createButton.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(418, 39);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(14, 17);
			this.label3.TabIndex = 9;
			this.label3.Text = "x";
			// 
			// texScalingYTextBox
			// 
			this.texScalingYTextBox.Location = new System.Drawing.Point(438, 36);
			this.texScalingYTextBox.Name = "texScalingYTextBox";
			this.texScalingYTextBox.Size = new System.Drawing.Size(56, 22);
			this.texScalingYTextBox.TabIndex = 10;
			this.texScalingYTextBox.Text = "1.0";
			this.texScalingYTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// cancelButton
			// 
			this.cancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cancelButton.Location = new System.Drawing.Point(356, 373);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(142, 27);
			this.cancelButton.TabIndex = 11;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// GeomDescForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(510, 412);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.texScalingYTextBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.createButton);
			this.Controls.Add(this.insideOutCheckBox);
			this.Controls.Add(this.extrapolationTextBox);
			this.Controls.Add(this.extrapolationLabel);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.texScalingXTextBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.shapePropertiesPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "GeomDescForm";
			this.Text = "GeomDescForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel shapePropertiesPanel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox texScalingXTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox extrapolationTextBox;
		private System.Windows.Forms.Label extrapolationLabel;
		private System.Windows.Forms.CheckBox insideOutCheckBox;
		private System.Windows.Forms.Button createButton;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox texScalingYTextBox;
		private System.Windows.Forms.Button cancelButton;


	}
}