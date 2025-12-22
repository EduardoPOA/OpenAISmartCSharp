using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using Eduardo.OpenAISmartTest.Options;
using Eduardo.OpenAISmartTest.ToolWindows;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Eduardo.OpenAISmartTest
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.OpenAISmartTestString)]
    [ProvideOptionPage(typeof(OptionPageGridGeneral), "OpenAI Smart Test", "General", 0, 0, true)]
    [ProvideProfile(typeof(OptionPageGridGeneral), "OpenAI Smart Test", "General", 0, 0, true)]
    //[ProvideOptionPage(typeof(OptionPageGridCommands), "OpenAI Smart Test", "Commands", 1, 1, true)]
    //[ProvideProfile(typeof(OptionPageGridCommands), "OpenAI Smart Test", "Commands", 1, 1, true)]
    [ProvideToolWindow(typeof(TerminalWindow))]
    [ProvideToolWindow(typeof(TerminalWindowTurbo))]
    public sealed class OpenAISmartTestPackage : ToolkitPackage
    {
        /// <summary>
        /// Gets the OptionPageGridGeneral object.
        /// </summary>
        public OptionPageGridGeneral OptionsGeneral
        {
            get
            {
                return (OptionPageGridGeneral)GetDialogPage(typeof(OptionPageGridGeneral));
            }
        }

        /// <summary>
        /// Gets the OptionPageGridCommands object.
        /// </summary>
        public OptionPageGridCommands OptionsCommands
        {
            get
            {
                return (OptionPageGridCommands)GetDialogPage(typeof(OptionPageGridCommands));
            }
        }

        /// <summary>
        /// Initializes the terminal window commands.
        /// </summary>
        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            //await TerminalWindowCommand.InitializeAsync(this);
            //await TerminalWindowTurboCommand.InitializeAsync(this);
        }
    }
}