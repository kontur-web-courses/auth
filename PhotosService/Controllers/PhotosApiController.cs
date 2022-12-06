using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotosService.Data;
using PhotosService.Models;

namespace PhotosService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/photos")]
    public class PhotosApiController : Controller
    {
        private readonly IPhotosRepository photosRepository;
        private readonly IMapper mapper;

        public PhotosApiController(IPhotosRepository photosRepository, IMapper mapper)
        {
            this.photosRepository = photosRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetPhotos(string ownerId)
        {
            var photoEntities = (await photosRepository.GetPhotosAsync(ownerId)).ToList();
            var photos = mapper.Map<List<PhotoDto>>(photoEntities);
            foreach(var photo in photos)
                photo.Url = GeneratePhotoUrl(photo);
            return Ok(photos.ToList());
        }

        [HttpGet("{id}/meta")]
        public async Task<IActionResult> GetPhotoMeta(Guid id)
        {
            var photoEntity = await photosRepository.GetPhotoMetaAsync(id);
            if (photoEntity == null)
                return NotFound();

            var photo = mapper.Map<PhotoDto>(photoEntity);
            photo.Url = GeneratePhotoUrl(photo);
            return Ok(photo);
        }

        [HttpGet("{id}/content")]
        public async Task<IActionResult> GetPhotoContent(Guid id)
        {
            var photoContent = await photosRepository.GetPhotoContentAsync(id);
            if (photoContent == null)
                return NotFound();

            return File(photoContent.Content, photoContent.ContentType, photoContent.FileName);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoto(PhotoToAddDto photo)
        {
            var content = Convert.FromBase64String(photo.Base64Content);
            var result = await photosRepository.AddPhotoAsync(photo.Title, photo.OwnerId, content);
            if (!result)
                return Conflict();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePhoto(Guid id, PhotoToUpdateDto photo)
        {
            var photoEntity = await photosRepository.GetPhotoMetaAsync(id);
            if (photoEntity == null)
                return NotFound();

            photoEntity.Title = photo.Title;
            var result = await photosRepository.UpdatePhotoAsync(photoEntity);
            if (!result)
                return Conflict();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(Guid id)
        {
            var photoEntity = await photosRepository.GetPhotoMetaAsync(id);
            if (photoEntity == null)
                return NotFound();

            var result = await photosRepository.DeletePhotoAsync(photoEntity);
            if (!result)
                return Conflict();
            return NoContent();
        }

        private string GeneratePhotoUrl(PhotoDto photo)
        {
            var relativeUrl = Url.Action(nameof(GetPhotoContent), new {
                id = photo.Id
            });
            var url = "https://localhost:6001" + relativeUrl;
            return url;
        }
    }
}
