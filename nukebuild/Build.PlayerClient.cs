using System;
using System.Diagnostics;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    AbsolutePath ClientBackendSourceDirectory => RootDirectory / "client/MarketRadio.Player";
    AbsolutePath ClientFrontendSourceDirectory => RootDirectory / "client/MarketRadio.Player.UI";
    AbsolutePath ClientBackendOutputDirectory => RootDirectory / "client/MarketRadio.Player/bin/Desktop";
    AbsolutePath ClientFrontendOutputDirectory => RootDirectory / "client/MarketRadio.Player/wwwroot";

    Target ClientRestoreBase => _ => _.Executes(() =>
    {
        Process process;

        if (IsWin)
        {
            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c dotnet tool restore",
                    WorkingDirectory = ClientBackendSourceDirectory
                }
            };
        }
        else
        {
            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "tool restore",
                    WorkingDirectory = ClientBackendSourceDirectory
                }
            };
        }

        process.Start();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new Exception("Base dependencies not restored");
        }
    });

    Target ClientCleanBackend => _ => _
        .DependsOn(ClientRestoreBase)
        .Executes(() =>
        {
            ClientBackendOutputDirectory.GlobFiles("*.*").ForEach(DeleteFile);
            ClientBackendOutputDirectory.GlobDirectories("*").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ClientBackendOutputDirectory);
        });

    Target ClientCleanFrontend => _ => _
        .DependsOn(ClientRestoreBase)
        .Executes(() =>
        {
            DeleteDirectory(ClientFrontendOutputDirectory);
            EnsureCleanDirectory(ClientFrontendOutputDirectory);
        });

    Target ClientRestoreBackend => _ => _
        .DependsOn(ClientCleanBackend)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });
    
    Target ClientRestoreFrontend => _ => _
        .DependsOn(ClientCleanFrontend)
        .Executes(() =>
        {
            Process process;

            if (IsWin)
            {
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c yarn",
                        WorkingDirectory = ClientFrontendSourceDirectory
                    }
                };
            }
            else
            {
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "yarn",
                        WorkingDirectory = ClientFrontendSourceDirectory
                    }
                };
            }

            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception("Frontend not restored");
            }
        });

    Target CompileClient => _ => _
        .DependsOn(ClientRestoreBackend, ClientCompileFrontend)
        .Executes(() =>
        {
            Process process;

            if (IsWin)
            {
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments =
                            "/c dotnet tool run electronize build /target win /PublishReadyToRun false /PublishSingleFile false",
                        WorkingDirectory = ClientBackendSourceDirectory
                    }
                };
            }
            else
            {
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments =
                            "tool run electronize build /target win /PublishReadyToRun false /PublishSingleFile false",
                        WorkingDirectory = ClientBackendSourceDirectory
                    }
                };
            }

            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception("Client not compiled");
            }
        });

    Target ClientCompileFrontend => _ => _
        .DependsOn(ClientRestoreFrontend)
        .Executes(() =>
        {
            Process process;

            if (IsWin)
            {
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c yarn build",
                        WorkingDirectory = ClientFrontendSourceDirectory
                    }
                };
            }
            else
            {
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "yarn",
                        Arguments = "build",
                        WorkingDirectory = ClientFrontendSourceDirectory
                    }
                };
            }

            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception("Frontend not build");
            }
        });
}