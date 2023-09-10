import {Injectable} from '@angular/core';
import {LevelInfo} from "../models/models";
import {lastValueFrom} from "rxjs";
import {HttpClient} from "@angular/common/http";
import {environment} from "../../environments/environment";
import {getPathFromLevel} from "../../util";

@Injectable({
	providedIn: 'root'
})
export class ConfigKeyService {
	constructor(private http: HttpClient) {
	}

  getConfigKey<T>(level: LevelInfo, key: string): Promise<T> {
    return lastValueFrom<T>(this.http.get<T>(`${environment.apiUrl}/ConfigKey/${getPathFromLevel(level)}/${key}`))
  }

  setConfigKey<T>(level: LevelInfo, key: string, value: T): Promise<void> {
    return lastValueFrom(this.http.put<void>(`${environment.apiUrl}/ConfigKey/${getPathFromLevel(level)}/${key}`, value, {
      headers: {
        "content-type": "application/json"
      }
    }));
  }
}
