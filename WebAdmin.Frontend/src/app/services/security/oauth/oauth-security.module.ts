import {NgModule} from "@angular/core";
import {OAuthModule, OAuthStorage} from "angular-oauth2-oidc";
import {SecurityService} from "../security.service";
import {OAuthSecurityService} from "./oauth-security.service";

@NgModule({
	imports: [
		OAuthModule.forRoot({
			resourceServer: {
				sendAccessToken: false
			}
		})
	],
	providers: [
		{provide: OAuthStorage, useValue: localStorage},
		{provide: SecurityService, useClass: OAuthSecurityService}
	]
})
export class OAuthSecurityModule {
}
