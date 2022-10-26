﻿using DevicePlatform.Data;
using DevicePlatform.Models;
using SmartDevicePlatformPlugin;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevicePlatform;

public partial class MainPage : ContentPage
{
	DeviceCollection deviceCollection;


    Uri uriBase = new Uri("https://localhost:7173/");
	Uri deviceUri;
    int timeoutMs = 5000;


    APIHandler apiHandler;

	string storedUser = null;
	string storedPassword = null;




	public MainPage()
	{
		InitializeComponent();
        InitBackendConnection();

        if (!LoadLocalData())
		{
			PresentLoginScreen();
		}
		else
		{
            apiHandler.Login(storedUser, storedPassword);
		}		

        SetupUI();

	}


    private void InitBackendConnection()
    {
        apiHandler = new APIHandler();
    }


	private async void SetupUI()
	{
        MainContentView.Children.Clear();

        if (ActiveUser.LoggedIn)
        {
            if (ActiveUser.Devices.Count != 0)
            {
                foreach (var device in deviceCollection.Devices)
                {
                    MainContentView.Children.Add(await device.Value.GetDeviceUI(""));
                }
            }
            else
            {
                MainContentView.Children.Add(new Label() 
                { 
                    Text = "No Devices Configured...", 
                    HorizontalOptions = LayoutOptions.Center 
                });
            }

            // On Windows:
            // Create a button for configuring a new device
            if (DeviceInfo.Current.Platform == Microsoft.Maui.Devices.DevicePlatform.WinUI)
            {
                Button addNewDeviceButton = new Button()
                {
                    WidthRequest = 250,
                    HeightRequest = 100,
                    CornerRadius = 10,
                    BackgroundColor = Colors.Cyan,
                    Text = "Configure New Device",
                    TextColor = Colors.Black,
                    FontSize = 15,


                };
                addNewDeviceButton.Clicked += AddNewDeviceButton_Clicked;
                MainContentView.Children.Add(addNewDeviceButton);
            }
        }
        else
        {
            Button goToLoginPageButton = new Button()
            {
                WidthRequest = 250,
                Text = "Login or Create User",
                FontSize = 15,
                HorizontalOptions = LayoutOptions.Center,
            };
            goToLoginPageButton.Clicked += GoToLoginButtonPage_Clicked;

            MainContentView.Children.Add(goToLoginPageButton);
            MainContentView.Children.Add(new Label() { Text = "To see and create devices", HorizontalOptions = LayoutOptions.Center });
        }      
    }

    private bool LoadLocalData()
	{
		// Pretend to try and look for stored User
		// If none is found, present login/create user screen.

		return storedUser != null;
	}


	private async Task<bool> PresentLoginScreen()
	{
		await Navigation.PushAsync(new LoginPage(apiHandler));
		return true;
	}


	private async void AddNewDeviceButton_Clicked(object sender, EventArgs e)
    {
        await apiHandler.AddNewDevice();

        await Navigation.PushAsync(new DeviceConfigurationManager.ConfigurationManager(deviceUri, ActiveUser.Devices));
        SetupUI();
    }


	private void ReRenderView()
	{

	}



    private async void GoToLoginButtonPage_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage(apiHandler));
    }

    private void ContentPage_Appearing(object sender, EventArgs e)
    {
        SetupUI();
    }
}

