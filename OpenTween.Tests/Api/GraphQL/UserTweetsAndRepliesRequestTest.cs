﻿// OpenTween - Client of Twitter
// Copyright (c) 2023 kim_upsilon (@kim_upsilon) <https://upsilo.net/~upsilon/>
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

using System.Threading.Tasks;
using Moq;
using OpenTween.Connection;
using OpenTween.Models;
using Xunit;

namespace OpenTween.Api.GraphQL
{
    public class UserTweetsAndRepliesRequestTest
    {
        [Fact]
        public async Task Send_Test()
        {
            using var apiResponse = await TestUtils.CreateApiResponse("Resources/Responses/UserTweetsAndReplies_SimpleTweet.json");

            var mock = new Mock<IApiConnection>();
            mock.Setup(x =>
                    x.SendAsync(It.IsAny<IHttpRequest>())
                )
                .Callback<IHttpRequest>(x =>
                {
                    var request = Assert.IsType<GetRequest>(x);
                    Assert.Equal(new("https://twitter.com/i/api/graphql/YlkSUg0mRBx7-EkxCvc-bw/UserTweetsAndReplies"), request.RequestUri);
                    var query = request.Query!;
                    Assert.Equal(2, query.Count);
                    Assert.Equal("""{"userId":"40480664","count":20,"includePromotedContent":true,"withCommunity":true,"withVoice":true,"withV2Timeline":true}""", query["variables"]);
                    Assert.True(query.ContainsKey("features"));
                    Assert.Equal("UserTweetsAndReplies", request.EndpointName);
                })
                .ReturnsAsync(apiResponse);

            var request = new UserTweetsAndRepliesRequest(userId: new("40480664"))
            {
                Count = 20,
            };

            var response = await request.Send(mock.Object);
            Assert.Single(response.Tweets);
            Assert.Equal("DAABCgABF_tTnZvAJxEKAAIWes8rE1oQAAgAAwAAAAEAAA", response.CursorTop?.Value.Value);
            Assert.Equal("DAABCgABF_tTnZu__-0KAAIWZa6KTRoAAwgAAwAAAAIAAA", response.CursorBottom?.Value.Value);

            mock.VerifyAll();
        }

        [Fact]
        public async Task Send_RequestCursor_Test()
        {
            using var apiResponse = await TestUtils.CreateApiResponse("Resources/Responses/UserTweetsAndReplies_SimpleTweet.json");

            var mock = new Mock<IApiConnection>();
            mock.Setup(x =>
                    x.SendAsync(It.IsAny<IHttpRequest>())
                )
                .Callback<IHttpRequest>(x =>
                {
                    var request = Assert.IsType<GetRequest>(x);
                    Assert.Equal(new("https://twitter.com/i/api/graphql/YlkSUg0mRBx7-EkxCvc-bw/UserTweetsAndReplies"), request.RequestUri);
                    var query = request.Query!;
                    Assert.Equal(2, query.Count);
                    Assert.Equal("""{"userId":"40480664","count":20,"includePromotedContent":true,"withCommunity":true,"withVoice":true,"withV2Timeline":true,"cursor":"aaa"}""", query["variables"]);
                    Assert.True(query.ContainsKey("features"));
                    Assert.Equal("UserTweetsAndReplies", request.EndpointName);
                })
                .ReturnsAsync(apiResponse);

            var request = new UserTweetsAndRepliesRequest(userId: new("40480664"))
            {
                Count = 20,
                Cursor = new("aaa"),
            };

            await request.Send(mock.Object);
            mock.VerifyAll();
        }
    }
}
