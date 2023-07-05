export interface GuildInfo {
  id: string;
  name: string;
  iconUrl: string | null;
}

export interface GuildConfig {
  id: string;
  cooldownSeconds: number | null;
  sourceNames: string[] | null;
  categories: GuildChannelCategory[];
}

export interface GuildChannelCategory {
  id: string | null;
  name: string | null;
  channels: GuildChannelConfig[];
}

export interface GuildChannelConfig {
  id: string;
  name: string;
  type: string;
  cooldownSeconds: number | null;
  sourceNames: string[] | null;
}
