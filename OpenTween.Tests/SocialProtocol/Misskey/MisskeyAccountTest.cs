﻿// OpenTween - Client of Twitter
// Copyright (c) 2024 kim_upsilon (@kim_upsilon) <https://upsilo.net/~upsilon/>
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

using System;
using Xunit;

namespace OpenTween.SocialProtocol.Misskey
{
    public class MisskeyAccountTest
    {
        [Fact]
        public void Initialize_Test()
        {
            var accountKey = AccountKey.New();
            using var account = new MisskeyAccount(accountKey);

            var accountSettings = new UserAccount
            {
                UniqueKey = accountKey.Id,
                AccountType = "Misskey",
                ServerHostname = "example.com",
                TokenSecret = "aaaaa",
                UserId = "abcdef",
                Username = "tetete",
            };
            account.Initialize(accountSettings, new());

            Assert.Equal(new("https://example.com/"), account.AccountState.ServerUri);
            Assert.Equal(new MisskeyUserId("abcdef"), account.UserId);
            Assert.Equal("tetete", account.UserName);
        }
    }
}
