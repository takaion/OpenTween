﻿// OpenTween - Client of Twitter
// Copyright (c) 2007-2011 kiri_feather (@kiri_feather) <kiri.feather@gmail.com>
//           (c) 2008-2011 Moz (@syo68k)
//           (c) 2008-2011 takeshik (@takeshik) <http://www.takeshik.org/>
//           (c) 2010-2011 anis774 (@anis774) <http://d.hatena.ne.jp/anis774/>
//           (c) 2010-2011 fantasticswallow (@f_swallow) <http://twitter.com/f_swallow>
// All rights reserved.
//
// This file is part of OpenTween.
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option)
// any later version.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License
// for more details.
//
// You should have received a copy of the GNU General Public License along
// with this program. If not, see <http://www.gnu.org/licenses/>, or write to
// the Free Software Foundation, Inc., 51 Franklin Street - Fifth Floor,
// Boston, MA 02110-1301, USA.

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTween.Models;
using OpenTween.SocialProtocol.Twitter;

namespace OpenTween
{
    public partial class ListAvailable : OTBaseForm
    {
        public ListElement? SelectedList { get; private set; }

        public ListAvailable()
            => this.InitializeComponent();

        private void OK_Button_Click(object sender, EventArgs e)
        {
            if (this.ListsList.SelectedIndex > -1)
            {
                this.SelectedList = (ListElement)this.ListsList.SelectedItem;
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            this.SelectedList = null;
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private async void ListAvailable_Shown(object sender, EventArgs e)
        {
            using (ControlTransaction.Disabled(this))
            {
                try
                {
                    var lists = (IReadOnlyList<ListElement>)TabInformations.GetInstance().SubscribableLists;
                    if (lists.Count == 0)
                        lists = await this.FetchListsAsync();

                    this.UpdateListsListBox(lists);
                }
                catch (OperationCanceledException)
                {
                    this.DialogResult = DialogResult.Cancel;
                    return;
                }
                catch (WebApiException)
                {
                    this.DialogResult = DialogResult.Abort;
                    return;
                }
            }
        }

        private void ListsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListElement? lst;
            if (this.ListsList.SelectedIndex > -1)
            {
                lst = (ListElement)this.ListsList.SelectedItem;
            }
            else
            {
                lst = null;
            }
            if (lst == null)
            {
                this.UsernameLabel.Text = "";
                this.NameLabel.Text = "";
                this.StatusLabel.Text = "";
                this.MemberCountLabel.Text = "0";
                this.SubscriberCountLabel.Text = "0";
                this.DescriptionText.Text = "";
            }
            else
            {
                this.UsernameLabel.Text = lst.Username + " / " + lst.Nickname;
                this.NameLabel.Text = lst.Name;
                if (lst.IsPublic)
                {
                    this.StatusLabel.Text = "Public";
                }
                else
                {
                    this.StatusLabel.Text = "Private";
                }
                this.MemberCountLabel.Text = lst.MemberCount.ToString("#,##0");
                this.SubscriberCountLabel.Text = lst.SubscriberCount.ToString("#,##0");
                this.DescriptionText.Text = lst.Description;
            }
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            using (ControlTransaction.Disabled(this))
            {
                try
                {
                    var lists = await this.FetchListsAsync();
                    this.UpdateListsListBox(lists);
                }
                catch (OperationCanceledException)
                {
                }
                catch (WebApiException ex)
                {
                    MessageBox.Show("Failed to get lists. (" + ex.Message + ")");
                }
            }
        }

        private async Task<IReadOnlyList<ListElement>> FetchListsAsync()
        {
            using var dialog = new WaitingDialog("Getting Lists...");
            var cancellationToken = dialog.EnableCancellation();

            var account = ((TweenMain)this.Owner).CurrentTabAccount;
            if (account is not TwitterAccount twitterAccount)
                throw new WebApiException($"Unsupported account type: {account.AccountType}");

            var task = twitterAccount.Legacy.GetListsApi();
            await dialog.WaitForAsync(this, task);

            cancellationToken.ThrowIfCancellationRequested();

            return TabInformations.GetInstance().SubscribableLists;
        }

        private void UpdateListsListBox(IEnumerable<ListElement> lists)
        {
            using (ControlTransaction.Update(this.ListsList))
            {
                this.ListsList.Items.Clear();
                this.ListsList.Items.AddRange(lists.ToArray());
                if (this.ListsList.Items.Count > 0)
                {
                    this.ListsList.SelectedIndex = 0;
                }
                else
                {
                    this.UsernameLabel.Text = "";
                    this.NameLabel.Text = "";
                    this.StatusLabel.Text = "";
                    this.MemberCountLabel.Text = "0";
                    this.SubscriberCountLabel.Text = "0";
                    this.DescriptionText.Text = "";
                }
            }
        }
    }
}
