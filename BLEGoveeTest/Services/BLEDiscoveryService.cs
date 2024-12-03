using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Extensions;
using System.Diagnostics;

namespace BLEGoveeTest.Services
{
    public class BLEDiscoveryService : IBLEDiscoveryService
    {
        private IBluetoothLE ble = CrossBluetoothLE.Current;
        private IAdapter adapter = CrossBluetoothLE.Current.Adapter;

        private bool isScanning = false;
        private bool isConnecting = false;

        public BluetoothState GetBleState()
        {
            return ble.State;
        }

        public bool IsScanning => isScanning;

        public bool IsConnecting => isConnecting;

        public async Task StartScanningAsync(Action<IDevice> onDeviceFound, CancellationToken cancellationToken)
        {
            try
            {
                isScanning = true;
                adapter.DeviceDiscovered += (s, a) =>
                {
                    if (a.Device is not null)
                    {
                        onDeviceFound.Invoke(a.Device);
                    }
                };
                await adapter.StartScanningForDevicesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                isScanning = false;
            }
        }

        public async Task<IDevice?> ConnectToDeviceAsync(Guid id, CancellationToken cancellationToken)
        {
            IDevice? device = null;
            try
            {
                isConnecting = true;
                device = await adapter.ConnectToKnownDeviceAsync(id, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                isConnecting = false;
            }

            return device;
        }

        public async Task DiscconnectFromDeviceAsync(IDevice connectedDevice)
        {
            try
            {
                await adapter.DisconnectDeviceAsync(connectedDevice);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task<IReadOnlyList<IService>> GetServicesForDeviceAsync(IDevice connectedDevice)
        {
            return await connectedDevice.GetServicesAsync();
        }

        public async Task<IReadOnlyList<ICharacteristic>> GetCharacteristicsAsync(IService connectedDeviceService)
        {
            return await connectedDeviceService.GetCharacteristicsAsync();
        }
    }
}
