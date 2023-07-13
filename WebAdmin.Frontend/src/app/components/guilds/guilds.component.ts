import {Component, OnInit} from '@angular/core';
import {GuildInfo} from "../../models/models";
import {ChannelConfigService} from "../../services/channel-config.service";
import {SecurityService} from "../../services/security/security.service";
import {ActivatedRoute, EventType, Router} from "@angular/router";
import {User} from "../../services/security/user";

@Component({
  selector: 'app-guilds',
  templateUrl: './guilds.component.html',
  styleUrls: ['./guilds.component.scss']
})
export class GuildsComponent implements OnInit {
  guilds!: GuildInfo[];
  currentGuildId!: string | null;

  constructor(private ccs: ChannelConfigService,
              private security: SecurityService,
              private router: Router,
              private activatedRoute: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.router.events.subscribe(evt => {
      if (evt.type == EventType.NavigationEnd) {
        this.currentGuildId = this.activatedRoute.snapshot.params['guildId'];
      }
    });

    const currentUser = this.security.currentUser();
    if (currentUser) {
      this.updateGuilds(currentUser);
    }

    this.security.userObservable().subscribe(user => {
      if (user) {
        this.updateGuilds(user);
      }
    });
  }

  updateGuilds(user: User) {
    this.guilds = Object.values(user.guilds);
  }
}
