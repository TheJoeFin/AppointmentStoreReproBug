using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AppointmentStoreReproBug
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public AppointmentStore ApptStore { get; set; }

        public MainPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async Task GetAppointmentAccess()
        {
            ApptStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadWrite);
            if (ApptStore != null)
            {
                ApptStore.ChangeTracker.Enable();
                ApptStore.StoreChanged += ApptStore_StoreChanged;
                ResultsTextBox.Text += "\nGained Appointment Access";
            }
            else
            {
                // show error, unable to access appointments
                ResultsTextBox.Text += "\nFailed to get Appointment Access";
            }
        }

        private async void ApptStore_StoreChanged(AppointmentStore sender, AppointmentStoreChangedEventArgs args)
        {
            AppointmentStoreChangeTracker changeTracker = ApptStore.GetChangeTracker("");
            AppointmentStoreChangeReader changeReader = changeTracker.GetChangeReader();
            IReadOnlyList<AppointmentStoreChange> changes = await changeReader.ReadBatchAsync();

            foreach (AppointmentStoreChange chg in changes)
            {
                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    // update your UI here
                    ResultsTextBox.Text += $"\n{chg.Appointment.Subject} {chg.ChangeType}";
                });
            }

            changeReader.AcceptChanges();
        }

        private async void GetAccessButton_Click(object sender, RoutedEventArgs e)
        {
            await GetAppointmentAccess();

        }
    }
}
