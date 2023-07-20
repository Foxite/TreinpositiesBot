import {Injectable} from '@angular/core';
import {GuildConfig} from "../models/models";
import {lastValueFrom, Observable} from "rxjs";
import {HttpClient} from "@angular/common/http";
import {environment} from "../../environments/environment";

@Injectable({
	providedIn: 'root'
})
export class ChannelConfigService {
	constructor(private http: HttpClient) {
	}

  async getGuild(id: string): Promise<GuildConfig | null> {
    const result = await lastValueFrom(this.http.get<GuildConfig>(`${environment.apiUrl}/ChannelConfig/${id}`, { observe: 'response' }));
    if (result.status == 404) {
      return null;
    } else if (result.status == 200) {
      return result.body;
    } else {
      throw new Error("Unsuccessful result: " + result.status);
    }
  }

  setGuildCooldown(guildId: string, cooldownSeconds: number | null): Promise<void> {
    return lastValueFrom(this.http.put<void>(`${environment.apiUrl}/ChannelConfig/${guildId}/Cooldown`, cooldownSeconds));
  }

  setGuildSources(guildId: string, sources: string[] | null): Promise<void> {
    return lastValueFrom(this.http.put<void>(`${environment.apiUrl}/ChannelConfig/${guildId}/Cooldown`, JSON.stringify(sources)));
  }

  setChannelCooldown(guildId: string, channelId: string, cooldownSeconds: number | null): Promise<void> {
    return lastValueFrom(this.http.put<void>(`${environment.apiUrl}/ChannelConfig/${guildId}/${channelId}/Cooldown`, cooldownSeconds));
  }

  setChannelSources(guildId: string, channelId: string, sources: string[] | null): Promise<void> {
    return lastValueFrom(this.http.put<void>(`${environment.apiUrl}/ChannelConfig/${guildId}/${channelId}/Cooldown`, JSON.stringify(sources)));
  }
}
