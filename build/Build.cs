using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Xml.Linq;
using helpers;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.ReSharper;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(NoFetch = true, DisableOnUnix = true)] readonly GitVersion GitVersion;

    [Solution] readonly Solution Solution;
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
                         .Before(Restore)
                         .Executes(() =>
                                   {
                                       SourceDirectory.GlobDirectories("**/bin", "**/obj")
                                                      .ForEach(DeleteDirectory);
                                       EnsureCleanDirectory(ArtifactsDirectory);
                                   });


    Target Compile =>
        _ => _
             .DependsOn(Restore)
             .Executes(() =>
                       {
                           DotNetMSBuild(s => s
                                              .SetTargetPath(Solution)
                                              .SetTargets("Rebuild")
                                              .SetConfiguration(Configuration)
                                              .SetMaxCpuCount(Environment.ProcessorCount)
                                              .SetAssemblyVersion(GitVersion.AssemblySemVer)
                                              .SetFileVersion(GitVersion.AssemblySemFileVer)
                                              .SetInformationalVersion(GitVersion.InformationalVersion)
                                              .SetNodeReuse(IsLocalBuild));
                       });

    Target GatherFiles => _ => _
                               .Description("Gathers caching providers to a local folder for use by package and deploy targets")
                               .DependsOn(Compile)
                               .Unlisted()
                               .Executes(() =>
                                         {
                                             EnsureCleanDirectory(TempWorkDir);

                                             // Some projects have extra files that need to be copied to output apart from the project dll and the configs. 
                                             var projectExtraFilesHook =
                                                 new Dictionary<string, Action<AbsolutePath, Configuration>>
                                                 {
                                                     {
                                                         "Redis", (outputDirectory, configuration) =>
                                                                   {
                                                                       var project = Solution.GetProject("Ucommerce.Redis");
                                                                       CopyFileToDirectory(project.GetOutputDir(configuration) / "Newtonsoft.Json.dll", outputDirectory / "bin");
                                                                       CopyFileToDirectory(project.GetOutputDir(configuration) / "NHibernate.Caches.Common.dll", outputDirectory / "bin");
                                                                       CopyFileToDirectory(project.GetOutputDir(configuration) / "NHibernate.Caches.StackExchangeRedis.dll", outputDirectory / "bin");
                                                                       CopyFileToDirectory(project.GetOutputDir(configuration) / "StackExchange.Redis.dll", outputDirectory / "bin");
                                                                       CopyFileToDirectory(project.GetOutputDir(configuration) / "System.IO.Pipelines.dll", outputDirectory / "bin");
                                                                       CopyFileToDirectory(project.GetOutputDir(configuration) / "System.Memory.dll", outputDirectory / "bin");
                                                                       CopyFileToDirectory(project.GetOutputDir(configuration) / "System.Numerics.Vectors.dll", outputDirectory / "bin");
                                                                       CopyFileToDirectory(project.GetOutputDir(configuration) / "System.Runtime.CompilerServices.Unsafe.dll", outputDirectory / "bin");
                                                                       CopyFileToDirectory(project.GetOutputDir(configuration) / "System.Threading.Channels.dll", outputDirectory / "bin");
                                                                       CopyFileToDirectory(project.GetOutputDir(configuration) / "System.Threading.Tasks.Extensions.dll", outputDirectory / "bin");
                                                                       CopyFileToDirectory(project.GetOutputDir(configuration) / "Pipelines.Sockets.Unofficial.dll", outputDirectory / "bin");
                                                                       CopyFileToDirectory(RootDirectory / "libs" / "System.Buffers.4.0.2" / "System.Buffers.dll", outputDirectory / "bin");
                                                                   }
                                                     }
                                                 };

                                             Solution
                                                 .GetProjects("Ucommerce.*")
                                                 .Where(project => project.Name != "Ucommerce.*.Tests")
                                                 .ForEach(project =>
                                                          {
                                                              var outputDir = project.GetOutputDir(Configuration);
                                                              var providerName = project.Name.Split(".")
                                                                  .Last();
                                                              CopyFileToDirectory(outputDir / $"{project.Name}.dll",
                                                                  TempWorkDir / providerName / "bin");
                                                              CopyDirectoryRecursively(project.Directory / "Configuration",
                                                                  TempWorkDir / providerName / "Configuration");
                                                              if (projectExtraFilesHook.ContainsKey(providerName))
                                                              {
                                                                  projectExtraFilesHook[providerName]
                                                                      .Invoke(TempWorkDir / providerName,
                                                                              Configuration);
                                                              }
                                                          });
                                         });

    Target Lint =>
        _ => _
             .DependsOn(Compile)
             .Executes(() =>
                       {
                           ReSharperTasks.ReSharperInspectCode(s => s.SetTargetPath(Solution)
                                                                     .SetSettings(SourceDirectory
                                                                         / $"{Solution.Name}.sln.DotSettings")
                                                                     .SetOutput(TemporaryDirectory / "inspection.xml")
                                                                     .SetSeverity(ReSharperSeverity.WARNING)
                                                                     .SetNoSwea(Continue)
                                                                     .SetProcessArgumentConfigurator(a =>
                                                                         a.Add("--no-build")));

                           var issues = XDocument.Load(TemporaryDirectory / "inspection.xml")
                                                 .Element("Report")
                                                 ?.Element("Issues")
                                                 ?.Elements("Project")
                                                 .Elements("Issue")
                                                 .Where(issue => issue.Attribute("TypeId")
                                                                      ?.Value != "UnusedAutoPropertyAccessor.Global")
                                                 .Where(issue => issue.Attribute("TypeId")
                                                                      ?.Value != "InvalidXmlDocComment")
                                                 .Where(issue => issue.Attribute("TypeId")
                                                                      ?.Value != "CSharpWarnings::CS1591");

                           var count = issues?.Count() ?? int.MaxValue;
                           issues.ForEach(issue => Log.Error(issue.ToString()));
                           Assert.True(count == 0, "Linting failed");
                       });

    // ReSharper disable once UnusedMember.Local
    Target Package => _ => _
                           .Description("Packages the caching providers to the artifacts folder as cachingProviders.zip")
                           .DependsOn(GatherFiles)
                           .Executes(() =>
                                     {
                                         DeleteFile(ArtifactsDirectory / "cachingProviders.zip");
                                         CompressionTasks.CompressZip(TempWorkDir,
                                                                      ArtifactsDirectory / "cachingProviders.zip");
                                     });

    Target Restore => _ => _
                          .Executes(() =>
                                    {
                                        DotNetRestore(s => s
                                                          .SetProjectFile(Solution));
                                    });


    AbsolutePath SourceDirectory => RootDirectory / "src";

    AbsolutePath TempWorkDir => TemporaryDirectory / "dist";

    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);
}
