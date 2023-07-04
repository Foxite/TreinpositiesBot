import {NgModule} from "@angular/core";
import {environment} from "../../../environments/environment";
import {HTTP_INTERCEPTORS} from "@angular/common/http";
import {SecurityHttpInterceptor} from "./interceptor";
import {DevSecurityModule} from "./dev/dev-security.module";
import {OAuthSecurityModule} from "./oauth/oauth-security.module";

@NgModule({
	imports: [
		environment.useAuth ? OAuthSecurityModule : DevSecurityModule,
	],
	providers: [
		{
			provide: HTTP_INTERCEPTORS,
			useClass: SecurityHttpInterceptor,
			multi: true,
		},
	]
})
export class SecurityModule {
}
