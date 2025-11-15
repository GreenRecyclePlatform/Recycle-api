using recycle.Application.DTOs;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Services
{
    public class AddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AddressService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Address>> GetAddresses(Guid userId)
        {

            var Addresses = await _unitOfWork.Addresses.GetAll(a => a.UserId == userId);

            return Addresses.ToList();
        }

        public async Task<Address> GetAddress(Guid id, Guid userId)
        {

            var address = await _unitOfWork.Addresses.GetAsync(a => a.Id == id);

            return address;
        }

        public async Task<Address> CreateAddress(AddressDto addressDto, Guid userId)
        {

            var entity = new Address
            {
                Street = addressDto.Street,
                City = addressDto.City,
                Governorate = addressDto.Governorate,
                PostalCode = addressDto.PostalCode,
                UserId = userId
            };

            await _unitOfWork.Addresses.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity;
        }

        public async Task<AddressDto> UpdateAddress(Guid id,
            AddressDto addressdto)
        {
            var address = await _unitOfWork.Addresses.GetAsync(a => a.Id == id);
            address.Street = addressdto.Street;
            address.City = addressdto.City;
            address.Governorate = addressdto.Governorate;
            address.PostalCode = addressdto.PostalCode;

            await _unitOfWork.SaveChangesAsync();

            return addressdto;

        }

        public async Task<bool> DeleteAddress(Address address)
        {
           
            await _unitOfWork.Addresses.RemoveAsync(address);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
