﻿namespace Fixie.Tests;

class TestProject : ITestProject
{
    public void Configure(TestConfiguration configuration, TestEnvironment environment)
    {
        if (environment.IsDevelopment())
            configuration.Reports.Add<DiffToolReport>();
        else
            configuration.Reports.Add(new GitHubReport(environment));
    }
}