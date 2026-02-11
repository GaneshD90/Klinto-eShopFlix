using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CatalogService.Domain.Tests;

public class ProductExtensionsTests
{
    [Fact]
    public void Activate_SetsStatusToActive()
    {
        var product = new Product { Status = (byte)ProductStatus.Draft };

        product.Activate();

        product.GetStatus().Should().Be(ProductStatus.Active);
    }

    [Fact]
    public void Deactivate_SetsStatusToInactive()
    {
        var product = new Product { Status = (byte)ProductStatus.Active };

        product.Deactivate();

        product.GetStatus().Should().Be(ProductStatus.Inactive);
    }

    [Fact]
    public void UpdateBasics_UpdatesFieldsAndTimestamp()
    {
        var product = new Product
        {
            Name = "Old",
            Slug = "old",
            ShortDescription = "old",
            LongDescription = "old",
            BrandId = 1,
            ManufacturerId = 2,
            CategoryId = 3,
            IsSearchable = false,
            Weight = 1.5m,
            Dimensions = "10x10",
            PrimaryImageUrl = "old.png",
            SeoTitle = "old",
            SeoDescription = "old",
            SeoKeywords = "old"
        };

        var before = DateTime.UtcNow;

        product.UpdateBasics(
            name: "New",
            slug: "new",
            shortDescription: "short",
            longDescription: "long",
            brandId: 10,
            manufacturerId: 11,
            categoryId: 12,
            isSearchable: true,
            weight: 2.5m,
            dimensions: "20x20",
            primaryImageUrl: "new.png",
            seoTitle: "seo",
            seoDescription: "desc",
            seoKeywords: "keywords");

        product.Name.Should().Be("New");
        product.Slug.Should().Be("new");
        product.ShortDescription.Should().Be("short");
        product.LongDescription.Should().Be("long");
        product.BrandId.Should().Be(10);
        product.ManufacturerId.Should().Be(11);
        product.CategoryId.Should().Be(12);
        product.IsSearchable.Should().BeTrue();
        product.Weight.Should().Be(2.5m);
        product.Dimensions.Should().Be("20x20");
        product.PrimaryImageUrl.Should().Be("new.png");
        product.SeoTitle.Should().Be("seo");
        product.SeoDescription.Should().Be("desc");
        product.SeoKeywords.Should().Be("keywords");
        product.LastModifiedDate.Should().NotBeNull();
        product.LastModifiedDate.Should().BeOnOrAfter(before);
    }
}
