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
	private Guid deviceID;
	private bool newDevice;

    private SerialConfiguratorView serialConfiguratorView;
    private IPlatformPlugin workingDevicePlugin;


	/// <summary>
	/// Creates a new device and starts the configuration process
	/// </summary>
	/// <param name="backendAPI"></param>
	/// <param name="devices"></param>
	public ConfigurationManager()
	{
		InitializeComponent();

		newDevice = true;

		serialConfiguratorView = new SerialConfiguratorView(SetupSerialConnection);
		ConfigurationView.Children.Add(serialConfiguratorView);
	}

    /// <summary>
    /// Starts the configuration process of an existing device
    /// </summary>
    /// <param name="backendAPI"></param>
    /// <param name="devices"></param>
    /// <param name="deviceID"></param>
    public ConfigurationManager(Guid deviceID)
    {
        InitializeComponent();

        this.deviceID = deviceID;
        newDevice = false;

        SerialConfiguratorView serialConfiguratorView = new SerialConfiguratorView(SetupSerialConnection);
        ConfigurationView.Children.Add(serialConfiguratorView);
    }


	/// <summary>
	/// Opens a serial connection
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
            Debug.WriteLine("Send Configuration To Device: " + jsonData);
            serialPort.WriteLine(jsonData);
            return;
        }

		// If device acknowledges that it has been configured
		if (receivedData.Equals(deviceConfiguredCommand))
		{
			if (newDevice)
			{
				workingDevicePlugin.DeviceDescriptor.SetDeviceDescritptor(workingDevicePlugin.DeviceConfigurator.GetDeviceDescriptor());
                //await ActiveUserSingleton.Instance.apiController.AddNewDevice(workingDevicePlugin.DeviceDescriptor);
				await ActiveUser.Instance.AddNewDevicePlugin(workingDevicePlugin);

                if (serialPort != null &&
					serialPort.IsOpen)
                {
                    serialPort.Close();
                    serialPort.Dispose();
                }

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
                    workingDevicePlugin = new SmartCurtainsPlatformPlugin.SmartCurtainsPlatformPlugin(ActiveUser.Instance.User.UserID,
                                                                                                      ActiveUser.Instance.hubConnection,
                                                                                                      ActiveUser.Instance.RemoveDevicePlugin);
                else
                    workingDevicePlugin = ActiveUser.Instance.DevicePlugins.GetDevicePlugin(deviceID);
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
}
#pragma warning restore CA1416 // Validate platform compatibility