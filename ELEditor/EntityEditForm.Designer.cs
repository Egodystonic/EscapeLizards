namespace Egodystonic.EscapeLizards.Editor {
	partial class EntityEditForm {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntityEditForm));
			this.label1 = new System.Windows.Forms.Label();
			this.tagField = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.movementFrameList = new System.Windows.Forms.ListBox();
			this.travelTimeField = new System.Windows.Forms.NumericUpDown();
			this.addMovementFrameButton = new System.Windows.Forms.Button();
			this.deleteMovementFrameButton = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.geometryButton = new System.Windows.Forms.Button();
			this.materialButton = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.alternateDirectionCheckbox = new System.Windows.Forms.CheckBox();
			this.smoothingCheckbox = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.initDelayBox = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.travelTimeField)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.initDelayBox)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(37, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Tag:";
			// 
			// tagField
			// 
			this.tagField.Location = new System.Drawing.Point(56, 10);
			this.tagField.Name = "tagField";
			this.tagField.Size = new System.Drawing.Size(284, 22);
			this.tagField.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(13, 45);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(124, 17);
			this.label2.TabIndex = 2;
			this.label2.Text = "Movement Frames";
			// 
			// movementFrameList
			// 
			this.movementFrameList.FormattingEnabled = true;
			this.movementFrameList.ItemHeight = 16;
			this.movementFrameList.Location = new System.Drawing.Point(16, 68);
			this.movementFrameList.Name = "movementFrameList";
			this.movementFrameList.Size = new System.Drawing.Size(114, 164);
			this.movementFrameList.TabIndex = 3;
			// 
			// travelTimeField
			// 
			this.travelTimeField.DecimalPlaces = 2;
			this.travelTimeField.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.travelTimeField.Location = new System.Drawing.Point(172, 97);
			this.travelTimeField.Name = "travelTimeField";
			this.travelTimeField.Size = new System.Drawing.Size(73, 22);
			this.travelTimeField.TabIndex = 11;
			// 
			// addMovementFrameButton
			// 
			this.addMovementFrameButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.addMovementFrameButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.addMovementFrameButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.addMovementFrameButton.Location = new System.Drawing.Point(172, 45);
			this.addMovementFrameButton.Margin = new System.Windows.Forms.Padding(0);
			this.addMovementFrameButton.Name = "addMovementFrameButton";
			this.addMovementFrameButton.Size = new System.Drawing.Size(46, 25);
			this.addMovementFrameButton.TabIndex = 13;
			this.addMovementFrameButton.Text = "Add";
			this.addMovementFrameButton.UseVisualStyleBackColor = false;
			// 
			// deleteMovementFrameButton
			// 
			this.deleteMovementFrameButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			this.deleteMovementFrameButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.deleteMovementFrameButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.deleteMovementFrameButton.Location = new System.Drawing.Point(227, 45);
			this.deleteMovementFrameButton.Margin = new System.Windows.Forms.Padding(0);
			this.deleteMovementFrameButton.Name = "deleteMovementFrameButton";
			this.deleteMovementFrameButton.Size = new System.Drawing.Size(46, 25);
			this.deleteMovementFrameButton.TabIndex = 14;
			this.deleteMovementFrameButton.Text = "Del";
			this.deleteMovementFrameButton.UseVisualStyleBackColor = false;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(12, 235);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(136, 17);
			this.label5.TabIndex = 15;
			this.label5.Text = "Geometry + Material";
			// 
			// geometryButton
			// 
			this.geometryButton.Location = new System.Drawing.Point(16, 256);
			this.geometryButton.Name = "geometryButton";
			this.geometryButton.Size = new System.Drawing.Size(321, 23);
			this.geometryButton.TabIndex = 16;
			this.geometryButton.Text = "button8";
			this.geometryButton.UseVisualStyleBackColor = true;
			// 
			// materialButton
			// 
			this.materialButton.Location = new System.Drawing.Point(16, 285);
			this.materialButton.Name = "materialButton";
			this.materialButton.Size = new System.Drawing.Size(321, 23);
			this.materialButton.TabIndex = 17;
			this.materialButton.Text = "button9";
			this.materialButton.UseVisualStyleBackColor = true;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(169, 77);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(104, 17);
			this.label4.TabIndex = 21;
			this.label4.Text = "Travel Time (s)";
			// 
			// alternateDirectionCheckbox
			// 
			this.alternateDirectionCheckbox.AutoSize = true;
			this.alternateDirectionCheckbox.Location = new System.Drawing.Point(172, 152);
			this.alternateDirectionCheckbox.Name = "alternateDirectionCheckbox";
			this.alternateDirectionCheckbox.Size = new System.Drawing.Size(147, 21);
			this.alternateDirectionCheckbox.TabIndex = 23;
			this.alternateDirectionCheckbox.Text = "Alternate Direction";
			this.alternateDirectionCheckbox.UseVisualStyleBackColor = true;
			// 
			// smoothingCheckbox
			// 
			this.smoothingCheckbox.AutoSize = true;
			this.smoothingCheckbox.Location = new System.Drawing.Point(172, 130);
			this.smoothingCheckbox.Name = "smoothingCheckbox";
			this.smoothingCheckbox.Size = new System.Drawing.Size(164, 21);
			this.smoothingCheckbox.TabIndex = 12;
			this.smoothingCheckbox.Text = "Transition Smoothing";
			this.smoothingCheckbox.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(169, 192);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(101, 17);
			this.label3.TabIndex = 25;
			this.label3.Text = "Initial Delay (s)";
			// 
			// initDelayBox
			// 
			this.initDelayBox.DecimalPlaces = 2;
			this.initDelayBox.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.initDelayBox.Location = new System.Drawing.Point(172, 212);
			this.initDelayBox.Name = "initDelayBox";
			this.initDelayBox.Size = new System.Drawing.Size(73, 22);
			this.initDelayBox.TabIndex = 24;
			// 
			// EntityEditForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(352, 318);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.initDelayBox);
			this.Controls.Add(this.alternateDirectionCheckbox);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.materialButton);
			this.Controls.Add(this.geometryButton);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.deleteMovementFrameButton);
			this.Controls.Add(this.addMovementFrameButton);
			this.Controls.Add(this.smoothingCheckbox);
			this.Controls.Add(this.travelTimeField);
			this.Controls.Add(this.movementFrameList);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.tagField);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "EntityEditForm";
			this.Text = "EntityEditForm";
			((System.ComponentModel.ISupportInitialize)(this.travelTimeField)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.initDelayBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tagField;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListBox movementFrameList;
		private System.Windows.Forms.NumericUpDown travelTimeField;
		private System.Windows.Forms.Button addMovementFrameButton;
		private System.Windows.Forms.Button deleteMovementFrameButton;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button geometryButton;
		private System.Windows.Forms.Button materialButton;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox alternateDirectionCheckbox;
		private System.Windows.Forms.CheckBox smoothingCheckbox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown initDelayBox;
	}
}