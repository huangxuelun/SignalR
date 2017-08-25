// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ChatSample.Hubs
{
    [Authorize]
    public class Chat : HubWithPresence
    {
        public Chat(IUserTracker<Chat> userTracker)
            : base(userTracker)
        {
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId).InvokeAsync("SetUsersOnline", await GetUsersOnline());
            await Groups.AddAsync(Context.ConnectionId, "test");

            await base.OnConnectedAsync();
        }

        public override Task OnUsersJoined(UserDetails[] users)
        {
            return Clients.Client(Context.ConnectionId).InvokeAsync("UsersJoined", new[] { users });
        }

        public override Task OnUsersLeft(UserDetails[] users)
        {
            return Clients.Client(Context.ConnectionId).InvokeAsync("UsersLeft", new[] { users });
        }

        public async Task Send(string message)
        {
            if (message.StartsWith("remove "))
            {
                message = message.Substring(7);
                await Groups.RemoveAsync(message, "test");
            }
            else
                await Clients.Group("test").InvokeAsync("Send", Context.User.Identity.Name, message);
            //await Clients.All.InvokeAsync("Send", Context.User.Identity.Name, message);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return Groups.RemoveAsync(Context.ConnectionId, "test");
        }
    }
}
