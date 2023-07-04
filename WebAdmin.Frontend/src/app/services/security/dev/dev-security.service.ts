import {Observable} from "rxjs";
import {User} from "../user";
import {SecurityService} from "../security.service";
import {HttpRequest} from "@angular/common/http";
import {Injectable} from "@angular/core";

@Injectable()
export class DevSecurityService extends SecurityService {
	override setup(): void { }
	override login(): void { }
	override logout(): void { }

	override currentUser(): User | null {
		return {
      name: "Test User",
      guilds: {
        346682476149866497: {
          id: 346682476149866497,
          name: "Foxite's bot factory",
          iconUrl: "https://cdn.discordapp.com/icons/346682476149866497/efa839e385bd832d1b2edc33a40504ae.webp?size=512"
        },
        872837910725017601: {
          id: 872837910725017601,
          name: "Corsac Emotes 2",
          iconUrl: null,
        }
      }
    };
	}

	override userUpdated(): Observable<User | null> {
		return new Observable(sub => sub.next(this.currentUser()));
	}

	override readyToAuthorize(): Promise<void> {
		return new Promise(() => {});
	}

	override authorizeRequest(request: HttpRequest<any>): HttpRequest<any> {
		return request.clone({
			headers: request.headers
				.append("x-debug-claims", "sub=Test User")
				.append("x-debug-claims", "uid=1234")
				.append("x-debug-claims", "role=Mellifera Admin")
		});
	}
}
