namespace GameLauncher
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // Dodajemy ProgressBar
        private System.Windows.Forms.ProgressBar progressBarPythonInstall;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.progressBarPythonInstall = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // progressBarPythonInstall
            // 
            this.progressBarPythonInstall.Location = new System.Drawing.Point(12, 420);
            this.progressBarPythonInstall.Name = "progressBarPythonInstall";
            this.progressBarPythonInstall.Size = new System.Drawing.Size(776, 23);
            this.progressBarPythonInstall.TabIndex = 0;
            this.progressBarPythonInstall.Visible = false;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.progressBarPythonInstall);
            this.Name = "Form1";
            this.Text = "Game Launcher";
            this.ResumeLayout(false);
        }
    }
}
