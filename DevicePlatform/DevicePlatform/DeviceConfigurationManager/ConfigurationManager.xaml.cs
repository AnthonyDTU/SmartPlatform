namespace DevicePlatform.DeviceConfigurationManager;

using DevicePlatform.BackendControllers;
using DevicePlatform.Data;
using SmartDevicePlatformPlugin;
using System.Diagnostics;
using System.IO.Ports;
using System.Text.Json;

#pragma warning disable CA1416 // Validate platform compatibility
public partial class ConfigurationManager : ContentPage
{
	private string getDeviceModelCommand = "deviceModel?";
	private string setDeviceModelCommand = "deviceModel:";

	private string getDeviceReadyForConfigCommand = "readyForConfig?";
    private string setDeviceReadyForConfigCommand = "readyForConfig!";

	private string deviceConfiguredCommand = "deviceConfigured!";

    private int baudRate = 115200;
	private int dataBits = 8;
    private Parity parity = Parity.None;
    private StopBits stopBits = StopBits.One;

    private SerialPort serialPort;
	private string deviceModel;
	private DevicePluginCollection devices;
	private Guid deviceID;
	private bool newDevice;

    SerialConfiguratorView serialConfiguratorView;
    IPlatformPlugin workingDevicePlugin;


	/// <summary>
	/// Creates a new device and starts the configuration process
	/// </summary>
	/// <param name="backendAPI"></param>
	/// <param name="devices"></param>
	public ConfigurationManager(DevicePluginCollection devices)
	{
		InitializeComponent();


		this.devices = devices;
		newDevice = true;

		serialConfiguratorView = new SerialConfiguratorView(SetupSerialConnection);
		ConfigurationView.Children.Add(serialConfiguratorView);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void SetupSerialConnectionButton_Clicked(object sender, EventArgs e)
	{
		serialConfiguratorView.GetSelectedComport();
        try
        {
            serialPort = new SerialPort(serialConfiguratorView.GetSelectedComport(),
                                        baudRate,
                                        parity,
                                        dataBits,
                                        stopBits);

            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.Open();

            if (serialPort.IsOpen)
            {
                QueryDeviceModel();
            }
        }
        catch (Exception)
        {

            throw;
        }
    }


	/// <summary>
	/// Starts the configuration process of an existing device
	/// </summary>
	/// <param name="backendAPI"></param>
	/// <param name="devices"></param>
	/// <param name="deviceID"></param>
	public ConfigurationManager(DevicePluginCollection devices, Guid deviceID)
    {
        InitializeComponent();

        this.devices = devices;
        this.deviceID = deviceID;
		newDevice = false;

        SerialConfiguratorView serialConfiguratorView = new SerialConfiguratorView(SetupSerialConnection);
        ConfigurationView.Children.Add(serialConfiguratorView);
    }

	/// <summary>
	/// Opens the serial port an queries the device for its model
	/// </summary>
	/// <param name="COMPort"></param>
    private void SetupSerialConnection(string COMPort)
	{
        try
        {
			if (serialPort != null && serialPort.IsOpen)
				return; 

            serialPort = new SerialPort(COMPort,
                                        baudRate,
                                        parity,
                                        dataBits,
                                        stopBits);

			serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.Open();

			if (serialPort.IsOpen)
			{
				QueryDeviceModel();
			}
        }
        catch (Exception)
        {

            throw;
        }
    }

	/// <summary>
	/// When data is recived on the serial port
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private async void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		string receivedData = serialPort.ReadLine();

		// If the device reports its model
		if (receivedData.StartsWith(setDeviceModelCommand))
		{
			receivedData = receivedData.Replace(setDeviceModelCommand, string.Empty);
			deviceModel = receivedData;

			CreateNewPlatformPlugin();
			SetupConfigurationView();
			return;
		}

		// If the device says it is ready for configuration
        if (receivedData.Equals(setDeviceReadyForConfigCommand))
        {
			string jsonData = "newConfig:" + JsonSerializer.Serialize<NodeConfiguration>(workingDevicePlugin.DeviceConfigurator.BuildNewConfiguration());
            serialPort.WriteLine(jsonData);
            return;
        }

		// If device acknowledges that it has been configured
		if (receivedData.Equals(deviceConfiguredCommand))
		{
			if (newDevice)
			{
				workingDevicePlugin.DeviceDescriptor.SetDeviceDescritptor(workingDevicePlugin.DeviceConfigurator.GetDeviceDescriptor());
				//ActiveUser.DevicesPlugins.AddNewDevicePlugin(workingDevicePlugin);
                await ActiveUser.apiController.AddNewDevice(workingDevicePlugin.DeviceDescriptor);
                Navigation.PopAsync();
            }
			else
			{
				// UpdateDevice
			}
		}
    }

	/// <summary>
	/// Queries the device model
	/// </summary>
	private void QueryDeviceModel()
	{
		serialPort.WriteLine(getDeviceModelCommand);
	}

	/// <summary>
	/// Creates a new platform pluging based on what model the device has reported
	/// </summary>
	private void CreateNewPlatformPlugin()
	{
        switch (deviceModel)
        {
            case "Smart Curtains":
                if (newDevice)
                    workingDevicePlugin = new SmartCurtainsPlatformPlugin.SmartCurtainsPlatformPlugin(ActiveUser.User.UserID, 
																									  ActiveUser.hubConnection, 
																									  ActiveUser.RemoveDevicePlugin);
                else
                    workingDevicePlugin = ActiveUser.DevicesPlugins.GetDevicePlugin(deviceID);
                break;

            default:
				Debug.WriteLine("Unknown device");
				break;
        }
    }

	/// <summary>
	/// Gets the configuration view from the platform plugin
	/// </summary>
	private void SetupConfigurationView()
	{
		Dispatcher.Dispatch(() =>
		{
            Button sendConfigurationToDeviceButton = new Button()
            {
                Text = "Send Configuration To Device",
                WidthRequest = 400,

            };
            sendConfigurationToDeviceButton.Clicked += SendConfigurationToDeviceButton_Clicked;

            ConfigurationView.Children.Add(workingDevicePlugin.DeviceConfigurator.GetConfigurationView());
            ConfigurationView.Children.Add(sendConfigurationToDeviceButton);
        });        
	}

	/// <summary>
	/// Prepares the device for configuration
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <exception cref="NotImplementedException"></exception>
	private void SendConfigurationToDeviceButton_Clicked(object sender, EventArgs e)
	{
		serialPort.WriteLine(getDeviceReadyForConfigCommand);
	}

	/// <summary>
	/// Deconstructor 
	/// Closes the serial-port
	/// </summary>
	~ConfigurationManager()
	{
		if (serialPort != null &&
			serialPort.IsOpen)
		{
			serialPort.Close();
			serialPort.Dispose();
		}
	}

}
#pragma warning restore CA1416 // Validate platform compatibility