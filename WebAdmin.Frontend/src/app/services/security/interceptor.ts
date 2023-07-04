import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from "@angular/common/http";
import {SecurityService} from "./security.service";
import {Observable, of} from "rxjs";
import {environment} from "../../../environments/environment";
import {Injectable} from "@angular/core";

@Injectable()
export class SecurityHttpInterceptor implements HttpInterceptor {
	private static readonly authorizedUrls = ["https://discord.com/api"];

	constructor(private securityService: SecurityService) {
	}

	private shouldAuthorize(req: HttpRequest<any>): boolean {
		for (const url of SecurityHttpInterceptor.authorizedUrls) {
			if (req.url.startsWith(url)) {
				return true;
			}
		}

		return false;
	}

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		console.log(req.url);
		if (this.shouldAuthorize(req)) {
			console.log("yes");
			return of(this.securityService.readyToAuthorize()).pipe(() => {
				const authorizedRequest = this.securityService.authorizeRequest(req);
				return next.handle(authorizedRequest);
			})
		} else {
			console.log("no");
			return next.handle(req);
		}
	}
}
