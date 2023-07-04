import {Component, OnInit} from '@angular/core';
import {Observable} from "rxjs";
import {GuildInfo} from "../../models/models";
import {ChannelConfigService} from "../../services/channel-config.service";
import {SecurityService} from "../../services/security/security.service";

@Component({
  selector: 'app-guilds',
  templateUrl: './guilds.component.html',
  styleUrls: ['./guilds.component.scss']
})
export class GuildsComponent implements OnInit {
  guilds!: GuildInfo[];
  currentGuild!: GuildInfo | null;

  constructor(private ccs: ChannelConfigService,
              private security: SecurityService) {
  }

  ngOnInit(): void {
    const currentUser = this.security.currentUser();
    if (currentUser) {
      this.guilds = Object.values(currentUser.guilds);
    }

    this.security.userObservable().subscribe(user => {
      if (user) {
        this.guilds = Object.values(user.guilds);
      }
    });
  }
}
