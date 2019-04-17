using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhotoApp.Data;
using PhotoApp.Models;

namespace PhotoApp.Controllers
{
    public class PhotoController : Controller
    {
        private readonly IPhotoRepository photoRepository;
        private readonly IHostingEnvironment hostingEnvironment;

        public PhotoController(IPhotoRepository photoRepository, IHostingEnvironment hostingEnvironment)
        {
            this.photoRepository = photoRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var ownerId = GetOwnerId();
            var photoEntities = await photoRepository.GetPhotosAsync(ownerId);
            var photos = Mapper.Map<IEnumerable<Photo>>(photoEntities);

            var model = new PhotoIndexModel(photos.ToList());
            return View(model);
        }

        public async Task<IActionResult> GetPhoto(Guid id)
        {
            var photoEntity = await photoRepository.GetPhotoAsync(id);
            if (photoEntity == null)
                return NotFound();

            var photo = Mapper.Map<Photo>(photoEntity);

            var model = new GetPhotoModel(photo);
            return View(model);
        }

        public async Task<IActionResult> EditPhoto(Guid id)
        {
            var photo = await photoRepository.GetPhotoAsync(id);
            if (photo == null)
                return NotFound();

            var viewModel = new EditPhotoModel
            {
                Id = photo.Id,
                Title = photo.Title
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPhoto(EditPhotoModel editPhotoModel)
        {
            if (editPhotoModel == null || !ModelState.IsValid)
                return View();

            var photoEntity = await photoRepository.GetPhotoAsync(editPhotoModel.Id);
            if (photoEntity == null)
                return NotFound();

            Mapper.Map(editPhotoModel, photoEntity);

            await photoRepository.UpdatePhotoAsync(photoEntity);
            if (!await photoRepository.SaveAsync())
                throw new Exception($"Updating photo with {editPhotoModel.Id} failed on save.");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeletePhoto(Guid id)
        {
            var photoEntity = await photoRepository.GetPhotoAsync(id);
            if (photoEntity == null)
                return NotFound();

            await photoRepository.DeletePhotoAsync(photoEntity);

            if (!await photoRepository.SaveAsync())
                throw new Exception($"Deleting photo with {id} failed on save.");

            return RedirectToAction("Index");
        }

        public IActionResult AddPhoto()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoto(AddPhotoModel addPhotoModel)
        {
            if (addPhotoModel == null || !ModelState.IsValid)
                return View();

            var file = addPhotoModel.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
                return View();

            var fileName = SavePhotoFile(file);
            var photoEntity = Mapper.Map<PhotoEntity>(addPhotoModel);
            photoEntity.FileName = fileName;
            var ownerId = GetOwnerId();
            photoEntity.OwnerId = ownerId;

            await photoRepository.AddPhotoAsync(photoEntity);
            if (!await photoRepository.SaveAsync())
                throw new Exception($"Adding a photo failed on save.");

            return RedirectToAction("Index");
        }

        private string SavePhotoFile(IFormFile file)
        {
            byte[] photoBytes;
            using (var fileStream = file.OpenReadStream())
            using (var memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                photoBytes = memoryStream.ToArray();
            }

            var webRootPath = hostingEnvironment.WebRootPath;
            var fileName = Guid.NewGuid() + ".jpg";
            var filePath = Path.Combine($"{webRootPath}/photos/{fileName}");
            System.IO.File.WriteAllBytes(filePath, photoBytes);
            return fileName;
        }

        private string GetOwnerId()
        {
            return "a83b72ed-3f99-44b5-aa32-f9d03e7eb1fd";
        }
    }
}
