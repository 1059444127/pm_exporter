using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pm.Service
{
    [RunInstaller(true)]
    public abstract class  WinServiceInstaller: System.Configuration.Install.Installer
    {
        protected abstract IPmServiceMetadata GetServiceMetadata();

        protected WinServiceInstaller()
        {
            InitializeComponent();


            var metadata = GetServiceMetadata();

            if (metadata == null)
                throw new NullReferenceException("для ServiceInstaller необходимо определить поле ServiceMetadata");

            this.serviceInstaller1.Description = metadata.ServiceDescription;
            this.serviceInstaller1.DisplayName = metadata.ServiceDisplayName;
            this.serviceInstaller1.ServiceName = metadata.ServiceName;
            this.serviceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }


        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller1
            // 
            this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceProcessInstaller1.Password = null;
            this.serviceProcessInstaller1.Username = null;
            this.serviceProcessInstaller1.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstaller1_AfterInstall);
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller1.Description = "Служба МКС";
            this.serviceInstaller1.DisplayName = "Служба МКС";
            this.serviceInstaller1.ServiceName = "ServerVts";
            this.serviceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.serviceInstaller1.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceInstaller1_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
                this.serviceProcessInstaller1,
                this.serviceInstaller1});
        }

        #endregion

        protected System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
        protected System.ServiceProcess.ServiceInstaller serviceInstaller1;
    }
}
