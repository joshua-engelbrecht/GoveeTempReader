using BLEGoveeTest.Services;
using Microsoft.AspNetCore.Components;
using Plugin.BLE.Abstractions.Contracts;
using System.Diagnostics;

namespace BLEGoveeTest.Components.Pages
{
    public partial class Home
    {
        [Inject] private IBLEDiscoveryService BLEDiscoveryService { get; set; } = default!;

        private BluetoothState bleState = BluetoothState.Unknown;

        private List<IDevice> Devices = new List<IDevice>();
        private IDevice? ConnectedDevice { get; set; } = default!;
        private CancellationTokenSource ConnectCancellationSource { get; set; } = default!;
        private CancellationTokenSource ScanCancellationSource { get; set; } = default!;
        private bool IsScanning => BLEDiscoveryService.IsScanning;
        private bool IsConnecting => BLEDiscoveryService.IsConnecting;
        protected override void OnInitialized()
        {
            bleState = BLEDiscoveryService.GetBleState();
        }

        private async Task StartScanning()
        {
            ScanCancellationSource = new CancellationTokenSource();
            Devices.Clear();
            await InvokeAsync(StateHasChanged);
            await BLEDiscoveryService.StartScanningAsync(device =>
            {
                if (device is not null)
                {
                    if (device.Name.Contains("GVH"))
                    {
                        Devices.Add(device);
                        InvokeAsync(StateHasChanged);
                        ScanCancellationSource.Cancel();
                    }
                    Debug.WriteLine($"Device found: {device.Name}");
                }
            }, ScanCancellationSource.Token);
        }

        private async Task ConnectToDevice(Guid id)
        {
            try
            {
                ConnectCancellationSource = new CancellationTokenSource();
                ConnectedDevice = await BLEDiscoveryService.ConnectToDeviceAsync(id, ConnectCancellationSource.Token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task GetServices()
        {
            if (ConnectedDevice is not null)
            {
                try
                {
                    var services = await BLEDiscoveryService.GetServicesForDeviceAsync(ConnectedDevice);
                    foreach (IService service in services)
                    {
                        Debug.WriteLine($"Service: {service.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void StopConnectingToDevice()
        {
            ConnectCancellationSource.Cancel();
        }

        private void StopScanningForDevice()
        {
            ScanCancellationSource.Cancel();
        }

        private async Task DisconnectFromDevice()
        {
            if (ConnectedDevice is not null)
            {
                try
                {
                    await BLEDiscoveryService.DiscconnectFromDeviceAsync(ConnectedDevice);
                    ConnectedDevice = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
