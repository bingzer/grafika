﻿using Grafika.Animations;
using System;
using System.Threading.Tasks;

namespace Grafika.Services
{
    public interface ICommentService : IService
    {
        Task<AuthenticationToken> GenerateAuthenticationToken(IUser user);
        Task<Uri> GenerateDisqusRemoteUrl(Animation animation, User user);
    }
}
