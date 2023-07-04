import {NgModule} from "@angular/core";
import {SecurityService} from "../security.service";
import {DevSecurityService} from "./dev-security.service";

@NgModule({
	providers: [
		{provide: SecurityService, useClass: DevSecurityService}
	]
})
export class DevSecurityModule {
}
