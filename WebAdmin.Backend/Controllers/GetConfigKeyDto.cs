using System.Text.Json.Nodes;

namespace WebAdmin.Backend.Controllers;

public record GetConfigKeyDto(
	string OverrideLevel,
	JsonNode? Value
);
