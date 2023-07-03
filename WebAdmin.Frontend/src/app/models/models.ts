export interface GuildConfig {
  id: number;
  cooldownSeconds: number | null;
  sourceNames: string[] | null;
  channels: ChannelConfig[];
}

export interface ChannelConfig {
  guildId: number;
  id: number;
  cooldownSeconds: number | null;
  sourceNames: string[] | null;
}

export interface GuildInfo {
  id: number;
  name: string;
  iconUrl: string | null;
}
