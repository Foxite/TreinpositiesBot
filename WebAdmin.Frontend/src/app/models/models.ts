export interface GuildInfo {
  id: string;
  name: string;
  iconUrl: string | null;
}

export interface ItemConfig {
  itemType: "channel" | "guild";
  id: string;
  cooldownSeconds: number | null;
  sourceNames: string[] | null;
}

export interface GuildConfig extends ItemConfig {
  itemType: "guild";
  categories: GuildChannelCategory[];
}

export interface GuildChannelCategory {
  id: string | null;
  name: string | null;
  channels: GuildChannelConfig[];
}

export interface GuildChannelConfig extends ItemConfig {
  itemType: "channel";
  name: string;
  type: string;
}
