using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using WebAdmin.Backend.Config;

namespace WebAdmin.Backend.Controllers;

// TODO: e2e testing of this controller.
[ApiController]
[Route("[controller]")]
public class ConfigKeyController : ControllerBase {
	private readonly ILogger<ConfigKeyController> m_Logger;
	private readonly ConnectionMultiplexer m_Redis;
	private readonly IOptions<RedisConfig> m_RedisConfig;

	public ConfigKeyController(ILogger<ConfigKeyController> logger, ConnectionMultiplexer redis, IOptions<RedisConfig> redisConfig) {
		m_Logger = logger;
		m_Redis = redis;
		m_RedisConfig = redisConfig;
	}

	[HttpGet("{overrideLevel}/{key}")]
	public async Task<IActionResult> GetConfigKey([FromRoute] string overrideLevel, [FromRoute] string key) {
		var database = m_Redis.GetDatabase(m_RedisConfig.Value.Database);
		
		// TODO: better validation of override level.
		// TODO: access controls.
		int[] splitIndices = overrideLevel.IndicesOf(":").ToArray();

		for (int i = splitIndices.Length; i >= -1; i--) {
			string level;
			if (i == splitIndices.Length) {
				level = overrideLevel;
			} else if (i == -1) {
				level = "";
			} else {
				level = overrideLevel[..splitIndices[i]];
			}
			RedisValue result = await database.StringGetAsync(new RedisKey(level + "/" + key));
			if (result != RedisValue.Null) {
				return Ok(new GetConfigKeyDto(level, result.ToString()));
			}
		}
		
		return NotFound();
	}
	
	/* Not needed anymore.
	[HttpGet("{overrideLevel}/{key}")]
	public async Task<IActionResult> GetAllConfigKeys([FromRoute] string overrideLevel) {
		var database = m_Redis.GetDatabase(m_RedisConfig.Value.Database);
		
		// TODO: better validation of override level.
		// TODO: access controls.
		int[] splitIndices = overrideLevel.IndicesOf(":").ToArray();

		var configKeys = new Dictionary<string, GetConfigKeyDto>();

		foreach (var server in m_Redis.GetServers()) {
			for (int i = splitIndices.Length; i >= -1; i--) {
				string level;
				if (i == splitIndices.Length) {
					level = overrideLevel;
				} else if (i == -1) {
					level = "";
				} else {
					level = overrideLevel[..splitIndices[i]];
				}

				IEnumerable<RedisKey> matches = server.Keys(m_RedisConfig.Value.Database, level + "/*");
				foreach (RedisKey match in matches) {
					string configKey = match.ToString().Split("/")[^1];
					if (!configKeys.ContainsKey(configKey)) {
						string itemOverrideLevel = string.Join("/", match.ToString().Split("/")[..^2]);
						RedisValue value = database.StringGet(match);

						configKeys.Add(configKey, new GetConfigKeyDto(itemOverrideLevel, value.ToString()));
					}
				}
			}
		}

		return Ok(configKeys);
	}*/

	[HttpPut("{overrideLevel}/{key}")]
	public Task<IActionResult> PutConfigKey([FromRoute] string overrideLevel, [FromRoute] string key) {
		var database = m_Redis.GetDatabase(m_RedisConfig.Value.Database);
		
		// TODO: better validation of override level.
		// TODO: access controls.
		database.StringSet(new RedisKey(overrideLevel + "/" + key), new RedisValue(key));
		return Task.FromResult<IActionResult>(NoContent());
	}
}