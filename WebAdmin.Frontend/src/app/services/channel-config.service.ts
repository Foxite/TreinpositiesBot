import {Injectable} from '@angular/core';
import {GuildConfig, GuildInfo} from "../models/models";
import {lastValueFrom, Observable} from "rxjs";
import {HttpClient} from "@angular/common/http";
import {environment} from "../../environments/environment";

@Injectable({
	providedIn: 'root'
})
export class ChannelConfigService {
	constructor(private http: HttpClient) {
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
