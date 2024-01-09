using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo1 : MonoBehaviour
{
    string deviceName = "Cube"; // Device name to connect to
    public bool isScanningDevices = false;
    public Text deviceScanButtonText;
    public Text deviceScanStatusText;
    public GameObject deviceScanResultProto;
    public Button serviceScanButton;
    public Text serviceScanStatusText;
    public Dropdown serviceDropdown;
    public Button characteristicScanButton;
    public Text characteristicScanStatusText;
    public Dropdown characteristicDropdown;
    public Button subscribeButton;
    public Text subcribeText;
    public Button writeButton;
    public InputField writeInput;
    public Text errorText;

    Transform scanResultRoot;
    string selectedDeviceId;
    string selectedServiceId = "{4d7d1101-ee27-40b2-836c-17505c1044e7}";
    string selectedCharacteristicId = "{4d7d1106-ee27-40b2-836c-17505c1044e8}";
    bool isSubscribed = false;
    private Queue<Action> mainThreadActions = new Queue<Action>();

    void Start()
    {
        Debug.Log("Start called");
        try
        {
            scanResultRoot = deviceScanResultProto.transform.parent;
            deviceScanResultProto.transform.SetParent(null);
            StartDeviceScan();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in Start: " + ex.Message);
            errorText.text = ex.Message;
        }
    }

    public void StartDeviceScan()
    {
        Debug.Log("StartDeviceScan called");
        isScanningDevices = true;
        deviceScanButtonText.text = "Stop Scan";
        deviceScanStatusText.text = "Scanning...";
        string[] serviceUUIDs = { "{4d7d1101-ee27-40b2-836c-17505c1044e7}"}; 

        BluetoothLEHardwareInterface.Initialize(true, false, 
            () => 
            {
                BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(serviceUUIDs, (address, name) => 
                {
                    if (name == deviceName)
                    {
                        ConnectToDevice(address);
                        BluetoothLEHardwareInterface.StopScan();
                    }
                });
            }, 
            (error) => 
            {
                Debug.LogError("BLE Error: " + error);
            });
    }

    public void ConnectToDevice(string address)
    {
        BluetoothLEHardwareInterface.ConnectToPeripheral(address, 
            (peripheralName) => 
            {
                Debug.Log("Connected to " + peripheralName);
                selectedDeviceId = address;
                // Directly subscribe to the characteristic
                SubscribeCharacteristic(selectedServiceId, selectedCharacteristicId);
            }, 
            null, // Service discovery not explicitly required
            null, // Characteristic discovery not explicitly required
            (disconnectAddress) => 
            {
                Debug.Log("Disconnected from " + disconnectAddress);
            });
    }


 
    public void SubscribeCharacteristic(string service, string characteristic)
    {
        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(selectedDeviceId, service, characteristic, 
            (deviceAddress, notification) => 
            {
                Debug.Log("Subscribed to " + characteristic);
            }, 
            (deviceAddress, notifiedCharacteristic, data) => 
            {
                Debug.Log("Received data from " + notifiedCharacteristic);
                // Process data here
            });
    }

    public void WriteCharacteristic(string data)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
        BluetoothLEHardwareInterface.WriteCharacteristic(selectedDeviceId, selectedServiceId, selectedCharacteristicId, bytes, bytes.Length, true, (characteristicUUID) => 
        {
            Debug.Log("Wrote to " + characteristicUUID);
        });
    }

    void Update()
    {
        while (mainThreadActions.Count > 0)
        {
            Action action = mainThreadActions.Dequeue();
            action.Invoke();
        }
    }

    void OnDestroy()
    {
        BluetoothLEHardwareInterface.DeInitialize(() => 
        {
            Debug.Log("BLE Deinitialized");
        });
    }
}
