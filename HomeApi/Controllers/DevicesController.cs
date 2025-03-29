using System;
using System.Threading.Tasks;
using AutoMapper;
using HomeApi.Contracts.Models.Devices;
using HomeApi.Data;
using HomeApi.Data.Models;
using HomeApi.Data.Queries;
using HomeApi.Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace HomeApi.Controllers
{
    /// <summary>
    /// Контроллер устройсив
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceRepository _devices;
        private readonly IRoomRepository _rooms;
        private readonly IMapper _mapper;
        private readonly HomeApiContext _context;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(IDeviceRepository devices, IRoomRepository rooms, IMapper mapper, HomeApiContext context, ILogger<DevicesController> logger)
        {
            _devices = devices;
            _rooms = rooms;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Просмотр списка подключенных устройств
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetDevices()
        {
            var devices = await _devices.GetDevices();

            var resp = new GetDevicesResponse
            {
                DeviceAmount = devices.Length,
                Devices = _mapper.Map<Device[], DeviceView[]>(devices)
            };

            return StatusCode(200, resp);
        }

        /// <summary>
        /// Добавление нового устройства
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Add(AddDeviceRequest request)
        {
            var room = await _rooms.GetRoomByName(request.RoomLocation);
            if (room == null)
                return StatusCode(400, $"Ошибка: Комната {request.RoomLocation} не подключена. Сначала подключите комнату!");

            var device = await _devices.GetDeviceByName(request.Name);
            if (device != null)
                return StatusCode(400, $"Ошибка: Устройство {request.Name} уже существует.");

            var newDevice = _mapper.Map<AddDeviceRequest, Device>(request);
            await _devices.SaveDevice(newDevice, room);

            return StatusCode(201, $"Устройство {request.Name} добавлено. Идентификатор: {newDevice.Id}");
        }

        /// <summary>
        /// Обновление существующего устройства
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation(Summary = "Обновление устройства", Description = "Обновляет устройство по его идентификатору.")]
        [SwaggerResponse(200, "Устройство успешно обновлено.")]
        [SwaggerResponse(400, "Неверный запрос.")]
        [SwaggerResponse(404, "Устройство не найдено.")]
        public async Task<IActionResult> UpdateDevice(Guid id, [FromBody] AddDeviceRequest updatedDevice)
        {
            _logger.LogInformation($"Запрос на обновление устройства с ID: {id}");
            if (updatedDevice == null || id == Guid.Empty)
            {
                _logger.LogWarning($"Неверный запрос на обновление устройства с ID: {id}");
                return BadRequest(new { message = "Invalid request data" });
            }

            var device = await _devices.GetDeviceById(id);
            if (device == null)
            {
                _logger.LogWarning($"Устройство с ID: {id} не найдено.");
                return NotFound(new { message = "Device not found" });
            }

            var room = await _rooms.GetRoomByName(updatedDevice.RoomLocation);
            if (room == null)
            {
                _logger.LogWarning($"Комната с именем: {updatedDevice.RoomLocation} не найдена.");
                return NotFound(new { message = "Room not found" });
            }
            string newName = null;
            string newSerial = null;
            if (updatedDevice.Name != device.Name)
            {
                newName = updatedDevice.Name;
            }
            if (updatedDevice.SerialNumber != device.SerialNumber)
            {
                newSerial = updatedDevice.SerialNumber;
            }

            var query = new UpdateDeviceQuery(newName, newSerial);

            await _devices.UpdateDevice(device, room, query);
            _logger.LogInformation($"Устройство с ID: {id} успешно обновлено.");
            return Ok(new { message = "Device updated successfully" });
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerOperation(Summary = "Удаление устройства", Description = "Удаляет устройство по его идентификатору.")]
        [SwaggerResponse(200, "Устройство успешно удалено.")]
        [SwaggerResponse(404, "Устройство не найдено.")]
        public async Task<IActionResult> DeleteDevice(Guid id)
        {
            _logger.LogInformation($"Запрос на удаление устройства с ID: {id}");

            var device = await _devices.GetDeviceById(id);

            if (device == null)
            {
                _logger.LogWarning($"Устройство с ID: {id} не найдено.");
                return NotFound();
            }

            await _devices.DeleteDevice(device);
            _logger.LogInformation($"Устройство с ID: {id} успешно удалено.");
            return Ok();
        }
    }
}
