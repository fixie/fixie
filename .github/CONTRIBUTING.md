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

**Honestly, you don't want to do this.** Adding or removing a target framework (eg. `net9.0`) is especially impactful and carries with it several gotchas specific to Fixie's roadmap planning, implementation, packaging, end to end testing, and end user environment support goals.

**An unanticipated PR for adding or removing a target framework will be rejected.**
