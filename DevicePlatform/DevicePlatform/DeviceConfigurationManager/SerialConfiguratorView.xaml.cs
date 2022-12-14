namespace DevicePlatform.DeviceConfigurationManager;

using System.IO.Ports;

#pragma warning disable CA1416 // Validate platform compatibility
public partial class SerialConfiguratorView : ContentView
{
    public delegate void ConnectButtonPressed(string COMPort);
    public ConnectButtonPressed connectButtonPressed;

    public SerialConfiguratorView(ConnectButtonPressed callbackHandler)
    {
        connectButtonPressed = callbackHandler;
        InitializeComponent();
        COMPortPicker.ItemsSource = SerialPort.GetPortNames();
        COMPortPicker.SelectedIndex = 0;
    }

    /// <summary>
    /// Calls back to the configuration controller, to let it know a serial connection should be opened
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OpenDeviceButton_Clicked(object sender, EventArgs e)
    {
        if (connectButtonPressed != null)
            connectButtonPressed(COMPortPicker.SelectedItem as string);        
    }

    /// <summary>
    /// Updates the avaliable comports 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UpdatePorts_Clicked(object sender, EventArgs e)
    {
        string selectedPort = (string)COMPortPicker.SelectedItem;
        COMPortPicker.ItemsSource = SerialPort.GetPortNames();
        COMPortPicker.SelectedItem = selectedPort;
    }

    /// <summary>
    /// Gets the selected comport from the comport picker control
    /// </summary>
    /// <returns></returns>
    public string GetSelectedComport()
    {
        return COMPortPicker.SelectedItem as string;
    }
}
#pragma warning restore CA1416 // Validate platform compatibility