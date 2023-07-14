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

  getGuild(id: string): Promise<GuildConfig | null> {
    return new Promise((resolve, reject) => {
      let lastValue: GuildConfig | null = null;
      this.http.get<GuildConfig>(`${environment.apiUrl}/ChannelConfig/${id}`).subscribe({
        next: (value: GuildConfig) => {
          console.log(value);
          lastValue = value;
        },
        complete: () => resolve(lastValue),
        error: (error: any) => {
          if (error.status === 404) {
            resolve(null);
          } else {
            reject(error);
          }
        }
      });
    });
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
