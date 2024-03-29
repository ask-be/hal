using System.Text.Json;
using ASK.HAL;
using ASK.HAL.Serialization.Json;
using FluentAssertions;

namespace HAL.Tests;

public class ResourceTests
{
    private readonly IResourceFactory _resourceFactory = new ResourceFactory(new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters = {new ResourceJsonConverter()},
        WriteIndented = true,
    });
    
    [Fact]
    public void CanCreateMinimalResource()
    {
        var self = "http://self";
        
        var r = _resourceFactory.Create(self);
        r.Should().NotBeNull();
        r.Self.Should().Be(self);
        r.GetLink("self")!.Href.Should().Be(self);
    }
    
    [Fact]
    public void CanCreateResourceWithOtherLink()
    {
        var linkUri = "http://link";

        var r = _resourceFactory.Create("http://self").AddLink("other", new Uri(linkUri));
        var link = r.GetLink("other");
        link.Should().NotBeNull();
        link!.Href.Should().Be(linkUri);
    }
    
    [Fact]
    public void CannotAddTwoLinksWithSameName()
    {
        var linkUri = new Uri("http://link");

        Assert.Throws<ResourceException>(() => _resourceFactory.Create("http://self")
                                                               .AddLink("same", linkUri)
                                                               .AddLink("same", linkUri));
    }
    
    [Fact]
    public void CannotRemoveLinksThatDoesNotExists()
    {
        Assert.Throws<ResourceException>(() => _resourceFactory.Create("http://self").RemoveLink("does_not_exists"));
    }
    
    [Fact]
    public void CannotRetrieveLinkTheDoesNotExists()
    {
        var linkUri = new Uri("http://link");

        var r = _resourceFactory.Create("http://self").AddLink("exists", linkUri);

        r.GetLink("does_not_exists").Should().BeNull();
    }
    
    [Fact]
    public void CanCreateResourceWithMultiValuedLink()
    {
        var linkUri = new Uri("http://link");
        var linkUri2 = new Uri("http://link2");

        var r = _resourceFactory.Create("http://self")
                                .AddLinks("other", 
                                    new Link(linkUri,name:"1"), 
                                    new Link(linkUri2,name:"2"));
        
        var links = r.GetLinks("other");
        links.First(x => x.Name == "1").Href.Should().Be(linkUri);
        links.First(x => x.Name == "2").Href.Should().Be(linkUri2);
    }

    [Fact]
    public void ResourceCanHavePropertiesFromAnonymousClass()
    {
        var self = "http://self";
        var r = _resourceFactory.Create(self);
        r.Add(new
        {
            FirstName = "John",
            LastName = "Doe",
            BirthDate = new DateTime(2024, 4, 23,0,0,0,DateTimeKind.Local)
        });

        r.GetValue<string>("FirstName").Should().Be("John");
        r.GetValue<string>("LastName").Should().Be("Doe");

        var birthDate = r.GetValue<DateTime>("BirthDate");
        birthDate.Year.Should().Be(2024);
        birthDate.Month.Should().Be(4);
        birthDate.Day.Should().Be(23);
    }
    
    [Fact]
    public void ResourcePropertiesNameAreCaseInsensitive()
    {
        var self = "http://self";
        var r = _resourceFactory.Create(self);
        r.Add(new
        {
            FirstName = "John",
        });

        r.GetValue<string>("FirstName").Should().Be("John");
        r.GetValue<string>("firstName").Should().Be("John");
        r.GetValue<string>("FIRSTNAME").Should().Be("John");
        r.GetValue<string>("firstname").Should().Be("John");
    }
    
    [Fact]
    public void ResourceGetValueReturnNullIfPropertyDoesNotExists()
    {
        var self = "http://self";
        var r = _resourceFactory.Create(self);
        r.Add(new
        {
            FirstName = "John",
        });

        r.GetValue<string>("Address").Should().BeNull();
    }
    
    [Fact]
    public void ResourceGetValueShouldThrowResourceExceptionIfPropertyCastIsInvalid()
    {
        var self = "http://self";
        var r = _resourceFactory.Create(self);
        r.Add(new
        {
            FirstName = "John",
        });
        Assert.Throws<ResourceException>(() => r.GetValue<int>("firstName"));
    }
    
    [Fact]
    public void ResourceAsShouldReturnObject()
    {
        var self = "http://self";
        var r = _resourceFactory.Create(self);
        r.Add(new
        {
            FirstName = "John",
            LastName = "Doe",
            BirthDate = new DateTime(2024, 4, 23,0,0,0,DateTimeKind.Local)
        });

        var e = r.As<Employee>();
        e.FirstName.Should().Be("John");
        e.LastName.Should().Be("Doe");
        e.BirthDate.Year.Should().Be(2024);
        e.BirthDate.Month.Should().Be(4);
        e.BirthDate.Day.Should().Be(23);
    }

    public record Employee(string FirstName, string LastName, DateTime BirthDate);

    [Fact]
    public void EnsureWeIgnoreLinkAdditionalJsonProperties()
    {
        var json = "{\"_links\":{\"self\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e\"},\"diagrams\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e/model/diagrams\"},\"model\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e/model\"},\"hubs\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e/model/hubs\"},\"links\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e/model/links\"},\"sourceSystems\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e/metavault/sourcesystems\"},\"versions\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e/versions\"},\"environments\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e/environments\"},\"hubmappings\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e/mappings/hubs\"},\"linkmappings\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e/mappings/links\"},\"snapshots\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e/model/snapshots\"},\"servers\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e/servers\",\"method\":[\"GET\"]},\"dataqualities\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e/dataquality\"},\"queries\":{\"href\":\"http://localhost:5000/api/projects/3af0f98a85684c88bc6e59bb9d62e43e/queries\"},\"testtemplate\":{\"href\":\"http://localhost:5000/api/templates/test\",\"method\":[\"POST\"]}},\"id\":\"3af0f98a85684c88bc6e59bb9d62e43e\",\"name\":\"Dylan_Bob\",\"technicalName\":\"Dylan_Bob\",\"displayName\":\"Sales\",\"creationDate\":\"2024-03-26T15:21:12.0569411Z\",\"description\":\"Sales Domain\",\"numberOfHubs\":0,\"numberOfSources\":0,\"numberOfDataQualityControls\":0}";
        var rrr = ResourceJsonSerializer.Deserialize(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = {new ResourceJsonConverter()},
            WriteIndented = true,
        });
    }
}