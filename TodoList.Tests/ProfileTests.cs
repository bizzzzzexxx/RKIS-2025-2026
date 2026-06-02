using System;
using Xunit;

namespace TodoList.Tests;

public class ProfileTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesProfileWithGuid()
    {
        // Arrange
        var login = "user1";
        var password = "pass123";
        var firstName = "John";
        var lastName = "Doe";
        var birthYear = 1990;

        // Act
        var profile = new Profile(login, password, firstName, lastName, birthYear);

        // Assert
        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal(login, profile.Login);
        Assert.Equal(password, profile.Password);
        Assert.Equal(firstName, profile.FirstName);
        Assert.Equal(lastName, profile.LastName);
        Assert.Equal(birthYear, profile.BirthYear);
    }

    [Fact]
    public void Constructor_WithGuid_AssignsGivenId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var login = "user2";
        var password = "pass";
        var firstName = "Jane";
        var lastName = "Smith";
        var birthYear = 1985;

        // Act
        var profile = new Profile(id, login, password, firstName, lastName, birthYear);

        // Assert
        Assert.Equal(id, profile.Id);
        Assert.Equal(login, profile.Login);
        Assert.Equal(password, profile.Password);
        Assert.Equal(firstName, profile.FirstName);
        Assert.Equal(lastName, profile.LastName);
        Assert.Equal(birthYear, profile.BirthYear);
    }

    [Fact]
    public void Constructor_NullLogin_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Profile(null, "pass", "John", "Doe", 1990));
    }

    [Fact]
    public void Constructor_NullPassword_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Profile("user", null, "John", "Doe", 1990));
    }

    [Fact]
    public void Constructor_NullFirstName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Profile("user", "pass", null, "Doe", 1990));
    }

    [Fact]
    public void Constructor_NullLastName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Profile("user", "pass", "John", null, 1990));
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var profile = new Profile("user", "secret", "John", "Doe", 1990);

        // Act
        var result = profile.VerifyPassword("secret");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        // Arrange
        var profile = new Profile("user", "secret", "John", "Doe", 1990);

        // Act
        var result = profile.VerifyPassword("wrong");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetInfo_ReturnsFormattedStringWithNameAndAge()
    {
        // Arrange
        var profile = new Profile("user", "pass", "John", "Doe", 1990);
        var expectedAge = DateTime.Now.Year - 1990;

        // Act
        var info = profile.GetInfo();

        // Assert
        Assert.Contains("John", info);
        Assert.Contains("Doe", info);
        Assert.Contains(expectedAge.ToString(), info);
    }
}