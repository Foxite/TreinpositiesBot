import {NgModule} from "@angular/core";
import {HTTP_INTERCEPTORS} from "@angular/common/http";
import {SecurityHttpInterceptor} from "./interceptor";
import {OAuthModule, OAuthStorage} from "angular-oauth2-oidc";
import {SecurityService} from "./security.service";

@NgModule({
	imports: [
    OAuthModule.forRoot({
      resourceServer: {
        sendAccessToken: false
      }
    })
	],
	providers: [
    SecurityService,
    {
      provide: OAuthStorage,
      useValue: localStorage
    },
		{
			provide: HTTP_INTERCEPTORS,
			useClass: SecurityHttpInterceptor,
			multi: true,
		},
	]
})
export class SecurityModule {
}
