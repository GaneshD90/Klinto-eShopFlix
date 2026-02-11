using CatalogService.Application.Products.Commands;
using CatalogService.Application.Products.Validators;
using FluentAssertions;
using Xunit;

namespace CatalogService.Application.Tests.Products.Validators;

public class CreateProductCommandValidatorTests
{
    [Fact]
    public void Validate_WhenSlugInvalid_ReturnsError()
    {
        var validator = new CreateProductCommandValidator();
        var command = new CreateProductCommand
        {
            Name = "Test",
            Slug = "Invalid Slug",
            Status = 1
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Slug");
    }

    [Fact]
    public void Validate_WhenStatusInvalid_ReturnsError()
    {
        var validator = new CreateProductCommandValidator();
        var command = new CreateProductCommand
        {
            Name = "Test",
            Slug = "valid-slug",
            Status = 250
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
    }
}
