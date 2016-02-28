﻿using System;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using PDS.Framework;
using PDS.Witsml.Studio.ViewModels;

namespace PDS.Witsml.Studio
{
    /// <summary>
    /// Provides application level properties and functionality.
    /// </summary>
    public static class AppExtensions
    {
        /// <summary>
        /// Provides a reference the application dependecy injection container
        /// </summary>
        /// <param name="app">The reference to the application</param>
        /// <returns>The application IoC container</returns>
        public static IContainer Container(this Application app)
        {
            return ((Bootstrapper)app.Resources["bootstrapper"]).Container;
        }

        /// <summary>
        /// Provides a reference the root application shell
        /// </summary>
        /// <param name="app">The reference to the application</param>
        /// <returns>The application shell</returns>
        public static IShellViewModel Shell(this Application app)
        {
            return app.MainWindow.DataContext as IShellViewModel;
        }

        /// <summary>
        /// Provides a reference to a Caliburn WindowManager
        /// </summary>
        /// <param name="app">The reference to the application</param>
        /// <returns>A reference to a WindowManager</returns>
        public static IWindowManager WindowManager(this Application app)
        {
            return app.Container().Resolve<IWindowManager>();
        }

        /// <summary>
        /// Displays an error message using a MessageBox
        /// </summary>
        /// <param name="app">The reference to the application</param>
        /// <param name="message">The error message to be displayed</param>
        /// <param name="error">An optional Exception parameter</param>
        public static void ShowError(this Application app, string message, Exception error = null)
        {
#if DEBUG
            if (error != null)
            {
                message = string.Format("{0}{2}{2}{1}", message, error.Message, Environment.NewLine);
            }
#endif
            MessageBox.Show(Application.Current.MainWindow, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Displays a confirmation message using a MessageBox
        /// </summary>
        /// <param name="app">The reference to the application</param>
        /// <param name="message">The confirmation message to be displayed</param>
        /// <param name="buttons">The Windows MessageBoxButton type for the dialog buttons</param>
        /// <returns>Returns true of the MessageBoxResult is OK or Yes, false otherwise</returns>
        public static bool ShowConfirm(this Application app, string message, MessageBoxButton buttons = MessageBoxButton.OKCancel)
        {
            var result = MessageBox.Show(Application.Current.MainWindow, message, "Confirm", buttons, MessageBoxImage.Question);
            return (result == MessageBoxResult.OK || result == MessageBoxResult.Yes);
        }

        /// <summary>
        /// Displays an information message using a MessageBox
        /// </summary>
        /// <param name="app">The reference to the application</param>
        /// <param name="message">The information message to be displayed</param>
        public static void ShowInfo(this Application app, string message)
        {
            MessageBox.Show(Application.Current.MainWindow, message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Displays a dialog for the view represented by the viewModel
        /// </summary>
        /// <param name="app">The reference to the application</param>
        /// <param name="viewModel">The view model for the view displayed in the dialog</param>
        /// <returns>The dialog result</returns>
        public static bool ShowDialog(this Application app, object viewModel)
        {
            return app.WindowManager().ShowDialog(viewModel).GetValueOrDefault();
        }

        /// <summary>
        /// Invokes the specified action on the application's UI thread.
        /// </summary>
        /// <param name="app">The application instance.</param>
        /// <param name="action">The action.</param>
        /// <param name="priority">The priority.</param>
        public static void Invoke(this Application app, System.Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            app.Dispatcher.BeginInvoke(action, priority);
        }
    }
}
