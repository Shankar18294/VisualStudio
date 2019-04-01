﻿using System;
using System.Windows;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using GitHub.Services;
using GitHub.VisualStudio.UI.Services;
using GitHubCore;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitHub.VisualStudio.TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloneOrOpenRepository_Click(object sender, RoutedEventArgs e)
        {
            CloneOrOpenRepositoryAsync();
        }

        private void CreateRepository_Click(object sender, RoutedEventArgs e)
        {
            CreateRepositoryAsync();
        }

        async Task CreateRepositoryAsync()
        {
            var compositionContainer = CreateCompositionContainer();

            var dialogService = compositionContainer.GetExportedValue<IDialogService>();
            var connection = await dialogService.ShowLoginDialog();
            if(connection != null)
            {
                await dialogService.ShowCreateRepositoryDialog(connection);
            }
        }

        async Task CloneOrOpenRepositoryAsync()
        {
            var compositionContainer = CreateCompositionContainer();

            var url = null as string;
            var dialogService = compositionContainer.GetExportedValue<IDialogService>();
            var cloneDialogResult = await dialogService.ShowCloneDialog(null, url);
            if (cloneDialogResult != null)
            {
                var repositoryCloneService = compositionContainer.GetExportedValue<IRepositoryCloneService>();
                await repositoryCloneService.CloneOrOpenRepository(cloneDialogResult);
            }
        }

        CompositionContainer CreateCompositionContainer()
        {
            var compositionServices = new CompositionServices();
            var exports = CreateOutOfProcExports();
            var compositionContainer = compositionServices.CreateCompositionContainer(exports);

            var gitHubServiceProvider = compositionContainer.GetExportedValue<IGitHubServiceProvider>();
            var externalShowDialogService = new ExternalShowDialogService(gitHubServiceProvider, this);
            compositionContainer.ComposeExportedValue<IShowDialogService>(externalShowDialogService);
            return compositionContainer;
        }

        static CompositionContainer CreateOutOfProcExports()
        {
            var container = new CompositionContainer();
            var serviceProvider = new MySVsServiceProvider();
            container.ComposeExportedValue<SVsServiceProvider>(serviceProvider);
            return container;
        }

        class MySVsServiceProvider : SVsServiceProvider
        {
            public object GetService(Type serviceType)
            {
                Console.WriteLine($"GetService: {serviceType}");
                return null;
            }
        }
    }
}