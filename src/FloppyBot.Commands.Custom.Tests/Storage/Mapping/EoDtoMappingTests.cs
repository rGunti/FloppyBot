using System.Collections.Immutable;
using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.LiteDb;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Custom.Storage.Entities.Internal;

namespace FloppyBot.Commands.Custom.Tests.Storage.Mapping;

[TestClass]
public class EoDtoMappingTests
{
    private readonly IRepositoryFactory _repositoryFactory;

    public EoDtoMappingTests()
    {
        _repositoryFactory = LiteDbRepositoryFactory.CreateMemoryInstance();
    }

    [TestMethod]
    public void CustomCommandDtoAndEo()
    {
        Assert.IsTrue(
            TestConversionAndStorage<CustomCommandDescription, CustomCommandDescriptionEo>(
                new CustomCommandDescription
                {
                    Id = "someId",
                    Name = "someCommand",
                    Aliases = new[] { "a", "b" }.ToImmutableHashSet(),
                    Owners = new[] { "Mock/Channel1" }.ToImmutableHashSet(),
                    Responses = new[]
                    {
                        new CommandResponse(ResponseType.Text, "Hello World"),
                    }.ToImmutableList(),
                    Limitations = new CommandLimitation
                    {
                        MinLevel = PrivilegeLevel.Moderator,
                        LimitedToUsers = new[] { "Twitch/User" }.ToImmutableHashSet(),
                        Cooldown = new[]
                        {
                            new CooldownDescription(PrivilegeLevel.Moderator, 2500),
                            new CooldownDescription(PrivilegeLevel.Administrator, 0),
                        }.ToImmutableHashSet(),
                    },
                }
            )
        );
    }

    [TestMethod]
    public void CommandResponseDtoAndEo()
    {
        Assert.IsTrue(
            Enum.GetValues<ResponseType>()
                .Select(t => new CommandResponse(t, "Hello World"))
                .All(TestConversion<CommandResponse, CommandResponseEo>)
        );
    }

    [TestMethod]
    public void CommandLimitationDtoAndEo()
    {
        Assert.IsTrue(
            Enum.GetValues<PrivilegeLevel>()
                .Select(l => new CommandLimitation
                {
                    MinLevel = l,
                    Cooldown = new[]
                    {
                        new CooldownDescription(PrivilegeLevel.Superuser, 0),
                        new CooldownDescription(PrivilegeLevel.Administrator, 100),
                        new CooldownDescription(PrivilegeLevel.Moderator, 30000),
                    }.ToImmutableHashSet(),
                })
                .All(TestConversion<CommandLimitation, CommandLimitationEo>)
        );
    }

    [TestMethod]
    public void CooldownDescriptionDtoAndEo()
    {
        Assert.IsTrue(
            Enum.GetValues<PrivilegeLevel>()
                .Select(l => new CooldownDescription(l, 2301))
                .All(TestConversion<CooldownDescription, CooldownDescriptionEo>)
        );
    }

    private static bool TestConversion<TDto, TEo>(TDto dto)
        where TDto : notnull
        where TEo : notnull
    {
        var eo = ConvertToEo<TDto, TEo>(dto);
        var backDto = ConvertToDto<TEo, TDto>(eo);

        Assert.AreEqual(dto, backDto);
        return true;
    }

    private static TEo ConvertToEo<TDto, TEo>(TDto dto) =>
        dto switch
        {
            CustomCommandDescription d => (TEo)(object)d.ToEo(),
            CommandResponse d => (TEo)(object)d.ToEo(),
            CommandLimitation d => (TEo)(object)d.ToEo(),
            CooldownDescription d => (TEo)(object)d.ToEo(),
            _ => throw new NotSupportedException($"No ToEo mapping for {typeof(TDto).Name}"),
        };

    private static TDto ConvertToDto<TEo, TDto>(TEo eo) =>
        eo switch
        {
            CustomCommandDescriptionEo e => (TDto)(object)e.ToDto(),
            CommandResponseEo e => (TDto)(object)e.ToDto(),
            CommandLimitationEo e => (TDto)(object)e.ToDto(),
            CooldownDescriptionEo e => (TDto)(object)e.ToDto(),
            _ => throw new NotSupportedException($"No ToDto mapping for {typeof(TEo).Name}"),
        };

    private bool TestConversionAndStorage<TDto, TEo>(TDto dto)
        where TDto : notnull
        where TEo : class, IEntity<TEo>
    {
        var repo = _repositoryFactory.GetRepository<TEo>();

        var eo = ConvertToEo<TDto, TEo>(dto);
        var insertedEo = repo.Insert(eo);
        var fetchedEo = repo.GetById(insertedEo.Id);

        var backDto = ConvertToDto<TEo, TDto>(fetchedEo!);
        Assert.AreEqual(dto, backDto);
        return true;
    }
}
