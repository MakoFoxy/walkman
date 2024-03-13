using System;
using System.Diagnostics;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    AbsolutePath SelectionsLoaderBackendSourceDirectory => RootDirectory / "selections-loader" / "backend"  / "MarketRadio.SelectionsLoader";
    AbsolutePath SelectionsLoaderFrontendSourceDirectory => RootDirectory / "selections-loader" / "frontend";

    AbsolutePath SelectionsLoaderBackendOutputDirectory => SelectionsLoaderBackendSourceDirectory / "bin" / "Desktop";

    AbsolutePath SelectionsLoaderFrontendOutputDirectory => SelectionsLoaderBackendSourceDirectory / "wwwroot";
    
    Target SelectionsLoaderRestoreBase => _ => _.Executes(() =>
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
                    WorkingDirectory = SelectionsLoaderBackendSourceDirectory
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
                    WorkingDirectory = SelectionsLoaderBackendSourceDirectory
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
    
    Target SelectionsLoaderRestoreBackend => _ => _
        .DependsOn(SelectionsLoaderRestoreBase)
        .Executes(() =>
    {
        DotNetRestore(_ => _.SetProjectFile(Solution));
    });

    Target SelectionsLoaderCleanBackend => _ => _
        .DependsOn(SelectionsLoaderRestoreBackend)
        .Executes(() =>
        {
            SelectionsLoaderBackendOutputDirectory.GlobFiles("*.*").ForEach(DeleteFile);
            SelectionsLoaderBackendOutputDirectory.GlobDirectories("*").ForEach(DeleteDirectory);
            EnsureCleanDirectory(SelectionsLoaderBackendOutputDirectory);
        });

    Target SelectionsLoaderCleanFrontend => _ => _
        .DependsOn(SelectionsLoaderRestoreBackend, SelectionsLoaderRestoreBase)
        .Executes(() =>
        {
            DeleteDirectory(SelectionsLoaderFrontendOutputDirectory);
            EnsureCleanDirectory(SelectionsLoaderFrontendOutputDirectory);
        });

    Target SelectionsLoaderRestoreFrontend => _ => _
        .DependsOn(SelectionsLoaderCleanFrontend)
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
                        WorkingDirectory = SelectionsLoaderFrontendSourceDirectory
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
                        WorkingDirectory = SelectionsLoaderFrontendSourceDirectory
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

    Target CompileSelectionsLoader => _ => _
        .DependsOn(SelectionsLoaderRestoreBackend, SelectionsLoaderCompileFrontend)
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
                        WorkingDirectory = SelectionsLoaderBackendSourceDirectory
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
                        WorkingDirectory = SelectionsLoaderBackendSourceDirectory
                    }
                };
            }

            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception("SelectionLoader not compiled");
            }
        });

    Target SelectionsLoaderCompileFrontend => _ => _
        .DependsOn(SelectionsLoaderRestoreFrontend)
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
                        WorkingDirectory = SelectionsLoaderFrontendSourceDirectory
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
                        WorkingDirectory = SelectionsLoaderFrontendSourceDirectory
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