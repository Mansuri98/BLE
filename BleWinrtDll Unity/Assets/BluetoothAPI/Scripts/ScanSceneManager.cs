using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArduinoBluetoothAPI;
using System;
using System.Text;
using UnityEngine.Android;
using UnityEngine.UI;

public class ScanSceneManager : MonoBehaviour
{
    BluetoothHelper bluetoothHelper;
    string deviceName = "Cube"; // Device name to connect to
    private BluetoothHelperService service;
    private BluetoothHelperCharacteristic characteristic;
    private int lastDiceNumber = -1; // Variable to store the last dice number

    
    // Define the UUIDs for Service and Characteristic
    const string ServiceUUID = "4d7d1101-ee27-40b2-836c-17505c1044e7";
    const string CharacteristicUUID = "4d7d1106-ee27-40b2-836c-17505c1044e8";

    public Text text;
    public GameObject sphere;
    string received_message;

    void Start()
    {
#if UNITY_ANDROID
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
        callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
        callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;

        if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN") || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADVERTISE") || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
            Permission.RequestUserPermissions(new string[] { "android.permission.BLUETOOTH_SCAN", "android.permission.BLUETOOTH_ADVERTISE", "android.permission.BLUETOOTH_CONNECT" }, callbacks);
#else
    setupBluetooth();
#endif
        
