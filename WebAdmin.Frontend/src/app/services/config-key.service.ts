import {Injectable} from '@angular/core';
import {LevelInfo} from "../models/models";
import {lastValueFrom} from "rxjs";
import {HttpClient} from "@angular/common/http";
import {environment} from "../../environments/environment";

@Injectable({
	providedIn: 'root'
})
export class ConfigKeyService {
	constructor(private http: HttpClient) {
	}

  async getConfigKey<T>(level: LevelInfo, key: string): Promise<ConfigItem<T> | null> {
    return new Promise((resolve, reject) => {
      let latest: ConfigItem<T> | null = null;
      this.http.get<ConfigItem<T>>(`${environment.apiUrl}/ConfigKey/${level.path}/${key}`)
        .subscribe({
          next: value => latest = value,
          complete: () => resolve(latest),
          error: error => {
            if (error.status === 404) {
              resolve(null);
            } else {
              reject(error);
            }
          }
        })
    });
  }

  setConfigKey<T>(level: LevelInfo, key: string, value: T): Promise<void> {
    return lastValueFrom(this.http.put<void>(`${environment.apiUrl}/ConfigKey/${level.path}/${key}`, value, {
      headers: {
        "content-type": "application/json"
      }
    }));
  }

  deleteConfigKey<T>(level: LevelInfo, key: string): Promise<void> {
    return lastValueFrom(this.http.delete<void>(`${environment.apiUrl}/ConfigKey/${level.path}/${key}`));
  }
}

export interface ConfigItem<T> {
  overrideLevel: string,
  value: T,
}
