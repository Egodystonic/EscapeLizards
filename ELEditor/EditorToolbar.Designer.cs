using System;
using System.Linq;

namespace Egodystonic.EscapeLizards.Editor {
	partial class EditorToolbar {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorToolbar));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.geomList = new System.Windows.Forms.ListBox();
			this.label4 = new System.Windows.Forms.Label();
			this.materialList = new System.Windows.Forms.ListBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.metaTitleLabel = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.metaFilenameLabel = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.metaTypeLabel = new System.Windows.Forms.Label();
			this.geomDeleteButton = new System.Windows.Forms.Button();
			this.createEntityButton = new System.Windows.Forms.Button();
			this.geomEditButton = new System.Windows.Forms.Button();
			this.geomCloneButton = new System.Windows.Forms.Button();
			this.matCloneButton = new System.Windows.Forms.Button();
			this.matEditButton = new System.Windows.Forms.Button();
			this.matDeleteButton = new System.Windows.Forms.Button();
			this.entityCloneButton = new System.Windows.Forms.Button();
			this.entityEditButton = new System.Windows.Forms.Button();
			this.entityDeleteButton = new System.Windows.Forms.Button();
			this.entityList = new System.Windows.Forms.ListBox();
			this.label6 = new System.Windows.Forms.Label();
			this.reinitSceneButton = new System.Windows.Forms.Button();
			this.cameraResetButton = new System.Windows.Forms.Button();
			this.magicButton = new System.Windows.Forms.Button();
			this.enablePickingCheckBox = new System.Windows.Forms.CheckBox();
			this.disableGridsCheckBox = new System.Windows.Forms.CheckBox();
			this.addCurvePB = new System.Windows.Forms.PictureBox();
			this.miscPB = new System.Windows.Forms.PictureBox();
			this.texturingPB = new System.Windows.Forms.PictureBox();
			this.metaPB = new System.Windows.Forms.PictureBox();
			this.addModelPB = new System.Windows.Forms.PictureBox();
			this.addPlanePB = new System.Windows.Forms.PictureBox();
			this.addSpherePB = new System.Windows.Forms.PictureBox();
			this.addConePB = new System.Windows.Forms.PictureBox();
			this.addCuboidPB = new System.Windows.Forms.PictureBox();
			this.newFilePB = new System.Windows.Forms.PictureBox();
			this.loadFilePB = new System.Windows.Forms.PictureBox();
			this.saveFilePB = new System.Windows.Forms.PictureBox();
			this.matBakeButton = new System.Windows.Forms.Button();
			this.matSwapButton = new System.Windows.Forms.Button();
			this.setMatButton = new System.Windows.Forms.Button();
			this.setGeomButton = new System.Windows.Forms.Button();
			this.uvPanAmountPicker = new System.Windows.Forms.NumericUpDown();
			this.uvPanUNegButton = new System.Windows.Forms.Button();
			this.uvPanVNegButton = new System.Windows.Forms.Button();
			this.uvPanVPosButton = new System.Windows.Forms.Button();
			this.uvPanUPosButton = new System.Windows.Forms.Button();
			this.enableDoFCheckbox = new System.Windows.Forms.CheckBox();
			this.pauseMoversCheckBox = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.addCurvePB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.miscPB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.texturingPB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.metaPB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.addModelPB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.addPlanePB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.addSpherePB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.addConePB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.addCuboidPB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.newFilePB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.loadFilePB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.saveFilePB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.uvPanAmountPicker)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(8, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(165, 24);
			this.label1.TabIndex = 1;
			this.label1.Text = "Save / Load / Meta";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(8, 107);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(132, 24);
			this.label2.TabIndex = 7;
			this.label2.Text = "Add Geometry";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(402, 218);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(84, 24);
			this.label3.TabIndex = 12;
			this.label3.Text = "Materials";
			// 
			// geomList
			// 
			this.geomList.FormattingEnabled = true;
			this.geomList.ItemHeight = 16;
			this.geomList.Location = new System.Drawing.Point(406, 42);
			this.geomList.Name = "geomList";
			this.geomList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.geomList.Size = new System.Drawing.Size(325, 132);
			this.geomList.TabIndex = 13;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(242, 105);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(54, 24);
			this.label4.TabIndex = 16;
			this.label4.Text = "Misc.";
			// 
			// materialList
			// 
			this.materialList.FormattingEnabled = true;
			this.materialList.ItemHeight = 16;
			this.materialList.Location = new System.Drawing.Point(406, 246);
			this.materialList.Name = "materialList";
			this.materialList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.materialList.Size = new System.Drawing.Size(324, 132);
			this.materialList.TabIndex = 19;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(402, 15);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(92, 24);
			this.label5.TabIndex = 18;
			this.label5.Text = "Geometry";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(8, 329);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(46, 20);
			this.label7.TabIndex = 21;
			this.label7.Text = "Title:";
			// 
			// metaTitleLabel
			// 
			this.metaTitleLabel.AutoSize = true;
			this.metaTitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.metaTitleLabel.ForeColor = System.Drawing.Color.Gray;
			this.metaTitleLabel.Location = new System.Drawing.Point(60, 329);
			this.metaTitleLabel.Name = "metaTitleLabel";
			this.metaTitleLabel.Size = new System.Drawing.Size(66, 20);
			this.metaTitleLabel.TabIndex = 22;
			this.metaTitleLabel.Text = "Untitled";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.Location = new System.Drawing.Point(8, 354);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(82, 20);
			this.label8.TabIndex = 23;
			this.label8.Text = "Filename:";
			// 
			// metaFilenameLabel
			// 
			this.metaFilenameLabel.AutoSize = true;
			this.metaFilenameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.metaFilenameLabel.ForeColor = System.Drawing.Color.Gray;
			this.metaFilenameLabel.Location = new System.Drawing.Point(96, 354);
			this.metaFilenameLabel.Name = "metaFilenameLabel";
			this.metaFilenameLabel.Size = new System.Drawing.Size(84, 20);
			this.metaFilenameLabel.TabIndex = 24;
			this.metaFilenameLabel.Text = "untitled.ell";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label10.Location = new System.Drawing.Point(8, 302);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(50, 20);
			this.label10.TabIndex = 25;
			this.label10.Text = "Type:";
			// 
			// metaTypeLabel
			// 
			this.metaTypeLabel.AutoSize = true;
			this.metaTypeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.metaTypeLabel.ForeColor = System.Drawing.Color.Gray;
			this.metaTypeLabel.Location = new System.Drawing.Point(60, 302);
			this.metaTypeLabel.Name = "metaTypeLabel";
			this.metaTypeLabel.Size = new System.Drawing.Size(99, 20);
			this.metaTypeLabel.TabIndex = 26;
			this.metaTypeLabel.Text = "Game Level";
			// 
			// geomDeleteButton
			// 
			this.geomDeleteButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			this.geomDeleteButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.geomDeleteButton.Location = new System.Drawing.Point(548, 8);
			this.geomDeleteButton.Name = "geomDeleteButton";
			this.geomDeleteButton.Size = new System.Drawing.Size(63, 28);
			this.geomDeleteButton.TabIndex = 27;
			this.geomDeleteButton.Text = "Delete";
			this.geomDeleteButton.UseVisualStyleBackColor = false;
			// 
			// createEntityButton
			// 
			this.createEntityButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.createEntityButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.createEntityButton.Location = new System.Drawing.Point(482, 425);
			this.createEntityButton.Name = "createEntityButton";
			this.createEntityButton.Size = new System.Drawing.Size(61, 27);
			this.createEntityButton.TabIndex = 28;
			this.createEntityButton.Text = "Create";
			this.createEntityButton.UseVisualStyleBackColor = false;
			// 
			// geomEditButton
			// 
			this.geomEditButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.geomEditButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.geomEditButton.Location = new System.Drawing.Point(617, 8);
			this.geomEditButton.Name = "geomEditButton";
			this.geomEditButton.Size = new System.Drawing.Size(46, 28);
			this.geomEditButton.TabIndex = 29;
			this.geomEditButton.Text = "Edit";
			this.geomEditButton.UseVisualStyleBackColor = false;
			// 
			// geomCloneButton
			// 
			this.geomCloneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.geomCloneButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.geomCloneButton.Location = new System.Drawing.Point(669, 8);
			this.geomCloneButton.Name = "geomCloneButton";
			this.geomCloneButton.Size = new System.Drawing.Size(61, 28);
			this.geomCloneButton.TabIndex = 30;
			this.geomCloneButton.Text = "Clone";
			this.geomCloneButton.UseVisualStyleBackColor = false;
			// 
			// matCloneButton
			// 
			this.matCloneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.matCloneButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.matCloneButton.Location = new System.Drawing.Point(670, 214);
			this.matCloneButton.Name = "matCloneButton";
			this.matCloneButton.Size = new System.Drawing.Size(61, 28);
			this.matCloneButton.TabIndex = 33;
			this.matCloneButton.Text = "Clone";
			this.matCloneButton.UseVisualStyleBackColor = false;
			// 
			// matEditButton
			// 
			this.matEditButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.matEditButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.matEditButton.Location = new System.Drawing.Point(617, 214);
			this.matEditButton.Name = "matEditButton";
			this.matEditButton.Size = new System.Drawing.Size(46, 28);
			this.matEditButton.TabIndex = 32;
			this.matEditButton.Text = "Edit";
			this.matEditButton.UseVisualStyleBackColor = false;
			// 
			// matDeleteButton
			// 
			this.matDeleteButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			this.matDeleteButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.matDeleteButton.Location = new System.Drawing.Point(548, 214);
			this.matDeleteButton.Name = "matDeleteButton";
			this.matDeleteButton.Size = new System.Drawing.Size(63, 28);
			this.matDeleteButton.TabIndex = 31;
			this.matDeleteButton.Text = "Delete";
			this.matDeleteButton.UseVisualStyleBackColor = false;
			// 
			// entityCloneButton
			// 
			this.entityCloneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.entityCloneButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.entityCloneButton.Location = new System.Drawing.Point(670, 424);
			this.entityCloneButton.Name = "entityCloneButton";
			this.entityCloneButton.Size = new System.Drawing.Size(61, 28);
			this.entityCloneButton.TabIndex = 38;
			this.entityCloneButton.Text = "Clone";
			this.entityCloneButton.UseVisualStyleBackColor = false;
			// 
			// entityEditButton
			// 
			this.entityEditButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.entityEditButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.entityEditButton.Location = new System.Drawing.Point(618, 424);
			this.entityEditButton.Name = "entityEditButton";
			this.entityEditButton.Size = new System.Drawing.Size(46, 28);
			this.entityEditButton.TabIndex = 37;
			this.entityEditButton.Text = "Edit";
			this.entityEditButton.UseVisualStyleBackColor = false;
			// 
			// entityDeleteButton
			// 
			this.entityDeleteButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			this.entityDeleteButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.entityDeleteButton.Location = new System.Drawing.Point(549, 424);
			this.entityDeleteButton.Name = "entityDeleteButton";
			this.entityDeleteButton.Size = new System.Drawing.Size(63, 28);
			this.entityDeleteButton.TabIndex = 36;
			this.entityDeleteButton.Text = "Delete";
			this.entityDeleteButton.UseVisualStyleBackColor = false;
			// 
			// entityList
			// 
			this.entityList.FormattingEnabled = true;
			this.entityList.ItemHeight = 16;
			this.entityList.Location = new System.Drawing.Point(13, 455);
			this.entityList.Name = "entityList";
			this.entityList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.entityList.Size = new System.Drawing.Size(718, 420);
			this.entityList.TabIndex = 35;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.Location = new System.Drawing.Point(13, 427);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(127, 24);
			this.label6.TabIndex = 34;
			this.label6.Text = "Geom Entities";
			// 
			// reinitSceneButton
			// 
			this.reinitSceneButton.Location = new System.Drawing.Point(238, 204);
			this.reinitSceneButton.Name = "reinitSceneButton";
			this.reinitSceneButton.Size = new System.Drawing.Size(142, 23);
			this.reinitSceneButton.TabIndex = 42;
			this.reinitSceneButton.Text = "Reinitialize Scene";
			this.reinitSceneButton.UseVisualStyleBackColor = true;
			// 
			// cameraResetButton
			// 
			this.cameraResetButton.Location = new System.Drawing.Point(268, 233);
			this.cameraResetButton.Name = "cameraResetButton";
			this.cameraResetButton.Size = new System.Drawing.Size(112, 23);
			this.cameraResetButton.TabIndex = 43;
			this.cameraResetButton.Text = "Reset Camera";
			this.cameraResetButton.UseVisualStyleBackColor = true;
			// 
			// magicButton
			// 
			this.magicButton.Location = new System.Drawing.Point(399, 391);
			this.magicButton.Name = "magicButton";
			this.magicButton.Size = new System.Drawing.Size(332, 29);
			this.magicButton.TabIndex = 44;
			this.magicButton.Text = "Bake Selected Entities and Set Tex Scaling";
			this.magicButton.UseVisualStyleBackColor = true;
			// 
			// enablePickingCheckBox
			// 
			this.enablePickingCheckBox.AutoSize = true;
			this.enablePickingCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.enablePickingCheckBox.Location = new System.Drawing.Point(195, 268);
			this.enablePickingCheckBox.Name = "enablePickingCheckBox";
			this.enablePickingCheckBox.Size = new System.Drawing.Size(185, 21);
			this.enablePickingCheckBox.TabIndex = 46;
			this.enablePickingCheckBox.Text = "Enable Left-Click Picking";
			this.enablePickingCheckBox.UseVisualStyleBackColor = true;
			// 
			// disableGridsCheckBox
			// 
			this.disableGridsCheckBox.AutoSize = true;
			this.disableGridsCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.disableGridsCheckBox.Location = new System.Drawing.Point(196, 286);
			this.disableGridsCheckBox.Name = "disableGridsCheckBox";
			this.disableGridsCheckBox.Size = new System.Drawing.Size(184, 21);
			this.disableGridsCheckBox.TabIndex = 47;
			this.disableGridsCheckBox.Text = "Disable Transform Grids";
			this.disableGridsCheckBox.UseVisualStyleBackColor = true;
			// 
			// addCurvePB
			// 
			this.addCurvePB.Cursor = System.Windows.Forms.Cursors.Hand;
			this.addCurvePB.Image = global::Egodystonic.EscapeLizards.Editor.Properties.Resources.icon_curve;
			this.addCurvePB.Location = new System.Drawing.Point(12, 204);
			this.addCurvePB.Name = "addCurvePB";
			this.addCurvePB.Size = new System.Drawing.Size(62, 62);
			this.addCurvePB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.addCurvePB.TabIndex = 48;
			this.addCurvePB.TabStop = false;
			// 
			// miscPB
			// 
			this.miscPB.Cursor = System.Windows.Forms.Cursors.Hand;
			this.miscPB.Image = global::Egodystonic.EscapeLizards.Editor.Properties.Resources.icon_lighting;
			this.miscPB.Location = new System.Drawing.Point(316, 134);
			this.miscPB.Name = "miscPB";
			this.miscPB.Size = new System.Drawing.Size(64, 64);
			this.miscPB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.miscPB.TabIndex = 17;
			this.miscPB.TabStop = false;
			// 
			// texturingPB
			// 
			this.texturingPB.Cursor = System.Windows.Forms.Cursors.Hand;
			this.texturingPB.Image = global::Egodystonic.EscapeLizards.Editor.Properties.Resources.icon_texture;
			this.texturingPB.Location = new System.Drawing.Point(246, 134);
			this.texturingPB.Name = "texturingPB";
			this.texturingPB.Size = new System.Drawing.Size(64, 64);
			this.texturingPB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.texturingPB.TabIndex = 15;
			this.texturingPB.TabStop = false;
			// 
			// metaPB
			// 
			this.metaPB.Cursor = System.Windows.Forms.Cursors.Hand;
			this.metaPB.Image = global::Egodystonic.EscapeLizards.Editor.Properties.Resources.icon_substractbrush;
			this.metaPB.Location = new System.Drawing.Point(258, 36);
			this.metaPB.Name = "metaPB";
			this.metaPB.Size = new System.Drawing.Size(64, 64);
			this.metaPB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.metaPB.TabIndex = 14;
			this.metaPB.TabStop = false;
			// 
			// addModelPB
			// 
			this.addModelPB.Cursor = System.Windows.Forms.Cursors.Hand;
			this.addModelPB.Image = global::Egodystonic.EscapeLizards.Editor.Properties.Resources.icon_modelimport;
			this.addModelPB.Location = new System.Drawing.Point(152, 134);
			this.addModelPB.Name = "addModelPB";
			this.addModelPB.Size = new System.Drawing.Size(64, 64);
			this.addModelPB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.addModelPB.TabIndex = 11;
			this.addModelPB.TabStop = false;
			// 
			// addPlanePB
			// 
			this.addPlanePB.Cursor = System.Windows.Forms.Cursors.Hand;
			this.addPlanePB.Image = global::Egodystonic.EscapeLizards.Editor.Properties.Resources.icon_plane;
			this.addPlanePB.Location = new System.Drawing.Point(151, 204);
			this.addPlanePB.Name = "addPlanePB";
			this.addPlanePB.Size = new System.Drawing.Size(64, 64);
			this.addPlanePB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.addPlanePB.TabIndex = 10;
			this.addPlanePB.TabStop = false;
			// 
			// addSpherePB
			// 
			this.addSpherePB.Cursor = System.Windows.Forms.Cursors.Hand;
			this.addSpherePB.Image = global::Egodystonic.EscapeLizards.Editor.Properties.Resources.icon_sphere;
			this.addSpherePB.Location = new System.Drawing.Point(82, 204);
			this.addSpherePB.Name = "addSpherePB";
			this.addSpherePB.Size = new System.Drawing.Size(64, 64);
			this.addSpherePB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.addSpherePB.TabIndex = 9;
			this.addSpherePB.TabStop = false;
			// 
			// addConePB
			// 
			this.addConePB.Cursor = System.Windows.Forms.Cursors.Hand;
			this.addConePB.Image = global::Egodystonic.EscapeLizards.Editor.Properties.Resources.icon_cylinder;
			this.addConePB.Location = new System.Drawing.Point(82, 134);
			this.addConePB.Name = "addConePB";
			this.addConePB.Size = new System.Drawing.Size(64, 64);
			this.addConePB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.addConePB.TabIndex = 8;
			this.addConePB.TabStop = false;
			// 
			// addCuboidPB
			// 
			this.addCuboidPB.Cursor = System.Windows.Forms.Cursors.Hand;
			this.addCuboidPB.Image = global::Egodystonic.EscapeLizards.Editor.Properties.Resources.icon_cube;
			this.addCuboidPB.Location = new System.Drawing.Point(12, 134);
			this.addCuboidPB.Name = "addCuboidPB";
			this.addCuboidPB.Size = new System.Drawing.Size(64, 64);
			this.addCuboidPB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.addCuboidPB.TabIndex = 6;
			this.addCuboidPB.TabStop = false;
			// 
			// newFilePB
			// 
			this.newFilePB.Cursor = System.Windows.Forms.Cursors.Hand;
			this.newFilePB.Image = global::Egodystonic.EscapeLizards.Editor.Properties.Resources.icon_newfile;
			this.newFilePB.Location = new System.Drawing.Point(152, 36);
			this.newFilePB.Name = "newFilePB";
			this.newFilePB.Size = new System.Drawing.Size(64, 64);
			this.newFilePB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.newFilePB.TabIndex = 3;
			this.newFilePB.TabStop = false;
			// 
			// loadFilePB
			// 
			this.loadFilePB.Cursor = System.Windows.Forms.Cursors.Hand;
			this.loadFilePB.Image = global::Egodystonic.EscapeLizards.Editor.Properties.Resources.icon_openfile;
			this.loadFilePB.Location = new System.Drawing.Point(82, 36);
			this.loadFilePB.Name = "loadFilePB";
			this.loadFilePB.Size = new System.Drawing.Size(64, 64);
			this.loadFilePB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.loadFilePB.TabIndex = 2;
			this.loadFilePB.TabStop = false;
			// 
			// saveFilePB
			// 
			this.saveFilePB.Cursor = System.Windows.Forms.Cursors.Hand;
			this.saveFilePB.Image = global::Egodystonic.EscapeLizards.Editor.Properties.Resources.icon_savefile;
			this.saveFilePB.Location = new System.Drawing.Point(12, 36);
			this.saveFilePB.Name = "saveFilePB";
			this.saveFilePB.Size = new System.Drawing.Size(64, 64);
			this.saveFilePB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.saveFilePB.TabIndex = 0;
			this.saveFilePB.TabStop = false;
			// 
			// matBakeButton
			// 
			this.matBakeButton.BackColor = System.Drawing.Color.Wheat;
			this.matBakeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.matBakeButton.Location = new System.Drawing.Point(624, 180);
			this.matBakeButton.Name = "matBakeButton";
			this.matBakeButton.Size = new System.Drawing.Size(107, 28);
			this.matBakeButton.TabIndex = 49;
			this.matBakeButton.Text = "Global Scale";
			this.matBakeButton.UseVisualStyleBackColor = false;
			// 
			// matSwapButton
			// 
			this.matSwapButton.BackColor = System.Drawing.Color.Wheat;
			this.matSwapButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.matSwapButton.Location = new System.Drawing.Point(511, 180);
			this.matSwapButton.Name = "matSwapButton";
			this.matSwapButton.Size = new System.Drawing.Size(107, 28);
			this.matSwapButton.TabIndex = 50;
			this.matSwapButton.Text = "Global Swap";
			this.matSwapButton.UseVisualStyleBackColor = false;
			// 
			// setMatButton
			// 
			this.setMatButton.BackColor = System.Drawing.Color.Wheat;
			this.setMatButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.setMatButton.Location = new System.Drawing.Point(402, 425);
			this.setMatButton.Name = "setMatButton";
			this.setMatButton.Size = new System.Drawing.Size(74, 27);
			this.setMatButton.TabIndex = 51;
			this.setMatButton.Text = "Set Mat";
			this.setMatButton.UseVisualStyleBackColor = false;
			// 
			// setGeomButton
			// 
			this.setGeomButton.BackColor = System.Drawing.Color.Wheat;
			this.setGeomButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.setGeomButton.Location = new System.Drawing.Point(310, 425);
			this.setGeomButton.Name = "setGeomButton";
			this.setGeomButton.Size = new System.Drawing.Size(86, 27);
			this.setGeomButton.TabIndex = 52;
			this.setGeomButton.Text = "Set Geom";
			this.setGeomButton.UseVisualStyleBackColor = false;
			// 
			// uvPanAmountPicker
			// 
			this.uvPanAmountPicker.DecimalPlaces = 2;
			this.uvPanAmountPicker.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.uvPanAmountPicker.Location = new System.Drawing.Point(201, 403);
			this.uvPanAmountPicker.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.uvPanAmountPicker.Name = "uvPanAmountPicker";
			this.uvPanAmountPicker.Size = new System.Drawing.Size(55, 22);
			this.uvPanAmountPicker.TabIndex = 53;
			// 
			// uvPanUNegButton
			// 
			this.uvPanUNegButton.Location = new System.Drawing.Point(152, 402);
			this.uvPanUNegButton.Name = "uvPanUNegButton";
			this.uvPanUNegButton.Size = new System.Drawing.Size(43, 23);
			this.uvPanUNegButton.TabIndex = 55;
			this.uvPanUNegButton.Text = "L";
			this.uvPanUNegButton.UseVisualStyleBackColor = true;
			// 
			// uvPanVNegButton
			// 
			this.uvPanVNegButton.Location = new System.Drawing.Point(208, 374);
			this.uvPanVNegButton.Name = "uvPanVNegButton";
			this.uvPanVNegButton.Size = new System.Drawing.Size(43, 23);
			this.uvPanVNegButton.TabIndex = 56;
			this.uvPanVNegButton.Text = "U";
			this.uvPanVNegButton.UseVisualStyleBackColor = true;
			// 
			// uvPanVPosButton
			// 
			this.uvPanVPosButton.Location = new System.Drawing.Point(208, 429);
			this.uvPanVPosButton.Name = "uvPanVPosButton";
			this.uvPanVPosButton.Size = new System.Drawing.Size(43, 23);
			this.uvPanVPosButton.TabIndex = 57;
			this.uvPanVPosButton.Text = "D";
			this.uvPanVPosButton.UseVisualStyleBackColor = true;
			// 
			// uvPanUPosButton
			// 
			this.uvPanUPosButton.Location = new System.Drawing.Point(262, 402);
			this.uvPanUPosButton.Name = "uvPanUPosButton";
			this.uvPanUPosButton.Size = new System.Drawing.Size(43, 23);
			this.uvPanUPosButton.TabIndex = 58;
			this.uvPanUPosButton.Text = "R";
			this.uvPanUPosButton.UseVisualStyleBackColor = true;
			// 
			// enableDoFCheckbox
			// 
			this.enableDoFCheckbox.AutoSize = true;
			this.enableDoFCheckbox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.enableDoFCheckbox.Location = new System.Drawing.Point(214, 304);
			this.enableDoFCheckbox.Name = "enableDoFCheckbox";
			this.enableDoFCheckbox.Size = new System.Drawing.Size(166, 21);
			this.enableDoFCheckbox.TabIndex = 59;
			this.enableDoFCheckbox.Text = "Enable Depth of Field";
			this.enableDoFCheckbox.UseVisualStyleBackColor = true;
			// 
			// pauseMoversCheckBox
			// 
			this.pauseMoversCheckBox.AutoSize = true;
			this.pauseMoversCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.pauseMoversCheckBox.Location = new System.Drawing.Point(260, 322);
			this.pauseMoversCheckBox.Name = "pauseMoversCheckBox";
			this.pauseMoversCheckBox.Size = new System.Drawing.Size(120, 21);
			this.pauseMoversCheckBox.TabIndex = 60;
			this.pauseMoversCheckBox.Text = "Pause Movers";
			this.pauseMoversCheckBox.UseVisualStyleBackColor = true;
			// 
			// EditorToolbar
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(740, 884);
			this.Controls.Add(this.pauseMoversCheckBox);
			this.Controls.Add(this.enableDoFCheckbox);
			this.Controls.Add(this.uvPanUPosButton);
			this.Controls.Add(this.uvPanVPosButton);
			this.Controls.Add(this.uvPanVNegButton);
			this.Controls.Add(this.uvPanUNegButton);
			this.Controls.Add(this.uvPanAmountPicker);
			this.Controls.Add(this.setGeomButton);
			this.Controls.Add(this.setMatButton);
			this.Controls.Add(this.matSwapButton);
			this.Controls.Add(this.matBakeButton);
			this.Controls.Add(this.addCurvePB);
			this.Controls.Add(this.disableGridsCheckBox);
			this.Controls.Add(this.enablePickingCheckBox);
			this.Controls.Add(this.magicButton);
			this.Controls.Add(this.cameraResetButton);
			this.Controls.Add(this.reinitSceneButton);
			this.Controls.Add(this.entityCloneButton);
			this.Controls.Add(this.entityEditButton);
			this.Controls.Add(this.entityDeleteButton);
			this.Controls.Add(this.entityList);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.matCloneButton);
			this.Controls.Add(this.matEditButton);
			this.Controls.Add(this.matDeleteButton);
			this.Controls.Add(this.geomCloneButton);
			this.Controls.Add(this.geomEditButton);
			this.Controls.Add(this.createEntityButton);
			this.Controls.Add(this.geomDeleteButton);
			this.Controls.Add(this.metaTypeLabel);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.metaFilenameLabel);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.metaTitleLabel);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.materialList);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.miscPB);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.texturingPB);
			this.Controls.Add(this.metaPB);
			this.Controls.Add(this.geomList);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.addModelPB);
			this.Controls.Add(this.addPlanePB);
			this.Controls.Add(this.addSpherePB);
			this.Controls.Add(this.addConePB);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.addCuboidPB);
			this.Controls.Add(this.newFilePB);
			this.Controls.Add(this.loadFilePB);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.saveFilePB);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "EditorToolbar";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "EscapeLizards Editor Toolbar";
			((System.ComponentModel.ISupportInitialize)(this.addCurvePB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.miscPB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.texturingPB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.metaPB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.addModelPB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.addPlanePB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.addSpherePB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.addConePB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.addCuboidPB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.newFilePB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.loadFilePB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.saveFilePB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.uvPanAmountPicker)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox saveFilePB;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox loadFilePB;
		private System.Windows.Forms.PictureBox newFilePB;
		private System.Windows.Forms.PictureBox addModelPB;
		private System.Windows.Forms.PictureBox addPlanePB;
		private System.Windows.Forms.PictureBox addSpherePB;
		private System.Windows.Forms.PictureBox addConePB;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.PictureBox addCuboidPB;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListBox geomList;
		private System.Windows.Forms.PictureBox metaPB;
		private System.Windows.Forms.PictureBox miscPB;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.PictureBox texturingPB;
		private System.Windows.Forms.ListBox materialList;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label metaTitleLabel;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label metaFilenameLabel;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label metaTypeLabel;
		private System.Windows.Forms.Button geomDeleteButton;
		private System.Windows.Forms.Button createEntityButton;
		private System.Windows.Forms.Button geomEditButton;
		private System.Windows.Forms.Button geomCloneButton;
		private System.Windows.Forms.Button matCloneButton;
		private System.Windows.Forms.Button matEditButton;
		private System.Windows.Forms.Button matDeleteButton;
		private System.Windows.Forms.Button entityCloneButton;
		private System.Windows.Forms.Button entityEditButton;
		private System.Windows.Forms.Button entityDeleteButton;
		private System.Windows.Forms.ListBox entityList;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button reinitSceneButton;
		private System.Windows.Forms.Button cameraResetButton;
		private System.Windows.Forms.Button magicButton;
		private System.Windows.Forms.CheckBox enablePickingCheckBox;
		private System.Windows.Forms.CheckBox disableGridsCheckBox;
		private System.Windows.Forms.PictureBox addCurvePB;
		private System.Windows.Forms.Button matBakeButton;
		private System.Windows.Forms.Button matSwapButton;
		private System.Windows.Forms.Button setMatButton;
		private System.Windows.Forms.Button setGeomButton;
		private System.Windows.Forms.NumericUpDown uvPanAmountPicker;
		private System.Windows.Forms.Button uvPanUNegButton;
		private System.Windows.Forms.Button uvPanVNegButton;
		private System.Windows.Forms.Button uvPanVPosButton;
		private System.Windows.Forms.Button uvPanUPosButton;
		private System.Windows.Forms.CheckBox enableDoFCheckbox;
		private System.Windows.Forms.CheckBox pauseMoversCheckBox;
	}
}