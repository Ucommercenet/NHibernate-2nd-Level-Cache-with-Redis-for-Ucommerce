using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

namespace helpers
{
    public static class ProjectExtensions
    {
        public static void CopyProjectBinDir(this Project project, AbsolutePath targetDir, Configuration configuration)
        {
            FileSystemTasks.CopyDirectoryRecursively(project.GetOutputDir(configuration), targetDir);
        }

        public static AbsolutePath GetOutputDir(this Project project, Configuration configuration)
        {
            return project.Directory / "bin" / configuration  / "net48";
        }
    }
}
