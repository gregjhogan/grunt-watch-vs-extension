namespace activelow.GruntWatchPackage
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;

    using EnvDTE;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    using Process = System.Diagnostics.Process;

    internal sealed class SolutionEvents : IVsSolutionEvents
    {
        private readonly IDictionary<string, Process> processes = new Dictionary<string, Process>();  

        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            var project = this.GetProject(pRealHierarchy);
            var directory = this.FindGruntFile(project);

            if (directory != null && !processes.ContainsKey(project.UniqueName))
            {
                var process = this.StartGruntWatch(directory);
                processes.Add(project.UniqueName, process);
            }

            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            var project = this.GetProject(pHierarchy);
            var directory = this.FindGruntFile(project);

            if (directory != null && !processes.ContainsKey(project.UniqueName))
            {
                var process = this.StartGruntWatch(directory);
                processes.Add(project.UniqueName, process);
            }

            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            var project = this.GetProject(pHierarchy);
            if (this.processes.ContainsKey(project.UniqueName))
            {
                this.processes[project.UniqueName].CloseMainWindow();
                this.processes.Remove(project.UniqueName);
            }

            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            foreach (var key in this.processes.Keys)
            {
                this.processes[key].CloseMainWindow();
            }
            this.processes.Clear();

            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            var project = this.GetProject(pRealHierarchy);
            if (this.processes.ContainsKey(project.UniqueName))
            {
                this.processes[project.UniqueName].CloseMainWindow();
                this.processes.Remove(project.UniqueName);
            }

            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        private Project GetProject(IVsHierarchy hierarchy)
        {
            object project;

            ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out project));

            return (project as Project);
        }

        private string FindGruntFile(Project project)
        {
            foreach (ProjectItem item in project.ProjectItems)
            {
                if (item.Name.ToUpperInvariant() == "GRUNTFILE.JS")
                {
                    return Path.GetDirectoryName(item.FileNames[0]);
                }
            }

            return null;
        }

        private Process StartGruntWatch(string directory)
        {
            var process = new ProcessStartInfo("cmd.exe", "/C npm install & grunt watch");
            process.WorkingDirectory = directory;
            process.UseShellExecute = true;
            return Process.Start(process);
        }
    }
}
