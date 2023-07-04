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
		return new User("Test User");
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
