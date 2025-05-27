using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using WoWSChatTranslator.ViewModels;

namespace WoWSChatTranslator.Views
{
    class WindowBehaivor: Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Closed += this.WindowClosed;
        }

        private void WindowClosed(object? sender, EventArgs e)
        {
            MainViewModel? viewModel = (this.AssociatedObject.DataContext as MainViewModel);
            viewModel?.SaveSettings();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.Closed -= this.WindowClosed;
        }
    }
}
