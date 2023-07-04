import {SecurityService} from "../security.service";
import {filter, Observable, Subscriber} from "rxjs";
import {User} from "../user";
import {OAuthService} from "angular-oauth2-oidc";
import {authCodeFlowConfig} from "../../../../environments/environment";
import {HttpRequest} from "@angular/common/http";
import {Injectable} from "@angular/core";

@Injectable()
export class OAuthSecurityService extends SecurityService {
	private profile: Observable<User | null>;
	private profileSubscriber!: Subscriber<User | null>;

	constructor(private oauthService: OAuthService) {
		super();

		this.profile = new Observable<User | null>(sub => this.profileSubscriber = sub);
	}

	override setup(): void {
		this.oauthService.configure(authCodeFlowConfig);
		//this.oauthService.loadDiscoveryDocument(authCodeFlowConfig.discoveryDocumentUrl)
    this.oauthService.tryLogin()
			.then(() => {
				if (!this.oauthService.hasValidIdToken() || !this.oauthService.hasValidAccessToken()) {
					this.oauthService.initLoginFlow();
				}
			});

		//this.oauthService.setupAutomaticSilentRefresh();

		// Automatically load user profile
		this.oauthService.events
			.pipe(filter((e) => e.type === 'token_received'))
			.subscribe(async (_) => {
				await this.oauthService.loadUserProfile()
				this.profileSubscriber.next(this.currentUser());
			});
	}

	override login() {
	}

	override logout() {
	}

	override currentUser(): User | null {
		const claims: any = this.oauthService.getIdentityClaims();
		if (!claims || !claims.hasOwnProperty('name')) {
			return null;
		} else {
			//return new User(claims['name']);
      // TODO
      return {
        name: claims['name'],
        guilds: {}
      };
		}
	}

	override userUpdated(): Observable<User | null> {
		return this.profile;
	}

	override readyToAuthorize(): Promise<void> {
		return new Promise((res, rej) => {
			this.userUpdated().subscribe(user => {
				if (user) {
					res();
				} else {
					rej();
				}
			})
		})
	}

	override authorizeRequest(request: HttpRequest<any>): HttpRequest<any> {
		const token = this.oauthService.getAccessToken();
		return request.clone({
			headers: request.headers.set("authorization", "Bearer " + token)
		});
	}
}
