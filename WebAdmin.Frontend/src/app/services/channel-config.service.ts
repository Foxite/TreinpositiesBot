import {Injectable} from '@angular/core';
import {LevelInfo} from "../models/models";
import {lastValueFrom} from "rxjs";
import {HttpClient} from "@angular/common/http";
import {environment} from "../../environments/environment";

@Injectable({
	providedIn: 'root'
})
export class ChannelConfigService {
	constructor(private http: HttpClient) {
	}

  private getLevelString(level: LevelInfo): string {
    let levelStack = [ level.id ]; // todo use actual stack?

    while (level.parent) {
      level = level.parent
      levelStack = [ level.id, ...levelStack ];
    }

    return levelStack.join(":");
  }

  getConfigKey<T>(level: LevelInfo, key: string): Promise<T> {
    return lastValueFrom<T>(this.http.get<T>(`${environment.apiUrl}/ChannelConfig/${this.getLevelString(level)}/${key}`))
  }

  setConfigKey<T>(level: LevelInfo, key: string, value: string): Promise<void> {
    return lastValueFrom(this.http.put<void>(`${environment.apiUrl}/ChannelConfig/${this.getLevelString(level)}/${key}`, value));
  }
}
