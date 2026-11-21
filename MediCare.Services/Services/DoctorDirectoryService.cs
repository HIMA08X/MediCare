using MediCare.Data.Entities;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTOs.Doctors;
using MediCare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.Services
{
    public sealed class DoctorDirectoryService : IDoctorDirectoryService
    {
        private const int DefaultPageSize = 10;
        private const int MaximumPageSize = 50;

        private readonly IDoctorScheduleService _doctorScheduleService;
        private readonly IDoctorRepository _doctorRepository;
        private readonly ISpecializationRepository _specializationRepository;

        public DoctorDirectoryService(
            IDoctorRepository doctorRepository,
            ISpecializationRepository specializationRepository,
            IDoctorScheduleService doctorScheduleService)
        {
            _doctorRepository = doctorRepository;
            _specializationRepository = specializationRepository;
            _doctorScheduleService = doctorScheduleService;
        }

        public async Task<DoctorDirectoryPageDto> SearchAsync(
            DoctorSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(request.PageNumber, 1);

            var requestedPageSize = request.PageSize <= 0
                ? DefaultPageSize
                : request.PageSize;

            var pageSize = Math.Min(requestedPageSize, MaximumPageSize);

            if (request.AvailableOn is null)
            {
                var (doctors, totalCount) = await _doctorRepository.SearchPublicAsync(
                    request.Keyword,
                    request.City,
                    request.SpecializationId,
                    pageNumber,
                    pageSize,
                    cancellationToken);

                var items = doctors
                    .Select(doctor => MapDoctor(doctor, null))
                    .ToArray();

                return CreatePage(items, pageNumber, pageSize, totalCount);
            }

            var candidates = await _doctorRepository.SearchPublicCandidatesAsync(
                request.Keyword,
                request.City,
                request.SpecializationId,
                cancellationToken);

            var availableDoctors = new List<(DoctorProfile Doctor, DateTime NextSlotUtc)>();

            foreach (var doctor in candidates)
            {
                var slotsResult = await _doctorScheduleService.GetAvailableSlotsAsync(
                    doctor.Id,
                    request.AvailableOn.Value,
                    cancellationToken);

                if (!slotsResult.Succeeded)
                {
                    continue;
                }

                var firstSlot = slotsResult.Data?
                    .OrderBy(slot => slot.StartUtc)
                    .FirstOrDefault();

                if (firstSlot is not null)
                {
                    availableDoctors.Add((doctor, firstSlot.StartUtc));
                }
            }

            var availableCount = availableDoctors.Count;

            var availableItems = availableDoctors
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(item => MapDoctor(item.Doctor, item.NextSlotUtc))
                .ToArray();

            return CreatePage(
                availableItems,
                pageNumber,
                pageSize,
                availableCount);
        }

        private static DoctorCardDto MapDoctor(
            DoctorProfile doctor,
            DateTime? nextAvailableStartUtc)
        {
            return new DoctorCardDto
            {
                DoctorProfileId = doctor.Id,
                FullName = doctor.User.FullName,
                City = doctor.City,
                Bio = doctor.Bio,
                ConsultationFee = doctor.ConsultationFee,
                SlotMinutes = doctor.SlotMinutes,
                NextAvailableStartUtc = nextAvailableStartUtc,
                Specializations = doctor.Specializations
                    .Where(link => link.Specialization.IsActive)
                    .Select(link => link.Specialization.Name)
                    .OrderBy(name => name)
                    .ToArray()
            };
        }

        private static DoctorDirectoryPageDto CreatePage(
            IReadOnlyList<DoctorCardDto> items,
            int pageNumber,
            int pageSize,
            int totalCount)
        {
            return new DoctorDirectoryPageDto
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalCount == 0
                    ? 0
                    : (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<DoctorCardDto?> GetDetailsAsync(
            Guid doctorProfileId,
            CancellationToken cancellationToken = default)
        {
            var doctor = await _doctorRepository.GetPublicByIdAsync(
                doctorProfileId,
                cancellationToken);

            if (doctor is null)
            {
                return null;
            }

            return new DoctorCardDto
            {
                DoctorProfileId = doctor.Id,
                FullName = doctor.User.FullName,
                City = doctor.City,
                Bio = doctor.Bio,
                ConsultationFee = doctor.ConsultationFee,
                SlotMinutes = doctor.SlotMinutes,
                Specializations = doctor.Specializations
                    .Where(link => link.Specialization.IsActive)
                    .Select(link => link.Specialization.Name)
                    .OrderBy(name => name)
                    .ToArray()
            };
        }

        public async Task<IReadOnlyList<SpecializationDto>> GetSpecializationsAsync(
            CancellationToken cancellationToken = default)
        {
            var specializations = await _specializationRepository.GetActiveAsync(
                cancellationToken);

            return specializations
                .Select(specialization => new SpecializationDto
                {
                    Id = specialization.Id,
                    Name = specialization.Name,
                    Description = specialization.Description
                })
                .ToArray();
        }
    }
}
