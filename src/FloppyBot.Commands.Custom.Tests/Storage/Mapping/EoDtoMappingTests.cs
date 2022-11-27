using System.Collections.Immutable;
using AutoMapper;
using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.LiteDb;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Custom.Storage.Entities.Internal;

namespace FloppyBot.Commands.Custom.Tests.Storage.Mapping;

[TestClass]
public class EoDtoMappingTests
{
    private readonly IMapper _mapper;
    private readonly IRepositoryFactory _repositoryFactory;

    public EoDtoMappingTests()
    {
        _mapper = new MapperConfiguration(c => c.AddProfile<CustomCommandStorageProfile>())
            .CreateMapper();
        _repositoryFactory = LiteDbRepositoryFactory.CreateMemoryInstance();
    }

    private bool TestConversion<TDto, TEo>(TDto dto)
    {
        // convert dto -> eo -> dto
        var eo = _mapper.Map<TEo>(dto);
        var backDto = _mapper.Map<TDto>(eo);

        // conversion is successful when both DTOs are equal
        Assert.AreEqual(dto, backDto);

        return true;
    }

    private bool TestConversionAndStorage<TDto, TEo>(TDto dto) where TEo : class, IEntity<TEo>
    {
        var repo = _repositoryFactory.GetRepository<TEo>();

        // convert dto -> eo -> dto
        var eo = _mapper.Map<TEo>(dto);

        var insertedEo = repo.Insert(eo);
        var fetchedEo = repo.GetById(insertedEo.Id);

        var backDto = _mapper.Map<TDto>(fetchedEo);

        // conversion is successful when both DTOs are equal
        Assert.AreEqual(dto, backDto);

        return true;
    }

    [TestMethod]
    public void CustomCommandDtoAndEo()
    {
        Assert.IsTrue(
            TestConversionAndStorage<CustomCommandDescription, CustomCommandDescriptionEo>(new CustomCommandDescription
            {
                Id = "someId",
                Name = "someCommand",
                Aliases = new[] { "a", "b" }.ToImmutableHashSet(),
                Owners = new[] { "Mock/Channel1" }.ToImmutableHashSet(),
                Responses = new[]
                {
                    new CommandResponse(
                        ResponseType.Text,
                        "Hello World")
                }.ToImmutableList(),
                Limitations = new CommandLimitation
                {
                    MinLevel = PrivilegeLevel.Moderator,
                    Cooldown = new[]
                    {
                        new CooldownDescription(PrivilegeLevel.Moderator, 2500),
                        new CooldownDescription(PrivilegeLevel.Administrator, 0),
                    }.ToImmutableHashSet()
                }
            }));
    }

    [TestMethod]
    public void CommandResponseDtoAndEo()
    {
        Assert.IsTrue(Enum.GetValues<ResponseType>()
            .Select(t => new CommandResponse(t, "Hello World"))
            .All(TestConversion<CommandResponse, CommandResponseEo>));
    }

    [TestMethod]
    public void CommandLimitationDtoAndEo()
    {
        Assert.IsTrue(Enum.GetValues<PrivilegeLevel>()
            .Select(l => new CommandLimitation
            {
                MinLevel = l,
                Cooldown = new[]
                {
                    new CooldownDescription(PrivilegeLevel.Superuser, 0),
                    new CooldownDescription(PrivilegeLevel.Administrator, 100),
                    new CooldownDescription(PrivilegeLevel.Moderator, 30000),
                }.ToImmutableHashSet()
            })
            .All(TestConversion<CommandLimitation, CommandLimitationEo>));
    }

    [TestMethod]
    public void CooldownDescriptionDtoAndEo()
    {
        Assert.IsTrue(Enum.GetValues<PrivilegeLevel>()
            .Select(l => new CooldownDescription(l, 2301))
            .All(TestConversion<CooldownDescription, CooldownDescriptionEo>));
    }
}
