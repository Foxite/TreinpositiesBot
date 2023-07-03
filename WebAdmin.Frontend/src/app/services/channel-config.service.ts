import {Injectable} from '@angular/core';
import {GuildConfig, GuildInfo} from "../models/models";
import {lastValueFrom, Observable} from "rxjs";
import { of as observableOf } from 'rxjs';
import {HttpClient} from "@angular/common/http";
import {environment} from "../../environments/environment";

@Injectable({
	providedIn: 'root'
})
export class ChannelConfigService {
	constructor(private http: HttpClient) {
	}

  getGuilds(): Promise<GuildInfo[]> {
    return new Promise((res) => res([
      {
        id: 346682476149866497,
        name: "Foxite's bot factory",
        iconUrl: "https://cdn.discordapp.com/icons/346682476149866497/efa839e385bd832d1b2edc33a40504ae.webp?size=512"
      },
      {
        id: 872837910725017601,
        name: "Corsac Emotes 2",
        iconUrl: null,
      }
    ]));
  }

  getGuild(id: number): Promise<GuildConfig> {
    return lastValueFrom(this.http.get<GuildConfig>(`${environment.apiUrl}/ChannelConfig/${id}`));
  }

  setGuildCooldown(guildId: number, cooldownSeconds: number | null): Promise<void> {
    return lastValueFrom(this.http.put<void>(`${environment.apiUrl}/ChannelConfig/${guildId}/Cooldown`, cooldownSeconds));
  }

  setGuildSources(guildId: number, sources: string[] | null): Promise<void> {
    return lastValueFrom(this.http.put<void>(`${environment.apiUrl}/ChannelConfig/${guildId}/Cooldown`, JSON.stringify(sources)));
  }

  setChannelCooldown(guildId: number, channelId: number, cooldownSeconds: number | null): Promise<void> {
    return lastValueFrom(this.http.put<void>(`${environment.apiUrl}/ChannelConfig/${guildId}/${channelId}/Cooldown`, cooldownSeconds));
  }

  setChannelSources(guildId: number, channelId: number, sources: string[] | null): Promise<void> {
    return lastValueFrom(this.http.put<void>(`${environment.apiUrl}/ChannelConfig/${guildId}/${channelId}/Cooldown`, JSON.stringify(sources)));
  }
}
