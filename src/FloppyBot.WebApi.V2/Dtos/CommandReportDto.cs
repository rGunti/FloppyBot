namespace FloppyBot.WebApi.V2.Dtos;

public record CommandReportDto(CommandAbstractDto Command, CommandConfigurationDto? Configuration);
