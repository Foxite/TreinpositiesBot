import {Injectable} from "@angular/core";
import {HttpClient} from "@angular/common/http";
import {lastValueFrom} from "rxjs";

@Injectable()
export class DiscordService {
  private readonly apiUrl = "https://discord.com/api";

  constructor(private http: HttpClient) {
  }

  getCurrentAuthorization(): Promise<DiscordCurrentAuthorization> {
    return lastValueFrom(this.http.get<DiscordCurrentAuthorization>(`${this.apiUrl}/oauth2/@me`));
  }

  getCurrentUserGuilds(): Promise<DiscordGuildSummary[]> {
    return lastValueFrom(this.http.get<DiscordGuildSummary[]>(`${this.apiUrl}/users/@me/guilds`));
  }
}

export interface DiscordCurrentAuthorization {
  application: {
    id: string,
    name: string,
    icon: string,
    description: string,
    hook: boolean,
    bot_public: boolean,
    bot_require_code_grant: boolean,
    verify_key: string
  };
  scopes: string[];
  expires: Date;
  user: {
    id: string,
    username: string,
    global_name: string,
    avatar: string,
    discriminator: string,
    public_flags: number
    avatar_decoration: unknown
  };
}

export interface DiscordGuildSummary {
  id: string;
  name: string;
  icon: string;
  owner: boolean;
  permissions: number;
  permissions_new: string; // unknown
  features: string[];
}
