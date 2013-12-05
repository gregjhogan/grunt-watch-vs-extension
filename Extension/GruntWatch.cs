namespace activelow.GruntWatchPackage
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Process = System.Diagnostics.Process;
    using System.Text;

    internal sealed class GruntWatch
    {
        private readonly IDictionary<Project, Process> processes = new Dictionary<Project, Process>();

        public void RegisterProject(Project project)
        {
            var gruntFile = this.FindGruntFile(project.ProjectItems);

            if (gruntFile != null && HasWatchTarget(gruntFile) && !processes.ContainsKey(project))
            {
                var process = StartGruntWatch(Path.GetDirectoryName(gruntFile));
                processes.Add(project, process);
            }
        }

        public void UnregisterProject(Project project)
        {
            if (this.processes.ContainsKey(project))
            {
                var process = this.processes[project];
                StopGruntWatch(process);
                this.processes.Remove(project);
            }
        }

        private string FindGruntFile(ProjectItems items)
        {
            foreach (ProjectItem item in items)
            {
                if (item.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder || item.Kind == EnvDTE.Constants.vsProjectItemKindVirtualFolder)
                {
                    var fileName = FindGruntFile(item.ProjectItems);

                    if (fileName != null)
                    {
                        return fileName;
                    }
                }
                else if (item.Name.ToUpperInvariant() == "GRUNTFILE.JS")
                {
                    return item.FileNames[0];
                }
            }

            return null;
        }

        private static bool HasWatchTarget(string file)
        {
            // to improve this would require parsing the javascript
            // or mocking node.js and grunt and reading the config
            return File.ReadAllText(file).IndexOf("watch") != -1;
        }

        private static Process StartGruntWatch(string directory)
        {
            var process = new ProcessStartInfo("cmd.exe", "/C npm install & grunt watch");
            process.WorkingDirectory = directory;
            process.UseShellExecute = true;
            return Process.Start(process);
        }

        private static void StopGruntWatch(Process process)
        {
            try
            {
                process.CloseMainWindow();
            }
            catch { }
        }
    }
}
