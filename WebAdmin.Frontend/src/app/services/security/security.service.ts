import {Observable} from "rxjs";
import {User} from "./user";
import {HttpRequest} from "@angular/common/http";
import {Injectable} from "@angular/core";

export abstract class SecurityService {
	abstract setup(): void;
	abstract login(): void;
	abstract logout(): void;
	abstract currentUser(): User | null;
	abstract userObservable(): Observable<User | null>;
	abstract readyToAuthorize(): Promise<void>;
	abstract authorizeRequest(request: HttpRequest<any>): HttpRequest<any>;
}
