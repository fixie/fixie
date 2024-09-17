## Start With an Issue

It's best to start the discussion with an Issue. This way, we can come to agreement and set expectations before you commit time and effort to a change that might be rejected.


## Bug Fixes

Even unanticipated bug fix PRs are very likely to be included, but you can improve the odds by providing as much context and clarity as possible in your PR. The more clear your description of the problem and of your reasoning behind the fix, the easier it is to evaluate the change for inclusion.

Your bug fix is most likely to be adopted swiftly if you include a minimal reproduction case, either in the PR description or ideally within the branch. If you can do so within the branch, reveal the problem with a failing test and then resolve it. If the revelation and resolution are in distinct commits, that makes it easier to truly witness and confirm the problem during code review.


## Missing Description

**An unanticipated PR with little to no Description is very likely to be rejected.**

Receiving a PR is a job. A job can be welcome and fulfilling, but a job with no context is likely one you wouldn't want to sign up for either. Asking the reviewer to spend time and energy just to figure out what is going on or what train of thought you had in secret is a rough way to start a conversation.


## New Features

**An unanticipated PR that adds a new feature is very likely to be rejected.**

The Fixie organization's driving design goal is to provide minimal APIs with flexible and idiomatic customization mechanisms *so that adding new features won't even be necessary*. If something feels like a missing feature, the real missing feature may be some broader change to the customization mechanisms.

Please start by opening an Issue to discuss the use case and the proposed change.


## Breaking Changes

**An unanticipated PR that includes breaking changes is very likely to be rejected.**

To minimize wasted effort and rework, major version bumps need to take into account the .NET framework release cycle as detailed below. We need to coordinate and plan ahead for breaking changes that would involve a major version bump.

A breaking change also has a large hidden cost in the QA effort prior to a release: most of Fixie's development effort hides in its IDE integrations, which can only truly be tested by hand in end-to-end sample solutions outside of the Fixie solution itself.

Please start by opening an Issue to discuss the motivation, the proposed change, and the full scope of the change beyond the code itself.


## Code Style Changes

**An unanticipated PR that includes code style changes like whitespace or naming conventions is extremely likely to be rejected.**

These are incredibly disruptive to any other branch in progress, and increases the code review effort when such changes are mixed in with another substantive change.


## Sweeping Changes

**An unanticipated PR that includes sweeping changes to many files across the solution is extremely likely to be rejected.**

An example of such a change would be adopting a new C# feature solution-wide without a motivating use case. These are incredibly disruptive to any other branch in progress.

Although we do apply this kind of work from time to time, it is often a carefully scheduled activity early in development of a planned breaking major version bump, and that timing is influenced by Microsoft's own target framework support windows.

Please start by opening an Issue to discuss the motivation, the proposed change, and the risks of adding defects.


## Target Framework Changes

**Honestly, you probably don't want to do this.** Adding or removing a target framework (eg. `net9.0`) is especially impactful and carries with it several gotchas specific to Fixie's roadmap planning, implementation, and end user environment support goals.

**An unanticipated PR for adding or removing a target framework will be closed.**

An *anticipated* PR for adding or removing a target framework must follow the guidance below *to the letter* to ensure success.

* We commit to the following "support windows":

    | Fixie Version  | Supported Target Frameworks |
    | ------------- | ------------- |
    | `3.x`  | `netcoreapp3.1`, `net6.0`, `net7.0`, `net8.0` |
    | *4.x (planned)* | `net9.0`  |

* We can adopt a new target framework the moment it is available, taking care to treat it as a non-breaking change. Additionally, we want to add such a framework *before* the next breaking major version bump to this project, to ensure end users have a frictionless upgrade path as they migrate their own solutions to newer target frameworks.

    * As soon as a new target framework has a Release Candidate published, we can experiment with it in a branch, following the PR guidance below, but should **not** merge that work as sound until after the final release is published.