        Debug.Log("Start method initiated");
        try
        {
            Debug.Log("Setting Bluetooth characteristics");

            BluetoothHelper.BLE_AS_CLIENT = true;
            BluetoothHelper.BLE = true;
            BluetoothHelper.ASYNC_EVENTS = true;
            bluetoothHelper = BluetoothHelper.GetInstance();
            bluetoothHelper.OnConnected += OnConnected;
            bluetoothHelper.OnConnectionFailed += OnConnectionFailed;
            bluetoothHelper.OnDataReceived += OnMessageReceived;
            bluetoothHelper.OnScanEnded += OnScanEnded;
            bluetoothHelper.OnServiceNotFound += OnServiceNotFound;
            bluetoothHelper.OnCharacteristicNotFound += OnCharacteristicNotFound;
            bluetoothHelper.OnCharacteristicChanged += OnCharacteristicChanged;

            Debug.Log($"Device Name set to: {deviceName}");
            Debug.Log($"Service UUID: {ServiceUUID}");
            Debug.Log($"Characteristic UUID: {CharacteristicUUID}");

            // Set custom UUIDs for BLE
            characteristic = new BluetoothHelperCharacteristic(CharacteristicUUID);
            service = new BluetoothHelperService(ServiceUUID);
            service.addCharacteristic(characteristic);
            characteristic.setService(ServiceUUID);
            // bluetoothHelper.setTxCharacteristic(characteristic);
            // bluetoothHelper.setRxCharacteristic(characteristic);
            // bluetoothHelper.setTerminatorBasedStream("\n"); 

            Debug.Log("Checking if scanning nearby devices is necessary");
            if (!bluetoothHelper.ScanNearbyDevices())
            {
                sphere.GetComponent<Renderer>().material.color = Color.black;
                bluetoothHelper.setDeviceName(deviceName);
                bluetoothHelper.Connect();
            }
            else
            {
                Debug.Log("Started scanning nearby devices");
                text.text = "start scan";
            }
        }
        catch (BluetoothHelper.BlueToothNotEnabledException ex)
        {
            sphere.GetComponent<Renderer>().material.color = Color.yellow;
            Debug.Log(ex.ToString());
            text.text = ex.Message;
        }
        Debug.Log("Start method completed");

    }

    private void OnCharacteristicChanged(BluetoothHelper helper, byte[] value, BluetoothHelperCharacteristic bluetoothHelperCharacteristic)
    {
        Debug.Log($"characteristic was changed and method is invoked {value}+{bluetoothHelperCharacteristic}");

        if (value != null && value.Length > 0)
        {
            char receivedChar = Convert.ToChar(value[0]);
            Debug.Log($"Received ASCII value:" + value[0]);
            Debug.Log($"charchter:" + receivedChar);

            if (char.IsDigit(receivedChar))
            {
                lastDiceNumber = int.Parse(receivedChar.ToString());
               // Debug.Log($"Parsed dice number: {lastDiceNumber}");
                Debug.Log($"Parsed dice number:" + lastDiceNumber);

            }
        }
    }


    private void OnCharacteristicNotFound(BluetoothHelper helper, string s, string characteristic1)
    {
        Debug.Log($"characteristic1 was not found and method is invoked {s}+{characteristic1}");
    }

    private void OnServiceNotFound(BluetoothHelper helper, string s)
    {
        Debug.Log($"Service was not found and method is invoked {s}");
    }
    void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
    }

    void PermissionCallbacks_PermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
    }

    void PermissionCallbacks_PermissionGranted(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");

        setupBluetooth();
    }
       void setupBluetooth()
    {
        try
        {
            if (bluetoothHelper != null)
                return;

            Debug.Log("HI");

            BluetoothHelper.BLE = true;  //use Bluetooth Low Energy Technology
            bluetoothHelper = BluetoothHelper.GetInstance();
            bluetoothHelper.OnConnected += (helper) =>
            {
                List<BluetoothHelperService> services = helper.getGattServices();
                foreach (BluetoothHelperService s in services)
                {
                    Debug.Log("Service : " + s.getName());
                    foreach (BluetoothHelperCharacteristic item in s.getCharacteristics())
                    {
                        Debug.Log(item.getName());
                    }
                }

                Debug.Log("Connected");
                BluetoothHelperCharacteristic c = new BluetoothHelperCharacteristic("ffe1");
                c.setService("ffe0");
                bluetoothHelper.Subscribe(c);
                //sendData();
            };
            bluetoothHelper.OnConnectionFailed += (helper) =>
            {
                Debug.Log("Connection failed");
            };
            bluetoothHelper.OnScanEnded += OnScanEnded;
            bluetoothHelper.OnServiceNotFound += (helper, serviceName) =>
            {
                Debug.Log(serviceName);
            };
            bluetoothHelper.OnCharacteristicNotFound += (helper, serviceName, characteristicName) =>
            {
                Debug.Log(characteristicName);
            };
            bluetoothHelper.OnCharacteristicChanged += (helper, value, characteristic) =>
            {
                Debug.Log(characteristic.getName());
                Debug.Log(value[0]);
            };

            // BluetoothHelperService service = new BluetoothHelperService("FFE0");
            // service.addCharacteristic(new BluetoothHelperCharacteristic("FFE1"));
            // BluetoothHelperService service2 = new BluetoothHelperService("180A");
            // service.addCharacteristic(new BluetoothHelperCharacteristic("2A24"));
            // bluetoothHelper.Subscribe(service);
            // bluetoothHelper.Subscribe(service2);
            // bluetoothHelper.ScanNearbyDevices();

            // BluetoothHelperService service = new BluetoothHelperService("19B10000-E8F2-537E-4F6C-D104768A1214");
            // service.addCharacteristic(new BluetoothHelperCharacteristic("19B10001-E8F2-537E-4F6C-D104768A1214"));
            //BluetoothHelperService service2 = new BluetoothHelperService("180A");
            //service.addCharacteristic(new BluetoothHelperCharacteristic("2A24"));
            // bluetoothHelper.Subscribe(service);
            //bluetoothHelper.Subscribe(service2);
            bluetoothHelper.ScanNearbyDevices();

        }
        catch (Exception ex)
        {
            Debug.Log(ex.StackTrace);
        }
    }

    IEnumerator blinkSphere()
    {
        sphere.GetComponent<Renderer>().material.color = Color.cyan;
        yield return new WaitForSeconds(0.5f);
        sphere.GetComponent<Renderer>().material.color = Color.green;
    }

  /*  void OnMessageReceived(BluetoothHelper helper)
    {
        received_message = helper.Read();
        text.text = received_message;
        Debug.Log(System.DateTime.Now.Second);
    }*/

  private List<string> nearbyDeviceNames = new List<string>();

  private bool cubeDeviceFound = false;

  void OnScanEnded(BluetoothHelper helper, LinkedList<BluetoothDevice> nearbyDevices)
  {
      if (!cubeDeviceFound)
      {
          foreach (BluetoothDevice device in nearbyDevices)
          {
              Debug.Log("Found device: " + device.DeviceName);
              if (device.DeviceName == deviceName)
              {
                  Debug.Log($"FOUND Cube!! Device Name: {device.DeviceName}");
                  bluetoothHelper.setDeviceName(deviceName);
                  bluetoothHelper.Connect();
                  Debug.Log($"connected to the Cube!! Device Name: {device.DeviceName}");
                  cubeDeviceFound = true;
                  text.text = "Found Cube, connecting..."; // Update text when Cube is found
                  break;
              }
          }

          if (!cubeDeviceFound)
          {
              Debug.Log("Cube not found. Continuing to scan...");
              text.text = "Cube not found, scanning..."; // Update text if Cube is not found
              helper.ScanNearbyDevices();
          }
      }
  }


  void Update()
  {
      if (!bluetoothHelper.IsBluetoothEnabled())
      {
          bluetoothHelper.EnableBluetooth(true);
      }

      // Call the method to handle the dice number update
      if (lastDiceNumber != -1) 
      {
          Debug.Log("dice value is " + lastDiceNumber); // Log the dice value before handling the update
          HandleDiceNumberUpdate();
      }
  }

