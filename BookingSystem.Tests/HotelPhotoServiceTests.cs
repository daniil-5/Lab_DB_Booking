using AutoFixture;
using BookingSystem.Application.DTOs.HotelPhoto;
using BookingSystem.Application.Services;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Domain.Other;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace BookingSystem.Tests
{
    public class HotelPhotoServiceTests
    {
        private readonly Mock<IRepository<HotelPhoto>> _hotelPhotoRepoMock;
        private readonly Mock<IPhotoRepository> _cloudinaryRepoMock;
        private readonly HotelPhotoService _service;
        private readonly Fixture _fixture;

        public HotelPhotoServiceTests()
        {
            _fixture = new Fixture();
    
            // Configure AutoFixture to handle circular references
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    
            _hotelPhotoRepoMock = new Mock<IRepository<HotelPhoto>>();
            _cloudinaryRepoMock = new Mock<IPhotoRepository>();
            _service = new HotelPhotoService(_hotelPhotoRepoMock.Object, _cloudinaryRepoMock.Object);
        }

        [Fact]
        public async Task CreateHotelPhotoAsync_ValidInput_CreatesPhoto()
        {
            // Arrange
            var createDto = _fixture.Create<CreateHotelPhotoDto>();
            HotelPhoto capturedPhoto = null;
            
            _hotelPhotoRepoMock.Setup(x => x.AddAsync(It.IsAny<HotelPhoto>()))
                .Callback<HotelPhoto>(p => capturedPhoto = p)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateHotelPhotoAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.HotelId, result.HotelId);
            Assert.Equal(createDto.Url, result.Url);
            Assert.Equal(createDto.PublicId, result.PublicId);
            Assert.Equal(createDto.Description, result.Description);
            Assert.Equal(createDto.IsMain, result.IsMain);
            
            _hotelPhotoRepoMock.Verify(x => x.AddAsync(It.IsAny<HotelPhoto>()), Times.Once);
            
            // Verify the captured photo has correct properties
            Assert.NotNull(capturedPhoto);
            Assert.Equal(createDto.HotelId, capturedPhoto.HotelId);
            Assert.Equal(createDto.Url, capturedPhoto.Url);
            Assert.Equal(createDto.PublicId, capturedPhoto.PublicId);
        }

        [Fact]
        public async Task UploadHotelPhotoAsync_ValidInput_UploadsAndCreatesPhoto()
        {
            // Arrange
            var hotelId = 1;
            var description = "Test photo";
            var isMain = true;
            
            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(1024); // Non-empty file
            
            var uploadResult = new PhotoUploadResult 
            { 
                Url = "http://example.com/photo.jpg", 
                PublicId = "photo123" 
            };
            
            _cloudinaryRepoMock.Setup(x => x.UploadPhotoAsync(It.IsAny<IFormFile>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(uploadResult);
            
            HotelPhoto capturedPhoto = null;
            _hotelPhotoRepoMock.Setup(x => x.AddAsync(It.IsAny<HotelPhoto>()))
                .Callback<HotelPhoto>(p => capturedPhoto = p)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UploadHotelPhotoAsync(file.Object, hotelId, description, isMain);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(hotelId, result.HotelId);
            Assert.Equal(uploadResult.Url, result.Url);
            Assert.Equal(uploadResult.PublicId, result.PublicId);
            Assert.Equal(description, result.Description);
            Assert.Equal(isMain, result.IsMain);
            
            _cloudinaryRepoMock.Verify(x => x.UploadPhotoAsync(It.IsAny<IFormFile>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            _hotelPhotoRepoMock.Verify(x => x.AddAsync(It.IsAny<HotelPhoto>()), Times.Once);
            
            // Verify the captured photo has correct properties
            Assert.NotNull(capturedPhoto);
            Assert.Equal(hotelId, capturedPhoto.HotelId);
            Assert.Equal(uploadResult.Url, capturedPhoto.Url);
            Assert.Equal(uploadResult.PublicId, capturedPhoto.PublicId);
            Assert.Equal(description, capturedPhoto.Description);
            Assert.Equal(isMain, capturedPhoto.IsMain);
        }
        
        [Fact]
        public async Task UploadHotelPhotoAsync_EmptyFile_ThrowsArgumentException()
        {
            // Arrange
            var hotelId = 1;
            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(0); // Empty file
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.UploadHotelPhotoAsync(file.Object, hotelId));
                
            Assert.Contains("File is required", exception.Message);
            Assert.Equal("file", exception.ParamName);
        }
        
        [Fact]
        public async Task UploadHotelPhotoAsync_InvalidHotelId_ThrowsArgumentException()
        {
            // Arrange
            var hotelId = 0; // Invalid ID
            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(1024); // Non-empty file
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.UploadHotelPhotoAsync(file.Object, hotelId));
                
            Assert.Contains("Hotel ID must be a positive number", exception.Message);
            Assert.Equal("hotelId", exception.ParamName);
        }

        [Fact]
        public async Task UploadMultipleHotelPhotosAsync_ValidInput_UploadsAndCreatesPhotos()
        {
            // Arrange
            var hotelId = 1;
    
            // Create mocks for IFormFile properly
            var file1 = new Mock<IFormFile>();
            file1.Setup(f => f.Length).Returns(1024);
    
            var file2 = new Mock<IFormFile>();
            file2.Setup(f => f.Length).Returns(1024);
    
            var files = new List<IFormFile> { file1.Object, file2.Object };
    
            var uploadResults = new List<PhotoUploadResult>
            {
                new PhotoUploadResult { Url = "http://example.com/photo1.jpg", PublicId = "photo1" },
                new PhotoUploadResult { Url = "http://example.com/photo2.jpg", PublicId = "photo2" }
            };
    
            _cloudinaryRepoMock.Setup(x => x.UploadPhotosAsync(It.IsAny<IEnumerable<IFormFile>>(), It.IsAny<int>()))
                .ReturnsAsync(uploadResults);
    
            var capturedPhotos = new List<HotelPhoto>();
            _hotelPhotoRepoMock.Setup(x => x.AddAsync(It.IsAny<HotelPhoto>()))
                .Callback<HotelPhoto>(p => capturedPhotos.Add(p))
                .Returns(Task.CompletedTask);

            // Act
            var results = await _service.UploadMultipleHotelPhotosAsync(files, hotelId);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(files.Count, results.Count());
    
            _cloudinaryRepoMock.Verify(x => x.UploadPhotosAsync(It.IsAny<IEnumerable<IFormFile>>(), It.IsAny<int>()), Times.Once);
            _hotelPhotoRepoMock.Verify(x => x.AddAsync(It.IsAny<HotelPhoto>()), Times.Exactly(files.Count));
    
            // Verify the captured photos
            Assert.Equal(files.Count, capturedPhotos.Count);
            Assert.All(capturedPhotos, p => Assert.Equal(hotelId, p.HotelId));
            Assert.Collection(capturedPhotos,
                p1 => {
                    Assert.Equal(uploadResults[0].Url, p1.Url);
                    Assert.Equal(uploadResults[0].PublicId, p1.PublicId);
                },
                p2 => {
                    Assert.Equal(uploadResults[1].Url, p2.Url);
                    Assert.Equal(uploadResults[1].PublicId, p2.PublicId);
                }
            );
        }
        
        [Fact]
        public async Task UploadMultipleHotelPhotosAsync_EmptyFilesList_ThrowsArgumentException()
        {
            // Arrange
            var hotelId = 1;
            var emptyFiles = new List<IFormFile>();
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.UploadMultipleHotelPhotosAsync(emptyFiles, hotelId));
                
            Assert.Contains("Files are required", exception.Message);
            Assert.Equal("files", exception.ParamName);
        }
        
        [Fact]
        public async Task UploadMultipleHotelPhotosAsync_NullFilesList_ThrowsArgumentException()
        {
            // Arrange
            var hotelId = 1;
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.UploadMultipleHotelPhotosAsync(null, hotelId));
                
            Assert.Contains("Files are required", exception.Message);
            Assert.Equal("files", exception.ParamName);
        }

        [Fact]
        public async Task UpdateHotelPhotoAsync_ValidInput_UpdatesPhoto()
        {
            // Arrange
            var photoId = 1;
            var updateDto = _fixture.Build<UpdateHotelPhotoDto>()
                .With(p => p.Id, photoId)
                .Create();
            
            var existingPhoto = _fixture.Build<HotelPhoto>()
                .With(p => p.Id, photoId)
                .Create();
            
            _hotelPhotoRepoMock.Setup(x => x.GetByIdAsync(photoId))
                .ReturnsAsync(existingPhoto);
            
            HotelPhoto capturedPhoto = null;
            _hotelPhotoRepoMock.Setup(x => x.UpdateAsync(It.IsAny<HotelPhoto>()))
                .Callback<HotelPhoto>(p => capturedPhoto = p)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateHotelPhotoAsync(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.Id, result.Id);
            Assert.Equal(updateDto.HotelId, result.HotelId);
            Assert.Equal(updateDto.Url, result.Url);
            Assert.Equal(updateDto.PublicId, result.PublicId);
            Assert.Equal(updateDto.Description, result.Description);
            Assert.Equal(updateDto.IsMain, result.IsMain);
            
            _hotelPhotoRepoMock.Verify(x => x.UpdateAsync(It.IsAny<HotelPhoto>()), Times.Once);
            
            // Verify the photo was updated correctly
            Assert.NotNull(capturedPhoto);
            Assert.Equal(updateDto.HotelId, capturedPhoto.HotelId);
            Assert.Equal(updateDto.Url, capturedPhoto.Url);
            Assert.Equal(updateDto.PublicId, capturedPhoto.PublicId);
            Assert.Equal(updateDto.Description, capturedPhoto.Description);
            Assert.Equal(updateDto.IsMain, capturedPhoto.IsMain);
        }
        
        [Fact]
        public async Task UpdateHotelPhotoAsync_InvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var photoId = 999; // Non-existent ID
            var updateDto = _fixture.Build<UpdateHotelPhotoDto>()
                .With(p => p.Id, photoId)
                .Create();
            
            _hotelPhotoRepoMock.Setup(x => x.GetByIdAsync(photoId))
                .ReturnsAsync((HotelPhoto)null);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.UpdateHotelPhotoAsync(updateDto));
                
            Assert.Contains($"Photo with ID {photoId} not found", exception.Message);
            _hotelPhotoRepoMock.Verify(x => x.UpdateAsync(It.IsAny<HotelPhoto>()), Times.Never);
        }

        [Fact]
        public async Task DeleteHotelPhotoAsync_ValidId_DeletesPhoto()
        {
            // Arrange
            var photoId = 1;
            var publicId = "photo123";
            
            var existingPhoto = _fixture.Build<HotelPhoto>()
                .With(p => p.Id, photoId)
                .With(p => p.PublicId, publicId)
                .Create();
            
            _hotelPhotoRepoMock.Setup(x => x.GetByIdAsync(photoId))
                .ReturnsAsync(existingPhoto);
            
            _cloudinaryRepoMock.Setup(x => x.DeletePhotoAsync(publicId))
                .ReturnsAsync(true);
            
            _hotelPhotoRepoMock.Setup(x => x.DeleteAsync(photoId))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteHotelPhotoAsync(photoId);

            // Assert
            _cloudinaryRepoMock.Verify(x => x.DeletePhotoAsync(publicId), Times.Once);
            _hotelPhotoRepoMock.Verify(x => x.DeleteAsync(photoId), Times.Once);
        }
        
        [Fact]
        public async Task DeleteHotelPhotoAsync_InvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var photoId = 999; // Non-existent ID
            
            _hotelPhotoRepoMock.Setup(x => x.GetByIdAsync(photoId))
                .ReturnsAsync((HotelPhoto)null);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.DeleteHotelPhotoAsync(photoId));
                
            Assert.Contains($"Photo with ID {photoId} not found", exception.Message);
            _hotelPhotoRepoMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
            _cloudinaryRepoMock.Verify(x => x.DeletePhotoAsync(It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public async Task DeleteHotelPhotoAsync_CloudinaryDeletionFails_ThrowsException()
        {
            // Arrange
            var photoId = 1;
            var publicId = "photo123";
    
            // Create photo directly instead of using AutoFixture to avoid circular references
            var existingPhoto = new HotelPhoto
            {
                Id = photoId,
                PublicId = publicId,
                Url = "http://example.com/photo.jpg"
            };
    
            _hotelPhotoRepoMock.Setup(x => x.GetByIdAsync(photoId))
                .ReturnsAsync(existingPhoto);
    
            _cloudinaryRepoMock.Setup(x => x.DeletePhotoAsync(publicId))
                .ReturnsAsync(false); // Deletion fails
    
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _service.DeleteHotelPhotoAsync(photoId));
        
            Assert.Contains($"Failed to delete photo with public ID {publicId}", exception.Message);
            _hotelPhotoRepoMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
        

        [Fact]
        public async Task GetHotelPhotoByIdAsync_ValidId_ReturnsPhoto()
        {
            // Arrange
            var photoId = 1;
            var photo = _fixture.Build<HotelPhoto>()
                .With(p => p.Id, photoId)
                .Create();
            
            _hotelPhotoRepoMock.Setup(x => x.GetByIdAsync(photoId))
                .ReturnsAsync(photo);

            // Act
            var result = await _service.GetHotelPhotoByIdAsync(photoId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(photoId, result.Id);
            Assert.Equal(photo.HotelId, result.HotelId);
            Assert.Equal(photo.Url, result.Url);
            Assert.Equal(photo.PublicId, result.PublicId);
        }
        
        [Fact]
        public async Task GetHotelPhotoByIdAsync_InvalidId_ReturnsNull()
        {
            // Arrange
            var photoId = 999; // Non-existent ID
            
            _hotelPhotoRepoMock.Setup(x => x.GetByIdAsync(photoId))
                .ReturnsAsync((HotelPhoto)null);

            // Act
            var result = await _service.GetHotelPhotoByIdAsync(photoId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPhotosByHotelIdAsync_ValidHotelId_ReturnsPhotos()
        {
            // Arrange
            var hotelId = 1;
            var photos = _fixture.Build<HotelPhoto>()
                .With(p => p.HotelId, hotelId)
                .With(p => p.IsDeleted, false)
                .CreateMany(3)
                .ToList();
            
            _hotelPhotoRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(photos);

            // Act
            var results = await _service.GetPhotosByHotelIdAsync(hotelId);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(photos.Count, results.Count());
            Assert.All(results, r => Assert.Equal(hotelId, r.HotelId));
        }
        
        [Fact]
        public async Task GetPhotosByHotelIdAsync_HotelWithNoPhotos_ReturnsEmptyList()
        {
            // Arrange
            var hotelId = 1;
            var otherHotelId = 2;
            var photos = _fixture.Build<HotelPhoto>()
                .With(p => p.HotelId, otherHotelId) // Different hotel ID
                .With(p => p.IsDeleted, false)
                .CreateMany(3)
                .ToList();
            
            _hotelPhotoRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(photos);

            // Act
            var results = await _service.GetPhotosByHotelIdAsync(hotelId);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }
        
        [Fact]
        public async Task GetPhotosByHotelIdAsync_FiltersDeletedPhotos()
        {
            // Arrange
            var hotelId = 1;
            var photos = new List<HotelPhoto>
            {
                _fixture.Build<HotelPhoto>()
                    .With(p => p.HotelId, hotelId)
                    .With(p => p.IsDeleted, false)
                    .Create(),
                _fixture.Build<HotelPhoto>()
                    .With(p => p.HotelId, hotelId)
                    .With(p => p.IsDeleted, true) // Deleted photo
                    .Create()
            };
            
            _hotelPhotoRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(photos);

            // Act
            var results = await _service.GetPhotosByHotelIdAsync(hotelId);

            // Assert
            Assert.NotNull(results);
            Assert.Single(results); // Only one non-deleted photo
        }

        [Fact]
        public async Task SetMainPhotoAsync_ValidInput_SetsMainPhoto()
        {
            // Arrange
            var hotelId = 1;
            var photoId = 2;
            
            var photos = new List<HotelPhoto>
            {
                new HotelPhoto { Id = 1, HotelId = hotelId, IsMain = true },
                new HotelPhoto { Id = photoId, HotelId = hotelId, IsMain = false },
                new HotelPhoto { Id = 3, HotelId = hotelId, IsMain = false }
            };
            
            _hotelPhotoRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(photos);
            
            var updatedPhotos = new List<HotelPhoto>();
            _hotelPhotoRepoMock.Setup(x => x.UpdateAsync(It.IsAny<HotelPhoto>()))
                .Callback<HotelPhoto>(p => updatedPhotos.Add(p))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SetMainPhotoAsync(photoId, hotelId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(photoId, result.Id);
            Assert.True(result.IsMain);
            
            // Should update exactly 2 photos: the old main and the new main
            _hotelPhotoRepoMock.Verify(x => x.UpdateAsync(It.IsAny<HotelPhoto>()), Times.Exactly(2));
            
            // Verify updates: old main photo should be false, new one should be true
            Assert.Equal(2, updatedPhotos.Count);
            Assert.Contains(updatedPhotos, p => p.Id == 1 && !p.IsMain);
            Assert.Contains(updatedPhotos, p => p.Id == photoId && p.IsMain);
        }
        
        [Fact]
        public async Task SetMainPhotoAsync_InvalidPhotoId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var hotelId = 1;
            var invalidPhotoId = 999; // Non-existent photo ID
            
            var photos = new List<HotelPhoto>
            {
                new HotelPhoto { Id = 1, HotelId = hotelId, IsMain = true },
                new HotelPhoto { Id = 2, HotelId = hotelId, IsMain = false }
            };
            
            _hotelPhotoRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(photos);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.SetMainPhotoAsync(invalidPhotoId, hotelId));
                
            Assert.Contains($"Photo with ID {invalidPhotoId} not found", exception.Message);
            _hotelPhotoRepoMock.Verify(x => x.UpdateAsync(It.IsAny<HotelPhoto>()), Times.Never);
        }

        [Fact]
        public async Task GetTransformedImageUrlAsync_ValidInput_ReturnsTransformedUrl()
        {
            // Arrange
            var photoId = 1;
            var publicId = "photo123";
            var transformation = "w_500,h_500,c_fill";
            var transformedUrl = "http://example.com/photo_transformed.jpg";
            
            var photo = _fixture.Build<HotelPhoto>()
                .With(p => p.Id, photoId)
                .With(p => p.PublicId, publicId)
                .Create();
            
            _hotelPhotoRepoMock.Setup(x => x.GetByIdAsync(photoId))
                .ReturnsAsync(photo);
            
            _cloudinaryRepoMock.Setup(x => x.GetImageUrlAsync(publicId, transformation))
                .ReturnsAsync(transformedUrl);

            // Act
            var result = await _service.GetTransformedImageUrlAsync(photoId, transformation);

            // Assert
            Assert.Equal(transformedUrl, result);
            _cloudinaryRepoMock.Verify(x => x.GetImageUrlAsync(publicId, transformation), Times.Once);
        }
        
        [Fact]
        public async Task GetTransformedImageUrlAsync_InvalidPhotoId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var photoId = 999; // Non-existent ID
            var transformation = "w_500,h_500,c_fill";
            
            _hotelPhotoRepoMock.Setup(x => x.GetByIdAsync(photoId))
                .ReturnsAsync((HotelPhoto)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.GetTransformedImageUrlAsync(photoId, transformation));
                
            Assert.Contains($"Photo with ID {photoId} not found", exception.Message);
            _cloudinaryRepoMock.Verify(x => x.GetImageUrlAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SyncCloudinaryPhotosAsync_ValidHotelId_SyncsPhotos()
        {
            // Arrange
            var hotelId = 1;
            
            // Existing photos in database
            var dbPhotos = new List<HotelPhoto>
            {
                new HotelPhoto { Id = 1, HotelId = hotelId, PublicId = "photo1", Url = "http://example.com/photo1.jpg" },
                new HotelPhoto { Id = 2, HotelId = hotelId, PublicId = "photo2", Url = "http://example.com/photo2.jpg" },
                new HotelPhoto { Id = 3, HotelId = hotelId, PublicId = null, Url = "http://external-source.com/photo3.jpg" }
            };
            
            // Photos from Cloudinary
            var cloudinaryPhotos = new List<PhotoUploadResult>
            {
                new PhotoUploadResult { PublicId = "photo1", Url = "http://example.com/photo1.jpg" },
                new PhotoUploadResult { PublicId = "photo3", Url = "http://example.com/photo3.jpg" } // New photo in Cloudinary
            };
            
            _hotelPhotoRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(dbPhotos);
            
            _cloudinaryRepoMock.Setup(x => x.GetHotelPhotosFromCloudinaryAsync(hotelId, It.IsAny<int>()))
                .ReturnsAsync(cloudinaryPhotos);
            
            _hotelPhotoRepoMock.Setup(x => x.UpdateAsync(It.IsAny<HotelPhoto>()))
                .Returns(Task.CompletedTask);
            
            _hotelPhotoRepoMock.Setup(x => x.AddAsync(It.IsAny<HotelPhoto>()))
                .Returns(Task.CompletedTask);
            
            // Mock for uploading a photo from URL
            _cloudinaryRepoMock.Setup(x => x.UploadPhotoAsync(It.IsAny<IFormFile>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new PhotoUploadResult { PublicId = "new_photo", Url = "http://example.com/new_photo.jpg" });

            // Act
            await _service.SyncCloudinaryPhotosAsync(hotelId);

            // Assert - Verify interactions
            _hotelPhotoRepoMock.Verify(x => x.GetAllAsync(), Times.AtLeastOnce);
            _cloudinaryRepoMock.Verify(x => x.GetHotelPhotosFromCloudinaryAsync(hotelId, It.IsAny<int>()), Times.AtLeastOnce);
            
            // Should update photo 2 (missing from Cloudinary)
            _hotelPhotoRepoMock.Verify(x => x.UpdateAsync(It.Is<HotelPhoto>(p => 
                p.Id == 2 && p.PublicId == null)), Times.Once);
            
            // Should add photo3 from Cloudinary to DB
            _hotelPhotoRepoMock.Verify(x => x.AddAsync(It.Is<HotelPhoto>(p => 
                p.PublicId == "photo3")), Times.Once);
        }
        [Fact]
        public async Task UploadHotelPhotoAsync_NullDescription_UsesDefaultDescription()
        {
            // Arrange
            var hotelId = 1;
            var isMain = false;
    
            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(1024); // Non-empty file
    
            var uploadResult = new PhotoUploadResult 
            { 
                Url = "http://example.com/photo.jpg", 
                PublicId = "photo123" 
            };
    
            _cloudinaryRepoMock.Setup(x => x.UploadPhotoAsync(It.IsAny<IFormFile>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(uploadResult);
    
            HotelPhoto capturedPhoto = null;
            _hotelPhotoRepoMock.Setup(x => x.AddAsync(It.IsAny<HotelPhoto>()))
                .Callback<HotelPhoto>(p => capturedPhoto = p)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UploadHotelPhotoAsync(file.Object, hotelId, null, isMain);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("No description", result.Description);
    
            Assert.NotNull(capturedPhoto);
            Assert.Equal("No description", capturedPhoto.Description);
        }
        
        [Fact]
        public async Task GetAllHotelPhotosAsync_ReturnsAllPhotos()
        {
            // Arrange
            var photos = _fixture.CreateMany<HotelPhoto>(3).ToList();
    
            _hotelPhotoRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(photos);

            // Act
            var results = await _service.GetAllHotelPhotosAsync();

            // Assert
            Assert.NotNull(results);
            Assert.Equal(photos.Count, results.Count());
    
            for (int i = 0; i < photos.Count; i++)
            {
                var original = photos[i];
                var result = results.ElementAt(i);
        
                Assert.Equal(original.Id, result.Id);
                Assert.Equal(original.HotelId, result.HotelId);
                Assert.Equal(original.Url, result.Url);
                Assert.Equal(original.PublicId, result.PublicId);
            }
        }
    }
}