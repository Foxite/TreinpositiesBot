import {Injectable} from "@angular/core";
import {HttpClient} from "@angular/common/http";
import {lastValueFrom} from "rxjs";
import {DiscordCurrentAuthorization} from "./discord-current-authorization";
import {DiscordGuildSummary} from "./discord-guild-summary";
import {CachedValue} from "./cached-value";

@Injectable()
export class DiscordService {
  private readonly apiUrl = "https://discord.com/api"

  constructor(private http: HttpClient) {
  }

  private async getCachedOrApi<T>(endpoint: string, maxAgeSeconds: number): Promise<T> {
    const cacheKey = "DiscordService__Cached__" + endpoint;

    console.log(cacheKey);

    const cachedJson = sessionStorage.getItem(cacheKey);
    if (cachedJson) {
      console.log("found");
      const cachedValue = JSON.parse(cachedJson);
      console.log(cachedValue.retrieved);
      console.log(new Date(new Date(cachedValue.retrieved).valueOf() + maxAgeSeconds * 1000));
      console.log(new Date());
      if (cachedValue && new Date().valueOf() - new Date(cachedValue.retrieved).valueOf() < maxAgeSeconds * 1000) {
        console.log("cached");
        return cachedValue.value;
      } else {
        console.log("old");
      }
    } else {
      console.log("no");
    }

    const result: T = await lastValueFrom(this.http.get<T>(endpoint));
    sessionStorage.setItem(cacheKey, JSON.stringify(new CachedValue(new Date(), result)));
    return result;
  }

  getCurrentAuthorization(): Promise<DiscordCurrentAuthorization> {
    return this.getCachedOrApi(`${this.apiUrl}/oauth2/@me`, 60);
  }

  getCurrentUserGuilds(): Promise<DiscordGuildSummary[]> {
    return this.getCachedOrApi(`${this.apiUrl}/users/@me/guilds`, 60);
  }
}
