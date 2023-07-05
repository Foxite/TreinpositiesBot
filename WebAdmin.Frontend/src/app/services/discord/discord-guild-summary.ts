export interface DiscordGuildSummary {
  id: string;
  name: string;
  icon: string;
  owner: boolean;
  permissions: number;
  permissions_new: string; // unknown
  features: string[];
}