* We can only remove a target framework in agreement with this project's roadmap and promised support window. Otherwise, we break the experience for end users who are still using the older framework. In general, we defer removing a framework until the next planned breaking major version, and when we do we take into account Microsoft's own support windows in deciding what to deprecate. Although in general we wish to drop target frameworks when the frameworks themselves fall out of Microsoft's support window, it's more important to this project that we avoid breaking end users who aren't able to migrate away from them as aggressively. A team *trying* to upgrade from a deprecated target framework needs their tests to run throughout the process! Also, because every *other* .NET release is a Long Term Support release, we are very likely to retain the intervening non-LTS target frameworks longer than their official sunset date by Microsoft.

* Since removing old frameworks often raises the lowest common C# version available to us, it can be tempting to adopt new language features in the same PR when removing a target framework. Do not mix such changes in with the target framework removal PR. We'd rather adopt such things with care in separate PRs so that the historically-challenging framework changes can be evaluated as easily as possible.

* Because target framework changes are already subtle enough to warrant these instructions, and because target framework changes **significantly increase the end to end QA effort prior to each release**, avoid including any additional changes in the branch, deferring nice-to-haves for subsequent work. If during development you detect a *need* for an additional change, consider this as evidence for that change to be submitted in isolation *prior* to this PR, so that this PR can stick to the guided commits below.

* If you need to add or update `#if` conditional compilation logic, try to phrase those blocks with explicit mentions of the relevant frameworks (e.g. `#if NET9_0`). When dealing with `#else`, make sure that the overall conditional is phrased such that the *oldest framework appears literally in the directives*. Imagine years later when that specific target framework is to be removed: by phrasing these explicitly with their eventual removal in mind, we set ourselves up for success, ensuring that simple repo-wide text searches will draw us to every location that needs attention at the time the target framework is removed, ensuring the elimination of dead code and avoiding subtle silent behavior changes!

* Do not use automated migration tools. Verifying the output at code review time would involve doing the work again manually in a throw-away branch and then diffing the branch heads anyway, which as outlined below results in a nicely focused branch that is easy to review on its own.

* The code changes provoked by build and test failures during the processes below tend to be subtle and impactful. Making a change just to get a test to turn green, for instance, might in fact be hiding a new defect or regression. Be sure to provide context and reasoning behind each code change in the commits themselves. This increases the chances of a successful and swift code review and merge.

### Adding a Supported Target Framework?

<details><summary>Follow exactly this commit by commit guidance...</summary>

1. Include the new target framework's *Major.Minor* .NET SDK version number in all GitHub Actions workflows when `actions/setup-dotnet` is invoked.
   
   Although the build image likely has the SDKs installed already, resulting in a fast no-op, being explicit here future-proofs our builds in the event GitHub later phases out an SDK from the build image.
   
   Commit.

2. Visit and address all `<TargetFramework>` and `<TargetFrameworks>` elements in project reference dependency order so that each individual `*.csproj` line touched can appear in distinct commits, making it more clear which other code changes are direct consequences of which `*.csproj` touches, and to ease development by letting you address things one at a time. The goal upon each `*.csproj` update is to get the solution compiling again. Common issues at this point include new compiler warnings and errors. You may also encounter build issues provoked by existing `#if` directives, urging you to account for the new target framework's own compilation symbol.

    1. Add the target framework to `<TargetFrameworks>` in `Fixie.csproj` and resolve any build issues.
       
       Commit.

    2. **Do NOT place the target framework in** `Fixie.Console.csproj`.
       
       As a `dotnet tool` definition, this project must only target the *lowest* framework from our framework support window. It relies on `<RollForward>Major</RollForward>` instead of cross-targeting, so that the command `dotnet fixie` will still work in the presence of varied installed frameworks.

    3. Add the target framework to `<TargetFrameworks>` in `Fixie.TestAdapter.csproj` and resolve any build issues.
       
       Commit.

    4. Due to unfortunate influence from the atypical `Microsoft.NET.Test.Sdk` package, `Fixie.TestAdapter` is packaged using an explicit `*.nuspec` file. Any change to the `<TargetFrameworks>` and `<PackageReference ...>` elements must be mirrored in the nuspec. In particular, `<dependencies>` needs a `<group ...>` dedicated to the new target framework and its dependencies, and the right build of `Fixie.TestAdapter.dll` must be explicitly placed into the package's `lib/` folder structure.
       
       Commit.

    5. Add the target framework to `<TargetFrameworks>` in `Fixie.Tests.csproj` and resolve any build issues.
       
       Commit.

