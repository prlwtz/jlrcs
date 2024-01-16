namespace jlr_sample
{
    partial class Form1
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
            this.label1 = new Label();
            this.textBoxUser = new TextBox();
            this.textBoxPass = new TextBox();
            this.label2 = new Label();
            this.buttonConnect = new Button();
            this.textBoxDeviceID = new TextBox();
            this.label3 = new Label();
            this.buttonClear = new Button();
            this.listViewVehicles = new ListView();
            this.columnHeader1 = new ColumnHeader();
            this.label4 = new Label();
            this.label5 = new Label();
            this.textBoxVehicleLastUpdated = new TextBox();
            this.label7 = new Label();
            this.labelNumVehicles = new Label();
            this.listViewVehicleStatus = new ListView();
            this.columnHeader2 = new ColumnHeader();
            this.columnHeader3 = new ColumnHeader();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new Size(70, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "User (email)";
            // 
            // textBoxUser
            // 
            this.textBoxUser.Location = new Point(111, 6);
            this.textBoxUser.Name = "textBoxUser";
            this.textBoxUser.Size = new Size(295, 23);
            this.textBoxUser.TabIndex = 1;
            // 
            // textBoxPass
            // 
            this.textBoxPass.Location = new Point(111, 35);
            this.textBoxPass.Name = "textBoxPass";
            this.textBoxPass.PasswordChar = '*';
            this.textBoxPass.Size = new Size(295, 23);
            this.textBoxPass.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new Point(12, 38);
            this.label2.Name = "label2";
            this.label2.Size = new Size(57, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Password";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new Point(412, 6);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new Size(93, 52);
            this.buttonConnect.TabIndex = 4;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += this.buttonConnect_Click;
            // 
            // textBoxDeviceID
            // 
            this.textBoxDeviceID.Location = new Point(111, 64);
            this.textBoxDeviceID.Name = "textBoxDeviceID";
            this.textBoxDeviceID.ReadOnly = true;
            this.textBoxDeviceID.Size = new Size(295, 23);
            this.textBoxDeviceID.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new Point(12, 67);
            this.label3.Name = "label3";
            this.label3.Size = new Size(53, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "DeviceID";
            // 
            // buttonClear
            // 
            this.buttonClear.Location = new Point(412, 64);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new Size(93, 23);
            this.buttonClear.TabIndex = 7;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += this.buttonClear_Click;
            // 
            // listViewVehicles
            // 
            this.listViewVehicles.Columns.AddRange(new ColumnHeader[] { this.columnHeader1 });
            this.listViewVehicles.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.listViewVehicles.FullRowSelect = true;
            this.listViewVehicles.GridLines = true;
            this.listViewVehicles.Location = new Point(111, 93);
            this.listViewVehicles.MultiSelect = false;
            this.listViewVehicles.Name = "listViewVehicles";
            this.listViewVehicles.Size = new Size(295, 135);
            this.listViewVehicles.TabIndex = 8;
            this.listViewVehicles.UseCompatibleStateImageBehavior = false;
            this.listViewVehicles.View = View.Details;
            this.listViewVehicles.SelectedIndexChanged += this.listViewVehicles_SelectedIndexChanged;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "VIN";
            this.columnHeader1.Width = 250;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new Point(12, 125);
            this.label4.Name = "label4";
            this.label4.Size = new Size(57, 15);
            this.label4.TabIndex = 9;
            this.label4.Text = "Vehicle(s)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new Point(12, 252);
            this.label5.Name = "label5";
            this.label5.Size = new Size(75, 15);
            this.label5.TabIndex = 10;
            this.label5.Text = "Last updated";
            // 
            // textBoxVehicleLastUpdated
            // 
            this.textBoxVehicleLastUpdated.Location = new Point(111, 249);
            this.textBoxVehicleLastUpdated.Name = "textBoxVehicleLastUpdated";
            this.textBoxVehicleLastUpdated.ReadOnly = true;
            this.textBoxVehicleLastUpdated.Size = new Size(295, 23);
            this.textBoxVehicleLastUpdated.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point, 0);
            this.label7.Location = new Point(111, 231);
            this.label7.Name = "label7";
            this.label7.Size = new Size(137, 15);
            this.label7.TabIndex = 14;
            this.label7.Text = "Select VIN to load details";
            // 
            // labelNumVehicles
            // 
            this.labelNumVehicles.AutoSize = true;
            this.labelNumVehicles.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.labelNumVehicles.Location = new Point(12, 93);
            this.labelNumVehicles.Name = "labelNumVehicles";
            this.labelNumVehicles.Size = new Size(27, 32);
            this.labelNumVehicles.TabIndex = 15;
            this.labelNumVehicles.Text = "0";
            // 
            // listViewVehicleStatus
            // 
            this.listViewVehicleStatus.Columns.AddRange(new ColumnHeader[] { this.columnHeader2, this.columnHeader3 });
            this.listViewVehicleStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.listViewVehicleStatus.FullRowSelect = true;
            this.listViewVehicleStatus.GridLines = true;
            this.listViewVehicleStatus.Location = new Point(111, 278);
            this.listViewVehicleStatus.MultiSelect = false;
            this.listViewVehicleStatus.Name = "listViewVehicleStatus";
            this.listViewVehicleStatus.Size = new Size(661, 271);
            this.listViewVehicleStatus.TabIndex = 16;
            this.listViewVehicleStatus.UseCompatibleStateImageBehavior = false;
            this.listViewVehicleStatus.View = View.Details;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 330;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Value";
            this.columnHeader3.Width = 300;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(784, 561);
            this.Controls.Add(this.listViewVehicleStatus);
            this.Controls.Add(this.labelNumVehicles);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxVehicleLastUpdated);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.listViewVehicles);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxDeviceID);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxPass);
            this.Controls.Add(this.textBoxUser);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "JLR Sample";
            this.Load += this.Form1_Load;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBoxUser;
        private TextBox textBoxPass;
        private Label label2;
        private Button buttonConnect;
        private TextBox textBoxDeviceID;
        private Label label3;
        private Button buttonClear;
        private ListView listViewVehicles;
        private Label label4;
        private ColumnHeader columnHeader1;
        private Label label5;
        private TextBox textBoxVehicleLastUpdated;
        private Label label7;
        private Label labelNumVehicles;
        private ListView listViewVehicleStatus;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
    }
}
