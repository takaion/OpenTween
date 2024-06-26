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
using System.Threading.Tasks;
using Moq;
using OpenTween.Connection;
using Xunit;

namespace OpenTween.Api.Misskey
{
    public class NoteTimelineRequestTest
    {
        [Fact]
        public async Task Send_Test()
        {
            var response = TestUtils.CreateApiResponse(new MisskeyNote());

            var mock = new Mock<IApiConnection>();
            mock.Setup(x =>
                    x.SendAsync(It.IsAny<IHttpRequest>())
                )
                .Callback<IHttpRequest>(x =>
                {
                    var request = Assert.IsType<PostJsonRequest>(x);
                    Assert.Equal(new("notes/timeline", UriKind.Relative), request.RequestUri);
                    Assert.Equal(
                        """{"limit":100,"sinceId":"aaaaa","untilId":"bbbbb"}""",
                        request.JsonString
                    );
                })
                .ReturnsAsync(response);

            var request = new NoteTimelineRequest
            {
                Limit = 100,
                SinceId = new("aaaaa"),
                UntilId = new("bbbbb"),
            };
            await request.Send(mock.Object);

            mock.VerifyAll();
        }
    }
}
