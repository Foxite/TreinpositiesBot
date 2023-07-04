import {GuildInfo} from "../../models/models";

export interface User {
	name: string;
  guilds: Record<number, GuildInfo>;
}
