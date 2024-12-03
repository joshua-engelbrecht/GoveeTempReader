using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLEGoveeTest.Services
{
    internal interface IBLEDiscoveryService
    {
        BluetoothState GetBleState();
        bool IsScanning { get; }
        bool IsConnecting { get; }

        Task StartScanningAsync(Action<IDevice> onDeviceFound, CancellationToken cancellationToken);
        Task<IDevice?> ConnectToDeviceAsync(Guid id, CancellationToken cancellationToken);
        Task DiscconnectFromDeviceAsync(IDevice connectedDevice);
        Task<IReadOnlyList<IService>> GetServicesForDeviceAsync(IDevice connectedDevice);
        Task<IReadOnlyList<ICharacteristic>> GetCharacteristicsAsync(IService connectedDeviceService);
    }
}
