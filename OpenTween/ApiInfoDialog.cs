﻿// OpenTween - Client of Twitter
// Copyright (c) 2015 spx (@5px)
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
using OpenTween.Api;
using OpenTween.Api.TwitterV2;

namespace OpenTween
{
    public partial class ApiInfoDialog : OTBaseForm
    {
        private RateLimitCollection? rateLimits;
        private IDisposable? unsubscribeRateLimitUpdate;

        public RateLimitCollection? RateLimits
        {
            get => this.rateLimits;
            set
            {
                if (this.rateLimits == value)
                    return;

                this.rateLimits = value;
                this.InitializeRateLimits(value);
            }
        }

        public ApiInfoDialog()
            => this.InitializeComponent();

        private readonly List<string> tlEndpoints = new()
        {
            GetTimelineRequest.EndpointName,
            "/statuses/mentions_timeline",
            "/statuses/show/:id",
            "/statuses/user_timeline",
            "/favorites/list",
            "/direct_messages/events/list",
            "/direct_messages",
            "/direct_messages/sent",
            "/lists/statuses",
            "/search/tweets",
        };

        private void InitializeRateLimits(RateLimitCollection? rateLimits)
        {
            this.ListViewApi.Items.Clear();
            this.unsubscribeRateLimitUpdate?.Dispose();

            if (rateLimits == null)
                return;

            // TL更新用エンドポイントの追加
            var group = this.ListViewApi.Groups[0];
            foreach (var endpoint in this.tlEndpoints)
            {
                var apiLimit = rateLimits[endpoint];
                if (apiLimit == null)
                    continue;

                this.AddListViewItem(endpoint, apiLimit, group);
            }

            // その他
            group = this.ListViewApi.Groups[1];
            var apiStatuses = rateLimits.Where(x => !this.tlEndpoints.Contains(x.Key)).OrderBy(x => x.Key);
            foreach (var (endpoint, apiLimit) in apiStatuses)
            {
                this.AddListViewItem(endpoint, apiLimit, group);
            }

            this.unsubscribeRateLimitUpdate = rateLimits.SubscribeAccessLimitUpdated(this.TwitterApiStatus_AccessLimitUpdated);
        }

        private void AddListViewItem(string endpoint, ApiLimit apiLimit, ListViewGroup group)
        {
            var subitems = new[]
            {
                endpoint,
                apiLimit.AccessLimitRemain + "/" + apiLimit.AccessLimitCount,
                apiLimit.AccessLimitResetDate.ToLocalTimeString(),
            };
            var item = new ListViewItem(subitems)
            {
                Group = group,
            };

            this.ListViewApi.Items.Add(item);
        }

        private void UpdateEndpointLimit(string endpoint)
        {
            var apiLimit = this.RateLimits?[endpoint];
            if (apiLimit != null)
            {
                var item = this.ListViewApi.Items.Cast<ListViewItem>().Single(x => x.SubItems[0].Text == endpoint);
                item.SubItems[1].Text = apiLimit.AccessLimitRemain + "/" + apiLimit.AccessLimitCount;
                item.SubItems[2].Text = apiLimit.AccessLimitResetDate.ToLocalTimeString();
            }
        }

        private async void TwitterApiStatus_AccessLimitUpdated(RateLimitCollection sender, RateLimitCollection.AccessLimitUpdatedEventArgs e)
        {
            try
            {
                if (this.InvokeRequired && !this.IsDisposed)
                {
                    await this.InvokeAsync(() => this.TwitterApiStatus_AccessLimitUpdated(sender, e));
                }
                else
                {
                    var endpoint = e.EndpointName;
                    if (endpoint != null)
                        this.UpdateEndpointLimit(endpoint);
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            ScaleChildControl(this.ListViewApi, factor);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.unsubscribeRateLimitUpdate?.Dispose();
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    // ちらつき軽減用
    public class BufferedListView : ListView
    {
        public BufferedListView()
            => this.DoubleBuffered = true;
    }
}
