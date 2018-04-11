﻿using System;
using System.Collections.Generic;
using System.Linq;
using AppGet.Http;
using AppGet.Update;
using NLog;

namespace AppGet.Github.Releases
{
    public interface IReleaseClient
    {
        List<AppGetRelease> GetReleases();
    }

    public class GitHubReleaseClient : IReleaseClient
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public GitHubReleaseClient(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public List<AppGetRelease> GetReleases()
        {
            _logger.Trace("Checking for AppGet updates...");
            var uri = new Uri($"https://api.github.com/repos/appget/appget/releases?{GithubKeys.AuthQuery}&no_cache={Guid.NewGuid()}");
            var response = _httpClient.Get(uri);
            var releases = response.AsResource<List<GithubRelease>>();
            _logger.Trace($"Found {releases.Count} releases");

            return releases.Select(c => new AppGetRelease
                {
                    Url = c.Assets.Single(a => a.browser_download_url.EndsWith(".exe")).browser_download_url,
                    Version = new Version(c.tag_name)
                })
                .ToList();
        }
    }
}