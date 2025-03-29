﻿using AutoMapper;
using HomeApi.Configuration;
using HomeApi.Contracts.Models.Devices;
using HomeApi.Contracts.Models.Home;
using HomeApi.Contracts.Models.Rooms;
using HomeApi.Data.Models;

namespace HomeApi
{
    /// <summary>
    /// Настройки маппинга всех сущностей приложения
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// В конструкторе настроим соответствие сущностей при маппинге
        /// </summary>
        public MappingProfile()
        {
            CreateMap<Address, AddressInfo>();
            CreateMap<HomeOptions, InfoResponse>()
                .ForMember(m => m.AddressInfo,
                    opt => opt.MapFrom(src => src.Address));

            // Валидация запросов:
            CreateMap<AddDeviceRequest, Device>()
                .ForMember(d => d.Location,
                    opt => opt.MapFrom(r => r.RoomLocation))
                .ForMember(d => d.SerialNumber,
                    opt => opt.MapFrom(r => r.SerialNumber))
                .ForMember(d => d.CurrentVolts,
                    opt => opt.MapFrom(r => r.CurrentVolts))
                .ForMember(d => d.GasUsage,
                    opt => opt.MapFrom(r => r.GasUsage))
                .ForMember(d => d.Model,
                    opt => opt.MapFrom(r => r.Model))
                .ForMember(d => d.Manufacturer,
                    opt => opt.MapFrom(r => r.Manufacturer));
            CreateMap<AddRoomRequest, Room>();
            CreateMap<Device, DeviceView>();
        }
    }
}
