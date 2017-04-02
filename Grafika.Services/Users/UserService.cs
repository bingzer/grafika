﻿using System;
using System.Threading.Tasks;
using Grafika.Utilities;
using Grafika.Configurations;
using Microsoft.Extensions.Options;

namespace Grafika.Services.Users
{
    public class UserService : EntityService<User, UserQueryOptions, IUserRepository>, IUserService
    {
        private readonly IAwsUsersRepository _awsUsers;
        private readonly ContentConfiguration _contentConfig;

        public UserService(IServiceContext userContext, 
            IUserRepository userRepo, 
            IUserValidator validator,
            IAwsUsersRepository awsUsers,
            IOptions<ContentConfiguration> contentOpts)
            : base(userContext, userRepo, validator)
        {
            _awsUsers = awsUsers;
            _contentConfig = contentOpts.Value;
        }

        //public async Task<IEnumerable<User>> GetUsers(UserQueryOptions options)
        //{
        //    var users = await _repo.Find(options);

        //    _validator.Sanitize(users, User);

        //    return users;
        //}

        //public async Task<User> GetUser(string userId)
        //{
        //    var user = await FindById(userId);

        //    _validator.Sanitize(new User[] { user }, User);

        //    return user;
        //}

        //public async Task<User> CreateUser(User user)
        //{
        //    Ensure.ArgumentNotNull(user, "user");

        //    _validator.Validate(user);

        //    return await _repo.Add(user);
        //}

        //public async Task<User> UpdateUser(User user)
        //{
        //    Ensure.ArgumentNotNull(user, "user");

        //    await _repo.CheckUsernameAvailability(user, user.Username);

        //    var update = new User(user.Id)
        //    {
        //        DateModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        //        FirstName = user.FirstName,
        //        LastName = user.LastName,
        //        Prefs = user.Prefs,
        //        Subscriptions = user.Subscriptions,
        //        Username = user.Username
        //    };

        //    return await _repo.Update(update);
        //}

        //public async Task<User> DeleteUser(string userId)
        //{
        //    var user = await FindById(userId);

        //    return await _repo.Remove(user);
        //}

        public async Task UpdateLastSeen(User user)
        {
            var update = new User(user.Id) { Stats = new UserStats { DateLastSeen = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() } };

            await Repository.Update(update);
        }

        public async Task<string> GetAvatarOrBackdropUrl(string userId, string type)
        {
            var user = await GetById(userId);

            string url = null;
            switch (type)
            {
                case "avatar":
                    url = user?.Prefs?.Avatar ?? Utility.CombineUrl(_contentConfig.Url, _contentConfig.DefaultAvatarPath);
                    break;
                case "backdrop":
                    url = user?.Prefs?.Backdrop ?? Utility.CombineUrl(_contentConfig.Url, _contentConfig.DefaultBackdropPath);
                    break;
                default: throw new NotValidException("type = " + type);
            }

            // fixed stupid relative url
            if (url.StartsWith("/") || url.StartsWith("//"))
                url = Utility.CombineUrl(_contentConfig.Url, url);

            return url;
        }

        public async Task<ISignedUrl> CreateSignedUrl(string userId, string imageType, string contentType)
        {
            var user = await GetById(userId);
            return await _awsUsers.CreateSignedUrl(user, imageType, contentType);
        }

        protected internal override async Task<User> CreateEntityForUpdate(User source)
        {
            await Repository.CheckUsernameAvailability(source, source.Username);

            return new User(source.Id)
            {
                DateModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                FirstName = source.FirstName,
                LastName = source.LastName,
                Prefs = source.Prefs,
                Subscriptions = source.Subscriptions,
                Username = source.Username
            };
        }

        //private async Task<User> FindById(string userId)
        //{
        //    if (!_repo.ValidateId(userId))
        //        throw new NotFoundExeption();

        //    var user = await _repo.First(new UserQueryOptions { Id = userId });
        //    if (user == null)
        //        throw new NotFoundExeption();

        //    return user;
        //}
    }
}
