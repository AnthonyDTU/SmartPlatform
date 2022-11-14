﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartPlatformBackendAPI.Data;
using SmartPlatformBackendAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SmartPlatformBackendAPI.Controllers
{
    [Route("api/{userID}/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly SmartPlatformAPIDbContext dbContext;

        public DevicesController(SmartPlatformAPIDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        // GET api/{userID}/Devices/getRequestedState?guid={deviceID}&key={deviceKey}"
        [HttpGet("getRequestedState")]
        public IActionResult Get([FromRoute] Guid userID, Guid deviceID, string deviceKey)
        {
            User? user = dbContext.Users.Find(userID);
            if (user == null || user.DeviceDescriptors == null)
                return NotFound();

            foreach (DeviceDecriptor device in user.DeviceDescriptors)
            {
                if (device.DeviceID == deviceID && device.DeviceKey == deviceKey)
                    return Ok(device);
            }
            return NotFound();
        }

        // POST api/{userID}/Devices"
        [HttpPost]
        public IActionResult Post([FromRoute] Guid userID, [FromBody] DeviceDecriptor updatedDevice)
        {
            User? user = dbContext.Users.Find(userID);
            if (user == null || user.DeviceDescriptors == null)
                return NotFound();

            int deviceIndex = -1;
            foreach (DeviceDecriptor device in user.DeviceDescriptors)
            {
                if (device.DeviceID == updatedDevice.DeviceID && device.DeviceKey == updatedDevice.DeviceKey)
                {
                    //user.Devices.Remove(device);
                    //user.Devices.
                    deviceIndex = user.DeviceDescriptors.ToList().IndexOf(device);
                }
            }

            if (deviceIndex != -1)
            {
                //user.Devices.RemoveAt(deviceIndex);
                //user.Devices.Insert(deviceIndex, updatedDevice);
                return Ok();
            }
            else
            {
                return NotFound();
            }


            
        }

        // PUT api/{userID}/Devices"
        [HttpPut]
        public IActionResult Put([FromRoute] Guid userID, [FromBody] DeviceDecriptor newDevice)
        {
            User? user = dbContext.Users.Include(d => d.DeviceDescriptors).Single(u => u.UserID == newDevice.UserID);

            if (user != null)
            {
                user.DeviceDescriptors.Add(newDevice);
                dbContext.SaveChanges();
                return Ok(user);
            }
            else
                return NotFound();
        }

        // DELETE api/{userID}/Devices/{deviceID}"
        [HttpDelete("{deviceID}")]
        public IActionResult Delete([FromRoute] Guid userID, Guid deviceID)
        {
            User? user = dbContext.Users.Include(d => d.DeviceDescriptors).Single(u => u.UserID == userID);
            if (user == null || user.DeviceDescriptors == null)
                return Unauthorized();

            foreach (DeviceDecriptor device in user.DeviceDescriptors)
            {
                if (device.DeviceID == deviceID)
                {
                    user.DeviceDescriptors.Remove(device);
                    dbContext.SaveChanges();
                    return Ok();
                }
            }
            return NotFound();
        }
    }
}
