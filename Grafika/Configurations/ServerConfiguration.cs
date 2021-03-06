﻿using Grafika.Utilities;
using System.Collections.Generic;

namespace Grafika.Configurations
{
    public class ServerConfiguration
    {
        public string Url { get; set; }
        public string ApiUrl => Utility.CombineUrl(Url, "api");
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Secret { get; set; }

        public string OAuthCallbackPath { get; set; }

        public string PrivacyPath { get; set; }
        public string EulaPath { get; set; }
        public string BackdropsPath { get; set; }

        public string DefaultAvatarPath { get; set; }
        public string DefaultBackdropPath { get; set; }

        public IEnumerable<string> HandpickedAnimationIds { get; set; }
        public int UsersCount { get; set; }
        public int UsersSearchableCount { get; set; } = 100;
        public int AnimationsCount { get; set; }
        public int AnimationsSearchableCount { get; set; } = 150;

        public string PageTitle { get; set; } = "A stop-motion animation maker | Grafika";
        public string PageDescription { get; set; } = "A super simple stop-motion animation maker, viewer, sharer and community for the web and Android by animators like you provided by Grafika";
        public string PageKeyword { get; set; } = "grafika, animation,stop-motion animation, stop-motion animation maker, online animation maker,animation creator";
    }
}
