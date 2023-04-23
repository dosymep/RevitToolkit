﻿using System.IO.Enumeration;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    Target Compile => _ => _
        .TriggeredBy(Cleaning)
        .Executes(() =>
        {
            foreach (var configuration in GlobBuildConfigurations())
                DotNetBuild(settings => settings
                    .SetConfiguration(configuration)
                    .SetVersion(GetPackVersion(configuration))
                    .SetVerbosity(DotNetVerbosity.Minimal));
        });

    string GetPackVersion(string configuration)
    {
        if (VersionMap.TryGetValue(configuration, out var value)) return value;
        throw new Exception($"Can't find pack version for configuration: {configuration}");
    }

    List<string> GlobBuildConfigurations()
    {
        var configurations = Solution.Configurations
            .Select(pair => pair.Key)
            .Select(config =>
            {
                var platformIndex = config.LastIndexOf('|');
                return config.Remove(platformIndex);
            })
            .Where(config =>
            {
                foreach (var wildcard in Configurations)
                    if (FileSystemName.MatchesSimpleExpression(wildcard, config))
                        return true;

                return false;
            })
            .ToList();

        if (configurations.Count == 0)
            throw new Exception($"The solution's configurations cannot be found using the specified patterns: {string.Join(" | ", Configurations)}");

        return configurations;
    }
}