// Method to handle dice number updates
  private void HandleDiceNumberUpdate()
  {
      // Update the UI or game objects based on the lastDiceNumber
      VisualizeDiceNumber(lastDiceNumber);

      // Reset the lastDiceNumber after handling
      lastDiceNumber = -1;
  }
    void OnConnected(BluetoothHelper helper)
    {
        //sphere.GetComponent<Renderer>().material.color = Color.green;
        //text.text = "Connected to Cube"; 
        try
        {
            foreach (var services in helper.getGattServices())
            {
                Debug.Log($"information about service{services.getName()}");

                if (services.getName().Contains(ServiceUUID))
                {
                    Debug.Log($"our service is found!");
                    foreach (var characteristics in services.getCharacteristics())
                    {
                        Debug.Log($"information about service{characteristics.getName()}");

                        if (characteristics.getName().Contains(CharacteristicUUID))
                        {
                            Debug.Log($"our characteristics is found!");
                            bluetoothHelper.Subscribe(characteristics);
                            helper.ReadCharacteristic(characteristics);


                        }

                    }
                    

                }

            }
            
            
            // characteristic = new BluetoothHelperCharacteristic(CharacteristicUUID);
            // service = new BluetoothHelperService(ServiceUUID);
            // bluetoothHelper.Subscribe(service);
            // Debug.Log($"subscribed to cube service {service}");
            // bluetoothHelper.Subscribe(characteristic);
            // Debug.Log($"subscribed to cube characteristic {characteristic}");
            //
            //helper.StartListening();
            // if (characteristic != null)
            // {
            //     //helper.ReadCharacteristic(characteristic);
            //     Debug.Log("characteristic is not null");
            //
            // }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        Debug.Log("OnConnected is Invoked");
      //  OnMessageReceived(helper);
       // Debug.Log("OnMessageReceived is getting called");

    }
    void OnConnectionFailed(BluetoothHelper helper)
    {
     //   sphere.GetComponent<Renderer>().material.color = Color.red;
        Debug.Log("Connection Failed");
    }
    // void OnMessageReceived(BluetoothHelper helper)
    // {
    //     // Assuming the dice sends a single byte representing the number
    //     byte[] data = helper.ReadBytes();
    //     if (data != null && data.Length > 0)
    //     {
    //         // Convert the first byte to an integer
    //         int diceNumber = data[0];
    //         received_message = diceNumber.ToString();
    //         Debug.Log("Dice Number: " + diceNumber);
    //     }
    // }
    
    // void OnMessageReceived(BluetoothHelper helper)
    // {
    //     // Assuming the dice sends a single byte representing the number
    //     byte[] data = helper.ReadBytes();
    //     if (data != null && data.Length > 0)
    //     {
    //         int diceNumber = data[0]; // Convert the first byte to an integer
    //         received_message = diceNumber.ToString();
    //         Debug.Log("Dice Number: " + diceNumber);
    //
    //         // Call a method to visualize the dice number
    //         VisualizeDiceNumber(diceNumber);
    //     }
    // }

    void OnMessageReceived(BluetoothHelper helper)
    {
        Debug.Log("OnMessageReceived is Invoked");

        // Receive and process incoming data
        byte[] data = helper.ReadBytes();
        Debug.Log($"data is being read + {data}");
        if (data != null && data.Length > 0)
        {
            Debug.Log($"data is not null");

            int diceNumber = data[0]; // Convert the first byte to an integer
            received_message = diceNumber.ToString();
            Debug.Log("Dice Number: " + diceNumber);
            string response = "Received dice number: " + diceNumber;
            SendData(data);
            VisualizeDiceNumber(diceNumber);
        }
        Debug.Log($"data is null");

    }

    void SendData(byte[] data)
    {
        byte[] payload = (data);
        bluetoothHelper.SendData(payload);
        Debug.Log("Sent response: " + data);
    }
    
    void VisualizeDiceNumber(int number)
    {
        if (text != null)
        {
            text.text = "Dice rolled: " + number;
        }
        //Change sphere color
        if (sphere != null)
        {
            Color newColor = Color.white; 
            switch (number)
            {
                case 1: newColor = Color.red; break;
                case 2: newColor = Color.green; break;
                case 3: newColor = Color.blue; break;
                case 4: newColor = Color.yellow; break;
                case 5: newColor = Color.magenta; break;
                case 6: newColor = Color.cyan; break;
            }
            sphere.GetComponent<Renderer>().material.color = newColor;
        }
    }

    void OnGUI()
    {
        if (bluetoothHelper != null)
            bluetoothHelper.DrawGUI();

        if (bluetoothHelper.isConnected())
        {
            // Disconnect button
            if (GUI.Button(new Rect(Screen.width / 2 - Screen.width / 10, Screen.height - 2 * Screen.height / 10, Screen.width / 5, Screen.height / 10), "Disconnect"))
            {
                bluetoothHelper.Disconnect();
                sphere.GetComponent<Renderer>().material.color = Color.blue;
            }

            // Display the received message
            if (!string.IsNullOrEmpty(received_message))
            {
                GUI.Label(new Rect(Screen.width / 2 - Screen.width / 10, Screen.height / 10, Screen.width / 5, Screen.height / 10), "Received: " + received_message);
            }
            else
            {
                GUI.Label(new Rect(Screen.width / 2 - Screen.width / 10, Screen.height / 10, Screen.width / 5, Screen.height / 10), "No message received");
            }

            // Display the list of nearby devices
            DisplayNearbyDevicesList();
        }
    }

    void DisplayNearbyDevicesList()
    {
        GUI.Label(new Rect(10, 150, 200, 20), "Nearby Devices:");

        for (int i = 0; i < nearbyDeviceNames.Count; i++)
        {
            GUI.Label(new Rect(20, 170 + i * 20, 200, 20), nearbyDeviceNames[i]);
        }
    }
    void OnDestroy()
    {
        if (bluetoothHelper != null)
            bluetoothHelper.Disconnect();
    }
}
