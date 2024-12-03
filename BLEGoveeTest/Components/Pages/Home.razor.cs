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
        private List<IService> Services = new List<IService>();
        private IDevice? ConnectedDevice { get; set; } = default!;
        private CancellationTokenSource CancellationSource { get; set; } = default!;
        private bool IsScanning => BLEDiscoveryService.IsScanning;
        private bool IsConnecting => BLEDiscoveryService.IsConnecting;
        protected override void OnInitialized()
        {
            bleState = BLEDiscoveryService.GetBleState();
        }

        private async Task StartScanning()
        {
            CancellationSource = new CancellationTokenSource();
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
                        CancellationSource.Cancel();
                    }
                    Debug.WriteLine($"Device found: {device.Name}");
                }
            }, CancellationSource.Token);
        }

        private async Task ConnectToDevice(Guid id)
        {
            try
            {
                CancellationSource = new CancellationTokenSource();
                ConnectedDevice = await BLEDiscoveryService.ConnectToDeviceAsync(id, CancellationSource.Token);
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
                Services.Clear();
                CancellationSource = new();
                try
                {
                    var services = await BLEDiscoveryService.GetServicesForDeviceAsync(ConnectedDevice, CancellationSource.Token);
                    foreach (IService service in services)
                    {
                        Services.Add(service);

                        Debug.WriteLine($"Service: {service.Name}, ID: {service.Id}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private async Task GetCharacteristicAsync(IService service)
        {
            try
            {
                if (ConnectedDevice is not null)
                {
                    var connectedService = await ConnectedDevice.GetServiceAsync(service.Id);
                    var characteristics = await BLEDiscoveryService.GetCharacteristicsAsync(connectedService);
                    foreach (ICharacteristic characteristic in characteristics)
                    {
                        Debug.WriteLine($"Characteristic: {characteristic.Name}, ID: {characteristic.Id}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void TriggerCancellation()
        {
            CancellationSource.Cancel();
        }

        private async Task DisconnectFromDevice()
        {
            if (ConnectedDevice is not null)
            {
                try
                {
                    await BLEDiscoveryService.DisconnectFromDeviceAsync(ConnectedDevice);
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