3. Now that the new target framework has been well-placed throughout the solution, and the solution builds, run all tests and resolve test failures.
   
   Since `Fixie.Tests` is targeting all the supported frameworks, this test run will be the first chance to expose any *runtime* behavior variance introduced by the new target framework. Ensure the tests can pass meaningfully across the target framework support window.
   
   Resolve each kind of failure in a dedicated commit with a clear explanation.

4. Run `pwsh ./build.ps1 --pack` to locally exercise the packaging steps and resolve any issues.
   
   The most likely thing to go wrong here would be in failing to mirror `Fixie.TestAdapter` changes in both the `*.csproj` and `*.nuspec` files.
   
   Verify that all built package files have the same intended version number.
   
   Resolve each kind of failure in a dedicated commit with a clear explanation.
</details>

### Removing a Supported Target Framework?

<details><summary>Follow exactly this commit by commit guidance...</summary>

These commits approach the projects in dependency order, so that the whole solution meaningfully builds and passes tests at each step. For the steps that alter a project's `<TargetFramework>` or `<TargetFrameworks>` values, the commit should show the update to that value, the resolution of any resulting build errors, and the removal of now-unreachable code for affected `#if` conditional compilation directives. After the project-specific commit, there should be no remaining occurrences of the corresponding conditional compilation symbol in that project. Additionally, scan all `#if` directives in that project in case they are indirectly affected by the removal of the target framework, such as implicit relevance when some *other* symbol is negated (e.g. `#if !NET9_0` blocks may be affected when `net8.0` is being removed).

1. Fixie.Tests.csproj
   
   Commit:
   > Remove deprecated target framework from the `Fixie.Tests` project, phasing out now-unreachable conditional compilation paths.

2. Fixie.TestAdapter.csproj
   
   Commit:
   > Remove deprecated target framework from the `Fixie.TestAdapter` project, phasing out now-unreachable conditional compilation paths.

3. Fixie.TestAdapter.nuspec
   
   Due to unfortunate influence from the atypical `Microsoft.NET.Test.Sdk` package, `Fixie.TestAdapter` is packaged using an explicit `*.nuspec` file. Any change to the `<TargetFrameworks>` and `<PackageReference ...>` elements in the previous commit must be mirrored in the nuspec. In particular, `<group ...>` and `<file ...>` elements will be dedicated to each intended target framework.
   
   Commit:
   > Remove deprecated target framework from the `Fixie.TestAdapter` nuspec, so that it aligns with the corresponding csproj.

4. Fixie.csproj
   
   Commit:
   > Remove deprecated target framework from the `Fixie` project, phasing out now-unreachable conditional compilation paths.

5. Fixie.Console.csproj
   
   Because it defines a `dotnet ...` tool, this project must only target the *lowest* supported target framework version. Therefore, when removing the target framework present in this project, the real move is to update it to the *new* lowest end of the support window.
   
   Commit:
   > Raise deprecated target framework in the `Fixie.Console` project to the next lowest target framework found across all projects, allowing `RollForward` to provide cover for the supported framework range.

6. Remove the old target framework's *Major.Minor* .NET SDK version number from all GitHub Actions workflows when `actions/setup-dotnet` is invoked, now that it is no longer needed for reliable Continuous Integration.
   
   Commit:
   > Remove setup of deprecated .NET SDK from GitHub Actions workflows, now that no projects in the solution depend on the associated target framework at runtime.
</details>