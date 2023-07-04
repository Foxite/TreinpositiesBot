import {SecurityService} from "../security.service";
import {filter, Observable, Subscriber} from "rxjs";
import {User} from "../user";
import {OAuthService} from "angular-oauth2-oidc";
import {authCodeFlowConfig} from "../../../../environments/environment";
import {HttpRequest} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {DiscordService} from "../../discord.service";
import {GuildInfo} from "../../../models/models";

@Injectable()
export class OAuthSecurityService extends SecurityService {
  private static currentIndex = 0;

  private index;
	private profile: Observable<User | null>;
	private profileSubscriber!: Subscriber<User | null>;
  private user: User | null;

	constructor(private oauthService: OAuthService,
              private discordService: DiscordService) {
		super();
    this.index = OAuthSecurityService.currentIndex++;
    this.profile = new Observable<User | null>(sub => this.profileSubscriber = sub);
    this.user = null;
	}

	override setup(): void {
    console.log("hoi!");
    this.profile.subscribe(console.log);

		this.oauthService.configure(authCodeFlowConfig);

		// Automatically load user profile
		this.oauthService.events
			.pipe(filter((e) => e.type === 'token_received'))
			.subscribe((_) => this.updateCurrentUser());
	}

	override login() {
    this.oauthService.tryLogin()
      .then(async () => {
        if (this.oauthService.hasValidAccessToken()) {
          console.log("tryLogin")
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

  private async updateCurrentUser(): Promise<void> {

    console.log("updateCurrentUser")
    const currentAuth = await this.discordService.getCurrentAuthorization();
    const guilds = await this.discordService.getCurrentUserGuilds();

    this.profile.subscribe((item) => {
      console.log("yo");
      console.log(item);
      console.log("oy");
    });

    const guildInfos: Record<number, GuildInfo> = {};
    for (const guild of guilds) {
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

    console.log(this.user);

    console.log(this);
    // todo fix: this does not trigger subscribers
    this.profileSubscriber.next(this.user);
    console.log("doñe");
  }
}
