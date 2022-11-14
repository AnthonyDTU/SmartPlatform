﻿using DevicePlatform.Data;
using DevicePlatform.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SmartDevicePlatformPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DevicePlatform.BackendControllers
{
    public class APIController
    {
        HttpClient backendAPI;
        Uri uriBase = new Uri("https://localhost:7173/");

        /// <summary>
        /// 
        /// </summary>
        public APIController()
        {
            InitBackendConnection();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitBackendConnection()
        {
            backendAPI = new HttpClient();
            backendAPI.BaseAddress = uriBase;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<HttpStatusCode> Login(string username, string password)
        {
            try
            {
                var result = await backendAPI.GetAsync($"api/Users/loginAttempt?username={username}&pass={password}".Replace(" ", "%20"));
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    User user = await result.Content.ReadFromJsonAsync<User>();
                    await ActiveUser.ConfigureActiveUser(user);
                }
                else if (result.StatusCode == HttpStatusCode.NotFound)
                {
                    // Non existing user
                }
                else
                {
                    // Other
                }

                return result.StatusCode;

            }
            catch (Exception ex)
            {
                return HttpStatusCode.Ambiguous;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<HttpStatusCode> CreateNewUser(string username, string password)
        {
            NewUser newUser = new NewUser()
            {
                UserName = username,
                Password = password
            };

            try
            {

                var result = await backendAPI.PutAsJsonAsync("api/Users", newUser);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    User user = await result.Content.ReadFromJsonAsync<User>();
                    await ActiveUser.ConfigureActiveUser(user);
                }
                else if (result.StatusCode == HttpStatusCode.Conflict)
                {

                }
                else
                {
                    // Other error
                }
                return result.StatusCode;
            }
            catch (Exception)
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<HttpStatusCode> UpdateUser()
        {
            if (ActiveUser.LoggedIn == false)
            {
                return HttpStatusCode.PreconditionFailed;
            }

            try
            {
                var result = await backendAPI.PostAsJsonAsync("api/Users", ActiveUser.User);
                if (result.StatusCode == HttpStatusCode.OK)
                {

                }
                else if (result.StatusCode == HttpStatusCode.NotFound)
                {

                }
                return result.StatusCode;

            }
            catch (Exception)
            {
                return HttpStatusCode.BadRequest;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<HttpStatusCode> AddNewDevice(DeviceDescriptor deviceDescriptor)
        {
            string uri = $"api/Users";

            try
            {
                var result = await backendAPI.PostAsJsonAsync(uri, deviceDescriptor);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    User user = await result.Content.ReadFromJsonAsync<User>();
                    ActiveUser.UpdateActiveUser(user);
                }

                return result.StatusCode;
            }
            catch (Exception)
            {
                return HttpStatusCode.BadRequest;
            }
        }

        public async Task<HttpStatusCode> DeleteUser()
        {
            throw new NotImplementedException();
        }

        public async Task<HttpStatusCode> DeleteDevice(Guid userID, Guid deviceID)
        {
            string uri = $"api/{userID}/Devices/{deviceID}";

            try
            {
                var result = await backendAPI.DeleteAsync(uri);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    User user = await result.Content.ReadFromJsonAsync<User>();
                    ActiveUser.UpdateActiveUser(user);
                }

                return result.StatusCode;
            }
            catch (Exception)
            {
                return HttpStatusCode.BadRequest;
            }
        }



    }
}
