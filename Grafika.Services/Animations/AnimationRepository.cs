﻿using Grafika.Animations;
using Grafika.Data;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Grafika.Utilities;

namespace Grafika.Services.Animations
{
    public class AnimationRepository : RepositoryBase<IDataContext, Animation, AnimationQueryOptions>, IAnimationRepository
    {
        private readonly IBulkRemoveProvider<Animation> _bulkRemoveProvider;
        private readonly ITextSearchProvider<Animation, AnimationQueryOptions> _textSearchProvider;

        public AnimationRepository(IDataContext dataContext,
            ITextSearchProvider<Animation, AnimationQueryOptions> textSearchProvider, 
            IBulkRemoveProvider<Animation> bulkRemoveProvider) 
            : base(dataContext)
        {
            _textSearchProvider = textSearchProvider;
            _bulkRemoveProvider = bulkRemoveProvider;
        }

        public Task RemoveByIds(IEnumerable<string> animationIds)
        {
            return _bulkRemoveProvider.BulkRemove(_dataContext.Animations, animationIds);
        }

        protected override async Task<IEnumerable<Animation>> Query(AnimationQueryOptions options = null)
        {
            IQueryable<Animation> query = _dataContext.Animations;

            if (!string.IsNullOrEmpty(options.Term))
                return await _textSearchProvider.TextSearchAsync(_dataContext.Animations, options);

            if (!string.IsNullOrEmpty(options.Id))
                query = query.Where(q => q.Id == options.Id);
            if (!string.IsNullOrEmpty(options.UserId))
                query = query.Where(q => q.UserId == options.UserId);
            if (options.IsPublic.HasValue)
                query = query.Where(q => q.IsPublic == options.IsPublic);
            if (options.IsRemoved.HasValue)
                query = query.Where(q => q.IsRemoved == options.IsRemoved);
            if (options.IsRandom == true)
                query = CreateRandomQuery(query);
            if (!string.IsNullOrEmpty(options.RelatedToAnimationId))
                query = CreateRelatedToQuery(query, options);

            query = OrderBy(query, options.Sort);

            return query;
        }

        #region Private Supporting Methods
        private IQueryable<Animation> OrderBy(IQueryable<Animation> query, SortOptions sortOptions)
        {
            // order by
            switch (sortOptions?.Name)
            {
                case "views" when sortOptions.Direction == SortDirection.Ascending:
                    query = query.OrderBy(q => q.Views);
                    break;
                case "views" when sortOptions.Direction == SortDirection.Descending:
                    query = query.OrderByDescending(q => q.Views);
                    break;
                case "lastModified" when sortOptions.Direction == SortDirection.Ascending:
                    query = query.OrderBy(q => q.DateModified);
                    break;
                case "lastModified" when sortOptions.Direction == SortDirection.Descending:
                    query = query.OrderByDescending(q => q.DateModified);
                    break;
                case "rating" when sortOptions.Direction == SortDirection.Ascending:
                    query = query.OrderBy(q => q.Rating);
                    break;
                case "rating" when sortOptions.Direction == SortDirection.Descending:
                    query = query.OrderByDescending(q => q.Rating);
                    break;
                default:
                    query = query.OrderByDescending(anim => anim.DateModified);
                    break;
            }
            return query;
        }

        private IQueryable<Animation> CreateRandomQuery(IQueryable<Animation> query)
        {
            Func<Animation, bool> criteria = (anim) => anim.IsRemoved == false && anim.IsPublic == true && anim.TotalFrame > 5;

            var totalCount = _dataContext.Animations.Count(criteria);
            var random = Utility.RandomlyPickFrom(0, totalCount);

            return query.Where(criteria).Skip(random).AsQueryable();
        }

        private IQueryable<Animation> CreateRelatedToQuery(IQueryable<Animation> query, AnimationQueryOptions options)
        {
            Func<Animation, bool> criteria = (anim) => anim.IsRemoved == false && anim.IsPublic == true && anim.TotalFrame > 5 && anim.Id != options.RelatedToAnimationId;

            var totalCount = _dataContext.Animations.Count(criteria);
            var random = Utility.RandomlyPickFrom(0, totalCount);

            return query.Where(criteria).Skip(random).Take(options.PageSize).ToList().AsQueryable();
        }
        #endregion
    }
}