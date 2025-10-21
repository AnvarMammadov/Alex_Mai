using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Alex_Mai.Views
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl
    {
        public ChatView()
        {
            InitializeComponent();
            // Subscribe to DataContextChanged event to access the ViewModel
            DataContextChanged += ChatView_DataContextChanged;
        }

        private void ChatView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            // Unsubscribe from old ViewModel if exists
            if (e.OldValue is ViewModels.ChatViewModel oldVm && oldVm.Messages != null)
            {
                oldVm.Messages.CollectionChanged -= Messages_CollectionChanged;
            }

            // Subscribe to new ViewModel's Messages collection change
            if (e.NewValue is ViewModels.ChatViewModel newVm && newVm.Messages != null)
            {
                newVm.Messages.CollectionChanged += Messages_CollectionChanged;
                // Initial scroll to bottom if needed
                ScrollToBottom();
            }
        }

        private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // When a new message is added, scroll to the bottom
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // Ensure UI updates before scrolling
                Dispatcher.InvokeAsync(() => ScrollToBottom(), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void ScrollToBottom()
        {
            if (MessageScrollViewer != null) // Check if ScrollViewer exists
            {
                MessageScrollViewer.ScrollToBottom();
            }
        }

        // Optional: Unsubscribe when the UserControl is unloaded to prevent memory leaks
        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModels.ChatViewModel vm && vm.Messages != null)
            {
                vm.Messages.CollectionChanged -= Messages_CollectionChanged;
            }
            DataContextChanged -= ChatView_DataContextChanged; // Unsubscribe from DataContextChanged as well
        }
    }
}

