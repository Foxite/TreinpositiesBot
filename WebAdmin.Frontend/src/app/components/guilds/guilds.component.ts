import { Component } from '@angular/core';
import {Observable} from "rxjs";
import {GuildInfo} from "../../models/models";
import {ChannelConfigService} from "../../services/channel-config.service";
import {SecurityService} from "../../services/security/security.service";

@Component({
  selector: 'app-guilds',
  templateUrl: './guilds.component.html',
  styleUrls: ['./guilds.component.scss']
})
export class GuildsComponent {
  guilds!: GuildInfo[];
  currentGuild!: GuildInfo | null;

  constructor(private ccs: ChannelConfigService,
              private security: SecurityService) {
  }

  ngOnInit(): void {
    if (this.security.currentUser()) {
      this.guilds = Object.values(this.security.currentUser()!.guilds);
    }
  }

  setCurrentGuild(guild: GuildInfo) {
    this.currentGuild = guild;
  }
}
