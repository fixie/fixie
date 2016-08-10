# Fixie's `dotnet test` Runner

## Introduction

The runner may be invoked by end users at the command line,
or by IDEs. Command line arguments determine which mode we
are in.

The presence of --port ##### indicates that an IDE is making
the request. IDEs also redundantly send --designtime when they
send --port.

1) IDE requests test discovery:
    --designtime --port ##### --list

2) IDE requests test execution:
    --designtime --port #### --wait-command

3) Console mode is assumed whenever --port is omitted. When
   --port is omitted, the following arguments would make no
   sense: --designtime, --wait-command.