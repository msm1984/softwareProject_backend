﻿using AnalysisData.Data;
using AnalysisData.Models.GraphModel.File;
using AnalysisData.Models.UserModel;
using AnalysisData.Repositories.GraphRepositories.UserFileRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


public class UserFileRepositoryTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly UserFileRepository _sut;

    public UserFileRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped(_ => new ApplicationDbContext(options));
        _serviceProvider = serviceCollection.BuildServiceProvider();

        _sut = new UserFileRepository(CreateDbContext());
    }

    private ApplicationDbContext CreateDbContext()
    {
        return _serviceProvider.GetRequiredService<ApplicationDbContext>();
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserFileToDatabase_Whenever()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = CreateDbContext();

        // Arrange
        var userFile = new UserFile
        {
            UserId = Guid.NewGuid(),
            FileId = 1
        };

        // Act
        await _sut.AddAsync(userFile);
        var result = await context.UserFiles.FirstOrDefaultAsync(x => x.UserId == userFile.UserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userFile.UserId, result.UserId);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUserFiles_WhenUserFilesExist()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = CreateDbContext();

        // Arrange
        var userFiles = new List<UserFile>
        {
            new() { UserId = Guid.NewGuid(), FileId = 1 },
            new() { UserId = Guid.NewGuid(), FileId = 2 }
        };
        await context.UserFiles.AddRangeAsync(userFiles);
        await context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserFile_WhenUserIdExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = CreateDbContext();

        // Arrange
        var userId = Guid.NewGuid();
        var userFile = new UserFile { UserId = userId, FileId = 1 };
        await context.UserFiles.AddAsync(userFile);
        await context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenUserIdDoesNotExist()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = CreateDbContext();

        // Arrange
        var userId = Guid.NewGuid();
        var userFile = new UserFile { UserId = userId, FileId = 1 };
        await context.UserFiles.AddAsync(userFile);
        await context.SaveChangesAsync();
        // Act
        var result = await _sut.GetByUserIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByFileIdAsync_ShouldReturnUserFiles_WhenFileIdExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = CreateDbContext();

        // Arrange
        var role = new Role { RoleName = "Admin", RolePolicy = "gold" };
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var users = new List<User>
        {
            new()
            {
                Id = userId1, Username = "user1", Password = "@Test11234",
                Email = "user1@gmail.com", FirstName = "user1", LastName = "user1",
                PhoneNumber = "09111111111", ImageURL = null, Role = role
            },
            new()
            {
                Id = userId2, Username = "user2", Password = "@Test21234",
                Email = "user2@gmail.com", FirstName = "user2", LastName = "user2",
                PhoneNumber = "09111111112", ImageURL = null, Role = role
            }
        };
        context.Roles.Add(role);
        context.Users.AddRange(users);

        var fileId = 1;
        var userFiles = new List<UserFile>
        {
            new() { UserId = userId1, FileId = fileId },
            new() { UserId = userId2, FileId = fileId }
        };
        await context.UserFiles.AddRangeAsync(userFiles);
        await context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByFileIdAsync(fileId);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetUserIdsAccessHasToFile_ShouldReturnUserIds_WhenFileIdExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = CreateDbContext();

        // Arrange
        var fileId = 1;
        var userFiles = new List<UserFile>
        {
            new() { UserId = Guid.NewGuid(), FileId = fileId },
            new() { UserId = Guid.NewGuid(), FileId = fileId }
        };
        await context.UserFiles.AddRangeAsync(userFiles);
        await context.SaveChangesAsync();

        // Act
        var result = await _sut.GetUserIdsAccessHasToFile(fileId);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task DeleteByUserIdAsync_ShouldDeleteUserFile_WhenUserIdExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = CreateDbContext();

        // Arrange
        var userId = Guid.NewGuid();
        var userFile = new UserFile { UserId = userId, FileId = 1 };
        await context.UserFiles.AddAsync(userFile);
        await context.SaveChangesAsync();

        // Act
        await _sut.DeleteByUserIdAsync(userId);
        var result = await context.UserFiles.FirstOrDefaultAsync(x => x.UserId == userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteByUserIdAsync_ShouldDoNothing_WhenUserIdDoesNotExist()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = CreateDbContext();

        // Arrange
        var userId = Guid.NewGuid();
        var userFile = new UserFile { UserId = userId, FileId = 1 };
        await context.UserFiles.AddAsync(userFile);
        await context.SaveChangesAsync();

        // Act
        await _sut.DeleteByUserIdAsync(Guid.NewGuid());

        // Assert
        var result = await context.UserFiles.CountAsync();
        Assert.Equal(1, result);
    }
}