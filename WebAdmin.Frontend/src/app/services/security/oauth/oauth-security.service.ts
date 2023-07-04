import {SecurityService} from "../security.service";
import {filter, Observable, Subject} from "rxjs";
import {User} from "../user";
import {OAuthService} from "angular-oauth2-oidc";
import {authCodeFlowConfig} from "../../../../environments/environment";
import {HttpRequest} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {DiscordService} from "../../discord.service";
import {GuildInfo} from "../../../models/models";

@Injectable()
export class OAuthSecurityService extends SecurityService {
	private readonly userSubject: Subject<User | null>;
  private user: User | null;

	constructor(private oauthService: OAuthService,
              private discordService: DiscordService) {
		super();
    this.userSubject = new Subject<User | null>();
    this.user = null;
	}

	override setup(): void {
		this.oauthService.configure(authCodeFlowConfig);

		this.oauthService.events
			.pipe(filter((e) => e.type === 'token_received'))
			.subscribe((_) => this.updateCurrentUser());
	}

	override login() {
    this.oauthService.tryLogin()
      .then(async () => {
        if (this.oauthService.hasValidAccessToken()) {
          await this.updateCurrentUser();
        } else {
          await this.oauthService.initLoginFlow();
        }
      })
      .catch(console.error);
  }

	override logout() {
	}

	override currentUser(): User | null {
    return this.user;
	}

	override userObservable(): Observable<User | null> {
		return this.userSubject;
	}

	override readyToAuthorize(): Promise<void> {
		return new Promise((res, rej) => {
			this.userObservable().subscribe(user => {
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

  private async updateCurrentUser(): Promise<void> {
    const currentAuth = await this.discordService.getCurrentAuthorization();
    const guilds = await this.discordService.getCurrentUserGuilds();
    const guildInfos: Record<number, GuildInfo> = {};
    for (const guild of guilds) {
      const manageGuild = (1 << 5);
      const admin = (1 << 3);

      const hasNoRights = (guild.permissions & (manageGuild | admin)) == 0;
      if (hasNoRights) {
        continue;
      }

      const guildId = parseInt(guild.id);
      guildInfos[guildId] = {
        id: guildId,
        name: guild.name,
        iconUrl: `https://cdn.discordapp.com/icons/${guild.id}/${guild.icon}.png`
      };
    }

    this.user = {
      name: currentAuth.user.global_name,
      guilds: guildInfos
    };

    this.userSubject.next(this.user);
  }
}
