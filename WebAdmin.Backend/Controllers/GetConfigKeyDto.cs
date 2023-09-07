namespace WebAdmin.Backend.Controllers;

public record GetConfigKeyDto(
	string OverrideLevel,
	string Value
);
