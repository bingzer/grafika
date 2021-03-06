﻿namespace Grafika
{
    public class SeriesQueryOptions : SearchQueryOptions
    {
        public const string SortByLastModified = "lastModified";

        public string Name { get; set; }
        public string UserId { get; set; }
        public string AnimationId { get; set; }

        /// <summary>
        /// Auto populate animations
        /// </summary>
        public bool? LoadAnimations { get; set; } = false;
    }
}
