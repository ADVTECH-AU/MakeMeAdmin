﻿// <copyright file="SubmitRequestForm.cs" company="Sinclair Community College">
// Copyright (c) Sinclair Community College. All rights reserved.
// </copyright>

namespace SinclairCC.MakeMeAdmin
{
    using System;
    using System.Reflection;
    using System.ServiceModel;
    using System.Security.Principal;
    using System.Windows.Forms;

    /// <summary>
    /// This form allows the user to submit a request for administrator-level rights.
    /// </summary>
    internal partial class SubmitRequestForm : Form
    {
        private bool userIsDirectAdmin = false;
        private bool userWasAdminOnLastCheck = false;
        private System.Timers.Timer notifyIconTimer;

        /// <summary>
        /// Initializes a new instance of the SubmitRequestForm class.
        /// </summary>
        public SubmitRequestForm()
        {
            this.InitializeComponent();
            this.Icon = Properties.Resources.SecurityLock;
            this.notifyIcon.Icon = Properties.Resources.SecurityLock;
            this.SetFormText();

            this.notifyIconTimer = new System.Timers.Timer(5000);
            this.notifyIconTimer.AutoReset = true;
            this.notifyIconTimer.Elapsed += NotifyIconTimerElapsed;
        }

        void NotifyIconTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.UpdateUserAdministratorStatus();
            if (this.userIsDirectAdmin != this.userWasAdminOnLastCheck)
            {
                this.userWasAdminOnLastCheck = this.userIsDirectAdmin;
                if (!this.userIsDirectAdmin)
                {
                    this.notifyIconTimer.Stop();
                    notifyIcon.ShowBalloonTip(5000, "Make Me Admin", "You are no longer a member of the Administrators group.", ToolTipIcon.Info);
                }
            }
        }

        private void SetFormText()
        {
            System.Text.StringBuilder formText = new System.Text.StringBuilder();
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (attributes.Length == 0)
            {
                formText.Append("Make Me Admin");
            }
            else
            {
                formText.Append(((AssemblyProductAttribute)attributes[0]).Product);
            }
            formText.Append(' ');
            formText.Append(Assembly.GetExecutingAssembly().GetName().Version.ToString(2));

            this.Text = formText.ToString();
            this.notifyIcon.Text = formText.ToString();
        }

        /// <summary>
        /// This function handles the Click event for the Submit button.
        /// </summary>
        /// <param name="sender">
        /// The button being clicked.
        /// </param>
        /// <param name="e">
        /// Data specific to this event.
        /// </param>
        private void ClickSubmitButton(object sender, EventArgs e)
        {
            this.DisableButtons();
            this.appStatus.Text = "Adding you to the Administrators group.";
            addUserBackgroundWorker.RunWorkerAsync();
        }

        private void addUserBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string address = string.Format("net.pipe://{0}/MakeMeAdmin/Service", Shared.GetFullyQualifiedHostName());
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            EndpointAddress ep = new EndpointAddress(address);
            IServiceContract channel = ChannelFactory<IServiceContract>.CreateChannel(binding, ep);
            try
            {
                channel.AddPrincipalToAdministratorsGroup(WindowsIdentity.GetCurrent().User.Value);
            }
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
                // TODO: What do we do with this error?
            }
        }

        private void addUserBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(this, "An error occurred while adding you to the Administrators group." + System.Environment.NewLine + "Please make sure the Make Me Admin service is running.", "Make Me Admin", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, 0);
            }

            this.UpdateUserAdministratorStatus();

            if (this.userIsDirectAdmin)
            {
                this.appStatus.Text = "Ready.";
                this.userWasAdminOnLastCheck = true;
                this.notifyIconTimer.Start();
                notifyIcon.Visible = true;
                this.Visible = false;
                this.ShowInTaskbar = false;
                notifyIcon.ShowBalloonTip(5000, "Make Me Admin", "You are now a member of the Administrators group.", ToolTipIcon.Info);
            }
            else if (!buttonStateWorker.IsBusy)
            {
                buttonStateWorker.RunWorkerAsync();
            }
        }

        private void ClickRemoveRightsButton(object sender, EventArgs e)
        {
            this.DisableButtons();
            this.appStatus.Text = "Removing you from the Administrators group.";
            removeUserBackgroundWorker.RunWorkerAsync();
        }

        private void removeUserBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string address = string.Format("net.pipe://{0}/MakeMeAdmin/Service", Shared.GetFullyQualifiedHostName());
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            EndpointAddress ep = new EndpointAddress(address);
            IServiceContract channel = ChannelFactory<IServiceContract>.CreateChannel(binding, ep);
            channel.RemovePrincipalFromAdministratorsGroup(System.Security.Principal.WindowsIdentity.GetCurrent().User.Value);
        }

        private void removeUserBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(this, "An error occurred while removing you from the Administrators group." + System.Environment.NewLine + "Please make sure the Make Me Admin service is running.", "Make Me Admin", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, 0);
            }

            if (!buttonStateWorker.IsBusy)
            {
                buttonStateWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// This function handles the Click event for the Exit button.
        /// </summary>
        /// <param name="sender">
        /// The button being clicked.
        /// </param>
        /// <param name="e">
        /// Data specific to this event.
        /// </param>
        private void ClickExitButton(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DisableButtons()
        {
            this.addMeButton.Enabled = false;
            this.removeMeButton.Enabled = false;
        }

        private void FormLoad(object sender, EventArgs e)
        {
            this.DisableButtons();
            this.appStatus.Text = Properties.Resources.CheckingAdminStatus;
            buttonStateWorker.RunWorkerAsync();
        }

        private void UpdateUserAdministratorStatus()
        {
            this.userIsDirectAdmin = LocalAdministratorGroup.CurrentUserIsMemberOfAdministratorsDirectly();
        }

        private void DoButtonStateWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            this.UpdateUserAdministratorStatus();
        }

        private void ButtonStateWorkCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            bool userIsAuthorized = Shared.UserIsAuthorized(WindowsIdentity.GetCurrent());
            this.addMeButton.Enabled = !this.userIsDirectAdmin && userIsAuthorized;
            if (addMeButton.Enabled)
            {
                addMeButton.Text = "Grant Me Administrator Rights";
            }
            else if (this.userIsDirectAdmin)
            {
                addMeButton.Text = "You already have administrator rights.";
            }
            else if (!userIsAuthorized)
            {
                addMeButton.Text = "You are not authorized to use this application.";
            }
            this.removeMeButton.Enabled = this.userIsDirectAdmin;
            this.appStatus.Text = "Ready.";

            if (this.addMeButton.Enabled)
            {
                this.addMeButton.Focus();
            }
            else if (this.removeMeButton.Enabled)
            {
                this.removeMeButton.Focus();
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
        }

        private void SubmitRequestForm_VisibleChanged(object sender, EventArgs e)
        {
            if (!buttonStateWorker.IsBusy)
            {
                buttonStateWorker.RunWorkerAsync();
            }
        }

        private void notifyIcon_BalloonTipClosed(object sender, EventArgs e)
        {
            if (!this.userIsDirectAdmin)
            {
                /*
                notifyIcon.Visible = false;
                this.Visible = true;
                this.ShowInTaskbar = true;
                */
                this.Close();
            }

        }
    }
}
